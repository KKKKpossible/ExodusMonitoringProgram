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
    public partial class parameter_select_form_2 : Form
    {
        public GetEventHandler parameter_send_event;
        /*0_para
        1_threshold
        2_freq_pwr_mea
        3_freq_pwr_proc
        4_input_mea
        5_input_proc*/
        public parameter_select_form_2()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            parameter_send_event(data: "0_para");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            parameter_send_event(data: "1_freq_pwr_mea");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            parameter_send_event(data: "2_freq_pwr_proc");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            parameter_send_event(data: "3_input_mea");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            parameter_send_event(data: "4_input_proc");
        }

    }
}
