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
    public partial class freq_pwr_mea_form_4 : Form
    {
        int progress_value = 0;
        public GetEventHandler freq_pwr_mea_form_send_event;

        public freq_pwr_mea_form_4()
        {
            InitializeComponent();

            // 아래의 자료형을 사용해서 리턴을 주고 받을 예정이다.
            /*
            public string[] freq_pwr_atten_volt = new string[1000];
            public string[] freq_pwr_adc = new string[1000];
            public string[] freq_pwr_dbm = new string[1000];
            public int freq_pwr_numdata = 0;
            */
        }

        public void set_textbox_from_main(string cmd, string data, string[] data_arr)
        {
            /*
            (sjkim_inst.data_instance.dac_adc_voltage_0 / 4095.0F * 3.3F).ToString(), 
                                      sjkim_inst.data_instance.fwd_adc_voltage_0.ToString()};
            */

            try
            {
                if (this.InvokeRequired == true)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (cmd == "FREQ_PWR_MEA_VOLTAGE_PUSH")
                        {
                            textBox2.Text = (float.Parse(data_arr[0]) * 3.3F / 4095.0F).ToString("0.000");  // data_arr 0: dac voltage
                            textBox3.Text = float.Parse(data_arr[1]).ToString("0.000");  // data_arr 1: fwd voltage
                        }
                    });
                }
                else
                {
                    if (cmd == "FREQ_PWR_MEA_VOLTAGE_PUSH")
                    {
                        textBox2.Text = (float.Parse(data_arr[0])).ToString("0.000");  // data_arr 0: dac voltage
                        textBox3.Text = float.Parse(data_arr[1]).ToString("0.000");  // data_arr 1: fwd voltage
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void set_datagrid_from_main(string cmd, string data, string[] data_arr)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                freq_pwr_mea_form_send_event(cmd: "FREQ_PWR_MEA_VOLTAGE_FETCH");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void freq_pwr_mea_form_4_Load(object sender, EventArgs e)
        {
            label19.Text = "";
            timer1.Start();
        }

        private void freq_pwr_mea_form_4_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            label19.Text = "Auto scan started";

            progress_value += 1;
            if(progress_value > 100)
            {
                progress_value = 100;
            }
            if(progress_value < 0)
            {
                progress_value = 0;
            }
            progressBar1.Value = progress_value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = folderBrowserDialog1.ShowDialog();
            if(dr == DialogResult.OK)
            {
                string folderpath = folderBrowserDialog1.SelectedPath;
                textBox1.Text = folderpath;
            }
        }

        void step_click_procedure()
        {

            if (textBox14.Text != string.Empty)
            {
                float transfer = Single.Parse(textBox2.Text) + Single.Parse(textBox14.Text);
                textBox4.Text = transfer.ToString();
                freq_pwr_mea_form_send_event(cmd: "FREQ_PWR_MEA_SET_DAC_VOLTAGE", data: textBox4.Text);
            }
            else
            {
                MessageBox.Show("Please fill step voltage textbox");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                step_click_procedure();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if(textBox10.Text == String.Empty)
            {
                MessageBox.Show("Please fill frequency textbox");
            }
            else
            {
                // Reset 1)freq array, 2)numdata, 3)datagrid
                if (dataGridView1.Rows.Count != 0)
                {
                    dataGridView1.Rows.Clear();
                    dataGridView1.Refresh();
                }
                freq_pwr_mea_form_send_event(cmd: "FREQ_PWR_MEA_SET_DAC_VOLTAGE", data: "0");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            freq_pwr_mea_form_send_event(cmd: "FREQ_PWR_MEA_SET_DAC_VOLTAGE", data: textBox4.Text);
        }

        void add_procedure()
        {

            if (textBox10.Text == String.Empty)
            {
                MessageBox.Show("Please fill frequency textbox");
                return;
            }
            if (textBox14.Text == string.Empty)
            {
                MessageBox.Show("Please fill step voltage textbox");
                return;
            }
            if (textBox7.Text == String.Empty)
            {
                MessageBox.Show("Please fill rf power textbox");
                return;
            }
            dataGridView1.Rows.Add(dataGridView1.Rows.Count, textBox2.Text, textBox3.Text, textBox7.Text);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                add_procedure();
            }
            catch(Exception ex) { MessageBox.Show(ex.ToString()); };
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                textBox15.Text = dataGridView1.Rows[dataGridView1.CurrentCellAddress.Y].Cells[0].Value.ToString();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.Rows.RemoveAt(dataGridView1.CurrentCellAddress.Y);
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataGridView1.Rows[i].Cells[0].Value = i;
                }
                textBox15.Text = dataGridView1.Rows[dataGridView1.CurrentCellAddress.Y].Cells[0].Value.ToString();
            }
            catch
            {
                MessageBox.Show("TABLE IS EMPTY");
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox10.Text == String.Empty)
                {
                    MessageBox.Show("Please fill frequency textbox");
                    return;
                }
                if (textBox14.Text == string.Empty)
                {
                    MessageBox.Show("Please fill step voltage textbox");
                    return;
                }
                if (textBox7.Text == String.Empty)
                {
                    MessageBox.Show("Please fill rf power textbox");
                    return;
                }
                if (textBox1.Text == string.Empty)
                {
                    MessageBox.Show("Please fill path textbox");
                    return;
                }
                string file_name = textBox1.Text + "\\fwd" + textBox10.Text + ".txt";
                FileStream fs = File.Create(file_name);
                fs.Close();

                StreamWriter sw = new StreamWriter(file_name);
                sw.WriteLine(textBox10.Text);
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    sw.WriteLine(dataGridView1.Rows[i].Cells[0].Value);
                    sw.WriteLine(dataGridView1.Rows[i].Cells[1].Value);
                    sw.WriteLine(dataGridView1.Rows[i].Cells[2].Value);
                    sw.WriteLine(dataGridView1.Rows[i].Cells[3].Value);
                }
                sw.Close();

                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = openFileDialog1.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    string file_name = openFileDialog1.SafeFileName;
                    string file_full_name = openFileDialog1.FileName;
                    string file_path = file_full_name.Replace(file_name, "");
                    
                    while(dataGridView1.RowCount != 0)
                    {
                        dataGridView1.Rows.RemoveAt(0);
                    }
                    dataGridView1.Refresh();

                    StreamReader sr = new StreamReader(file_full_name);

                    string freq = sr.ReadLine();
                    if(freq != null)
                    {
                        textBox10.Text = freq;
                    }

                    string read_buff_no = string.Empty;
                    string read_buff_att = string.Empty;
                    string read_buff_pwr = string.Empty;
                    string read_buff_dbm = string.Empty;
                    while ((read_buff_no != null) && (read_buff_att != null) && (read_buff_pwr != null) && (read_buff_dbm != null))
                    {
                        read_buff_no = sr.ReadLine();
                        read_buff_att = sr.ReadLine();
                        read_buff_pwr = sr.ReadLine();
                        read_buff_dbm = sr.ReadLine();
                        if ((read_buff_no != null) && (read_buff_att != null) && (read_buff_pwr != null) && (read_buff_dbm != null))
                        {
                            dataGridView1.Rows.Add(read_buff_no, read_buff_att, read_buff_pwr, read_buff_dbm);
                        }
                    }
                    sr.Close();
                }
                else if (dr == DialogResult.Cancel)
                {
                    return;
                }

                return;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                int error_occured = 0;
                int error_row = -1;
                int error_col = -1;
                float error_val = 0;

                if (dataGridView1.RowCount > 1)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        bool checker = false;
                        bool ascending = false;

                        if (error_occured != 0)
                        {
                            switch(error_occured)
                            {
                                case 1:  // assending error
                                    MessageBox.Show("DATA ERROR LINE: " + error_row.ToString() + "\r\n" 
                                                    + "DATA ERROR COLOMN: " + error_col.ToString() + "\r\n" 
                                                    + "Not asscending" + error_val.ToString());
                                    break;

                                case 2:  // descending error
                                    MessageBox.Show("DATA ERROR LINE: " + error_row.ToString() + "\r\n" 
                                                    + "DATA ERROR COLOMN: " + error_col.ToString() + "\r\n" 
                                                    + "Not descending" + error_val.ToString());
                                    break;

                                case 3:  // same value error
                                    MessageBox.Show("DATA ERROR LINE: " + error_row.ToString() + "\r\n" 
                                                    + "DATA ERROR COLOMN: " + error_col.ToString() + "\r\n" 
                                                    + "Same value: " + error_val.ToString());
                                    break;

                            }
                            break;
                        }

                        for (int j = 0; j < dataGridView1.RowCount - 1; j++)
                        {
                            if (float.Parse((string)dataGridView1.Rows[j].Cells[i].Value.ToString())
                                > float.Parse((string)dataGridView1.Rows[j + 1].Cells[i].Value.ToString()))
                            {
                                if (checker == false)
                                {
                                    checker = true;
                                    ascending = false;
                                }
                                else if (ascending == true)
                                {
                                    error_occured = 1;
                                    error_row = j;
                                    error_col = i;

                                    error_val = float.Parse((string)dataGridView1.Rows[j + 1].Cells[i].Value);
                                    break;
                                }
                            }
                            else if(float.Parse(dataGridView1.Rows[j].Cells[i].Value.ToString())
                                < float.Parse(dataGridView1.Rows[j + 1].Cells[i].Value.ToString()))
                            {
                                if (checker == false)
                                {
                                    checker = true;
                                    ascending = true;
                                }
                                else if (ascending == false)
                                {
                                    error_occured = 2;
                                    error_row = j;
                                    error_col = i;
                                    error_val = float.Parse((string)dataGridView1.Rows[j + 1].Cells[i].Value);
                                    break;
                                }
                            }
                            else
                            {
                                error_occured = 3;
                                error_row = j;
                                error_col = i;
                                error_val = float.Parse((string)dataGridView1.Rows[j + 1].Cells[i].Value);
                                break;
                            }
                        }
                    }
                    if(error_occured == 0)
                    {
                        MessageBox.Show("CLEAR");
                    }
                }
                else
                {
                    MessageBox.Show("DATA CHECK NOT NEEDED");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void textBox7_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Enter:
                    {
                        add_procedure();
                        break;
                    }
                default:
                    break;
            }
        }

        private void textBox14_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    {
                        try
                        {
                            step_click_procedure();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    {
                        freq_pwr_mea_form_send_event(cmd: "FREQ_PWR_MEA_SET_DAC_VOLTAGE", data: textBox4.Text);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
