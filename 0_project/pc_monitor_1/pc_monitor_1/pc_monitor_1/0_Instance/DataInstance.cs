using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static pc_monitor_1._2_Data.SjkimCmd;
using static pc_monitor_1._2_Data.SjkimData;

namespace pc_monitor_1._0_Instance
{
    internal class DataInstance
    {
        public float agc_dB_value = 0;
        public float alc_dBm_value = 0;

        public string[] ip_address = { "0", "0", "0", "0" };
        public string port = "0";

        public string freq_data = "0";

        public string[] operate_time = new string[1000];
        public string[] operate_code = new string[1000];
        public string[] operate_value_0 = new string[1000];
        public string[] operate_value_1 = new string[1000];

        public string[] para_adc_data = new string[(int)SetParaADC_enum.LENGTH];
        public string[] threshold_data = new string[(int)SetParaTHRESHOLD_enum.LENGTH];

        public string[] table_vtc_data = new string[(int)TableData_VTC_enum.LENGTH];
        public string[] table_fet_data = new string[(int)TableData_FET_enum.LENGTH];
        public string[] fault_data = new string[(int)FAULT_enum.LENGTH];

        public string[] power_status_data = new string[(int)StatusPower_enum.LENGTH];

        public List<byte> serial_1_receive_buffer = new List<byte>();
        public List<byte> serial_2_receive_buffer = new List<byte>();
        public List<byte> serial_3_receive_buffer = new List<byte>();
        public List<byte> ethernet_receive_buffer = new List<byte>();

        public int serial_1_receive_head = 0;
        public int serial_2_receive_head = 0;
        public int serial_3_receive_head = 0;
        public int ethernet_receive_head = 0;

        public int serial_1_receive_tail = 0;
        public int serial_2_receive_tail = 0;
        public int serial_3_receive_tail = 0;
        public int ethernet_receive_tail = 0;

        public int serial_1_parse_head = 0;
        public int serial_2_parse_head = 0;
        public int serial_3_parse_head = 0;
        public int ethernet_parse_head = 0;

        public int serial_1_parse_tail = 0;
        public int serial_2_parse_tail = 0;
        public int serial_3_parse_tail = 0;
        public int ethernet_parse_tail = 0;

        public byte[] serial_1_parse_buffer = new byte[1000];
        public byte[] serial_2_parse_buffer = new byte[1000];
        public byte[] serial_3_parse_buffer = new byte[1000];
        public byte[] ethernet_parse_buffer = new byte[1000];

        public List<byte> serial_1_transmit_buffer = new List<byte>();
        public List<byte> serial_2_transmit_buffer = new List<byte>();
        public List<byte> serial_3_transmit_buffer = new List<byte>();
        public List<byte> ethernet_transmit_buffer = new List<byte>();

        public bool serial_1_special_char_detected = false;
        public bool serial_2_special_char_detected = false;
        public bool serial_3_special_char_detected = false;
        public bool ethernet_special_char_detected = false;

        public string[] cmd_list = new string[(int)CmdList_enum.LENGTH];
        public Communication_enum comm_state = Communication_enum.NONE;

        public string now_time = null;

        public List<byte> transfer_list = new List<byte>();

        public int one_time_data_length = (int)20;

        public bool timer_2_state = false;

        public TcpClient client;
        public NetworkStream ns;
        public StreamReader sr;
        public StreamWriter sw;
        public BinaryWriter bw;

        public CmdList_enum receive_cmd_state = CmdList_enum.LENGTH;

        public List<float> fwd_freq = new List<float>();
        public List<float> fwd_atten = new List<float>();
        public List<float> fwd_adc = new List<float>();
        public List<float> fwd_dbm = new List<float>();

        public List<float> input_freq = new List<float>();
        public List<float> input_adc = new List<float>();
        public List<float> input_dbm = new List<float>();

        public bool comm_busy = false;

        public bool ethernet_connected = false;
        public bool normal_sent = false;

        public void IntDataSet(List<byte> list, int data)
        {
            List<byte> Inttobyte = new List<byte>(System.BitConverter.GetBytes(data));
            list.AddRange(Inttobyte);
        }

        public int IntDataGet(byte[] arr)
        {
            int ret = BitConverter.ToInt32(arr, 0);

            return ret;
        }

        public void FloatDataSet(List<byte> list, float data)
        {
            List<byte> floattobyte = new List<byte>(System.BitConverter.GetBytes(data));
            list.AddRange(floattobyte);
        }

        public float FloatDataGet(byte[] arr)
        {
            float ret = BitConverter.ToSingle(arr, 0);

            return ret;
        }

        public bool alc_agc_state = false;

        public int timer2_count = 0;
        public bool fault_occured_before = false;

        public float fwd_adc_voltage_0;
        public float fwd_adc_voltage_1 = 0;
        public float rfl_adc_voltage_0;
        public float rfl_adc_voltage_1 = 0;
        public float input_adc_voltage_0;
        public UInt16 dac_adc_voltage_0;

        public List<string> file_transfer = new List<string>();

        public bool ethernet_opened_first = false;

        public string now_button = "";  // TABLE, FAULT, ALC, COMM

        public bool load_last = false;
        public bool input_detect_high = false;

        public int pwr_display_delay_count_0 = 0;
        public int pwr_display_delay_count_1 = 0;
        public int pwr_display_delay_count_2 = 0;

        public bool agc_loaded_first = false;
        public bool alc_loaded_first = false;
        public bool sys_id_loaded_first = false;
        public bool monitoring_opend = false;
        public bool cmd_mode_on = false;

        public string monitoring_idn;
        public string monitoring_model;
        public string monitoring_serial_num;
        public string monitoring_firmware;
        public string monitoring_software;
        public string monitoring_description;
        public string monitoring_manufacture;

        public bool display_all_toggle_pushed = false;

        public int freq_data_ret_count;

        public bool checksum_error_off = true;
    }
}
