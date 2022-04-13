using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pc_monitor_1
{
    public partial class set_freq_form_8 : Form
    {
        public GetEventHandler freq_send_event;

        public set_freq_form_8()
        {
            InitializeComponent();
        }

        public void set_textbox_from_main(string cmd, string data, string[] data_arr)
        {
            if(cmd == "SET_FREQ_FORM_TEXTBOX_SET_DATA")
            {
                textBox1.Text = data;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            freq_send_event(cmd: "SET_FREQ_FORM_FREQ_SET_DATA", data: textBox1.Text);
            this.Close();
        }
    }
}
