using pc_monitor_1._2_Data;
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

namespace pc_monitor_1
{
    public partial class input_pwr_progress_form_7 : Form
    {
        public GetEventHandler input_pwr_progress_form_send_event;
        private bool combine_done;
        private bool read_from_file;
        List<string> freq_list = new List<string>();
        List<string[]> input_adc_list = new List<string[]>();
        List<string[]> input_dbm_list = new List<string[]>();
        float step_value;
        private bool sending_busy_state;
        private int progress_adder;
        private bool callback_first_sent;
        private int timer_tick;
        private int send_state;
        private int sent_freq_list_index;
        private int sent_freq_list_max;

        int read_number_freq_max = 0;
        int read_numberof_freq = 0;
        string read_step_value = "";
        List<int> read_item_length = new List<int>();
        List<string> read_freq_list = new List<string>();
        List<string[]> read_input_adc_list = new List<string[]>();
        List<string[]> read_input_dbm_list = new List<string[]>();

        public input_pwr_progress_form_7()
        {
            InitializeComponent();
        }

        public void set_datagrid_from_main(string cmd, string data, string[] data_arr)
        {
            try
            {
                if (cmd == "FREQ_INPUT_SET_RETURN")
                {
                    if (combine_done == true)
                    {
                        if (sending_busy_state == true)
                        {
                            if (callback_first_sent == false)
                            {
                                callback_first_sent = true;
                            }
                            List<string> transfer = new List<string>();
                            send_loop(transfer);
                        }
                    }
                }

                if (cmd == "FREQ_INPUT_GET_OPTION")
                {
                    read_numberof_freq += 1;
                    read_number_freq_max = int.Parse(data_arr[0]);
                    read_step_value = data_arr[1];
                    read_item_length.Add(int.Parse(data_arr[2]));
                    read_freq_list.Add(data_arr[3]);

                    if (this.InvokeRequired == true)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            textBox5.Text = "";  // number of freq
                            textBox6.Text = "";  // frequency mhz arr
                            textBox3.Text = "";  // step value
                        });
                    }
                    else
                    {
                        textBox3.Text = "";  // number of freq
                        textBox6.Text = "";  // frequency mhz arr
                        textBox3.Text = "";  // step value
                    }
                }

                if(cmd == "FREQ_INPUT_GET_INPUT_DET_DATA")
                {
                    read_input_adc_list.Add(data_arr);
                }

