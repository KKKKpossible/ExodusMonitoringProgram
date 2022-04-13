using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static pc_monitor_1._2_Data.SjkimData;
using System.Windows.Forms;
using System.Drawing;

namespace pc_monitor_1._0_Instance
{
    internal class SjkimInstance
    {
        public bool[] g_control_button;
        public Form[] sjkim_forms;
        public DataInstance data_instance;

        public SjkimInstance()
        {
            g_control_button = new bool[(int)ControlButton_enum.LENGTH];
            sjkim_forms = new Form[(int)Form_enum.LENGTH];

            data_instance = new DataInstance();
        }
    }
}
