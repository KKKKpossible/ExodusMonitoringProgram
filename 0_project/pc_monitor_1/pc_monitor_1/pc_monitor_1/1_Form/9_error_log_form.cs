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
using static pc_monitor_1._2_Data.SjkimCmd;

namespace pc_monitor_1
{
    public partial class error_log_form_9 : Form
    {
        public GetEventHandler error_form_send_event;
        int now_length_2 = 0;

        int now_index;
        string now_time;
        string now_cmd;
        string now_value_0;
        string now_value_1;

        DataTable data_table_load = new DataTable();
        DataTable data_table_buffer = new DataTable();

        public error_log_form_9()
        {
            InitializeComponent();
            dataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            /*
            foreach (DataGridViewColumn item in dataGridView2.Columns)
            {
                item.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            */
        }

        // load operate 100 clicked
        private void button8_Click(object sender, EventArgs e)
        {
            if (dataGridView2.InvokeRequired == true)
            {
                dataGridView2.Invoke((MethodInvoker)delegate
                {
                    button8.Enabled = false;
                    button7.Enabled = false;
                    dataGridView2.Rows.Clear();
                    dataGridView2.Refresh();
                });
            }
            else
            {
                button8.Enabled = false;
                button7.Enabled = false;
                dataGridView2.Rows.Clear();
                dataGridView2.Refresh();
            }
            now_index = 0;
            error_form_send_event(cmd: "OPERATE_LOG_FORM_LOAD_100");
        }

        // load operate 1000 clicked
        private void button7_Click(object sender, EventArgs e)
        {
            if (dataGridView2.InvokeRequired == true)
            {
                dataGridView2.Invoke((MethodInvoker)delegate
                {
                    button7.Enabled = false;
                    button8.Enabled = false;
                    dataGridView2.Rows.Clear();
                    dataGridView2.Refresh();
                });
            }
            else
            {
                button8.Enabled = false;
                button7.Enabled = false;
                dataGridView2.Rows.Clear();
                dataGridView2.Refresh();
            }
            now_index = 0;
            error_form_send_event(cmd: "OPERATE_LOG_FORM_LOAD_1000");
        }

        public void load_operate_100(string cmd, string data, string[] data_arr)
        {
            if (cmd == "OPERATE_LOG_FORM_LOAD_100")
            {
                if (now_length_2 != 0)
                {
                    dataGridView2.Rows.Clear();
                    dataGridView2.Refresh();
                }

                for (int i = 0; i < 100; i++)
                {
                    dataGridView2.Rows.Add(i, data_arr[i], data_arr[i + 100], data_arr[i + 200], data_arr[i + 300]);
                }
                now_length_2 = 100;
            }
        }

        public void load_operate_1000(string cmd, string data, string[] data_arr)
        {
            if (cmd == "OPERATE_LOG_FORM_LOAD_1000")
            {
                if (now_length_2 != 0)
                {
                    dataGridView2.Rows.Clear();
                    dataGridView2.Refresh();
                }

                for (int i = 0; i < 1000; i++)
                {
                    dataGridView2.Rows.Add(i, data_arr[i], data_arr[i + 1000], data_arr[i + 2000], data_arr[i + 3000]);
                }
                now_length_2 = 1000;
            }
        }

        // operate delete clicked
        private void button6_Click(object sender, EventArgs e)
        {
            if (dataGridView2.InvokeRequired == true)
            {
                dataGridView2.Invoke((MethodInvoker)delegate
                {
                    button8.Enabled = true;
                    button7.Enabled = true;
                    dataGridView2.Rows.Clear();
                    dataGridView2.Refresh();
                });
            }
            else
            {
                button8.Enabled = true;
                button7.Enabled = true;
                dataGridView2.Rows.Clear();
                dataGridView2.Refresh();
            }
        }

        // error file save clicked
        private void button4_Click(object sender, EventArgs e)
        {

        }

