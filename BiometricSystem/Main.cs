using System.Windows.Forms;
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.IO;//.StreamReader;

using SecuGen.FDxSDKPro.Windows;
namespace BiometricSystem
{

    public partial class Main : Form
    {
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

        private System.Windows.Forms.RadioButton[] m_RadioButton;
      
        string imgLoc = "";
     
        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            EnableButtons(false);
            // Init Button
            m_RadioButton = new RadioButton[11];


            m_SecurityLevel = SGFPMSecurityLevel.NORMAL;
            m_StoredTemplate = null;
            m_ImageWidth = 260;
            m_ImageHeight = 300;
            m_Dpi = 500;

            m_FPM = new SGFingerPrintManager();
            Int32 iError;
            string enum_device;

            comboBox1.Items.Clear();

            // Enumerate Device
            iError = m_FPM.EnumerateDevice();

            // Get enumeration info into SGFPMDeviceList
            m_DevList = new SGFPMDeviceList[m_FPM.NumberOfDevice];

            for (int i = 0; i < m_FPM.NumberOfDevice; i++)
            {
                m_DevList[i] = new SGFPMDeviceList();
                m_FPM.GetEnumDeviceInfo(i, m_DevList[i]);
                enum_device = m_DevList[i].DevName.ToString() + " : " + m_DevList[i].DevID;
                comboBox1.Items.Add(enum_device);
            }

            if (comboBox1.Items.Count > 0)
            {
                // Add Auto Selection
                enum_device = "Auto Selection";
                comboBox1.Items.Add(enum_device);

                comboBox1.SelectedIndex = 0;  //First selected one
            }

            StatusBar.Text = "Click Init Button";

            if (m_useAnsiTemplate)
            {
                StatusBar.Text = "Format used: ANSI 378 Format";
            }
            else
            {
                StatusBar.Text = "Format used: ISO 19794-2 Format";
            }
        }

