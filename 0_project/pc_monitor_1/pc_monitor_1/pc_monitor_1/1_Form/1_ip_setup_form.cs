using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pc_monitor_1
{
    public partial class ip_setup_form_1 : Form
    {
        public GetEventHandler ip_form_send_event;

        public ip_setup_form_1()
        {
            InitializeComponent();
        }

        public void set_textbox_from_main(string cmd, string data, string[] data_arr)
        {
            if (cmd == "IP_SETUP_FORM_TEXTBOX_SET_DATA_ARR")
            {
                textBox1.Text = data_arr[0];
                textBox2.Text = data_arr[1];
                textBox3.Text = data_arr[2];
                textBox4.Text = data_arr[3];
                textBox5.Text = data_arr[4];
            }
            if(cmd == "READ_FROM_SYS_IP_DATA")
            {
                textBox1.Text = data_arr[0];
                textBox2.Text = data_arr[1];
                textBox3.Text = data_arr[2];
                textBox4.Text = data_arr[3];
                textBox5.Text = data_arr[4];
            }
        }
        void check_textbox_255()
        {
            int buff = Int32.Parse(textBox1.Text);
            if (buff < 0)
            {
                textBox1.Text = "0";
            }
            if (buff > 255)
            {
                textBox1.Text = "255";
            }
            buff = Int32.Parse(textBox2.Text);
            if (buff < 0)
            {
                textBox2.Text = "0";
            }
            if (buff > 255)
            {
                textBox2.Text = "255";
            }
            buff = Int32.Parse(textBox3.Text);
            if (buff < 0)
            {
                textBox3.Text = "0";
            }
            if (buff > 255)
            {
                textBox3.Text = "255";
            }
            buff = Int32.Parse(textBox4.Text);
            if (buff < 0)
            {
                textBox4.Text = "0";
            }
            if (buff > 255)
            {
                textBox4.Text = "255";
            }
        }

        // write to sys
        private void button1_Click(object sender, EventArgs e)
        {
            check_textbox_255();
            string[] transfer = { textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text };
            ip_form_send_event(cmd: "IP_PORT_WRITE_TO_SYSTEM", data_arr: transfer);
        }

        // read from sys
        private void button2_Click(object sender, EventArgs e)
        {
            ip_form_send_event(cmd: "IP_PORT_READ_FROM_SYSTEM");
        }

        // write to file
        private void button3_Click(object sender, EventArgs e)
        {
            check_textbox_255();

            Properties.Settings.Default.sett_ip_0 = String.Empty;
            Properties.Settings.Default.sett_ip_1 = String.Empty;
            Properties.Settings.Default.sett_ip_2 = String.Empty;
            Properties.Settings.Default.sett_ip_3 = String.Empty;
            Properties.Settings.Default.sett_ip_4 = String.Empty;

            Properties.Settings.Default.sett_ip_0 = textBox1.Text;
            Properties.Settings.Default.sett_ip_1 = textBox2.Text;
            Properties.Settings.Default.sett_ip_2 = textBox3.Text;
            Properties.Settings.Default.sett_ip_3 = textBox4.Text;
            Properties.Settings.Default.sett_ip_4 = textBox5.Text;
            Properties.Settings.Default.Save();
            /*
            FileStream fs = File.Create("ip.txt");
            fs.Close();

            StreamWriter sw = new StreamWriter("ip.txt");
            sw.WriteLine(textBox1.Text);
            sw.WriteLine(textBox2.Text);
            sw.WriteLine(textBox3.Text);
            sw.WriteLine(textBox4.Text);
            sw.WriteLine(textBox5.Text);
            sw.Close();
            */
        }

        // read from file
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = Properties.Settings.Default.sett_ip_0;
                textBox2.Text = Properties.Settings.Default.sett_ip_1;
                textBox3.Text = Properties.Settings.Default.sett_ip_2;
                textBox4.Text = Properties.Settings.Default.sett_ip_3;
                textBox5.Text = Properties.Settings.Default.sett_ip_4;
                string[] transfer = { textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text };
                ip_form_send_event(cmd: "IP_PORT_READ_FROM_FILE", data_arr: transfer);
                /*
                StreamReader sr = new StreamReader("ip.txt");
                string read_all = sr.ReadToEnd();
                sr.Close();
                string[] datas = read_all.Split(new string[] {"\r\n"}, StringSplitOptions.None);

                textBox1.Text = datas[0];
                textBox2.Text = datas[1];
                textBox3.Text = datas[2];
                textBox4.Text = datas[3];
                textBox5.Text = datas[4];

                string[] transfer = { textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text };
                ip_form_send_event(cmd: "IP_PORT_READ_FROM_FILE", data_arr: transfer);
                */
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ip_setup_form_1_FormClosing(object sender, FormClosingEventArgs e)
        {
            check_textbox_255();

            Properties.Settings.Default.sett_ip_0 = String.Empty;
            Properties.Settings.Default.sett_ip_1 = String.Empty;
            Properties.Settings.Default.sett_ip_2 = String.Empty;
            Properties.Settings.Default.sett_ip_3 = String.Empty;
            Properties.Settings.Default.sett_ip_4 = String.Empty;

            Properties.Settings.Default.sett_ip_0 = textBox1.Text;
            Properties.Settings.Default.sett_ip_1 = textBox2.Text;
            Properties.Settings.Default.sett_ip_2 = textBox3.Text;
            Properties.Settings.Default.sett_ip_3 = textBox4.Text;
            Properties.Settings.Default.sett_ip_4 = textBox5.Text;
            Properties.Settings.Default.Save();
        }
    }
}