        // operate file save clicked
        private void button5_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text file|*.txt";
            saveFileDialog1.ShowDialog();
            string path = saveFileDialog1.FileName;
            if (path != "")
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        for (int j = 0; j < (int)dataGridView2.Rows[i].Cells.Count; j++)
                        {
                            sw.Write(dataGridView2.Rows[i].Cells[j].Value.ToString() + " ");
                        }
                        sw.WriteLine();
                    }

                }
            }
            
        }

        private void error_log_form_9_Load(object sender, EventArgs e)
        {
            if ((DataTable)dataGridView2.DataSource != null)
            {
                ((DataTable)dataGridView2.DataSource).Rows.Clear();
            }

            data_table_load.Columns.Add("Seq", typeof(string));
            data_table_load.Columns.Add("Time", typeof(string));
            data_table_load.Columns.Add("Code", typeof(string));
            data_table_load.Columns.Add("Value 0", typeof(string));
            data_table_load.Columns.Add("Value 1", typeof(string));

            data_table_buffer.Columns.Add("Seq", typeof(string));
            data_table_buffer.Columns.Add("Time", typeof(string));
            data_table_buffer.Columns.Add("Code", typeof(string));
            data_table_buffer.Columns.Add("Value 0", typeof(string));
            data_table_buffer.Columns.Add("Value 1", typeof(string));
        }

        private void error_log_form_9_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((DataTable)dataGridView2.DataSource != null)
            {
                ((DataTable)dataGridView2.DataSource).Rows.Clear();
            }
        }


        /*
        public enum CmdList_enum
        {
            ONLINE = 0,         // 0
            STANDBY,            // 1
            ALC_ON,             // 2
            ALC_OFF,            // 3
            ALC_SET,            // 4
            AGC_SET,            // 5
            ALC_READ,           // 6
            AGC_READ,           // 7
            SET_TIME,           // 8
            FREQ_SET,           // 9
            SERIAL_START,       // 10
            USB_START,          // 11
            ETHERNETSTART,      // 12
            SERIAL_STOP,        // 13 
            USB_STOP,           // 14
            ETHERNET_STOP,      // 15
            IP_SET,             // 16
            IP_GET,             // 17
            LOG_LOAD_100,       // 18
            LOG_LOAD_1000,      // 19
            PARA_SET_0,         // 20
            PARA_GET_0,         // 21
            PARA_GET_1,         // 22
            FREQ_PWR_GET,       // 23
            FREQ_PWR_SET,       // 24
            FREQ_INPUT_GET,     // 25
            FREQ_INPUT_SET,     // 26
            NORMAL,             // 27
            FAULT_LOG,          // 28
            LENGTH
        }

        public enum Faultmask_enum
        {
            FAULT_FWD = 1,      // 1
            FAULT_VSWR,         // 2
            FAULT_INPUT,        // 3
            FAULT_TEMPERATURE,  // 4
            FAULT_CURRENT,      // 5
            FAULT_INTERLOCK,    // 6
            FAULT_VOLTAGE,      // 7
            FAULT_INTERNAL,     // 8
            FAULT_MASK_LENGTH
        };
        */

        public void set_datagrid_from_main(string cmd, string data, string[] data_arr)
        {
            if (cmd == "ERROR_LOG_CLEAR")
            {
                if (dataGridView2.InvokeRequired == true)
                {
                    dataGridView2.Invoke((MethodInvoker)delegate
                    {
                        dataGridView2.Rows.Clear();
                        dataGridView2.Refresh();
                    });
                }
                else
                {
                    dataGridView2.Rows.Clear();
                    dataGridView2.Refresh();
                }
            }
            else if (cmd == "ERROR_LOG_WRITE_INDEX")
            {
                // now_index = Int32.Parse(data);
            }
            else if (cmd == "ERROR_LOG_WRITE_TIME")
            {
                now_index += 1;
                now_time = data;
            }
            else if (cmd == "ERROR_LOG_WRITE_CMD")
            {
                int int_data = int.Parse(data);
                string string_data = string.Empty;

                switch (int_data)
                {
                    case (int)CmdList_enum.ONLINE:  // 0
                        string_data = "ONLINE";  
                        break;
                    case (int)CmdList_enum.STANDBY:  // 1
                        string_data = "STANDBY";
                        break;
                    case (int)CmdList_enum.ALC_ON:  // 2
                        string_data = "ALC_ON";
                        break;
                    case (int)CmdList_enum.ALC_OFF:  // 3
                        string_data = "ALC_OFF";
                        break;
                    case (int)CmdList_enum.ALC_SET:  // 4
                        string_data = "ALC_SET";
                        break;
                    case (int)CmdList_enum.AGC_SET:  // 5
                        string_data = "AGC_SET";
                        break;
                    case (int)CmdList_enum.ALC_READ:  // 6
                        string_data = "ALC_READ";
                        break;
                    case (int)CmdList_enum.AGC_READ:  // 7
                        string_data = "AGC_READ";
                        break;
                    case (int)CmdList_enum.SET_TIME:  // 8
                        string_data = "SET_TIME";
                        break;
                    case (int)CmdList_enum.FREQ_SET:  // 9
                        string_data = "FREQ_SET";
                        break;
                    case (int)CmdList_enum.SERIAL_START:  // 10
                        string_data = "SERIAL_START";
                        break;
                    case (int)CmdList_enum.USB_START:  // 11
                        string_data = "USB_START";
                        break;
                    case (int)CmdList_enum.ETHERNETSTART:  // 12
                        string_data = "ETHERNETSTART";
                        break;
                    case (int)CmdList_enum.SERIAL_STOP:  // 13
                        string_data = "SERIAL_STOP";
                        break;
                    case (int)CmdList_enum.USB_STOP:  // 14
                        string_data = "USB_STOP";
                        break;
                    case (int)CmdList_enum.ETHERNET_STOP:  // 15
                        string_data = "ETHERNET_STOP";
                        break;
                    case (int)CmdList_enum.IP_SET:  // 16
                        string_data = "IP_SET";
                        break;
                    case (int)CmdList_enum.IP_GET:  // 17
                        string_data = "IP_GET";
                        break;
                    case (int)CmdList_enum.LOG_LOAD_100:  // 18
                        string_data = "LOG_LOAD_100";
                        break;
                    case (int)CmdList_enum.LOG_LOAD_1000:  // 19
                        string_data = "LOG_LOAD_1000";
                        break;
                    case (int)CmdList_enum.PARA_SET_0:  // 20
                        string_data = "PARA_SET_0";
                        break;
                    case (int)CmdList_enum.PARA_GET_0:  // 21
                        string_data = "PARA_GET_0";
                        break;
                    case (int)CmdList_enum.PARA_GET_1:  // 22
                        string_data = "PARA_GET_1";
                        break;
                    case (int)CmdList_enum.FREQ_PWR_GET:  // 23
                        string_data = "FREQ_PWR_GET";
                        break;
                    case (int)CmdList_enum.FREQ_PWR_SET:  // 24
                        string_data = "FREQ_PWR_SET";
                        break;
                    case (int)CmdList_enum.FREQ_INPUT_GET:  // 25
                        string_data = "FREQ_INPUT_GET";
                        break;
                    case (int)CmdList_enum.FREQ_INPUT_SET:  // 26
                        string_data = "FREQ_INPUT_SET";
                        break;
                    case (int)CmdList_enum.NORMAL:  // 27
                        string_data = "NORMAL";
                        break;
                    case (int)CmdList_enum.FAULT_LOG:  // 28
                        string_data = "FAULT_LOG";
                        break;
                    case (int)CmdList_enum.FAULT_LOG + (int)Faultmask_enum.FAULT_FWD:  // 28 + 1
                        string_data = "FAULT_FWD";
                        break;
                    case (int)CmdList_enum.FAULT_LOG + (int)Faultmask_enum.FAULT_VSWR:  // 28 + 1
                        string_data = "FAULT_VSWR";
                        break;
                    case (int)CmdList_enum.FAULT_LOG + (int)Faultmask_enum.FAULT_INPUT:  // 28 + 1
                        string_data = "FAULT_INPUT";
                        break;
                    case (int)CmdList_enum.FAULT_LOG + (int)Faultmask_enum.FAULT_TEMPERATURE:  // 28 + 1
                        string_data = "FAULT_TEMPERATURE";
                        break;
                    case (int)CmdList_enum.FAULT_LOG + (int)Faultmask_enum.FAULT_CURRENT:  // 28 + 1
                        string_data = "FAULT_CURRENT";
                        break;
                    case (int)CmdList_enum.FAULT_LOG + (int)Faultmask_enum.FAULT_INTERLOCK:  // 28 + 1
                        string_data = "FAULT_INTERLOCK";
                        break;
                    case (int)CmdList_enum.FAULT_LOG + (int)Faultmask_enum.FAULT_VOLTAGE:  // 28 + 1
                        string_data = "FAULT_VOLTAGE";
                        break;
                    case (int)CmdList_enum.FAULT_LOG + (int)Faultmask_enum.FAULT_INTERNAL:  // 28 + 1
                        string_data = "FAULT_INTERNAL";
                        break;
                    default:
                        string_data = "UNKNOWN CODE: " + int_data.ToString();
                        break;
                }

                now_cmd = string_data;

            }
            else if (cmd == "ERROR_LOG_WRITE_VALUE_0")
            {
                now_value_0 = data;
            }
            else if (cmd == "ERROR_LOG_WRITE_VALUE_1")
            {
                now_value_1 = data;
                switch(now_cmd)
                {
                    case "IP_SET":
                        {
                            double ip_raw_0 = float.Parse(now_value_0);
                            double ip_raw_1 = float.Parse(now_value_1);

                            int[] ip_buffer = new int[4];
                            ip_buffer[0] = (int)ip_raw_0;
                            ip_buffer[1] = ((int)(Math.Round(ip_raw_0 * 1000.0F, 1))) % 1000;
                            ip_buffer[2] = (int)ip_raw_1;
                            ip_buffer[3] = ((int)(Math.Round(ip_raw_1 * 1000.0F, 1))) % 1000;
                            now_value_0 = ip_buffer[0].ToString() + "." + ip_buffer[1].ToString() + "." + ip_buffer[2].ToString() + "." + ip_buffer[3].ToString();
                            now_value_1 = "";
                        }
                        break;
                    default:
                        break;
                }
                if (dataGridView2.InvokeRequired == true)
                {
                    dataGridView2.Invoke((MethodInvoker)delegate
                    {
                        dataGridView2.Rows.Add(now_index, now_time, now_cmd, now_value_0, now_value_1);
                    });
                }
                else
                {
                    dataGridView2.Rows.Add(now_index, now_time, now_cmd, now_value_0, now_value_1);
                }
                now_time = "";
                now_cmd = "";
                now_value_0 = "";
                now_value_1 = "";
            }
            else if (cmd == "ERROR_LOG_WRITE_DONE")
            {
                if (dataGridView2.InvokeRequired == true)
                {
                    dataGridView2.Invoke((MethodInvoker)delegate
                    {
                        button8.Enabled = true;
                        button7.Enabled = true;
                        dataGridView2.Refresh();

                        data_table_load.Clear();
                        data_table_buffer.Clear();
                        for(int i = 0; i < dataGridView2.Rows.Count; i++)
                        {
                            data_table_load.Rows.Add(dataGridView2.Rows[i].Cells[0].Value, dataGridView2.Rows[i].Cells[1].Value, dataGridView2.Rows[i].Cells[2].Value, dataGridView2.Rows[i].Cells[3].Value, dataGridView2.Rows[i].Cells[4].Value);
                        }
                    });
                }
                else
                {
                    button8.Enabled = true;
                    button7.Enabled = true;
                    dataGridView2.Refresh();

                    data_table_load.Clear();
                    data_table_buffer.Clear();
                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        data_table_load.Rows.Add(dataGridView2.Rows[i].Cells[0].Value, dataGridView2.Rows[i].Cells[1].Value, dataGridView2.Rows[i].Cells[2].Value, dataGridView2.Rows[i].Cells[3].Value, dataGridView2.Rows[i].Cells[4].Value);
                    }
                }
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            data_table_buffer = data_table_load.Copy();
            if (dataGridView2.InvokeRequired == true)
            {
                dataGridView2.Invoke((MethodInvoker)delegate
                {
                    button8.Enabled = true;
                    button7.Enabled = true;
                    dataGridView2.Rows.Clear();
                    dataGridView2.Refresh();
                    for (int i = 0; i < data_table_buffer.Rows.Count; i++)
                    {
                        dataGridView2.Rows.Add(data_table_buffer.Rows[i].ItemArray[0], data_table_buffer.Rows[i].ItemArray[1], data_table_buffer.Rows[i].ItemArray[2], data_table_buffer.Rows[i].ItemArray[3], data_table_buffer.Rows[i].ItemArray[4]);
                    }
                });
            }
            else
            {
                button8.Enabled = true;
                button7.Enabled = true;
                dataGridView2.Rows.Clear();
                dataGridView2.Refresh();
                for (int i = 0; i < data_table_buffer.Rows.Count; i++)
                {
                    dataGridView2.Rows.Add(data_table_buffer.Rows[i].ItemArray[0], data_table_buffer.Rows[i].ItemArray[1], data_table_buffer.Rows[i].ItemArray[2], data_table_buffer.Rows[i].ItemArray[3], data_table_buffer.Rows[i].ItemArray[4]);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            data_table_buffer = data_table_load.Copy();
            if (dataGridView2.InvokeRequired == true)
            {
                dataGridView2.Invoke((MethodInvoker)delegate
                {
                    button8.Enabled = true;
                    button7.Enabled = true;
                    dataGridView2.Rows.Clear();
                    dataGridView2.Refresh();
                    for (int i = 0; i < data_table_buffer.Rows.Count; i++)
                    {
                        if(data_table_buffer.Rows[i].ItemArray[2].ToString().Contains("FAULT") == false)
                        {
                            dataGridView2.Rows.Add(data_table_buffer.Rows[i].ItemArray[0], data_table_buffer.Rows[i].ItemArray[1], data_table_buffer.Rows[i].ItemArray[2], data_table_buffer.Rows[i].ItemArray[3], data_table_buffer.Rows[i].ItemArray[4]);
                        }
                    }
                });
            }
            else
            {
                button8.Enabled = true;
                button7.Enabled = true;
                dataGridView2.Rows.Clear();
                dataGridView2.Refresh();
                for (int i = 0; i < data_table_buffer.Rows.Count; i++)
                {
                    if (data_table_buffer.Rows[i].ItemArray[2].ToString().Contains("FAULT") == false)
                    {
                        dataGridView2.Rows.Add(data_table_buffer.Rows[i].ItemArray[0], data_table_buffer.Rows[i].ItemArray[1], data_table_buffer.Rows[i].ItemArray[2], data_table_buffer.Rows[i].ItemArray[3], data_table_buffer.Rows[i].ItemArray[4]);
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            data_table_buffer = data_table_load.Copy();
            if (dataGridView2.InvokeRequired == true)
            {
                dataGridView2.Invoke((MethodInvoker)delegate
                {
                    button8.Enabled = true;
                    button7.Enabled = true;
                    dataGridView2.Rows.Clear();
                    dataGridView2.Refresh();
                    for (int i = 0; i < data_table_buffer.Rows.Count; i++)
                    {
                        if (data_table_buffer.Rows[i].ItemArray[2].ToString().Contains("FAULT") == true)
                        {
                            dataGridView2.Rows.Add(data_table_buffer.Rows[i].ItemArray[0], data_table_buffer.Rows[i].ItemArray[1], data_table_buffer.Rows[i].ItemArray[2], data_table_buffer.Rows[i].ItemArray[3], data_table_buffer.Rows[i].ItemArray[4]);
                        }
                    }
                });
            }
            else
            {
                button8.Enabled = true;
                button7.Enabled = true;
                dataGridView2.Rows.Clear();
                dataGridView2.Refresh();
                for (int i = 0; i < data_table_buffer.Rows.Count; i++)
                {
                    if (data_table_buffer.Rows[i].ItemArray[2].ToString().Contains("FAULT") == true)
                    {
                        dataGridView2.Rows.Add(data_table_buffer.Rows[i].ItemArray[0], data_table_buffer.Rows[i].ItemArray[1], data_table_buffer.Rows[i].ItemArray[2], data_table_buffer.Rows[i].ItemArray[3], data_table_buffer.Rows[i].ItemArray[4]);
                    }
                }
            }
        }
    }
}
