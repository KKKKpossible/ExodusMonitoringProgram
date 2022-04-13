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
    public partial class input_pwr_mea_form_6 : Form
    {
        public GetEventHandler input_pwr_mea_form_send_event;

        public input_pwr_mea_form_6()
        {
            InitializeComponent();
            // 아래의 자료형을 사용해서 리턴을 주고 받을 예정이다.
            /*
            public string[] input_pwr_adc = new string[1000];
            public string[] input_pwr_dbm = new string[1000];
            public int input_pwr_numdata = 0;
            */
        }


        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = folderBrowserDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string folderpath = folderBrowserDialog1.SelectedPath;
                textBox1.Text = folderpath;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

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
                        if (cmd == "INPUT_PWR_MEA_VOLTAGE_FETCH")
                        {
                            textBox4.Text = float.Parse(data_arr[0]).ToString("0.000");
                        }
                    });
                }
                else
                {
                    if (cmd == "INPUT_PWR_MEA_VOLTAGE_FETCH")
                    {
                        textBox4.Text = float.Parse(data_arr[0]).ToString("0.000");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == String.Empty)
            {
                MessageBox.Show("Please fill frequency textbox");
            }
            else
            {
                if (dataGridView1.Rows.Count != 0)
                {
                    dataGridView1.Rows.Clear();
                    dataGridView1.Refresh();
                }
                // input_pwr_mea_form_send_event(cmd: "FREQ_PWR_MEA_SET_DAC_VOLTAGE", data: "0");
            }
        }

        void add_click_procedure()
        {

            if (textBox2.Text == String.Empty)
            {
                MessageBox.Show("Please fill frequency textbox");
                return;
            }
            if (textBox5.Text == String.Empty)
            {
                MessageBox.Show("Please fill rf power textbox");
                return;
            }
            dataGridView1.Rows.Add(dataGridView1.Rows.Count.ToString(), textBox4.Text, textBox5.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                add_click_procedure();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); };
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                textBox7.Text = dataGridView1.Rows[dataGridView1.CurrentCellAddress.Y].Cells[0].Value.ToString();
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
                dataGridView1.Rows.RemoveAt(dataGridView1.CurrentCellAddress.Y);
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataGridView1.Rows[i].Cells[0].Value = i;
                }
                textBox7.Text = dataGridView1.Rows[dataGridView1.CurrentCellAddress.Y].Cells[0].Value.ToString();
            }
            catch
            {
                MessageBox.Show("TABLE IS EMPTY");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                int error_occured = 0;
                int error_row = -1;
                int error_col = -1;
                float error_val = 0;

                if (dataGridView1.RowCount > 1)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        bool checker = false;
                        bool ascending = false;

                        if (error_occured != 0)
                        {
                            switch (error_occured)
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
                            if (float.Parse((string)dataGridView1.Rows[j].Cells[i].Value)
                                > float.Parse((string)dataGridView1.Rows[j + 1].Cells[i].Value))
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
                            else if (float.Parse((string)dataGridView1.Rows[j].Cells[i].Value)
                                < float.Parse((string)dataGridView1.Rows[j + 1].Cells[i].Value))
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
                    if (error_occured == 0)
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

        private void button8_Click(object sender, EventArgs e)
        {
            // file line description
            // line 0.. frequency value
            // line 3 * (n - 1) + 0.. number
            // line 3 * (n - 1) + 1.. input adc value
            // line 3 * (n - 1) + 2.. input dbm value
            // ,,,

            try
            {
                if (textBox2.Text == String.Empty)
                {
                    MessageBox.Show("Please fill frequency textbox");
                    return;
                }
                if (textBox5.Text == String.Empty)
                {
                    MessageBox.Show("Please fill rf power textbox");
                    return;
                }
                if (textBox1.Text == string.Empty)
                {
                    MessageBox.Show("Please fill path textbox");
                    return;
                }
                string file_name = textBox1.Text + "\\input" + textBox2.Text + ".txt";
                FileStream fs = File.Create(file_name);
                fs.Close();

                StreamWriter sw = new StreamWriter(file_name);
                sw.WriteLine(textBox2.Text);
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    sw.WriteLine(dataGridView1.Rows[i].Cells[0].Value);
                    sw.WriteLine(dataGridView1.Rows[i].Cells[1].Value);
                    sw.WriteLine(dataGridView1.Rows[i].Cells[2].Value);
                }
                sw.Close();
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
                    string file_name = openFileDialog1.SafeFileName;
                    string file_full_name = openFileDialog1.FileName;
                    string file_path = file_full_name.Replace(file_name, "");

                    while (dataGridView1.RowCount != 0)
                    {
                        dataGridView1.Rows.RemoveAt(0);
                    }
                    dataGridView1.Refresh();

                    StreamReader sr = new StreamReader(file_full_name);

                    string freq = sr.ReadLine();
                    if (freq != null)
                    {
                        textBox2.Text = freq;
                    }

                    string read_buff_no = string.Empty;
                    string read_buff_input = string.Empty;
                    string read_buff_pwr = string.Empty;
                    while ((read_buff_no != null) && (read_buff_input != null) && (read_buff_pwr != null))
                    {
                        read_buff_no = sr.ReadLine();
                        read_buff_input = sr.ReadLine();
                        read_buff_pwr = sr.ReadLine();
                        if ((read_buff_no != null) && (read_buff_input != null) && (read_buff_pwr != null))
                        {
                            dataGridView1.Rows.Add(read_buff_no, read_buff_input, read_buff_pwr);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                input_pwr_mea_form_send_event(cmd: "INPUT_PWR_MEA_VOLTAGE_FETCH");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void input_pwr_mea_form_6_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void input_pwr_mea_form_6_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
        }

        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Enter:
                    {
                        try
                        {
                            add_click_procedure();
                        }
                        catch (Exception ex) { MessageBox.Show(ex.ToString()); };
                        break;
                    }
                default:
                    break;
            }
        }

        private void textBox7_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    {
                        try
                        {
                            dataGridView1.Rows.RemoveAt(dataGridView1.CurrentCellAddress.Y);
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                dataGridView1.Rows[i].Cells[0].Value = i;
                            }
                            textBox7.Text = dataGridView1.Rows[dataGridView1.CurrentCellAddress.Y].Cells[0].Value.ToString();
                        }
                        catch
                        {
                            MessageBox.Show("TABLE IS EMPTY");
                        }
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
