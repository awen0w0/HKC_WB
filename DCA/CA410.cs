using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;


namespace WhiteBalanceCorrection
{
    class CA410 : IDisplayColorAnalyzer
    {
        private SerialPort m_SerialPortCA410;
        public ProbeStruct m_Probe;
        public bool m_isConn;
        private string m_sEXceptCom;

        public CA410()
        {
            m_SerialPortCA410 = new SerialPort();
            m_Probe = new ProbeStruct();
            m_isConn = false;
        }

        public CA410(string sEXceptCom)
        {
            m_SerialPortCA410 = new SerialPort();
            m_Probe = new ProbeStruct();
            m_isConn = false;
            m_sEXceptCom = sEXceptCom;
        }

        private bool Connect(string sCom)
        {
            try
            {
                m_SerialPortCA410.PortName = sCom;
                m_SerialPortCA410.BaudRate = 38400;
                m_SerialPortCA410.StopBits = System.IO.Ports.StopBits.One;
                m_SerialPortCA410.DataBits = 8;
                m_SerialPortCA410.DtrEnable = false;
                m_SerialPortCA410.RtsEnable = false;
                m_SerialPortCA410.Open();
                m_SerialPortCA410.ReadTimeout = 4000;
                m_isConn = true;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message.ToString());
                m_isConn = false;
            }

            return m_isConn;
        }

        public bool ConnectDevice(int ch, ref string errorMsg)
        {
            var ports = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                try
                {
                    if (m_sEXceptCom != string.Empty && m_sEXceptCom == port)
                    {
                        continue;
                    }
                    var result = Connect(port);
                    if (!result)
                    {
                        continue;
                    }

                    if (false == SetCom(true))
                    {
                        // errorMsg = "CA410开启通信失败，请检查是否连接色彩分析仪";
                        Disconnect();
                        continue;
                    }

                    if (false == SetChn(ch))
                    {
                        errorMsg = "CA410设置通道失败，请检查色彩分析仪是否有异常";
                        return false;
                    }

                    if (false == SetMMS())
                    {
                        errorMsg = "CA410设置通道测量模式，请检查色彩分析仪是否有异常";
                        return false;
                    }

                    if (false == ZeroCal())
                    {
                        errorMsg = "CA410校零失败，请检查色彩分析仪是否有异常";
                        return false;
                    }

                    return true;
                }
                catch (System.Exception err)
                {
                    errorMsg = "CA410连接失败，出错信息：" + err.Message.ToString();
                }
            }
            errorMsg = "CA410连接失败，请检查是否连接色彩分析仪";
            return false;
        }

        public bool Send(string sStr)
        {
            if (m_isConn)
            {
                try
                {
                    m_SerialPortCA410.WriteLine(sStr);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        public string Receive(int nReceiveTimeout = 1000)
        {
            string recStr = string.Empty;
            if (m_isConn)
            {
                try
                {
                    byte[] recByte = new byte[128];
                    bool bContinue = true;
                    while (bContinue)
                    {
                        int bytes = m_SerialPortCA410.Read(recByte, 0, recByte.Length);
                        recStr += Encoding.ASCII.GetString(recByte, 0, bytes);
                        for (int i = 0; i < 128; i++)
                        {
                            if (recByte[i] == 0X0D || recByte[i] == 0X0A)
                            {
                                bContinue = false;
                                break;
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    return recStr;
                }
            }
            return recStr;
        }

        public bool SetCom(bool bIsCom)
        {
            string sCmd = "COM,1";
            if (!bIsCom)
            {
                sCmd = "COM,0";
            }
            Send(sCmd);
            string recStr = Receive(5000);
            if (recStr == "OK00\r")
            {
                return true;
            }
            return false;
        }

        public bool SetChn(int cChnNo)
        {
            string sSetCh = "MCH," + cChnNo.ToString();
            //string sSetCh = "MCH," + cChnNo.ToString("00");
            Send(sSetCh);
            string recStr = Receive(5000);
            if (recStr == "OK00\r")
            {
                return true;
            }
            return false;
        }

        public bool SetMMS()
        {
            Send("MMS,1");
            string recStr = Receive(5000);
            if (recStr == "OK00\r")
            {
                return true;
            }
            return false;
        }

        public bool ZeroCal()
        {
            Send("ZRC");
            string recStr = Receive(5000);
            if (recStr == "OK00\r")
            {
                return true;
            }
            return false;
        }

        public void SetDisplayModel(int model)
        {
            string sendStr = "MDS," + model.ToString();
            Send(sendStr);
            string recStr = Receive(5000);
        }

        public void Disconnect()
        {
            try
            {
                if (m_isConn)
                {
                    SetCom(false);
                    m_SerialPortCA410.Close();
                    m_isConn = false;
                }
            }
            catch (System.Exception e)
            {

            }
        }

        public ProbeStruct Measure()
        {
            try
            {
                Send("MES,2");
                string recStr = Receive(5000);
                string[] recStrs = recStr.Replace("\r", "").Split(',');
                if (recStrs.Length == 11|| recStrs[0] == "OK04")
                {

                    if (recStrs[0] == "OK00")
                    {
                        if (Common.IsValidDecimal(recStrs[3]))
                        {
                            m_Probe.T = float.Parse(recStrs[3]);
                        }
                        if (Common.IsValidDecimal(recStrs[5]))
                        {
                            m_Probe.Lv = float.Parse(recStrs[5]);
                        }
                        if (Common.IsValidDecimal(recStrs[8]) &&
                            Common.IsValidDecimal(recStrs[9]) &&
                            Common.IsValidDecimal(recStrs[10]))
                        {
                            m_Probe.X = double.Parse(recStrs[8]);
                            m_Probe.Y = double.Parse(recStrs[9]);
                            m_Probe.Z = double.Parse(recStrs[10]);

                            m_Probe.sx = (float)(m_Probe.X / (m_Probe.X + m_Probe.Y + m_Probe.Z));
                            m_Probe.sy = (float)(m_Probe.Y / (m_Probe.X + m_Probe.Y + m_Probe.Z));
                        }
                        string sLog = "CA410测量结果 : x:" + m_Probe.sx.ToString() + " y:" + m_Probe.sy.ToString() + " Y:" + m_Probe.Lv.ToString() + " T:" + m_Probe.T.ToString();
                        LogHelper.WriteToLog(sLog, LogLevel.INFO);
                    }
                    else if (recStrs[0] == "OK04")
                    {
                        m_Probe.T = 0;
  
                        m_Probe.Lv = 0;
 
                        m_Probe.X = 0;
                        m_Probe.Y = 0;
                        m_Probe.Z = 0;

                        m_Probe.sx = 0;
                        m_Probe.sy = 0;

                    }
                    else
                    {
                        LogHelper.WriteToLog("ERROR: " + recStrs[0], LogLevel.INFO);
                        MessageBox.Show("色温仪链接已断开，即将重新打开校正工具，请重新插拔色温仪usb");


                        Application.Exit();
                        System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);

                    }
                }
                else
                {
                    LogHelper.WriteToLog("ERROR: "+ recStrs.Length.ToString(), LogLevel.INFO);
                    MessageBox.Show("色温仪链接已断开，即将重新打开校正工具，请重新插拔色温仪usb");

                    Application.Exit();
                    System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
            }
            catch (System.Exception e)
            {
            }
            return m_Probe;
        }

        public ProbeStruct MeasureEx(double tolerance = 0.0001, int maxMeasureCounts = 60)
        {
            return Measure();
        }
    }
}
