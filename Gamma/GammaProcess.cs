using CA200SRVRLib;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using static WhiteBalanceCorrection.ColorSpace;

namespace WhiteBalanceCorrection
{
    class GammaProcess
    {
        private IHkcCommunication m_HkcCommunication;

        private GammaScreenMode m_ScreenMode = GammaScreenMode.SCN_RGB;
        private GammaXYLv[] m_GammaXYLvValue;
        private GammaXYLv[] m_GammaGrayXYLvValue;
        private int m_nGammaXYLvValueNum = 0;
        private int m_nGammaXYLvValueNumAll = 0;
        private int m_nNormalNum = 255, m_nExtendNum = 255;
        private GammaXYZ[] m_GXYZOutputArr;
        private GammaRGB[] m_GRGBOutputArr;
        public GammaRGB[] m_GRGBOutputArrExtend = null;
        private float[] m_GammaWhiteLv;
        private double[] m_GammaWhiteIndex;
        private int m_nMCU = 98;
        public GammaProcess(IHkcCommunication SCommunication)
        {
            m_HkcCommunication = SCommunication;
        }

        public bool StartGamma(int nReceiveTimeout = 1000)
        {
            bool bIsOK = m_HkcCommunication.GammaStart();
            return bIsOK;
        }

        public ProbeStruct Measure(IDisplayColorAnalyzer DCA)
        {
            ProbeStruct Probe = DCA.MeasureEx();
            return Probe;
        }

        public bool MeasureOneEx(bool bSuccess, IHkcCommunication SCommunication, IDisplayColorAnalyzer DCA, int nProductCommonTimeout,
                int nProductPatternDelay, GammaXYLv[] GammaXYLvValue, int nGammaXYLvValueNum2, int nR, int nG, int nB, int[] nIndexs)
        {
            if (false == bSuccess)
            {
                return bSuccess;
            }
            for (int i = 0; i < nIndexs.Length; i++)
            {
                if (nIndexs[i] >= nGammaXYLvValueNum2)
                {
                    bSuccess = false;
                    break;
                }
            }
            if (bSuccess)
            {
                if (SCommunication.GammaSetPattern(nR, nG , nB , nProductCommonTimeout))
                {
                    LogHelper.WriteToLog("设置Gamma画面：" + nR.ToString() + " " + nG.ToString() + " " + nB.ToString(), LogLevel.INFO);
                    // 开始测量
                    Thread.Sleep(nProductPatternDelay);
                    ProbeStruct Probe = DCA.MeasureEx();
                    for (int i = 0; i < nIndexs.Length; i++)
                    {
                        GammaXYLvValue[nIndexs[i]].fX = Probe.sx;
                        GammaXYLvValue[nIndexs[i]].fY = Probe.sy;
                        GammaXYLvValue[nIndexs[i]].fLv = Probe.Lv;
                        GammaXYLvValue[nIndexs[i]].fXX = Probe.X ;
                        GammaXYLvValue[nIndexs[i]].fYY = Probe.Y ;
                        GammaXYLvValue[nIndexs[i]].fZZ = Probe.Z ;
                    }
                }
                else
                {
                    bSuccess = false;
                }
            }

                return bSuccess;
        }
        GammaRGB[] SRGB = new GammaRGB[27];
  
