using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;

namespace ComHelper
{
    public partial class Form11 : Form
    {
    //    private JustinIO.CommPort mycom1 = new JustinIO.CommPort();

        private byte[] recb;
        private SerialPort com1=new SerialPort();
        private void WriteLog(string sLog)
        {
            lstLog.Items.Add(DateTime.Now.ToLocalTime() + ": " + sLog);
            lstLog.SelectedIndex = lstLog.Items.Count - 1;
        }
        private delegate void singleParamDelegage(string sParam);
        private delegate void setControlDisplayDelegage(TextBox objCtl,string sText);
 
        public Form11()
        {
            InitializeComponent();
            //上电初始化
            //mycom1.PortNum = "COM8";
            //mycom1.BaudRate = 9600;
            //mycom1.ByteSize = 8;
            //mycom1.Parity = 1;
            //mycom1.StopBits = 0;
            //name
            //userInit();
           // FlowLayoutPanel.
            // Button btn =this.flowRadioButtonPanel.GetChildAtPoint();
        }

        private  void userInit(){

            tx_rx_flag = TX_FLAG;//初始化为tx
            radioButton42.Checked = true;

            String[] portName = System.IO.Ports.SerialPort.GetPortNames();

            foreach (String name in portName)
            {
                t_port.Items.Add(name);
            }
            //baudrate
            String[] baudRate = { "4800", "9600", "19200", "52700", "115200" };
            foreach (String baud in baudRate)
            {
                t_rate.Items.Add(baud);
            }
            //datalen
            String[] dataLen = { "5", "6", "7", "8" };
            foreach (String len in dataLen)
            {
                t_bytesize.Items.Add(len);
            }
            //stopbyte
            String[] stopByte = { "1", "1.5", "2" };
            foreach (String stop in stopByte)
            {
                t_stopbyte.Items.Add(stop);
            }
            //parity
            String[] prity = { "no", "odd", "even", "mark", "space" };
            foreach (String p in prity)
            {
                t_parity.Items.Add(p);
            }

        
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (com1.IsOpen)
            {
                com1.Close();
            }
            com1 = new SerialPort();
            try { 
            
            
            com1.PortName = t_port.Text; //1,2,3,4
            com1.BaudRate = Convert.ToInt32(t_rate.Text); //1200,2400,4800,9600,115200
            com1.DataBits = Convert.ToByte(t_bytesize.Text, 10); //8 bits
            String strprity = null;
            if (t_parity.Text.Equals("no")) { strprity = "0"; }
            if (t_parity.Text.Equals("odd")) { strprity = "1"; }
            if (t_parity.Text.Equals("even")) { strprity = "2"; }
            if (t_parity.Text.Equals("mark")) { strprity = "3"; }
            if (t_parity.Text.Equals("space")) { strprity = "4"; }
            com1.Parity = (Parity)Convert.ToByte(strprity, 10); // 0-4=no,odd,even,mark,space
            com1.StopBits = (StopBits)Convert.ToByte(t_stopbyte.Text, 10); // 0,1,2 = 1, 1.5, 2 
            com1.DataReceived += new SerialDataReceivedEventHandler(com1_DataReceived);
            //iTimeout = 3;
              }
            catch(System.ArgumentNullException ex){
                WriteLog("open again...");
            }
            try
            {
                com1.Open();
                ;//发送AA给下位机
                WriteLog("connecting...");
                Byte[] txHead = { Convert.ToByte("AA", 16)};
                com1.Write(txHead, 0, 1);
            }
            catch (Exception ex) { 
                WriteLog("fail to initiate    #"+com1.PortName+"::"+com1.BaudRate.ToString());
            }
        }

        void com1_DataReceived(object sender, SerialDataReceivedEventArgs e)

