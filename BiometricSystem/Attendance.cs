using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using SecuGen.FDxSDKPro.Windows;
namespace BiometricSystem
{
    public partial class Attendance : Form
    {
        private bool m_LedOn = false;
        private bool m_useAnsiTemplate = false; // true;  
        private SGFingerPrintManager m_FPM;
        private Int32 m_ImageWidth;
        private Int32 m_ImageHeight;
        private Int32 m_Dpi;
        private SGFPMSecurityLevel m_SecurityLevel;
        private SGFPMDeviceList[] m_DevList;
        private Byte[] m_RegMin1;
        private Byte[] m_RegMin2;
        private Byte[] m_VrfMin;
        private Byte[] m_StoredTemplate;
        private bool m_DeviceOpened;
        Byte[] fp_image;
        byte[] data;
        public Attendance()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AdminLogin al = new AdminLogin();
            al.Show();
            this.Hide();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void Attendance_Load(object sender, EventArgs e)
        {

            m_LedOn = false;

            m_SecurityLevel = SGFPMSecurityLevel.NORMAL;
            m_StoredTemplate = null;
            m_ImageWidth = 260;
            m_ImageHeight = 300;
            m_Dpi = 500;

            EnableButtons(false);


            m_FPM = new SGFingerPrintManager();
            Int32 iError;
            string enum_device;

            comboBox2.Items.Clear();

            // Enumerate Device
            iError = m_FPM.EnumerateDevice();

            // Get enumeration info into SGFPMDeviceList
            m_DevList = new SGFPMDeviceList[m_FPM.NumberOfDevice];

            for (int i = 0; i < m_FPM.NumberOfDevice; i++)
            {
                m_DevList[i] = new SGFPMDeviceList();
                m_FPM.GetEnumDeviceInfo(i, m_DevList[i]);
                enum_device = m_DevList[i].DevName.ToString() + " : " + m_DevList[i].DevID;
                comboBox2.Items.Add(enum_device);
            }

            if (comboBox2.Items.Count > 0)
            {
                // Add Auto Selection
                enum_device = "Auto Selection";
                comboBox2.Items.Add(enum_device);

                comboBox2.SelectedIndex = 0;  //First selected one
            }
         
        }
        public void listUnits()
        {
           
             var con = @"Server=127.0.0.1; port=3306; Uid=root; Database=students; Password=";
         
            using (var connection = new MySqlConnection(con))
            {
                connection.Open();
                var sql = @"SELECT UnitName from classes WHERE CourseName='"+textBox3.Text+"' AND Year='"+textBox4.Text+"'";
                using(var command = new MySqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader.GetString("UnitName"));
                        }
                    }
                }
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Byte[] fp_image = new Byte[m_ImageWidth * m_ImageHeight];
            Int32 error = (Int32)SGFPMError.ERROR_NONE;
            Int32 img_qlty = 0;
            Int32 info;


            if (m_DeviceOpened)
                error = m_FPM.GetImage(fp_image);


