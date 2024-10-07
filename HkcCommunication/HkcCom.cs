using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Security.Cryptography;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics.Eventing.Reader;
using static WhiteBalanceCorrection.ColorSpace;
using System.Reflection;

namespace WhiteBalanceCorrection
{
    class HkcCom : IHkcCommunication
    {
        public bool m_bSubPackage_Send = false, m_bSubPackage_Recv = true;
        public int m_nSubPackageSize = 128*8;

        private string m_ComPort;
        private int m_Port, mmWindowSize;
        public bool m_isConn;
        SerialPort m_SerialPortTV = new SerialPort();
        Formpattern patternf = new Formpattern();
      

        public HkcCom(string ComPort, int Port)
        {
            m_ComPort = ComPort;
            m_Port = Port;
            m_isConn = false;
        }

        public bool Connect(int timeout)
        {
            try
            {
                m_SerialPortTV.PortName = m_ComPort;
                m_SerialPortTV.BaudRate = m_Port;
                m_SerialPortTV.StopBits = System.IO.Ports.StopBits.One;
                m_SerialPortTV.DataBits = 8;
                m_SerialPortTV.DtrEnable = false;
                m_SerialPortTV.RtsEnable = false;
                m_SerialPortTV.Open();
                m_SerialPortTV.ReadTimeout = timeout;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message.ToString());
                m_isConn = false;
            }
            m_isConn = true;

            return m_isConn;
        }

        public void Disconnect()
        {
            if (m_isConn)
            {
                m_SerialPortTV.Close();
                m_isConn = false;
            }
        }