        {
                int  getByte = com1.ReadByte();

                if (keyValue == getByte)
            {


                if (this.InvokeRequired)
                {
                    Invoke(new singleParamDelegage(WriteLog), "success to recieve  #" + keyValue);
                }
                else
                {
                    WriteLog("successful");
                }

            }
                //170 = 0xaa
                else if (getByte == 170) {

                    if (this.InvokeRequired)
                    {
                        Invoke(new singleParamDelegage(WriteLog), "success to connect  #" + com1.PortName);
                    }
                    else
                    {
                        WriteLog("connect successfully");
                    }

                
                }
                else if(getByte == RX_FLAG){
                     if (this.InvokeRequired)
                    {
                        Invoke(new singleParamDelegage(WriteLog), "set RX" );
                    }

                }
                else if(getByte == TX_FLAG){
                     if (this.InvokeRequired)
                    {
                        Invoke(new singleParamDelegage(WriteLog), "set TX " );
                    }
                
                }



                else
                {

                    if (this.InvokeRequired)
                    {
                        Invoke(new singleParamDelegage(WriteLog), "recieve #" +"Dec:"+getByte+"(0x"+ToHexString(new byte[]{(byte)getByte})+")");
                    }
                    else
                    {
                        WriteLog("fail");
                    }


                }
            //System.Threading.Thread.Sleep(100);
            //System.IO.Ports.SerialPort com = (System.IO.Ports.SerialPort)sender;
            //byte[] buffer = new byte[com.BytesToRead];
            //com.Read(buffer, 0, buffer.Length);
            //parseUploadMsg(buffer);

            //string sLog = "接收到数据包(" +buffer.Length + ")：" + dis_package(buffer);
            //if (this.InvokeRequired)
            //{
            //    Invoke(new singleParamDelegage(WriteLog), sLog);
            //}else
            //{
            //    WriteLog(sLog);
            //}

            //if (this.InvokeRequired)
            //{
            //    Invoke(new singleParamDelegage(WriteLog), "接收中");
            //}
            //else
            //{
            //    WriteLog("接收中");
            //}

        }

        void setText(TextBox objCtl, string sText) 
        {
            objCtl.Text = sText;
        }


        void parseUploadMsg(byte[] bBuffer)
        {
            if (bBuffer[0] != 0xCA) return;

            int iLen = bBuffer[1] + 3;
            if (bBuffer.Length != iLen) return;

            switch (bBuffer[2]) { 
                case 0x10:
                    byte bSyncUsrId = bBuffer[3];
                    string sLog = "收到匹配用户信息:" + bSyncUsrId;
                   
                    byte[] b1 = mysendb(txtLock1.Text);
                    b1[11] = bSyncUsrId;
                    b1[b1.Length - 2] = getDatasXor(b1, 6, b1.Length - 3);
                    string s1= ToHexString(b1);
                    //txtLock1.Text = ToHexString(b1);


                    if (this.InvokeRequired)
                    {
                        Invoke(new singleParamDelegage(WriteLog), sLog);
                        Invoke(new setControlDisplayDelegage(setText), new object[] { txtLock1, s1 });
                    }
                    else
                    {
                        WriteLog(sLog);
                        setText(txtLock1, s1);
                    }

                    break;
            }


        }



        // private void WriteLog(string sLog)
        // {
        //     lstLog.Items.Add(DateTime.Now.ToLocalTime()+": "+sLog);
        //     lstLog.SelectedIndex = lstLog.Items.Count - 1;
        // }

        public string delspace(string putin)
        {
            string putout = "";
            for (int i = 0; i < putin.Length; i++)
            {
                if (putin[i] != ' ')
                    putout += putin[i];
            }
            return putout;
        }

        public byte[] mysendb(string sData)
        {
            string temps = delspace(sData);
            byte[] tempb = new byte[80];
            int j = 0;
            for (int i = 0; i < temps.Length; i = i + 2, j++)
                tempb[j] = Convert.ToByte(temps.Substring(i, 2), 16);
            byte[] send = new byte[j];
            Array.Copy(tempb, send, j);
            return send;
        }

