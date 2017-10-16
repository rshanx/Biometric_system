using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace BiometricSystem
{
    public partial class AdminLogin : Form
    {
        public AdminLogin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //check connection
                //connection to database
                string con = string.Empty;
            con = "Server=127.0.0.1; port=3306; Uid=root; Database=Students; Password=";
            string sql = string.Empty;
            sql = @"SELECT * FROM Login WHERE AdminID='"+textBox1.Text+"' and Password='"+textBox2.Text+"'";
                using (MySqlConnection sqlcon = new MySqlConnection(con))
                {
                    sqlcon.Open();
                    string[] Item = new string[1];
                    using (MySqlCommand com = new MySqlCommand(sql, sqlcon))
                    {
                        using (MySqlDataReader auth = com.ExecuteReader())
                        {
                            if(textBox1.Text!="")
                            if (auth.HasRows)
                            {
                                Navigate mm = new Navigate();
                                mm.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Login error()");
                                    textBox1.Text = "";
                                    textBox2.Text = "";
                                }
                        }
                    }
                    }

            }
            catch
            {

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Attendance att = new Attendance();
            att.Show();
            this.Hide();
        }
    }
}