            if (error == (Int32)SGFPMError.ERROR_NONE)
            {

                m_FPM.GetImageQuality(m_ImageWidth, m_ImageHeight, fp_image, ref img_qlty);
                progressBar1.Value = img_qlty;

                DrawImage(fp_image, pictureBox1);

                SGFPMFingerInfo finger_info = new SGFPMFingerInfo();
                //   finger_info.FingerNumber = (SGFPMFingerPosition)comboBoxSelFinger.SelectedIndex;
                finger_info.ImageQuality = (Int16)img_qlty;
                finger_info.ImpressionType = (Int16)SGFPMImpressionType.IMPTYPE_LP;
                finger_info.ViewNumber = 1;

                // CreateTemplate
                info = m_FPM.CreateTemplate(finger_info, fp_image, m_RegMin1);

                Image myImage = pictureBox1.Image;

                using (MemoryStream ms = new MemoryStream())
                {
                    myImage.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    data = ms.ToArray();
                }
                try
                {
                    string sql = string.Empty;
                    string con = string.Empty;
                    //  byte[] id1 = (byte[])(m_RegMin2);

                    con = "Server=127.0.0.1; port=3306; Uid=root; Database=students; Password=";

                    sql = @"SELECT f_id,f_id2,RegistrationNumber,Fullnames,YearOfStudy,PhoneNumber,Course, Photo,Department, fingerprint FROM users";
                    using (MySqlConnection sqlcon = new MySqlConnection(con))
                    {
                        sqlcon.Open();
                        string[] sub_Item = new string[7];
                        using (MySqlCommand com = new MySqlCommand(sql, sqlcon))
                        {
                            using (MySqlDataReader read = com.ExecuteReader())
                            {

                                if (read.HasRows)
                                {





                                    while (read.Read())
                                    {

                                        byte[] f_id = (byte[])(read["fingerprint"]);
                                        byte[] id = (byte[])(read["f_id"]);

                                        byte[] id2 = (byte[])(read["f_id2"]);


                                        bool matched = false;
                                        Int32 err = 0;
                                        err = m_FPM.MatchTemplate(id, m_RegMin1, m_SecurityLevel, ref matched);
                                        if (matched)
                                        {


                                            sub_Item[0] = read["RegistrationNumber"].ToString();
                                            sub_Item[1] = read["Fullnames"].ToString();
                                            sub_Item[2] = read["YearOfStudy"].ToString();
                                            sub_Item[3] = read["PhoneNumber"].ToString();
                                            sub_Item[4] = read["Course"].ToString();
                                            sub_Item[5] = read["Department"].ToString();
                                            byte[] img = (byte[])(read["Photo"]);

                                            MemoryStream ms = new MemoryStream(img);
                                            pictureBox2.Image = new Bitmap(ms);
                                            pictureBox2.Image = Image.FromStream(ms);

                                            MemoryStream ms1 = new MemoryStream(img);
                                            pictureBox2.Image = Image.FromStream(ms1);
                                            // photo.Image
                                            ms.Dispose();

                                            //populate the text boxes
                                            textBox1.Text = sub_Item[0];
                                            textBox2.Text = sub_Item[1];
                                            textBox3.Text = sub_Item[4];
                                            textBox4.Text = sub_Item[2];
                                            listUnits();

                                        }
                                        else
                                        {
                                            bool matched1 = false;
                                            Int32 err2 = 0;
                                            err2 = m_FPM.MatchTemplate(id2, m_RegMin1, m_SecurityLevel, ref matched1);
                                            if (matched1)
                                            {
                                                sub_Item[0] = read["RegistrationNumber"].ToString();
                                                sub_Item[1] = read["Fullnames"].ToString();
                                                sub_Item[2] = read["YearOfStudy"].ToString();
                                                sub_Item[3] = read["PhoneNumber"].ToString();
                                                sub_Item[4] = read["Course"].ToString();
                                                sub_Item[5] = read["Department"].ToString();
                                                byte[] img = (byte[])(read["Photo"]);

                                                MemoryStream ms = new MemoryStream(img);
                                                pictureBox2.Image = new Bitmap(ms);
                                                pictureBox2.Image = Image.FromStream(ms);

                                                MemoryStream ms1 = new MemoryStream(img);
                                                pictureBox2.Image = Image.FromStream(ms1);
                                                // photo.Image
                                                ms.Dispose();

                                                //populate the text boxes
                                                textBox1.Text = sub_Item[0];
                                                textBox2.Text = sub_Item[1];
                                                textBox3.Text = sub_Item[4];
                                                textBox4.Text = sub_Item[2];
                                                listUnits();
                                              

                                            }
                                            else
                                            {
                                               // textBox5.Text = "No fingerprint match found";
                                            }
                                        }


                                    }

                                }
                                else
                                {
                                    MessageBox.Show("No data found");
                                }
                            }
                        }
                        sqlcon.Close();
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
                if (error == (Int32)SGFPMError.ERROR_NONE)
                    textBox5.Text = "Fingerprint Image  captured";
                else
                    textBox5.Text = "GetMinutiae() Error : " + error;
            }
            else
                textBox5.Text = "GetImage() Error : " + error;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string insert = string.Empty;
            string connection2 = string.Empty;
            string check = string.Empty;
            string connect = string.Empty;
            DateTime date = DateTime.Now;
            string time = date.ToString("d");
            connection2 = "Server=127.0.0.1; port=3306; Uid=root; Database=students; Password=";
            insert = @"INSERT INTO attendance(Regnumber,Course,Class,Names,YearOfStudy,Attendance,Time) VALUES(@regnumber,@course,@class,@names,@YOS,@attendance,@time)";


            MySqlConnection connect3 = new MySqlConnection(connection2);
            MySqlCommand c = connect3.CreateCommand();
            c.CommandText = @"SELECT * FROM attendance WHERE Regnumber='" + textBox1.Text + "' AND Time='" + time + "' AND Class='"+comboBox1.Text+"'";
            connect3.Open();
            MySqlDataReader reader = c.ExecuteReader();
            if (reader.HasRows)
            {
                MessageBox.Show("You have already marked your attendance");
                comboBox1.Items.Clear();
            }
            else {

                using (MySqlConnection connect2 = new MySqlConnection(connection2))
                {

                    connect2.Open();
                    using (MySqlCommand ch = new MySqlCommand(check, connect2))
                    {

                        using (MySqlCommand comm = new MySqlCommand(insert, connect2))
                        {

                            comm.Parameters.AddWithValue("@regnumber", textBox1.Text);
                            comm.Parameters.AddWithValue("@names", textBox2.Text);
                            comm.Parameters.AddWithValue("@YOS", textBox4.Text);
                            comm.Parameters.AddWithValue("@course", textBox3.Text);
                            comm.Parameters.AddWithValue("@class", comboBox1.Text);
                            comm.Parameters.AddWithValue("@time", time);
                            comm.Parameters.AddWithValue("@attendance", 1);
                            // command.Parameters.AddWithValue("@Phone", textBox4.Text);
                            //  command.Parameters.AddWithValue("@img", img);
                            if (string.IsNullOrEmpty(comboBox1.Text))
                            {
                                MessageBox.Show("Required fields missing,fill out all the fields and try again");
                            }
                            else { 
                                comm.ExecuteNonQuery();
                                MessageBox.Show("Attendance marked successfully");
                                comboBox1.Items.Clear();
                                comboBox1.Text = "";
                                
                            }
                           

                        }


                    }
                }
            }
        }



        //====================================================defaults======================
        private void LedBtn_Click(object sender, System.EventArgs e)
        {
            m_LedOn = !m_LedOn;
            m_FPM.SetLedOn(m_LedOn);
        }
        private void EnableButtons(bool enable)
        {
            // ConfigBtn.Enabled = enable;       
            button1.Enabled = enable;
            button3.Enabled = enable;
            // GetLiveImageBtn.Enabled = enable;

            //GetBtn.Enabled = enable;
            //SetBrightnessBtn.Enabled = enable;
        }
        private void DrawImage(Byte[] imgData, PictureBox picBox)
        {
            int colorval;
            Bitmap bmp = new Bitmap(m_ImageWidth, m_ImageHeight);
            picBox.Image = (Image)bmp;

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    colorval = (int)imgData[(j * m_ImageWidth) + i];
                    bmp.SetPixel(i, j, Color.FromArgb(colorval, colorval, colorval));
                }
            }
            pictureBox1.Refresh();
        }
        void DisplayError(string funcName, int iError)
        {
            string text = "";

            switch (iError)
            {
                case 0:                             //SGFDX_ERROR_NONE				= 0,
                    text = "Error none";
                    break;

                case 1:                             //SGFDX_ERROR_CREATION_FAILED	= 1,
                    text = "Can not create object";
                    break;

                case 2:                             //   SGFDX_ERROR_FUNCTION_FAILED	= 2,
                    text = "Function Failed";
                    break;

                case 3:                             //   SGFDX_ERROR_INVALID_PARAM	= 3,
                    text = "Invalid Parameter";
                    break;

                case 4:                          //   SGFDX_ERROR_NOT_USED			= 4,
                    text = "Not used function";
                    break;

                case 5:                                //SGFDX_ERROR_DLLLOAD_FAILED	= 5,
                    text = "Can not create object";
                    break;

                case 6:                                //SGFDX_ERROR_DLLLOAD_FAILED_DRV	= 6,
                    text = "Can not load device driver";
                    break;
                case 7:                                //SGFDX_ERROR_DLLLOAD_FAILED_ALGO = 7,
                    text = "Can not load sgfpamx.dll";
                    break;

                case 51:                //SGFDX_ERROR_SYSLOAD_FAILED	   = 51,	// system file load fail
                    text = "Can not load driver kernel file";
                    break;

                case 52:                //SGFDX_ERROR_INITIALIZE_FAILED  = 52,   // chip initialize fail
                    text = "Failed to initialize the device";
                    break;

                case 53:                //SGFDX_ERROR_LINE_DROPPED		   = 53,   // image data drop
                    text = "Data transmission is not good";
                    break;

                case 54:                //SGFDX_ERROR_TIME_OUT			   = 54,   // getliveimage timeout error
                    text = "Time out";
                    break;

                case 55:                //SGFDX_ERROR_DEVICE_NOT_FOUND	= 55,   // device not found
                    text = "Device not found";
                    break;

                case 56:                //SGFDX_ERROR_DRVLOAD_FAILED	   = 56,   // dll file load fail
                    text = "Can not load driver file";
                    break;

                case 57:                //SGFDX_ERROR_WRONG_IMAGE		   = 57,   // wrong image
                    text = "Wrong Image";
                    break;

                case 58:                //SGFDX_ERROR_LACK_OF_BANDWIDTH  = 58,   // USB Bandwith Lack Error
                    text = "Lack of USB Bandwith";
                    break;

                case 59:                //SGFDX_ERROR_DEV_ALREADY_OPEN	= 59,   // Device Exclusive access Error
                    text = "Device is already opened";
                    break;

                case 60:                //SGFDX_ERROR_GETSN_FAILED		   = 60,   // Fail to get Device Serial Number
                    text = "Device serial number error";
                    break;

                case 61:                //SGFDX_ERROR_UNSUPPORTED_DEV		   = 61,   // Unsupported device
                    text = "Unsupported device";
                    break;

                // Extract & Verification error
                case 101:                //SGFDX_ERROR_FEAT_NUMBER		= 101, // utoo small number of minutiae
                    text = "The number of minutiae is too small";
                    break;

                case 102:                //SGFDX_ERROR_INVALID_TEMPLATE_TYPE		= 102, // wrong template type
                    text = "Template is invalid";
                    break;

                case 103:                //SGFDX_ERROR_INVALID_TEMPLATE1		= 103, // wrong template type
                    text = "1st template is invalid";
                    break;

                case 104:                //SGFDX_ERROR_INVALID_TEMPLATE2		= 104, // vwrong template type
                    text = "2nd template is invalid";
                    break;

                case 105:                //SGFDX_ERROR_EXTRACT_FAIL		= 105, // extraction fail
                    text = "Minutiae extraction failed";
                    break;

                case 106:                //SGFDX_ERROR_MATCH_FAIL		= 106, // matching  fail
                    text = "Matching failed";
                    break;

            }

            text = funcName + " Error # " + iError + " :" + text;
            textBox4.Text = text;
        }
        private void GetBtn_Click(object sender, System.EventArgs e)
        {
            SGFPMDeviceInfoParam pInfo = new SGFPMDeviceInfoParam();
            Int32 iError = m_FPM.GetDeviceInfo(pInfo);

            if (iError == (Int32)SGFPMError.ERROR_NONE)
            {
                m_ImageWidth = pInfo.ImageWidth;
                m_ImageHeight = pInfo.ImageHeight;



                ASCIIEncoding encoding = new ASCIIEncoding();


                //BrightnessUpDown.Value = pInfo.Brightness;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Int32 error;
            SGFPMDeviceName device_name = SGFPMDeviceName.DEV_UNKNOWN;
            Int32 device_id = (Int32)SGFPMPortAddr.USB_AUTO_DETECT;

            m_DeviceOpened = false;

            // Get device name
            if (comboBox2.Text == "DEV_FDU03 : 0")
                device_name = SGFPMDeviceName.DEV_FDU03;
         
            else if (comboBox2.Text == "Auto Selection")
                device_name = SGFPMDeviceName.DEV_AUTO;

            if (device_name != SGFPMDeviceName.DEV_UNKNOWN)
            {
                error = m_FPM.Init(device_name);

                if (error == (Int32)SGFPMError.ERROR_NONE)
                {
                    m_FPM.CloseDevice();
                    error = m_FPM.OpenDevice(device_id);
                }

                if (error == (Int32)SGFPMError.ERROR_NONE)
                {
                    SGFPMDeviceInfoParam pInfo = new SGFPMDeviceInfoParam();
                    m_FPM.GetDeviceInfo(pInfo);
                    m_ImageWidth = pInfo.ImageWidth;
                    m_ImageHeight = pInfo.ImageHeight;
                }
            }
            else
                error = m_FPM.InitEx(m_ImageWidth, m_ImageHeight, m_Dpi);

            if (error == (Int32)SGFPMError.ERROR_NONE)
            {
                EnableButtons(true);
                textBox5.Text = "Initialization Success";
            }
            else
            {
                EnableButtons(false);
                textBox5.Text = "Init() Error " + error;
                return;
            }

            if (m_useAnsiTemplate)
            {
                // Set template format to ANSI 378
                error = m_FPM.SetTemplateFormat(SGFPMTemplateFormat.ANSI378);
            }
            else
            {
                // Set template format to ISO 19794-2
                error = m_FPM.SetTemplateFormat(SGFPMTemplateFormat.ISO19794);
            }

            // Get Max template size
            Int32 max_template_size = 0;
            error = m_FPM.GetMaxTemplateSize(ref max_template_size);

            m_RegMin1 = new Byte[max_template_size];
            m_RegMin2 = new Byte[max_template_size];
            m_VrfMin = new Byte[max_template_size];

            // OpenDevice if device is selected
            if (device_name != SGFPMDeviceName.DEV_UNKNOWN)
            {
                error = m_FPM.OpenDevice(device_id);
                if (error == (Int32)SGFPMError.ERROR_NONE)
                {
                    m_DeviceOpened = true;
                }
                else
                {
                    textBox4.Text = "OpenDevice() Error : " + error;
                    EnableButtons(false);
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
