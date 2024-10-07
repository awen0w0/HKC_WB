using CA200SRVRLib;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static KClmtrBase.KClmtrWrapper.wFlickerSetting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using static WhiteBalanceCorrection.ColorSpace;

namespace WhiteBalanceCorrection
{
    class ColorGamut
    {
        private IHkcCommunication m_HkcCommunication;
        private SpaceType m_spaceType = SpaceType.sRGB;
        private POINT rR, rG, rB, rW;
        private POINT oR, oG, oB;
        public ColorGamut(IHkcCommunication SCommunication)
        {
            m_HkcCommunication = SCommunication;
        }

        public bool MeasureRGBW( IDisplayColorAnalyzer DCA, int nTimeout, int nProductPatternDelay,int CIE)
        {
            bool bSuccess = true;
            if (m_HkcCommunication.GammaSetPattern(255, 0, 0, nTimeout))
            {
                // 开始测量
                Thread.Sleep(nProductPatternDelay);
                ProbeStruct Probe = DCA.MeasureEx();
                rR.x = Probe.sx;
                rR.y = Probe.sy;
            }
            else
            {
                bSuccess = false;
                return false;
            }
            if (m_HkcCommunication.GammaSetPattern(0, 255, 0, nTimeout))
            {
                // 开始测量
                Thread.Sleep(nProductPatternDelay);
                ProbeStruct Probe = DCA.MeasureEx();
                rG.x = Probe.sx;
                rG.y = Probe.sy;
            }
            else
            {
                bSuccess = false;
                return false;
            }
            if (m_HkcCommunication.GammaSetPattern(0, 0, 255, nTimeout))
            {
                // 开始测量
                Thread.Sleep(nProductPatternDelay);
                ProbeStruct Probe = DCA.MeasureEx();
                rB.x = Probe.sx;
                rB.y = Probe.sy;
            }
            else
            {
                bSuccess = false;
                return false;
            }
            if (m_HkcCommunication.GammaSetPattern(255, 255, 255, nTimeout))
            {
                // 开始测量
                Thread.Sleep(nProductPatternDelay);
                ProbeStruct Probe = DCA.MeasureEx();
                rW.x = Probe.sx;
                rW.y = Probe.sy;
            }
            else
            {
                bSuccess = false;
                return false;
            }
            SpaceType mpaceType; mpaceType = SpaceType.sRGB;
            CalChromaCoverageRatio(rR, rG, rB, mpaceType, CIE);

             mpaceType = SpaceType.DCI_P3;
            CalChromaCoverageRatio(rR, rG, rB, mpaceType, CIE);

            mpaceType = SpaceType.AdobeRGB;
            CalChromaCoverageRatio(rR, rG, rB, mpaceType, CIE);

            return bSuccess;
        }


        private static double[] Cal3MatrixInv(double[] num)
        {
            double[] result = new double[9];
            double a11, a12, a13, a21, a22, a23, a31, a32, a33;
            a11 = num[0];
            a12 = num[1];
            a13 = num[2];
            a21 = num[3];
            a22 = num[4];
            a23 = num[5];
            a31 = num[6];
            a32 = num[7];
            a33 = num[8];
            double fabsA = a11 * a22 * a33 + a12 * a23 * a31 + a13 * a21 * a32
                    - a13 * a22 * a31 - a12 * a21 * a33 - a11 * a23 * a32;
            //代数余子式
            double m11, m12, m13, m21, m22, m23, m31, m32, m33;
            m11 = Math.Pow(-1, 2) * (a22 * a33 - a23 * a32);
            m12 = Math.Pow(-1, 3) * (a21 * a33 - a23 * a31);
            m13 = Math.Pow(-1, 4) * (a21 * a32 - a22 * a31);

            m21 = Math.Pow(-1, 3) * (a12 * a33 - a13 * a32);
            m22 = Math.Pow(-1, 4) * (a11 * a33 - a13 * a31);
            m23 = Math.Pow(-1, 5) * (a11 * a32 - a12 * a31);

            m31 = Math.Pow(-1, 4) * (a12 * a23 - a13 * a22);
            m32 = Math.Pow(-1, 5) * (a11 * a23 - a13 * a21);
            m33 = Math.Pow(-1, 6) * (a11 * a22 - a12 * a21);

            if (Math.Abs(fabsA) < 0.0)
            {
                throw new System.Exception(string.Format("fabsA = {0}", fabsA));
            }
            double derA = 1.0 / fabsA;
            result[0] = derA * m11;
            result[1] = derA * m21;
            result[2] = derA * m31;
            result[3] = derA * m12;
            result[4] = derA * m22;
            result[5] = derA * m32;
            result[6] = derA * m13;
            result[7] = derA * m23;
            result[8] = derA * m33;

            return result;
        }



