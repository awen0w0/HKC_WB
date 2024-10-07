using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using static System.Net.WebRequestMethods;

namespace WhiteBalanceCorrection
{
    public enum GammaOrderNum
    {
        ODN_8 = 0,
        ODN_16 = 1,
        ODN_32 = 2,
        ODN_64 = 3,
        ODN_128 = 4
    };

    public enum GammaScreenMode
    {
        SCN_RGB = 0,
        SCN_WRGB = 1
    };

    public enum CorrectType
    {
        CT_GAMMA = 0,
        CT_WB = 1,
        CT_LV = 2,
        CT_VCOM = 3,
        CT_COLOR = 4,
        CT_EDID = 5,
        CT_COLORGAMUT=6,
        CT_LANGUAGE= 7,
    };

    public class GammaXYLv
    {
        public float fX;
        public float fY;
        public float fLv;
        public double  fXX;
        public double fYY;
        public double fZZ;
    }

    public class GammaXYZ
    {
        public float fRX;
        public float fRY;
        public float fRZ;
        public float fGX;
        public float fGY;
        public float fGZ;
        public float fBX;
        public float fBY;
        public float fBZ;
        public float fWY;
    }

    public class GammaXYZTmp
    {
        public int nRIndex;
        public int nGIndex;
        public int nBIndex;
        public float fX;
        public float fY;
        public float fLv;
    }

    public class GammaRGB
    {
        public int nR;
        public int nG;
        public int nB;
        public float fLv;

        public GammaRGB()
        {

        }

        public GammaRGB(int r, int g, int b)
        {
            nR = r;
            nG = g;
            nB = b;
        }
    }

    public class GammaRGB2
    {
        public float fIndex;
        public float fR;
        public float fG;
        public float fB;
    }

    public class MeasuredData
    {
        public int nIndex;
        public float fX;
        public float fY;
        public float fLv;
    }

    class CalcGamma
    {

        public void ChangeXYLvValueToXYZValue2(GammaXYLv[] GammaXYLvValue, GammaXYZ[] GammaXYZValue, int nGammaXYZValueNum)
        {
            GammaXYLv[] GammaWXYLv = new GammaXYLv[nGammaXYZValueNum];
            for (int i = 0; i < nGammaXYZValueNum; i++)
            {
                GammaWXYLv[i] = new GammaXYLv();
                GammaWXYLv[i].fX = GammaXYLvValue[i].fX;
                GammaWXYLv[i].fY = GammaXYLvValue[i].fY;
                GammaWXYLv[i].fLv = GammaXYLvValue[i].fLv;
            }
            GammaXYLv[] GammaRXYLv = new GammaXYLv[nGammaXYZValueNum];
            for (int i = 0; i < nGammaXYZValueNum; i++)
            {
                GammaRXYLv[i] = new GammaXYLv();
                GammaRXYLv[i].fX = GammaXYLvValue[i + nGammaXYZValueNum].fX;
                GammaRXYLv[i].fY = GammaXYLvValue[i + nGammaXYZValueNum].fY;
                GammaRXYLv[i].fLv = GammaXYLvValue[i + nGammaXYZValueNum].fLv;
            }
            GammaXYLv[] GammaGXYLv = new GammaXYLv[nGammaXYZValueNum];
            for (int i = 0; i < nGammaXYZValueNum; i++)
            {
                GammaGXYLv[i] = new GammaXYLv();
                GammaGXYLv[i].fX = GammaXYLvValue[i + nGammaXYZValueNum * 2].fX;
                GammaGXYLv[i].fY = GammaXYLvValue[i + nGammaXYZValueNum * 2].fY;
                GammaGXYLv[i].fLv = GammaXYLvValue[i + nGammaXYZValueNum * 2].fLv;
            }
            GammaXYLv[] GammaBXYLv = new GammaXYLv[nGammaXYZValueNum];
            for (int i = 0; i < nGammaXYZValueNum; i++)
            {
                GammaBXYLv[i] = new GammaXYLv();
                GammaBXYLv[i].fX = GammaXYLvValue[i + nGammaXYZValueNum * 3].fX;
                GammaBXYLv[i].fY = GammaXYLvValue[i + nGammaXYZValueNum * 3].fY;
                GammaBXYLv[i].fLv = GammaXYLvValue[i + nGammaXYZValueNum * 3].fLv;
            }
            for (int i = 0; i < nGammaXYZValueNum; i++)
            {
                GammaXYZValue[i].fRX = GammaRXYLv[i].fX / GammaRXYLv[i].fY * GammaRXYLv[i].fLv;
                GammaXYZValue[i].fRY = GammaRXYLv[i].fLv;
                GammaXYZValue[i].fRZ = (1 - GammaRXYLv[i].fX - GammaRXYLv[i].fY) / GammaRXYLv[i].fY * GammaRXYLv[i].fLv;
                GammaXYZValue[i].fGX = GammaGXYLv[i].fX / GammaGXYLv[i].fY * GammaGXYLv[i].fLv;
                GammaXYZValue[i].fGY = GammaGXYLv[i].fLv;
                GammaXYZValue[i].fGZ = (1 - GammaGXYLv[i].fX - GammaGXYLv[i].fY) / GammaGXYLv[i].fY * GammaGXYLv[i].fLv;
                GammaXYZValue[i].fBX = GammaBXYLv[i].fX / GammaBXYLv[i].fY * GammaBXYLv[i].fLv;
                GammaXYZValue[i].fBY = GammaBXYLv[i].fLv;
                GammaXYZValue[i].fBZ = (1 - GammaBXYLv[i].fX - GammaBXYLv[i].fY) / GammaBXYLv[i].fY * GammaBXYLv[i].fLv;
                GammaXYZValue[i].fWY = GammaWXYLv[i].fLv;
            }
        }

