using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using pc_monitor_1._0_Instance;
using pc_monitor_1._1_Form;
using pc_monitor_1._2_Data;
using static pc_monitor_1._2_Data.SjkimCmd;
using static pc_monitor_1._2_Data.SjkimData;


/*
 *  Project description
 *  1. 모든 instance는 SjkimInstance.cs에서 구현
 *  2. 모든 instance의 동작은 SjkimInstance.cs에서 구현
 *  3. mainform에서는 mainform button, datagrid등의 동작 함수만 구현.
 *  2. 각각의 form에 대한 데이터는 각각의 form에서 멤버변수등으로 처리
 *  2. enum등은 Data.cs에서 구현.
 */
namespace pc_monitor_1
{
    public delegate void PushEventHandler(string cmd="", string data=null, string[] data_arr=null); // 부모 -> 자식
    public delegate void GetEventHandler(string cmd="", string data = null, string[] data_arr=null); // 자식 -> 부모
    public delegate void SafeCallFloatDelegate(float f_data);

    public partial class main_form_0 : Form
    {
        static SjkimInstance sjkim_inst = new SjkimInstance();
        public PushEventHandler[] main_push_event = new PushEventHandler[(int)Form_enum.LENGTH];
        public GetEventHandler main_get_event;

        static Point table_point;

        public System.Threading.Timer my_timer;
        private object thislock = new Object();

        Thread ethernet_thread;

        public main_form_0()
        {
            InitializeComponent();
        }

        // main form의 datagrid 설정
        private void datagird_set(int datagrid)
        {
            switch(datagrid)
            {
                case 1: // datagrid 1
                    dataGridView1.Rows.Add("VOLT 0"     , 1, "CURRENT 0", 1);
                    dataGridView1.Rows.Add("VOLT 1"     , 1, "CURRENT 1", 1);
                    dataGridView1.Rows.Add("TEMP. SYS"  , 1, "CURRENT 2", 1);
                    dataGridView1.Rows.Add("TEMP. HEAT" , 1, "CURRENT 3", 1);
                    dataGridView1.Rows.Add("TEMP. HPA 1", 1, "CURRENT 4", 1);
                    dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 255, 255);
                    dataGridView1.DefaultCellStyle.SelectionForeColor = Color.FromArgb(0, 0, 0);
                    // dataGridView1.Enabled = false;
                    dataGridView1.BackgroundColor = Color.FromArgb(0, 0, 50);
                    break;
                case 2: // data grid 2
                    dataGridView2.Rows.Add("FREQUENCY", "", "OPERATE TIME", 1);
                    dataGridView2.Rows.Add("IP", "", "RADIATE TIME", 1);
                    dataGridView2.Rows.Add("PORT", "", "STANDBY TIME", 1);
                    dataGridView2.Rows.Add("","", "", 1);
                    dataGridView2.Rows.Add("", "", "", 1);
                    dataGridView2.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 255, 255);
                    dataGridView2.DefaultCellStyle.SelectionForeColor = Color.FromArgb(0, 0, 0);


                    dataGridView2.Rows[3].Cells[1].Style.BackColor = Color.FromArgb(255, 255, 255);
                    dataGridView2.Rows[3].Cells[3].Style.BackColor = Color.FromArgb(255, 255, 255);
                    dataGridView2.Rows[4].Cells[1].Style.BackColor = Color.FromArgb(255, 255, 255);
                    dataGridView2.Rows[4].Cells[3].Style.BackColor = Color.FromArgb(255, 255, 255);
                    // dataGridView2.Enabled = false;
                    dataGridView2.BackgroundColor = Color.FromArgb(0, 0, 50);
                    break;
                case 3: // data grid 3
                    dataGridView3.Rows.Add("OVER INPUT PWR", "CLEAR"       , "VOLTAGE"     , "CLEAR");
                    dataGridView3.Rows.Add("VSWR"          , "CLEAR"       , "OVER CURRENT", "CLEAR");
                    dataGridView3.Rows.Add("OVER FWD PWR"  , "CLEAR"       , "OVER TEMP."  , "CLEAR");
                    dataGridView3.Rows.Add(""              , ""            , "FAN"         , "CLEAR");
                    dataGridView3.Rows.Add(""              , ""            , "INTERLOCK"   , "FAULT");
                    dataGridView3.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 255, 255);
                    dataGridView3.DefaultCellStyle.SelectionForeColor = Color.FromArgb(0, 0, 0);

                    dataGridView3.Rows[3].Cells[0].Style.BackColor = Color.FromArgb(255, 255, 255);
                    dataGridView3.Rows[3].Cells[1].Style.BackColor = Color.FromArgb(255, 255, 255);
                    dataGridView3.Rows[4].Cells[0].Style.BackColor = Color.FromArgb(255, 255, 255);
                    dataGridView3.Rows[4].Cells[1].Style.BackColor = Color.FromArgb(255, 255, 255);

                    // dataGridView3.Enabled = false;
                    dataGridView3.BackgroundColor = Color.FromArgb(0, 0, 50);