        private void OpenDeviceBtn_Click(object sender, EventArgs e)
        {
            Int32 error;
            SGFPMDeviceName device_name = SGFPMDeviceName.DEV_UNKNOWN;
            Int32 device_id = (Int32)SGFPMPortAddr.USB_AUTO_DETECT;

            m_DeviceOpened = false;

            // Get device name
            if (comboBox1.Text == "DEV_FDU03 : 0")
                device_name = SGFPMDeviceName.DEV_FDU03;

            else if (comboBox1.Text == "Auto Selection")
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
                StatusBar.Text = "Initialization Success";
            }
            else
            {
                EnableButtons(false);
                StatusBar.Text = "Init() Error " + error;
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
                    StatusBar.Text = "OpenDevice() Error : " + error;
                    EnableButtons(false);
                }
            }
        }

        private void BtnCapture1_Click(object sender, EventArgs e)
        {
            Byte[] fp_image = new Byte[m_ImageWidth * m_ImageHeight];
            Int32 error = (Int32)SGFPMError.ERROR_NONE;
            Int32 img_qlty = 0;
            Int32 info;


            if (m_DeviceOpened)
                error = m_FPM.GetImage(fp_image);
            else
                error = GetImageFromFile(fp_image);

            if (error == (Int32)SGFPMError.ERROR_NONE)
            {

                m_FPM.GetImageQuality(m_ImageWidth, m_ImageHeight, fp_image, ref img_qlty);
                progressBar_R1.Value = img_qlty;

                DrawImage(fp_image, pictureBoxR1);

                SGFPMFingerInfo finger_info = new SGFPMFingerInfo();
                //   finger_info.FingerNumber = (SGFPMFingerPosition)comboBoxSelFinger.SelectedIndex;
                finger_info.ImageQuality = (Int16)img_qlty;
                finger_info.ImpressionType = (Int16)SGFPMImpressionType.IMPTYPE_LP;
                finger_info.ViewNumber = 1;

                // CreateTemplate
                info = m_FPM.CreateTemplate(finger_info, fp_image, m_RegMin1);

                if (error == (Int32)SGFPMError.ERROR_NONE)
                    StatusBar.Text = "Image is captured";
                else
                    StatusBar.Text = "GetMinutiae() Error : " + error;
            }
            else
                StatusBar.Text = "GetImage() Error . Try again: " + error;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Byte[] fp_image = new Byte[m_ImageWidth * m_ImageHeight];
            Int32 error = (Int32)SGFPMError.ERROR_NONE;
            Int32 img_qlty = 0;

            if (m_DeviceOpened)
                error = m_FPM.GetImage(fp_image);
            else
                error = GetImageFromFile(fp_image);

            m_FPM.GetImageQuality(m_ImageWidth, m_ImageHeight, fp_image, ref img_qlty);
            progressBar1.Value = img_qlty;

            if (error == (Int32)SGFPMError.ERROR_NONE)
            {
                DrawImage(fp_image, pictureBoxR2);

                SGFPMFingerInfo finger_info = new SGFPMFingerInfo();
                // finger_info.FingerNumber = (SGFPMFingerPosition)comboBoxSelFinger.SelectedIndex;
                finger_info.ImageQuality = (Int16)img_qlty;
                finger_info.ImpressionType = (Int16)SGFPMImpressionType.IMPTYPE_LP;
                finger_info.ViewNumber = 1;

                error = m_FPM.CreateTemplate(finger_info, fp_image, m_RegMin2);

                if (error == (Int32)SGFPMError.ERROR_NONE)
                    StatusBar.Text = "Second image is captured";
                else
                    StatusBar.Text = "GetMinutiae() Error : " + error;
            }
            else
                StatusBar.Text = "GetImage() Error : " + error;
        }
        private void EnableButtons(bool enable)
        {
            BtnCapture1.Enabled = enable;
            button1.Enabled = enable;

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
            picBox.Refresh();
        }
        private Int32 GetImageFromFile(Byte[] data)
        {
            OpenFileDialog open_dlg;
            open_dlg = new OpenFileDialog();

            open_dlg.Title = "Image raw file dialog";
            open_dlg.Filter = "Image raw files (*.raw)|*.raw";

            if (open_dlg.ShowDialog() == DialogResult.OK)
            {
                FileStream inStream = File.OpenRead(open_dlg.FileName);

                BinaryReader br = new BinaryReader(inStream);

                Byte[] local_data = new Byte[data.Length];
                local_data = br.ReadBytes(data.Length);
                Array.Copy(local_data, data, data.Length);

                br.Close();
                return (Int32)SGFPMError.ERROR_NONE;
            }
            return (Int32)SGFPMError.ERROR_FUNCTION_FAILED;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Random rnd = new Random();
                int id = rnd.Next(1, 1000);
                string sql = string.Empty;
                string con = string.Empty;
                byte[] img = null;
                FileStream fs = new FileStream(imgLoc, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                img = br.ReadBytes((int)fs.Length);

                Image myImage = pictureBoxR1.Image;
                byte[] data;
                using (MemoryStream ms = new MemoryStream())
                {
                    myImage.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    data = ms.ToArray();
                }
                //connection to database
                con = "Server=127.0.0.1; port=3306; Uid=root; Database=Students; Password=";

                sql = @"INSERT  INTO Users (RegistrationNumber,Fullnames,YearOfStudy,PhoneNumber,Photo,Fingerprint,f_id,f_id2,Department,Course) VALUES (@StudentID,@Fnames,@Year,@Phone,@img,@f_image,@f_id,@f_id2,@department,@course)";
                using (MySqlConnection sqlcon = new MySqlConnection(con))
                {
                    sqlcon.Open();
                    using (MySqlCommand com = new MySqlCommand(sql, sqlcon))
                    {

                        if (textBox1.Text != "" || textBox2.Text != "" || textBox3.Text != "" || textBox4.Text != "" || comboBox2.Text != "")
                        {
                       
                            ////get values from users
                            com.Parameters.AddWithValue("@StudentID", textBox1.Text);
                            com.Parameters.AddWithValue("@Fnames", textBox2.Text);
                            com.Parameters.AddWithValue("@Year", textBox3.Text);
                            com.Parameters.AddWithValue("@Phone", textBox4.Text);
                            com.Parameters.AddWithValue("@img", img);
                            com.Parameters.AddWithValue("@f_image", data);
                            com.Parameters.AddWithValue("@f_id", m_RegMin1);
                            com.Parameters.AddWithValue("@f_id2", m_RegMin2);
                            com.Parameters.AddWithValue("@department", comboBox2.Text);
                            com.Parameters.AddWithValue("@course", comboBox3.Text);
                            com.ExecuteNonQuery();
                            //if successful
                            MessageBox.Show("Process Complete.....");
                            //empty textboxes
                            textBox1.Text = "";
                            textBox2.Text = "";
                            textBox3.Text = "";
                            textBox4.Text = "";
                            comboBox2.Text = "";
                        }
                        else
                        {
                            StatusBar.Text = "Please Fill all required fields";
                        }

                    }
                }
            }
            catch (Exception ex)
            {


                //incase of error

                MessageBox.Show(ex.Message);
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
               comboBox2.Text = "";
            }
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            try
            {

                //browse photo 
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|All Files (*.*)|*.*";
                dlg.Title = "Select student Photo";
                if (dlg.ShowDialog() == DialogResult.OK)
                {

                    imgLoc = dlg.FileName.ToString();
                    pictureBox2.ImageLocation = imgLoc;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Navigate nav = new Navigate();
            nav.Show();
            this.Hide();
        }
    }
}