        public bool Send(string sStr)
        {
            if (m_isConn)
            {
                try
                {
                    if (m_bSubPackage_Send)
                    {
                        while (sStr.Length > m_nSubPackageSize)
                        {
                            string sStrSend = sStr.Substring(0, m_nSubPackageSize);
                            m_SerialPortTV.Write(sStrSend);
                            sStr = sStr.Substring(m_nSubPackageSize, sStr.Length - m_nSubPackageSize);
                        }
                        m_SerialPortTV.Write(sStr);
                    }
                    else
                    {
                        m_SerialPortTV.Write(sStr);
                    }
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        public string Receive(int nReceiveTimeout=1000)
        {
            //Thread.Sleep(200);
            m_SerialPortTV.ReadTimeout = nReceiveTimeout;
            string recStr = string.Empty;
            byte[] recByte = new byte[m_nSubPackageSize];
            string key = "\r\n";
            string head = "paramsJson";
            string REE = "FAIL";
            string RE = "SUCCESS";
            string last = "}";
            if (m_isConn)
            {
                try
                {
                    if (m_bSubPackage_Recv)
                    {
                       
                        bool bContinue = true;
                        while (bContinue)
                        {
                            int bytes = m_SerialPortTV.Read(recByte, 0, recByte.Length);
                            recStr += Encoding.ASCII.GetString(recByte, 0, bytes);
                           
                            for (int i = 0; i < m_nSubPackageSize; i++)
                            {
                                if (recByte[i] == 0X0D && recByte[i + 1] == 0X0A&& recStr.ToLower().Contains(head.ToLower())&& (recStr.ToLower().Contains(REE.ToLower())|| recStr.ToLower().Contains(RE.ToLower())))
                                {
                                    bContinue = false;
                                    break;
                                }
                                else if(recStr.ToLower().Contains(key.ToLower())&& recStr.ToLower().Contains(head.ToLower()) && recStr.ToLower().Contains(REE.ToLower()) || recStr.ToLower().Contains(RE.ToLower()) && recStr.ToLower().Contains(last.ToLower()))
                                {
                                    bContinue = false;
                                    break;
                                }
                            }
                            
                        }
                    }
                    else
                    {
                        recStr = m_SerialPortTV.ReadLine();
                    }

                }
                catch (System.Exception ex)
                {
                    return ex.ToString();
                }
            }
            return recStr;
        }

        // ----------Gamma----------
        public bool GammaStart(int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_GammaStart();
            Send(sCMD);
            //LogHelper.WriteToLog("SEND GammaStart：" + sCMD, LogLevel.DEBUG);
            Thread.Sleep(100);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE GammaStart：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_GammaStart(sReceive);
            return bIsOK;
        }

        public bool GammaSetPattern(int nR, int nG, int nB, int nReceiveTimeout = 1000)
        {

            if (nR < 0) nR = 0;
            if (nG < 0) nG = 0;
            if (nB < 0) nB = 0;
            patternf.Show();
            Panel panel = patternf.panel1;

            int windowsize = mmWindowSize;
            patternf.WindowState = FormWindowState.Maximized;
            patternf.BackColor = Color.FromArgb(0, 0, 0);    
            if(windowsize==100) patternf.BackColor = Color.FromArgb(nR, nG, nB);
            Rectangle screenSize = Screen.PrimaryScreen.WorkingArea;
            int width = screenSize.Width,h= screenSize.Height;
            width = (int)(width * Math .Sqrt((double)windowsize /100)); h = (int)(h * Math.Sqrt((double)windowsize / 100));
            int Lx = screenSize.Width / 2 - width/2, Ly = screenSize.Height / 2 - h/2;
            panel.BackColor = Color.FromArgb(nR, nG, nB);
            panel.Location = new System.Drawing.Point(Lx, Ly);
            panel.Name = "panel1";
            panel.Size = new System.Drawing.Size(width, h);
            panel.TabIndex = 0;
            Application.DoEvents();
            patternf.Refresh();
            //string sCMD = HkcJson.EncodeJsonCMD_WBSetUserPattern(nR, nG, nB );
            //Thread.Sleep(100);
            
            //LogHelper.WriteToLog("SEND WBSetUserPattern：" + sCMD, LogLevel.DEBUG);
            //Send(sCMD);
            //Thread.Sleep(100);
            //string sReceive = Receive(nReceiveTimeout);
            // LogHelper.WriteToLog("RECEIVE WBSetUserPattern：" + sReceive, LogLevel.DEBUG);
            //bool bIsOK = HkcJson.DecodeJsonCMD_WBSetUserPattern(sReceive);
            return true;
        }
       
        public bool GammaSet(int[] nRArr, int[] nGArr, int[] nBArr, int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_GammaSet(nRArr, nGArr, nBArr);
            Send(sCMD);
            //LogHelper.WriteToLog("SEND GammaSet：" + sCMD, LogLevel.DEBUG);
            Thread.Sleep(100);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE GammaSet：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_GammaSet(sReceive);
            return bIsOK;
        }

        public bool GammaSave(int nNum, int[] nRArr, int[] nGArr, int[] nBArr, int nReceiveTimeout = 1000,int m_nMCU=98)
        {
            string sCMD = string.Empty;
            bool bIsOK = false;
            string CRC = string.Empty;
            int[] nRArr1 = new int [nRArr.Length / 2];
            int[] nRArr2 = new int[nRArr.Length / 2];
            int[] nGArr1 = new int[nRArr.Length / 2];
            int[] nGArr2 = new int[nRArr.Length / 2];
            int[] nBArr1 = new int[nRArr.Length / 2];
            int[] nBArr2 = new int[nRArr.Length / 2];
            for (int i = 0; i < nRArr.Length; i++)
            {
                if (i < nRArr.Length / 2)
                {
                    nRArr1[i] = nRArr[i];
                    nGArr1[i] = nGArr[i];
                    nBArr1[i] = nBArr[i];
                }
                else 
                {
                    nRArr2[i- nRArr.Length / 2] = nRArr[i];
                    nGArr2[i- nRArr.Length / 2] = nGArr[i];
                    nBArr2[i- nRArr.Length / 2] = nBArr[i];
                }           
            }

            byte num = 0;
            byte[] hex = new byte[2];
            for (int i = 0; i < nRArr.Length; i++)
            {
                if (nRArr[nRArr.Length - 1] < 256)
                {
                    num=(byte)(num + nRArr[i]);
                }
                else
                {
                    hex[0] = (byte)(nRArr[i] & 0xff);
                    hex[1] = (byte)((nRArr[i] >> 8) & 0xff);
                    num = (byte)(num + hex[0] + hex[1]);
                }
                /*
                if (nGArr[nGArr.Length - 1] < 256)
                {
                    num = (byte)(num + nGArr[i]);
                }
                else
                {
                    hex[0] = (byte)(nGArr[i] & 0xff);
                    hex[1] = (byte)((nGArr[i] >> 8) & 0xff);
                    num = (byte)(num + hex[0] + hex[1]);
                }
                if (nBArr[nBArr.Length - 1] < 256)
                {
                    num = (byte)(num + nBArr[i]);
                }
                else
                {
                    hex[0] = (byte)(nBArr[i] & 0xff);
                    hex[1] = (byte)((nBArr[i] >> 8) & 0xff);
                    num = (byte)(num + hex[0] + hex[1]);
                }*/
            }
            if(m_nMCU==9701)
            for (int i = 1; i < 9; i++)
            {
                int[] Arr = new int[32];
                Array.Copy(nRArr, (i - 1) * 32, Arr, 0, 32);
                sCMD = HkcJson.EncodeJsonCMD_GammaSaveDataN(nNum, i, Arr);

                Thread.Sleep(100);
                Send(sCMD);
                //LogHelper.WriteToLog("SEND GammaSave：" + sCMD, LogLevel.DEBUG);
                Thread.Sleep(100);
                string sReceive = Receive(nReceiveTimeout);
                //LogHelper.WriteToLog("RECEIVE GammaSave：" + sReceive, LogLevel.DEBUG);
                bIsOK = HkcJson.DecodeJsonCMD_GammaDataN(sReceive);
                if (!bIsOK) 
                    
                    
                    break;
            }
            /*
            for (int i = 1; i < 9; i++)
            {
                int[] Arr = new int[32];
                Array.Copy(nGArr, (i - 1) * 32, Arr, 0, 32);
                sCMD = HkcJson.EncodeJsonCMD_GammaSaveDataN(nNum, i+8, Arr);

                Thread.Sleep(100);
                Send(sCMD);
                //LogHelper.WriteToLog("SEND GammaSave：" + sCMD, LogLevel.DEBUG);
                Thread.Sleep(100);
                string sReceive = Receive(nReceiveTimeout);
                //LogHelper.WriteToLog("RECEIVE GammaSave：" + sReceive, LogLevel.DEBUG);
                bIsOK = HkcJson.DecodeJsonCMD_GammaDataN(sReceive);
                if (!bIsOK) 
                    break;
            }
            for (int i = 1; i < 9; i++)
            {
                int[] Arr = new int[32];
                Array.Copy(nBArr, (i - 1) * 32, Arr, 0, 32);
                sCMD = HkcJson.EncodeJsonCMD_GammaSaveDataN(nNum, i+16, Arr);

                Thread.Sleep(100);
                Send(sCMD);
                //LogHelper.WriteToLog("SEND GammaSave：" + sCMD, LogLevel.DEBUG);
                Thread.Sleep(100);
                string sReceive = Receive(nReceiveTimeout);
                //LogHelper.WriteToLog("RECEIVE GammaSave：" + sReceive, LogLevel.DEBUG);
                bIsOK = HkcJson.DecodeJsonCMD_GammaDataN(sReceive);
                if (!bIsOK)
                    break;
            }*/
            else
            for (int i = 1; i < 3; i++)
            {
                switch (i)
                {
                    case 1:
                        sCMD = HkcJson.EncodeJsonCMD_GammaSaveDataN(nNum, i, nRArr1);
                        break;
                    case 2:
                        sCMD = HkcJson.EncodeJsonCMD_GammaSaveDataN(nNum, i, nRArr2);
                        break;
                    case 3:
                        sCMD = HkcJson.EncodeJsonCMD_GammaSaveDataN(nNum, i, nGArr1);
                        break;
                    case 4:
                        sCMD = HkcJson.EncodeJsonCMD_GammaSaveDataN(nNum, i, nGArr2);
                        break;
                    case 5:
                        sCMD = HkcJson.EncodeJsonCMD_GammaSaveDataN(nNum, i, nBArr1);
                        break;
                    case 6:
                        sCMD = HkcJson.EncodeJsonCMD_GammaSaveDataN(nNum, i, nBArr2);
                        break;
                }
                Thread.Sleep(100);
                Send(sCMD);
                //LogHelper.WriteToLog("SEND GammaSave：" + sCMD, LogLevel.DEBUG);
                Thread.Sleep(100);
                string sReceive = Receive(nReceiveTimeout);              
                //LogHelper.WriteToLog("RECEIVE GammaSave：" + sReceive, LogLevel.DEBUG);
                bIsOK = HkcJson.DecodeJsonCMD_GammaDataN(sReceive);
                if (!bIsOK) break;
            }
            if (bIsOK)
            {
                Thread.Sleep(100);
                sCMD = HkcJson.EncodeJsonCMD_GammaCRC(nNum);
                Send(sCMD);
                Thread.Sleep(100);
                string sReceive = Receive(nReceiveTimeout);
                LogHelper.WriteToLog("RECEIVE GammaCRC：" + sReceive, LogLevel.DEBUG);
                CRC = HkcJson.DecodeJsonCMD_GammaCRC(sReceive);
                LogHelper.WriteToLog("RECEIVE GammaCRC：" + CRC.ToString() + "   NUM-CRC"+num.ToString(), LogLevel.DEBUG);
                if (num.ToString() != CRC) bIsOK=false  ;
            }
            return bIsOK;
        }

        public bool GammaApply(int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_GammaApply();
            Send(sCMD);
            //LogHelper.WriteToLog("SEND GammaApply：" + sCMD, LogLevel.DEBUG);
            Thread.Sleep(100);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE GammaApply：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_GammaApply(sReceive);
            return bIsOK;
        }

        public bool GammaQuit(int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_GammaQuit();
            Send(sCMD);
            //LogHelper.WriteToLog("SEND GammaQuit：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE GammaQuit：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_GammaQuit(sReceive);
            return bIsOK;
        }

        public bool GammaSaveMeasureData(List<GammaXYLv> GammaXYLvs, int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_GammaSaveMeasureData(GammaXYLvs);
            Send(sCMD);
            //LogHelper.WriteToLog("SEND GammaSaveMeasureData：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE GammaSaveMeasureData：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_GammaSaveMeasureData(sReceive);
            return bIsOK;
        }

        public List<GammaXYLv> GammaGetMeasureData(int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_GammaGetMeasureData();
            Send(sCMD);
            //LogHelper.WriteToLog("SEND GammaGetMeasureData：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE GammaGetMeasureData：" + sReceive, LogLevel.DEBUG);
            List<GammaXYLv> GammaXYLvs = new List<GammaXYLv>();
            GammaXYLvs = HkcJson.DecodeJsonCMD_GammaGetMeasureData(sReceive);
            return GammaXYLvs;
        }
        // ----------Gamma----------

        // ----------WB----------
        public bool WBSetUserPattern(int nR, int nG, int nB, int nReceiveTimeout = 1000)
        {

            return true;
        }



        public bool WBSave(int nNum, int nR, int nG, int nB, int nReceiveTimeout = 1000)
        {
            string sNum = "MS_COLOR_TEMP_NATURE";
            switch (nNum)
            {
                case 0:
                    sNum = "MS_COLOR_TEMP_NATURE";
                    break;
                case 1:
                    sNum = "MS_COLOR_TEMP_COOL";
                    break;
                case 2:
                    sNum = "MS_COLOR_TEMP_WARM";
                    break;
                case 10:
                    sNum = "sRGB";
                    break;
                case 11:
                    sNum = "P3";
                    break;
                case 12:
                    sNum = "Adobe";
                    break;
                case 13:
                    sNum = "Bluefilter";
                    break;
                case 14:
                    sNum = "HDR";
                    break;
                case 15:
                    sNum = "Bluefilter";
                    break;
                case 16:
                    sNum = "HDR";
                    break;
                default:
                    sNum = "MS_COLOR_TEMP_"+ (nNum+1).ToString();
                    break;
            }
            
            string sCMD = HkcJson.EncodeJsonCMD_WBSave(sNum, nR, nG, nB);
            Send(sCMD);
            Thread.Sleep(200);
            LogHelper.WriteToLog("SEND WBSave：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            LogHelper.WriteToLog("RECEIVE WBSave：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_WBSave(sReceive);
            Console.WriteLine("---WB " + sNum.ToString() + " RGB : " + nR.ToString() + " " + nG.ToString() + " " + nB.ToString(), LogLevel.INFO);
            LogHelper.WriteToLog("保存WB " + sNum.ToString() + " RGB : " + nR.ToString() + " " + nG.ToString() + " " + nB.ToString(), LogLevel.INFO);
            return bIsOK;
        }

        public bool WBQuit(int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_WBQuit();
            Send(sCMD);
            Thread.Sleep(100);
            //LogHelper.WriteToLog("SEND WBQuit：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE WBQuit：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_WBQuit(sReceive);
            return bIsOK;
        }
        public bool WBClose(int nReceiveTimeout = 1000)
        {
            patternf.Hide();
            string sCMD = HkcJson.EncodeJsonCMD_WBClose();
            Send(sCMD);
            Thread.Sleep(100);
            //LogHelper.WriteToLog("SEND WBQuit：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            LogHelper.WriteToLog("RECEIVE WBClose：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_WBClose(sReceive);
            return bIsOK;
        }
        // ----------WB----------

        // ----------Lv----------
        public int LvGetEBook(int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_GetEBookValue();
            Send(sCMD);
            Thread.Sleep(100);
            //LogHelper.WriteToLog("SEND GetMaxBackLightValue：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE GetMaxBackLightValue：" + sReceive, LogLevel.DEBUG);
            int nBackLight = HkcJson.DecodeJsonCMD_GetEBookValue(sReceive);
            return nBackLight;
        }
        public bool LvSetEBook(int nBackLight, int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_SetEBookValue(nBackLight);
            Send(sCMD);
            Thread.Sleep(100);
            //LogHelper.WriteToLog("SEND SetBackLight：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            LogHelper.WriteToLog("RECEIVE LvSetEBook：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_SetEBookValue(sReceive);
            return bIsOK;
        }
        public bool LvSaveEBook(int nBackLight, int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_SaveEBookValue(nBackLight);
            Send(sCMD);
            Thread.Sleep(100);
            //LogHelper.WriteToLog("SEND SetBackLight：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE SetBackLight：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_SaveEBookValue(sReceive);
            return bIsOK;
        }
        public int LvGetMax(int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_GetMaxBackLightValue();
            Send(sCMD);
            Thread.Sleep(100);
            //LogHelper.WriteToLog("SEND GetMaxBackLightValue：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE GetMaxBackLightValue：" + sReceive, LogLevel.DEBUG);
            int nBackLight = HkcJson.DecodeJsonCMD_GetMaxBackLightValue(sReceive);
            return nBackLight;
        }

        public int LvGet(int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_GetBackLight();
            Send(sCMD);
            Thread.Sleep(100);
            //LogHelper.WriteToLog("SEND GetBackLight：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE GetBackLight：" + sReceive, LogLevel.DEBUG);
            int nBackLight = HkcJson.DecodeJsonCMD_GetBackLight(sReceive);
            return nBackLight;
        }

        public bool LvSet(int nBackLight, int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_SetBackLight(nBackLight);
            Send(sCMD);
            Thread.Sleep(100);
            //LogHelper.WriteToLog("SEND SetBackLight：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            LogHelper.WriteToLog("RECEIVE SetBackLight：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_SetBackLight(sReceive);
            return bIsOK;
        }
        public bool LvSave(int nBackLight, int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_SaveBackLight(nBackLight);
            Send(sCMD);
            Thread.Sleep(100);
            //LogHelper.WriteToLog("SEND SetBackLight：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE SetBackLight：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_SaveBackLight(sReceive);
            return bIsOK;
        }
        public bool MaxLvSave(int nBackLight, int nReceiveTimeout = 1000)
        {
            string sCMD = HkcJson.EncodeJsonCMD_SaveMaxBackLight(nBackLight);
            Send(sCMD);
            Thread.Sleep(100);
            //LogHelper.WriteToLog("SEND SetBackLight：" + sCMD, LogLevel.DEBUG);
            string sReceive = Receive(nReceiveTimeout);
            //LogHelper.WriteToLog("RECEIVE SetBackLight：" + sReceive, LogLevel.DEBUG);
            bool bIsOK = HkcJson.DecodeJsonCMD_SaveMaxBackLight(sReceive);
            return bIsOK;
        }
        // ----------Lv----------
        // ----------EDID----------
        public bool SetEDID_SN(string sSN, int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool reSN = false;
            Thread.Sleep(100);
            string sCMD = HkcJson.EncodeJsonCMD_SetEDID_string("saveSN",sSN);
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            reSN = HkcJson.DecodeJsonCMD_SetEDID_string("autotest.EDID.saveSN", sReceive);

            LogHelper.WriteToLog("设置EDID SN返回值：" + reSN, LogLevel.INFO);
            return reSN;
        }
        public bool SetEDID_ID(string sID, int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool reSN = false;
            Thread.Sleep(100);
            string sCMD = HkcJson.EncodeJsonCMD_SetEDID_string("saveID", sID);
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            reSN = HkcJson.DecodeJsonCMD_SetEDID_string("autotest.EDID.saveID", sReceive);

            LogHelper.WriteToLog("设置EDID ID返回值：" + reSN, LogLevel.INFO);
            return reSN;
        }
        public bool SetEDID_PRODUCT_CODE(int  scode, int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool reSN = false;
            Thread.Sleep(100);
            string sCMD = HkcJson.EncodeJsonCMD_SetEDID_PRODUCT_CODE(scode);
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            reSN = HkcJson.DecodeJsonCMD_SetEDID_PRODUCT_CODE(sReceive);

            LogHelper.WriteToLog("设置EDID PRODUCT_CODE返回值：" + reSN, LogLevel.INFO);
            return reSN;
        }
        public bool SetEDID_DATE(int year,int week, int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool reSN = false;
            Thread.Sleep(100);
            string sCMD = HkcJson.EncodeJsonCMD_SetEDID_DATE(year,week);
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            reSN = HkcJson.DecodeJsonCMD_SetEDID_DATE(sReceive);

            LogHelper.WriteToLog("设置EDID DATE返回值：" + reSN, LogLevel.INFO);
            return reSN;
        }
        public bool SetEDID_NAME(string sNAME, int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool reSN = false;
            Thread.Sleep(100);
            string sCMD = HkcJson.EncodeJsonCMD_SetEDID_string("saveName", sNAME);
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            reSN = HkcJson.DecodeJsonCMD_SetEDID_string("autotest.EDID.saveName", sReceive);

            LogHelper.WriteToLog("设置EDID Name返回值：" + reSN, LogLevel.INFO);
            return reSN;
        }
        // ----------EDID----------
        // ----------ColorGamut----------
        public bool SetColorGamut(int [] nuble, string methodName,int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool reSN = false;
            Thread.Sleep(100);
            string sCMD = HkcJson.EncodeJsonCMD_SeGamut(methodName, nuble);
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            reSN = HkcJson.DecodeJsonCMD_SeGamut("autotest.Gamut."+ methodName, sReceive);

            if(!reSN) LogHelper.WriteToLog("设置"+ methodName + "ColorGamut 返回值：" + sReceive, LogLevel.INFO);
            return reSN;
        }
        // ----------ColorGamut----------
        // ----------Color----------
        public bool ColorSetPattern(ColorSpace.RGB rgb, int nReceiveTimeout)
        {
            GammaSetPattern((int)rgb.R, (int)rgb.G, (int)rgb.B, nReceiveTimeout);
            return true;
        }

        public bool ColorSendData(string  gamut, ColorSpace.RGB[] nRGBArr, int nReceiveTimeout = 1000)
        {
            string sCMD = string.Empty;
            bool bIsOK = false;
            string CRC = string.Empty;
            int [] nRArr1 = new int[nRGBArr.Length];
            int[] nGArr1 = new int[nRGBArr.Length];
            int[] nBArr1 = new int[nRGBArr.Length];
            for (int i = 0; i < nRGBArr.Length; i++)
            {
                nRArr1[i] =  (int)nRGBArr[i].R;
                  nGArr1[i] = (int)nRGBArr[i].G;
                nBArr1[i] = (int)nRGBArr[i].B;           
            }
         //  nRArr1[17-1] = 1;
         //  nGArr1[17-1] = 1;
         //  nBArr1[17-1] = 1;
            byte num = 0;
            byte[] hex = new byte[2];
            byte num1 = 0;
            byte[] hex1 = new byte[2];
            byte num2 = 0;
            byte[] hex2 = new byte[2];
            byte num3 = 0;
            for (int i = 0; i < nRArr1.Length; i++)
            {

                    hex[0] = (byte)((int)nRArr1[i] & 0xff);
                    hex[1] = (byte)(((int)nRArr1[i] >> 8) & 0xff);
                    num = (byte)(num + hex[0] + hex[1]);
                

                    hex1[0] = (byte)((int)nGArr1[i] & 0xff);
                    hex1[1] = (byte)(((int)nGArr1[i] >> 8) & 0xff);
                    num1 = (byte)(num1 + hex1[0] + hex1[1]);
                

                    hex2[0] = (byte)((int)nBArr1[i] & 0xff);
                    hex2[1] = (byte)(((int)nBArr1[i] >> 8) & 0xff);
                    num2 = (byte)(num2 + hex2[0] + hex2[1]);
                
            }

            num3 = (byte)(num + num1 + num2);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 1; j < 40; j++)
                {
                    int[] nums1 = null;
                    switch (i)
                    {
                        case 0:
                              if (j == 39)
                                nums1 = nRArr1.Skip(0 + 128 * (j - 1)).Take(49).ToArray();
                              else nums1 = nRArr1.Skip((0 + 128 * (j - 1))).Take(128).ToArray();
                            break;
                        case 1:
                            if (j == 39) nums1 = nGArr1.Skip(0 + 128 * (j - 1)).Take(49).ToArray();
                            else nums1 = nGArr1.Skip((0 + 128 * (j - 1))).Take(128).ToArray();
                            break;
                        case 2:
                            if (j == 39) nums1 = nBArr1.Skip(0 + 128 * (j - 1)).Take(49).ToArray();
                            else nums1 = nBArr1.Skip((0 + 128 * (j - 1))).Take(128).ToArray();
                            break;

                    }
                    sCMD = HkcJson.EncodeJsonCMD_ColorSaveDataN(i, j, nums1);
                    //LogHelper.WriteToLog(sCMD, LogLevel.DEBUG);
                    Thread.Sleep(100);
                    Send(sCMD);
                    //LogHelper.WriteToLog("SEND GammaSave：" + sCMD, LogLevel.DEBUG);
                    Thread.Sleep(100);
                    string sReceive = Receive(nReceiveTimeout);
                   // LogHelper.WriteToLog("RECEIVE 3DLUT Data ：" + sReceive+"  ,RGB:" +i+" , DATA : "+j, LogLevel.DEBUG);
                    bIsOK = HkcJson.DecodeJsonCMD_ColorDataN(sReceive);
                    if (!bIsOK) break;
                }
            }
            if (bIsOK)
            {
                
                Thread.Sleep(100);
                sCMD = HkcJson.EncodeJsonCMD_ColorCRC(gamut);
                //LogHelper.WriteToLog(sCMD, LogLevel.DEBUG);
                Send(sCMD);
                Thread.Sleep(100);
                string sReceive = Receive(30000);
                LogHelper.WriteToLog("RECEIVE 3DLUT CRC：" + sReceive, LogLevel.DEBUG);
                CRC = HkcJson.DecodeJsonCMD_ColorCRC(sReceive);
                if (num3.ToString() != CRC) bIsOK = false;
            }
            return bIsOK;
        }
        // ----------Color----------
        // ----------Other----------
        public string GetSystemInfo( int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            string sOriginalProductModel = string.Empty;

                string sCMD = HkcJson.EncodeJsonCMD_GetSystemInfo();
                Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
                sOriginalProductModel = HkcJson.DecodeJsonCMD_GetSystemInfo(sReceive);

            LogHelper.WriteToLog("获取机型产品型号返回值：" + sReceive, LogLevel.INFO);
            return sOriginalProductModel;
        }
        public bool SetSN_Display(int  sSND, int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool reSN = false;
            Thread.Sleep(100);
            string sCMD = HkcJson.EncodeJsonCMD_SetSN_Display(sSND);
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            reSN = HkcJson.DecodeJsonCMD_SetSN_Display(sReceive);

            LogHelper.WriteToLog("设置机型SN显示位数返回值：" + reSN, LogLevel.INFO);
            return reSN;
        }
        public bool Set_OnOffFunct(int sSND, int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool re = false;
            Thread.Sleep(100);
            string sCMD = HkcJson.EncodeJsonCMD_SetOthers(sSND, "SleepTimerSwitch");
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            re = HkcJson.DecodeJsonCMD_SetOthers(sReceive, "SleepTimerSwitch");

            LogHelper.WriteToLog("设置Set_OnOffFunct返回值：" + re, LogLevel.INFO);
            return re;
        }
        public bool SetEnergyTips(int sSND, int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool re = false;
            Thread.Sleep(100);
            string sCMD = HkcJson.EncodeJsonCMD_SetOthers(sSND, "energyTips");
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            re = HkcJson.DecodeJsonCMD_SetOthers(sReceive, "energyTips");

            LogHelper.WriteToLog("设置机型EnergyTips返回值：" + re, LogLevel.INFO);
            return re;
        }
        public bool Set_OSDdefLvValue(int sSND, int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool re = false;
            Thread.Sleep(100);
            string sCMD = HkcJson.EncodeJsonCMD_SetOthers(sSND, "OSDdefLightValue");
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            re = HkcJson.DecodeJsonCMD_SetOthers(sReceive, "OSDdefLightValue");

            LogHelper.WriteToLog("设置Set_OSDdefLvValue返回值：" + re, LogLevel.INFO);
            return re;
        }
        public bool SetLanguage(int sSND, int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool re = false;
            Thread.Sleep(100);
            string sCMD = HkcJson.EncodeJsonCMD_SetLanguage(sSND);
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            re = HkcJson.DecodeJsonCMD_SetLanguage(sReceive);

            LogHelper.WriteToLog("设置机型Language返回值：" + re, LogLevel.INFO);
            return re;
        }
        public bool SetMode(string methodname,int nReceiveTimeout = 1000)
        {
            string sReceive = string.Empty;
            bool re = false;
            string sCMD = HkcJson.EncodeJsonCMD_SetMode(methodname);
            Send(sCMD);
            Thread.Sleep(100);
            sReceive = Receive(nReceiveTimeout);
            re = HkcJson.DecodeJsonCMD_SetMode(sReceive, methodname);

            LogHelper.WriteToLog("SetMode返回值：" + sReceive, LogLevel.INFO);
            return re;
        }
        public void SetWindowSize(int WindowSize)
        {
            mmWindowSize = WindowSize;

        }
    }
}