                    break;
                default:
                    break;
            }
        }

        private void main_display()
        {
            // status power panel set
            button8.Text = sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.ONLINE_STANDBY];
            button3.Text = sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.CLEAR_FAULT];
            if (button12.Text == "《W》")
            {
                float fwd_buff = 0;
                if(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER] != null)
                {
                    fwd_buff = (float)Math.Pow(10.0F, (float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER]) / 10) - 3);
                    button7.Text = String.Format("{0:0.##}", fwd_buff);
                }
            }
            else
            {
                // button7.Text = String.Format("{0:0.00}", float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER]));
            }
            if (button5.Text == "《W》")
            {
                float rfl_buff = 0;
                if (sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.RFL_POWER] != null)
                {
                    rfl_buff = (float)Math.Pow(10.0F, (float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.RFL_POWER]) / 10) - 3);
                    button6.Text = String.Format("{0:0.##}", rfl_buff);
                }
            }
            else
            {
                // button6.Text = String.Format("{0:0.00}", float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.RFL_POWER]));
            }
            // button11.Text = sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.VSWR_STATUS_POWER];
            label9.Text = sjkim_inst.data_instance.monitoring_description;
            label13.Text = sjkim_inst.data_instance.monitoring_model;
            label1.Text = sjkim_inst.data_instance.monitoring_serial_num;

            // VOLT TEMP CURRENT TABLE SET
            for (int i = 0; i < 5; i++)
            {
                dataGridView1.Rows[i].Cells[1].Value = sjkim_inst.data_instance.table_vtc_data[i];
            }
            for (int i = 5; i < (int)TableData_VTC_enum.LENGTH; i++)
            {
                dataGridView1.Rows[i - 5].Cells[3].Value = sjkim_inst.data_instance.table_vtc_data[i];
            }
            // FREQ ELAPSED TIME TABLE SET
            for (int i = 0; i < 2; i++)
            {
                dataGridView2.Rows[i + 3].Cells[1].Value = sjkim_inst.data_instance.table_fet_data[i];
            }
            for (int i = 2; i < (int)TableData_FET_enum.LENGTH; i++)
            {
                dataGridView2.Rows[i - 2].Cells[3].Value = sjkim_inst.data_instance.table_fet_data[i];
            }
            // FAULT TABLE SET
            for (int i = 0; i < 3; i++)
            {
                dataGridView3.Rows[i].Cells[1].Value = sjkim_inst.data_instance.fault_data[i];
                if(sjkim_inst.data_instance.fault_data[i] == "FAULT")
                {
                    dataGridView3.Rows[i].Cells[1].Style.BackColor = Color.FromArgb(255, 50, 30);
                }
                else
                {
                    dataGridView3.Rows[i].Cells[1].Style.BackColor = Color.FromArgb(70, 255, 255);
                }
            }
            for (int i = 3; i < (int)FAULT_enum.LENGTH; i++)
            {
                dataGridView3.Rows[i - 3].Cells[3].Value = sjkim_inst.data_instance.fault_data[i];
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.Size = new Size(750, 470);
                table_point = dataGridView1.Location;
                sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.TABEL] = true;
                sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.VTC] = true;
                sjkim_inst.data_instance.now_button = "TABLE";

                panel8.BackColor = Color.FromArgb(0, 0, 50);
                panel5.BackColor = Color.FromArgb(0, 0, 50);
                datagird_set(1); // datagrid 1 set
                datagird_set(2); // datagrid 1 set
                datagird_set(3); // datagrid 1 set
                main_display();
                LocationSetBTC();

                sjkim_inst.data_instance.ip_address[0] = Properties.Settings.Default.sett_ip_0;
                sjkim_inst.data_instance.ip_address[1] = Properties.Settings.Default.sett_ip_1;
                sjkim_inst.data_instance.ip_address[2] = Properties.Settings.Default.sett_ip_2;
                sjkim_inst.data_instance.ip_address[3] = Properties.Settings.Default.sett_ip_3;
                sjkim_inst.data_instance.port = Properties.Settings.Default.sett_ip_4;

                switch (Properties.Settings.Default.last_connect_type)
                {
                    case "Serial":
                        {
                            comboBox1.DataSource = SerialPort.GetPortNames();
                            foreach (string portname in comboBox1.Items)
                            {
                                if (portname == Properties.Settings.Default.last_serial_portname)
                                {
                                    comboBox1.Text = portname;
                                    serialPort1.PortName = comboBox1.Text;
                                    serialPort1.Open();
                                    textBox5.Text = "Serial Opened";
                                    button78.BackColor = Color.FromArgb(255, 50, 30);
                                    sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.SERIAL;
                                    sjkim_inst.data_instance.normal_sent = false;
                                    wait_flag = false;
                                    wait = false;
                                    if (timer1.Enabled == false)
                                    {
                                        timer1.Start();
                                    }
                                    Properties.Settings.Default.last_connect_type = "Serial";
                                    Properties.Settings.Default.last_serial_portname = serialPort1.PortName;
                                    break;
                                }
                            }
                            break;
                        }
                    case "USB":
                        {
                            comboBox2.DataSource = SerialPort.GetPortNames();
                            foreach (string portname in comboBox2.Items)
                            {
                                if (portname == Properties.Settings.Default.last_serial_portname)
                                {
                                    comboBox2.Text = portname;
                                    serialPort2.PortName = comboBox2.Text;
                                    serialPort2.Open();
                                    textBox5.Text = "USB Opened";
                                    button79.BackColor = Color.FromArgb(255, 50, 30);
                                    sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.USB;
                                    sjkim_inst.data_instance.normal_sent = false;
                                    wait_flag = false;
                                    wait = false;
                                    if (timer1.Enabled == false)
                                    {
                                        timer1.Start();
                                    }
                                    Properties.Settings.Default.last_connect_type = "USB";
                                    Properties.Settings.Default.last_serial_portname = serialPort2.PortName;
                                    break;
                                }
                            }
                            break;
                        }
                    case "Bluetooth":
                        {
                            comboBox3.DataSource = SerialPort.GetPortNames();
                            foreach (string portname in comboBox3.Items)
                            {
                                if (portname == Properties.Settings.Default.last_serial_portname)
                                {
                                    comboBox3.Text = portname;
                                    serialPort3.PortName = comboBox3.Text;
                                    serialPort3.Open();
                                    textBox5.Text = "Bluetooth Opened";
                                    button80.BackColor = Color.FromArgb(255, 50, 30);
                                    sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.BLUETOOTH;
                                    sjkim_inst.data_instance.normal_sent = false;
                                    wait_flag = false;
                                    wait = false;
                                    if (timer1.Enabled == false)
                                    {
                                        timer1.Start();
                                    }
                                    Properties.Settings.Default.last_connect_type = "Bluetooth";
                                    Properties.Settings.Default.last_serial_portname = serialPort3.PortName;
                                    break;
                                }
                            }
                            break;
                        }
                    case "Ethernet":
                        {
                            if (sjkim_inst.data_instance.client == null)
                            {
                                sjkim_inst.data_instance.ethernet_connected = false;
                                if (sjkim_inst.data_instance.sr != null)
                                {
                                    sjkim_inst.data_instance.sr.Close();
                                }
                                if (sjkim_inst.data_instance.sw != null)
                                {
                                    sjkim_inst.data_instance.sw.Close();
                                }
                                if (sjkim_inst.data_instance.ns != null)
                                {
                                    sjkim_inst.data_instance.ns.Close();
                                }
                                if (ethernet_thread != null)
                                {
                                    ethernet_thread.Abort();
                                }

                                sjkim_inst.data_instance.ethernet_connected = true;
                                string ip_buff = String.Empty;
                                for (int i = 0; i < 3; i++)
                                {
                                    ip_buff += sjkim_inst.data_instance.ip_address[i] + ".";
                                }
                                ip_buff += sjkim_inst.data_instance.ip_address[3];
                                sjkim_inst.data_instance.client = new TcpClient();
                                var result = sjkim_inst.data_instance.client.BeginConnect(ip_buff, Int32.Parse(sjkim_inst.data_instance.port), null, null);

                                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                                if (!success)
                                {
                                    throw new Exception("Failed to connect.");
                                }
                                sjkim_inst.data_instance.ns = sjkim_inst.data_instance.client.GetStream();
                                sjkim_inst.data_instance.sr = new System.IO.StreamReader(sjkim_inst.data_instance.ns);
                                sjkim_inst.data_instance.sw = new System.IO.StreamWriter(sjkim_inst.data_instance.ns);
                                sjkim_inst.data_instance.bw = new System.IO.BinaryWriter(sjkim_inst.data_instance.ns);

                                ethernet_thread = new Thread(new ThreadStart(ethernet_received));
                                ethernet_thread.IsBackground = true;
                                ethernet_thread.Start();

                                textBox5.Text = "Ethernet Opened";
                                button81.BackColor = Color.FromArgb(255, 50, 30);
                                sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.ETHERNET;
                                sjkim_inst.data_instance.normal_sent = false;
                                sjkim_inst.data_instance.ethernet_opened_first = true;
                                wait_flag = false;
                                wait = false;
                                if (timer1.Enabled == false)
                                {
                                    timer1.Start();
                                }
                                Properties.Settings.Default.last_connect_type = "Ethernet";
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            catch
            {

            }
            // time_start(callback: timer_callback, starttime: 2000, sendtime:500);

        }

        void time_start(TimerCallback callback, int starttime, int sendtime)
        {
            my_timer = new System.Threading.Timer(callback, null, starttime, sendtime);
        }

        void time_stop()
        {
            // my_timer.Dispose();
        }
        
        List<byte> test = new List<byte>();

        void TransmitProcedure_ethernet()
        {
            try
            {
                byte[] trans_buff = sjkim_inst.data_instance.transfer_list.ToArray();
                sjkim_inst.data_instance.transfer_list.Clear();
                string transmit_string = Encoding.Default.GetString(trans_buff);
                sjkim_inst.data_instance.sw.Write(transmit_string);
                sjkim_inst.data_instance.sw.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void TransmitProcedure()
        {
            try
            {
                if(sjkim_inst.data_instance.comm_state == Communication_enum.ETHERNET)
                {
                    sjkim_inst.data_instance.one_time_data_length = 256;
                }
                else
                {
                    sjkim_inst.data_instance.one_time_data_length = 256;
                }
                int one_time_length = sjkim_inst.data_instance.one_time_data_length;
                
                byte[] trans_buff = new byte[one_time_length];
                char[] trans_buff_char = new char[one_time_length];
                int tr_count = 0;
                if (sjkim_inst.data_instance.transfer_list.Count <= one_time_length)
                {
                    int index = 0;
                    do
                    {
                        trans_buff[index] = sjkim_inst.data_instance.transfer_list.First();
                        // trans_buff_char[index] = (char)(sjkim_inst.data_instance.transfer_list.First());
                        sjkim_inst.data_instance.transfer_list.RemoveAt(index: 0);
                        index += 1;
                        tr_count += 1;
                    } while (sjkim_inst.data_instance.transfer_list.Count != 0);
                }
                else
                {
                    for (int i = 0; i < one_time_length; i++)
                    {
                        trans_buff[i] = sjkim_inst.data_instance.transfer_list.First();
                        trans_buff_char[i] = (char)(sjkim_inst.data_instance.transfer_list.First());
                        sjkim_inst.data_instance.transfer_list.RemoveAt(index: 0);
                    }
                    tr_count = one_time_length;
                }
                switch (sjkim_inst.data_instance.comm_state)
                {
                    case SjkimCmd.Communication_enum.SERIAL:
                        serialPort1.Write(buffer: trans_buff, offset: 0, count: tr_count);
                        break;
                    case SjkimCmd.Communication_enum.USB:
                        serialPort2.Write(buffer: trans_buff, offset: 0, count: tr_count);
                        break;
                    case SjkimCmd.Communication_enum.BLUETOOTH:
                        serialPort3.Write(buffer: trans_buff, offset: 0, count: tr_count);
                        break;
                    case Communication_enum.ETHERNET:
                        // string transmit_string = Encoding.Default.GetString(bytes: trans_buff, index: 0, count: tr_count);
                        // sjkim_inst.data_instance.sw.Write(transmit_string);
                        // sjkim_inst.data_instance.sw.Flush();
                        sjkim_inst.data_instance.ns.Write(buffer: trans_buff, offset: 0, size: tr_count);
                        sjkim_inst.data_instance.ns.Flush();
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                if(timer1.Enabled == true)
                {
                    timer1.Stop();
                }
                MessageBox.Show(ex.ToString());

            }
        }

        const int normal_block = 10;
        int normal_blocker = 0;
        bool wait_flag = false;
        bool wait = false;
        bool normal_wait = false;

        void timer_callback()
        {
            try
            {
                if(wait == true)
                {
                    return;
                }

                if(wait_flag == true)
                {
                    wait = true;
                }
                if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                {
                    if (sjkim_inst.data_instance.comm_busy == false)
                    {
                        sjkim_inst.data_instance.comm_busy = true;

                        if (sjkim_inst.data_instance.transfer_list.Count != 0)
                        {
                            if(normal_blocker == 0)
                            {
                                normal_blocker = normal_block; // normal block
                            }
                            TransmitProcedure();
                        }
                        else
                        {
                            if (sjkim_inst.data_instance.serial_1_transmit_buffer.Count != 0)
                            {
                                if (normal_blocker == 0)
                                {
                                    normal_blocker = normal_block; // normal block
                                }
                                sjkim_inst.data_instance.transfer_list.AddRange(sjkim_inst.data_instance.serial_1_transmit_buffer);
                                sjkim_inst.data_instance.serial_1_transmit_buffer.Clear();
                                TransmitProcedure();
                            }
                            else
                            {
                                if (sjkim_inst.data_instance.serial_2_transmit_buffer.Count != 0)
                                {
                                    if (normal_blocker == 0)
                                    {
                                        normal_blocker = normal_block; // normal block
                                    }
                                    sjkim_inst.data_instance.transfer_list.AddRange(sjkim_inst.data_instance.serial_2_transmit_buffer);
                                    sjkim_inst.data_instance.serial_2_transmit_buffer.Clear();
                                    TransmitProcedure();
                                }
                                else
                                {
                                    if (sjkim_inst.data_instance.serial_3_transmit_buffer.Count != 0)
                                    {
                                        if (normal_blocker == 0)
                                        {
                                            normal_blocker = normal_block; // normal block
                                        }
                                        sjkim_inst.data_instance.transfer_list.AddRange(sjkim_inst.data_instance.serial_3_transmit_buffer);
                                        sjkim_inst.data_instance.serial_3_transmit_buffer.Clear();
                                        TransmitProcedure();
                                    }
                                    else
                                    {
                                        if (sjkim_inst.data_instance.ethernet_transmit_buffer.Count != 0)
                                        {
                                            if (normal_blocker == 0)
                                            {
                                                normal_blocker = normal_block; // normal block
                                            }
                                            sjkim_inst.data_instance.transfer_list.AddRange(sjkim_inst.data_instance.ethernet_transmit_buffer);
                                            sjkim_inst.data_instance.ethernet_transmit_buffer.Clear();
                                            TransmitProcedure();
                                            // TransmitProcedure_ethernet();
                                        }
                                        else
                                        {
                                            if (sjkim_inst.data_instance.normal_sent == false)
                                            {
                                                if (normal_blocker == 0)
                                                {
                                                    if(normal_wait == false)
                                                    {
                                                        normal_data_push();
                                                        sjkim_inst.data_instance.normal_sent = true;
                                                    }
                                                }
                                                else
                                                {
                                                    normal_blocker -= 1;
                                                }
                                            }
                                            else  // normal sent time out
                                            {
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        sjkim_inst.data_instance.comm_busy = false;
                    }
                }
            }
            catch (Exception ex)
            {
                timer1.Stop();
                MessageBox.Show(ex.ToString());
            }
        }

        void normal_data_push()
        {
            List<byte> normal_cmd = new List<byte>();
            normal_cmd.Add((byte)SjkimCmd.CmdList_enum.NORMAL);
            byte cmd_enum = (byte)SjkimCmd.CmdList_enum.NORMAL;
            if (cmd_enum == '*')
            {
                normal_cmd.Add((byte)'2');
            }
            else if (cmd_enum == '\r')
            {
                normal_cmd.Add((byte)'3');
            }
            else if (cmd_enum == '\n')
            {
                normal_cmd.Add((byte)'4');
            }
            else if (cmd_enum == 0)
            {
                normal_cmd.Add((byte)'5');
            }
            byte checksum = normal_cmd[0];
            for (int i = 1; i < normal_cmd.Count; i++)
            {
                checksum ^= normal_cmd[i];
            }
            normal_cmd.Add(checksum);
            if (checksum == '*')
            {
                normal_cmd.Add((byte)'2');
            }
            else if (checksum == '\r')
            {
                normal_cmd.Add((byte)'3');
            }
            else if (checksum == '\n')
            {
                normal_cmd.Add((byte)'4');
            }
            else if (checksum == 0)
            {
                normal_cmd.Add((byte)'5');
            }
            normal_cmd.Insert(0, (byte)'*');
            normal_cmd.Insert(1, (byte)'0');
            normal_cmd.Add((byte)'*');
            normal_cmd.Add((byte)'1');
            if (sjkim_inst.data_instance.comm_state != Communication_enum.NONE)
            {
                sjkim_inst.data_instance.transfer_list.AddRange(normal_cmd);
                TransmitProcedure();
            }
        }

        void timer_operating()
        {
            if (textBox6.InvokeRequired == true)
            {
                textBox6.Invoke((MethodInvoker)delegate
                {
                    textBox6.Text = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    sjkim_inst.data_instance.now_time = textBox6.Text;
                });
            }
            else  // 아래는 진입하지 않는 코드..
            {
                textBox6.Text = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                sjkim_inst.data_instance.now_time = textBox6.Text;
            }
        }

        /*void timer_callback(object data)
        {
            lock (thislock)
            {
                if (textBox6.InvokeRequired == true)
                {
                    textBox6.Invoke((MethodInvoker)delegate
                    {
                        textBox6.Text = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                        sjkim_inst.data_instance.now_time = textBox6.Text;

                        if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                        {
                            switch (sjkim_inst.data_instance.comm_state)
                            {
                                case SjkimCmd.Communication_enum.SERIAL:
                                    if (sjkim_inst.data_instance.transfer_list.Count != 0)
                                    {
                                        TransmitProcedure();
                                    }
                                    else
                                    {
                                        if (sjkim_inst.data_instance.serial_1_transmit_buffer.Count != 0)
                                        {
                                            sjkim_inst.data_instance.transfer_list.AddRange(sjkim_inst.data_instance.serial_1_transmit_buffer);
                                            sjkim_inst.data_instance.serial_1_transmit_buffer.Clear();
                                            TransmitProcedure();
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    });
                }
                else  // 아래는 진입하지 않는 코드..
                {
                    textBox6.Text = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    sjkim_inst.data_instance.now_time = textBox6.Text;
                }
            }
        }*/
        // main form component들의 location, visible등을 클릭여부에 따라 조정
        private void LocationSetBTC()
        {
            /*TABEL = 0, 
            FAULT, 
            COMM, 
            VTC, // VOLT TEMP CUURENT
            FET, // FREQ ELAPSED TIME
            ALC_OFF,
            ALC_ON,
            READ_ATTEN,
            SET_ATTEN,
            ATTEN_1,
            ATTEN_01,
            ATTEN__1,
            ATTEN__01,
            SERIAL,
            USB,
            BLUETOOTH,
            ETHERNET,
            SET_TIME,
            ONLINE,
            FWD_DBM,
            REF_DBM,
            LENGTH*/

            for (int i = (int)SjkimData.ControlButton_enum.TABEL; i < (int)SjkimData.ControlButton_enum.LENGTH; i++)
            {
                if(sjkim_inst.g_control_button[i] == true)
                {
                    switch(i)
                    {
                        case (int)SjkimData.ControlButton_enum.TABEL:
                            if(sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.VTC] == true)
                            {
                                dataGridView1.Visible = true;
                            }
                            else
                            {
                                if(sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.FET] == true)
                                {
                                    dataGridView2.Location = dataGridView1.Location;
                                    dataGridView2.Visible = true;
                                }
                                else
                                {
                                    dataGridView1.Visible = true;
                                }
                            }
                            return;
                        case (int)SjkimData.ControlButton_enum.FAULT:
                            dataGridView3.Location = dataGridView1.Location;
                            dataGridView3.Visible = true;
                            return;
                        case (int)SjkimData.ControlButton_enum.COMM:
                            panel5.Location = dataGridView1.Location;
                            panel5.Visible = true;
                            return;
                        case (int)SjkimData.ControlButton_enum.VTC:
                            break;
                        case (int)SjkimData.ControlButton_enum.FET:
                            break;
                        case (int)SjkimData.ControlButton_enum.ALC_OFF:
                            panel8.Location = dataGridView1.Location;
                            panel8.Visible = true;
                            break;
                        case (int)SjkimData.ControlButton_enum.ALC_ON:
                            panel3.Location = dataGridView1.Location;
                            panel3.Visible = true;
                            break;
                        case (int)SjkimData.ControlButton_enum.READ_ATTEN:
                            break;
                        case (int)SjkimData.ControlButton_enum.SET_ATTEN:
                            break;
                        case (int)SjkimData.ControlButton_enum.ATTEN_1:
                            break;
                        case (int)SjkimData.ControlButton_enum.ATTEN_01:
                            break;
                        case (int)SjkimData.ControlButton_enum.ATTEN__1:
                            break;
                        case (int)SjkimData.ControlButton_enum.ATTEN__01:
                            break;
                        case (int)SjkimData.ControlButton_enum.SERIAL:
                            break;
                        case (int)SjkimData.ControlButton_enum.USB:
                            break;
                        case (int)SjkimData.ControlButton_enum.BLUETOOTH:
                            break;
                        case (int)SjkimData.ControlButton_enum.ETHERNET:
                            break;
                        case (int)SjkimData.ControlButton_enum.SET_TIME:
                            break;
                        case (int)SjkimData.ControlButton_enum.ONLINE:
                            if(button4.Text == "ONLINE")
                            {
                                button4.Text = "STANDBY";
                                // todo: send standby cmd
                            }
                            else
                            {
                                button4.Text = "ONLINE";
                                // todo: send online cmd
                            }
                            break;
                        case (int)SjkimData.ControlButton_enum.FWD_DBM:
                            break;
                        case (int)SjkimData.ControlButton_enum.REF_DBM:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        // main form component들의 global visible 설정
        private void visble_set(bool view)
        {
            if(view == false)
            {
                dataGridView1.Visible = false;
                dataGridView2.Visible = false;
                dataGridView3.Visible = false;
                panel3.Visible = false;
                panel5.Visible = false;
                panel8.Visible = false;
                button1.Visible = false;
                button15.Visible = false;
            }
            else
            {
                dataGridView1.Visible = true;
                dataGridView2.Visible = true;
                dataGridView3.Visible = true;
                panel3.Visible = true;
                panel5.Visible = true;
                panel8.Visible = true;
                button1.Visible = true;
                button15.Visible = true;
            }
        }

        // button true false global 설정
        private void all_SjkimInstance_set(bool SjkimInstance)
        {
            if(SjkimInstance == false)
            {
                for (int i = (int)SjkimData.ControlButton_enum.TABEL; i < (int)SjkimData.ControlButton_enum.LENGTH; i++)
                {
                    sjkim_inst.g_control_button[i] = false;
                }
            }
        }

        // Table 화면 내부 버튼 동작 색상 설정
        private void volt_temp_freq_elapse_button_color(bool vtc = false, bool fet = false)
        {
            if(vtc == true)
            {
                button15.BackColor = Color.FromArgb(255, 255, 255);
                button15.ForeColor = Color.FromArgb(0, 0, 0);
                button1.BackColor = Color.FromArgb(255, 50, 30);
                button1.ForeColor = Color.FromArgb(255, 255, 255);
                return;
            }
            if(fet == true)
            {
                button1.BackColor = Color.FromArgb(255, 255, 255);
                button1.ForeColor = Color.FromArgb(0, 0, 0);
                button15.BackColor = Color.FromArgb(255, 50, 30);
                button15.ForeColor = Color.FromArgb(255, 255, 255);
            }
        }

        // Control 버튼( 1)table, 2) fault, 3) alc, 4) comm 버튼 동작 색상 설정)
        private void table_fault_alc_comm_button_color(bool table = false, bool fault = false, bool alc = false, bool comm = false)
        {
            if(table == true)
            { 
                button10.BackColor = Color.FromArgb(255, 50, 30);  // table
                button10.ForeColor = Color.FromArgb(255, 255, 255);

                button13.BackColor = Color.FromArgb(255, 255, 255); // fault
                button13.ForeColor = Color.FromArgb(0, 0, 0);

                button9.BackColor = Color.FromArgb(255, 255, 255);
                button9.ForeColor = Color.FromArgb(0, 0, 0); // alc

                button2.BackColor = Color.FromArgb(255, 255, 255);  // comm
                button2.ForeColor = Color.FromArgb(0, 0, 0);

                button1.BackColor = Color.FromArgb(255, 50, 30);
                button1.ForeColor = Color.FromArgb(255, 255, 255);

                button15.BackColor = Color.FromArgb(255, 255, 255);
                button15.ForeColor = Color.FromArgb(0, 0, 0);

                return;
            }
            if (fault == true)
            {
                button10.BackColor = Color.FromArgb(255, 255, 255);  // table
                button10.ForeColor = Color.FromArgb(0, 0, 0);

                button13.BackColor = Color.FromArgb(255, 50, 30); // fault
                button13.ForeColor = Color.FromArgb(255, 255, 255);

                button9.BackColor = Color.FromArgb(255, 255, 255);
                button9.ForeColor = Color.FromArgb(0, 0, 0); // alc

                button2.BackColor = Color.FromArgb(255, 255, 255);  // comm
                button2.ForeColor = Color.FromArgb(0, 0, 0);

                return;
            }
            if (alc == true)
            {
                button10.BackColor = Color.FromArgb(255, 255, 255);  // table
                button10.ForeColor = Color.FromArgb(0, 0, 0);

                button13.BackColor = Color.FromArgb(255, 255, 255); // fault
                button13.ForeColor = Color.FromArgb(0, 0, 0);

                button9.BackColor = Color.FromArgb(255, 50, 30);
                button9.ForeColor = Color.FromArgb(255, 255, 255); // alc

                button2.BackColor = Color.FromArgb(255, 255, 255);  // comm
                button2.ForeColor = Color.FromArgb(0, 0, 0);

                return;
            }
            if (comm == true)
            {
                button10.BackColor = Color.FromArgb(255, 255, 255);  // table
                button10.ForeColor = Color.FromArgb(0, 0, 0);

                button13.BackColor = Color.FromArgb(255, 255, 255); // fault
                button13.ForeColor = Color.FromArgb(0, 0, 0);

                button9.BackColor = Color.FromArgb(255, 255, 255);
                button9.ForeColor = Color.FromArgb(0, 0, 0); // alc

                button2.BackColor = Color.FromArgb(255, 50, 30);  // comm
                button2.ForeColor = Color.FromArgb(255, 255, 255);

                return;
            }
        }

        // table button 동작 설정
        private void button10_Click(object sender, EventArgs e)
        {
            sjkim_inst.data_instance.now_button = "TABLE";
            if (sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.TABEL] == true)
            {
                return;
            }
            visble_set(view: false);
            button15.Visible = true;
            button1.Visible = true;
            table_fault_alc_comm_button_color(table: true);
            all_SjkimInstance_set(SjkimInstance: false);
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.TABEL] = true;
            LocationSetBTC();
        }

        // fault button 동작 설정
        private void button13_Click(object sender, EventArgs e)  // fault
        {
            sjkim_inst.data_instance.now_button = "FAULT";
            if (sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.FAULT] == true)
            {
                return;
            }
            visble_set(view: false);
            all_SjkimInstance_set(SjkimInstance: false);
            table_fault_alc_comm_button_color(fault: true);
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.FAULT] = true;
            LocationSetBTC();
        }

        // alc button 동작 설정
        private void button9_Click(object sender, EventArgs e) // alc
        {
            sjkim_inst.data_instance.now_button = "ALC";
            if ((sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.ALC_OFF] == true) || (sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.ALC_ON] == true))
            {
                return;
            }
            visble_set(view: false);

            table_fault_alc_comm_button_color(alc: true);
            all_SjkimInstance_set(SjkimInstance: false);
            if(sjkim_inst.data_instance.alc_agc_state == false)  // if agc is true:
            {
                sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.ALC_OFF] = true;
            }
            else
            {
                sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.ALC_ON] = true;
            }
            LocationSetBTC();
        }

        // comm button 동작 설정
        private void button2_Click(object sender, EventArgs e) // comm
        {
            sjkim_inst.data_instance.now_button = "COMM";
            if (sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.COMM] == true)
            {
                return;
            }
            visble_set(view: false);
            table_fault_alc_comm_button_color(comm: true);
            all_SjkimInstance_set(SjkimInstance: false);
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.COMM] = true;
            LocationSetBTC();
        }

        // table 내부 버튼인 vtc 버튼 동작 설정
        private void button1_Click(object sender, EventArgs e)  // voltage temp current
        {
            if(sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.VTC] == true)
            {
                return;
            }
            volt_temp_freq_elapse_button_color(vtc: true);

            visble_set(view: false);
            button15.Visible = true;
            button1.Visible = true;
            all_SjkimInstance_set(SjkimInstance: false);
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.TABEL] = true;
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.VTC] = true;
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.FET] = false;
            LocationSetBTC();
        }

        // table 내부 버튼인 fet 버튼 동작 설정
        private void button15_Click(object sender, EventArgs e)  // freq elapsed time
        {
            if (sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.FET] == true)
            {
                return;
            }
            volt_temp_freq_elapse_button_color(fet: true);
            visble_set(view: false);
            button15.Visible = true;
            button1.Visible = true;
            all_SjkimInstance_set(SjkimInstance: false);
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.TABEL] = true;
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.VTC] = false;
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.FET] = true;
            LocationSetBTC();
        }

        // main의 버튼 online 동작 설정
        private void button4_Click(object sender, EventArgs e) // online
        { 
            all_SjkimInstance_set(SjkimInstance: false);
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.ONLINE] = true;
            LocationSetBTC();
            if (button4.Text == "ONLINE")
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.STANDBY, comm_state: sjkim_inst.data_instance.comm_state);
            }
            else
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ONLINE, comm_state: sjkim_inst.data_instance.comm_state);
                sjkim_inst.data_instance.fault_occured_before = false;
            }
        }

        // alc on button 설정
        private void button73_Click(object sender, EventArgs e) // alc on
        {
            now_alc_set();
            visble_set(view: false);
            all_SjkimInstance_set(SjkimInstance: false);
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.ALC_ON] = true;
            LocationSetBTC();
            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_ON, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        // alc off button 설정
        private void button21_Click(object sender, EventArgs e) // alc off
        {
            now_agc_set();
            visble_set(view: false);
            all_SjkimInstance_set(SjkimInstance: false);
            sjkim_inst.g_control_button[(int)SjkimData.ControlButton_enum.ALC_OFF] = true;
            LocationSetBTC();
            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                button73.Enabled = false;
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_OFF, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        // set param button 설정
        private void button87_Click(object sender, EventArgs e)
        {
            NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.LOGIN_FORM], 
                    enum_var: (int)Form_enum.LOGIN_FORM, 
                    location: this.Location);
        }

        public void NewForm(ref Form sender, Point location, int enum_var)
        {
            if (sender != null)
            {
                if (enum_var == (int)Form_enum.LOGIN_FORM)
                {
                    if(((login_form_10)sender).opend == false)
                    {
                        sender.Close();
                    }
                }
                else
                {
                    sender.Close();
                }
            }
         
            Point parentPoint = location;
            switch (enum_var)
            {
                case (int)Form_enum.IP_SETUP_FORM:
                    {
                        sender = new ip_setup_form_1();
                        ((ip_setup_form_1)sender).ip_form_send_event += new GetEventHandler(this.IPAddressSet);
                        this.main_push_event[(int)Form_enum.IP_SETUP_FORM] = new PushEventHandler(((ip_setup_form_1)sender).set_textbox_from_main);

                        sjkim_inst.data_instance.ip_address[0] = Properties.Settings.Default.sett_ip_0;
                        sjkim_inst.data_instance.ip_address[1] = Properties.Settings.Default.sett_ip_1;
                        sjkim_inst.data_instance.ip_address[2] = Properties.Settings.Default.sett_ip_2;
                        sjkim_inst.data_instance.ip_address[3] = Properties.Settings.Default.sett_ip_3;
                        sjkim_inst.data_instance.port = Properties.Settings.Default.sett_ip_4;

                        string[] transmfer = { sjkim_inst.data_instance.ip_address[0],
                                                sjkim_inst.data_instance.ip_address[1],
                                                sjkim_inst.data_instance.ip_address[2],
                                                sjkim_inst.data_instance.ip_address[3],
                                                sjkim_inst.data_instance.port };
                        main_push_event[(int)Form_enum.IP_SETUP_FORM](cmd: "IP_SETUP_FORM_TEXTBOX_SET_DATA_ARR", data_arr: transmfer);
                        break;
                    }
                case (int)Form_enum.PARAMETER_SELECT_FORM:
                    {
                        sender = new parameter_select_form_2();
                        ((parameter_select_form_2)sender).parameter_send_event += new GetEventHandler(this.ParaSelectCheck);
                        break;
                    }
                case (int)Form_enum.SET_PARAM_FORM:
                    {
                        sender = new set_param_form_3();
                        ((set_param_form_3)sender).set_para_form_send_event += new GetEventHandler(this.SetParamSet);
                        this.main_push_event[(int)Form_enum.SET_PARAM_FORM] = new PushEventHandler(((set_param_form_3)sender).set_datagrid_from_main);
                        string[] transfer = new string[(int)SetParaADC_enum.LENGTH + (int)SetParaTHRESHOLD_enum.LENGTH];
                        for(int i = 0; i < (int)SetParaADC_enum.LENGTH; i++)
                        {
                            transfer[i] = sjkim_inst.data_instance.para_adc_data[i];
                        }
                        for (int i = 0; i < (int)SetParaTHRESHOLD_enum.LENGTH; i++)
                        {
                            transfer[i + (int)SetParaADC_enum.LENGTH] = sjkim_inst.data_instance.threshold_data[i];
                        }
                        main_push_event[(int)Form_enum.SET_PARAM_FORM](cmd: "SET_PARAM_FORM_DATAGRID_SET_DATA_ARR", data_arr: transfer);
                        break;
                    }
                case (int)Form_enum.FREQ_PWR_MEA_FORM:
                    {
                        sjkim_inst.data_instance.fwd_freq.Clear();
                        sjkim_inst.data_instance.fwd_atten.Clear();
                        sjkim_inst.data_instance.fwd_adc.Clear();
                        sjkim_inst.data_instance.fwd_dbm.Clear();
                        sender = new freq_pwr_mea_form_4();
                        ((freq_pwr_mea_form_4)sender).freq_pwr_mea_form_send_event += new GetEventHandler(this.FreqMeaFormSet);
                        this.main_push_event[(int)Form_enum.FREQ_PWR_MEA_FORM] = new PushEventHandler(((freq_pwr_mea_form_4)sender).set_datagrid_from_main);
                        break;
                    }
                case (int)Form_enum.FREQ_PWR_PROGRESS_FORM:
                    {
                        sjkim_inst.data_instance.fwd_freq.Clear();
                        sjkim_inst.data_instance.fwd_atten.Clear();
                        sjkim_inst.data_instance.fwd_adc.Clear();
                        sjkim_inst.data_instance.fwd_dbm.Clear();
                        sender = new freq_pwr_progress_form_5();
                        ((freq_pwr_progress_form_5)sender).freq_pwr_progress_form_send_event += new GetEventHandler(this.FreqProgressFormSet);
                        this.main_push_event[(int)Form_enum.FREQ_PWR_PROGRESS_FORM] = new PushEventHandler(((freq_pwr_progress_form_5)sender).set_datagrid_from_main);
                        break;
                    }
                case (int)Form_enum.INPUT_PWR_MEA_FORM:
                    {
                        sjkim_inst.data_instance.input_freq.Clear();
                        sjkim_inst.data_instance.input_adc.Clear();
                        sjkim_inst.data_instance.input_dbm.Clear();
                        sender = new input_pwr_mea_form_6();
                        ((input_pwr_mea_form_6)sender).input_pwr_mea_form_send_event += new GetEventHandler(this.InputMeasFormSet);
                        this.main_push_event[(int)Form_enum.INPUT_PWR_MEA_FORM] = new PushEventHandler(((input_pwr_mea_form_6)sender).set_textbox_from_main);
                        break;
                    }
                case (int)Form_enum.INPUT_PWR_PROGRESS_FORM:
                    {
                        sjkim_inst.data_instance.input_freq.Clear();
                        sjkim_inst.data_instance.input_adc.Clear();
                        sjkim_inst.data_instance.input_dbm.Clear();
                        sender = new input_pwr_progress_form_7();
                        ((input_pwr_progress_form_7)sender).input_pwr_progress_form_send_event += new GetEventHandler(this.InputProgressFormSet);
                        this.main_push_event[(int)Form_enum.INPUT_PWR_PROGRESS_FORM] = new PushEventHandler(((input_pwr_progress_form_7)sender).set_datagrid_from_main);
                        break;
                    }
                case (int)Form_enum.SET_FREQ_FORM:
                    {
                        sender = new set_freq_form_8();
                        ((set_freq_form_8)sender).freq_send_event += new GetEventHandler(this.FreqSetSet);
                        this.main_push_event[(int)Form_enum.SET_FREQ_FORM] = new PushEventHandler(((set_freq_form_8)sender).set_textbox_from_main);
                        main_push_event[(int)Form_enum.SET_FREQ_FORM](cmd: "SET_FREQ_FORM_TEXTBOX_SET_DATA", data: sjkim_inst.data_instance.freq_data);
                        break;
                    }
                case (int)Form_enum.ERROR_LOG_FORM:
                    {
                        sender = new error_log_form_9();
                        ((error_log_form_9)sender).error_form_send_event += new GetEventHandler(this.ErrorFormSet);
                        this.main_push_event[(int)Form_enum.ERROR_LOG_FORM] = new PushEventHandler(((error_log_form_9)sender).set_datagrid_from_main);
                        break;
                    }
                case (int)Form_enum.LOGIN_FORM:
                    {
                        if (sender != null)
                        {
                            if (((login_form_10)sender).opend == true)
                            {
                                NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.PARAMETER_SELECT_FORM],
                                        enum_var: (int)Form_enum.PARAMETER_SELECT_FORM,
                                        location: this.Location);
                                return;
                            }
                        }
                        sender = new login_form_10();
                        ((login_form_10)sender).login_send_event += new GetEventHandler(this.login_check);
                        break;
                    }
                case (int)Form_enum.MONITORING_FORM:
                    {
                        sender = new monitoring_form_11();
                        this.main_push_event[(int)Form_enum.MONITORING_FORM] = new PushEventHandler(((monitoring_form_11)sender).set_textbox_from_main);
                        ((monitoring_form_11)sender).monitoring_event += new GetEventHandler(this.monitoring_check);
                        if(sjkim_inst.data_instance.cmd_mode_on == false)
                        {
                            main_push_event[(int)Form_enum.MONITORING_FORM](cmd: "LOCAL_ON");
                        }
                        else
                        {
                            main_push_event[(int)Form_enum.MONITORING_FORM](cmd: "CMD_ON");
                        }
                        break;
                    }
                case (int)Form_enum.ALC_AGC_FORM:
                    {
                        sender = new alc_agc_set_form_12();
                        this.main_push_event[(int)Form_enum.ALC_AGC_FORM] = new PushEventHandler(((alc_agc_set_form_12)sender).set_textbox_from_main);
                        ((alc_agc_set_form_12)sender).alc_agc_event += new GetEventHandler(this.alc_agc_check);
                        
                        string[] transfer = { String.Format("{0:0.#}", sjkim_inst.data_instance.agc_dB_value),
                                              String.Format("{0:0.#}", sjkim_inst.data_instance.alc_dBm_value) };
                        main_push_event[(int)Form_enum.ALC_AGC_FORM](cmd: "ALC_AGC_FORM_TEXTBOX_SET_DATA_ARR", data_arr: transfer);
                        break;
                    }
                default:
                    {
                        MessageBox.Show("Enum에 포함되지 않은 Form입니다.");
                        return;
                    }
            }
            sender.StartPosition = FormStartPosition.Manual;
            sender.Location = new Point(parentPoint.X + 100, parentPoint.Y + 100);
            sender.Show();
        }


        private void monitoring_check(string cmd, string data, string[] data_arr)
        {
            if (cmd == "LOCAL_PUSHED")  
            {
                sjkim_inst.data_instance.cmd_mode_on = false;
                normal_wait = false;
            }
            if(cmd == "COMMAND_PUSHED")
            {
                sjkim_inst.data_instance.cmd_mode_on = true;
                normal_wait = true;
                string_transmit("\n");

            }
            if(cmd == "OPENED")
            {
                sjkim_inst.data_instance.monitoring_opend = true;
            }
            if(cmd == "CLOSED")
            {
                sjkim_inst.data_instance.monitoring_opend = false;
            }
            if(cmd == "SEND_PUSHED")
            {
                string_transmit(data);
            }
        }

        private void string_transmit(string data)
        {

            if (normal_wait == true)
            {
                if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                {
                    List<byte> transmit_buffer_byte_list = new List<byte>();
                    transmit_buffer_byte_list.AddRange(Encoding.ASCII.GetBytes(data));

                    switch (sjkim_inst.data_instance.comm_state)
                    {
                        case Communication_enum.NONE:
                            MessageBox.Show("No connect");
                            transmit_buffer_byte_list.Clear();
                            break;
                        case Communication_enum.SERIAL:
                            sjkim_inst.data_instance.serial_1_transmit_buffer.Clear();
                            sjkim_inst.data_instance.serial_1_transmit_buffer.AddRange(transmit_buffer_byte_list);
                            break;
                        case Communication_enum.USB:
                            sjkim_inst.data_instance.serial_2_transmit_buffer.Clear();
                            sjkim_inst.data_instance.serial_2_transmit_buffer.AddRange(transmit_buffer_byte_list);
                            break;
                        case Communication_enum.BLUETOOTH:
                            sjkim_inst.data_instance.serial_3_transmit_buffer.Clear();
                            sjkim_inst.data_instance.serial_3_transmit_buffer.AddRange(transmit_buffer_byte_list);
                            break;
                        case Communication_enum.ETHERNET:
                            sjkim_inst.data_instance.ethernet_transmit_buffer.Clear();
                            sjkim_inst.data_instance.ethernet_transmit_buffer.AddRange(transmit_buffer_byte_list);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void alc_agc_check(string cmd, string data, string[] data_arr)
        {
            try
            {
                if (cmd == "agc")  // agc set button clicked
                {
                    sjkim_inst.data_instance.agc_dB_value = (float)Math.Round(float.Parse(data), 1);
                    now_agc_set();

                    if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                    {
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_SET, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                }
                if (cmd == "alc")  // alc set button clicked
                {
                    sjkim_inst.data_instance.alc_dBm_value = (float)Math.Round(float.Parse(data), 1);
                    now_alc_set();

                    if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                    {
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_SET, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void InputProgressFormSet(string cmd, string data, string[] data_arr)
        {
            /*
            input_pwr_progress_form_send_event(cmd: "FREQ_PWR_PUSH");
            */
            try
            {
                sjkim_inst.data_instance.file_transfer.Clear();


                if (cmd == "FREQ_INPUT_PUSH")
                {
                    // data_arr[0] = step_value;
                    // data_arr[1] = freq data arr count;
                    // data_arr[2] = first input data item length;
                    // data_arr[3] = first frequency value;
                    // data_arr[4] = first freq input data
                    // ...
                    normal_wait = true;
                    while (sjkim_inst.data_instance.normal_sent == true)
                    {
                    }
                    if (sjkim_inst.data_instance.comm_state != Communication_enum.NONE)
                    {
                        for (int i = 0; i < data_arr.Length; i++)
                        {
                            sjkim_inst.data_instance.file_transfer.Add(data_arr[i]);
                        }
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.FREQ_INPUT_SET, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                }
                else if (cmd == "FREQ_INPUT_PULL")
                {
                    if (sjkim_inst.data_instance.comm_state != Communication_enum.NONE)
                    {
                        normal_wait = true;
                        while (sjkim_inst.data_instance.normal_sent == true)
                        {
                        }
                        sjkim_inst.data_instance.freq_data_ret_count = 0;
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.FREQ_INPUT_GET, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                }
                else if(cmd == "FREQ_INPUT_DONE")
                {
                    normal_wait = false;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void InputMeasFormSet(string cmd, string data, string[] data_arr)
        {
            try
            {
                if (sjkim_inst.data_instance.comm_state != Communication_enum.NONE)
                {
                    sjkim_inst.data_instance.file_transfer.Clear();
                    if (cmd == "INPUT_PWR_MEA_VOLTAGE_FETCH")
                    {
                        this.main_push_event[(int)Form_enum.INPUT_PWR_MEA_FORM] = 
                            new PushEventHandler(((input_pwr_mea_form_6)sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.INPUT_PWR_MEA_FORM]).set_textbox_from_main);
                        string[] transfer = { (sjkim_inst.data_instance.input_adc_voltage_0).ToString()};
                        main_push_event[(int)Form_enum.INPUT_PWR_MEA_FORM](cmd: "INPUT_PWR_MEA_VOLTAGE_FETCH", data_arr: transfer);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void FreqProgressFormSet(string cmd, string data, string[] data_arr)
        {
            try
            {

                /*
                freq_pwr_progress_form_send_event(cmd: "FREQ_PWR_PUSH");
                freq_pwr_progress_form_send_event(cmd: "FREQ_PWR_PULL");
                */

                sjkim_inst.data_instance.file_transfer.Clear();

                if (cmd == "FREQ_PWR_PUSH")
                {
                    // data_arr[0] = max data;
                    // data_arr[1] = coupling;
                    // data_arr[2] = freq data arr count;
                    // data_arr[3] = first freq data item length;
                    // data_arr[4] = first frequency value;
                    // data_arr[5] = first freq data atten value
                    // ...
                    // data_arr[5 + Int32.Parse(data_arr[1])] = first freq data fwd adc value
                    // ...
                    // data_arr[5 + (Int32.Parse(data_arr[1]) * 2)] = first freq data fwd dbm value
                    normal_wait = true;
                    while(sjkim_inst.data_instance.normal_sent == true)
                    {

                    }
                    if (data_arr != null)
                    {
                        for (int i = 0; i < data_arr.Length; i++)
                        {
                            sjkim_inst.data_instance.file_transfer.Add(data_arr[i]);
                        }
                    }
                    if (sjkim_inst.data_instance.comm_state != Communication_enum.NONE)
                    {
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.FREQ_PWR_SET, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                }
                else if (cmd == "FREQ_PWR_PULL")
                {
                    sjkim_inst.data_instance.freq_data_ret_count = 0;
                    normal_wait = true;
                    if (sjkim_inst.data_instance.comm_state != Communication_enum.NONE)
                    {
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.FREQ_PWR_GET, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                }
                else if(cmd == "FREQ_PWR_GET_FREQ_INDEX")
                {
                    if (data != null)
                    {
                        sjkim_inst.data_instance.file_transfer.Add("0");
                        sjkim_inst.data_instance.file_transfer.Add(data);
                    }
                    if (sjkim_inst.data_instance.comm_state != Communication_enum.NONE)
                    {
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.FREQ_PWR_GET, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                }
                else if(cmd == "FREQ_PWR_DONE")
                {
                    normal_wait = false;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void FreqMeaFormSet(string cmd, string data, string[] data_arr)
        {
            try
            {
                if (sjkim_inst.data_instance.comm_state != Communication_enum.NONE)
                {
                    sjkim_inst.data_instance.file_transfer.Clear();
                    if (cmd == "FREQ_PWR_MEA_SET_DAC_VOLTAGE")
                    {
                        sjkim_inst.data_instance.file_transfer.Add(((byte)(SjkimCmd.FreqCommand_enum.FWD_DAC_SET)).ToString());
                        sjkim_inst.data_instance.file_transfer.Add(data);
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.FREQ_PWR_SET, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                    else if (cmd == "FREQ_PWR_MEA_VOLTAGE_FETCH")
                    {
                        this.main_push_event[(int)Form_enum.FREQ_PWR_MEA_FORM] = new PushEventHandler(((freq_pwr_mea_form_4)sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.FREQ_PWR_MEA_FORM]).set_textbox_from_main);
                        string[] transfer = { (sjkim_inst.data_instance.dac_adc_voltage_0 / 65535.0F * 5.0F).ToString(),
                                          sjkim_inst.data_instance.fwd_adc_voltage_0.ToString()};
                        main_push_event[(int)Form_enum.FREQ_PWR_MEA_FORM](cmd: "FREQ_PWR_MEA_VOLTAGE_PUSH", data_arr: transfer);
                    }
                }
            }
            catch(Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void ErrorFormSet(string cmd, string data, string[] data_arr)
        {
            if (cmd == "OPERATE_LOG_FORM_LOAD_100")
            {
                if (sjkim_inst.data_instance.comm_state != Communication_enum.NONE)
                {
                    normal_wait = true;
                    ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.LOG_LOAD_100, comm_state: sjkim_inst.data_instance.comm_state);
                }
            }
            else if (cmd == "OPERATE_LOG_FORM_LOAD_1000")
            {
                if (sjkim_inst.data_instance.comm_state != Communication_enum.NONE)
                {
                    normal_wait = true;
                    ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.LOG_LOAD_1000, comm_state: sjkim_inst.data_instance.comm_state);
                }
                /*
                this.main_push_event =
                    new PushEventHandler(((error_log_form_9)sjkim_inst.sjkim_forms[(int)Form_enum.ERROR_LOG_FORM]).load_operate_1000);
                string[] transfer = new string[4000];
                for (int i = 0; i < 1000; i++)
                {
                    transfer[i] = sjkim_inst.data_instance.operate_time[i];
                }
                for (int i = 0; i < 1000; i++)
                {
                    transfer[i + 1000] = sjkim_inst.data_instance.operate_code[i];
                }
                for (int i = 0; i < 1000; i++)
                {
                    transfer[i + 2000] = sjkim_inst.data_instance.operate_value_0[i];
                }
                for (int i = 0; i < 1000; i++)
                {
                    transfer[i + 3000] = sjkim_inst.data_instance.operate_value_1[i];
                }
                main_push_event(cmd: "OPERATE_LOG_FORM_LOAD_1000", data_arr: transfer);
                */
            }
        }

        private void FreqSetSet(string cmd, string data, string[] data_arr)
        {
            if(cmd == "SET_FREQ_FORM_FREQ_SET_DATA")
            {
                sjkim_inst.data_instance.freq_data = data;

                if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                {
                    ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.FREQ_SET, comm_state: sjkim_inst.data_instance.comm_state);
                }
            }
        }

        private void SetParamSet(string cmd, string data, string[] data_arr)
        {
            if (cmd == "SET_PARAM_FORM_READ_FROM_SYS")
            {
                if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                {
                    ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.PARA_GET_0,
                                                     comm_state: sjkim_inst.data_instance.comm_state);
                }
            }
            if (cmd == "SET_PARAM_FORM_WRITE_TO_SYS")
            {
                normal_wait = true;
                for (int i = 0; i < (int)SetParaADC_enum.LENGTH; i++)
                {
                    sjkim_inst.data_instance.para_adc_data[i] = data_arr[i];
                }
                for (int i = 0; i < (int)SetParaTHRESHOLD_enum.LENGTH; i++)
                {
                    sjkim_inst.data_instance.threshold_data[i] = data_arr[i + (int)SetParaADC_enum.LENGTH];
                }

                if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                {
                    ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.PARA_SET_0,
                                                     comm_state: sjkim_inst.data_instance.comm_state);
                }
            }
            if (cmd == "SET_PARAM_FORM_READ_FROM_FILE")
            {
                // todo: read from file and send to form
            }
            if (cmd == "DISPLAY_ALL_TOGGLE_PUSEHD_OFF")
            {
                sjkim_inst.data_instance.display_all_toggle_pushed = false;
            }
            if (cmd == "DISPLAY_ALL_TOGGLE_PUSEHD_ON")
            {
                sjkim_inst.data_instance.display_all_toggle_pushed = true;
            }
            if (cmd == "SET_PARAM_FORM_WRITE_TO_FILE")
            {
                for (int i = 0; i < (int)SetParaADC_enum.LENGTH; i++)
                {
                    sjkim_inst.data_instance.para_adc_data[i] = data_arr[i];
                }
                for (int i = 0; i < (int)SetParaTHRESHOLD_enum.LENGTH; i++)
                {
                    sjkim_inst.data_instance.threshold_data[i] = data_arr[i + (int)SetParaADC_enum.LENGTH];
                }
                // todo: write to file
            }
        }
        private void IPAddressSet(string cmd, string data, string[] data_arr)
        {
            if(cmd == "IP_PORT_WRITE_TO_SYSTEM")
            {
                sjkim_inst.data_instance.ip_address[0] = data_arr[0];
                sjkim_inst.data_instance.ip_address[1] = data_arr[1];
                sjkim_inst.data_instance.ip_address[2] = data_arr[2];
                sjkim_inst.data_instance.ip_address[3] = data_arr[3];
                sjkim_inst.data_instance.port = data_arr[4];
                ParseStringToTransBuff(cmd: CmdList_enum.IP_SET, comm_state: sjkim_inst.data_instance.comm_state);
            }
            if (cmd == "IP_PORT_READ_FROM_SYSTEM")
            {
                // dataGridView2.Rows[1].Cells[1].Value
                // dataGridView2.Rows[2].Cells[1].Value

                this.main_push_event[(int)Form_enum.IP_SETUP_FORM] = new PushEventHandler(((ip_setup_form_1)sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.IP_SETUP_FORM]).set_textbox_from_main);
                string[] transfer = new string[5];
                transfer[0] = sjkim_inst.data_instance.ip_address[0];
                transfer[1] = sjkim_inst.data_instance.ip_address[1];
                transfer[2] = sjkim_inst.data_instance.ip_address[2];
                transfer[3] = sjkim_inst.data_instance.ip_address[3];
                transfer[4] = sjkim_inst.data_instance.port;

                                                
                main_push_event[(int)Form_enum.IP_SETUP_FORM](cmd: "READ_FROM_SYS_IP_DATA", data_arr: transfer);
                // ParseStringToTransBuff(cmd: CmdList_enum.IP_GET, comm_state: sjkim_inst.data_instance.comm_state);
            }

            if (cmd == "IP_PORT_READ_FROM_FILE")
            {
                sjkim_inst.data_instance.ip_address[0] = data_arr[0];
                sjkim_inst.data_instance.ip_address[1] = data_arr[1];
                sjkim_inst.data_instance.ip_address[2] = data_arr[2];
                sjkim_inst.data_instance.ip_address[3] = data_arr[3];
                sjkim_inst.data_instance.port = data_arr[4];
            }
        }

        private void ParaSelectCheck(string cmd, string data, string[] data_arr)
        {
            /*
             * 0_para
             * 1_threshold
             * 2_freq_pwr_mea
             * 3_freq_pwr_proc
             * 4_input_mea
             * 5_input_proc
             */
            switch (data)
            {
                case "0_para":
                    NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.SET_PARAM_FORM],
                            enum_var: (int)Form_enum.SET_PARAM_FORM,
                            location: this.Location);
                    break;
                case "1_freq_pwr_mea":
                    NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.FREQ_PWR_MEA_FORM],
                            enum_var: (int)Form_enum.FREQ_PWR_MEA_FORM,
                            location: this.Location);
                    break;
                case "2_freq_pwr_proc":
                    NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.FREQ_PWR_PROGRESS_FORM],
                            enum_var: (int)Form_enum.FREQ_PWR_PROGRESS_FORM,
                            location: this.Location);
                    break;
                case "3_input_mea":
                    NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.INPUT_PWR_MEA_FORM],
                            enum_var: (int)Form_enum.INPUT_PWR_MEA_FORM,
                            location: this.Location);
                    break;
                case "4_input_proc":
                    NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.INPUT_PWR_PROGRESS_FORM],
                            enum_var: (int)Form_enum.INPUT_PWR_PROGRESS_FORM,
                            location: this.Location);
                    break;
                default:
                    MessageBox.Show("unknown button");
                    return;
            }
        }

        private void login_check(string cmd, string data, string[] data_arr)
        {
            if (data == "true")
            {
                // MessageBox.Show("true");
                NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.PARAMETER_SELECT_FORM],
                        enum_var: (int)Form_enum.PARAMETER_SELECT_FORM,
                        location: this.Location);
                sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.LOGIN_FORM].Visible = false;
            }
            else
            {
                MessageBox.Show("false");
            }
        }
        // error log button
        private void button88_Click(object sender, EventArgs e)
        {
            NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.ERROR_LOG_FORM],
                    enum_var: (int)Form_enum.ERROR_LOG_FORM,
                    location: this.Location);
        }

        // monitoring
        private void button90_Click(object sender, EventArgs e)
        {
            NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.MONITORING_FORM],
                    enum_var: (int)Form_enum.MONITORING_FORM,
                    location: this.Location);
        }
        // ip config
        private void button85_Click(object sender, EventArgs e)
        {
            NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.IP_SETUP_FORM],
                    enum_var: (int)Form_enum.IP_SETUP_FORM,
                    location: this.Location);
        }
        // set freq
        private void button89_Click(object sender, EventArgs e)
        {
            NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.SET_FREQ_FORM],
                    enum_var: (int)Form_enum.SET_FREQ_FORM,
                    location: this.Location);

        }
        // alc mode on -> alc mode on
        private void button20_Click(object sender, EventArgs e)
        {
            now_alc_set();

            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_ON, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        // alc mode on -> alc value load
        private void button18_Click(object sender, EventArgs e)
        {
            now_alc_set();
        }
        // dbm button pushed
        private void button17_Click(object sender, EventArgs e)
        {
            /*
            if(button17.Text == "dBm")
            {
                button17.Text = "W";
            }
            else
            {
                button17.Text = "dBm";
            }
            now_alc_set();
            */
        }

        private void now_alc_set()
        {
            if (sjkim_inst.data_instance.alc_dBm_value != -999)
            { 
                button18.Text = String.Format("{0:0.#}", sjkim_inst.data_instance.alc_dBm_value);
            }
        }

        // read alc button pushed
        private void button22_Click(object sender, EventArgs e)
        {
            // now_alc_set();

            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_READ, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        // ALC OFF BUTTON CLICKED
        private void button72_Click(object sender, EventArgs e)
        {
            // now_agc_set();

            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_OFF, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        private void now_agc_set()
        {
            if(sjkim_inst.data_instance.agc_dB_value != -999)
            {
                button24.Text = String.Format("{0:0.#}", sjkim_inst.data_instance.agc_dB_value);
            }
        }
        // agc read
        private void button64_Click(object sender, EventArgs e)
        {
            now_agc_set();
            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_READ, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        // -0.1 alc button pushed
        private void button32_Click(object sender, EventArgs e)
        {
            sjkim_inst.data_instance.alc_dBm_value -= 0.1F;
            now_alc_set();

            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_SET, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        // +0.1 alc button pushed
        private void button30_Click(object sender, EventArgs e)
        {
            sjkim_inst.data_instance.alc_dBm_value += 0.1F;
            now_alc_set();
            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_SET, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        // -1.0 alc button pushed
        private void button31_Click(object sender, EventArgs e)
        {
            sjkim_inst.data_instance.alc_dBm_value -= 1F;
            now_alc_set();
            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_SET, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        // +1.0 alc button pushed
        private void button29_Click(object sender, EventArgs e)
        {
            sjkim_inst.data_instance.alc_dBm_value += 1F;
            now_alc_set();

            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_SET, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        private void button18_Click_1(object sender, EventArgs e)
        {
            NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.ALC_AGC_FORM],
                    enum_var: (int)Form_enum.ALC_AGC_FORM,
                    location: this.Location);
        }

        // AGC text set button clicked
        private void button24_Click_1(object sender, EventArgs e)
        {
            NewForm(sender: ref sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.ALC_AGC_FORM],
                    enum_var: (int)Form_enum.ALC_AGC_FORM,
                    location: this.Location);
        }

        // agc -0.1 set
        private void button68_Click(object sender, EventArgs e)
        {
            sjkim_inst.data_instance.agc_dB_value -= 0.1F;
            now_agc_set();
            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_SET, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }
        // agc + 0.1
        private void button66_Click(object sender, EventArgs e)
        {
            sjkim_inst.data_instance.agc_dB_value += 0.1F;
            now_agc_set();
            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_SET, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }
        // agc -1
        private void button67_Click(object sender, EventArgs e)
        {
            sjkim_inst.data_instance.agc_dB_value -= 1F;
            now_agc_set();

            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_SET, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }
        // agc +1
        private void button65_Click(object sender, EventArgs e)
        {
            sjkim_inst.data_instance.agc_dB_value += 1F;
            now_agc_set();

            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_SET, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }
        // set time
        private void button91_Click(object sender, EventArgs e)
        {
            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.SET_TIME, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        private void main_form_0_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public void quit_thread_loop()
        {
            try
            {
                int count = 0;
                while (sjkim_inst.data_instance.normal_sent == true)
                {
                    Thread.Sleep(100);
                    count += 1;
                    if (count >= 10)
                    {
                        sjkim_inst.data_instance.normal_sent = false;
                    }
                }
            }
            catch
            {

            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if(button12.Text == "《dBm》")
            {
                button12.Text = "《W》";
            }
            else
            {
                button12.Text = "《dBm》";
            }
            main_display();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (button5.Text == "《dBm》")
            {
                button5.Text = "《W》";
            }
            else
            {
                button5.Text = "《dBm》";
            }
            main_display();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (button12.Text == "《dBm》")
            {
                button12.Text = "《W》";
            }
            else
            {
                button12.Text = "《dBm》";
            }
            main_display();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (button5.Text == "《dBm》")
            {
                button5.Text = "《W》";
            }
            else
            {
                button5.Text = "《dBm》";
            }
            main_display();
        }

        private void comboBox1_Click(object sender, EventArgs e)
        {
            comboBox1.DataSource = SerialPort.GetPortNames();
            // comboBox1.Text = null;
        }

        private void comboBox2_Click(object sender, EventArgs e)
        {
            comboBox2.DataSource = SerialPort.GetPortNames();
            // comboBox2.Text = null;
        }

        private void comboBox3_Click(object sender, EventArgs e)
        {
            comboBox3.DataSource = SerialPort.GetPortNames();
            // comboBox3.Text = null;
        }

        void normal_flag_reset()
        {
            sjkim_inst.data_instance.normal_sent = false;
            wait_flag = false;
            wait = false;
            normal_wait = false;
        }

        // serial open
        private void button78_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen == false)
                {
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.Open();
                    textBox5.Text = "Serial Opened";
                    button78.BackColor = Color.FromArgb(255, 50, 30);
                    sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.SERIAL;
                    if(timer1.Enabled == false)
                    {
                        timer1.Start();
                    }
                    normal_flag_reset();
                    /*
                    if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                    {
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_READ, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                    */
                }
                else
                {
                    textBox5.Text = "Serial Closing";
                    Serial1CloseProcedure();
                    button78.BackColor = Color.FromArgb(255, 255, 255);
                    reset_first_procedrue();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void reset_first_procedrue()
        {
            sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.NONE;
            sjkim_inst.data_instance.normal_sent = true;
            sjkim_inst.data_instance.load_last = false;
            sjkim_inst.data_instance.sys_id_loaded_first = false;
            sjkim_inst.data_instance.agc_loaded_first = false;
            sjkim_inst.data_instance.alc_loaded_first = false;
        }

        private void Serial3CloseProcedure()
        {
            try
            {
                int data = serialPort3.BytesToRead;
                if (data != 0)
                {
                    MessageBox.Show("bluetooth_comminb_bytes_left");
                }
                sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.NONE;
                serialPort3.Close();
                textBox5.Text = "Bluetooth Closed";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Serial2CloseProcedure()
        {
            try
            {
                int data = serialPort2.BytesToRead;
                if (data != 0)
                {
                    MessageBox.Show("usb_comminb_bytes_left");
                }
                sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.NONE;
                serialPort2.Close();
                textBox5.Text = "USB Closed";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Serial1CloseProcedure()
        {
            try
            {
                int data = serialPort1.BytesToRead;
                if (data != 0)
                {
                    MessageBox.Show("serial_comminb_bytes_left wait..");
                }
                while (serialPort1.BytesToRead != 0)
                {
                }
                sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.NONE;
                serialPort1.Close();
                textBox5.Text = "Serial Closed";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                lock(thislock)
                {
                    this.Invoke(new EventHandler(Serial1DataProcedure));
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // int index_text = 0;
        // byte[] test_receive = new byte[4095];


        private void Serial1DataProcedure(object s, EventArgs e)
        {
            byte[] arr = new byte[4095];
            int length = serialPort1.BytesToRead;
            serialPort1.Read(arr, 0, length);
            /*
            for(int i = 0; i < length; i++)
            {
                test_receive[index_text] = arr[i];
                index_text += 1;
            }
            */
            /*string _indata = indata;
            byte[] arr = Encoding.Default.GetBytes(indata);*/
            if(sjkim_inst.data_instance.cmd_mode_on == true)
            {
                main_push_event[(int)Form_enum.MONITORING_FORM](cmd: "RECEIVED_DATA", data: Encoding.ASCII.GetString(arr));
            }

            for (int i = 0; i < length; i++)
            {
                if (arr[i] == (byte)'*')
                {
                    sjkim_inst.data_instance.serial_1_special_char_detected = true;
                }
                else if (arr[i] == (byte)'0') // index = 0
                {
                    if (sjkim_inst.data_instance.serial_1_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_1_receive_buffer.Clear();
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_1_receive_buffer.Add((byte)'0');
                    }
                    sjkim_inst.data_instance.serial_1_special_char_detected = false;
                }
                else if (arr[i] == (byte)'1') // parse start
                {
                    if (sjkim_inst.data_instance.serial_1_special_char_detected == true)
                    {
                        ParseReceiveBUff();
                        cmd_procedure();
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_1_receive_buffer.Add((byte)'1');
                    }
                    sjkim_inst.data_instance.serial_1_special_char_detected = false;
                }
                else if (arr[i] == (byte)'2') // special char 처리
                {
                    if (sjkim_inst.data_instance.serial_1_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_1_receive_buffer.Add((byte)'*');
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_1_receive_buffer.Add((byte)'2');
                    }
                    sjkim_inst.data_instance.serial_1_special_char_detected = false;
                }
                else if (arr[i] == (byte)'3') // special char 처리
                {
                    if (sjkim_inst.data_instance.serial_1_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_1_receive_buffer.Add((byte)'\r');
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_1_receive_buffer.Add((byte)'3');
                    }
                    sjkim_inst.data_instance.serial_1_special_char_detected = false;
                }
                else if (arr[i] == (byte)'4') // special char 처리
                {
                    if (sjkim_inst.data_instance.serial_1_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_1_receive_buffer.Add((byte)'\n');
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_1_receive_buffer.Add((byte)'4');
                    }
                    sjkim_inst.data_instance.serial_1_special_char_detected = false;
                }
                else
                {
                    sjkim_inst.data_instance.serial_1_receive_buffer.Add((byte)arr[i]);
                    sjkim_inst.data_instance.serial_1_special_char_detected = false;
                }
            }
        }

        // int index_text_2 = 0;
        // byte[] test_receive_2 = new byte[4095];

        private void Serial2DataProcedure(object s, EventArgs e)
        {
            byte[] arr = new byte[4095];
            int length = serialPort2.BytesToRead;
            serialPort2.Read(arr, 0, length);
            /*
            for(int i = 0; i < length; i++)
            {
                test_receive[index_text] = arr[i];
                index_text += 1;
            }
            */
            /*string _indata = indata;
            byte[] arr = Encoding.Default.GetBytes(indata);*/
            if (sjkim_inst.data_instance.cmd_mode_on == true)
            {
                main_push_event[(int)Form_enum.MONITORING_FORM](cmd: "RECEIVED_DATA", data: Encoding.ASCII.GetString(arr));
            }
            for (int i = 0; i < length; i++)
            {
                if (arr[i] == (byte)'*')
                {
                    sjkim_inst.data_instance.serial_2_special_char_detected = true;
                }
                else if (arr[i] == (byte)'0') // index = 0
                {
                    if (sjkim_inst.data_instance.serial_2_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_2_receive_buffer.Clear();
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_2_receive_buffer.Add((byte)'0');
                    }
                    sjkim_inst.data_instance.serial_2_special_char_detected = false;
                }
                else if (arr[i] == (byte)'1') // parse start
                {
                    if (sjkim_inst.data_instance.serial_2_special_char_detected == true)
                    {
                        ParseReceiveBUff();
                        cmd_procedure();
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_2_receive_buffer.Add((byte)'1');
                    }
                    sjkim_inst.data_instance.serial_2_special_char_detected = false;
                }
                else if (arr[i] == (byte)'2') // special char 처리
                {
                    if (sjkim_inst.data_instance.serial_2_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_2_receive_buffer.Add((byte)'*');
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_2_receive_buffer.Add((byte)'2');
                    }
                    sjkim_inst.data_instance.serial_2_special_char_detected = false;
                }
                else if (arr[i] == (byte)'3') // special char 처리
                {
                    if (sjkim_inst.data_instance.serial_2_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_2_receive_buffer.Add((byte)'\r');
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_2_receive_buffer.Add((byte)'3');
                    }
                    sjkim_inst.data_instance.serial_2_special_char_detected = false;
                }
                else if (arr[i] == (byte)'4') // special char 처리
                {
                    if (sjkim_inst.data_instance.serial_2_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_2_receive_buffer.Add((byte)'\n');
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_2_receive_buffer.Add((byte)'4');
                    }
                    sjkim_inst.data_instance.serial_2_special_char_detected = false;
                }
                else
                {
                    sjkim_inst.data_instance.serial_2_receive_buffer.Add((byte)arr[i]);
                    sjkim_inst.data_instance.serial_2_special_char_detected = false;
                }
            }
        }

        // int index_text_3 = 0;
        // byte[] test_receive_3 = new byte[4095];

        private void Serial3DataProcedure(object s, EventArgs e)
        {
            byte[] arr = new byte[4095];
            int length = serialPort3.BytesToRead;
            serialPort3.Read(arr, 0, length);
            /*
            for(int i = 0; i < length; i++)
            {
                test_receive[index_text] = arr[i];
                index_text += 1;
            }
            */
            /*string _indata = indata;
            byte[] arr = Encoding.Default.GetBytes(indata);*/
            if (sjkim_inst.data_instance.cmd_mode_on == true)
            {
                main_push_event[(int)Form_enum.MONITORING_FORM](cmd: "RECEIVED_DATA", data: Encoding.ASCII.GetString(arr));
            }
            for (int i = 0; i < length; i++)
            {
                if (arr[i] == (byte)'*')
                {
                    sjkim_inst.data_instance.serial_3_special_char_detected = true;
                }
                else if (arr[i] == (byte)'0') // index = 0
                {
                    if (sjkim_inst.data_instance.serial_3_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_3_receive_buffer.Clear();
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_3_receive_buffer.Add((byte)'0');
                    }
                    sjkim_inst.data_instance.serial_3_special_char_detected = false;
                }
                else if (arr[i] == (byte)'1') // parse start
                {
                    if (sjkim_inst.data_instance.serial_3_special_char_detected == true)
                    {
                        ParseReceiveBUff();
                        cmd_procedure();
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_3_receive_buffer.Add((byte)'1');
                    }
                    sjkim_inst.data_instance.serial_3_special_char_detected = false;
                }
                else if (arr[i] == (byte)'2') // special char 처리
                {
                    if (sjkim_inst.data_instance.serial_3_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_3_receive_buffer.Add((byte)'*');
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_3_receive_buffer.Add((byte)'2');
                    }
                    sjkim_inst.data_instance.serial_3_special_char_detected = false;
                }
                else if (arr[i] == (byte)'3') // special char 처리
                {
                    if (sjkim_inst.data_instance.serial_3_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_3_receive_buffer.Add((byte)'\r');
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_3_receive_buffer.Add((byte)'3');
                    }
                    sjkim_inst.data_instance.serial_3_special_char_detected = false;
                }
                else if (arr[i] == (byte)'4') // special char 처리
                {
                    if (sjkim_inst.data_instance.serial_3_special_char_detected == true)
                    {
                        sjkim_inst.data_instance.serial_3_receive_buffer.Add((byte)'\n');
                    }
                    else
                    {
                        sjkim_inst.data_instance.serial_3_receive_buffer.Add((byte)'4');
                    }
                    sjkim_inst.data_instance.serial_3_special_char_detected = false;
                }
                else
                {
                    sjkim_inst.data_instance.serial_3_receive_buffer.Add((byte)arr[i]);
                    sjkim_inst.data_instance.serial_3_special_char_detected = false;
                }
            }
        }

        void cmd_procedure()
        {
            switch ((byte)sjkim_inst.data_instance.receive_cmd_state)
            {
                case (byte)CmdList_enum.ONLINE:            // 0
                    break;
                case (byte)CmdList_enum.STANDBY:           // 1
                    break;
                case (byte)CmdList_enum.ALC_ON:            // 2
                    break;
                case (byte)CmdList_enum.ALC_OFF:           // 3
                    break;
                case (byte)CmdList_enum.ALC_SET:           // 4
                    break;
                case (byte)CmdList_enum.AGC_SET:           // 5
                    break;
                case (byte)CmdList_enum.ALC_READ:          // 6
                    if(button18.InvokeRequired)
                    {
                        this.Invoke(new Action(delegate ()
                        {
                            if(sjkim_inst.data_instance.alc_dBm_value != -999)
                            {
                                button18.Text = sjkim_inst.data_instance.alc_dBm_value.ToString();
                            }
                        }));
                    }
                    else
                    {
                        if (sjkim_inst.data_instance.alc_dBm_value != -999)
                        {
                            button18.Text = sjkim_inst.data_instance.alc_dBm_value.ToString();
                        }
                    }

                    if (sjkim_inst.data_instance.sys_id_loaded_first == false)
                    {
                        sjkim_inst.data_instance.sys_id_loaded_first = true;
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.SYS_ID_GET, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                    break;
                case (byte)CmdList_enum.AGC_READ:          // 7
                    if (button24.InvokeRequired)
                    {
                        this.Invoke(new Action(delegate ()
                        {
                            if (sjkim_inst.data_instance.agc_dB_value != -999)
                            {
                                button24.Text = sjkim_inst.data_instance.agc_dB_value.ToString();
                            }
                        }));
                    }
                    else
                    {
                        if (sjkim_inst.data_instance.agc_dB_value != -999)
                        {
                            button24.Text = sjkim_inst.data_instance.agc_dB_value.ToString();
                        }
                    }
                    if (sjkim_inst.data_instance.alc_loaded_first == false)
                    {
                        sjkim_inst.data_instance.alc_loaded_first = true;
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_READ, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                    break;
                case (byte)CmdList_enum.SET_TIME:          // 8
                    break;
                case (byte)CmdList_enum.FREQ_SET:          // 9
                    break;
                case (byte)CmdList_enum.SERIAL_START:      // 10
                    break;
                case (byte)CmdList_enum.USB_START:         // 11
                    break;
                case (byte)CmdList_enum.ETHERNETSTART:     // 12
                    break;
                case (byte)CmdList_enum.SERIAL_STOP:       // 13
                    break;
                case (byte)CmdList_enum.USB_STOP:          // 14
                    break;
                case (byte)CmdList_enum.ETHERNET_STOP:     // 15
                    break;
                case (byte)CmdList_enum.IP_SET:            // 16
                    break;
                case (byte)CmdList_enum.IP_GET:            // 17
                    break;
                case (byte)CmdList_enum.LOG_LOAD_100:      // 18
                    break;
                case (byte)CmdList_enum.LOG_LOAD_1000:     // 19
                    break;
                case (byte)CmdList_enum.PARA_SET_0:        // 20
                    {
                        this.main_push_event[(int)Form_enum.SET_PARAM_FORM] = new PushEventHandler(((set_param_form_3)sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.SET_PARAM_FORM]).set_datagrid_from_main);
                        main_push_event[(int)Form_enum.SET_PARAM_FORM](cmd: "SET_PARAM_FORM_PARAM_WRITED");
                    }
                    break;
                case (byte)CmdList_enum.PARA_GET_0:        // 21
                    break;
                case (byte)CmdList_enum.PARA_GET_1:        // 22
                    {
                        this.main_push_event[(int)Form_enum.SET_PARAM_FORM] = new PushEventHandler(((set_param_form_3)sjkim_inst.sjkim_forms[(int)SjkimData.Form_enum.SET_PARAM_FORM]).set_datagrid_from_main);
                        string[] transfer = new string[(int)SetParaADC_enum.LENGTH + (int)SetParaTHRESHOLD_enum.LENGTH];
                        for (int i = 0; i < (int)SetParaADC_enum.LENGTH; i++)
                        {
                            transfer[i] = sjkim_inst.data_instance.para_adc_data[i];
                        }
                        for (int i = 0; i < (int)SetParaTHRESHOLD_enum.LENGTH; i++)
                        {
                            transfer[i + (int)SetParaADC_enum.LENGTH] = sjkim_inst.data_instance.threshold_data[i];
                        }
                        main_push_event[(int)Form_enum.SET_PARAM_FORM](cmd: "SET_PARAM_FORM_DATAGRID_SET_DATA_ARR", data_arr: transfer);
                    }
                    break;
                case (byte)CmdList_enum.FREQ_PWR_GET:      // 23
                    break;
                case (byte)CmdList_enum.FREQ_PWR_SET:      // 24
                    break;
                case (byte)CmdList_enum.FREQ_INPUT_GET:    // 25
                    break;
                case (byte)CmdList_enum.FREQ_INPUT_SET:    // 26
                    break;
                case (byte)CmdList_enum.NORMAL:            // 27
                    {
                        if (sjkim_inst.data_instance.load_last == false)
                        {
                            sjkim_inst.data_instance.load_last = true;
                            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                            {
                                Properties.Settings.Default.last_connect_type = String.Empty;


                                switch (sjkim_inst.data_instance.comm_state)
                                {
                                    case SjkimCmd.Communication_enum.SERIAL:
                                        Properties.Settings.Default.last_connect_type = "Serial";
                                        Properties.Settings.Default.last_serial_portname = serialPort1.PortName;
                                        break;
                                    case SjkimCmd.Communication_enum.USB:
                                        Properties.Settings.Default.last_connect_type = "USB";
                                        Properties.Settings.Default.last_serial_portname = serialPort2.PortName;
                                        break;
                                    case SjkimCmd.Communication_enum.BLUETOOTH:
                                        Properties.Settings.Default.last_connect_type = "Bluetooth";
                                        Properties.Settings.Default.last_serial_portname = serialPort3.PortName;
                                        break;
                                    case SjkimCmd.Communication_enum.ETHERNET:
                                        Properties.Settings.Default.last_connect_type = "Ethernet";
                                        break;
                                    default:
                                        break;
                                }
                                Properties.Settings.Default.Save();

                                if (sjkim_inst.data_instance.agc_loaded_first == false)
                                {
                                    normal_wait = true;
                                    sjkim_inst.data_instance.agc_loaded_first = true;
                                    ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_READ, comm_state: sjkim_inst.data_instance.comm_state);
                                }
                            }
                        }
                        break;
                    }
                case (byte)CmdList_enum.SYS_ID_GET:        // 28
                    normal_wait = false;
                    break;
                case (byte)CmdList_enum.FAULT_LOG:         // 29
                    break;
                case (byte)CmdList_enum.LENGTH:            // receive procedure
                    break;
                default:
                    break;
            }
        }

        private void parse_start(string ch)
        {
            switch(ch)
            {
                case "serial_1":
                    break;
                case "serial_2":
                    break;
                case "serial_3":
                    break;
                case "ethernet":
                    break;
                default:
                    MessageBox.Show("unknonw parse channel");
                    break;
            }
        }

        // ethernet open button clicked
        private void button81_Click(object sender, EventArgs e)
        {
            try
            {
                if (sjkim_inst.data_instance.client == null)
                {
                    sjkim_inst.data_instance.ethernet_connected = false;
                    if(sjkim_inst.data_instance.sr != null)
                    {
                        sjkim_inst.data_instance.sr.Close();
                    }
                    if (sjkim_inst.data_instance.sw != null)
                    {
                        sjkim_inst.data_instance.sw.Close();
                    }
                    if (sjkim_inst.data_instance.ns != null)
                    {
                        sjkim_inst.data_instance.ns.Close();
                    }
                    if (ethernet_thread != null)
                    {
                        ethernet_thread.Abort();
                    }

                    sjkim_inst.data_instance.ethernet_connected = true;
                    string ip_buff = String.Empty;
                    for (int i = 0; i < 3; i++)
                    {
                        ip_buff += sjkim_inst.data_instance.ip_address[i] + ".";
                    }
                    ip_buff += sjkim_inst.data_instance.ip_address[3];

                    sjkim_inst.data_instance.client = new TcpClient();
                    var result = sjkim_inst.data_instance.client.BeginConnect(ip_buff, Int32.Parse(sjkim_inst.data_instance.port), null, null);

                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                    if (!success)
                    {
                        throw new Exception("Failed to connect.");
                    }
                    sjkim_inst.data_instance.ns = sjkim_inst.data_instance.client.GetStream();
                    sjkim_inst.data_instance.sr = new System.IO.StreamReader(sjkim_inst.data_instance.ns);
                    sjkim_inst.data_instance.sw = new System.IO.StreamWriter(sjkim_inst.data_instance.ns);
                    sjkim_inst.data_instance.bw = new System.IO.BinaryWriter(sjkim_inst.data_instance.ns);

                    ethernet_thread = new Thread(new ThreadStart(ethernet_received));
                    ethernet_thread.IsBackground = true;
                    ethernet_thread.Start();

                    textBox5.Text = "Ethernet Opened";
                    button81.BackColor = Color.FromArgb(255, 50, 30);
                    sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.ETHERNET;
                    sjkim_inst.data_instance.ethernet_opened_first = true;
                    if (timer1.Enabled == false)
                    {
                        timer1.Start();
                    }
                    normal_flag_reset();
                    /*
                    if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                    {
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_READ, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                    */
                }
                else
                {
                    sjkim_inst.data_instance.ethernet_connected = false;
                    sjkim_inst.data_instance.sr.Close();
                    sjkim_inst.data_instance.sw.Close();
                    sjkim_inst.data_instance.ns.Close();
                    sjkim_inst.data_instance.client.Close();
                    if (ethernet_thread != null)
                    {
                        ethernet_thread.Abort();
                    }

                    textBox5.Text = "Ethernet Closing";
                    button81.BackColor = Color.FromArgb(255, 255, 255);
                    sjkim_inst.data_instance.client = null;
                    reset_first_procedrue();
                }
            }
            catch (Exception ex)
            {
                sjkim_inst.data_instance.client = null;
                sjkim_inst.data_instance.ethernet_connected = false;
                sjkim_inst.data_instance.normal_sent = true;
                MessageBox.Show(ex.ToString());
            }
        }

        public void ethernet_received()
        {
            try
            {
                byte[] bytes = new byte[1024];
                while (sjkim_inst.data_instance.ethernet_connected)
                {
                    Thread.Sleep(300);
                    int length;


                    while ((length = sjkim_inst.data_instance.ns.Read(bytes, 0, bytes.Length)) != 0) // cmd도 receive되는 문제가 있음.
                    {
                        if (sjkim_inst.data_instance.cmd_mode_on == true)
                        {
                            main_push_event[(int)Form_enum.MONITORING_FORM](cmd: "RECEIVED_DATA", data: Encoding.ASCII.GetString(bytes, 0, length));
                        }

                        for (int i = 0; i < length; i++)
                        {
                            if (bytes[i] == (byte)'*')
                            {
                                sjkim_inst.data_instance.ethernet_special_char_detected = true;
                            }
                            else if (bytes[i] == (byte)'0') // index = 0
                            {
                                if (sjkim_inst.data_instance.ethernet_special_char_detected == true)
                                {
                                    sjkim_inst.data_instance.ethernet_receive_buffer.Clear();
                                }
                                else
                                {
                                    sjkim_inst.data_instance.ethernet_receive_buffer.Add((byte)'0');
                                }
                                sjkim_inst.data_instance.ethernet_special_char_detected = false;
                            }
                            else if (bytes[i] == (byte)'1') // parse start
                            {
                                if (sjkim_inst.data_instance.ethernet_special_char_detected == true)
                                {
                                    ParseReceiveBUff();
                                    cmd_procedure();
                                }
                                else
                                {
                                    sjkim_inst.data_instance.ethernet_receive_buffer.Add((byte)'1');
                                }
                                sjkim_inst.data_instance.ethernet_special_char_detected = false;
                            }
                            else if (bytes[i] == (byte)'2') // special char 처리
                            {
                                if (sjkim_inst.data_instance.ethernet_special_char_detected == true)
                                {
                                    sjkim_inst.data_instance.ethernet_receive_buffer.Add((byte)'*');
                                }
                                else
                                {
                                    sjkim_inst.data_instance.ethernet_receive_buffer.Add((byte)'2');
                                }
                                sjkim_inst.data_instance.ethernet_special_char_detected = false;
                            }
                            else if (bytes[i] == (byte)'3') // special char 처리
                            {
                                if (sjkim_inst.data_instance.ethernet_special_char_detected == true)
                                {
                                    sjkim_inst.data_instance.ethernet_receive_buffer.Add((byte)'\r');
                                }
                                else
                                {
                                    sjkim_inst.data_instance.ethernet_receive_buffer.Add((byte)'3');
                                }
                                sjkim_inst.data_instance.ethernet_special_char_detected = false;
                            }
                            else if (bytes[i] == (byte)'4') // special char 처리
                            {
                                if (sjkim_inst.data_instance.ethernet_special_char_detected == true)
                                {
                                    sjkim_inst.data_instance.ethernet_receive_buffer.Add((byte)'\n');
                                }
                                else
                                {
                                    sjkim_inst.data_instance.ethernet_receive_buffer.Add((byte)'4');
                                }
                                sjkim_inst.data_instance.ethernet_special_char_detected = false;
                            }
                            else
                            {
                                sjkim_inst.data_instance.ethernet_receive_buffer.Add((byte)bytes[i]);
                                sjkim_inst.data_instance.ethernet_special_char_detected = false;
                            }
                        }
                        
                    }
                }
                
            }
            catch
            {
                // MessageBox.Show(ex.ToString());
            }
        }

        // set alc
        private void button28_Click(object sender, EventArgs e)
        {
            if(sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.ALC_SET, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }
        // set atten
        private void button71_Click(object sender, EventArgs e)
        {
            if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
            {
                ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_SET, comm_state: sjkim_inst.data_instance.comm_state);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer_callback();
        }

        // int log100_count = 0;

        void ParseReceiveBUff()
        {
            try
            {
                List<byte> receive_buffer_byte_list = new List<byte>();
                switch (sjkim_inst.data_instance.comm_state)
                {
                    case Communication_enum.NONE:
                        break;
                    case Communication_enum.SERIAL:
                        receive_buffer_byte_list = new List<byte>(sjkim_inst.data_instance.serial_1_receive_buffer);
                        break;
                    case Communication_enum.USB:
                        receive_buffer_byte_list = new List<byte>(sjkim_inst.data_instance.serial_2_receive_buffer);
                        break;
                    case Communication_enum.BLUETOOTH:
                        receive_buffer_byte_list = new List<byte>(sjkim_inst.data_instance.serial_3_receive_buffer);
                        break;
                    case Communication_enum.ETHERNET:
                        receive_buffer_byte_list = new List<byte>(sjkim_inst.data_instance.ethernet_receive_buffer);
                        break;
                    default:
                        break;
                }
                if (receive_buffer_byte_list[0] != (byte)CmdList_enum.NORMAL)
                {
                }
                byte checksum = receive_buffer_byte_list[receive_buffer_byte_list.Count - 1];
                byte checksum_buff = receive_buffer_byte_list[0];
                for (int i = 1; i < receive_buffer_byte_list.Count - 1; i++)
                {
                    checksum_buff ^= receive_buffer_byte_list[i];
                }
                if(checksum == checksum_buff)
                {
                    if (receive_buffer_byte_list.Count != 0)
                    {
                        bool cmd_only = false;
                        if(receive_buffer_byte_list.Count == 2)
                        {
                            cmd_only = true;
                        }
                        else
                        {
                            cmd_only = false;
                        }
                        switch (receive_buffer_byte_list[0])
                        {
                            case (byte)CmdList_enum.ONLINE:            // 0
                                break;
                            case (byte)CmdList_enum.STANDBY:           // 1
                                break;
                            case (byte)CmdList_enum.ALC_ON:            // 2
                                break;
                            case (byte)CmdList_enum.ALC_OFF:           // 3
                                break;
                            case (byte)CmdList_enum.ALC_SET:           // 4
                                break;
                            case (byte)CmdList_enum.AGC_SET:           // 5
                                break;
                            case (byte)CmdList_enum.ALC_READ:          // 6
                                if(cmd_only == false)
                                {
                                    byte[] alc_buff = new byte[4];
                                    for (int i = 0; i < 4; i++)
                                    {
                                        alc_buff[i] = receive_buffer_byte_list[1 + i];
                                    }
                                    sjkim_inst.data_instance.alc_dBm_value = 
                                        (float)Math.Round(sjkim_inst.data_instance.FloatDataGet(alc_buff), 1);
                                }
                                break;
                            case (byte)CmdList_enum.AGC_READ:          // 7
                                if (cmd_only == false)
                                {
                                    byte[] agc_buff = new byte[4];
                                    for (int i = 0; i < 4; i++)
                                    {
                                        agc_buff[i] = receive_buffer_byte_list[1 + i];
                                    }
                                    sjkim_inst.data_instance.agc_dB_value = 
                                        (float)Math.Round(sjkim_inst.data_instance.FloatDataGet(agc_buff), 1);
                                }
                                break;
                            case (byte)CmdList_enum.SET_TIME:          // 8
                                break;
                            case (byte)CmdList_enum.FREQ_SET:          // 9
                                break;
                            case (byte)CmdList_enum.SERIAL_START:      // 10
                                break;
                            case (byte)CmdList_enum.USB_START:         // 11
                                break;
                            case (byte)CmdList_enum.ETHERNETSTART:     // 12
                                break;
                            case (byte)CmdList_enum.SERIAL_STOP:       // 13
                                break;
                            case (byte)CmdList_enum.USB_STOP:          // 14
                                break;
                            case (byte)CmdList_enum.ETHERNET_STOP:     // 15
                                break;
                            case (byte)CmdList_enum.IP_SET:            // 16
                                break;
                            case (byte)CmdList_enum.IP_GET:            // 17
                                if (cmd_only == false)
                                {
                                    byte[] agc_buff = new byte[4];
                                    for (int i = 0; i < 4; i++)
                                    {
                                        agc_buff[i] = receive_buffer_byte_list[1 + i];
                                    }
                                    sjkim_inst.data_instance.agc_dB_value =
                                        (float)Math.Round(sjkim_inst.data_instance.FloatDataGet(agc_buff), 1);
                                }
                                break;
                            case (byte)CmdList_enum.LOG_LOAD_100:      // 18
                                if (cmd_only == false)
                                {
                                    ParseStringToTransBuff(cmd: CmdList_enum.LOG_READ, comm_state: sjkim_inst.data_instance.comm_state);
                                }
                                break;
                            case (byte)CmdList_enum.LOG_LOAD_1000:     // 19
                                if (cmd_only == false)
                                {
                                    ParseStringToTransBuff(cmd: CmdList_enum.LOG_READ, comm_state: sjkim_inst.data_instance.comm_state);
                                }
                                break;
                            case (byte)CmdList_enum.LOG_READ:          // 19
                                if (cmd_only == false)
                                {
                                    // index 1: state
                                    // index 2: year
                                    // index 3: month
                                    // index 4: date
                                    // index 5: hour
                                    // index 6: minute
                                    // index 7: second
                                    // index 8: cmd
                                    // index 9 ~ 12: value_0
                                    int parse_index = 2;
                                    for(int i = parse_index; i < receive_buffer_byte_list.Count - 1; i += 15)
                                    {
                                        string time_string = receive_buffer_byte_list[i].ToString() + "y "    // year
                                                             + receive_buffer_byte_list[i + 1].ToString() + "m "  // month
                                                             + receive_buffer_byte_list[i + 2].ToString() + "d "  // date
                                                             + receive_buffer_byte_list[i + 3].ToString() + "h "  // hour
                                                             + receive_buffer_byte_list[i + 4].ToString() + "m "  // minute
                                                             + receive_buffer_byte_list[i + 5].ToString() + "s";  // second
                                        byte cmd_buff = receive_buffer_byte_list[i + 6];

                                        byte[] buff_0 = { receive_buffer_byte_list[i + 7], receive_buffer_byte_list[i + 8],
                                                          receive_buffer_byte_list[i + 9], receive_buffer_byte_list[i + 10] };
                                        float value_0 = BitConverter.ToSingle(buff_0, 0);

                                        byte[] buff_1 = { receive_buffer_byte_list[i + 11], receive_buffer_byte_list[i + 12],
                                                          receive_buffer_byte_list[i + 13], receive_buffer_byte_list[i + 14] };
                                        float value_1 = BitConverter.ToSingle(buff_1, 0);

                                        main_push_event[(int)Form_enum.ERROR_LOG_FORM](cmd: "ERROR_LOG_WRITE_TIME", data: time_string);
                                        main_push_event[(int)Form_enum.ERROR_LOG_FORM](cmd: "ERROR_LOG_WRITE_CMD", data: cmd_buff.ToString());
                                        main_push_event[(int)Form_enum.ERROR_LOG_FORM](cmd: "ERROR_LOG_WRITE_VALUE_0", data: value_0.ToString());
                                        main_push_event[(int)Form_enum.ERROR_LOG_FORM](cmd: "ERROR_LOG_WRITE_VALUE_1", data: value_1.ToString());
                                    }

                                    if (receive_buffer_byte_list[1] == 0)
                                    {
                                        ParseStringToTransBuff(cmd: CmdList_enum.LOG_READ, comm_state: sjkim_inst.data_instance.comm_state);
                                    }
                                    else if (receive_buffer_byte_list[1] == 1)
                                    {
                                        main_push_event[(int)Form_enum.ERROR_LOG_FORM](cmd: "ERROR_LOG_WRITE_DONE");
                                        normal_wait = false;
                                        ParseStringToTransBuff(cmd: CmdList_enum.NORMAL, comm_state: sjkim_inst.data_instance.comm_state);
                                    }
                                }
                                break;
                            case (byte)CmdList_enum.PARA_SET_0:        // 20
                                if (cmd_only == false)
                                {
                                    normal_wait = false;
                                }
                                break;
                            case (byte)CmdList_enum.PARA_GET_0:        // 21
                                if (cmd_only == false)
                                {
                                    int index = 1;
                                    for (int i = 0; i < (int)SetParaADC_enum.LENGTH; i++)
                                    {
                                        byte[] byte_buff = new byte[4];
                                        for (int j = 0; j < 4; j++)
                                        {
                                            byte_buff[j] = receive_buffer_byte_list[index + j];
                                        }
                                        sjkim_inst.data_instance.para_adc_data[i] = String.Format("{0:0.0#}", BitConverter.ToSingle(byte_buff, 0));
                                        index += 4;
                                    }
                                    ParseStringToTransBuff(cmd: CmdList_enum.PARA_GET_1, comm_state: sjkim_inst.data_instance.comm_state);
                                }
                                break;
                            case (byte)CmdList_enum.PARA_GET_1:          // 22
                                if (cmd_only == false)
                                {
                                    int index = 1;
                                    for (int i = 0; i < (int)SetParaTHRESHOLD_enum.LENGTH; i++)
                                    {
                                        byte[] byte_buff = new byte[4];
                                        for (int j = 0; j < 4; j++)
                                        {
                                            byte_buff[j] = receive_buffer_byte_list[index + j];
                                        }
                                        sjkim_inst.data_instance.threshold_data[i] = String.Format("{0:0.0#}", BitConverter.ToSingle(byte_buff, 0));
                                        index += 4;
                                    }
                                }
                                break;
                            case (byte)CmdList_enum.FREQ_PWR_GET:      // 23
                                if(cmd_only == false)
                                {
                                    int index = 1;
                                    int freq_lenght_max = BitConverter.ToInt32(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    float ref_0dbm = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    float offset = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    int item_length = BitConverter.ToInt32(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    int freq_mhz_data = BitConverter.ToInt32(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    string[] transfer_option = new string[5];
                                    transfer_option[0] = freq_lenght_max.ToString();
                                    transfer_option[1] = ref_0dbm.ToString();
                                    transfer_option[2] = offset.ToString(); 
                                    transfer_option[3] = item_length.ToString();
                                    transfer_option[4] = freq_mhz_data.ToString();
                                    List<string> item = new List<string>();
                                    for (int i = 0; i < item_length; i++)
                                    {
                                        item.Add((BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index)).ToString());
                                        index += 4;
                                    }
                                    switch (sjkim_inst.data_instance.freq_data_ret_count)
                                    {
                                        case 0:
                                            {
                                                if (main_push_event[(int)Form_enum.FREQ_PWR_PROGRESS_FORM] != null)
                                                {
                                                    this.main_push_event[(int)Form_enum.FREQ_PWR_PROGRESS_FORM](cmd: "FREQ_PWR_GET_OPTION", data_arr: transfer_option);
                                                    this.main_push_event[(int)Form_enum.FREQ_PWR_PROGRESS_FORM](cmd: "FREQ_PWR_GET_ATTEN_DATA", data_arr: item.ToArray()) ;
                                                }
                                                break;
                                            }
                                        case 1:
                                            {
                                                if (main_push_event[(int)Form_enum.FREQ_PWR_PROGRESS_FORM] != null)
                                                {
                                                    this.main_push_event[(int)Form_enum.FREQ_PWR_PROGRESS_FORM](cmd: "FREQ_PWR_GET_FORWARD_DET_DATA", data_arr: item.ToArray(), data: freq_mhz_data.ToString());
                                                }
                                                break;
                                            }
                                        case 2:
                                            {
                                                if (main_push_event[(int)Form_enum.FREQ_PWR_PROGRESS_FORM] != null)
                                                {
                                                    this.main_push_event[(int)Form_enum.FREQ_PWR_PROGRESS_FORM](cmd: "FREQ_PWR_GET_DBM_DATA", data_arr: item.ToArray(), data: freq_mhz_data.ToString());
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    sjkim_inst.data_instance.freq_data_ret_count += 1;
                                    sjkim_inst.data_instance.freq_data_ret_count %= 3;
                                }
                                break;
                            case (byte)CmdList_enum.FREQ_PWR_SET:      // 24
                                if (cmd_only == false)
                                {
                                    if(receive_buffer_byte_list.Count == 3 && receive_buffer_byte_list[1] == 1)
                                    {
                                        int index = 1;
                                        byte file_command = receive_buffer_byte_list[index];
                                        index += 1;
                                        switch (file_command)
                                        {
                                            case (byte)SjkimCmd.FreqCommand_enum.FWD_DAC_SET:
                                                break;
                                            case (byte)SjkimCmd.FreqCommand_enum.FWD_FILE_SET:
                                                if (main_push_event[(int)Form_enum.FREQ_PWR_PROGRESS_FORM] != null)
                                                {
                                                    this.main_push_event[(int)Form_enum.FREQ_PWR_PROGRESS_FORM](cmd: "FREQ_PWR_SET_RETURN");
                                                }
                                                break;
                                            case (byte)SjkimCmd.FreqCommand_enum.FWD_FILE_GET:
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                                
                                break;
                            case (byte)CmdList_enum.FREQ_INPUT_GET:    // 25
                                {
                                    if (cmd_only == false)
                                    {
                                        int index = 1;
                                        int freq_lenght_max = BitConverter.ToInt32(receive_buffer_byte_list.ToArray(), index);
                                        index += 4;
                                        float step_value = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                        index += 4;
                                        int item_length = BitConverter.ToInt32(receive_buffer_byte_list.ToArray(), index);
                                        index += 4;
                                        int freq_mhz_data = BitConverter.ToInt32(receive_buffer_byte_list.ToArray(), index);
                                        index += 4;
                                        string[] transfer_option = new string[4];
                                        transfer_option[0] = freq_lenght_max.ToString();
                                        transfer_option[1] = step_value.ToString();
                                        transfer_option[2] = item_length.ToString();
                                        transfer_option[3] = freq_mhz_data.ToString();
                                        List<string> item = new List<string>();
                                        for (int i = 0; i < item_length; i++)
                                        {
                                            item.Add((BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index)).ToString());
                                            index += 4;
                                        }
                                        switch (sjkim_inst.data_instance.freq_data_ret_count)
                                        {
                                            case 0:
                                                {
                                                    if (main_push_event[(int)Form_enum.INPUT_PWR_PROGRESS_FORM] != null)
                                                    {
                                                        this.main_push_event[(int)Form_enum.INPUT_PWR_PROGRESS_FORM](cmd: "FREQ_INPUT_GET_OPTION", data_arr: transfer_option);
                                                        this.main_push_event[(int)Form_enum.INPUT_PWR_PROGRESS_FORM](cmd: "FREQ_INPUT_GET_INPUT_DET_DATA", data_arr: item.ToArray(), data: freq_mhz_data.ToString());
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (main_push_event[(int)Form_enum.INPUT_PWR_PROGRESS_FORM] != null)
                                                    {
                                                        this.main_push_event[(int)Form_enum.INPUT_PWR_PROGRESS_FORM](cmd: "FREQ_INPUT_DBM_DATA", data_arr: item.ToArray(), data: freq_mhz_data.ToString());
                                                    }
                                                    break;
                                                }
                                            default:
                                                break;
                                        }
                                        sjkim_inst.data_instance.freq_data_ret_count += 1;
                                        sjkim_inst.data_instance.freq_data_ret_count %= 2;
                                    }
                                    break;
                                }
                            case (byte)CmdList_enum.FREQ_INPUT_SET:    // 26
                                {
                                    if (cmd_only == false)
                                    {
                                        int index = 1;
                                        byte file_command = receive_buffer_byte_list[index];
                                        index += 1;
                                        switch (file_command)
                                        {
                                            case (byte)SjkimCmd.FreqCommand_enum.INPUT_FILE_SET:
                                                {
                                                    if (main_push_event[(int)Form_enum.INPUT_PWR_PROGRESS_FORM] != null)
                                                    {
                                                        this.main_push_event[(int)Form_enum.INPUT_PWR_PROGRESS_FORM](cmd: "FREQ_INPUT_SET_RETURN");
                                                    }
                                                    break;
                                                }
                                            case (byte)SjkimCmd.FreqCommand_enum.INPUT_FILE_GET:
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    break;
                                }
                            case (byte)CmdList_enum.NORMAL:            // 27
                                // sjkim_inst.data_instance.normal_sent = false;
                                if (cmd_only == false)
                                {
                                    // index 0: status
                                    // index 1 ~ 4: fault
                                    // index 5 ~ 9: fwd
                                    // index 10 ~ 13: rfl
                                    // index 14 ~ 17: vswr
                                    // index 18 ~ 21: input
                                    // index 22: temp board
                                    // index 23 ~ 26: temp_0
                                    // index 27 ~ 30: temp_1
                                    // index 31 ~ 34: temp_2
                                    // index 35 ~ 38: voltage_0
                                    // index 39 ~ 42: voltage_1
                                    // index 43 ~ 46: freq_mhz
                                    // index 47 ~ 52: rtc
                                    // index 53 ~ 56: ip
                                    // index 57 ~ 58: port
                                    // index 59 ~ 60: dac voltage 12bit -> 16bit
                                    // index 61 ~ 64: fwd detect voltage
                                    // index 65 ~ 68: rfl detect voltage
                                    // index 69 ~ 72: input detect voltage
                                    // index 73 ~ 76: radiate hour
                                    // index 77 ~ 77: radiate minute
                                    // index 78 ~ 78: radiate second
                                    // index 79 ~ 82: standby hour
                                    // index 83 ~ 83: standby minute
                                    // index 84 ~ 84: standby second
                                    // index 85 ~ 88: operate hour
                                    // index 89 ~ 89: operate minute
                                    // index 90 ~ 90: operate second
                                    // index 91 ~ 91: input detect high
                                    // index 92 ~ 95: current_0
                                    // index 96 ~ 99: current_1
                                    // index 100 ~ 100: display all channel
                                    // index 101 ~ 103: display all value

                                    sjkim_inst.data_instance.normal_sent = false;
                                    int index = 1;
                                    // index 0: status
                                    // 1) online check
                                    // 2) alc check
                                    byte status_buff = receive_buffer_byte_list[index];
                                    bool online_standby_buff = false;
                                    index += 1;
                                    // 1) online check
                                    if ((status_buff & (1 << 0)) == 0)  // standby
                                    {
                                        if (button8.InvokeRequired == true)
                                        {
                                            button8.Invoke((MethodInvoker)delegate
                                            {
                                                button8.Text = "STANDBY";
                                            });
                                        }
                                        else
                                        {
                                            button8.Text = "STANDBY";
                                        }
                                        online_standby_buff = false;
                                    }
                                    else  // online
                                    {
                                        if (button8.InvokeRequired == true)
                                        {
                                            button8.Invoke((MethodInvoker)delegate
                                            {
                                                button8.Text = "ONLINE";
                                            });
                                        }
                                        else
                                        {
                                            button8.Text = "ONLINE";
                                        }
                                        online_standby_buff = true;
                                    }
                                    // 2) alc check
                                    if ((status_buff & (1 << 1)) == 0)  // agc
                                    {
                                        if(sjkim_inst.data_instance.alc_agc_state == true)
                                        {
                                            sjkim_inst.data_instance.alc_agc_state = false;
                                        }
                                        if(button73.InvokeRequired == true)
                                        {
                                            button73.Invoke((MethodInvoker)delegate
                                            {
                                                if (button73.Enabled == false)
                                                {
                                                    button73.Enabled = true;
                                                }
                                            });
                                        }
                                        else
                                        {
                                            if (button73.Enabled == false)
                                            {
                                                button73.Enabled = true;
                                            }
                                        }
                                    }
                                    else  // alc
                                    {
                                        if (sjkim_inst.data_instance.alc_agc_state == false)
                                        {
                                            sjkim_inst.data_instance.alc_agc_state = true;
                                        }
                                    }
                                    // index 1 ~ 4: fault
                                    Int32 fault_buff = BitConverter.ToInt32(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    bool over_fwd_fault_buff = false;
                                    bool over_vswr_fault_buff = false;
                                    bool over_input_fault_buff = false;
                                    bool over_temp_fault_buff = false;
                                    bool over_current_fault_buff = false;
                                    bool over_interlock_fault_buff = false;
                                    bool over_voltage_fault_buff = false;
                                    // bool internal_fault = false;  // 미구현
                                    // bool over_fan_fault_buff = false;  // 미구현

                                    if ((fault_buff & (1 << (int)SjkimCmd.Faultmask_enum.FAULT_FWD)) != 0)  // fwd
                                    {
                                        over_fwd_fault_buff = true;
                                    }
                                    if ((fault_buff & (1 << (int)SjkimCmd.Faultmask_enum.FAULT_VSWR)) != 0)  // vswr
                                    {
                                        over_vswr_fault_buff = true;
                                    }
                                    if ((fault_buff & (1 << (int)SjkimCmd.Faultmask_enum.FAULT_INPUT)) != 0)  // input
                                    {
                                        over_input_fault_buff = true;
                                    }
                                    if ((fault_buff & (1 << (int)SjkimCmd.Faultmask_enum.FAULT_TEMPERATURE)) != 0)  // temperature
                                    {
                                        over_temp_fault_buff = true;
                                    }
                                    if ((fault_buff & (1 << (int)SjkimCmd.Faultmask_enum.FAULT_CURRENT)) != 0)  // current
                                    {
                                        over_current_fault_buff = true;
                                    }
                                    if ((fault_buff & (1 << (int)SjkimCmd.Faultmask_enum.FAULT_INTERLOCK)) != 0)  // interlock
                                    {
                                        over_interlock_fault_buff = true;
                                    }
                                    if ((fault_buff & (1 << (int)SjkimCmd.Faultmask_enum.FAULT_VOLTAGE)) != 0)  // voltage
                                    {
                                        over_voltage_fault_buff = true;
                                    }
                                    if ((fault_buff & (1 << (int)SjkimCmd.Faultmask_enum.FAULT_INTERNAL)) != 0)  // interlnal
                                    {
                                        // internal_fault = true;
                                    }

                                    if (fault_buff == 0)  // fault check
                                    {
                                        if (button3.InvokeRequired == true)
                                        {
                                            button3.Invoke((MethodInvoker)delegate
                                            {
                                                button3.Text = "CLEAR";
                                                button3.BackColor = Color.FromArgb(70, 255, 255);
                                            });
                                            dataGridView3.Invoke((MethodInvoker)delegate
                                            {
                                                dataGridView3.Rows[0].Cells[1].Value = "CLEAR";
                                                dataGridView3.Rows[0].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                dataGridView3.Rows[1].Cells[1].Value = "CLEAR";
                                                dataGridView3.Rows[1].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                dataGridView3.Rows[2].Cells[1].Value = "CLEAR";
                                                dataGridView3.Rows[2].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                dataGridView3.Rows[0].Cells[3].Value = "CLEAR";
                                                dataGridView3.Rows[0].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                dataGridView3.Rows[1].Cells[3].Value = "CLEAR";
                                                dataGridView3.Rows[1].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                dataGridView3.Rows[2].Cells[3].Value = "CLEAR";
                                                dataGridView3.Rows[2].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                dataGridView3.Rows[3].Cells[3].Value = "CLEAR";
                                                dataGridView3.Rows[3].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                dataGridView3.Rows[4].Cells[3].Value = "CLEAR";
                                                dataGridView3.Rows[4].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                            });
                                        }
                                        else
                                        {
                                            button3.Text = "CLEAR";
                                            button3.BackColor = Color.FromArgb(70, 255, 255);
                                            dataGridView3.Rows[0].Cells[1].Value = "CLEAR";
                                            dataGridView3.Rows[0].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                            dataGridView3.Rows[1].Cells[1].Value = "CLEAR";
                                            dataGridView3.Rows[1].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                            dataGridView3.Rows[2].Cells[1].Value = "CLEAR";
                                            dataGridView3.Rows[2].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                            dataGridView3.Rows[0].Cells[3].Value = "CLEAR";
                                            dataGridView3.Rows[0].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                            dataGridView3.Rows[1].Cells[3].Value = "CLEAR";
                                            dataGridView3.Rows[1].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                            dataGridView3.Rows[2].Cells[3].Value = "CLEAR";
                                            dataGridView3.Rows[2].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                            dataGridView3.Rows[3].Cells[3].Value = "CLEAR";
                                            dataGridView3.Rows[3].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                            dataGridView3.Rows[4].Cells[3].Value = "CLEAR";
                                            dataGridView3.Rows[4].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);

                                        }
                                    }
                                    else
                                    {
                                        if (button3.InvokeRequired == true)
                                        {
                                            button3.Invoke((MethodInvoker)delegate
                                            {
                                                button3.Text = "FAULT";
                                                button3.BackColor = Color.FromArgb(255, 50, 30);
                                            });

                                            dataGridView3.Invoke((MethodInvoker)delegate
                                            {
                                                if (sjkim_inst.data_instance.fault_occured_before == false)
                                                {
                                                    if (over_fwd_fault_buff == true)
                                                    {
                                                        dataGridView3.Rows[2].Cells[1].Value = "FAULT";
                                                        dataGridView3.Rows[2].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                    }
                                                    else
                                                    {
                                                        dataGridView3.Rows[2].Cells[1].Value = "CLEAR";
                                                        dataGridView3.Rows[2].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                    }
                                                    if (over_vswr_fault_buff == true)
                                                    {
                                                        dataGridView3.Rows[1].Cells[1].Value = "FAULT";
                                                        dataGridView3.Rows[1].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                    }
                                                    else
                                                    {
                                                        dataGridView3.Rows[1].Cells[1].Value = "CLEAR";
                                                        dataGridView3.Rows[1].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                    }
                                                    if (over_input_fault_buff == true)
                                                    {
                                                        dataGridView3.Rows[0].Cells[1].Value = "FAULT";
                                                        dataGridView3.Rows[0].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                    }
                                                    else
                                                    {
                                                        dataGridView3.Rows[0].Cells[1].Value = "CLEAR";
                                                        dataGridView3.Rows[0].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                    }
                                                    if (over_temp_fault_buff == true)
                                                    {
                                                        dataGridView3.Rows[2].Cells[3].Value = "FAULT";
                                                        dataGridView3.Rows[2].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                    }
                                                    else
                                                    {
                                                        dataGridView3.Rows[2].Cells[3].Value = "CLEAR";
                                                        dataGridView3.Rows[2].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                    }
                                                    if (over_current_fault_buff == true)
                                                    {
                                                        dataGridView3.Rows[1].Cells[3].Value = "FAULT";
                                                        dataGridView3.Rows[1].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                    }
                                                    else
                                                    {
                                                        dataGridView3.Rows[1].Cells[3].Value = "CLEAR";
                                                        dataGridView3.Rows[2].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                    }
                                                    if (over_interlock_fault_buff == true)
                                                    {
                                                        dataGridView3.Rows[4].Cells[3].Value = "FAULT";
                                                        dataGridView3.Rows[4].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                    }
                                                    else
                                                    {
                                                        dataGridView3.Rows[4].Cells[3].Value = "CLEAR";
                                                        dataGridView3.Rows[4].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                    }
                                                    if (over_voltage_fault_buff == true)
                                                    {
                                                        dataGridView3.Rows[0].Cells[3].Value = "FAULT";
                                                        dataGridView3.Rows[0].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                    }
                                                    else
                                                    {
                                                        dataGridView3.Rows[0].Cells[3].Value = "CLEAR";
                                                        dataGridView3.Rows[0].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                    }

                                                    dataGridView3.Rows[3].Cells[3].Value = "CLEAR";  // FAN 미구현으로 clear only
                                                    dataGridView3.Rows[3].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                }
                                            });
                                        }
                                        else
                                        {
                                            if (sjkim_inst.data_instance.fault_occured_before == false)
                                            {
                                                button3.Text = "FAULT";
                                                button3.BackColor = Color.FromArgb(255, 50, 30);

                                                if (over_fwd_fault_buff == true)
                                                {
                                                    dataGridView3.Rows[2].Cells[1].Value = "FAULT";
                                                    dataGridView3.Rows[2].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                }
                                                else
                                                {
                                                    dataGridView3.Rows[2].Cells[1].Value = "CLEAR";
                                                    dataGridView3.Rows[2].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                }
                                                if (over_vswr_fault_buff == true)
                                                {
                                                    dataGridView3.Rows[1].Cells[1].Value = "FAULT";
                                                    dataGridView3.Rows[1].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                }
                                                else
                                                {
                                                    dataGridView3.Rows[1].Cells[1].Value = "CLEAR";
                                                    dataGridView3.Rows[1].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                }
                                                if (over_input_fault_buff == true)
                                                {
                                                    dataGridView3.Rows[0].Cells[1].Value = "FAULT";
                                                    dataGridView3.Rows[0].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                }
                                                else
                                                {
                                                    dataGridView3.Rows[0].Cells[1].Value = "CLEAR";
                                                    dataGridView3.Rows[0].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                }
                                                if (over_temp_fault_buff == true)
                                                {
                                                    dataGridView3.Rows[2].Cells[3].Value = "FAULT";
                                                    dataGridView3.Rows[2].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                }
                                                else
                                                {
                                                    dataGridView3.Rows[2].Cells[3].Value = "CLEAR";
                                                    dataGridView3.Rows[2].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                }
                                                if (over_current_fault_buff == true)
                                                {
                                                    dataGridView3.Rows[1].Cells[3].Value = "FAULT";
                                                    dataGridView3.Rows[1].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                }
                                                else
                                                {
                                                    dataGridView3.Rows[1].Cells[3].Value = "CLEAR";
                                                    dataGridView3.Rows[1].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                }
                                                if (over_interlock_fault_buff == true)
                                                {
                                                    dataGridView3.Rows[4].Cells[3].Value = "FAULT";
                                                    dataGridView3.Rows[4].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                }
                                                else
                                                {
                                                    dataGridView3.Rows[4].Cells[3].Value = "CLEAR";
                                                    dataGridView3.Rows[4].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                }
                                                if (over_voltage_fault_buff == true)
                                                {
                                                    dataGridView3.Rows[0].Cells[3].Value = "FAULT";
                                                    dataGridView3.Rows[0].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(255, 70, 30);
                                                }
                                                else
                                                {
                                                    dataGridView3.Rows[0].Cells[3].Value = "CLEAR";
                                                    dataGridView3.Rows[0].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                                }

                                                dataGridView3.Rows[3].Cells[3].Value = "CLEAR";  // FAN 미구현으로 clear only
                                                dataGridView3.Rows[3].Cells[3].Style.BackColor = System.Drawing.Color.FromArgb(70, 255, 255);
                                            }
                                        }
                                    }
                                    // index 5 ~ 9: fwd
                                    float fwd_buff = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    int count_pwr_max = 3;
                                    index += 4;
                                    if (online_standby_buff == false)
                                    {
                                        sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER] = null;
                                        if (button7.InvokeRequired == true)
                                        {
                                            button7.Invoke((MethodInvoker)delegate
                                            {
                                                button7.Text = "";
                                            });
                                        }
                                        else
                                        {
                                            button7.Text = "";
                                        }
                                    }
                                    else
                                    {
                                        if(sjkim_inst.data_instance.input_detect_high == true)
                                        {
                                            sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER] = fwd_buff.ToString();
                                            if (sjkim_inst.data_instance.pwr_display_delay_count_0 <= count_pwr_max)
                                            {
                                                sjkim_inst.data_instance.pwr_display_delay_count_0 += 1;
                                            }
                                            else
                                            {
                                                sjkim_inst.data_instance.pwr_display_delay_count_0 = 0;
                                                if (button12.InvokeRequired == true)
                                                {
                                                    button12.Invoke((MethodInvoker)delegate
                                                    {
                                                        if (button12.Text == "《W》")
                                                        {
                                                            if (sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER] != null)
                                                            {
                                                                fwd_buff = (float)Math.Pow(10.0F, (float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER]) / 10) - 3);

                                                                button7.Invoke((MethodInvoker)delegate
                                                                {
                                                                    button7.Text = String.Format("{0:0.00}", fwd_buff);
                                                                });
                                                            }
                                                        }
                                                        else
                                                        {
                                                            button7.Invoke((MethodInvoker)delegate
                                                            {
                                                                button7.Text = String.Format("{0:0.00}", float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER]));
                                                            });
                                                        }
                                                    });
                                                }
                                                else
                                                {
                                                    if (button12.Text == "《W》")
                                                    {
                                                        if (sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER] != null)
                                                        {
                                                            fwd_buff = (float)Math.Pow(10.0F, (float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER]) / 10) - 3);

                                                            button7.Text = String.Format("{0:0.00}", fwd_buff);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        button7.Text = String.Format("{0:0.00}", float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER]));
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER] = null;
                                            if (button7.InvokeRequired == true)
                                            {
                                                button7.Invoke((MethodInvoker)delegate
                                                {
                                                    button7.Text = "";
                                                });
                                            }
                                            else
                                            {
                                                button7.Text = "";
                                            }
                                        }
                                    }
                                    // index 10 ~ 13: rfl
                                    float rfl_buff = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    if (online_standby_buff == false)
                                    {
                                        sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER] = null;
                                        if (button6.InvokeRequired == true)
                                        {
                                            button6.Invoke((MethodInvoker)delegate
                                            {
                                                button6.Text = "";
                                            });
                                        }
                                        else
                                        {
                                            button6.Text = "";
                                        }
                                    }
                                    else
                                    {
                                        if (sjkim_inst.data_instance.input_detect_high == true)
                                        {
                                            sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.RFL_POWER] = rfl_buff.ToString();
                                            if (sjkim_inst.data_instance.pwr_display_delay_count_1 <= count_pwr_max)
                                            {
                                                sjkim_inst.data_instance.pwr_display_delay_count_1 += 1;
                                            }
                                            else
                                            {
                                                sjkim_inst.data_instance.pwr_display_delay_count_1 = 0;

                                                if (button5.InvokeRequired == true)
                                                {
                                                    button5.Invoke((MethodInvoker)delegate
                                                    {
                                                        if (button5.Text == "《W》")
                                                        {
                                                            if (sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.RFL_POWER] != null)
                                                            {
                                                                rfl_buff = (float)Math.Pow(10.0F, (float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.RFL_POWER]) / 10) - 3);

                                                                button6.Invoke((MethodInvoker)delegate
                                                                {
                                                                    button6.Text = String.Format("{0:0.##}", rfl_buff);
                                                                });
                                                            }
                                                        }
                                                        else
                                                        {
                                                            button6.Invoke((MethodInvoker)delegate
                                                            {
                                                                button6.Text = String.Format("{0:0.##}", float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.RFL_POWER]));
                                                            });
                                                        }
                                                    });
                                                }
                                                else
                                                {
                                                    if (button5.Text == "《W》")
                                                    {
                                                        if (sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.RFL_POWER] != null)
                                                        {
                                                            rfl_buff = (float)Math.Pow(10.0F, (float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.RFL_POWER]) / 10) - 3);
                                                            button6.Text = String.Format("{0:0.##}", rfl_buff);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        button6.Text = String.Format("{0:0.##}", float.Parse(sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.RFL_POWER]));
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER] = null;
                                            if (button6.InvokeRequired == true)
                                            {
                                                button6.Invoke((MethodInvoker)delegate
                                                {
                                                    button6.Text = "";
                                                });
                                            }
                                            else
                                            {
                                                button6.Text = "";
                                            }
                                        }
                                    }
                                    // index 14 ~ 17: vswr
                                    float vswr_buff = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);

                                    if (online_standby_buff == false)
                                    {
                                        sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.VSWR_STATUS_POWER] = null;
                                        if (button11.InvokeRequired == true)
                                        {
                                            button11.Invoke((MethodInvoker)delegate
                                            {
                                                button11.Text = "";
                                            });
                                        }
                                        else
                                        {
                                            button11.Text = "";
                                        }
                                    }
                                    else
                                    {
                                        if (sjkim_inst.data_instance.input_detect_high == true)
                                        {

                                            if (sjkim_inst.data_instance.pwr_display_delay_count_2 <= count_pwr_max)
                                            {
                                                sjkim_inst.data_instance.pwr_display_delay_count_2 += 1;
                                            }
                                            else
                                            {
                                                sjkim_inst.data_instance.pwr_display_delay_count_2 = 0;
                                                if (button11.InvokeRequired == true)
                                                {
                                                    button11.Invoke((MethodInvoker)delegate
                                                    {
                                                        button11.Text = String.Format("{0:0.##}", vswr_buff);
                                                    });
                                                }
                                                else
                                                {
                                                    button11.Text = String.Format("{0:0.##}", vswr_buff);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sjkim_inst.data_instance.power_status_data[(int)StatusPower_enum.FWD_POWER] = null;
                                            if (button7.InvokeRequired == true)
                                            {
                                                button7.Invoke((MethodInvoker)delegate
                                                {
                                                    button11.Text = "";
                                                });
                                            }
                                            else
                                            {
                                                button11.Text = "";
                                            }
                                        }
                                    }
                                    index += 4;
                                    // index 18 ~ 21: input
                                    float input_buff = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    // index 22: temp board
                                    byte temp_board_buff = receive_buffer_byte_list[index];
                                    index += 1;
                                    if (dataGridView1.InvokeRequired == true)
                                    {
                                        dataGridView1.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView1.Rows[2].Cells[1].Value = temp_board_buff + "︒C";
                                        });
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[2].Cells[1].Value = temp_board_buff + "︒C";
                                    }
                                    // index 23 ~ 26: temp_0
                                    float temp_0_buff = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    if (dataGridView1.InvokeRequired == true)
                                    {
                                        dataGridView1.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView1.Rows[4].Cells[1].Value = String.Format("{0:0}", temp_0_buff) + "︒C";
                                        });
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[4].Cells[1].Value = String.Format("{0:0}", temp_0_buff) + "︒C";
                                    }
                                    // index 27 ~ 30: temp_1
                                    float temp_1_buff = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    if (dataGridView1.InvokeRequired == true)
                                    {
                                        dataGridView1.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView1.Rows[3].Cells[1].Value = String.Format("{0:0}", temp_1_buff) + "︒C";
                                        });
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[3].Cells[1].Value = String.Format("{0:0}", temp_1_buff) + "︒C";
                                    }
                                    // index 31 ~ 34: temp_2
                                    float temp_2_buff = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    // index 35 ~ 38: voltage_0
                                    float voltage_0_buff = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    if (dataGridView1.InvokeRequired == true)
                                    {
                                        dataGridView1.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView1.Rows[0].Cells[1].Value = String.Format("{0:0.##}", voltage_0_buff) + " V";
                                        });
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[0].Cells[1].Value = String.Format("{0:0.##}", voltage_0_buff) + " V"; 
                                    }
                                    // index 39 ~ 42: voltage_1
                                    float voltage_1_buff = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    if (dataGridView1.InvokeRequired == true)
                                    {
                                        dataGridView1.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView1.Rows[1].Cells[1].Value = String.Format("{0:0.##}", voltage_1_buff) + " V";
                                        });
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[1].Cells[1].Value = String.Format("{0:0.##}", voltage_1_buff) + " V";
                                    }
                                    // index 43 ~ 46: freq_mhz
                                    Int32 freq_mhz_buff = BitConverter.ToInt32(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    if (dataGridView2.InvokeRequired == true)
                                    {
                                        dataGridView2.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView2.Rows[0].Cells[1].Value = freq_mhz_buff + " MHz";
                                        });
                                    }
                                    else
                                    {
                                        dataGridView2.Rows[0].Cells[1].Value = freq_mhz_buff + " MHz";
                                    }

                                    // index 47 ~ 52: rtc

                                    string time_transfer = string.Empty;
                                    byte time_buff = receive_buffer_byte_list[index];  // year
                                    time_transfer += "20";
                                    if (time_buff / 16 != 0)
                                    {
                                        time_transfer += (time_buff / 16).ToString();
                                    }
                                    else
                                    {
                                        time_transfer += "0";
                                    }
                                    time_transfer += (time_buff % 16).ToString() + "-";
                                    index += 1;
                                    time_buff = receive_buffer_byte_list[index];  // month
                                    if (time_buff / 16 != 0)
                                    {
                                        time_transfer += (time_buff / 16).ToString();
                                    }
                                    time_transfer += (time_buff % 16).ToString() + "-";
                                    index += 1;
                                    time_buff = receive_buffer_byte_list[index];  // date
                                    if (time_buff / 16 != 0)
                                    {
                                        time_transfer += (time_buff / 16).ToString();
                                    }
                                    time_transfer += (time_buff % 16).ToString() + "-";
                                    index += 1;
                                    time_buff = receive_buffer_byte_list[index];  // hour
                                    if (time_buff / 16 != 0)
                                    {
                                        time_transfer += (time_buff / 16).ToString();
                                    }
                                    time_transfer += (time_buff % 16).ToString() + "-";
                                    index += 1;
                                    time_buff = receive_buffer_byte_list[index];  // minute
                                    if (time_buff / 16 != 0)
                                    {
                                        time_transfer += (time_buff / 16).ToString();
                                    }
                                    time_transfer += (time_buff % 16).ToString() + "-";
                                    index += 1;
                                    time_buff = receive_buffer_byte_list[index];  // second
                                    if (time_buff / 16 != 0)
                                    {
                                        time_transfer += (time_buff / 16).ToString();
                                    }
                                    time_transfer += (time_buff % 16).ToString();
                                    index += 1;

                                    if (textBox6.InvokeRequired == true)
                                    {
                                        textBox6.Invoke((MethodInvoker)delegate
                                        {
                                            textBox6.Text = time_transfer;
                                        });
                                    }
                                    else
                                    {
                                        textBox6.Text = time_transfer;
                                    }

                                    // index 53 ~ 56: ip
                                    byte[] ip_buff = new byte[4];
                                    ip_buff[0] = receive_buffer_byte_list[index];
                                    index += 1;
                                    ip_buff[1] = receive_buffer_byte_list[index];
                                    index += 1;
                                    ip_buff[2] = receive_buffer_byte_list[index];
                                    index += 1;
                                    ip_buff[3] = receive_buffer_byte_list[index];
                                    index += 1;
                                    sjkim_inst.data_instance.ip_address[0] = ip_buff[0].ToString();
                                    sjkim_inst.data_instance.ip_address[1] = ip_buff[1].ToString();
                                    sjkim_inst.data_instance.ip_address[2] = ip_buff[2].ToString();
                                    sjkim_inst.data_instance.ip_address[3] = ip_buff[3].ToString();
                                    string ip_string = ip_buff[0].ToString() + "."
                                                       + ip_buff[1].ToString() + "."
                                                       + ip_buff[2].ToString() + "."
                                                       + ip_buff[3].ToString();
                                    if (dataGridView2.InvokeRequired == true)
                                    {
                                        dataGridView2.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView2.Rows[1].Cells[1].Value = ip_string;
                                        });
                                    }
                                    else
                                    {
                                        dataGridView2.Rows[1].Cells[1].Value = ip_string;
                                    }
                                    // index 57 ~ 58: port
                                    short port_buff = 0;
                                    port_buff = BitConverter.ToInt16(receive_buffer_byte_list.ToArray(), index);
                                    sjkim_inst.data_instance.port = port_buff.ToString();
                                    index += 2;
                                    if (dataGridView2.InvokeRequired == true)
                                    {
                                        dataGridView2.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView2.Rows[2].Cells[1].Value = port_buff;
                                        });
                                    }
                                    else
                                    {
                                        dataGridView2.Rows[2].Cells[1].Value = port_buff;
                                    }
                                    // index 59 ~ 60: dac voltage 12bit -> 16bit
                                    // dac_adc_voltage_0;
                                    sjkim_inst.data_instance.dac_adc_voltage_0 = BitConverter.ToUInt16(receive_buffer_byte_list.ToArray(), index);
                                    index += 2;
                                    // index 61 ~ 64: fwd detect voltage
                                    // fwd_adc_voltage_0;
                                    sjkim_inst.data_instance.fwd_adc_voltage_0 = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    // index 65 ~ 68: rfl detect voltage
                                    // rfl_adc_voltage_0;
                                    sjkim_inst.data_instance.rfl_adc_voltage_0 = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    // index 69 ~ 72: input detect voltage
                                    // input_adc_voltage_0;
                                    sjkim_inst.data_instance.input_adc_voltage_0 = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;

                                    // index 73 ~ 76: radiate hour
                                    int rad_h = BitConverter.ToInt32(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    // index 77 ~ 77: radiate minute
                                    int rad_m = receive_buffer_byte_list[index];
                                    index += 1;
                                    // index 78 ~ 78: radiate second
                                    int rad_s = receive_buffer_byte_list[index];
                                    index += 1;
                                    if (dataGridView2.InvokeRequired == true)
                                    {
                                        dataGridView2.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView2.Rows[1].Cells[3].Value = rad_h.ToString("D2") + " : " + rad_m.ToString("D2");
                                        });
                                    }
                                    else
                                    {
                                        dataGridView2.Rows[1].Cells[3].Value = rad_h.ToString("D2") + " : " + rad_m.ToString("D2");
                                    }
                                    // index 79 ~ 82: standby hour
                                    int st_h = BitConverter.ToInt32(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    // index 83 ~ 83: standby minute
                                    int st_m = receive_buffer_byte_list[index];
                                    index += 1;
                                    // index 84 ~ 84: standby second
                                    int st_s = receive_buffer_byte_list[index];
                                    index += 1;
                                    if (dataGridView2.InvokeRequired == true)
                                    {
                                        dataGridView2.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView2.Rows[2].Cells[3].Value = st_h.ToString("D2") + " : " + st_m.ToString("D2");
                                        });
                                    }
                                    else
                                    {
                                        dataGridView2.Rows[2].Cells[3].Value = st_h.ToString("D2") + " : " + st_m.ToString("D2");
                                    }

                                    // index 85 ~ 88: operate hour
                                    int op_h = BitConverter.ToInt32(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    // index 89 ~ 89: operate minute
                                    int op_m = receive_buffer_byte_list[index];
                                    index += 1;
                                    // index 90 ~ 90: operate second
                                    int op_s = receive_buffer_byte_list[index];
                                    index += 1;
                                    if (dataGridView2.InvokeRequired == true)
                                    {
                                        dataGridView2.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView2.Rows[0].Cells[3].Value = op_h.ToString("D2") + " : " + op_m.ToString("D2");
                                        });
                                    }
                                    else
                                    {
                                        dataGridView2.Rows[0].Cells[3].Value = op_h.ToString("D2") + " : " + op_m.ToString("D2");
                                    }
                                    // index 91 ~ 91: input detect high
                                    if (receive_buffer_byte_list[index] == 1)
                                    {
                                        sjkim_inst.data_instance.input_detect_high = true;
                                    }
                                    else
                                    {
                                        sjkim_inst.data_instance.input_detect_high = false;
                                    }
                                    index += 1;

                                    // index 92 ~ 95: current_0
                                    if(dataGridView1.InvokeRequired == true)
                                    {
                                        dataGridView1.Invoke((MethodInvoker)delegate
                                        {
                                            dataGridView1.Rows[0].Cells[3].Value = String.Format("{0:0.##}", BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index)) + " A";
                                        });
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[0].Cells[3].Value = String.Format("{0:0.##}", BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index)) + " A";
                                    }
                                    index += 4;
                                    // index 96 ~ 99: current_1
                                    // dataGridView1.Rows[0].Cells[4].Value = String.Format("{0:0.##}", BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index));
                                    index += 4;
                                    // index 100 ~ 100: display all channel
                                    byte display_all_channel_buff = receive_buffer_byte_list[index];
                                    index += 1;
                                    // index 101 ~ 103: display all value
                                    float display_all_channel_data = BitConverter.ToSingle(receive_buffer_byte_list.ToArray(), index);
                                    index += 4;
                                    string[] transfer = new string[2];
                                    transfer[0] = display_all_channel_buff.ToString();
                                    transfer[1] = display_all_channel_data.ToString();
                                    if(sjkim_inst.data_instance.display_all_toggle_pushed == true)
                                    {
                                        if (this.main_push_event[(int)Form_enum.SET_PARAM_FORM] != null)
                                        {
                                            main_push_event[(int)Form_enum.SET_PARAM_FORM](cmd: "SET_DISPLAY_ALL_DATA", data_arr: transfer);
                                        }
                                    }
                                }
                                else
                                {
                                    if(sjkim_inst.data_instance.ethernet_opened_first == true)
                                    {
                                        sjkim_inst.data_instance.ethernet_opened_first = false;
                                        normal_data_push();
                                    }
                                }
                                break;
                            case (byte)CmdList_enum.SYS_ID_GET:        // 28
                                {
                                    if (cmd_only == false)
                                    {
                                        int count_comma = 0;
                                        int start_index = 1;
                                        int last_comma_index = 0;
                                        int last_data_index = 1;
                                        int content_max = 7; // 0. idn, 1. serial, 2. model, 3. manufacture, 4. firmware, 5. software 6. description

                                        for (int i = start_index; i < receive_buffer_byte_list.Count - 1; i++)
                                        {
                                            if(receive_buffer_byte_list[i] == 44)
                                            {
                                                receive_buffer_byte_list[i] = 0;
                                                List<byte> parse_buff = new List<byte>();
                                                for (int j = last_data_index; j < i; j++)
                                                {
                                                    parse_buff.Add(receive_buffer_byte_list[j]);
                                                }
                                                last_data_index = i + 1;
                                                switch (count_comma)
                                                {
                                                    case 0:  // idn
                                                        {
                                                            sjkim_inst.data_instance.monitoring_idn = Encoding.ASCII.GetString(parse_buff.ToArray());
                                                            break;
                                                        }
                                                    case 1:  // serial
                                                        {
                                                            sjkim_inst.data_instance.monitoring_serial_num = Encoding.ASCII.GetString(parse_buff.ToArray());
                                                            break;
                                                        }
                                                    case 2:  // model
                                                        {
                                                            sjkim_inst.data_instance.monitoring_model = Encoding.ASCII.GetString(parse_buff.ToArray());
                                                            break;
                                                        }
                                                    case 3:  // manufacutre
                                                        {
                                                            sjkim_inst.data_instance.monitoring_manufacture = Encoding.ASCII.GetString(parse_buff.ToArray());
                                                            break;
                                                        }
                                                    case 4:  // firmware
                                                        {
                                                            sjkim_inst.data_instance.monitoring_firmware = Encoding.ASCII.GetString(parse_buff.ToArray());
                                                            break;
                                                        }
                                                    case 5:  // software
                                                        {
                                                            sjkim_inst.data_instance.monitoring_software = Encoding.ASCII.GetString(parse_buff.ToArray());
                                                            break;
                                                        }
                                                    default: 
                                                        break;
                                                }
                                                last_comma_index = i;
                                                count_comma += 1;
                                            }
                                        }
                                        if(count_comma == content_max - 1)
                                        {
                                            List<byte> parse_buff = new List<byte>();
                                            for (int j = last_data_index; j < receive_buffer_byte_list.Count - 1; j++)  // description
                                            {
                                                parse_buff.Add(receive_buffer_byte_list[j]);
                                            }
                                            sjkim_inst.data_instance.monitoring_description = Encoding.ASCII.GetString(parse_buff.ToArray());
                                            
                                        }

                                        sjkim_inst.data_instance.monitoring_idn = sjkim_inst.data_instance.monitoring_idn.Substring(0, sjkim_inst.data_instance.monitoring_idn.Length - 1);
                                        sjkim_inst.data_instance.monitoring_idn = " " + sjkim_inst.data_instance.monitoring_idn;
                                        sjkim_inst.data_instance.monitoring_serial_num = sjkim_inst.data_instance.monitoring_serial_num.Substring(0, sjkim_inst.data_instance.monitoring_serial_num.Length - 1);
                                        sjkim_inst.data_instance.monitoring_serial_num = " " + sjkim_inst.data_instance.monitoring_serial_num;
                                        sjkim_inst.data_instance.monitoring_firmware = sjkim_inst.data_instance.monitoring_firmware.Substring(0, sjkim_inst.data_instance.monitoring_firmware.Length - 1);
                                        sjkim_inst.data_instance.monitoring_firmware = " " + sjkim_inst.data_instance.monitoring_firmware;
                                        sjkim_inst.data_instance.monitoring_software = sjkim_inst.data_instance.monitoring_software.Substring(0, sjkim_inst.data_instance.monitoring_software.Length - 1);
                                        sjkim_inst.data_instance.monitoring_software = " " + sjkim_inst.data_instance.monitoring_software;
                                        sjkim_inst.data_instance.monitoring_description = sjkim_inst.data_instance.monitoring_description.Substring(0, sjkim_inst.data_instance.monitoring_description.Length - 1);
                                        sjkim_inst.data_instance.monitoring_description = " " + sjkim_inst.data_instance.monitoring_description;
                                        sjkim_inst.data_instance.monitoring_manufacture = sjkim_inst.data_instance.monitoring_manufacture.Substring(0, sjkim_inst.data_instance.monitoring_manufacture.Length - 1);
                                        sjkim_inst.data_instance.monitoring_manufacture = " " + sjkim_inst.data_instance.monitoring_manufacture;

                                        if (this.InvokeRequired == true)
                                        {
                                            this.Invoke((MethodInvoker)delegate
                                            {
                                                this.Text = sjkim_inst.data_instance.monitoring_software + " " + sjkim_inst.data_instance.monitoring_model + " " + sjkim_inst.data_instance.monitoring_description;
                                                label9.Text = sjkim_inst.data_instance.monitoring_description;
                                                label13.Text = sjkim_inst.data_instance.monitoring_model;
                                                label1.Text = sjkim_inst.data_instance.monitoring_serial_num;
                                            });
                                        }
                                        else
                                        {
                                            this.Text = sjkim_inst.data_instance.monitoring_software + " " + sjkim_inst.data_instance.monitoring_model + " " + sjkim_inst.data_instance.monitoring_description;
                                            label9.Text = sjkim_inst.data_instance.monitoring_description;
                                            label13.Text = sjkim_inst.data_instance.monitoring_model;
                                            label1.Text = sjkim_inst.data_instance.monitoring_serial_num;

                                            Point location_buff_mid = new Point(label9.Location.X + (label9.Width / 2), label9.Location.Y);
                                            Point dest_loc_buff_mid = new Point(panel4.Location.X + (panel4.Width / 2), panel4.Location.Y);
                                            int gap = dest_loc_buff_mid.X - location_buff_mid.X;
                                            label9.Location = new Point(label9.Location.X + gap, label9.Location.Y);

                                            location_buff_mid = new Point(label13.Location.X + (label13.Width / 2), label13.Location.Y);
                                            dest_loc_buff_mid = new Point(panel6.Location.X + (panel6.Width / 2), panel6.Location.Y);
                                            gap = dest_loc_buff_mid.X - location_buff_mid.X;
                                            label13.Location = new Point(label13.Location.X + gap, label13.Location.Y);

                                            location_buff_mid = new Point(label1.Location.X + (label1.Width / 2), label1.Location.Y);
                                            dest_loc_buff_mid = new Point(panel7.Location.X + (panel7.Width / 2), panel7.Location.Y);
                                            gap = dest_loc_buff_mid.X - location_buff_mid.X;
                                            label1.Location = new Point(label1.Location.X + gap, label1.Location.Y);
                                        }
                                    }
                                }
                                break;
                            case (byte)CmdList_enum.FAULT_LOG:         // 29
                                break;
                            case (byte)CmdList_enum.LENGTH:            // receive procedure
                                break;
                            default:
                                // MessageBox.Show("unknown cmd");
                                break;
                        }
                        if(cmd_only == true)
                        {
                            sjkim_inst.data_instance.receive_cmd_state = CmdList_enum.LENGTH;
                        }
                        else
                        {
                            sjkim_inst.data_instance.receive_cmd_state = (CmdList_enum)receive_buffer_byte_list[0];
                        }
                    }
                    else
                    {
                        MessageBox.Show("receive buffer is empty");
                    }
                }
                else
                {
                    if(sjkim_inst.data_instance.checksum_error_off == true)
                    {
                        return;
                    }
                    MessageBox.Show("checksum error");
                    // sjkim_inst.data_instance.receive_cmd_state = CmdList_enum.LENGTH;
                    /*
                    if(sjkim_inst.data_instance.normal_sent == true)
                    {
                        sjkim_inst.data_instance.normal_sent = false;
                    }
                    */
                }
            }
            catch
            {
                timer1.Stop();
                MessageBox.Show("Error occured\r\nplease restart connection");
                // MessageBox.Show(ex.ToString());
            }
        }

        void ParseStringToTransBuff(CmdList_enum cmd = CmdList_enum.LENGTH, Communication_enum comm_state = Communication_enum.NONE, byte transmit_state = 0)
        {
            try
            {
                byte cmd_buff = (byte)cmd;
                List<byte> transmit_buffer_byte_list = new List<byte>();
                byte freq_command;

                switch (cmd_buff)
                {
                    case (byte)CmdList_enum.ONLINE:            // 0
                        break;
                    case (byte)CmdList_enum.STANDBY:           // 1
                        break;
                    case (byte)CmdList_enum.ALC_ON:            // 2
                        break;
                    case (byte)CmdList_enum.ALC_OFF:           // 3
                        break;
                    case (byte)CmdList_enum.ALC_SET:           // 4
                        sjkim_inst.data_instance.FloatDataSet(list: transmit_buffer_byte_list, data: sjkim_inst.data_instance.alc_dBm_value);
                        break;
                    case (byte)CmdList_enum.AGC_SET:           // 5
                        sjkim_inst.data_instance.FloatDataSet(list: transmit_buffer_byte_list, data: sjkim_inst.data_instance.agc_dB_value);
                        break;
                    case (byte)CmdList_enum.ALC_READ:          // 6
                        break;
                    case (byte)CmdList_enum.AGC_READ:          // 7
                        break;
                    case (byte)CmdList_enum.SET_TIME:          // 8
                        byte[] data = Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                        transmit_buffer_byte_list.AddRange(data);
                        break;
                    case (byte)CmdList_enum.FREQ_SET:          // 9
                        sjkim_inst.data_instance.IntDataSet(list: transmit_buffer_byte_list, data: Int32.Parse(sjkim_inst.data_instance.freq_data));
                        break;
                    case (byte)CmdList_enum.SERIAL_START:      // 10
                        break;
                    case (byte)CmdList_enum.USB_START:         // 11
                        break;
                    case (byte)CmdList_enum.ETHERNETSTART:     // 12
                        break;
                    case (byte)CmdList_enum.SERIAL_STOP:       // 13
                        break;
                    case (byte)CmdList_enum.USB_STOP:          // 14
                        break;
                    case (byte)CmdList_enum.ETHERNET_STOP:     // 15
                        break;
                    case (byte)CmdList_enum.IP_SET:            // 16
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                int ip_buffer = Int32.Parse(sjkim_inst.data_instance.ip_address[i]);
                                sjkim_inst.data_instance.IntDataSet(list: transmit_buffer_byte_list, data: ip_buffer);
                            }

                            int buffer = Int32.Parse(sjkim_inst.data_instance.port);
                            sjkim_inst.data_instance.IntDataSet(list: transmit_buffer_byte_list, data: buffer);
                            break;
                        }
                    case (byte)CmdList_enum.IP_GET:            // 17
                        break;
                    case (byte)CmdList_enum.LOG_LOAD_100:      // 18
                        {
                            break;
                        }
                    case (byte)CmdList_enum.LOG_LOAD_1000:     // 19
                        {
                            break;
                        }
                    case (byte)CmdList_enum.LOG_READ:     // 20
                        {
                            break;
                        }
                    case (byte)CmdList_enum.PARA_SET_0:        // 21
                        for (int i = 0; i < sjkim_inst.data_instance.para_adc_data.Length; i++)
                        {
                            sjkim_inst.data_instance.FloatDataSet(list: transmit_buffer_byte_list, data: float.Parse(sjkim_inst.data_instance.para_adc_data[i]));
                        }
                        for (int i = 0; i < sjkim_inst.data_instance.threshold_data.Length; i++)
                        {
                            sjkim_inst.data_instance.FloatDataSet(list: transmit_buffer_byte_list, data: float.Parse(sjkim_inst.data_instance.threshold_data[i]));
                        }
                        break;
                    case (byte)CmdList_enum.PARA_GET_0:        // 22
                        break;
                    case (byte)CmdList_enum.PARA_GET_1:        // 23
                        break;
                    case (byte)CmdList_enum.FREQ_PWR_GET:      // 24
                        break;
                    case (byte)CmdList_enum.FREQ_PWR_SET:      // 25
                        {
                            // data_arr[0] = max data;
                            // data_arr[1] = coupling;
                            // data_arr[2] = freq data arr count;
                            // data_arr[3] = first freq data item length;
                            // data_arr[4] = first frequency value;
                            // data_arr[5] = first freq data atten value
                            // ...
                            // data_arr[5 + Int32.Parse(data_arr[1])] = first freq data fwd adc value
                            // ...
                            // data_arr[5 + (Int32.Parse(data_arr[1]) * 2)] = first freq data fwd dbm value

                            int file_transfer_index = 0;

                            freq_command = byte.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                            file_transfer_index += 1;
                            transmit_buffer_byte_list.Add(freq_command);

                            switch(freq_command)
                            {
                                case (byte)FreqCommand_enum.FWD_DAC_SET:
                                    {
                                        sjkim_inst.data_instance.FloatDataSet(list: transmit_buffer_byte_list, data: Single.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]));
                                        file_transfer_index += 1;
                                        break;
                                    }
                                case (byte)FreqCommand_enum.FWD_FILE_SET:
                                    {
                                        float max_data = float.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                                        file_transfer_index += 1;
                                        sjkim_inst.data_instance.FloatDataSet(list: transmit_buffer_byte_list, data: max_data);

                                        float coupling = float.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                                        file_transfer_index += 1;
                                        sjkim_inst.data_instance.FloatDataSet(list: transmit_buffer_byte_list, data: coupling);

                                        int freq_data_arr_count = Int32.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                                        file_transfer_index += 1;
                                        sjkim_inst.data_instance.IntDataSet(list: transmit_buffer_byte_list, data: freq_data_arr_count);

                                        int item_length = Int32.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                                        file_transfer_index += 1;
                                        sjkim_inst.data_instance.IntDataSet(list: transmit_buffer_byte_list, data: item_length);

                                        int freq_value = Int32.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                                        file_transfer_index += 1;
                                        sjkim_inst.data_instance.IntDataSet(list: transmit_buffer_byte_list, data: freq_value);

                                        byte send_state = byte.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                                        file_transfer_index += 1;
                                        transmit_buffer_byte_list.Add(send_state);

                                        for (int j = 0; j < item_length; j++)
                                        {
                                            sjkim_inst.data_instance.FloatDataSet(list: transmit_buffer_byte_list, data: float.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]));
                                            file_transfer_index += 1;
                                        }

                                        break;
                                    }
                                case (byte)FreqCommand_enum.FWD_FILE_GET:
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                    case (byte)CmdList_enum.FREQ_INPUT_GET:    // 26
                        break;
                    case (byte)CmdList_enum.FREQ_INPUT_SET:    // 27
                        {
                            int file_transfer_index = 0;

                            freq_command = byte.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                            file_transfer_index += 1;
                            transmit_buffer_byte_list.Add(freq_command);
                            switch(freq_command)
                            {
                                case (byte)FreqCommand_enum.INPUT_FILE_SET:
                                    {
                                        float step_value = float.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                                        file_transfer_index += 1;
                                        sjkim_inst.data_instance.FloatDataSet(list: transmit_buffer_byte_list, data: step_value);

                                        int freq_data_arr_count = Int32.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                                        file_transfer_index += 1;
                                        sjkim_inst.data_instance.IntDataSet(list: transmit_buffer_byte_list, data: freq_data_arr_count);

                                        int item_length = Int32.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                                        file_transfer_index += 1;
                                        sjkim_inst.data_instance.IntDataSet(list: transmit_buffer_byte_list, data: item_length);

                                        int freq_value = Int32.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                                        file_transfer_index += 1;
                                        sjkim_inst.data_instance.IntDataSet(list: transmit_buffer_byte_list, data: freq_value);

                                        byte send_state = byte.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]);
                                        file_transfer_index += 1;
                                        transmit_buffer_byte_list.Add(send_state);

                                        for (int j = 0; j < item_length; j++)
                                        {
                                            sjkim_inst.data_instance.FloatDataSet(list: transmit_buffer_byte_list, data: float.Parse(sjkim_inst.data_instance.file_transfer[file_transfer_index]));
                                            file_transfer_index += 1;
                                        }
                                        break;
                                    }
                                case (byte)FreqCommand_enum.INPUT_FILE_GET:
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                    case (byte)CmdList_enum.NORMAL:            // 28
                        break;
                    case (byte)CmdList_enum.SYS_ID_GET:        // 29
                        break;
                    case (byte)CmdList_enum.FAULT_LOG:         // 30
                        break;
                    case (byte)CmdList_enum.LENGTH:            // receive procedure
                        break;
                    default:
                        MessageBox.Show("unknown cmd");
                        break;
                }

                byte checksum = cmd_buff;
                for (int i = 0; i < transmit_buffer_byte_list.Count; i++)
                {
                    checksum ^= (byte)transmit_buffer_byte_list[i];
                }
                transmit_buffer_byte_list.Insert(0, (byte)'*');
                transmit_buffer_byte_list.Insert(1, (byte)'0');
                transmit_buffer_byte_list.Insert(2, (byte)cmd);

                for (int i = 2; i < transmit_buffer_byte_list.Count; i++)
                {
                    if (transmit_buffer_byte_list[i] == (byte)'*')
                    {
                        transmit_buffer_byte_list.Insert(i + 1, (byte)'2');
                    }
                    else if (transmit_buffer_byte_list[i] == '\r')
                    {
                        transmit_buffer_byte_list[i] = (byte)'*';
                        transmit_buffer_byte_list.Insert(i + 1, (byte)'3');
                    }
                    else if (transmit_buffer_byte_list[i] == '\n')
                    {
                        transmit_buffer_byte_list[i] = (byte)'*';
                        transmit_buffer_byte_list.Insert(i + 1, (byte)'4');
                    }
                    else if (transmit_buffer_byte_list[i] == '\0')
                    {
                        transmit_buffer_byte_list[i] = (byte)'*';
                        transmit_buffer_byte_list.Insert(i + 1, (byte)'5');
                    }
                }

                if (checksum == (byte)'*')
                {
                    transmit_buffer_byte_list.Add((byte)'*');
                    transmit_buffer_byte_list.Add((byte)'2');
                }
                else if (checksum == (byte)'\r')
                {
                    transmit_buffer_byte_list.Add((byte)'*');
                    transmit_buffer_byte_list.Add((byte)'3');
                }
                else if (checksum == (byte)'\n')
                {
                    transmit_buffer_byte_list.Add((byte)'*');
                    transmit_buffer_byte_list.Add((byte)'4');
                }
                else if (checksum == (byte)'\0')
                {
                    transmit_buffer_byte_list.Add((byte)'*');
                    transmit_buffer_byte_list.Add((byte)'5');
                }
                else
                {
                    transmit_buffer_byte_list.Add((byte)checksum);
                }
                transmit_buffer_byte_list.Add((byte)'*');
                transmit_buffer_byte_list.Add((byte)'1');

                switch (comm_state)
                {
                    case Communication_enum.NONE:
                        MessageBox.Show("No connect");
                        transmit_buffer_byte_list.Clear();
                        break;
                    case Communication_enum.SERIAL:
                        sjkim_inst.data_instance.serial_1_transmit_buffer.Clear();
                        sjkim_inst.data_instance.serial_1_transmit_buffer.AddRange(transmit_buffer_byte_list);
                        break;
                    case Communication_enum.USB:
                        sjkim_inst.data_instance.serial_2_transmit_buffer.Clear();
                        sjkim_inst.data_instance.serial_2_transmit_buffer.AddRange(transmit_buffer_byte_list);
                        break;
                    case Communication_enum.BLUETOOTH:
                        sjkim_inst.data_instance.serial_3_transmit_buffer.Clear();
                        sjkim_inst.data_instance.serial_3_transmit_buffer.AddRange(transmit_buffer_byte_list);
                        break;
                    case Communication_enum.ETHERNET:
                        sjkim_inst.data_instance.ethernet_transmit_buffer.Clear();
                        sjkim_inst.data_instance.ethernet_transmit_buffer.AddRange(transmit_buffer_byte_list);
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // usb clicked
        private void button79_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort2.IsOpen == false)
                {
                    serialPort2.PortName = comboBox2.Text;
                    serialPort2.Open();
                    textBox5.Text = "USB Opened";
                    button79.BackColor = Color.FromArgb(255, 50, 30);
                    sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.USB;
                    sjkim_inst.data_instance.normal_sent = false;
                    wait_flag = false;
                    wait = false;
                    normal_wait = false;
                    if (timer1.Enabled == false)
                    {
                        timer1.Start();
                    }
                    normal_flag_reset();
                    /*
                    if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                    {
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_READ, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                    */
                }
                else
                {
                    textBox5.Text = "USB Closing";
                    Serial2CloseProcedure();
                    button79.BackColor = Color.FromArgb(255, 255, 255);
                    sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.NONE;
                    reset_first_procedrue();
                }
            }
            catch (Exception ex)
            {
                sjkim_inst.data_instance.normal_sent = true;
                MessageBox.Show(ex.ToString());
            }
        }

        // bluetooth
        private void button80_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort3.IsOpen == false)
                {
                    serialPort3.PortName = comboBox3.Text;
                    serialPort3.Open();
                    textBox5.Text = "Bluetooth Opened";
                    button80.BackColor = Color.FromArgb(255, 50, 30);
                    sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.BLUETOOTH;
                    if (timer1.Enabled == false)
                    {
                        timer1.Start();
                    }
                    normal_flag_reset();
                    /*
                    if (sjkim_inst.data_instance.comm_state != SjkimCmd.Communication_enum.NONE)
                    {
                        ParseStringToTransBuff(cmd: SjkimCmd.CmdList_enum.AGC_READ, comm_state: sjkim_inst.data_instance.comm_state);
                    }
                    */
                }
                else
                {
                    textBox5.Text = "Bluetooth Closing";
                    Serial3CloseProcedure();
                    button80.BackColor = Color.FromArgb(255, 255, 255);
                    sjkim_inst.data_instance.comm_state = SjkimCmd.Communication_enum.NONE;
                    reset_first_procedrue();
                }
            }
            catch (Exception ex)
            {
                sjkim_inst.data_instance.normal_sent = true;
                MessageBox.Show(ex.ToString());
            }
        }

        private void serialPort2_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                lock (thislock)
                {
                    this.Invoke(new EventHandler(Serial2DataProcedure));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void serialPort3_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            try
            {
                lock (thislock)
                {
                    this.Invoke(new EventHandler(Serial3DataProcedure));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