        public bool MeasureAll(int nMeasurePics, IDisplayColorAnalyzer DCA, int nTimeout, int nProductPatternDelay)
        {
            bool bSuccess = true;
            int size = 9;
            switch (nMeasurePics)
            {
                case 13:
                    m_nGammaXYLvValueNum = 8;
                    m_nGammaXYLvValueNumAll = 20; // (4 + 1) * 4
                    size = 5;
                    break;
                case 25:
                    m_nGammaXYLvValueNum = 12;
                    m_nGammaXYLvValueNumAll = 36; // (8 + 1) * 4
                    break;
                case 49:
                    m_nGammaXYLvValueNum = 20;
                    m_nGammaXYLvValueNumAll = 68; // (16 + 1) * 4
                    break;
                case 97:
                    m_nGammaXYLvValueNum = 36;
                    m_nGammaXYLvValueNumAll = 132; // (32 + 1) * 4
                    break;
                case 193:
                    m_nGammaXYLvValueNum = 68;
                    m_nGammaXYLvValueNumAll = 260; // (64 + 1) * 4
                    break;
                case 385:
                    m_nGammaXYLvValueNum = 132;
                    m_nGammaXYLvValueNumAll = 516; // (128 + 1) * 4
                    break;
            }
            m_GammaXYLvValue = new GammaXYLv[m_nGammaXYLvValueNumAll];
            for (int i = 0; i < m_nGammaXYLvValueNumAll; i++)
            {
                m_GammaXYLvValue[i] = new GammaXYLv();
            }

            GammaRGB[] gRGB = new GammaRGB[nMeasurePics];
            gRGB[0] = new GammaRGB(0, 0, 0);
            int nRGBNum = (nMeasurePics + 2) / 3 - 1;
            for (int i = 0; i < nRGBNum; i++)
            {
                gRGB[i + 1] = new GammaRGB(256 / nRGBNum * (i + 1), 0, 0);
            }
            for (int i = 0; i < nRGBNum; i++)
            {
                gRGB[i + nRGBNum + 1] = new GammaRGB(0, 256 / nRGBNum * (i + 1), 0);
            }
            for (int i = 0; i < nRGBNum; i++)
            {
                gRGB[i + nRGBNum * 2 + 1] = new GammaRGB(0, 0, 256 / nRGBNum * (i + 1));
            }
            for (int i = 0; i < nMeasurePics; i++)
            {
                gRGB[i].nR = (gRGB[i].nR >= 256) ? 255 : gRGB[i].nR;
                gRGB[i].nG = (gRGB[i].nG >= 256) ? 255 : gRGB[i].nG;
                gRGB[i].nB = (gRGB[i].nB >= 256) ? 255 : gRGB[i].nB;
            }
            int[][] nIndexs = new int[nMeasurePics][];
            switch (nMeasurePics)
            {
                case 13:
                    nIndexs[0] = new int[] { 0, 5, 10, 15 };
                    break;
                case 25:
                    nIndexs[0] = new int[] { 0, 9, 18, 27 };
                    break;
                case 49:
                    nIndexs[0] = new int[] { 0, 17, 34, 51 };
                    break;
                case 97:
                    nIndexs[0] = new int[] { 0, 33, 66, 99 };
                    break;
                case 193:
                    nIndexs[0] = new int[] { 0, 65, 130, 195 };
                    break;
                case 385:
                    nIndexs[0] = new int[] { 0, 129, 258, 387 };
                    break;
            }
            for (int i = 0; i < nRGBNum; i++)
            {
                nIndexs[i + 1] = new int[] { (i + 1) + (nRGBNum + 1) };
            }
            for (int i = 0; i < nRGBNum; i++)
            {
                nIndexs[i + nRGBNum + 1] = new int[] { (i + 1) + (nRGBNum + 1) * 2 };
            }
            for (int i = 0; i < nRGBNum; i++)
            {
                nIndexs[i + nRGBNum * 2 + 1] = new int[] { (i + 1) + (nRGBNum + 1) * 3 };
            }
            if (m_HkcCommunication.GammaSetPattern(255, 255, 255, nTimeout))
            {
                Thread.Sleep(nProductPatternDelay);
                ProbeStruct Probe = Measure(DCA);
                m_whiteLight = Probe.Lv;
            }
            if (m_HkcCommunication == null)
            {
                bSuccess = false;
            }
            else
            {/*
                for (int i = 0; i < nMeasurePics; i++)
                {
                    bSuccess = MeasureOneEx(bSuccess, m_HkcCommunication, DCA, nTimeout, nProductPatternDelay,
                        m_GammaXYLvValue, m_nGammaXYLvValueNumAll, gRGB[i].nR, gRGB[i].nG, gRGB[i].nB, nIndexs[i]);
                }*/
                
                 m_GammaWhiteLv = new float[m_nGammaXYLvValueNumAll / 4];
                 m_GammaWhiteIndex = new double[m_nGammaXYLvValueNumAll / 4];
               for (int i = 0; i < m_nGammaXYLvValueNumAll / 4; i++)
               {
                   if (m_HkcCommunication.GammaSetPattern(gRGB[i].nR, gRGB[i].nR, gRGB[i].nR, nTimeout))
                   {
                       LogHelper.WriteToLog("设置Gamma画面：" + gRGB[i].nR.ToString() + " " + gRGB[i].nR.ToString() + " " + gRGB[i].nR.ToString(), LogLevel.INFO);
                       // 开始测量
                       Thread.Sleep(nProductPatternDelay);
                       ProbeStruct Probe = DCA.MeasureEx();
                       m_GammaWhiteLv[i] = Probe.Lv;
                        m_GammaXYLvValue[i].fX = Probe.sx;
                        m_GammaXYLvValue[i].fY = Probe.sy;
                        m_GammaXYLvValue[i].fLv = Probe.Lv;
                        m_GammaXYLvValue[i].fXX = Probe.X;
                        m_GammaXYLvValue[i].fYY = Probe.Y;
                        m_GammaXYLvValue[i].fZZ = Probe.Z;
                        //Thread.Sleep(100);
                   }
               }
               m_GammaWhiteIndex[0] = 2.2;
               m_GammaWhiteIndex[m_nGammaXYLvValueNumAll / 4 - 1] = 2.2;

               for (int i = 1; i < m_nGammaXYLvValueNumAll / 4-1; i++)
               {
                   double a = (double)gRGB[i].nR / gRGB[m_nGammaXYLvValueNumAll / 4 - 1].nR;
                   double b = m_GammaWhiteLv[i] / m_GammaWhiteLv[m_nGammaXYLvValueNumAll / 4 - 1];
                   m_GammaWhiteIndex[i] = Math.Log(b)/ Math.Log(a);
               }
            }
            

            



            return bSuccess;
        }
        private class MapRGB
        {
            public int size;
            public double[] R = new double[256];
            public double[] G = new double[256];
            public double[] B = new double[256];
            public double[] RLv = new double[256];
            public double[] GLv = new double[256];
            public double[] BLv = new double[256];
            public double[] RX = new double[256];
            public double[] GX = new double[256];
            public double[] BX = new double[256];
            public double[] RZ = new double[256];
            public double[] GZ = new double[256];
            public double[] BZ = new double[256];

