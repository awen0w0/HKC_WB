using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WhiteBalanceCorrection
{
    class LvProcess
    {
        private IHkcCommunication m_HkcCommunication;

        public LvProcess(IHkcCommunication SCommunication)
        {
            m_HkcCommunication = SCommunication;
        }

        public ProbeStruct Measure(IDisplayColorAnalyzer DCA)
        {
            ProbeStruct Probe = DCA.MeasureEx();
            LogHelper.WriteToLog("亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);
            return Probe;
        }

        public bool CheckLv(float fLv, float fTargetLv)
        {
            bool bOK = false;
            if (fLv >= fTargetLv)
            {
                bOK = true;
            }
            return bOK;
        }

        public bool AdjustmengEbookLv(IDisplayColorAnalyzer DCA, ref int EbookLV, int LastEbookLV, int ProductPatternDelay, int nCMDTimeout )
        {
            int DefLv = m_HkcCommunication.LvGetEBook(nCMDTimeout);
            LogHelper.WriteToLog("获取E-Book OSD Value : " + DefLv.ToString(), LogLevel.INFO);
            //Thread.Sleep(ProductPatternDelay);
            m_HkcCommunication.LvSaveEBook(DefLv);
            m_HkcCommunication.SetMode("Ebook", nCMDTimeout);
            EbookLV = DefLv;
            Thread.Sleep(ProductPatternDelay);
            ProbeStruct Probe = DCA.MeasureEx();
            LogHelper.WriteToLog("E-Book亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);
            int nLvAdjStep = 1;
            int times = 0;
            if (Math.Abs(Probe.Lv - 150) <= 4)
            {
                return true;
            }
            else if (LastEbookLV != 0)
            {
                EbookLV = LastEbookLV;
                //Thread.Sleep(100);
                m_HkcCommunication.LvSaveEBook(EbookLV);
                m_HkcCommunication.SetMode("Ebook", nCMDTimeout);
                Thread.Sleep(ProductPatternDelay);
                Probe = DCA.MeasureEx();
                LogHelper.WriteToLog("E-Book OSD Value:" + EbookLV.ToString() + "E-Book 亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);
                if (Math.Abs(Probe.Lv - 150) <= 4)   return true;
                
            }
            while (Math.Abs(Probe.Lv - 150) > 4)
            {
                if (Probe.Lv > 150)
                {
                    EbookLV -= nLvAdjStep;
                    //Thread.Sleep(100);
                    m_HkcCommunication.LvSaveEBook(EbookLV);
                    m_HkcCommunication.SetMode("Ebook", nCMDTimeout);
                    Thread.Sleep(ProductPatternDelay);
                    Probe = DCA.MeasureEx();
                    LogHelper.WriteToLog("E-Book OSD Value:" + EbookLV.ToString() + "E-Book 亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);

                }
                else
                {
                    EbookLV += nLvAdjStep;
                    m_HkcCommunication.LvSaveEBook(EbookLV);
                    m_HkcCommunication.SetMode("Ebook", nCMDTimeout);
                    Thread.Sleep(ProductPatternDelay);
                    Probe = DCA.MeasureEx();
                    LogHelper.WriteToLog("E-Book OSD Value:" + EbookLV.ToString() + "E-Book 亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);
                }
                if (times > 20) return false;
                times++;
            }



            return true;
        }

            public bool AdjustmengLv(IDisplayColorAnalyzer DCA, int fDefMaxLvPWM , int fMaxLvPWM , float fTargetMaxLv , float fTargetLv, float fDefRangeLv, float fMaxRangeLv, int nLvAdjStep, int ProductPatternDelay, int nCMDTimeout, ref int nMaxBackLightOutput,ref float  m_Max_Lv, ref int nBackLightOutput,bool mbCorrectMaxLv, bool mbCorrectDefLv, int  Last_Max_Lv_Backlight, int Last_Standard_Lv_Backlight)
        {
            bool bOK = false;
            bool MaxOK = true ;
            // 先获取最大背光值，然后设置进整机
            //  int nMaxBackLight = m_HkcCommunication.LvGetMax(nCMDTimeout);
            int nMaxBackLight = fMaxLvPWM;
            if (!mbCorrectMaxLv) nMaxBackLight = fDefMaxLvPWM;
            int outBackLight = nMaxBackLight;
            LogHelper.WriteToLog("获取最大背光值 : Max BackLight:" + nMaxBackLight.ToString(), LogLevel.INFO);
            nBackLightOutput = nMaxBackLight;
            m_HkcCommunication.LvSet(nMaxBackLight);
            Thread.Sleep(ProductPatternDelay);
            ProbeStruct Probe = DCA.MeasureEx();
            LogHelper.WriteToLog("MAX_PWM亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);
            float fLv = Probe.Lv;
            float  MaxLv = fLv;
            m_Max_Lv = MaxLv;
            #region  Correct MAX_Lv
            if (mbCorrectMaxLv)
            {
                if (Math.Abs(MaxLv - fTargetMaxLv) <= fMaxRangeLv)
                {
                    nMaxBackLightOutput = nMaxBackLight;
                }
                else
                {
                    if (fTargetMaxLv > MaxLv)
                    {
                        LogHelper.WriteToLog("当前最大PWM背光值亮度 : Max BackLight Lv:" + MaxLv.ToString()+"小于目标亮度 ："+ fTargetMaxLv.ToString(), LogLevel.INFO);
                        return false;
                    }
                    if (Last_Max_Lv_Backlight != 0)
                    {
                        nMaxBackLight = Last_Max_Lv_Backlight;                      
                        m_HkcCommunication.LvSet(Last_Max_Lv_Backlight);
                        Thread.Sleep(ProductPatternDelay);
                        Probe = DCA.MeasureEx();
                        MaxLv = Probe.Lv;
                        m_Max_Lv = Probe.Lv;
                        LogHelper.WriteToLog("MAX背光值 : PWM:" + Last_Max_Lv_Backlight.ToString() + " MAX亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);
                    }
                    while (Math.Abs(MaxLv - fTargetMaxLv) > fMaxRangeLv)
                    {
                        if (MaxLv > fTargetMaxLv)
                        {
                            nMaxBackLight -= nLvAdjStep;
                            nMaxBackLightOutput = nMaxBackLight;
                            if (nMaxBackLight < outBackLight / 2)
                            {
                                MaxOK = false;
                                break;
                            }
                            m_HkcCommunication.LvSet(nMaxBackLight);
                            Thread.Sleep(ProductPatternDelay);
                            Probe = DCA.MeasureEx();
                            LogHelper.WriteToLog("MAX背光值 : PWM:" + nMaxBackLight.ToString() + " MAX亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);
                            MaxLv = Probe.Lv;
                            m_Max_Lv= Probe.Lv; 
                        }
                        else
                        {
                            if (nMaxBackLight <= fMaxLvPWM) nMaxBackLight += nLvAdjStep;
                            else
                            {
                                LogHelper.WriteToLog("当前最大背光值 : Max BackLight:" + nMaxBackLight.ToString() + "大于限制最大背光值 ：" + fMaxLvPWM.ToString(), LogLevel.INFO);
                                MaxOK = false;
                                break;
                            }
                            nMaxBackLightOutput = nMaxBackLight;
                            if (nMaxBackLight < outBackLight / 2)
                            {
                                MaxOK = false;
                                break;
                            }
                            m_HkcCommunication.LvSet(nMaxBackLight);
                            Thread.Sleep(ProductPatternDelay);
                            Probe = DCA.MeasureEx();
                            LogHelper.WriteToLog("MAX背光值 : PWM:" + nMaxBackLight.ToString() + " MAX亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);
                            MaxLv = Probe.Lv;
                            m_Max_Lv = Probe.Lv;
                        }
                    }

                }
            
            }
            #endregion 
            if (mbCorrectDefLv)
            {
                LogHelper.WriteToLog("获取默认亮度最大背光值 : Max BackLight:" + fDefMaxLvPWM.ToString(), LogLevel.INFO);
                m_HkcCommunication.LvSet(fDefMaxLvPWM);
                nBackLightOutput = fDefMaxLvPWM;
                Thread.Sleep(ProductPatternDelay);
                 Probe = DCA.MeasureEx();
                LogHelper.WriteToLog("DefMAX_PWM亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);
                 fLv = Probe.Lv;
            }
            if (fLv < fTargetLv && MaxOK && mbCorrectDefLv)
            {
                if (Math.Abs(fLv - fTargetLv) <= fDefRangeLv)
                {
                    bOK = true;
                }
                else
                {
                    LogHelper.WriteToLog("最大背光值亮度小于默认目标亮度； 最大亮度 ： " + Probe.Lv.ToString() + "    默认目标亮度 ：" + fTargetLv.ToString(), LogLevel.INFO);
                }
            }
            else if (MaxOK && mbCorrectDefLv)
            {
                if (Math.Abs(fLv - fTargetLv) <= fDefRangeLv)
                {
                    bOK = true;
                }
                else
                {
                    int nBackLight = m_HkcCommunication.LvGet(nCMDTimeout);
                    if (Last_Standard_Lv_Backlight != 0) nBackLight = Last_Standard_Lv_Backlight;
                    m_HkcCommunication.LvSet(nBackLight);
                    Thread.Sleep(ProductPatternDelay);
                    Probe = DCA.MeasureEx();
                    LogHelper.WriteToLog("亮度测量结果 : Y:" + Probe.Lv.ToString() + "    背光值 : PWM:" + nBackLight.ToString(), LogLevel.INFO);
                    nBackLightOutput = nBackLight;
                    fLv = Probe.Lv;
                    while (Math.Abs(fLv - fTargetLv) > fDefRangeLv)
                    {
                        if (fLv > fTargetLv)
                        {
                            nBackLight -= nLvAdjStep;
                            nBackLightOutput = nBackLight;
                            if (nBackLight < fDefMaxLvPWM / 4)
                            {
                                bOK = false;
                                break;
                            }
                            m_HkcCommunication.LvSet(nBackLight);
                            Thread.Sleep(ProductPatternDelay);
                            Probe = DCA.MeasureEx();
                            LogHelper.WriteToLog("背光值 : PWM:" + nBackLight.ToString() + " 亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);
                            fLv = Probe.Lv;
                        }
                        else
                        {
                            if (nBackLight > fDefMaxLvPWM)
                            {
                                LogHelper.WriteToLog("当前背光值 :  PWM:" + nBackLight.ToString() + "大于限制最大背光值 ：" + fDefMaxLvPWM.ToString(), LogLevel.INFO);
                                bOK = false;
                                break;
                            }
                            else nBackLight += nLvAdjStep;
                            nBackLightOutput = nBackLight;
                            m_HkcCommunication.LvSet(nBackLight);
                            Thread.Sleep(ProductPatternDelay);
                            Probe = DCA.MeasureEx();
                            LogHelper.WriteToLog("背光值 : PWM:" + nBackLight.ToString() + " 亮度测量结果 : Y:" + Probe.Lv.ToString(), LogLevel.INFO);
                            fLv = Probe.Lv;
                        }
                    }
                    if (Math.Abs(fLv - fTargetLv) <= fDefRangeLv)
                    {
                        bOK = true;
                    }

                }
            }
            else
                bOK=MaxOK;
            return bOK;
        }

     
    }
}