        //显示包信息
        public string dis_package(byte[] reb)
        {
            string temp = "";
            foreach (byte b in reb)
                temp += b.ToString("X2") + " ";
            return temp;
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (!com1.IsOpen)
            {
                //MessageBox.Show("请先打开串口!");
                WriteLog("please open serialPort firstly.");
                return;
            }
            //-------------------------发送数据-------------------------------------
            try {

             Byte[] txData = { (byte)keyValue };
             Byte[] tx_rx = { (byte)tx_rx_flag };
          
             com1.Write(tx_rx, 0, 1);
             com1.Write(txData, 0, 1);

             WriteLog("          send      #" + keyValue.ToString());
             
            }
            catch (Exception ex) { 
            
            }



            ////-------------------------发送数据-------------------------------------

            //if (radioButton1.Checked) {
            //    ;//发送Chanel 0 数据
            //    try
            //    {
            //        Byte[] txData = { 0, 0, 0};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }
            //}
            //else if (radioButton2.Checked) {
            //    ;//发送Chanel 1 数据
            //     try
            //    {
            //        Byte[] txData = { 1, 1, 1};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");

            //    }
           
            //}
            //else if (radioButton3.Checked) {
            //    ;//发送Chanel 2 数据
            //     try
            //    {
            //        Byte[] txData = { 2, 2, 2};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }
           
            //}
            //else if (radioButton4.Checked)
            //{
            //    ;//发送Chanel 3 数据
            //     try
            //    {
            //        Byte[] txData = { 3, 3, 3};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}
            //else if (radioButton5.Checked)
            //{
            //    ;//发送Chanel 4 数据
            //     try
            //    {
            //        Byte[] txData = { 0, 0, 0};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}

            // else if (radioButton6.Checked)
            //{
            //    ;//发送Chanel 4 数据
            //     try
            //    {
            //        Byte[] txData = { 5, 5, 5};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}
            //else if (radioButton7.Checked)
            //{
            //    ;//发送Chanel 4 数据
            //     try
            //    {
            //        Byte[] txData = { 6, 6, 6};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}
            //else if (radioButton8.Checked)
            //{
            //    ;//发送Chanel 4 数据
            //     try
            //    {
            //        Byte[] txData = { 7, 7, 7};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}
            //else if (radioButton9.Checked)
            //{
            //    ;//发送Chanel 4 数据
            //     try
            //    {
            //        Byte[] txData = { 8, 8, 8};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}
            //else if (radioButton10.Checked)
            //{
            //    ;//发送Chanel 4 数据
            //     try
            //    {
            //        Byte[] txData = { 9, 9, 9};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}
            //else if (radioButton11.Checked)
            //{
            //    ;//发送Chanel 4 数据
            //     try
            //    {
            //        Byte[] txData = { 10, 10, 10};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}
            //else if (radioButton12.Checked)
            //{
            //    ;//发送Chanel 4 数据
            //     try
            //    {
            //        Byte[] txData = { 11, 11, 11};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}

            //else if (radioButton13.Checked)
            //{
            //    ;//发送Chanel 4 数据
            //     try
            //    {
            //        Byte[] txData = { 12, 12, 12};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}
            //else if (radioButton14.Checked)
            //{
            //    ;//发送Chanel 4 数据
            //     try
            //    {
            //        Byte[] txData = { 13, 13, 13};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}
            //else if (radioButton15.Checked)
            //{
            //    ;//发送Chanel 4 数据
            //     try
            //    {
            //        Byte[] txData = { 14, 14, 14};
            //        com1.Write(txData , 0, 3);
            //        WriteLog("send successful.");
            //    }
            //    catch (Exception ex){

            //        WriteLog(" fail.");
                
            //    }

            //}

            //----------------------------------------------------------------------
            //if (radioButton53.Checked)
            //{
            //    sData = txtNonLock.Text.Trim();
            //}
            //else if (radioButton50.Checked)
            //{
            //    int iUtc = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            //    byte[] bUtc = BitConverter.GetBytes(iUtc);

            //    byte[] b1 = mysendb(txtLock.Text);
            //    b1[7] = bUtc[3];
            //    b1[8] = bUtc[2];
            //    b1[9] = bUtc[1];
            //    b1[10] = bUtc[0];
            //    b1[b1.Length - 2] = getDatasXor(b1, 6, b1.Length - 3);
            //    txtLock.Text = ToHexString(b1);

            //    sData = txtLock.Text.Trim();
            //}
            //else if (radioButton51.Checked)
            //{
                
            //    int iUtc = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            //    byte[] bUtc = BitConverter.GetBytes(iUtc);

            //    byte[] b1 = mysendb(txtLock1.Text);
            //    b1[7] = bUtc[3];
            //    b1[8] = bUtc[2];
            //    b1[9] = bUtc[1];
            //    b1[10] = bUtc[0];
            //    b1[b1.Length - 2] = getDatasXor(b1, 6, b1.Length - 3);
            //    txtLock1.Text = ToHexString(b1);

            //    sData = txtLock1.Text.Trim();
            //}
            //else
            //{
            //    sData = txtSleep.Text.Trim();
            //}

            //byte[] temp1 = mysendb(sData);
            //com1.Write(temp1, 0, temp1.Length);
            //WriteLog("发送数据：" + dis_package(temp1));

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (com1.IsOpen)
            {
                com1.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //byte[] b1 = new byte[] { 0xca, 0x0a, 0x10, 0x03, 0x56, 0x0a, 0x05, 0x8c, 0x80, 0x10, 0xB2, 0x00, 0x23 };
            lstLog.Items.Clear();            

        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            if (txtWeight.Text.Length == 0 || txtFat.Text.Length == 0) 
            {
                MessageBox.Show("请输入参数!");
                return;
            }

            UInt16 iWeight = UInt16.Parse(txtWeight.Text);
            UInt16 iFat = UInt16.Parse(txtFat.Text);

            byte[] bWeight = BitConverter.GetBytes(iWeight);
            byte[] bFat = BitConverter.GetBytes(iFat);
            byte[] b1 = mysendb(txtNonLock.Text);
            b1[11] = bWeight[1];
            b1[12] = bWeight[0];
            b1[b1.Length - 2] = getDatasXor(b1, 7, b1.Length - 3);
            txtNonLock.Text = ToHexString(b1);

            b1 = mysendb(txtLock.Text);
            b1[12] = bWeight[1];
            b1[13] = bWeight[0];
            b1[b1.Length - 2] = getDatasXor(b1, 6, b1.Length - 3);
            txtLock.Text = ToHexString(b1);

            b1 = mysendb(txtLock1.Text);
            b1[12] = bWeight[1];
            b1[13] = bWeight[0];
            b1[14] = bFat[1];
            b1[15] = bFat[0];
            b1[b1.Length - 2] = getDatasXor(b1, 6, b1.Length - 3);
            txtLock1.Text = ToHexString(b1);

        }


        public byte getDatasXor(byte[] src, int istart, int iend)
        {

            byte dataCheckByte = src[istart];
            for (byte i = (byte)(istart + 1); i <= iend; i++)
            {
                dataCheckByte ^= src[i];
            }
            return dataCheckByte;
        }

        public string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
        {
            string hexString = string.Empty;

            if (bytes != null)
            {

                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {

                    strB.Append(bytes[i].ToString("X2"));
                    if (i < bytes.Length - 1)
                    {
                        strB.Append(" ");
                    }

                }

                hexString = strB.ToString();

            } return hexString;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            txtWeight.Text = "569";
            txtFat.Text = "40";
            txtNonLock.Text = "10 00 00 C5 14 96 CA 11 0F 00 01 02 39 55 DB BC 5A 00 00 00 00 00 00 00 4c 8D";
            txtLock.Text = "10 00 00 C5 15 96 01 00 00 00 0a 00 02 39 00 00 00 00 00 00 00 00 00 00 00 30 46";
            txtLock1.Text = "10 00 00 C5 15 96 01 00 00 00 0a 00 02 39 00 28 00 00 00 00 00 00 00 00 00 18 46";
            txtSleep.Text = "10 00 00 C5 01 80 44";

        }


        private void button5_Click(object sender, EventArgs e)
        {
            com1.Close();
            if (!com1.IsOpen)
            {

                WriteLog("close successfully");
            }
        }

        private void t_port_TextChanged(object sender, EventArgs e)
        {
            //mycom1.PortNum = t_port.Text;
        }

        private void t_rate_TextChanged(object sender, EventArgs e)
        {
            //mycom1.BaudRate = Convert.ToInt32(t_rate.Text);

        }

        private void t_bytesize_TextChanged(object sender, EventArgs e)
        {
            //mycom1.ByteSize = (byte)Int16.Parse(t_bytesize.Text);
            //mycom1.ByteSize = Convert.ToByte(t_bytesize.Text, 10); //8 bits
        }

        private void t_stopbyte_TextChanged(object sender, EventArgs e)
        {
            //mycom1.StopBits = Convert.ToByte(t_stopbyte.Text, 10); // 0,1,2 = 1, 1.5, 2 
        }

        private void t_parity_TextChanged(object sender, EventArgs e)
        {

        }
 
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {

                WriteLog("choosed    #"+(radioButton1.TabIndex*2+2402));
               keyValue = radioButton1.TabIndex;
            }
            }
          private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                WriteLog("choosed    #"+(radioButton2.TabIndex*2+2402));
              keyValue = radioButton2.TabIndex;
            }
          }
           private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                WriteLog("choosed    #"+(radioButton3.TabIndex*2+2402));
                keyValue = radioButton3.TabIndex;
            }
               }
          private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                 WriteLog("choosed    #"+(radioButton4.TabIndex*2+2402));
                 keyValue = radioButton4.TabIndex;
            }  
          }

          private void radioButton5_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton5.Checked)
              {
                  WriteLog("choosed    #" + (radioButton5.TabIndex*2+2402));
                  keyValue = radioButton5.TabIndex;
              }
          }

          private void radioButton6_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton6.Checked)
              {
                  WriteLog("choosed    #" + (radioButton6.TabIndex*2+2402));
                  keyValue = radioButton6.TabIndex;
              }
          }

          private void radioButton7_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton7.Checked)
              {
                  WriteLog("choosed    #" + (radioButton7.TabIndex*2+2402));
                  keyValue = radioButton7.TabIndex;
              }
          }

          private void radioButton8_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton8.Checked)
              {
                  WriteLog("choosed    #" + (radioButton8.TabIndex*2+2402));
                  keyValue = radioButton8.TabIndex;
              }
          }

          private void radioButton9_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton9.Checked)
              {
                  WriteLog("choosed    #" + (radioButton9.TabIndex*2+2402));
                  keyValue = radioButton9.TabIndex;
              }
          }

          private void radioButton10_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton10.Checked)
              {
                  WriteLog("choosed    #" + (radioButton10.TabIndex*2+2402));
                  keyValue = radioButton10.TabIndex;
              }
          }

          private void radioButton11_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton11.Checked)
              {
                  WriteLog("choosed    #" + (radioButton11.TabIndex*2+2402));
                  keyValue = radioButton11.TabIndex;
              }
          }

          private void radioButton13_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton13.Checked)
              {
                  WriteLog("choosed    #" + (radioButton13.TabIndex*2+2402));
                  keyValue = radioButton13.TabIndex;
              }
          }

          private void radioButton12_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton12.Checked)
              {
                  WriteLog("choosed    #" + (radioButton12.TabIndex*2+2402));
                  keyValue = radioButton12.TabIndex;
              }
          }

          private void radioButton14_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton14.Checked)
              {
                  WriteLog("choosed    #" + (radioButton14.TabIndex*2+2402));
                  keyValue = radioButton14.TabIndex;
              }
          }

          private void radioButton15_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton15.Checked)
              {
                  WriteLog("choosed    #" + (radioButton15.TabIndex*2+2402));
                  keyValue = radioButton15.TabIndex;
              }
          }

          private void radioButton16_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton16.Checked)
              {
                  WriteLog("choosed    #" + (radioButton16.TabIndex*2+2402));
                  keyValue = radioButton16.TabIndex;
              }
          }

          private void radioButton17_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton17.Checked)
              {
                  WriteLog("choosed    #" + (radioButton17.TabIndex*2+2402));
                  keyValue = radioButton17.TabIndex;
              }
          }

          private void radioButton18_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton18.Checked)
              {
                  WriteLog("choosed    #" + (radioButton18.TabIndex*2+2402));
                  keyValue = radioButton18.TabIndex;
              }
          }

          private void radioButton19_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton19.Checked)
              {
                  WriteLog("choosed    #" + (radioButton19.TabIndex*2+2402));
                  keyValue = radioButton19.TabIndex;
              }
          }

          private void radioButton20_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton20.Checked)
              {
                  WriteLog("choosed    #" + (radioButton20.TabIndex*2+2402));
                  keyValue = radioButton20.TabIndex;
              }
          }

          private void radioButton21_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton21.Checked)
              {
                  WriteLog("choosed    #" + (radioButton21.TabIndex*2+2402));
                  keyValue = radioButton21.TabIndex;
              }
          }

          private void radioButton22_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton22.Checked)
              {
                  WriteLog("choosed    #" + (radioButton22.TabIndex*2+2402));
                  keyValue = radioButton22.TabIndex;
              }
          }

          private void radioButton23_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton23.Checked)
              {
                  WriteLog("choosed    #" + (radioButton23.TabIndex*2+2402));
                  keyValue = radioButton23.TabIndex;
              }
          }

          private void radioButton24_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton24.Checked)
              {
                  WriteLog("choosed    #" + (radioButton24.TabIndex*2+2402));
                  keyValue = radioButton24.TabIndex;
              }
          }

          private void radioButton25_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton25.Checked)
              {
                  WriteLog("choosed    #" + (radioButton25.TabIndex*2+2402));
                  keyValue = radioButton25.TabIndex;
              }
          }

          private void radioButton26_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton26.Checked)
              {
                  WriteLog("choosed    #" + (radioButton26.TabIndex*2+2402));
                  keyValue = radioButton26.TabIndex;
              }
          }

          private void radioButton27_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton27.Checked)
              {
                  WriteLog("choosed    #" + (radioButton27.TabIndex*2+2402));
                  keyValue = radioButton27.TabIndex;
              }
          }

          private void radioButton28_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton28.Checked)
              {
                  WriteLog("choosed    #" + (radioButton28.TabIndex*2+2402));
                  keyValue = radioButton28.TabIndex;
              }
          }

          private void radioButton29_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton29.Checked)
              {
                  WriteLog("choosed    #" + (radioButton29.TabIndex*2+2402));
                  keyValue = radioButton29.TabIndex;
              }
          }

          private void radioButton30_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton30.Checked)
              {
                  WriteLog("choosed    #" + (radioButton30.TabIndex*2+2402));
                  keyValue = radioButton30.TabIndex;
              }
          }

          private void radioButton31_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton31.Checked)
              {
                  WriteLog("choosed    #" + (radioButton31.TabIndex*2+2402));
                  keyValue = radioButton31.TabIndex;
              }
          }

          private void radioButton32_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton32.Checked)
              {
                  WriteLog("choosed    #" + (radioButton32.TabIndex*2+2402));
                  keyValue = radioButton32.TabIndex;
              }
          }

          private void radioButton33_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton33.Checked)
              {
                  WriteLog("choosed    #" + (radioButton33.TabIndex*2+2402));
                  keyValue = radioButton33.TabIndex;
              }
          }

          private void radioButton34_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton34.Checked)
              {
                  WriteLog("choosed    #" + (radioButton34.TabIndex*2+2402));
                  keyValue = radioButton34.TabIndex;
              }
          }

          private void radioButton35_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton35.Checked)
              {
                  WriteLog("choosed    #" + (radioButton35.TabIndex*2+2402));
                  keyValue = radioButton35.TabIndex;
              }
          }

          private void radioButton36_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton36.Checked)
              {
                  WriteLog("choosed    #" + (radioButton36.TabIndex*2+2402));
                  keyValue = radioButton36.TabIndex;
              }
          }

          private void radioButton37_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton37.Checked)
              {
                  WriteLog("choosed    #" + (radioButton37.TabIndex*2+2402));
                  keyValue = radioButton37.TabIndex;
              }
          }

          private void radioButton38_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton38.Checked)
              {
                  WriteLog("choosed    #" + (radioButton38.TabIndex*2+2402));
                  keyValue = radioButton38.TabIndex;
              }
          }

          private void radioButton39_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton39.Checked)
              {
                  WriteLog("choosed    #" + (radioButton39.TabIndex*2+2402));
                  keyValue = radioButton39.TabIndex;
              }
          }

          private void radioButton40_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton40.Checked)
              {
                  WriteLog("choosed    #" + (radioButton40.TabIndex*2+2402));
                  keyValue = radioButton40.TabIndex;
              }
          }

          private void t_port_SelectedIndexChanged(object sender, EventArgs e)
          {
        

          }

          private void Form1_Load(object sender, EventArgs e)
          {
              userInit();
          }

          private void button6_Click(object sender, EventArgs e)
          {
              MessageBox.Show(
                  
             "Version 1.0\n1.配置好串口.\n2.点击Open按钮，上位机将下发0xAA\n3.如果上位机收到0xAA则会提示connect successful\n4.点击Send,上位机将下发1个byte数据\n5.如果收到与当前下发数据相同数据，会提示\n6.下发数据为0-49 \n7.TX_FLAG_199,RX_FLAG_188\n8.打印收到的非命令数据","使用方法");
              


          }

          private void button7_Click(object sender, EventArgs e)
          {

              t_bytesize.Items.Clear();
              t_parity.Items.Clear();
              t_port.Items.Clear();
              t_rate.Items.Clear();
              t_stopbyte.Items.Clear();
              lstLog.Items.Clear();
              ;//
              radioButton1.Checked = true;
              userInit();
          }

          private void radioButton41_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton41.Checked) {
                  tx_rx_flag = RX_FLAG;


                  if (com1.IsOpen)
                  {
                      //-------------------------发送数据-------------------------------------
                      try
                      {
                          Byte[] txData = { (byte)tx_rx_flag };
                          com1.Write(txData, 0, 1);
                          WriteLog("          send     #RX_FLAG");

                      }
                      catch (Exception ex)
                      {               }

                  }
                  else { WriteLog("check serialport"); }

              }


              

          }

          private void radioButton42_CheckedChanged(object sender, EventArgs e)
          {
              if (radioButton42.Checked)
              {
                  tx_rx_flag = TX_FLAG;

                  if (com1.IsOpen)
                  {
                      //-------------------------发送数据-------------------------------------
                      try
                      {
                          Byte[] txData = { (byte)tx_rx_flag };
                          com1.Write(txData, 0, 1);
                          WriteLog("           send     #TX_FLAG");

                      }
                      catch (Exception ex)
                      { }
                  }
                  else { WriteLog("check serialport"); }

              }
          }
            






         }
    }