        public void CopyXYZValue(GammaXYZ GammaXYZValue1, GammaXYZ GammaXYZValue2)
        {
            GammaXYZValue2.fRX = GammaXYZValue1.fRX;
            GammaXYZValue2.fRY = GammaXYZValue1.fRY;
            GammaXYZValue2.fRZ = GammaXYZValue1.fRZ;
            GammaXYZValue2.fGX = GammaXYZValue1.fGX;
            GammaXYZValue2.fGY = GammaXYZValue1.fGY;
            GammaXYZValue2.fGZ = GammaXYZValue1.fGZ;
            GammaXYZValue2.fBX = GammaXYZValue1.fBX;
            GammaXYZValue2.fBY = GammaXYZValue1.fBY;
            GammaXYZValue2.fBZ = GammaXYZValue1.fBZ;
            GammaXYZValue2.fWY = GammaXYZValue1.fWY;
        }

        public void ExtendXYZValue(GammaXYZ[] GammaXYZValue, int nGammaXYZValueNum, GammaXYZ[] GammaXYZValueExtend, int nGammaXYZValueExtendNum, float fGamma)
        {
            for (int i = 1; i < nGammaXYZValueNum; i++)
            {
                int Num1 = ((i - 1) * nGammaXYZValueExtendNum) / (nGammaXYZValueNum - 1);
                int Num2 = (i * nGammaXYZValueExtendNum) / (nGammaXYZValueNum - 1);
                if (Num2 == nGammaXYZValueExtendNum)
                {
                    Num2--;
                }
                CopyXYZValue(GammaXYZValue[i - 1], GammaXYZValueExtend[Num1]);
                CopyXYZValue(GammaXYZValue[i], GammaXYZValueExtend[Num2]);
                int nStep = Num2 - Num1;
                for (int j = Num1 + 1; j < Num2; j++)
                {
                    /*
                    GammaXYZValueExtend[j].fRX = (GammaXYZValueExtend[Num2].fRX - GammaXYZValueExtend[Num1].fRX) / nStep * j + (GammaXYZValueExtend[Num2].fRX - (GammaXYZValueExtend[Num2].fRX - GammaXYZValueExtend[Num1].fRX) / nStep * Num2);
                    GammaXYZValueExtend[j].fRY = (GammaXYZValueExtend[Num2].fRY - GammaXYZValueExtend[Num1].fRY) / nStep * j + (GammaXYZValueExtend[Num2].fRY - (GammaXYZValueExtend[Num2].fRY - GammaXYZValueExtend[Num1].fRY) / nStep * Num2);
                    GammaXYZValueExtend[j].fRZ = (GammaXYZValueExtend[Num2].fRZ - GammaXYZValueExtend[Num1].fRZ) / nStep * j + (GammaXYZValueExtend[Num2].fRZ - (GammaXYZValueExtend[Num2].fRZ - GammaXYZValueExtend[Num1].fRZ) / nStep * Num2);
                    GammaXYZValueExtend[j].fGX = (GammaXYZValueExtend[Num2].fGX - GammaXYZValueExtend[Num1].fGX) / nStep * j + (GammaXYZValueExtend[Num2].fGX - (GammaXYZValueExtend[Num2].fGX - GammaXYZValueExtend[Num1].fGX) / nStep * Num2);
                    GammaXYZValueExtend[j ].fGY = (GammaXYZValueExtend[Num2].fGY - GammaXYZValueExtend[Num1].fGY) / nStep * j + (GammaXYZValueExtend[Num2].fGY - (GammaXYZValueExtend[Num2].fGY - GammaXYZValueExtend[Num1].fGY) / nStep * Num2);
                    GammaXYZValueExtend[j].fGZ = (GammaXYZValueExtend[Num2].fGZ - GammaXYZValueExtend[Num1].fGZ) / nStep * j + (GammaXYZValueExtend[Num2].fGZ - (GammaXYZValueExtend[Num2].fGZ - GammaXYZValueExtend[Num1].fGZ) / nStep * Num2);
                    GammaXYZValueExtend[j].fBX = (GammaXYZValueExtend[Num2].fBX - GammaXYZValueExtend[Num1].fBX) / nStep * j + (GammaXYZValueExtend[Num2].fBX - (GammaXYZValueExtend[Num2].fBX - GammaXYZValueExtend[Num1].fBX) / nStep * Num2);
                    GammaXYZValueExtend[j].fBZ = (GammaXYZValueExtend[Num2].fBZ - GammaXYZValueExtend[Num1].fBZ) / nStep * j + (GammaXYZValueExtend[Num2].fBZ - (GammaXYZValueExtend[Num2].fBZ - GammaXYZValueExtend[Num1].fBZ) / nStep * Num2);
                    GammaXYZValueExtend[j].fBY = (GammaXYZValueExtend[Num2].fBY - GammaXYZValueExtend[Num1].fBY) / nStep * j + (GammaXYZValueExtend[Num2].fBY - (GammaXYZValueExtend[Num2].fBY - GammaXYZValueExtend[Num1].fBY) / nStep * Num2);
                    GammaXYZValueExtend[j].fWY = (GammaXYZValueExtend[Num2].fWY - GammaXYZValueExtend[Num1].fWY) / nStep * j + (GammaXYZValueExtend[Num2].fWY - (GammaXYZValueExtend[Num2].fWY - GammaXYZValueExtend[Num1].fWY) / nStep * Num2);
                    */
                     GammaXYZValueExtend[j].fRX = ((float)Math.Exp((float)Math.Log(GammaXYZValueExtend[Num2].fRX) - (((float)Math.Log(GammaXYZValueExtend[Num2].fRX) - (float)Math.Log(GammaXYZValueExtend[Num1].fRX)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1))) * (float)Math.Log(Num2)) * (float)Math.Pow(j, (((float)Math.Log(GammaXYZValueExtend[Num2].fRX) - (float)Math.Log(GammaXYZValueExtend[Num1].fRX)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1)))));
                     GammaXYZValueExtend[j].fRY = ((float)Math.Exp((float)Math.Log(GammaXYZValueExtend[Num2].fRY) - (((float)Math.Log(GammaXYZValueExtend[Num2].fRY) - (float)Math.Log(GammaXYZValueExtend[Num1].fRY)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1))) * (float)Math.Log(Num2)) * (float)Math.Pow(j, (((float)Math.Log(GammaXYZValueExtend[Num2].fRY) - (float)Math.Log(GammaXYZValueExtend[Num1].fRY)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1)))));
                     GammaXYZValueExtend[j].fRZ = ((float)Math.Exp((float)Math.Log(GammaXYZValueExtend[Num2].fRZ) - (((float)Math.Log(GammaXYZValueExtend[Num2].fRZ) - (float)Math.Log(GammaXYZValueExtend[Num1].fRZ)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1))) * (float)Math.Log(Num2)) * (float)Math.Pow(j, (((float)Math.Log(GammaXYZValueExtend[Num2].fRZ) - (float)Math.Log(GammaXYZValueExtend[Num1].fRZ)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1)))));
                     GammaXYZValueExtend[j].fGX = ((float)Math.Exp((float)Math.Log(GammaXYZValueExtend[Num2].fGX) - (((float)Math.Log(GammaXYZValueExtend[Num2].fGX) - (float)Math.Log(GammaXYZValueExtend[Num1].fGX)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1))) * (float)Math.Log(Num2)) * (float)Math.Pow(j, (((float)Math.Log(GammaXYZValueExtend[Num2].fGX) - (float)Math.Log(GammaXYZValueExtend[Num1].fGX)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1)))));
                     GammaXYZValueExtend[j].fGY = ((float)Math.Exp((float)Math.Log(GammaXYZValueExtend[Num2].fGY) - (((float)Math.Log(GammaXYZValueExtend[Num2].fGY) - (float)Math.Log(GammaXYZValueExtend[Num1].fGY)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1))) * (float)Math.Log(Num2)) * (float)Math.Pow(j, (((float)Math.Log(GammaXYZValueExtend[Num2].fGY) - (float)Math.Log(GammaXYZValueExtend[Num1].fGY)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1)))));
                     GammaXYZValueExtend[j].fGZ = ((float)Math.Exp((float)Math.Log(GammaXYZValueExtend[Num2].fGZ) - (((float)Math.Log(GammaXYZValueExtend[Num2].fGZ) - (float)Math.Log(GammaXYZValueExtend[Num1].fGZ)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1))) * (float)Math.Log(Num2)) * (float)Math.Pow(j, (((float)Math.Log(GammaXYZValueExtend[Num2].fGZ) - (float)Math.Log(GammaXYZValueExtend[Num1].fGZ)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1)))));
                     GammaXYZValueExtend[j].fBX = ((float)Math.Exp((float)Math.Log(GammaXYZValueExtend[Num2].fBX) - (((float)Math.Log(GammaXYZValueExtend[Num2].fBX) - (float)Math.Log(GammaXYZValueExtend[Num1].fBX)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1))) * (float)Math.Log(Num2)) * (float)Math.Pow(j, (((float)Math.Log(GammaXYZValueExtend[Num2].fBX) - (float)Math.Log(GammaXYZValueExtend[Num1].fBX)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1)))));
                     GammaXYZValueExtend[j].fBZ = ((float)Math.Exp((float)Math.Log(GammaXYZValueExtend[Num2].fBZ) - (((float)Math.Log(GammaXYZValueExtend[Num2].fBZ) - (float)Math.Log(GammaXYZValueExtend[Num1].fBZ)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1))) * (float)Math.Log(Num2)) * (float)Math.Pow(j, (((float)Math.Log(GammaXYZValueExtend[Num2].fBZ) - (float)Math.Log(GammaXYZValueExtend[Num1].fBZ)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1)))));
                     GammaXYZValueExtend[j].fBY = ((float)Math.Exp((float)Math.Log(GammaXYZValueExtend[Num2].fBY) - (((float)Math.Log(GammaXYZValueExtend[Num2].fBY) - (float)Math.Log(GammaXYZValueExtend[Num1].fBY)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1))) * (float)Math.Log(Num2)) * (float)Math.Pow(j, (((float)Math.Log(GammaXYZValueExtend[Num2].fBY) - (float)Math.Log(GammaXYZValueExtend[Num1].fBY)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1)))));
                     GammaXYZValueExtend[j].fWY = ((float)Math.Exp((float)Math.Log(GammaXYZValueExtend[Num2].fWY) - (((float)Math.Log(GammaXYZValueExtend[Num2].fWY) - (float)Math.Log(GammaXYZValueExtend[Num1].fWY)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1))) * (float)Math.Log(Num2)) * (float)Math.Pow(j, (((float)Math.Log(GammaXYZValueExtend[Num2].fWY) - (float)Math.Log(GammaXYZValueExtend[Num1].fWY)) / ((float)Math.Log(Num2) - (float)Math.Log(Num1)))));
                   
                }
              
                if (Num1 != 0)
                {
                    GammaXYZValueExtend[Num1].fRX = (GammaXYZValueExtend[Num1 - 1].fRX + GammaXYZValueExtend[Num1 + 1].fRX) / 2;
                    GammaXYZValueExtend[Num1].fRY = (GammaXYZValueExtend[Num1 - 1].fRY + GammaXYZValueExtend[Num1 + 1].fRY) / 2;
                    GammaXYZValueExtend[Num1].fRZ = (GammaXYZValueExtend[Num1 - 1].fRZ + GammaXYZValueExtend[Num1 + 1].fRZ) / 2;
                    GammaXYZValueExtend[Num1].fGX = (GammaXYZValueExtend[Num1 - 1].fGX + GammaXYZValueExtend[Num1 + 1].fGX) / 2;
                    GammaXYZValueExtend[Num1].fGY = (GammaXYZValueExtend[Num1 - 1].fGY + GammaXYZValueExtend[Num1 + 1].fGY) / 2;
                    GammaXYZValueExtend[Num1].fGZ = (GammaXYZValueExtend[Num1 - 1].fGZ + GammaXYZValueExtend[Num1 + 1].fGZ) / 2;
                    GammaXYZValueExtend[Num1].fBX = (GammaXYZValueExtend[Num1 - 1].fBX + GammaXYZValueExtend[Num1 + 1].fBX) / 2;
                    GammaXYZValueExtend[Num1].fBZ = (GammaXYZValueExtend[Num1 - 1].fBZ + GammaXYZValueExtend[Num1 + 1].fBZ) / 2;
                    GammaXYZValueExtend[Num1].fBY = (GammaXYZValueExtend[Num1 - 1].fBY + GammaXYZValueExtend[Num1 + 1].fBY) / 2;
                    GammaXYZValueExtend[Num1].fWY = (GammaXYZValueExtend[Num1 - 1].fWY + GammaXYZValueExtend[Num1 + 1].fWY) / 2;                                                                                                                                                                                                                                                  
                }
            }
        }

        public float CalcX(GammaXYZ[] GammaXYZValueExtend, int nGammaXYZValueExtendNum, int nRIndex, int nGIndex, int nBIndex)
        {
            nRIndex = nRIndex < 0 ? 0 : nRIndex;
            nRIndex = nRIndex > nGammaXYZValueExtendNum - 1 ? nGammaXYZValueExtendNum - 1 : nRIndex;
            nGIndex = nGIndex < 0 ? 0 : nGIndex;
            nGIndex = nGIndex > nGammaXYZValueExtendNum - 1 ? nGammaXYZValueExtendNum - 1 : nGIndex;
            nBIndex = nBIndex < 0 ? 0 : nBIndex;
            nBIndex = nBIndex > nGammaXYZValueExtendNum - 1 ? nGammaXYZValueExtendNum - 1 : nBIndex;
            float fXMolecule = GammaXYZValueExtend[nRIndex].fRX + GammaXYZValueExtend[nGIndex].fGX + GammaXYZValueExtend[nBIndex].fBX;
            float fDenominator = GammaXYZValueExtend[nRIndex].fRX + GammaXYZValueExtend[nRIndex].fRY + GammaXYZValueExtend[nRIndex].fRZ +
                GammaXYZValueExtend[nGIndex].fGX + GammaXYZValueExtend[nGIndex].fGY + GammaXYZValueExtend[nGIndex].fGZ +
                GammaXYZValueExtend[nBIndex].fBX + GammaXYZValueExtend[nBIndex].fBY + GammaXYZValueExtend[nBIndex].fBZ;
            return fXMolecule / fDenominator;
        }

        public float CalcY(GammaXYZ[] GammaXYZValueExtend, int nGammaXYZValueExtendNum, int nRIndex, int nGIndex, int nBIndex)
        {
            nRIndex = nRIndex < 0 ? 0 : nRIndex;
            nRIndex = nRIndex > nGammaXYZValueExtendNum - 1 ? nGammaXYZValueExtendNum - 1 : nRIndex;
            nGIndex = nGIndex < 0 ? 0 : nGIndex;
            nGIndex = nGIndex > nGammaXYZValueExtendNum - 1 ? nGammaXYZValueExtendNum - 1 : nGIndex;
            nBIndex = nBIndex < 0 ? 0 : nBIndex;
            nBIndex = nBIndex > nGammaXYZValueExtendNum - 1 ? nGammaXYZValueExtendNum - 1 : nBIndex;
            float fYMolecule = GammaXYZValueExtend[nRIndex].fRY + GammaXYZValueExtend[nGIndex].fGY + GammaXYZValueExtend[nBIndex].fBY;
            float fDenominator = GammaXYZValueExtend[nRIndex].fRX + GammaXYZValueExtend[nRIndex].fRY + GammaXYZValueExtend[nRIndex].fRZ +
                GammaXYZValueExtend[nGIndex].fGX + GammaXYZValueExtend[nGIndex].fGY + GammaXYZValueExtend[nGIndex].fGZ +
                GammaXYZValueExtend[nBIndex].fBX + GammaXYZValueExtend[nBIndex].fBY + GammaXYZValueExtend[nBIndex].fBZ;
            return fYMolecule / fDenominator;
        }

        public float CalcLv(GammaXYZ[] GammaXYZValueExtend, int nGammaXYZValueExtendNum, int nRIndex, int nGIndex, int nBIndex)
        {
            nRIndex = nRIndex < 0 ? 0 : nRIndex;
            nRIndex = nRIndex > nGammaXYZValueExtendNum - 1 ? nGammaXYZValueExtendNum - 1 : nRIndex;
            nGIndex = nGIndex < 0 ? 0 : nGIndex;
            nGIndex = nGIndex > nGammaXYZValueExtendNum - 1 ? nGammaXYZValueExtendNum - 1 : nGIndex;
            nBIndex = nBIndex < 0 ? 0 : nBIndex;
            nBIndex = nBIndex > nGammaXYZValueExtendNum - 1 ? nGammaXYZValueExtendNum - 1 : nBIndex;
            //    return GammaXYZValueExtend[nRIndex].fRY + GammaXYZValueExtend[nGIndex].fGY + GammaXYZValueExtend[nBIndex].fBY;
            return GammaXYZValueExtend[nRIndex].fWY;
        }



        public void CalcGYVGmaArr(float fGamma, float fMinLv, float fMaxLv, float[] GYV_Gma, int nNormalNum,double [] m_GammaWhiteIndex)
        {
            
            double[] ex = new double[nNormalNum+1];
            ex[0] = 0;
            for (int i = 0; i < (m_GammaWhiteIndex.Length-1); i++)
            {
                for (int j = 0; j < (nNormalNum + 1) / (m_GammaWhiteIndex.Length-1);j++)
                {
                    if (i >= (m_GammaWhiteIndex.Length / 3))
                    {
                        if (i == (m_GammaWhiteIndex.Length - 2))
                        {
                            double k = (m_GammaWhiteIndex[i] - m_GammaWhiteIndex[i-1]) / ((nNormalNum + 1) / (m_GammaWhiteIndex.Length - 1));
                            double b = m_GammaWhiteIndex[i-1] - k * ((i-1) * (nNormalNum + 1) / (m_GammaWhiteIndex.Length - 1));

                            if((i * (nNormalNum + 1) / (m_GammaWhiteIndex.Length - 1) + j)< nNormalNum + 1) ex[i * (nNormalNum + 1) / (m_GammaWhiteIndex.Length - 1) + j] = 2.15 - k * (i * (nNormalNum + 1) / (m_GammaWhiteIndex.Length - 1) + j) - b;
                        }
                        else
                        {
                            double k = (m_GammaWhiteIndex[i + 1] - m_GammaWhiteIndex[i]) / ((nNormalNum + 1) / (m_GammaWhiteIndex.Length - 1));
                            double b = m_GammaWhiteIndex[i] - k * (i * (nNormalNum + 1) / (m_GammaWhiteIndex.Length - 1));

                            ex[i * (nNormalNum + 1) / (m_GammaWhiteIndex.Length - 1) + j] = 2.15-k * (i * (nNormalNum + 1) / (m_GammaWhiteIndex.Length - 1) + j) - b;
                        }
                       
                    }
                    else
                        ex[i * (nNormalNum + 1) / (m_GammaWhiteIndex.Length - 1) + j] = 0;
                        
                   
                }           
            }
            ex[nNormalNum] = 0;

            

            float GammaIndex = fGamma;
            for (int i = 0; i <= nNormalNum; i++)
            {
                GammaIndex = fGamma;
                if (fGamma == (float)10.0)
                {
                    GammaIndex = (float)(-0.00000000040537332040953 * Math.Pow(i, 4) + 0.00000028190286544458100 * Math.Pow(i, 3) - 0.00007364759345335400000 * Math.Pow(i, 2) + 0.00905824447411523000000 * i + 1.79117987615713000000000);
                }
                else if (fGamma > 10) GammaIndex = (float)2.2;

                float fTmp = (float)Math.Pow(((float)i / (float)nNormalNum), GammaIndex);// + ex[i]);
                GYV_Gma[i] = (fMaxLv - fMinLv) * fTmp + fMinLv;

            }
        }

        public void CalcGYVGmaArr_Medical(float fGamma, float fMinLv, float fMaxLv, float[] GYV_Gma, int nNormalNum)
        {
            double A = 71.498068f, B = 94.593053f, C = 41.912053f, D = 9.8247004f, E = 0.28175407f, F = -1.1878455f, G = -0.18014349f, H = 0.14710899f, I = -0.017046845f;
            double a = -1.3011877f, b = -2.5840191E-2f, c = 8.0242636E-2f, d = -1.0320229E-1f, e = 1.3646699E-1f, f = 2.8745620E-2f, g = -2.5468404E-2f, h = -3.1978977E-3f, k = 1.2992634E-4f, m = 1.3635334E-3f;
            double JndMinTmp = Math.Log10((double)fMinLv);
            double JndMin = A + B * JndMinTmp + C * Math.Pow(JndMinTmp, 2) + D * Math.Pow(JndMinTmp, 3) + E * Math.Pow(JndMinTmp, 4) +
                F * Math.Pow(JndMinTmp, 5) + G * Math.Pow(JndMinTmp, 6) + H * Math.Pow(JndMinTmp, 7) + I * Math.Pow(JndMinTmp, 8);
            double JndMaxTmp = Math.Log10((double)fMaxLv);
            double JndMax = A + B * JndMaxTmp + C * Math.Pow(JndMaxTmp, 2) + D * Math.Pow(JndMaxTmp, 3) + E * Math.Pow(JndMaxTmp, 4) +
                F * Math.Pow(JndMaxTmp, 5) + G * Math.Pow(JndMaxTmp, 6) + H * Math.Pow(JndMaxTmp, 7) + I * Math.Pow(JndMaxTmp, 8);
            float[] JndArr = new float[nNormalNum + 1];
            float[] LnJndArr = new float[nNormalNum + 1];
            for (int i = 0; i <= nNormalNum; i++)
            {
                JndArr[i] = (float)(JndMax - JndMin) * i / nNormalNum + (float)JndMin;
                double LnJndTmp = Math.Log(JndArr[i], Math.E);
                double LnJndMolecule = a + c * LnJndTmp + e * Math.Pow(LnJndTmp, 2) + g * Math.Pow(LnJndTmp, 3) + m * Math.Pow(LnJndTmp, 4);
                double LnJndDenominator = 1 + b * LnJndTmp + d * Math.Pow(LnJndTmp, 2) + f * Math.Pow(LnJndTmp, 3) + h * Math.Pow(LnJndTmp, 4) + k * Math.Pow(LnJndTmp, 5);
                LnJndArr[i] = (float)LnJndMolecule / (float)LnJndDenominator;
                GYV_Gma[i] = (float)Math.Pow(10f, (double)LnJndArr[i]);
            }
        }

        public void CalcOutputRGB(GammaXYZ[] GammaXYZValueExtend, GammaXYZTmp[] GammaXYZTmp,
            int nGammaXYZValueExtendNum, float[] GYV_Gma, GammaRGB[] GRGBOutputArr, int nNormalNum)
        {
            GRGBOutputArr[0].nR = 0;
            GRGBOutputArr[0].nG = 0;
            GRGBOutputArr[0].nB = 0;
            for (int i = 1; i <= nNormalNum; i++)
            {
                float fLvMin = 9999f;
                for (int j = 0; j < nGammaXYZValueExtendNum; j++)
                {
                    if (fLvMin > Math.Abs(GammaXYZTmp[j].fLv - GYV_Gma[i]))
                    {
                        fLvMin = Math.Abs(GammaXYZTmp[j].fLv - GYV_Gma[i]);
                        GRGBOutputArr[i].nR = GammaXYZTmp[j].nRIndex;
                        GRGBOutputArr[i].nG = GammaXYZTmp[j].nGIndex;
                        GRGBOutputArr[i].nB = GammaXYZTmp[j].nBIndex;
                        GRGBOutputArr[i].fLv = GammaXYZTmp[j].fLv;
                    }
                }
            }
        }

        public void OutputRGBBelowValueInterpolation(GammaRGB[] GRGBOutputArr, int nNormalNum, int nBelowR, int nBelowG, int nBelowB)
        {
            if (nBelowR > nNormalNum)
            {
                return;
            }
            for (int i = 1; i < nBelowR; i++)
            {
                GRGBOutputArr[i].nR = (GRGBOutputArr[0].nR * (nBelowR - i) + GRGBOutputArr[nBelowR].nR * i) / nBelowR;
            }
            for (int i = 1; i < nBelowG; i++)
            {
                GRGBOutputArr[i].nG = (GRGBOutputArr[0].nG * (nBelowG - i) + GRGBOutputArr[nBelowG].nG * i) / nBelowG;
            }
            for (int i = 1; i < nBelowB; i++)
            {
                GRGBOutputArr[i].nB = (GRGBOutputArr[0].nB * (nBelowB - i) + GRGBOutputArr[nBelowB].nB * i) / nBelowB;
            }
        }


        public void CalcXYZTmp2(GammaScreenMode ScreenMode, GammaXYZ[] GammaXYZValueExtend, GammaXYZTmp[] GammaXYZTmp, int nGammaXYZValueExtendNum)
        {
            for (int i = 0; i < nGammaXYZValueExtendNum; i++)
            {
                int nMaxIndex = i;
                nMaxIndex = (nMaxIndex > (nGammaXYZValueExtendNum - 1)) ? nGammaXYZValueExtendNum - 1 : nMaxIndex;
                int nRIndex = nMaxIndex, nGIndex = nMaxIndex, nBIndex = nMaxIndex;
                if (i == 0)
                {
                    nRIndex = 0;
                    nGIndex = 0;
                    nBIndex = 0;
                }
                else
                {
                    bool bIsStop = false, bXIsStop = false, bYIsStop = false;
                    while (bIsStop == false)
                    {
                        float fX = CalcX(GammaXYZValueExtend, nGammaXYZValueExtendNum, nRIndex, nGIndex, nBIndex);
                        float fY = CalcY(GammaXYZValueExtend, nGammaXYZValueExtendNum, nRIndex, nGIndex, nBIndex);

                                bXIsStop = true;
                                bYIsStop = true;
                       
                        if (bXIsStop && bYIsStop)
                        {
                            bIsStop = true;
                        }
                    }
                }
                float fXTmp = CalcX(GammaXYZValueExtend, nGammaXYZValueExtendNum, nRIndex, nGIndex, nBIndex);
                float fYTmp = CalcY(GammaXYZValueExtend, nGammaXYZValueExtendNum, nRIndex, nGIndex, nBIndex);
                float fLvTmp = CalcLv(GammaXYZValueExtend, nGammaXYZValueExtendNum, nRIndex, nGIndex, nBIndex);
                GammaXYZTmp[i].nRIndex = nRIndex;
                GammaXYZTmp[i].nGIndex = nGIndex;
                GammaXYZTmp[i].nBIndex = nBIndex;
                GammaXYZTmp[i].fX = fXTmp;
                GammaXYZTmp[i].fY = fYTmp;
                GammaXYZTmp[i].fLv = fLvTmp;
                switch (ScreenMode)
                {
                    case GammaScreenMode.SCN_RGB:
                        GammaXYZTmp[i].fLv = fLvTmp;
                        break;
                    case GammaScreenMode.SCN_WRGB:
                        GammaXYZTmp[i].fLv = GammaXYZValueExtend[nGIndex].fGY;
                        break;
                }
            }
        }

        public void ExtendRGBOutputArr(GammaRGB[] GRGBOutputArr, int nNormalNum, ref GammaRGB[] GRGBOutputArrExtend, int nExtendNum, GammaXYZ[] GammaXYZValueExtend, int nGammaXYZValueExtendNum)
        {
            if (nExtendNum == 256 || (nExtendNum != 512 && nExtendNum != 1024))
            {
                GRGBOutputArrExtend = GRGBOutputArr;
                LogHelper.WriteToLog("GRGBOutputArrExtend = GRGBOutputArr" , LogLevel.INFO);
                return;
            }
            int nIndexTimes = 1;
            if (nExtendNum == 512)
            {
                nIndexTimes = 2;
            }
            else if (nExtendNum == 1024)
            {
                nIndexTimes = 4;
            }
            GammaRGB2[] GammaRGB2Value = new GammaRGB2[nNormalNum + 1];
            GammaRGB2[] GammaRGB2Value2 = new GammaRGB2[nExtendNum + 1];
            for (int i = 0; i <= nNormalNum; i++)
            {
                GammaRGB2Value[i] = new GammaRGB2();
                GammaRGB2Value[i].fIndex = (float)i * nIndexTimes;
                GammaRGB2Value[i].fR = (float)GRGBOutputArr[i].nR;
                GammaRGB2Value[i].fG = (float)GRGBOutputArr[i].nG;
                GammaRGB2Value[i].fB = (float)GRGBOutputArr[i].nB;
            }
            for (int i = 0; i <= nExtendNum; i++)
            {
                GammaRGB2Value2[i] = new GammaRGB2();
            }
            for (int i = 0; i <= nExtendNum; i++)
            {
                if (i == 0)
                {
                    GammaRGB2Value2[i].fIndex = (float)i;
                    GammaRGB2Value2[i].fR = 0f;
                    GammaRGB2Value2[i].fG = 0f;
                    GammaRGB2Value2[i].fB = 0f;
                    continue;
                }
                if (i == nExtendNum)
                {
                    GammaRGB2Value2[i].fIndex = (float)i;
                    GammaRGB2Value2[i].fR = GammaRGB2Value[nNormalNum].fR;
                    GammaRGB2Value2[i].fG = GammaRGB2Value[nNormalNum].fG;
                    GammaRGB2Value2[i].fB = GammaRGB2Value[nNormalNum].fB;
                    continue;
                }
                float fI = (float)i;
                for (int j = 0; j < nNormalNum; j++)
                {
                    if (fI > GammaRGB2Value[j].fIndex && fI <= GammaRGB2Value[j + 1].fIndex)
                    {
                        GammaRGB2Value2[i].fIndex = (float)i;
                        GammaRGB2Value2[i].fR = ((GammaRGB2Value[j + 1].fIndex - fI) * GammaRGB2Value[j].fR + (fI - GammaRGB2Value[j].fIndex) * GammaRGB2Value[j + 1].fR) / (GammaRGB2Value[j + 1].fIndex - GammaRGB2Value[j].fIndex);
                        GammaRGB2Value2[i].fG = ((GammaRGB2Value[j + 1].fIndex - fI) * GammaRGB2Value[j].fG + (fI - GammaRGB2Value[j].fIndex) * GammaRGB2Value[j + 1].fG) / (GammaRGB2Value[j + 1].fIndex - GammaRGB2Value[j].fIndex);
                        GammaRGB2Value2[i].fB = ((GammaRGB2Value[j + 1].fIndex - fI) * GammaRGB2Value[j].fB + (fI - GammaRGB2Value[j].fIndex) * GammaRGB2Value[j + 1].fB) / (GammaRGB2Value[j + 1].fIndex - GammaRGB2Value[j].fIndex);
                        break;
                    }
                }
            }

            for (int i = 0; i <= nExtendNum; i++)
            {
                GRGBOutputArrExtend[i].nR = (int)GammaRGB2Value2[i].fR;
                GRGBOutputArrExtend[i].nG = (int)GammaRGB2Value2[i].fG;
                GRGBOutputArrExtend[i].nB = (int)GammaRGB2Value2[i].fB;
                GRGBOutputArrExtend[i].fLv = CalcLv(GammaXYZValueExtend, nGammaXYZValueExtendNum, GRGBOutputArrExtend[i].nR, GRGBOutputArrExtend[i].nG, GRGBOutputArrExtend[i].nB);
            }
        }


        
        public static bool CalcGammaData2(GammaScreenMode ScreenMode, float fGamma,/* float fTargetX, float fTargetXRange,
                float fTargetY, float fTargetYRange, */GammaXYLv[] GammaXYLvValue, int nGammaXYLvValueNum, GammaXYZ[] GXYZOutputArr,
                GammaRGB[] GRGBOutputArr, int nNormalNum, ref GammaRGB[] GRGBOutputArrExtend, int nExtendNum, int nDataRange,
                int nBelowR, int nBelowG, int nBelowB, ref float fX, ref float fY,double[] m_GammaWhiteIndex, int LowgrayLinear)
        {
            int nGammaXYZValueNum = nGammaXYLvValueNum - 3, nGammaXYZValueExtendNum = nDataRange;
            GammaXYZ[] GammaXYZValue = new GammaXYZ[nGammaXYZValueNum];
            for (int i = 0; i < nGammaXYZValueNum; i++)
            {
                GammaXYZValue[i] = new GammaXYZ();
            }
            GammaXYZ[] GammaXYZValueExtend = new GammaXYZ[nGammaXYZValueExtendNum];
            for (int i = 0; i < nGammaXYZValueExtendNum; i++)
            {
                GammaXYZValueExtend[i] = new GammaXYZ();
            }
            GammaXYZTmp[] GammaXYZTmp = new GammaXYZTmp[nGammaXYZValueExtendNum];
            for (int i = 0; i < nGammaXYZValueExtendNum; i++)
            {
                GammaXYZTmp[i] = new GammaXYZTmp();
            }
            float[] GYV_Gma = new float[nNormalNum + 1];
            float[] GOutputGLArr = new float[nNormalNum + 1];

            CalcGamma calcGamma = new CalcGamma();
            calcGamma.ChangeXYLvValueToXYZValue2(GammaXYLvValue, GammaXYZValue, nGammaXYZValueNum);
            calcGamma.ExtendXYZValue(GammaXYZValue, nGammaXYZValueNum, GammaXYZValueExtend, nGammaXYZValueExtendNum, fGamma);
            calcGamma.CalcXYZTmp2(ScreenMode,  GammaXYZValueExtend, GammaXYZTmp, nGammaXYZValueExtendNum);
            if (fGamma == 99f)
            {
                calcGamma.CalcGYVGmaArr_Medical(fGamma, GammaXYZTmp[0].fLv, GammaXYZTmp[nGammaXYZValueExtendNum - 1].fLv, GYV_Gma, nNormalNum);
            }
            else
            {

                calcGamma.CalcGYVGmaArr(fGamma, GammaXYZTmp[0].fLv, GammaXYZTmp[nGammaXYZValueExtendNum - 1].fLv, GYV_Gma, nNormalNum, m_GammaWhiteIndex);
               // calcGamma.CalcGYVGmaArr(fGamma, GammaXYZTmp[0].fLv, GammaXYZTmp[nub].fLv, GYV_Gma, nNormalNum, m_GammaWhiteIndex);
            }
            calcGamma.CalcOutputRGB(GammaXYZValueExtend, GammaXYZTmp, nGammaXYZValueExtendNum, GYV_Gma, GRGBOutputArr, nNormalNum);
            if (LowgrayLinear==1)
                calcGamma.OutputRGBBelowValueInterpolation(GRGBOutputArr, nNormalNum, nBelowR, nBelowG, nBelowB);
            calcGamma.ExtendRGBOutputArr(GRGBOutputArr, nNormalNum, ref GRGBOutputArrExtend, nExtendNum, GammaXYZValueExtend, nGammaXYZValueExtendNum);

            bool bOK = true;
            fX = calcGamma.CalcX(GammaXYZValueExtend, nGammaXYZValueExtendNum, GRGBOutputArr[nNormalNum].nR, GRGBOutputArr[nNormalNum].nG, GRGBOutputArr[nNormalNum].nB);
            fY = calcGamma.CalcY(GammaXYZValueExtend, nGammaXYZValueExtendNum, GRGBOutputArr[nNormalNum].nR, GRGBOutputArr[nNormalNum].nG, GRGBOutputArr[nNormalNum].nB);
            /*if (Math.Abs(fX - fTargetX) < fTargetXRange && Math.Abs(fY - fTargetY) < fTargetYRange)
            {
                bOK = true;
            }*/
            return bOK;
        }
    }
}