            public double[] RGBLv = new double[256];
            public double[] RGBX = new double[256];
            public double[] RGBZ = new double[256];

            public double[] R_a = new double[256];
            public double[] R_b = new double[256];
            public double[] R_c = new double[256];

            public double[] G_a = new double[256];
            public double[] G_b = new double[256];
            public double[] G_c = new double[256];

            public double[] B_a = new double[256];
            public double[] B_b = new double[256];
            public double[] B_c = new double[256];

        }
        private double[] m_transforMatrix = new double[9];
        private double[] m_invTransforMatrix = new double[9];
        private readonly MapRGB m_mapRGB = new MapRGB();
        private CIEXYZ m_blackXYZ;
        private readonly double[] m_maxRGBLight = new double[3];

        private ColorTemperature m_colorTemperature = ColorTemperature.D65;
        private SpaceType m_spaceType = SpaceType.sRGB;
        private Interpolation m_interpolation = Interpolation.PowerExponent;
        private double m_whiteLight;
        private static readonly double m_min2Zero = 0.0;
       
      
  
  
        public bool CheckGammaPattern()
        {
            bool bSuccess = true;
            if (m_nGammaXYLvValueNumAll != 0)
            {
                int nNumTmp = m_nGammaXYLvValueNumAll / 4;
                for (int i = 1; i < nNumTmp-1; i++)
                {
                    /* if (m_GammaXYLvValue[i + nNumTmp].fLv < m_GammaXYLvValue[i - 1 + nNumTmp].fLv ||
                         m_GammaXYLvValue[i + nNumTmp * 2].fLv < m_GammaXYLvValue[i - 1 + nNumTmp * 2].fLv ||
                         m_GammaXYLvValue[i + nNumTmp * 3].fLv < m_GammaXYLvValue[i - 1 + nNumTmp * 3].fLv)*/
                    if (m_GammaXYLvValue[i  ].fLv < m_GammaXYLvValue[i - 1 ].fLv)
                     {
                        bSuccess = false;
                        break;
                    }
                }
            }

            LogHelper.WriteToLog("CheckGammaPattern :  " + bSuccess, LogLevel.INFO);
            return bSuccess;
        }