        public static double[] xyToXYZ(POINT SR, POINT SG, POINT SB, POINT SW)
        {
            double Rx, Ry, Gx, Gy, Bx, By, Wx, Wy;
            Rx = SR.x ; Ry = SR.y;
            Gx = SG.x; Gy = SG.y;
            Bx = SB.x; By = SB.y;
            Wx = SW.x; Wy = SW.y;
            CIEXYZ[] xyz = new CIEXYZ[3];
            double[] a = new double[9];

            xyz[2].Y = By * ((Wy - Ry) * (Gy * (Rx - Wx) + Ry * (Wx - Gx)) - Wy * (Ry - Gy) * (Wx - Rx)) / (Wy * (Ry - Gy) * (By * (Rx - Wx) + Ry * (Wx - Bx)) - Wy * (Ry - By) * (Gy * (Rx - Wx) + Ry * (Wx - Gx)));
            xyz[1].Y = -(Gy * (Wx - Rx) / (Gy * (Rx - Wx) + Ry * (Wx - Gx)) + Gy * (By * (Rx - Wx) + Ry * (Wx - Bx)) * xyz[2].Y / (By * (Gy * (Rx - Wx) + Ry * (Wx - Gx))));
            xyz[0].Y = 1 - xyz[1].Y - xyz[2].Y;
            xyz[0].Z = (1 - Rx - Ry) / Ry * xyz[0].Y;
            xyz[1].Z = (1 - Gx - Gy) / Gy * xyz[1].Y;
            xyz[2].Z = (1 - Bx - By) / By * xyz[2].Y;
            xyz[0].X = Rx / Ry * xyz[0].Y;
            xyz[1].X = Gx / Gy * xyz[1].Y;
            xyz[2].X = Bx / By * xyz[2].Y;

            a[0] = xyz[0].X;
            a[1] = xyz[1].X;
            a[2] = xyz[2].X;
            a[3] = xyz[0].Y;
            a[4] = xyz[1].Y;
            a[5] = xyz[2].Y;
            a[6] = xyz[0].Z;
            a[7] = xyz[1].Z;
            a[8] = xyz[2].Z;
            return a;

        }
        public int [] CalLargrangeTargetRGB( SpaceType mpaceType)
        {

            double[] Sxyz = new double[9];
            double[] Txyz = new double[9];
            POINT red, green, blue;
            double sx, sy;
            switch (mpaceType)
            {
                case SpaceType.sRGB:
                    red.x = 0.64;
                    red.y = 0.33;
                    green.x = 0.30;
                    green.y = 0.60;
                    blue.x = 0.15;
                    blue.y = 0.06;
                    break;
                case SpaceType.DCI_P3:
                case SpaceType.Display_P3:
                    red.x = 0.68;
                    red.y = 0.32;
                    green.x = 0.265;
                    green.y = 0.69;
                    blue.x = 0.15;
                    blue.y = 0.06;
                    break;
                case SpaceType.AdobeRGB:
                    red.x = 0.64;
                    red.y = 0.33;
                    green.x = 0.21;
                    green.y = 0.71;
                    blue.x = 0.15;
                    blue.y = 0.06;
                    break;
                case SpaceType.BT_2020:
                    red.x = 0.708;
                    red.y = 0.292;
                    green.x = 0.170;
                    green.y = 0.797;
                    blue.x = 0.131;
                    blue.y = 0.046;
                    break;
                default:
                    //默认为sRGB
                    red.x = 0.64;
                    red.y = 0.33;
                    green.x = 0.30;
                    green.y = 0.60;
                    blue.x = 0.15;
                    blue.y = 0.06;
                    break;
            }
            Sxyz = xyToXYZ(rR, rG, rB, rW);
            Txyz = xyToXYZ(red, green, blue, rW);
            double[] inv_Txyz = Cal3MatrixInv(Txyz);



            double[] matrix = new double[9];


            matrix[0] = Sxyz[0] * inv_Txyz[0] + Sxyz[3] * inv_Txyz[1] + Sxyz[6] * inv_Txyz[2];
            matrix[1] = Sxyz[1] * inv_Txyz[0] + Sxyz[4] * inv_Txyz[1] + Sxyz[7] * inv_Txyz[2];
            matrix[2] = Sxyz[2] * inv_Txyz[0] + Sxyz[5] * inv_Txyz[1] + Sxyz[8] * inv_Txyz[2];
            matrix[3] = Sxyz[0] * inv_Txyz[3] + Sxyz[3] * inv_Txyz[4] + Sxyz[6] * inv_Txyz[5];
            matrix[4] = Sxyz[1] * inv_Txyz[3] + Sxyz[4] * inv_Txyz[4] + Sxyz[7] * inv_Txyz[5];
            matrix[5] = Sxyz[2] * inv_Txyz[3] + Sxyz[5] * inv_Txyz[4] + Sxyz[8] * inv_Txyz[5];
            matrix[6] = Sxyz[0] * inv_Txyz[6] + Sxyz[3] * inv_Txyz[7] + Sxyz[6] * inv_Txyz[8];
            matrix[7] = Sxyz[1] * inv_Txyz[6] + Sxyz[4] * inv_Txyz[7] + Sxyz[7] * inv_Txyz[8];
            matrix[8] = Sxyz[2] * inv_Txyz[6] + Sxyz[5] * inv_Txyz[7] + Sxyz[8] * inv_Txyz[8];



            double[] invMatrix = Cal3MatrixInv(matrix);

            double[] inrgb = new double[9];
            inrgb[0] = 1023;
            inrgb[1] = 0;
            inrgb[2] = 0;
            inrgb[3] = 0;
            inrgb[4] = 1023;
            inrgb[5] = 0;
            inrgb[6] = 0;
            inrgb[7] = 0;
            inrgb[8] = 1023;


            double[] outRGB = new double[9];


            outRGB[0] = inrgb[0] * invMatrix[0] + inrgb[3] * invMatrix[1] + inrgb[6] * invMatrix[2];
            outRGB[1] = inrgb[1] * invMatrix[0] + inrgb[4] * invMatrix[1] + inrgb[7] * invMatrix[2];
            outRGB[2] = inrgb[2] * invMatrix[0] + inrgb[5] * invMatrix[1] + inrgb[8] * invMatrix[2];
            outRGB[3] = inrgb[0] * invMatrix[3] + inrgb[3] * invMatrix[4] + inrgb[6] * invMatrix[5];
            outRGB[4] = inrgb[1] * invMatrix[3] + inrgb[4] * invMatrix[4] + inrgb[7] * invMatrix[5];
            outRGB[5] = inrgb[2] * invMatrix[3] + inrgb[5] * invMatrix[4] + inrgb[8] * invMatrix[5];
            outRGB[6] = inrgb[0] * invMatrix[6] + inrgb[3] * invMatrix[7] + inrgb[6] * invMatrix[8];
            outRGB[7] = inrgb[1] * invMatrix[6] + inrgb[4] * invMatrix[7] + inrgb[7] * invMatrix[8];
            outRGB[8] = inrgb[2] * invMatrix[6] + inrgb[5] * invMatrix[7] + inrgb[8] * invMatrix[8];

            int[] oRGB = new int[9];
            for (int i = 0; i < 9; i++)
            {
                if (outRGB[i] < 0) oRGB[i] = 0;
                else oRGB[i] = (int)outRGB[i];
                if (outRGB[i] > 1023) oRGB[i] = 1023;
            }
            LogHelper.WriteToLog(mpaceType.ToString() + "色域 R= " + oRGB[0].ToString() + " , " + oRGB[3].ToString() + " , " + oRGB[6].ToString() , LogLevel.INFO);
            LogHelper.WriteToLog(mpaceType.ToString() + "色域 G= " + oRGB[1].ToString() + " , " + oRGB[4].ToString() + " , " + oRGB[7].ToString(), LogLevel.INFO);
            LogHelper.WriteToLog(mpaceType.ToString() + "色域 B= " + oRGB[2].ToString() + " , " + oRGB[5].ToString() + " , " + oRGB[8].ToString(), LogLevel.INFO);
            return oRGB;
        }
        public double[] CaPoitTargetRGB(double Tolerance, SpaceType mpaceType, int[] RGBgain,IDisplayColorAnalyzer DCA, int nTimeout, int nProductPatternDelay,int CIE)
        {
            POINT red, green, blue;
            POINT Tr,Tg, Tb;
            double[] Ratio = new double[2];
            string setmode = "sRGB", savemode = "saveSRGB";;
            switch (mpaceType)
            {
                case SpaceType.sRGB:
                    red.x = 0.64;
                    red.y = 0.33;
                    green.x = 0.30;
                    green.y = 0.60;
                    blue.x = 0.15;
                    blue.y = 0.06;
                    setmode = "sRGB";
                    savemode = "saveSRGB";
                    break;
                case SpaceType.DCI_P3:
                case SpaceType.Display_P3:
                    red.x = 0.68;
                    red.y = 0.32;
                    green.x = 0.265;
                    green.y = 0.69;
                    blue.x = 0.15;
                    blue.y = 0.06;
                    setmode = "P3";
                    savemode = "saveP3";
                    break;
                case SpaceType.AdobeRGB:
                    red.x = 0.64;
                    red.y = 0.33;
                    green.x = 0.21;
                    green.y = 0.71;
                    blue.x = 0.15;
                    blue.y = 0.06;
                    setmode = "Adobe";
                    savemode = "saveAdobe";
                    break;
                case SpaceType.BT_2020:
                    red.x = 0.708;
                    red.y = 0.292;
                    green.x = 0.170;
                    green.y = 0.797;
                    blue.x = 0.131;
                    blue.y = 0.046;
                    break;
                default:
                    //默认为sRGB
                    red.x = 0.64;
                    red.y = 0.33;
                    green.x = 0.30;
                    green.y = 0.60;
                    blue.x = 0.15;
                    blue.y = 0.06;
                    setmode = "sRGB";
                    savemode = "saveSRGB";
                    break;
            }
            Tr = red; Tg = green; Tb = blue;
            double Tkrg = (red.y - green.y) / (red.x - green.x);
            double Tkgb = (green.y - blue.y) / (green.x - blue.x);
            double Tkbr = (blue.y - red.y) / (blue.x - red.x);
            double TBrg = red.y - Tkrg * red.x;
            double TBgb = green.y - Tkgb * green.x;
            double TBbr = blue.y - Tkbr * blue.x;

            double krg = (rR.y - rG.y) / (rR.x - rG.x);
            double kgb = (rG.y - rB.y) / (rG.x - rB.x);
            double kbr = (rB.y - rR.y) / (rB.x - rR.x);
            double Brg = rR.y - krg * rR.x;
            double Bgb = rG.y - kgb * rG.x;
            double Bbr = rB.y - kbr * rB.x;
            //--------------RR-----------------------------------
            if ((red.x * krg + Brg) >= red.y && (red.x * kbr + Bbr) <= red.y)
            {
                Tr = red;
            }
            else if(((red.x * krg + Brg) <= red.y)&& ((red.x * kbr + Bbr) >= red.y)) Tr = rR;
            else if ((red.x * krg + Brg) <= red.y)
            {
 
                Tr.x = (TBbr - Brg) / (krg - Tkbr);
                Tr.y = Tr.x * Tkbr + TBbr;

            }
            else if ((red.x * kbr + Bbr) >= red.y)
            {
                Tr.x = (TBrg - Bbr) / (kbr - Tkrg);
                Tr.y = Tr.x * Tkrg + TBrg;
            }
            
            //--------------------GGG------------------------
            if ((green.x * krg + Brg) >= green.y && (green.x * kgb + Bgb) >= green.y)
            {
                Tg = green;
            }
            else if(((green.x * krg + Brg) <= green.y)&& ((green.x * kgb + Bgb) <= green.y)) Tg = rG;
            else if ((green.x * krg + Brg) <= green.y)
            {

                Tg.x = (TBgb - Brg) / (krg - Tkgb);
                Tg.y = Tg.x * Tkgb + TBgb;

            }
            else if ((green.x * kgb + Bgb) <= green.y)
            {
                Tg.x = (TBrg - Bgb) / (kgb - Tkrg);
                Tg.y = Tg.x * Tkrg + TBrg;
            }
      
            //--------------------BB---------------------------
            if ((blue.x * kgb + Bgb) >= blue.y && (blue.x * kbr + Bbr) <= blue.y)
            {
                Tb = blue;
            }
            else if(((blue.x * kgb + Bgb) <= blue.y)&& ((blue.x * kbr + Bbr) >= blue.y)) Tb = rB;          
            else if ((blue.x * kgb + Bgb) <= blue.y)
            {

                Tb.x = (TBbr - Bgb) / (kgb - Tkbr);
                Tb.y = Tb.x * Tkbr + TBbr;

            }
            else if ((blue.x * kbr + Bbr) >= blue.y)
            {
                Tb.x = (TBgb - Bbr) / (kbr - Tkgb);
                Tb.y = Tb.x * Tkgb + TBgb;
            }
            bool cal = true;int calR = 0, calB = 0, calG = 0;
            int[] mrgb = RGBgain;
            int[] Lmrgb =new int [9];
            int[] LLmrgb = new int[9];
            LogHelper.WriteToLog("TR="+Tr.x+ " ," + Tr.y +"  TG=" + Tg.x + " ," + Tg.y + "  TB=" + Tb.x + " ," + Tb.y, LogLevel.INFO);
            int nub = 0;
            while (cal)
            {
                bool ook = m_HkcCommunication.SetColorGamut(mrgb, savemode, nTimeout);
                Thread.Sleep(200);
                m_HkcCommunication.SetMode(setmode, nTimeout);


                calR = CalcR(Tolerance,Tr, ref mrgb, DCA,  nTimeout,  nProductPatternDelay);
                calG = CalcG(Tolerance,Tg,ref mrgb, DCA, nTimeout, nProductPatternDelay);
                calB = CalcB(Tolerance,Tb, ref mrgb, DCA, nTimeout, nProductPatternDelay);

                LogHelper.WriteToLog(mpaceType.ToString() + " R= " + mrgb[0].ToString() + " , " + mrgb[3].ToString() + " , " + mrgb[6].ToString(), LogLevel.INFO);
               LogHelper.WriteToLog(mpaceType.ToString() + " G= " + mrgb[1].ToString() + " , " + mrgb[4].ToString() + " , " + mrgb[7].ToString(), LogLevel.INFO);
                LogHelper.WriteToLog(mpaceType.ToString() + " B= " + mrgb[2].ToString() + " , " + mrgb[5].ToString() + " , " + mrgb[8].ToString(), LogLevel.INFO);
                if (calR==1 && calG==1 && calB==1) cal = false;
                else if (calR == -1 || calG == -1 || calB == -1)
                    break;
                nub++;
                if (nub > 25)
                {
                    LogHelper.WriteToLog("校正超过限定次数异常失败 ", LogLevel.INFO);
                    break;
                }
                if ((Lmrgb == mrgb || LLmrgb == mrgb)&& nub>2) 
                    break;
                if (nub % 2 == 1) Lmrgb = (int[])mrgb.Clone();
                else LLmrgb = (int[])mrgb.Clone();
            }
            if (cal) 
                LogHelper.WriteToLog("Gamut校正到误差范围失败 ：-1", LogLevel.INFO);

            Ratio[0] = CalChromaAreaRatio(oR, oG, oB, mpaceType, CIE);
            Ratio[1] = CalChromaCoverageRatio(oR, oG, oB, mpaceType, CIE);
            return Ratio;
        }
        public int CalcR(double Tolerance,POINT Tr, ref int[] mrgb, IDisplayColorAnalyzer DCA, int nTimeout, int nProductPatternDelay)
        {
            bool OK = false;
            int G = (mrgb[3]+1)/4-1,B=  (mrgb[6]+1)/4-1;
            float x=0, y=0;
            //if (m_HkcCommunication.GammaSetPattern((mrgb[0]+1)/4-1, G, B, nTimeout))
            if (m_HkcCommunication.GammaSetPattern(255, 0, 0, nTimeout))
            {
                // 开始测量
                Thread.Sleep(nProductPatternDelay);
                ProbeStruct Probe = DCA.MeasureEx();
                x = Probe.sx;
                y = Probe.sy;
                oR.x = x;
                oR.y = y;
                int Rsetp = 2, Gstep = 2, Bstep = 2;
                if (Math.Abs(x - Tr.x) > 0.03) Bstep = 30; 
                else if (Math.Abs(x - Tr.x) > 0.02) Bstep = 20;
                else if (Math.Abs(x - Tr.x) > 0.01) Bstep = 5;
                if (Math.Abs(y - Tr.y) > 0.03) Gstep = 30;
                else if (Math.Abs(y - Tr.y) > 0.02) Gstep = 20;
                else if (Math.Abs(y - Tr.y) > 0.01) Gstep = 5;
                if ((Math.Abs(y - Tr.y) > 0.03)&& (Math.Abs(x - Tr.x) > 0.03)) Bstep = 30;
                else if ((Math.Abs(y - Tr.y) > 0.02) && (Math.Abs(x - Tr.x) > 0.02)) Bstep = 20;
                else if ((Math.Abs(y - Tr.y) > 0.01) && (Math.Abs(x - Tr.x) > 0.01)) Bstep = 5;

                if (Math.Abs(x - Tr.x) < Tolerance && Math.Abs(y - Tr.y) < Tolerance) return 1;
                else if (x > Tr.x && y > Tr.y)
                {          

                    if (B < 128) B = B + Bstep;
                    else return -1;
                }
                else if (x < Tr.x && y < Tr.y)
                {
                    if (B > Bstep) B = B - Bstep;
                    else return -1;
                }
                else
                {
                    if (y > Tr.y)
                    {
                        if (G > Gstep) G = G - Gstep;
                        else return -1;
                    }
                    else if (y < Tr.y)
                    {
                        if (G < 128) G = G + Gstep;
                        else return -1;
                    }
                } 
            }
            else return -1;
            mrgb[3] = (G + 1) * 4 - 1; mrgb[6] = (B + 1) * 4 - 1;
            if (mrgb[3] < 0) mrgb[3] = 0;
            if (mrgb[6] < 0) mrgb[6] = 0;

            return 0;
        }
        public int CalcG(double Tolerance, POINT Tg, ref int[] mrgb, IDisplayColorAnalyzer DCA, int nTimeout, int nProductPatternDelay)
        {
            bool OK = false;
            int R = (mrgb[1] + 1) / 4 - 1, B = (mrgb[7] + 1) / 4 - 1;
            float x = 0, y = 0;
            // if (m_HkcCommunication.GammaSetPattern(R, (mrgb[4] + 1) / 4 - 1, B, nTimeout))
            if (m_HkcCommunication.GammaSetPattern(0, 255, 0, nTimeout))
            {
                // 开始测量
                Thread.Sleep(nProductPatternDelay);
                ProbeStruct Probe = DCA.MeasureEx();
                x = Probe.sx;
                y = Probe.sy;
                oG.x = x;
                oG.y = y;
                int Rsetp = 2, Gstep = 2, Bstep = 2;
                if (Math.Abs(x - Tg.x) > 0.03) Rsetp = 30;
                else if (Math.Abs(x - Tg.x) > 0.02) Rsetp = 20;
                else if (Math.Abs(x - Tg.x) > 0.01) Rsetp = 5;
                if (Math.Abs(y - Tg.y) > 0.03) Bstep = 30;
                else if (Math.Abs(y - Tg.y) > 0.02) Bstep = 20;
                else if (Math.Abs(y - Tg.y) > 0.01) Bstep = 5;
                if ((Math.Abs(y - Tg.y) > 0.03) && (Math.Abs(x - Tg.x) > 0.03)) Bstep = 30;
                else if ((Math.Abs(y - Tg.y) > 0.02) && (Math.Abs(x - Tg.x) > 0.02)) Bstep = 20;
                else if ((Math.Abs(y - Tg.y) > 0.01) && (Math.Abs(x - Tg.x) > 0.01)) Bstep = 5;

                if (Math.Abs(x - Tg.x) < Tolerance && Math.Abs(y - Tg.y) < Tolerance) return 1;
                else if (x > Tg.x && y > Tg.y)
                {

                    if (B < 128) B = B + Bstep;
                    else return -1;
                }
                else if (x < Tg.x && y < Tg.y)
                {
                    if (B > Bstep) B = B - Bstep;
                    else return -1;
                }
                else
                {
                    if (x > Tg.x)
                    {
                        if (R > Rsetp) R = R - Rsetp;
                        else return -1;
                    }
                    else if (x < Tg.x)
                    {
                        if (R < 128) R = R + Rsetp;
                        else return -1;
                    }
                }
            }
            else return -1;
            mrgb[1] = (R + 1) * 4 - 1; mrgb[7] = (B + 1) * 4 - 1;
            if (mrgb[1] < 0) mrgb[1] = 0;
            if (mrgb[7] < 0) mrgb[7] = 0;

            return 0;
        }
        public int CalcB(double Tolerance, POINT Tb, ref int[] mrgb, IDisplayColorAnalyzer DCA, int nTimeout, int nProductPatternDelay)
        {
            bool OK = false;
            int R = (mrgb[2] + 1) / 4 - 1, G = (mrgb[5] + 1) / 4 - 1;
            float x = 0, y = 0;
            //if (m_HkcCommunication.GammaSetPattern(R, G, (mrgb[8] + 1) / 4 - 1, nTimeout))
            if (m_HkcCommunication.GammaSetPattern(0, 0, 255, nTimeout))
            {
                // 开始测量
                Thread.Sleep(nProductPatternDelay);
                ProbeStruct Probe = DCA.MeasureEx();
                x = Probe.sx;
                y = Probe.sy;
                oB.x = x;
                oB.y = y;
                int Rsetp = 2, Gstep = 2, Bstep = 2;
                if (Math.Abs(x - Tb.x) > 0.04) Rsetp = 30;
                else if (Math.Abs(x - Tb.x) > 0.02) Rsetp = 20;
                else if (Math.Abs(y - Tb.y) > 0.01) Gstep = 5;
                if (Math.Abs(x - Tb.x) > 0.04) Rsetp = 30;
                else if (Math.Abs(x - Tb.x) > 0.02) Rsetp = 20;
                else if (Math.Abs(y - Tb.y) > 0.01) Gstep = 5;


                if (Math.Abs(x - Tb.x) < Tolerance && Math.Abs(y - Tb.y) < Tolerance) return 1;
                else 
                {
                    if (Math.Abs(y - Tb.y) > Tolerance)
                    {
                        if (y > Tb.y)
                        {
                            if (G > Gstep) G = G - Gstep;
                            else return -1;
                        }
                        else
                        {
                            if (G < 128) G = G + Gstep;
                            else return -1;
                        }
                    }
                    if (Math.Abs(x - Tb.x) > Tolerance)
                    {
                        if (x > Tb.x)
                        {
                            if (R > Rsetp) R = R - Rsetp;
                            else return -1;
                        }
                        else
                        {
                            if (R < 128) R = R + Rsetp;
                            else return -1;
                        }
                    }
                }
            }
            else return -1;
            mrgb[2] = (R + 1) * 4 - 1; mrgb[5] = (G + 1) * 4 - 1;
            if (mrgb[2] < 0) mrgb[2] = 0;
            if (mrgb[5] < 0) mrgb[5] = 0;
            return 0;
        }


