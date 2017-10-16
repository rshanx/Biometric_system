using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;

namespace BiometricSystem
{
    public partial class AddNew : Form
    {
        public AddNew()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Navigate nav = new Navigate();
            nav.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
          
                string sql = string.Empty;
                string con = string.Empty;
             
                //connection to database
                con = "Server=127.0.0.1; port=3306; Uid=root; Database=Students; Password=";

                sql = @"INSERT  INTO classes (Lecturer,UnitName,CourseName,Department,Year,Lec_Id)VALUES (@Lecturer,@Unit,@Course,@Depart,@Year,@lec_id)";
                using (MySqlConnection sqlcon = new MySqlConnection(con))
                {
                    sqlcon.Open();
                    using (MySqlCommand com = new MySqlCommand(sql, sqlcon))
                    {
                        if (textBox1.Text != "" || comboBox3.Text != "" || textBox3.Text != "")
                        {
                            ////get values from users
                            com.Parameters.AddWithValue("@Lecturer", textBox1.Text);
                            com.Parameters.AddWithValue("@Course", comboBox3.Text);
                            com.Parameters.AddWithValue("@Unit", textBox3.Text);
                            com.Parameters.AddWithValue("@Year", textBox2.Text);
                            com.Parameters.AddWithValue("@Depart", comboBox1.SelectedItem);
                            com.Parameters.AddWithValue("@lec_id", textBox4.Text);
                            com.ExecuteNonQuery();
                            //if successful
                            MessageBox.Show("Processing Complete.....");
                            //empty textboxes
                            textBox1.Text = "";
                            comboBox3.Text = "";
                            textBox3.Text = "";
                        }
                        else
                        {
                            MessageBox.Show("All fields are required to be filled");
                        }

                    }
                }
            }
            catch (Exception ex)
            {


                //incase of error

                MessageBox.Show(ex.Message);
                textBox1.Text = "";
                comboBox3.Text = "";
                textBox3.Text = "";
            
            }
        }

        private void AddNew_Load(object sender, EventArgs e)
        {

        }
    }
}