        public bool GetGRGBOutputArr(float fGamma,/* float fTargetX, float fTargetXRange, float fTargetY,
                        float fTargetYRange, */int nBelowR, int nBelowG, int nBelowB, int nExtendNum, int nDataRange, ref float fX, ref float fY,int LowgrayLinear)
        {
            bool bSuccess = true;
            m_GXYZOutputArr = new GammaXYZ[m_nNormalNum + 1];
            for (int i = 0; i <= m_nNormalNum; i++)
            {
                m_GXYZOutputArr[i] = new GammaXYZ();
            }
            m_GRGBOutputArr = new GammaRGB[m_nNormalNum + 1];
            for (int i = 0; i <= m_nNormalNum; i++)
            {
                m_GRGBOutputArr[i] = new GammaRGB();
            }
            m_nExtendNum = nExtendNum;
            if (m_GRGBOutputArrExtend == null)
            {
                m_GRGBOutputArrExtend = new GammaRGB[m_nExtendNum + 1];
                for (int i = 0; i <= m_nExtendNum; i++)
                {
                    m_GRGBOutputArrExtend[i] = new GammaRGB();
                }
            }
            bSuccess = CalcGamma.CalcGammaData2(m_ScreenMode, fGamma,/* fTargetX, fTargetXRange, fTargetY, fTargetYRange,*/
                        m_GammaXYLvValue, m_nGammaXYLvValueNum, m_GXYZOutputArr, m_GRGBOutputArr, m_nNormalNum,
                        ref m_GRGBOutputArrExtend, m_nExtendNum, nDataRange, nBelowR, nBelowG, nBelowB, ref fX, ref fY, m_GammaWhiteIndex, LowgrayLinear);
            return bSuccess;
        }
        public bool SetGammaRGBDataOri(int m_nExtendNum, int nDataRange, int nTimeout,int index,int mMCU)
        {
            m_nMCU = mMCU;
            int[] nRArr = new int[m_nExtendNum ];
            int[] nGArr = new int[m_nExtendNum ];
            int[] nBArr = new int[m_nExtendNum ];
           // var bsSuccess = m_HkcCommunication.SetMode("gamma2", nTimeout);
            for (int i = 0; i < m_nExtendNum; i++)
            {
                nRArr[i] = (int)((float)nDataRange / (float)m_nExtendNum * (float)i);
                nGArr[i] = (int)((float)nDataRange / (float)m_nExtendNum * (float)i);
                nBArr[i] = (int)((float)nDataRange / (float)m_nExtendNum * (float)i);
                nRArr[i] = (nRArr[i] >= nDataRange) ? nDataRange - 1 : nRArr[i];
                nGArr[i] = (nGArr[i] >= nDataRange) ? nDataRange - 1 : nGArr[i];
                nBArr[i] = (nBArr[i] >= nDataRange) ? nDataRange - 1 : nBArr[i];
            }

            bool bSuccess = m_HkcCommunication.GammaSave(index, nRArr, nGArr, nBArr, nTimeout, m_nMCU);
            Thread.Sleep(100);
            if (bSuccess)
                if (index==10) bSuccess = m_HkcCommunication.SetMode("sRGB", nTimeout);
                else if(index == 11) bSuccess = m_HkcCommunication.SetMode("P3", nTimeout);
                else if(index == 12) bSuccess = m_HkcCommunication.SetMode("Adobe", nTimeout);
                else      bSuccess = m_HkcCommunication.SetMode("gamma1", nTimeout);
           // if (bSuccess) bSuccess = m_HkcCommunication.SetMode("sRGB", nTimeout);
             return bSuccess;
        }

