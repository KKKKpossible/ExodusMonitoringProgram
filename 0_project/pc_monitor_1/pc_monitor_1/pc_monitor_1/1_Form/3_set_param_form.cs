using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static pc_monitor_1._2_Data.SjkimData;

namespace pc_monitor_1
{
    public partial class set_param_form_3 : Form
    {
        private bool opend_before = false;

        public GetEventHandler set_para_form_send_event;

        string display_all_channel;
        string display_all_value;
        Thread setter_thread;
        bool display_all_data_pushed = false;
        bool thread_busy = false;
        bool toggle_pushed = false;

        public set_param_form_3()
        {
            InitializeComponent();

            this.Size = new Size(1251, 745);
            DataGridSet(0);
            DataGridSet(1);
            DataGridSet(2);
        }

        private void DataGridSet(int num)
        {
            switch(num)
            {
                // data grid 0 설정
                case 0:
                    dataGridView1.Rows.Add("Reflected - Coupling Value: 0", 0);
                    dataGridView1.Rows.Add("Reflected - Reference Voltage: 0", 0);
                    dataGridView1.Rows.Add("Reflected - Step Value: 0", 0);
                    dataGridView1.Rows.Add("Reflected - Coupling Value: 1", 0);
                    dataGridView1.Rows.Add("Reflected - Reference Voltage: 1", 0);
                    dataGridView1.Rows.Add("Reflected - Step Value: 1", 0);
                    dataGridView1.Rows.Add("Voltage Rate:0", 0);
                    dataGridView1.Rows.Add("Voltage Rate:1", 0);
                    dataGridView1.Rows.Add("Current Rate:0", 0);
                    dataGridView1.Rows.Add("Current Offset:0", 0);
                    dataGridView1.Rows.Add("Current Rate:1", 0);
                    dataGridView1.Rows.Add("Current Offset:1", 0);
                    dataGridView1.Rows.Add("Current Rate:2", 0);
                    dataGridView1.Rows.Add("Current Offset:2", 0);
                    dataGridView1.Rows.Add("Current Rate:3", 0);
                    dataGridView1.Rows.Add("Current Offset:3", 0);
                    dataGridView1.Rows.Add("Current Rate:4", 0);
                    dataGridView1.Rows.Add("Current Offset:4", 0);
                    dataGridView1.Rows.Add("Temperature Rate:0", 100);
                    dataGridView1.Rows.Add("Temperature Offset:0", -500);
                    dataGridView1.Rows.Add("Temperature Rate:1", 100);
                    dataGridView1.Rows.Add("Temperature Offset:1", 0);
                    dataGridView1.Rows.Add("Temperature Rate:2", 100);
                    dataGridView1.Rows.Add("Temperature Offset 2", -500);

                    // column 0
                    dataGridView1.Columns[0].ReadOnly = true;
                    dataGridView1.Columns[0].Width = 350;

                    // column 1
                    dataGridView1.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView1.BackgroundColor = Color.FromArgb(255, 255, 255);

                    for(int i = 0; i < dataGridView1.RowCount; i++)
                    {
                        dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.FromArgb(70, 255, 255);
                    }
                    foreach (DataGridViewColumn item in dataGridView1.Columns)
                    {
                        item.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }

                    break;
                // data grid 1 설정
                case 1:
                    dataGridView2.Rows.Add("VOLT - Set: 0", 0);
                    dataGridView2.Rows.Add("VOLT - Tolerance: 0", 0);
                    dataGridView2.Rows.Add("VOLT - Set: 1", 0);
                    dataGridView2.Rows.Add("VOLT - Tolerance: 1", 0);
                    dataGridView2.Rows.Add("Over Current Value: 0", 0);
                    dataGridView2.Rows.Add("Over Current Value: 1", 0);
                    dataGridView2.Rows.Add("Over Current Value: 2", 0);
                    dataGridView2.Rows.Add("Over Current Value: 3", 0);
                    dataGridView2.Rows.Add("Over Current Value: 4", 0);
                    dataGridView2.Rows.Add("Input Duty Threshold Voltage(V+-)", 0);
                    dataGridView2.Rows.Add("Input Signal Limit Voltage: 0(V+-)", 0);
                    dataGridView2.Rows.Add("Over Input Pwr Alarm Level: 0(dBm)", 0);
                    dataGridView2.Rows.Add("Output Signal Limit voltage: 0(V+-)", 0);
                    dataGridView2.Rows.Add("Over Fwd Pwr Alarm Level: 0", 0);
                    dataGridView2.Rows.Add("VSWR Alarm Level", 0);
                    dataGridView2.Rows.Add("Averaging time in millisec", 0);
                    dataGridView2.Rows.Add("ALC Max Power(dBm)", 0);
                    dataGridView2.Rows.Add("DUTY Alarm Level(%)", 0);
                    dataGridView2.Rows.Add("Online TTL(0=Low, 1=High)", 0);
                    dataGridView2.Rows.Add("DAC Voltage Limit Low", 0);
                    dataGridView2.Rows.Add("DAC Voltage Limit High", 0);
                    dataGridView2.Rows.Add("InterLock TTL(0=Low, 1=High)", 0);

                    dataGridView2.EnableHeadersVisualStyles = false;
                    // column 1
                    dataGridView2.Columns[0].ReadOnly = true;
                    dataGridView2.Columns[0].Width = 350;
                    // column 1
                    dataGridView2.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView2.BackgroundColor = Color.FromArgb(255, 255, 255);

                    for (int i = 0; i < dataGridView2.RowCount; i++)
                    {
                        dataGridView2.Rows[i].Cells[1].Style.BackColor = Color.FromArgb(70, 255, 255);
                    }

                    foreach (DataGridViewColumn item in dataGridView2.Columns)
                    {
                        item.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    break;
                case 2:
                    {
                        dataGridView3.Rows.Add("Forward voltage_0", 0);      // 0.. forward 0
                        dataGridView3.Rows.Add("Forward voltage_1", 0);      // 1.. forward 1
                        dataGridView3.Rows.Add("Reflected voltage_0", 0);    // 2.. reflect 0
                        dataGridView3.Rows.Add("Reflected voltage_1", 0);    // 3.. reflect 1
                        dataGridView3.Rows.Add("Input voltage_0", 0);        // 4.. input 0
                        dataGridView3.Rows.Add("Input voltage_1", 0);        // 5.. input 1
                        dataGridView3.Rows.Add("Voltage voltage_0", 0);      // 6.. voltage 0
                        dataGridView3.Rows.Add("Voltage voltage_1", 0);      // 7.. voltage 1
                        dataGridView3.Rows.Add("Current voltage_0", 0);      // 8.. current 0
                        dataGridView3.Rows.Add("Current voltage_1", 0);      // 9.. current 1
                        dataGridView3.Rows.Add("Current voltage_2", 0);      // 10.. current 2
                        dataGridView3.Rows.Add("Current voltage_3", 0);      // 11.. current 3
                        dataGridView3.Rows.Add("Current voltage_4", 0);      // 12.. current 4
                        dataGridView3.Rows.Add("Temperature voltage_0", 0);  // 13.. temp 0
                        dataGridView3.Rows.Add("Temperature voltage_1", 0);  // 14.. temp 1
                        dataGridView3.Rows.Add("Temperature voltage_2", 0);  // 15.. temp 2
                        dataGridView3.Rows.Add("Online state", 0);           // 16.. online gpio state
                        dataGridView3.Rows.Add("Interlock state", 0);        // 17.. interlock gpio state
                        dataGridView3.Rows.Add("DAC voltage", 0);            // 18.. dac_0 voltage

                        dataGridView3.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dataGridView3.BackgroundColor = Color.FromArgb(255, 255, 255);

                        for (int i = 0; i < dataGridView3.RowCount; i++)
                        {
                            dataGridView3.Rows[i].Cells[1].Style.BackColor = Color.FromArgb(70, 255, 255);
                        }
                        break;
                    }
                default:
                    MessageBox.Show("unknown datagrid num");
                    break;
            }
        }

        private void datagrid_set_thread_all_once()
        {
            while(true)
            {
                if(display_all_data_pushed == true)
                {
                    thread_busy = true;
                    display_all_data_pushed = false;

                    if (dataGridView3.InvokeRequired == true)
                    {
                        dataGridView3.Invoke((MethodInvoker)delegate
                        {
                            if (int.Parse(display_all_channel) < (int)All_para_voltage_display_enum.ALL_DISPLAY_LENGTH)
                            {
                                for (int i = 0; i < dataGridView3.Rows.Count; i++)
                                {
                                    dataGridView3.Rows[i].Cells[1].Style.BackColor = Color.FromArgb(70, 255, 255);
                                }
                                if (dataGridView3.RowCount > 0)
                                {
                                    dataGridView3.Rows[int.Parse(display_all_channel)].Cells[1].Value = string.Format("{0:0.000}", float.Parse(display_all_value));
                                    dataGridView3.Rows[int.Parse(display_all_channel)].Cells[1].Style.BackColor = Color.Red;
                                }
                            }
                            else
                            {
                                dataGridView3.Refresh();
                            }
                        });
                    }
                    else
                    {
                        if (int.Parse(display_all_channel) < (int)All_para_voltage_display_enum.ALL_DISPLAY_LENGTH)
                        {
                            for (int i = 0; i < dataGridView3.Rows.Count; i++)
                            {
                                dataGridView3.Rows[i].Cells[1].Style.BackColor = Color.FromArgb(70, 255, 255);
                            }
                            if (dataGridView3.RowCount > 0)
                            {
                                dataGridView3.Rows[int.Parse(display_all_channel)].Cells[1].Value = string.Format("{0:0.000}", float.Parse(display_all_value));
                                dataGridView3.Rows[int.Parse(display_all_channel)].Cells[1].Style.BackColor = Color.Red;
                            }
                        }
                        else
                        {
                            dataGridView3.Refresh();
                        }
                    }

                    thread_busy = false;
                }
            }
        }

        public void set_datagrid_from_main(string cmd, string data, string[] data_arr)
        {
            try
            {
                if(cmd == "SET_DISPLAY_ALL_DATA")
                {
                    if(toggle_pushed == true)
                    {
                        if (setter_thread == null)
                        {
                            setter_thread = new Thread(() => datagrid_set_thread_all_once());
                            setter_thread.IsBackground = true;
                            setter_thread.Start();
                        }
                        if (thread_busy == false)
                        {
                            display_all_channel = data_arr[0];
                            display_all_value = data_arr[1];
                            display_all_data_pushed = true;
                        }
                    }
                }

                if (cmd == "SET_PARAM_FORM_DATAGRID_SET_DATA_ARR")
                {

                    if (dataGridView1.InvokeRequired == true)
                    {
                        dataGridView1.Invoke((MethodInvoker)delegate
                        {
                            for (int i = 0; i < (int)SetParaADC_enum.LENGTH; i++)
                            {
                                dataGridView1.Rows[i].Cells[1].Value = data_arr[i];
                            }
                        });
                    }
                    else  // 아래는 진입하지 않는 코드..
                    {
                        for (int i = 0; i < (int)SetParaADC_enum.LENGTH; i++)
                        {
                            dataGridView1.Rows[i].Cells[1].Value = data_arr[i];
                        }
                        dataGridView1.Refresh();
                    }

                    if (dataGridView2.InvokeRequired == true)
                    {
                        dataGridView1.Invoke((MethodInvoker)delegate
                        {
                            for (int i = 0; i < (int)SetParaTHRESHOLD_enum.LENGTH; i++)
                            {
                                dataGridView2.Rows[i].Cells[1].Value = data_arr[i + (int)SetParaADC_enum.LENGTH];
                            }
                        });
                    }
                    else
                    {
                        for (int i = 0; i < (int)SetParaTHRESHOLD_enum.LENGTH; i++)
                        {
                            dataGridView2.Rows[i].Cells[1].Value = data_arr[i + (int)SetParaADC_enum.LENGTH];
                        }
                        dataGridView2.Refresh();
                    }
                    if(opend_before == true)
                    {
                        if (textBox1.InvokeRequired == true)
                        {
                            textBox1.Invoke((MethodInvoker)delegate
                            {
                                textBox1.Text = "Read done";
                            });
                        }
                        else
                        {
                            textBox1.Text = "Read done";
                        }
                    }
                    opend_before = true;
                }

                if(cmd == "SET_PARAM_FORM_PARAM_WRITED")
                {
                    if(textBox1.InvokeRequired == true)
                    {
                        textBox1.Invoke((MethodInvoker)delegate
                        {
                            textBox1.Text = "Write done";
                            button1.Enabled = true;
                        });
                    }
                    else
                    {
                        textBox1.Text = "Write done";
                        button1.Enabled = true;
                    }
                }

                if(dataGridView1.InvokeRequired == true)
                {
                    dataGridView1.Invoke((MethodInvoker)delegate
                    {
                        dataGridView1.Refresh();
                    });
                }
                else
                {
                    dataGridView1.Refresh();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // read from sys
        private void button1_Click(object sender, EventArgs e)
        {
            set_para_form_send_event(cmd: "SET_PARAM_FORM_READ_FROM_SYS");
            textBox1.Text = "Parameter reading...";
        }
        // write to sys
        private void button2_Click(object sender, EventArgs e)
        {
            string[] transfer = new string[(int)SetParaADC_enum.LENGTH + (int)SetParaTHRESHOLD_enum.LENGTH];
            for(int i = 0; i < (int)SetParaADC_enum.LENGTH; i++)
            {
                transfer[i] = (string)dataGridView1.Rows[i].Cells[1].Value;
            }
            for (int i = 0; i < (int)SetParaTHRESHOLD_enum.LENGTH; i++)
            {
                transfer[i + (int)SetParaADC_enum.LENGTH] = (string)dataGridView2.Rows[i].Cells[1].Value;
            }
            set_para_form_send_event(cmd: "SET_PARAM_FORM_WRITE_TO_SYS", data_arr: transfer);
            textBox1.Text = "Parameter writting";
            button1.Enabled = false;
        }
        // read from file
        private void button3_Click(object sender, EventArgs e)
        {
            set_para_form_send_event(cmd: "SET_PARAM_FORM_READ_FROM_FILE");
        }
        // write to file
        private void button4_Click(object sender, EventArgs e)
        {
            string[] transfer = new string[(int)SetParaADC_enum.LENGTH + (int)SetParaTHRESHOLD_enum.LENGTH];
            for (int i = 0; i < (int)SetParaADC_enum.LENGTH; i++)
            {
                transfer[i] = (string)dataGridView1.Rows[i].Cells[1].Value;
            }
            for (int i = 0; i < (int)SetParaTHRESHOLD_enum.LENGTH; i++)
            {
                transfer[i + (int)SetParaADC_enum.LENGTH] = (string)dataGridView2.Rows[i].Cells[1].Value;
            }
            set_para_form_send_event(cmd: "SET_PARAM_FORM_WRITE_TO_FILE", data_arr: transfer);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (this.InvokeRequired == true)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (dataGridView3.Visible == true)
                    {
                        this.Size = new Size(1251, 745);
                        dataGridView3.Visible = false;
                        toggle_pushed = false;
                    }
                    else
                    {
                        this.Size = new Size(1639, 745);
                        dataGridView3.Visible = true;
                        toggle_pushed = true;
                    }
                });
            }
            else
            {

                if (dataGridView3.Visible == true)
                {
                    this.Size = new Size(1251, 745);
                    dataGridView3.Visible = false;
                    toggle_pushed = false;
                }
                else
                {
                    this.Size = new Size(1639, 745);
                    dataGridView3.Visible = true;
                    toggle_pushed = true;
                }
            }
            if(toggle_pushed == true)
            {
                set_para_form_send_event(cmd: "DISPLAY_ALL_TOGGLE_PUSEHD_ON");
            }
            else
            {
                set_para_form_send_event(cmd: "DISPLAY_ALL_TOGGLE_PUSEHD_OFF");
            }
        }

        private void set_param_form_3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(toggle_pushed == true)
            {
                set_para_form_send_event(cmd: "DISPLAY_ALL_TOGGLE_PUSEHD_OFF");
            }
            if (setter_thread != null)
            {
                setter_thread.Abort();
            }
        }
    }
}
