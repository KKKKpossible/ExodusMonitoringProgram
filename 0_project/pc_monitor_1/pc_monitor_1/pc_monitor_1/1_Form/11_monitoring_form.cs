using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pc_monitor_1._1_Form
{
    public partial class monitoring_form_11 : Form
    {
        public GetEventHandler monitoring_event;


        public monitoring_form_11()
        {
            InitializeComponent();
        }

        public void set_textbox_from_main(string cmd, string data, string[] data_arr)
        {
            if(this.InvokeRequired == true)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (cmd == "LOCAL_ON")
                    {
                        button1.BackColor = Color.Red;
                        button2.BackColor = DefaultBackColor;
                    }
                    if (cmd == "CMD_ON")
                    {
                        button1.BackColor = DefaultBackColor;
                        button2.BackColor = Color.Red;
                    }
                    if (cmd == "RECEIVED_DATA")
                    {
                        if (button2.BackColor == Color.Red)
                        {
                            textBox3.Text += data;
                        }
                    }
                });
            }    
            else
            {
                if (cmd == "LOCAL_ON")
                {
                    button1.BackColor = Color.Red;
                    button2.BackColor = DefaultBackColor;
                }
                if (cmd == "CMD_ON")
                {
                    button1.BackColor = DefaultBackColor;
                    button2.BackColor = Color.Red;
                }
                if (cmd == "RECEIVED_DATA")
                {
                    if (button2.BackColor == Color.Red)
                    {
                        textBox3.Text += data;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                monitoring_event(cmd: "LOCAL_PUSHED");


                if (button1.InvokeRequired == true)
                {
                    button1.Invoke((MethodInvoker)delegate
                    {
                        button1.BackColor = Color.Red;
                        button2.BackColor = DefaultBackColor;
                    });
                }
                else
                {
                    button1.BackColor = Color.Red;
                    button2.BackColor = DefaultBackColor;
                }
            }
            catch
            {

            }
        }

        void cmd_push_procedure()
        {
            try
            {
                monitoring_event(cmd: "COMMAND_PUSHED");
                textBox3.Text = "";
                if (button1.InvokeRequired == true)
                {
                    button1.Invoke((MethodInvoker)delegate
                    {
                        button1.BackColor = DefaultBackColor;
                        button2.BackColor = Color.Red;
                    });
                }
                else
                {
                    button1.BackColor = DefaultBackColor;
                    button2.BackColor = Color.Red;
                }
            }
            catch
            {

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                cmd_push_procedure();
            }
            catch
            {

            }
        }

        private void cmd_transmit_pushed()
        {
            if (textBox1.InvokeRequired == true)
            {
                textBox1.Invoke((MethodInvoker)delegate
                {

                    monitoring_event(cmd: "SEND_PUSHED", data: textBox1.Text + "\n");
                    textBox2.Text = "";
                    textBox3.Text = "";
                    textBox2.Text = textBox1.Text;
                });
            }
            else
            {
                monitoring_event(cmd: "SEND_PUSHED", data: textBox1.Text + "\n");
                textBox2.Text = "";
                textBox3.Text = "";
                textBox2.Text = textBox1.Text;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (button1.BackColor == Color.Red)
                {
                    cmd_push_procedure();
                    textBox2.Text = "";
                    textBox2.Text = "PLEASE SEND AGAIN";
                }
                else if (button2.BackColor == Color.Red)
                {
                    cmd_transmit_pushed();
                }
            }
            catch
            {

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox2.InvokeRequired == true)
                {
                    textBox2.Invoke((MethodInvoker)delegate
                    {

                        textBox2.Text = "";
                    });
                }
                else
                {
                    textBox2.Text = "";
                }
            }
            catch
            {

            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox3.InvokeRequired == true)
                {
                    textBox3.Invoke((MethodInvoker)delegate
                    {
                        textBox3.Text = "";
                    });
                }
                else
                {
                    textBox3.Text = "";
                }
            }
            catch
            {

            }
        }

        private void monitoring_form_11_Load(object sender, EventArgs e)
        {
            monitoring_event(cmd: "OPENED");
        }

        private void monitoring_form_11_FormClosing(object sender, FormClosingEventArgs e)
        {

            monitoring_event(cmd: "CLOSED");
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    {
                        try
                        {
                            if(button1.BackColor == Color.Red)
                            {
                                cmd_push_procedure();
                                textBox2.Text = "";
                                textBox2.Text = "PLEASE SEND AGAIN";
                            }
                            else if(button2.BackColor == Color.Red)
                            {
                                cmd_transmit_pushed();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
