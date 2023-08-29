using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using MySql.Data.MySqlClient;
using System.Threading;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;

namespace BASSCOMPORT
{

    public partial class frrmMain : Form
    {
        String senda,sendb,sendc;
        double x1, x2, x3, y1, y2, y3, a, b, c;
        string dataOUT;
        string sendWith;
        string DataIn;
        int dataINLength;
        int[] dataInDec;
        int analogMode = 0;  // analogMode1 akım çıkışı      analogMode 2  gerilim çıkışı
        StreamWriter objStreamWriter;
        //string pathFile = @"C:\Users\ufuk\Desktop\BASS\SOFTWARE\2-SOFTWARE\BASSCOMPORT\_My Source File\SerialData.txt";
        bool state_AppendText = true;
        string pathFile;
        int unitchoice = 0;
        float k = 1;
        string[] numbers;
        int counter = 0;
        string received;
        string dnsUpper, dnsLower, dnsK;
        MySqlConnection myConnection;
        MySqlCommand myCommand;
        string[] valuesX = new string[8];
        double[] sendkVal = new double[3];

        public static double[] Polynomial(double[] x, double[] y, int order)
        {
            var design = Matrix<double>.Build.Dense(x.Length, order + 1, (i, j) => Math.Pow(x[i], j));
            return MultipleRegression.QR(design, Vector<double>.Build.Dense(y)).ToArray();
        }





        #region my Own Method


        #region Custom EventHandler

        public delegate void UpdateDelegate(object sender, UpdateDataEventArgs args);

        public event UpdateDelegate UpdateDataEventHandler;

        public class UpdateDataEventArgs : EventArgs
        { 
        
        
        }
        protected void RefreshDataGridViewForm2()
        {
            UpdateDataEventArgs args = new UpdateDataEventArgs();
            UpdateDataEventHandler.Invoke(this,args);
        }


        #endregion

        #region RX Data Format
        private string RxDataFormat(int[] dataInput)
        {

            string strOut = " ";


            if (toolStripComboBox4.Text == "Char")
            {
                foreach (int element in dataInput)
                {
                    strOut += Convert.ToChar(element);
                }
            }

       

            
            return strOut;

        }
        #endregion


        #endregion
        #region GUI Method

        public frrmMain()
        {
            Thread t = new Thread(new ThreadStart(StartForm));
            t.Start();
            Thread.Sleep(3500);
            InitializeComponent();
            t.Abort();
        }

