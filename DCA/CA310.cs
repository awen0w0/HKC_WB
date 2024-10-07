using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace WhiteBalanceCorrection
{
    class CA310 : IDisplayColorAnalyzer
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);

        CA200SRVRLib.Ca200 m_ObjCa200 = null;
        CA200SRVRLib.Ca m_ObjCa;
        CA200SRVRLib.Probe m_ObjProbe;
        CA200SRVRLib.Memory m_ObjMemory;
        ProbeStruct m_Probe;

        public enum CaDisplayMode
        {
            DSP_LXY = 0,
            DSP_DUV = 1,
            DSP_ANL = 2,
            DSP_ANLG = 3,
            DSP_ANLR = 4,
            DSP_PUV = 5,
            DSP_FMA = 6,
            DSP_XYZ = 7,
            DSP_JEITA = 8
        };

        public CA310()
        {
            try
            {
                m_ObjCa200 = new CA200SRVRLib.Ca200();
                if (m_ObjCa200 == null)
                {
                    System.Windows.Forms.MessageBox.Show("please install CA310 sdk driver");
                }
                m_Probe = new ProbeStruct();
            }
            catch (Exception err)
            {
                System.Windows.Forms.MessageBox.Show(err.Message.ToString());
            }
        }

        public bool ConnectDevice(int ch, ref string errorMsg)
        {
            try
            {
                m_ObjCa200.AutoConnect();
            }
            catch (Exception err)
            {
                errorMsg = "CA310连接失败，出错信息：" + err.Message.ToString();
                return false;
            }

            if (m_ObjCa200 != null)
            {
                m_ObjCa = m_ObjCa200.SingleCa;
            }
            else
            {
                return false;
            }

            if (m_ObjCa != null)
            {
                m_ObjProbe = m_ObjCa.SingleProbe;
                m_ObjMemory = m_ObjCa.Memory;

                m_ObjCa.DisplayMode = (int)(CaDisplayMode.DSP_LXY);
                m_ObjMemory.ChannelNO = ch;
                m_ObjCa.RemoteMode = 1;
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool ZeroCal()
        {
            try
            {
                if (m_ObjCa != null)
                {
                    m_ObjCa.CalZero();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("CA310 NOT CONNECTED");
                }
            }
            catch (Exception err)
            {
                System.Windows.Forms.MessageBox.Show(err.Message.ToString());
                return false;
            }
            return true;
        }

        public void SetDisplayModel(int model)
        {
            if (m_ObjCa != null)
            {
                m_ObjCa.DisplayMode = model;
            }
        }

        public void Disconnect()
        {
            if (null != m_ObjCa)
            {
                m_ObjCa.RemoteMode = 0;
            }
            if (null != m_ObjCa200)
            {
                m_ObjCa200 = null;
            }
            if (null != m_ObjCa)
            {
                m_ObjCa = null;
            }
            if (null != m_ObjProbe)
            {
                m_ObjProbe = null;
            }
            if (null != m_ObjMemory)
            {
                m_ObjMemory = null;
            }
        }

        public ProbeStruct Measure()
        {
            try
            {
                m_ObjCa.Measure(0);
            }
            catch (System.Exception ex)
            {
            	
            }
            switch (m_ObjCa.DisplayMode)
            {
                case (int)CaDisplayMode.DSP_LXY:
                    {
                        m_Probe.sx = m_ObjProbe.sx;
                        m_Probe.sy = m_ObjProbe.sy;
                        m_Probe.Lv = m_ObjProbe.Lv;
                        m_Probe.X = m_ObjProbe.X;
                        m_Probe.Y = m_ObjProbe.Y;
                        m_Probe.Z = m_ObjProbe.Z;
                        m_Probe.T = (float)m_ObjProbe.T;
                        string sLog = "CA310测量结果 : x:" + m_Probe.sx.ToString() + " y:" + m_Probe.sy.ToString() + " Y:" + m_Probe.Lv.ToString() + " T:" + m_Probe.T.ToString();
                        //Console.WriteLine(sLog);
                        LogHelper.WriteToLog(sLog, LogLevel.INFO);
                    }
                    break;
                case (int)CaDisplayMode.DSP_ANL:
                    {
                        m_Probe.R = m_ObjProbe.R;
                        m_Probe.G = m_ObjProbe.G;
                        m_Probe.B = m_ObjProbe.B;
                    }
                    break;
                case (int)CaDisplayMode.DSP_XYZ:
                    m_Probe.sx = m_ObjProbe.sx;
                    m_Probe.sy = m_ObjProbe.sy;
                    m_Probe.Lv = m_ObjProbe.Lv;
                    m_Probe.X = m_ObjProbe.X;
                    m_Probe.Y = m_ObjProbe.Y;
                    m_Probe.Z = m_ObjProbe.Z;
                    m_Probe.T = (float)m_ObjProbe.T;
                    break;
                case (int)CaDisplayMode.DSP_FMA:
                    m_Probe.FlickerFMA = m_ObjProbe.FlckrFMA;
                    break;
            }
            return m_Probe;
        }

        public ProbeStruct MeasureEx(double tolerance = 0.0001, int maxMeasureCounts = 60)
        {
            return Measure();
        }

        public void SetDisplayMode()
        {
            m_ObjCa.DisplayMode = (int)(CaDisplayMode.DSP_LXY);
        }
    }
}
