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
    public partial class freq_pwr_progress_form_5 : Form
    {
        public GetEventHandler freq_pwr_progress_form_send_event;

        List<string> freq_list = new List<string>();
        List<string[]> atten_list = new List<string[]>();
        List<string[]> fwd_adc_list = new List<string[]>();
        List<string[]> fwd_dbm_list = new List<string[]>();
        List<string> f_p_index_list = new List<string>();

        float fwd_max = 0;
        float coupling = 0;
        bool combine_done = false;
        bool sending_busy_state = false;
        int send_state = 0;
        int sent_freq_list_index = 0;
        int sent_freq_list_max = 0;
        int progress_adder = 0;
        bool read_from_file = false;
        bool callback_first_sent = false;
        int timer_tick = 0;

        int read_number_freq_max = 0;
        int read_numberof_freq = 0;
        string read_0dbm_ref = "";
        string read_offset = "";
        List<int> read_item_length = new List<int>();
        List<string> read_freq_list = new List<string>();
        List<string[]> read_atten_list = new List<string[]>();
        List<string[]> read_fwd_adc_list = new List<string[]>();
        List<string[]> read_fwd_dbm_list = new List<string[]>();

        public freq_pwr_progress_form_5()
        {
            InitializeComponent();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                if (combine_done == true)
                {
                    if(sending_busy_state == false)
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

                        timer1.Start();
                        callback_first_sent = false;
                        timer_tick = 0;

                        send_state = 0;
                        sent_freq_list_index = 0;
                        sent_freq_list_max = freq_list.Count;

                        if (this.InvokeRequired == true)
                        {
                            label5.Invoke((MethodInvoker)delegate
                            {
                                label5.Text = "";
                            });
                            progressBar1.Invoke((MethodInvoker)delegate
                            {
                                progressBar1.Value = 0;
                            });
                        }
                        else
                        {
                            label5.Text = "";
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
                            label5.Invoke((MethodInvoker)delegate
                            {
                                label5.Text = "Sending data...";
                            });
                        }
                        else
                        {
                            label5.Text = "Sending data...";
                        }
                    }
                    else
                    {
                        if (this.InvokeRequired == true)
                        {
                            label5.Invoke((MethodInvoker)delegate
                            {
                                label5.Text = "Wait...";
                            });
                        }
                        else
                        {
                            label5.Text = "Wait...";
                        }
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

        void read_from_sys_set_procedure()
        {
            progressBar1.Value = 100 / read_number_freq_max * read_numberof_freq;
            if (read_numberof_freq == read_number_freq_max)
            {
                progressBar1.Value = 100;
                label5.Text = "Read from system done, Data Combine done";
                timer2.Start();

                textBox3.Text = read_freq_list.Count.ToString();
                textBox4.Text = Math.Round(float.Parse(read_0dbm_ref), 2).ToString();
                textBox5.Text = Math.Round(float.Parse(read_offset), 2).ToString();
                for (int i = 0; i < read_freq_list.Count; i++)
                {
                    textBox6.Text += read_freq_list[i];
                    if (i != read_freq_list.Count - 1)
                    {
                        textBox6.Text += ", ";
                    }
                }

                freq_list.Clear();
                atten_list.Clear();
                fwd_adc_list.Clear();
                fwd_dbm_list.Clear();
                f_p_index_list.Clear();
                fwd_max = 0;
                coupling = 0;

                for(int i = 0; i < read_freq_list.Count; i++)
                {
                    freq_list.Add(read_freq_list[i]);
                }
                for(int i = 0; i < read_atten_list.Count; i++)
                {
                    atten_list.Add(read_atten_list[i]);
                }
                for (int i = 0; i < read_fwd_adc_list.Count; i++)
                {
                    fwd_adc_list.Add(read_fwd_adc_list[i]);
                }

                for (int i = 0; i < read_fwd_dbm_list.Count; i++)
                {
                    fwd_dbm_list.Add(read_fwd_dbm_list[i]);
                }
                fwd_max = float.Parse(textBox4.Text);
                coupling = float.Parse(textBox5.Text);

                combine_done = true;
                freq_pwr_progress_form_send_event(cmd: "FREQ_PWR_DONE");
            }
        }

        public void set_datagrid_from_main(string cmd, string data, string[] data_arr)
        {
            try
            {
                if (cmd == "FREQ_PWR_SET_RETURN")
                {
                    if (combine_done == true)
                    {
                        if (sending_busy_state == true)
                        {
                            if(callback_first_sent == false)
                            {
                                callback_first_sent = true;
                            }
                            List<string> transfer = new List<string>();
                            send_loop(transfer);
                        }
                    }
                }

                if (cmd == "FREQ_PWR_GET_OPTION")
                {
                    read_numberof_freq += 1;
                    read_number_freq_max = int.Parse(data_arr[0]);
                    read_0dbm_ref = data_arr[1];
                    read_offset = data_arr[2];
                    read_item_length.Add(int.Parse(data_arr[3]));
                    read_freq_list.Add(data_arr[4]);

                    if (this.InvokeRequired == true)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            textBox3.Text = "";  // number of freq
                            textBox6.Text = "";  // frequency mhz arr
                            textBox4.Text = "";  // 0dbm ref
                            textBox5.Text = "";  // offset
                        });
                    }
                    else
                    {
                        textBox3.Text = "";  // number of freq
                        textBox6.Text = "";  // frequency mhz arr
                        textBox4.Text = "";  // 0dbm ref
                        textBox5.Text = "";  // offset
                    }
                }
                if(cmd == "FREQ_PWR_GET_ATTEN_DATA")
                {
                    read_atten_list.Add(data_arr);
                }
                if(cmd == "FREQ_PWR_GET_FORWARD_DET_DATA")
                {
                    read_fwd_adc_list.Add(data_arr);
                }
                if(cmd == "FREQ_PWR_GET_DBM_DATA")
                {
                    read_fwd_dbm_list.Add(data_arr);
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

        private void load_received_data()
        {

        }

        private void send_loop(List<string> transfer)
        {
            if (sent_freq_list_index == freq_list.Count)
            {
                sending_busy_state = false;

                if (this.InvokeRequired == true)
                {
                    progressBar1.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = 100;
                        label5.Text = "Done";
                        if(timer2.Enabled == false)
                        {
                            timer2.Start();
                        }
                    });
                }
                else
                {
                    progressBar1.Value = 100;
                    label5.Text = "Done";
                    if (timer2.Enabled == false)
                    {
                        timer2.Start();
                    }
                }
                freq_pwr_progress_form_send_event(cmd: "FREQ_PWR_DONE", data_arr: transfer.ToArray());
                return;
            }

            transfer.Add(((byte)(SjkimCmd.FreqCommand_enum.FWD_FILE_SET)).ToString());
            transfer.Add(fwd_max.ToString());
            transfer.Add(coupling.ToString());
            transfer.Add(freq_list.Count.ToString());

            transfer.Add(atten_list[sent_freq_list_index].Length.ToString());
            transfer.Add(freq_list[sent_freq_list_index].ToString());
            transfer.Add(send_state.ToString());

            if (send_state == 0)
            {
                for (int j = 0; j < atten_list[sent_freq_list_index].Length; j++)
                {
                    transfer.Add(atten_list[sent_freq_list_index][j].ToString());
                }
                send_state += 1;
            }
            else if (send_state == 1)
            {
                for (int j = 0; j < fwd_adc_list[sent_freq_list_index].Length; j++)
                {
                    transfer.Add(fwd_adc_list[sent_freq_list_index][j].ToString());
                }
                send_state += 1;
            }
            else if (send_state == 2)
            {
                for (int j = 0; j < fwd_dbm_list[sent_freq_list_index].Length; j++)
                {
                    transfer.Add(fwd_dbm_list[sent_freq_list_index][j].ToString());
                }
                send_state += 1;
                send_state %= 3;
                sent_freq_list_index += 1;
            }
            freq_pwr_progress_form_send_event(cmd: "FREQ_PWR_PUSH", data_arr: transfer.ToArray());

            timer1.Start();
            callback_first_sent = false;
            timer_tick = 0;

            int sum = progressBar1.Value + progress_adder;
            if(sum <= 100)
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
                    textBox2.Text = folderpath;

                    DirectoryInfo di = new DirectoryInfo(folderpath);
                    int fwd_file_num = 0;
                    textBox6.Text = "";

                    List<int> freq_list = new List<int>();

                    foreach (var item in di.GetFiles())
                    {
                        if (item.Name.Contains("fwd") && item.Name.Contains(".txt"))
                        {
                            if(item.Name.IndexOf("fwd") == 0)
                            {
                                string fwd_freq_searched = item.Name.Replace("fwd", "");
                                fwd_freq_searched = fwd_freq_searched.Replace(".txt", "");
                                if(fwd_freq_searched.All(char.IsDigit))
                                {
                                    freq_list.Add(int.Parse(fwd_freq_searched));
                                    fwd_file_num += 1;
                                }
                            }
                        }
                    }
                    var result = from n in freq_list orderby n select n;
                    int count = 0;
                    textBox3.Text = fwd_file_num.ToString();

                    foreach (int data in result)
                    {
                        textBox6.Text += data.ToString();
                        count += 1;
                        if(count != fwd_file_num)
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

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (check_text_box() == true)
                {
                    if(read_from_file == true)
                    {
                        combine_done = true;
                    }
                    else
                    {
                        freq_list.Clear();
                        atten_list.Clear();
                        fwd_adc_list.Clear();
                        fwd_dbm_list.Clear();
                        f_p_index_list.Clear();
                        fwd_max = 0;
                        coupling = 0;

                        DirectoryInfo di = new DirectoryInfo(textBox2.Text);

                        foreach (var item in di.GetFiles())
                        {
                            List<string> atten_list_buff = new List<string>();
                            List<string> fwd_adc_list_buff = new List<string>();
                            List<string> fwd_dbm_list_buff = new List<string>();

                            if (item.Name.Contains("fwd") && item.Name.Contains(".txt"))
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
                                                atten_list_buff.Add(reader);

                                                reader = sr.ReadLine();
                                                if (reader != null)
                                                {
                                                    fwd_adc_list_buff.Add(reader);

                                                    reader = sr.ReadLine();
                                                    if (reader != null)
                                                    {
                                                        fwd_dbm_list_buff.Add(reader);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                atten_list.Add(atten_list_buff.ToArray());
                                fwd_adc_list.Add(fwd_adc_list_buff.ToArray());
                                fwd_dbm_list.Add(fwd_dbm_list_buff.ToArray());
                                fwd_max = float.Parse(textBox4.Text);
                                coupling = float.Parse(textBox5.Text);
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

        private bool check_text_box()
        {
            if(textBox2.Text == "")
            {
                MessageBox.Show("Please fill path textbox");
                return false;
            }
            if(textBox3.Text == "" || textBox3.Text == "0")
            {
                MessageBox.Show("Please check number of frequency files");
                return false;
            }
            if(textBox4.Text == "")
            {
                MessageBox.Show("Please fill \"ATTEN 0dB Reference PWR(dBm)\"");
                return false;
            }

            if (textBox5.Text == "")
            {
                MessageBox.Show("Please fill \"Forward Power Offset(dBm)\"");
                return false;
            }

            return true;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                if (combine_done == true)
                {
                    DialogResult dr = saveFileDialog1.ShowDialog();
                    if(dr == DialogResult.OK)
                    {
                        string file_name = saveFileDialog1.FileName;
                        FileStream fs = File.Create(file_name);
                        fs.Close();

                        StreamWriter sw = new StreamWriter(file_name);

                        sw.WriteLine(fwd_max);
                        sw.WriteLine(coupling);
                        sw.WriteLine(freq_list.Count);

                        for (int i = 0; i < freq_list.Count; i++)
                        {
                            sw.WriteLine(atten_list[i].Length);
                            sw.WriteLine(freq_list[i]);
                            for(int j = 0; j < atten_list[i].Length; j++)
                            {
                                sw.WriteLine(atten_list[i][j]);
                            }
                            for (int j = 0; j < fwd_adc_list[i].Length; j++)
                            {
                                sw.WriteLine(fwd_adc_list[i][j]);
                            }
                            for (int j = 0; j < fwd_dbm_list[i].Length; j++)
                            {
                                sw.WriteLine(fwd_dbm_list[i][j]);
                            }
                        }
                        sw.Close();
                        label5.Text = "Write to file done";
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

        private void freq_pwr_progress_form_5_Load(object sender, EventArgs e)
        {
            label5.Text = "";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                if (sending_busy_state == false)
                {
                    read_atten_list.Clear();
                    read_fwd_adc_list.Clear();
                    read_fwd_dbm_list.Clear();

                    textBox3.Text = "";  // number of freq
                    textBox6.Text = "";  // frequency mhz arr
                    textBox4.Text = "";  // 0dbm ref
                    textBox5.Text = "";  // offset

                    read_numberof_freq = 0;
                    read_0dbm_ref = "";
                    read_offset = "";
                    read_item_length.Clear();
                    read_freq_list.Clear();

                    freq_pwr_progress_form_send_event(cmd: "FREQ_PWR_PULL", data: "-1");
                    label5.Text = "Receiveing data...";
                    progressBar1.Value = 0;
                }
                else
                {
                    label5.Text = "Wait...";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = openFileDialog1.ShowDialog();
                if(dr == DialogResult.OK)
                {
                    freq_list.Clear();
                    atten_list.Clear();
                    fwd_adc_list.Clear();
                    fwd_dbm_list.Clear();
                    fwd_max = 0;
                    coupling = 0;

                    string filename = openFileDialog1.FileName;
                    textBox2.Text = filename.Replace(openFileDialog1.SafeFileName, "");

                    StreamReader sr = new StreamReader(filename);
                    fwd_max = float.Parse(sr.ReadLine());
                    coupling = float.Parse(sr.ReadLine());
                    int freq_num = Int32.Parse(sr.ReadLine());

                    for(int i = 0; i < freq_num; i++)
                    {
                        string list_length = sr.ReadLine();
                        freq_list.Add(sr.ReadLine());
                        List<string> transfer = new List<string>();
                        for(int j = 0; j < Int32.Parse(list_length); j++)
                        {
                            transfer.Add(sr.ReadLine());
                        }
                        atten_list.Add(transfer.ToArray());
                        transfer.Clear();
                        for (int j = 0; j < Int32.Parse(list_length); j++)
                        {
                            transfer.Add(sr.ReadLine());
                        }
                        fwd_adc_list.Add(transfer.ToArray());
                        transfer.Clear();
                        for (int j = 0; j < Int32.Parse(list_length); j++)
                        {
                            transfer.Add(sr.ReadLine());
                        }
                        fwd_dbm_list.Add(transfer.ToArray());
                        transfer.Clear();
                    }
                    textBox3.Text = freq_num.ToString();
                    textBox6.Text = "";
                    for (int i = 0; i < freq_list.Count; i++)
                    {
                        if(textBox6.Text != "")
                        {
                            textBox6.Text += ", ";
                        }
                        textBox6.Text += freq_list[i].ToString();
                    }
                    textBox4.Text = fwd_max.ToString();
                    textBox5.Text = coupling.ToString();
                    combine_done = true;
                    label5.Text = "Read from file done";
                    read_from_file = true;
                }
            }
            catch(Exception ex)
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
                if(timer_tick >= 10)
                {
                    timer1.Stop();
                    sending_busy_state = false;
                    label5.Text = "timeout...";
                    progressBar1.Value = 0;
                    MessageBox.Show("Timeout... \r\nCheck Communication state");
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
    }
}