        public double[] Ratio(string mode, SpaceType mpaceType,IDisplayColorAnalyzer DCA, int nTimeout, int nProductPatternDelay,int CIE)
        {
            POINT Rr, Rg, Rb;
            double [] Ratio = new double [2];
            m_HkcCommunication.SetMode(mode, nTimeout);
            Thread.Sleep(100);
            m_HkcCommunication.GammaSetPattern(255, 0, 0, nTimeout);
            
                Thread.Sleep(100);
                ProbeStruct Probe = DCA.MeasureEx();
                Rr.x = Probe.sx;
                Rr.y = Probe.sy;

            m_HkcCommunication.GammaSetPattern(0, 255, 0, nTimeout);
            
                Thread.Sleep(100);
                 Probe = DCA.MeasureEx();
                Rg.x = Probe.sx;
                Rg.y = Probe.sy;

            m_HkcCommunication.GammaSetPattern(0, 0, 255, nTimeout);
            
                Thread.Sleep(100);
                 Probe = DCA.MeasureEx();
                Rb.x = Probe.sx;
                Rb.y = Probe.sy;

            Ratio[0] = CalChromaAreaRatio(Rr, Rb, Rg, mpaceType, CIE);
            Ratio[1] = CalChromaCoverageRatio(Rr, Rb, Rg, mpaceType, CIE);
            return Ratio;

        }

