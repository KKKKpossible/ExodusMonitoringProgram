using pc_monitor_1._0_Instance;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static pc_monitor_1._2_Data.SjkimData;

namespace pc_monitor_1
{
    public partial class login_form_10 : Form
    {
        const string password = "1234";
        string text_input = string.Empty;
        public bool opend;

        public GetEventHandler login_send_event;

        public login_form_10()
        {
            InitializeComponent();
            opend = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            text_input = textBox1.Text;
            if(opend == false)
            {
                if (text_input == password)
                {
                    login_send_event(data: "true");
                    opend = true;
                }
                else
                {
                    login_send_event(data: "false");
                }
            }
            else
            {
                login_send_event(data: "true");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
