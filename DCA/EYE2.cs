using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using eye2;

namespace WhiteBalanceCorrection
{
    class EYE2 : IDisplayColorAnalyzer
    {
        public ProbeStruct m_Probe;
        public bool m_isConn;
        private string m_sEXceptCom;
        public JCEYE2 EYE;

        public EYE2(string sEXceptCom)
        {
            EYE = new JCEYE2();
            m_Probe = new ProbeStruct();
            m_isConn = false;
            m_sEXceptCom = sEXceptCom;
        }
        public bool  Connect( string sCom)
        {
            try
            {
                m_isConn= EYE.Connect(sCom);
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
            var ports = EYE.GetPortlist();
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

                    if (false == SetChn(ch))
                    {
                        errorMsg = "EYE2设置通道失败，请检查色彩分析仪是否有异常";
                        return false;
                    }

                    if (false == SetMMS())
                    {
                        errorMsg = "EYE2设置通道测量模式，请检查色彩分析仪是否有异常";
                        return false;
                    }

                    if (false == ZeroCal())
                    {
                        errorMsg = "EYE2校零失败，请检查色彩分析仪是否有异常";
                        return false;
                    }

                    return true;
                }
                catch (System.Exception err)
                {
                    errorMsg = "EYE2连接失败，出错信息：" + err.Message.ToString();
                }
            }
            errorMsg = "EYE2连接失败，请检查是否连接色彩分析仪";
            return false;
        }


        public bool SetChn(int cChnNo)
        {
            return EYE.SetChannel(cChnNo);
        }

        public bool SetMMS()
        {
            if (EYE.SetSyncSpeedMode(4, "100", 1))//NTSC 60  AUTO
            {
                return true;
            }
            return false;
        }

        public bool ZeroCal()
        {
            return EYE.ZeroCalibration();
        }

        public void SetDisplayModel(int model)
        {
             EYE.SetMeasureMode(model);
        }

        public void  Disconnect()
        {
            try
            {
                if (m_isConn)
                {                
                    m_isConn = false;
                     EYE.DisConnect();
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
                    if ( EYE.Measure())
                    {

                            m_Probe.T = EYE.tcp ;
                            m_Probe.Lv = EYE.Lv ;
                            m_Probe.X = EYE.X;
                            m_Probe.Y = EYE.Y ;
                            m_Probe.Z = EYE.Z ;
                            m_Probe.sx = EYE.x; 
                            m_Probe.sy = EYE.y;
                            m_Probe.du = EYE.ud;
                            m_Probe.dv = EYE.vd;
                    string sLog = "EYE2测量结果 : x:" + m_Probe.sx.ToString() + " y:" + m_Probe.sy.ToString() + " Y:" + m_Probe.Lv.ToString() + " T:" + m_Probe.T.ToString();
                        LogHelper.WriteToLog(sLog, LogLevel.INFO);
                    }
                    else
                    {
                        LogHelper.WriteToLog("ERROR:  Measure返回false", LogLevel.INFO);
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