        public double CalChromaAreaRatio(POINT red_real, POINT green_real, POINT blue_real, SpaceType mpaceTypel,int CIE)
        {
            POINT red, green, blue;
            if (CIE==1976)
            {
                var rx = (4 * red_real.x) / (-2 * red_real.x + 12 * red_real.y + 3);
                var ry = (9 * red_real.y) / (-2 * red_real.x + 12 * red_real.y + 3);
                var gx = (4 * green_real.x) / (-2 * green_real.x + 12 * green_real.y + 3);
                var gy = (9 * green_real.y) / (-2 * green_real.x + 12 * green_real.y + 3);
                var bx = (4 * blue_real.x) / (-2 * blue_real.x + 12 * blue_real.y + 3);
                var by = (9 * blue_real.y) / (-2 * blue_real.x + 12 * blue_real.y + 3);
                red_real.x = rx;
                red_real.y = ry;
                green_real.x = gx;
                green_real.y = gy;
                blue_real.x = bx;
                blue_real.y = by;

                switch (mpaceTypel)
                {
                    case SpaceType.sRGB:
                        red.x = 0.451;
                        red.y = 0.523;
                        green.x = 0.125;
                        green.y = 0.563;
                        blue.x = 0.175;
                        blue.y = 0.158;
                        break;
                    case SpaceType.DCI_P3:
                    case SpaceType.Display_P3:
                        red.x = 0.496;
                        red.y = 0.526;
                        green.x = 0.099;
                        green.y = 0.578;
                        blue.x = 0.175;
                        blue.y = 0.158;
                        break;
                    case SpaceType.AdobeRGB:
                        red.x = 0.451;
                        red.y = 0.523;
                        green.x = 0.076;
                        green.y = 0.576;
                        blue.x = 0.175;
                        blue.y = 0.158;
                        break;
                    case SpaceType.BT_2020:
                        red.x = 0.708;
                        red.y = 0.292;
                        green.x = 0.170;
                        green.y = 0.797;
                        blue.x = 0.131;
                        blue.y = 0.046;
                        break;
                    default:
                        //默认为sRGB
                        red.x = 0.451;
                        red.y = 0.523;
                        green.x = 0.125;
                        green.y = 0.563;
                        blue.x = 0.175;
                        blue.y = 0.158;
                        break;
                }
            }
            else
            {
                switch (mpaceTypel)
                {
                    case SpaceType.sRGB:
                        red.x = 0.64;
                        red.y = 0.33;
                        green.x = 0.30;
                        green.y = 0.60;
                        blue.x = 0.15;
                        blue.y = 0.06;
                        break;
                    case SpaceType.DCI_P3:
                    case SpaceType.Display_P3:
                        red.x = 0.68;
                        red.y = 0.32;
                        green.x = 0.265;
                        green.y = 0.69;
                        blue.x = 0.15;
                        blue.y = 0.06;
                        break;
                    case SpaceType.AdobeRGB:
                        red.x = 0.64;
                        red.y = 0.33;
                        green.x = 0.21;
                        green.y = 0.71;
                        blue.x = 0.15;
                        blue.y = 0.06;
                        break;
                    case SpaceType.BT_2020:
                        red.x = 0.708;
                        red.y = 0.292;
                        green.x = 0.170;
                        green.y = 0.797;
                        blue.x = 0.131;
                        blue.y = 0.046;
                        break;
                    default:
                        //默认为sRGB
                        red.x = 0.64;
                        red.y = 0.33;
                        green.x = 0.30;
                        green.y = 0.60;
                        blue.x = 0.15;
                        blue.y = 0.06;
                        break;
                }
            }
            double  a = Math.Sqrt(Math.Pow(red.x - green.x, 2) + Math.Pow(red.y - green.y, 2));
            double  b = Math.Sqrt(Math.Pow(blue.x - green.x, 2) + Math.Pow(blue.y - green.y, 2));
            double  c = Math.Sqrt(Math.Pow(red.x - blue.x, 2) + Math.Pow(red.y - blue.y, 2));
            double p = (a + b + c) / 2;
            double s = Math.Sqrt(p * (p - a) * (p - b) * (p - c));

            double a1 = Math.Sqrt(Math.Pow(red_real.x - green_real.x, 2) + Math.Pow(red_real.y - green_real.y, 2));
            double b1 = Math.Sqrt(Math.Pow(blue_real.x - green_real.x, 2) + Math.Pow(blue_real.y - green_real.y, 2));
            double c1 = Math.Sqrt(Math.Pow(red_real.x - blue_real.x, 2) + Math.Pow(red_real.y - blue_real.y, 2));
            double p1 = (a1 + b1 + c1) / 2;
            double s1 = Math.Sqrt(p1 * (p1 - a1) * (p1 - b1) * (p1 - c1));
            double re = s1 / s*100;
            LogHelper.WriteToLog("CIE" + CIE.ToString() + "  " + mpaceTypel.ToString() + "色域面积比 ： " + re.ToString("0.00") + " ,  " + s1.ToString("0.00") + " / " + s.ToString("0.00"), LogLevel.INFO);
            return re;


        }
        public double CalChromaCoverageRatio(POINT red_real, POINT green_real, POINT blue_real, SpaceType mpaceTypel,int CIE)
        {

            POINT red, green, blue;
            if (CIE == 1976)
            {
                var rx = (4 * red_real.x) / (-2 * red_real.x + 12 * red_real.y + 3);
                var ry = (9 * red_real.y) / (-2 * red_real.x + 12 * red_real.y + 3);
                var gx = (4 * green_real.x) / (-2 * green_real.x + 12 * green_real.y + 3);
                var gy = (9 * green_real.y) / (-2 * green_real.x + 12 * green_real.y + 3);
                var bx = (4 * blue_real.x) / (-2 * blue_real.x + 12 * blue_real.y + 3);
                var by = (9 * blue_real.y) / (-2 * blue_real.x + 12 * blue_real.y + 3);
                red_real.x = rx;
                red_real.y = ry;
                green_real.x = gx;
                green_real.y = gy;
                blue_real.x = bx;
                blue_real.y = by;

                switch (mpaceTypel)
                {
                    case SpaceType.sRGB:
                        red.x = 0.451;
                        red.y = 0.523;
                        green.x = 0.125;
                        green.y = 0.563;
                        blue.x = 0.175;
                        blue.y = 0.158;
                        break;
                    case SpaceType.DCI_P3:
                    case SpaceType.Display_P3:
                        red.x = 0.496;
                        red.y = 0.526;
                        green.x = 0.099;
                        green.y = 0.578;
                        blue.x = 0.175;
                        blue.y = 0.158;
                        break;
                    case SpaceType.AdobeRGB:
                        red.x = 0.451;
                        red.y = 0.523;
                        green.x = 0.076;
                        green.y = 0.576;
                        blue.x = 0.175;
                        blue.y = 0.158;
                        break;
                    case SpaceType.BT_2020:
                        red.x = 0.708;
                        red.y = 0.292;
                        green.x = 0.170;
                        green.y = 0.797;
                        blue.x = 0.131;
                        blue.y = 0.046;
                        break;
                    default:
                        //默认为sRGB
                        red.x = 0.451;
                        red.y = 0.523;
                        green.x = 0.125;
                        green.y = 0.563;
                        blue.x = 0.175;
                        blue.y = 0.158;
                        break;
                }
            }
            else
            {
                switch (mpaceTypel)
                {
                    case SpaceType.sRGB:
                        red.x = 0.64;
                        red.y = 0.33;
                        green.x = 0.30;
                        green.y = 0.60;
                        blue.x = 0.15;
                        blue.y = 0.06;
                        break;
                    case SpaceType.DCI_P3:
                    case SpaceType.Display_P3:
                        red.x = 0.68;
                        red.y = 0.32;
                        green.x = 0.265;
                        green.y = 0.69;
                        blue.x = 0.15;
                        blue.y = 0.06;
                        break;
                    case SpaceType.AdobeRGB:
                        red.x = 0.64;
                        red.y = 0.33;
                        green.x = 0.21;
                        green.y = 0.71;
                        blue.x = 0.15;
                        blue.y = 0.06;
                        break;
                    case SpaceType.BT_2020:
                        red.x = 0.708;
                        red.y = 0.292;
                        green.x = 0.170;
                        green.y = 0.797;
                        blue.x = 0.131;
                        blue.y = 0.046;
                        break;
                    default:
                        //默认为sRGB
                        red.x = 0.64;
                        red.y = 0.33;
                        green.x = 0.30;
                        green.y = 0.60;
                        blue.x = 0.15;
                        blue.y = 0.06;
                        break;
                }
            }

            if (green.x == red.x || green.x == blue.x || blue.x == red.x)
            {
                throw new Exception();
            }
            //计算斜率
            double k1, k2, k3;
            k1 = (green.y - red.y) / (green.x - red.x);
            k2 = (green.y - blue.y) / (green.x - blue.x);
            k3 = (blue.y - red.y) / (blue.x - red.x);

            //计算直线方程方向角
            double k1x, k1y, k2x, k2y, k3x, k3y;
            double l1, l2, l3;
            l1 = Math.Sqrt(Math.Pow(green.y - red.y, 2) + Math.Pow(green.x - red.x, 2));
            l2 = Math.Sqrt(Math.Pow(blue.y - green.y, 2) + Math.Pow(blue.x - green.x, 2));
            l3 = Math.Sqrt(Math.Pow(red.y - blue.y, 2) + Math.Pow(red.x - blue.x, 2));
            k1x = (green.x - red.x) / l1;
            k1y = (green.y - red.y) / l1;
            k2x = (blue.x - green.x) / l2;
            k2y = (blue.y - green.y) / l2;
            k3x = (red.x - blue.x) / l3;
            k3y = (red.y - blue.y) / l3;


            double s_x, s_y, e_x, e_y;
            double inSidePoint = 0.0;
            double allPoint = 0.0;
            double step = 0.001;
            double t1, t2, t3;
            double p_x, p_y;

            //选定l1边，并归一化，以step步长分割线段
            for (double i = step; i < 1; i = i + step)
            {
                s_x = red.x + l1 * k1x * i;
                s_y = red.y + l1 * k1y * i;

                //以(s_x,s_y)为起点，k3为斜率与l2相交于(e_x,e_y)
                e_x = (-s_y + green.y + k3 * s_x - k2 * green.x) / (k3 - k2);
                e_y = k3 * (e_x - s_x) + s_y;

                //计算直线l的方向角
                double l, kx, ky;
                l = Math.Sqrt(Math.Pow(e_y - s_y, 2) + Math.Pow(e_x - s_x, 2));
                kx = (e_x - s_x) / l;
                ky = (e_y - s_y) / l;
                //以同样的步长step开始截取线段l上的点
                for (double j = step; j < 1 * l / l1; j = j + step)
                {
                    //遍历求线段l上的点
                    p_x = s_x + l * kx * j * l1 / l;
                    p_y = s_y + l * ky * j * l1 / l;

                    //根据向量关系求理论的点是否在实际的RGB三角形内
                    t1 = (red_real.x - p_x) * (green_real.y - p_y) - (red_real.y - p_y) * (green_real.x - p_x);
                    t2 = (green_real.x - p_x) * (blue_real.y - p_y) - (green_real.y - p_y) * (blue_real.x - p_x);
                    t3 = (blue_real.x - p_x) * (red_real.y - p_y) - (blue_real.y - p_y) * (red_real.x - p_x);
                    if ((t1 >= 0 & t2 >= 0 & t3 >= 0) || (t1 <= 0 & t2 <= 0 & t3 <= 0))
                    {
                        inSidePoint = inSidePoint + step;
                    }
                    allPoint = allPoint + step;
                }
            }
            //根据覆盖的点数据求百分比
            double percent = inSidePoint / allPoint * 100.0;

            LogHelper.WriteToLog("CIE"+CIE.ToString() +"  "+ mpaceTypel.ToString() + "色域覆盖比 ： " + percent.ToString("0.00"), LogLevel.INFO);
            return percent;
        }



    }
}