        public void StartForm()
        {

            Application.Run(new splashScreen());
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //for time 
           // timer1.Start();

            cBoxBaudRate.SelectedIndex = 4;
            


         
            btnOpen.Enabled = true;
            btnClose.Enabled = false;

            chBoxDTREnable.Checked = false; //FOR İNİTİALİZE
            serialPort1.DtrEnable = false;
            chBoxRTSEnable.Checked = false; //FOR İNİTİALİZE
            serialPort1.RtsEnable = false;
            btnSendData.Enabled = true;
            sendWith = "Both";


            toolStripComboBox3.Text = "BUTTON";
            toolStripComboBox1.Text = "Add to Old Data";
            toolStripComboBox2.Text = "Both";

            toolStripComboBox_appendOrOverwriteText.Text = "Append Text";
            toolStripComboBox_writeLineOrwriteText.Text = "WriteLine";


            //to get new project directory
            pathFile = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            pathFile += @"\_My Source File\SerialData.txt";

            //Console.WriteLine("==== Below is The Result===");
            //Console.WriteLine(pathFile);
            // C:\Users\ufuk\Desktop\BASS\SOFTWARE\2-SOFTWARE\BASSCOMPORT\_My Source File


            saveToToolStripMenuItem.Checked = false;
            saveToMYSQLToolStripMenuItem.Checked = false;




            toolStripComboBox4.Text = "Char";



        }
        private void oPENToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                serialPort1.PortName = cBoxCOMPORT.Text;
                serialPort1.BaudRate = Convert.ToInt32(cBoxBaudRate.Text);
                serialPort1.DataBits = Convert.ToInt32(cBoxDataBits.Text);
                serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text);
                serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), cBoxParity.Text);
                //serialPort1.Handshake = Handshake.None;

                serialPort1.Open();                            
                progressBar1.Value = 100;
                btnOpen.Enabled = false;
                btnClose.Enabled = true;
                lblStatusCom.Text = "ON";

                
                

            }

            catch (Exception err)
            {

                MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnOpen.Enabled = true;
                btnClose.Enabled = false;
                lblStatusCom.Text = "OFF";
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {

            try
            {
              
                serialPort1.PortName = cBoxCOMPORT.Text;
                serialPort1.BaudRate = Convert.ToInt32(cBoxBaudRate.Text);
                serialPort1.DataBits = Convert.ToInt32(cBoxDataBits.Text);
                serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text);
                serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), cBoxParity.Text);
                serialPort1.Open();
               
                progressBar1.Value = 100;
                label15.Text = "OPENED";
                label15.Visible = true;
                label15.BackColor = Color.Green;
                progressBar1.BackColor= Color.Green;
                btnOpen.Enabled = false;
                btnClose.Enabled = true;
                lblStatusCom.Text = "ON";
                serialPort1.WriteLine("uf" + "*" + "1" + "*" + "1" + "*"); // receive all default values from pico

            }

            catch(Exception err)
            {

                MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnOpen.Enabled = true;
                btnClose.Enabled = false;
                lblStatusCom.Text = "OFF";
            }
            
        }
        private void cLOSEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                progressBar1.Value = 0;
                btnOpen.Enabled = true;
                btnClose.Enabled = false;
                lblStatusCom.Text = "OFF";
            }
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("uf" + "*" + "4" + "*" + "1" + "*");
                serialPort1.Close();
                progressBar1.Value = 0;
                btnOpen.Enabled = true;
                btnClose.Enabled = false;
                lblStatusCom.Text = "OFF";
                label15.Text = "CLOSED";
                label15.BackColor = Color.Red;
                
            }

            checkBox2.Checked = false;
            checkBox1.Checked = false;
            if (tBoxDataIN.Text != "")
            {
                tBoxDataIN.Text = "";
            }
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
               dataOUT = tBoxDataOut.Text;

                if (sendWith == "None")
                {
                    //serialPort1.Write(dataOUT);
                    serialPort1.Write(new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x0A, 0xC5, 0xCD  }, 0, 8);
                }
                else if (sendWith== "Both") 
                {
                    // serialPort1.Write(dataOUT + "\r\n");
                    serialPort1.Write(new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x0A, 0xC5, 0xCD }, 0, 8);
                }
                else if (sendWith == "New Line")
                {
                    //serialPort1.Write(dataOUT + "\n");
                    serialPort1.Write(new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x0A, 0xC5, 0xCD }, 0, 8);
                }
                else if (sendWith == "Carriage Return")
                {
                    // serialPort1.Write(dataOUT + "\r");
                    serialPort1.Write(new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x0A, 0xC5, 0xCD }, 0, 8);
                }

            }
        }

        private void toolStripComboBox2_DropDownClosed(object sender, EventArgs e)
        {
            // None
            //Both
            //New Line
            //Carriage Return
            if (toolStripComboBox2.Text == "None")
            {
                sendWith = "None";
            }
            else if (toolStripComboBox2.Text == "Both")
            {
                sendWith = "Both";
            }
            else if (toolStripComboBox2.Text == "New Line")
            {
                sendWith = "New Line";
            }
            else if (toolStripComboBox2.Text == "Carriage Return")
            {
                sendWith = "Carriage Return";
            }
        }

        private void tBoxDataOut_TextChanged(object sender, EventArgs e)
        {
            int dataOUTLEnght= tBoxDataOut.TextLength;
            lblDataOutLength.Text = string.Format("{0:00}", dataOUTLEnght);// keep string format 2 digit



        }

    

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void chBoxDTREnable_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxDTREnable.Checked)
            {
                serialPort1.DtrEnable = true;
                MessageBox.Show("DTR Enabled", "Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
            else 
            {
                serialPort1.DtrEnable = false;
            }

        }

        private void chBoxRTSEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxRTSEnable.Checked)
            {
                serialPort1.RtsEnable = true;
                MessageBox.Show("RTS Enabled", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                serialPort1.RtsEnable = false;
            }
        }
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tBoxDataOut.Text != "")
            {
                tBoxDataOut.Text = "";
            }
        }
        private void btnClearDataOut_Click(object sender, EventArgs e)
        {
            if (tBoxDataIN.Text != "")
            {
                tBoxDataIN.Text = "";
            }
        }

        private void tBoxDataOut_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.Enter)
            {
                this.doSomething();
                e.Handled = true;
                e.SuppressKeyPress = true;
              
            }
            
        }

        private void doSomething()
        {
            if (serialPort1.IsOpen)
            {
                dataOUT = tBoxDataOut.Text;
                if (sendWith == "None")
                {
                    serialPort1.Write(dataOUT);
                }
                else if (sendWith == "Both")
                {
                    serialPort1.Write(dataOUT + "\r\n");
                }
                else if (sendWith == "New Line")
                {
                    serialPort1.Write(dataOUT + "\n");
                }
                else if (sendWith == "Carriage Return")
                {
                    serialPort1.Write(dataOUT + "\r");
                }                               
            }
        }

        private void chBoxWriteLine_ChangeUICues(object sender, UICuesEventArgs e)
        {

        }

        private void groupBox9_Enter(object sender, EventArgs e)
        {

        }

        private void label7_Click_1(object sender, EventArgs e)
        {

        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            List<int> dataBuffer = new List<int>();
            
            while (serialPort1.BytesToRead > 0)
            {
                try 
                {
                    dataBuffer.Add(serialPort1.ReadByte());
                }

                catch(Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }

            dataINLength = dataBuffer.Count();
            dataInDec = new int[dataINLength];
            dataInDec = dataBuffer.ToArray();

            this.Invoke(new EventHandler(ShowData)); //this method show the data in textbox
    
        }

      

        private void ShowData(object sender, EventArgs e)
        {

            //int dataINLength = DataIn.Length;

            DataIn = RxDataFormat(dataInDec) ;

            if (DataIn[1] == '*')
            {
                List<string> s = new List<string>(
                DataIn.Split(new string[] { "*" }, StringSplitOptions.None));

                if(s[1].Equals("0"))
                {
                    checkBox1.Checked = true;
                    checkBox2.Checked = false;
                }
                else if (s[1].Equals("1"))
                {
                    checkBox2.Checked = true;
                    checkBox1.Checked = false;
                }

                numericUpDown1.Text = s[2];
                numericUpDown2.Text = s[3];

                
                tBupper.Text = s[5];
                tBlower.Text = s[6];
                tBKFactor.Text = s[7];
                tBfilterDegree.Text = s[8];
                tBfilterSample.Text = s[9];


            }

            else
            {
                lblDataInLength.Text = String.Format("{0:00}", dataINLength);

                if (toolStripComboBox1.Text == "Always Update")
                {
                    tBoxDataIN.Text = DataIn;

                }
                else if (toolStripComboBox1.Text == "Add to Old Data")
                {
                    if (toolStripComboBox3.Text == "TOP")
                    {
                        tBoxDataIN.Text = tBoxDataIN.Text.Insert(0, DataIn);
                    }
                    else if (toolStripComboBox3.Text == "BUTTON")
                    {
                        tBoxDataIN.Text += DataIn;
                        received += DataIn;

                    }



                }
                tBoxDataIN.Text = tBoxDataIN.Text.Replace("\0", string.Empty);
                string[] values = tBoxDataIN.Text.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                string[] values2 = tBoxDataIN.Text.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
            }


        }




        private void clearToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (tBoxDataIN.Text != "")
            {
                tBoxDataIN.Text = "";
            }
        }
        private void btnClearDataIN_Click(object sender, EventArgs e)
        {
            if (tBoxDataIN.Text != "")
            {
                tBoxDataIN.Text = "";
            }
        }

        private void lblDataInLength_Click(object sender, EventArgs e)
        {

        }

        private void chBoxDTREnable_ChangeUICues(object sender, UICuesEventArgs e)
        {

        }

        private void cOMToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created by Ufuk Simsek", "Creator", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void endLineToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
 
        }

        private void toolStripComboBox_appendOrOverwriteText_DropDownClosed(object sender, EventArgs e)
        {
            if (toolStripComboBox_appendOrOverwriteText.Text == "Append Text")
            {
                state_AppendText = true;

            }

            else
            {
                state_AppendText = false;
            }
        }

        private void toolStripComboBox_writeLineOrwriteText_Click(object sender, EventArgs e)
        {

        }


        private void showDataToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form2 objForm2 = new Form2(this);
            objForm2.Show();
        }

        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            label8.Text = DateTime.Now.ToLongTimeString();
            label9.Text = DateTime.Now.ToLongDateString();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void cBoxCOMPORT_DropDown(object sender, EventArgs e)
        {

            string[] ports = SerialPort.GetPortNames();
            cBoxCOMPORT.Items.Clear();
            cBoxCOMPORT.Items.AddRange(ports);

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                dataOUT = tBoxDataOut.Text;
                //serialPort1.Write(dataOUT);
                
                serialPort1.WriteLine("uf"+ "*" + tBupper.Text + "*"+ tBlower.Text + "*" );
                tBupper.Clear();
                tBlower.Clear();
         

                tBoxDataIN.Text += "\r\n";  //gelen yeni veriyi new line da görmek için

            }
        }

        private void tBoxDataOut_KeyDown_1(object sender, KeyEventArgs e)
        {

        }

        private void tBoxDataIN_TextChanged(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void toolStripComboBox3_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
           
            if(serialPort1.IsOpen)
            {
                serialPort1.WriteLine("uf" + "*" + "B" + "*" + numericUpDown1.Text + "*");
                //tBoxDataIN.Text += "\r\n";
            }
         


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true || checkBox2.Checked == true)
            {
                if (serialPort1.IsOpen)
                {
                    dataOUT = tBoxDataOut.Text;
                    //serialPort1.Write(dataOUT);
                    serialPort1.WriteLine("uf" + "*" + "9" + "*" + numericUpDown1.Text + "*");  // save lower value to eeprom
                    numericUpDown1.BackColor = Color.Green;
                    //tBupper.Clear();
                    //tBlower.Clear();


                    tBoxDataIN.Text += "\r\n";  //gelen yeni veriyi new line da görmek için

                }

                else
                {
                    MessageBox.Show("OPEN COM PORT", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }




            }
            
            else
            {

                MessageBox.Show("Choose mA or Volt Output", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
                if (serialPort1.IsOpen)
                {
                try
                {
                    valuesX[0] = received.Substring("analog value\r\n", " \r\nmax analog"); //min analog
                    valuesX[1] = received.Substring("\r\nmax analog value\r\n", " \r\nmax pulse"); // max analog 
                    valuesX[2] = received.Substring("value\r\n", " \r\nmin"); // max pulse 
                    valuesX[3] = received.Substring("pulse value\r\n", " \r\nperiod"); // min pulse
                    valuesX[4] = received.Substring("\r\nperiod value\r\n", " \r\npulse"); // filter degree
                    valuesX[5] = received.Substring("\r\npulse out value\r\n", " \r\nk");  // pulse out value
                    valuesX[6] = received.Substring("\r\nk value\r\n", " \r\n");  // pulse out value

                    tBupper.Text=valuesX[2];
                    tBlower.Text=valuesX[3];
                    tBfilterDegree.Text=valuesX[4];
                    tBKFactor.Text=valuesX[5];
                    numericUpDown1.Value = Decimal.Parse(valuesX[0]);
                    numericUpDown2.Value = Decimal.Parse(valuesX[1]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }





                if (checkBox1.Checked == true)
                {
                    if (tBoxDataIN.Text != "")
                    {
                        tBoxDataIN.Text = "";
                    }

                    label14.Text = "4 mA";
                    label17.Text = "20 mA";
                    analogMode = 1;
                    serialPort1.WriteLine("uf" + "*" + "2" + "*" + "1" + "*"); // analog mode 4 20 mA

                    tBoxDataIN.Text += "\r\n";

                }

                else if (checkBox1.Checked == false)
                {
                    label14.Text = "NONE";
                    label17.Text = "NONE";
                    analogMode = 5;

                }


            }

                else
            {
                checkBox1.Checked = false;
                MessageBox.Show("OPEN COM PORT", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }







        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
          

            if (serialPort1.IsOpen)
            {



                if (checkBox2.Checked == true)
                {
                    if (tBoxDataIN.Text != "")
                    {
                        tBoxDataIN.Text = "";
                    }

                    label14.Text = "0 Volt";
                    label17.Text = "10 Volt";
                    analogMode = 2;
                    serialPort1.WriteLine("uf" + "*" + "3" + "*" + "2" + "*"); // analog mode 0-10 volt
                    tBoxDataIN.Text += "\r\n";
                }

                else if (checkBox2.Checked == false)
                {
                    label14.Text = "NONE";
                    label17.Text = "NONE";

                }
            }
            else
            {
                checkBox2.Checked = false;
                MessageBox.Show("OPEN COM PORT", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
           


        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("uf" + "*" + "C" + "*" + numericUpDown2.Text + "*");
                //tBoxDataIN.Text += "\r\n";
            }

          

        }

        private void button3_Click(object sender, EventArgs e)
        {


            if (serialPort1.IsOpen)
            {

                serialPort1.WriteLine("uf" + "*" + "A" + "*" + numericUpDown2.Text + "*"); // save upper value to eeprom
                numericUpDown2.BackColor = Color.Green;
                tBoxDataIN.Text += "\r\n";
            }

            else
            {
                MessageBox.Show("OPEN COM PORT", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void tBupper_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(tBupper.Text, "[^0-9]"))
            {
                MessageBox.Show("Please enter only numbers.");
                tBupper.Text = tBupper.Text.Remove(tBupper.Text.Length - 1);
            }
        }

        private void tBlower_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(tBlower.Text, "[^0-9]"))
            {
                MessageBox.Show("Please enter only numbers.");
                tBlower.Text = tBlower.Text.Remove(tBlower.Text.Length - 1);
            }
        }



        private void tBKFactor_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                String sendUpper;
                sendUpper = tBupper.Text;
                serialPort1.WriteLine("uf" + "*" + "M" + "*" + sendUpper + "*");
                tBupper.BackColor = Color.Green;
                tBoxDataIN.Text += "\r\n";
            }

            else
            {
                MessageBox.Show("OPEN COM PORT", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                tBfilterDegree.Text = tBfilterDegree.Text.Replace(',', '.');
                serialPort1.WriteLine("uf" + "*" + "7" + "*" + tBfilterDegree.Text + "*");
                tBfilterDegree.BackColor = Color.Green;
                tBoxDataIN.Text += "\r\n";

            }

            else
            {
                MessageBox.Show("OPEN COM PORT", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                tBfilterSample.Text = tBfilterSample.Text.Replace(',', '.');
                serialPort1.WriteLine("uf" + "*" + "8" + "*" + tBfilterSample.Text + "*");
                tBfilterSample.BackColor = Color.Green;
                tBoxDataIN.Text += "\r\n";
            }

            else
            {
                MessageBox.Show("OPEN COMPORT", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox3.Checked == true)
            {
                tBupper.Focus();
                groupBox2.Enabled = false;
            }
            else
                groupBox2.Enabled = true;

        }

        private void numericUpDown1_MouseClick(object sender, MouseEventArgs e)
        {
            numericUpDown1.BackColor = Color.Yellow;
        }

        private void numericUpDown1_Leave(object sender, EventArgs e)
        {
            numericUpDown1.BackColor = Color.White;
        }

        private void numericUpDown2_MouseClick(object sender, MouseEventArgs e)
        {
            numericUpDown2.BackColor = Color.Yellow;
        }

        private void numericUpDown2_Leave(object sender, EventArgs e)
        {
            numericUpDown2.BackColor = Color.White;
        }

        private void tBupper_MouseClick(object sender, MouseEventArgs e)
        {
            tBupper.BackColor = Color.Yellow;
        }

        private void tBupper_Leave(object sender, EventArgs e)
        {
            tBupper.BackColor = Color.White;
        }

        private void tBlower_MouseClick(object sender, MouseEventArgs e)
        {
            tBlower.BackColor = Color.Yellow;
        }



        private void tBKFactor_MouseClick(object sender, MouseEventArgs e)
        {
            tBKFactor.BackColor = Color.Yellow;
        }



        private void tBlower_Leave_1(object sender, EventArgs e)
        {
            tBlower.BackColor = Color.White;
        }

    

    

        private void textBox2_Click(object sender, EventArgs e)
        {
            tBfilterDegree.BackColor = Color.Yellow;
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            tBfilterDegree.BackColor = Color.White;
        }

        private void tBfilterSample_MouseClick(object sender, MouseEventArgs e)
        {
            tBfilterSample.BackColor = Color.Yellow;
        }

        private void tBfilterSample_Leave(object sender, EventArgs e)
        {
            tBfilterSample.BackColor = Color.White;
        }

        private void serialPort1_PinChanged(object sender, SerialPinChangedEventArgs e)
        {

        }

        private void tBfilterDegree_TextChanged(object sender, EventArgs e)
        {

        }

        private void tBfilterSample_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                tBfilterDegree.Text = tBfilterDegree.Text.Replace(',', '.');
                serialPort1.WriteLine("uf" + "*" + "X" + "*" + tBfilterDegree.Text + "*");
                tBfilterDegree.BackColor = Color.Green;
                tBoxDataIN.Text += "\r\n";

                numericUpDown1.BackColor = Color.White;
                numericUpDown2.BackColor = Color.White;
                tBupper.BackColor = Color.White;
                tBlower.BackColor = Color.White;
                tBKFactor.BackColor = Color.White;
                tBfilterDegree.BackColor = Color.White;
                tBfilterSample.BackColor = Color.White;
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                unit0.Checked = false;
                unit1.Checked = false;
                //button2.BackColor = Color.Green;

            }

            if (tBoxDataIN.Text != "")
            {
                tBoxDataIN.Text = "";
            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void unit1_CheckedChanged(object sender, EventArgs e)
        {

            
            if (unit1.Checked)
            {

               serialPort1.WriteLine("uf" + "*" + "L" + "*" + "1" + "*"); // lt/hour secildi

            }



        }

        private void unit0_CheckedChanged(object sender, EventArgs e)
        {

            if (unit0.Checked)
            {

                serialPort1.WriteLine("uf" + "*" + "K" + "*" + "1" + "*"); // lt/min secildi

            }


        }





        private void tBKFactor_Leave(object sender, EventArgs e)
        {
            tBKFactor.BackColor = Color.White;
        }

        private void checkBox4_CheckedChanged_1(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true)
            {
                tBupper.Focus();
                groupBox3.Enabled = false;
            }
            else
                groupBox3.Enabled = true;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked == true)
            {
                tBupper.Focus();
                groupBox4.Enabled = false;
            }
            else
                groupBox4.Enabled = true;
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen && checkBox9.Checked)
            {
                serialPort1.WriteLine("uf" + "*" + "W" + "*" + senda + "*");
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen && checkBox6.Checked)
            {
                serialPort1.WriteLine("uf" + "*" + "W" + "*" + senda + "*");
            }

        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen && checkBox7.Checked)
            {
                serialPort1.WriteLine("uf" + "*" + "Y" + "*" + senda + "*");
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine("uf" + "*" + "V" + "*" + senda + "*");
        }

        private void tBfilterDegree_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            serialPort1.WriteLine("uf" + "*" + "U" + "*" + "" + "*");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                String  sendLower;
                sendLower = tBlower.Text;
                serialPort1.WriteLine("uf" + "*" + "N" + "*" + sendLower + "*");
                tBlower.BackColor = Color.Green;
                tBoxDataIN.Text += "\r\n";
            }

            else
            {
                MessageBox.Show("OPEN COM PORT", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                String sendK;
                float sendKF;
                sendK = tBKFactor.Text;
                sendKF=float.Parse(sendK)*100;
                sendK=sendKF.ToString();
                serialPort1.WriteLine("uf" + "*" + "J" + "*" + sendK + "*");
                tBKFactor.BackColor = Color.Green;
                tBoxDataIN.Text += "\r\n";
            }
            else
            {
                MessageBox.Show("OPEN COM PORT", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void label21_Click_1(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                
            }

            else
            {
                MessageBox.Show("OPEN COMPORT", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked == true)
            {
                tBupper.Focus();
                groupBox5.Enabled = true;
            }
            else
                groupBox5.Enabled = false;
        }

        private void textBox1_TextChanged_2(object sender, EventArgs e)
        {

        }
        private void button11_Click_1(object sender, EventArgs e)
        {
            
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            
        }

        private void cBoxBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }

}
