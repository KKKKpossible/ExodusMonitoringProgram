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
    public partial class alc_agc_set_form_12 : Form
    {
        public GetEventHandler alc_agc_event;

        public alc_agc_set_form_12()
        {
            InitializeComponent();
        }

        // agc set
        private void button1_Click(object sender, EventArgs e)
        {
            alc_agc_event(cmd: "agc", data: textBox1.Text);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            alc_agc_event(cmd: "alc", data: textBox2.Text);
            this.Close();
        }

        public void set_textbox_from_main(string cmd, string data, string[] data_arr)
        {
            // cmd check
            if(cmd == "ALC_AGC_FORM_TEXTBOX_SET_DATA_ARR")
            {
                textBox1.Text = data_arr[0];
                textBox2.Text = data_arr[1];
            }
        }
    }
}