        public bool SaveGammaRGBData(int black,int nGammaIndex, int nTimeout)
        {
            bool bSuccess = true;
            if (m_HkcCommunication == null)
            {
                bSuccess = false;
            }
            else
            {
                int[] nRArr = new int[m_nExtendNum  ];
                int[] nGArr = new int[m_nExtendNum ];
                int[] nBArr = new int[m_nExtendNum ];
                string arr = string.Empty;
                string agg = string.Empty;
                string abb = string.Empty;

       

                for (int i = 0; i < m_nExtendNum; i++)
                {
                    nRArr[i] = (m_GRGBOutputArrExtend[i].nR != null) ? m_GRGBOutputArrExtend[i].nR : 0;
                    nGArr[i] = (m_GRGBOutputArrExtend[i].nG != null) ? m_GRGBOutputArrExtend[i].nG : 0;
                    nBArr[i] = (m_GRGBOutputArrExtend[i].nB != null) ? m_GRGBOutputArrExtend[i].nB : 0;
                }

                if (black > 0)
                {
                    var k = (nRArr[30] - nRArr[black]) / 30;
                    var b = nRArr[30] - k * 30;
                    for (int i = 1; i < 30; i++)
                    {
                        nRArr[i] = i * k + b;
                        nGArr[i] = nRArr[i];
                    }
                }
                else if (black < 0)
                {
                    var k = (nRArr[30+ black] - nRArr[0]) / 30;
                    var b = nRArr[30+ black] - k * 30;
                    for (int i = 1; i < 30; i++)
                    {
                        nRArr[i] = i * k + b;
                        nGArr[i] = nRArr[i];
                    }
                }

                for (int i = 0; i < m_nExtendNum; i++)
                {
                    arr += nRArr[i].ToString() + ",";
                    agg += nGArr[i].ToString() + ",";
                    abb += nBArr[i].ToString() + ",";
                }
                bSuccess = m_HkcCommunication.GammaSave(nGammaIndex, nRArr, nGArr, nBArr, nTimeout, m_nMCU);
                LogHelper.WriteToLog("Gamma数值RR:" + arr, LogLevel.INFO);
                LogHelper.WriteToLog("Gamma数值GG:" + agg, LogLevel.INFO);
                LogHelper.WriteToLog("Gamma数值BB:" + abb, LogLevel.INFO);
            }
            LogHelper.WriteToLog("保存Gamma数值:" + nGammaIndex.ToString()+"  "+ bSuccess, LogLevel.INFO);

            return bSuccess;
        }

   

        public GammaRGB GetGammaMaxRGBData()
        {
            GammaRGB RGBData = new GammaRGB();
            RGBData.nR = (m_GRGBOutputArrExtend[m_nExtendNum-1].nR != null) ? m_GRGBOutputArrExtend[m_nExtendNum - 1].nR : 0;
            RGBData.nG = (m_GRGBOutputArrExtend[m_nExtendNum-1].nG != null) ? m_GRGBOutputArrExtend[m_nExtendNum - 1].nG : 0;
            RGBData.nB = (m_GRGBOutputArrExtend[m_nExtendNum-1].nB != null) ? m_GRGBOutputArrExtend[m_nExtendNum - 1].nB : 0;
            return RGBData;
        }


    }
}
