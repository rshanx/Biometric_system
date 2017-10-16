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
    public partial class View : Form
    {
        MySqlConnection conn = new MySqlConnection("Server=127.0.0.1; port=3306; Uid=root; Database=Students; Password=");
        MySqlCommand comm;
        MySqlDataAdapter adp;
        int studentid;
        DataTable dt;
        public View()
        {
            InitializeComponent();
        }

        private void View_Load(object sender, EventArgs e)
        {
            display();
        }
        public void display()
        {
         
            conn.Open();

            adp = new MySqlDataAdapter("SELECT Id  ,RegistrationNumber, Fullnames,YearOfStudy,PhoneNumber,Department,Course FROM users",conn);

            dt = new DataTable();
            adp.Fill(dt);
            dataGridView1.DataSource = dt;
            conn.Close();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            studentid = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
            textBox1.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
            textBox4.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
            textBox2.Text = dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString();
            textBox3.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            conn.Open();
            comm = new MySqlCommand("UPDATE users set RegistrationNumber='" + textBox1.Text + "', Course='" + textBox2.Text + "', YearOfStudy='" + textBox3.Text + "', Fullnames='" + textBox4.Text + "'  WHERE Id='"+studentid+"'",conn);
            comm.ExecuteNonQuery();
            MessageBox.Show("Data has been Updated !!");
          
            conn.Close();
            display();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Navigate nav = new Navigate();
            nav.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            conn.Open();
            comm = new MySqlCommand("DELETE FROM users WHERE Id='"+studentid+"'", conn);
            comm.ExecuteNonQuery();
            MessageBox.Show("Data has been Deleted !!");

            conn.Close();
            display();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = dataGridView1.DataSource;
            bs.Filter = string.Format("CONVERT(" + dataGridView1.Columns[1].DataPropertyName + ",System.String) like '%" + textBox5.Text.Replace("'", "''") + "%'");
            dataGridView1.Refresh();
        }
    }
}