                if(cmd == "FREQ_INPUT_DBM_DATA")
                {
                    read_input_dbm_list.Add(data_arr);
                    if (this.InvokeRequired == true)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            read_from_sys_set_procedure();
                        });
                    }
                    else
                    {
                        read_from_sys_set_procedure();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void read_from_sys_set_procedure()
        {
            progressBar1.Value = 100 / read_number_freq_max * read_numberof_freq;
            if (read_numberof_freq == read_number_freq_max)
            {
                progressBar1.Value = 100;
                label10.Text = "Read from system done, Data Combine done";
                timer2.Start();

                textBox5.Text = read_freq_list.Count.ToString();
                textBox3.Text = Math.Round(float.Parse(read_step_value), 2).ToString();
                for (int i = 0; i < read_freq_list.Count; i++)
                {
                    textBox6.Text += read_freq_list[i];
                    if (i != read_freq_list.Count - 1)
                    {
                        textBox6.Text += ", ";
                    }
                }

                freq_list.Clear();
                input_adc_list.Clear();
                input_dbm_list.Clear();
                step_value = 0;

                for (int i = 0; i < read_freq_list.Count; i++)
                {
                    freq_list.Add(read_freq_list[i]);
                }
                for (int i = 0; i < read_input_adc_list.Count; i++)
                {
                    input_adc_list.Add(read_input_adc_list[i]);
                }
                for (int i = 0; i < read_input_dbm_list.Count; i++)
                {
                    input_dbm_list.Add(read_input_dbm_list[i]);
                }

                step_value = float.Parse(textBox3.Text);

                combine_done = true;
                input_pwr_progress_form_send_event(cmd: "FREQ_INPUT_DONE");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (combine_done == true)
                {
                    if (sending_busy_state == false)
                    {
                        timer1.Start();
                        callback_first_sent = false;
                        timer_tick = 0;

                        send_state = 0;
                        sent_freq_list_index = 0;
                        sent_freq_list_max = freq_list.Count;

                        if (this.InvokeRequired == true)
                        {
                            label10.Invoke((MethodInvoker)delegate
                            {
                                label10.Text = "";
                            });
                            progressBar1.Invoke((MethodInvoker)delegate
                            {
                                progressBar1.Value = 0;
                            });
                        }
                        else
                        {
                            label10.Text = "";
                            progressBar1.Value = 0;
                        }
                        sending_busy_state = true;
                        progress_adder = 100 / (3 * freq_list.Count);

                        List<string> transfer = new List<string>();

                        send_loop(transfer);

                        timer1.Start();
                        callback_first_sent = false;
                        timer_tick = 0;
                        if (this.InvokeRequired == true)
                        {
                            label10.Invoke((MethodInvoker)delegate
                            {
                                label10.Text = "Sending data...";
                            });
                        }
                        else
                        {
                            label10.Text = "Sending data...";
                        }
                    }
                    else
                    {
                        if (this.InvokeRequired == true)
                        {
                            label10.Invoke((MethodInvoker)delegate
                            {
                                label10.Text = "Wait...";
                            });
                        }
                        else
                        {
                            label10.Text = "Wait...";
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please combine first");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void send_loop(List<string> transfer)
        {
            if ((sent_freq_list_index == sent_freq_list_max) && (send_state == 0))
            {
                sending_busy_state = false;
                if (this.InvokeRequired == true)
                {
                    progressBar1.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = 100;
                        label10.Text = "Done";
                        if (timer2.Enabled == false)
                        {
                            timer2.Start();
                        }
                    });
                }
                else
                {
                    progressBar1.Value = 100;
                    label10.Text = "Done";
                    if (timer2.Enabled == false)
                    {
                        timer2.Start();
                    }
                }
                input_pwr_progress_form_send_event(cmd: "FREQ_INPUT_DONE");
                return;
            }

            // transmit data description
            // address 0.. command enum value
            // address 1.. step value from user
            // address 2.. whole frequency list length 
            // address 3.. transmitting frequency item length
            // address 4.. transmitting frequency's frequecny value
            // address 5.. send_state
            // address 6 ~ ... depend on send_state the data'll be adc data or dbm value

            // file line description
            // line 0.. frequency value
            // line 3 * (n - 1) + 0.. number
            // line 3 * (n - 1) + 1.. input adc value
            // line 3 * (n - 1) + 2.. input dbm value
            // ,,,

            transfer.Add(((byte)(SjkimCmd.FreqCommand_enum.INPUT_FILE_SET)).ToString());
            transfer.Add(step_value.ToString());
            transfer.Add(freq_list.Count.ToString());

            transfer.Add(input_adc_list[sent_freq_list_index].Length.ToString());
            transfer.Add(freq_list[sent_freq_list_index].ToString());
            transfer.Add(send_state.ToString());

            if (send_state == 0)
            {
                for (int j = 0; j < input_adc_list[sent_freq_list_index].Length; j++)
                {
                    transfer.Add(input_adc_list[sent_freq_list_index][j].ToString());
                }
                send_state += 1;
            }
            else if (send_state == 1)
            {
                for (int j = 0; j < input_dbm_list[sent_freq_list_index].Length; j++)
                {
                    transfer.Add(input_dbm_list[sent_freq_list_index][j].ToString());
                }
                send_state += 1;
                send_state %= 2;
                sent_freq_list_index += 1;
            }
            input_pwr_progress_form_send_event(cmd: "FREQ_INPUT_PUSH", data_arr: transfer.ToArray());

            timer1.Start();
            callback_first_sent = false;
            timer_tick = 0;

            int sum = progressBar1.Value + progress_adder;
            if (sum <= 100)
            {
                if (this.InvokeRequired == true)
                {
                    progressBar1.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = sum;
                    });
                }
                else
                {
                    progressBar1.Value = sum;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = folderBrowserDialog1.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    combine_done = false;
                    string folderpath = folderBrowserDialog1.SelectedPath;
                    textBox4.Text = folderpath;

                    DirectoryInfo di = new DirectoryInfo(folderpath);
                    int fwd_file_num = 0;
                    textBox5.Text = "";

                    List<int> freq_list = new List<int>();

                    foreach (var item in di.GetFiles())
                    {
                        if (item.Name.Contains("input") && item.Name.Contains(".txt"))
                        {
                            if (item.Name.IndexOf("input") == 0)
                            {
                                string input_freq_searched = item.Name.Replace("input", "");
                                input_freq_searched = input_freq_searched.Replace(".txt", "");
                                if (input_freq_searched.All(char.IsDigit))
                                {
                                    freq_list.Add(int.Parse(input_freq_searched));
                                    fwd_file_num += 1;
                                }
                            }
                        }
                    }
                    var result = from n in freq_list orderby n select n;
                    int count = 0;
                    textBox5.Text = fwd_file_num.ToString();
                    textBox6.Text = "";

                    foreach (int data in result)
                    {
                        textBox6.Text += data.ToString();
                        count += 1;
                        if (count != fwd_file_num)
                        {
                            textBox6.Text += ", ";
                        }
                    }
                    read_from_file = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private bool check_text_box()
        {
            if (textBox4.Text == "")
            {
                MessageBox.Show("Please fill path textbox");
                return false;
            }
            if (textBox5.Text == "" || textBox5.Text == "0")
            {
                MessageBox.Show("Please check number of frequency files");
                return false;
            }
            if (textBox3.Text == "")
            {
                MessageBox.Show("Please fill \"Step value textbox\"");
                return false;
            }

            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (check_text_box() == true)
                {
                    if (read_from_file == true)
                    {
                        combine_done = true;
                    }
                    else
                    {
                        freq_list.Clear();
                        input_adc_list.Clear();
                        input_dbm_list.Clear();
                        step_value = 0;

                        DirectoryInfo di = new DirectoryInfo(textBox4.Text);

                        foreach (var item in di.GetFiles())
                        {
                            List<string> input_adc_list_buff = new List<string>();
                            List<string> input_dbm_list_buff = new List<string>();

                            if (item.Name.Contains("input") && item.Name.Contains(".txt"))
                            {
                                StreamReader sr = new StreamReader(item.FullName);
                                string reader = sr.ReadLine();
                                freq_list.Add(reader);
                                string num_checker;

                                while (true)
                                {
                                    if (reader != null)
                                    {
                                        reader = sr.ReadLine();
                                        if (reader != null)
                                        {
                                            num_checker = reader;

                                            reader = sr.ReadLine();
                                            if (reader != null)
                                            {
                                                input_adc_list_buff.Add(reader);

                                                reader = sr.ReadLine();
                                                if (reader != null)
                                                {
                                                    input_dbm_list_buff.Add(reader);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                input_adc_list.Add(input_adc_list_buff.ToArray());
                                input_dbm_list.Add(input_dbm_list_buff.ToArray());
                                step_value = float.Parse(textBox3.Text);
                            }
                        }
                        combine_done = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                if (combine_done == true)
                {
                    DialogResult dr = saveFileDialog1.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        string file_name = saveFileDialog1.FileName;
                        FileStream fs = File.Create(file_name);
                        fs.Close();

                        StreamWriter sw = new StreamWriter(file_name);

                        sw.WriteLine(step_value);
                        sw.WriteLine(freq_list.Count);

                        for (int i = 0; i < freq_list.Count; i++)
                        {
                            sw.WriteLine(input_adc_list[i].Length);
                            sw.WriteLine(freq_list[i]);
                            for (int j = 0; j < input_adc_list[i].Length; j++)
                            {
                                sw.WriteLine(input_adc_list[i][j]);
                            }
                        }
                        sw.Close();
                        label10.Text = "Write to file done";
                    }
                    else
                    {

                    }
                }
                else
                {
                    MessageBox.Show("Please combine first");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = openFileDialog1.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    freq_list.Clear();
                    input_adc_list.Clear();
                    step_value = 0;

                    string filename = openFileDialog1.FileName;
                    textBox4.Text = filename.Replace(openFileDialog1.SafeFileName, "");

                    StreamReader sr = new StreamReader(filename);
                    step_value = float.Parse(sr.ReadLine());
                    int freq_num = Int32.Parse(sr.ReadLine());

                    for (int i = 0; i < freq_num; i++)
                    {
                        string list_length = sr.ReadLine();
                        freq_list.Add(sr.ReadLine());
                        List<string> transfer = new List<string>();
                        for (int j = 0; j < Int32.Parse(list_length); j++)
                        {
                            transfer.Add(sr.ReadLine());
                        }
                        input_adc_list.Add(transfer.ToArray());
                        transfer.Clear();
                    }
                    sr.Close();

                    textBox5.Text = freq_num.ToString();
                    textBox6.Text = "";
                    for (int i = 0; i < freq_list.Count; i++)
                    {
                        if (textBox6.Text != "")
                        {
                            textBox6.Text += ", ";
                        }
                        textBox6.Text += freq_list[i].ToString();
                    }
                    textBox3.Text = step_value.ToString();
                    combine_done = true;
                    label10.Text = "Read from file done";
                    read_from_file = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (callback_first_sent == true)
            {
                timer1.Stop();
            }
            else
            {
                timer_tick += 1;
                /*
                if (timer_tick >= 10)
                {
                    timer1.Stop();
                    sending_busy_state = false;
                    label10.Text = "timeout...";
                    progressBar1.Value = 0;
                    // MessageBox.Show("Timeout... \r\nCheck Communication state");
                }
                */
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
            if (progressBar1.Value == 100)
            {
                progressBar1.Value = 0;
            }
        }

        private void input_pwr_progress_form_7_Load(object sender, EventArgs e)
        {
            label10.Text = "";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (sending_busy_state == false)
                {
                    read_input_adc_list.Clear();
                    read_input_dbm_list.Clear();

                    textBox5.Text = "";  // number of freq
                    textBox6.Text = "";  // frequency mhz arr
                    textBox3.Text = "";  // step value

                    read_numberof_freq = 0;
                    read_step_value = "";
                    read_item_length.Clear();
                    read_freq_list.Clear();

                    input_pwr_progress_form_send_event(cmd: "FREQ_INPUT_PULL");
                    label10.Text = "Receiveing data...";
                    progressBar1.Value = 0;
                }
                else
                {
                    label10.Text = "Wait...";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
