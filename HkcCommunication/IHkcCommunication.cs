using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteBalanceCorrection
{
    public interface IHkcCommunication
    {
        bool Connect(int timeout);
        void Disconnect();
        bool Send(string sStr);
        string Receive(int nReceiveTimeout = 1000);

        // ----------Gamma----------
        bool GammaStart(int nReceiveTimeout = 1000);
        bool GammaSetPattern(int nR, int nG, int nB, int nReceiveTimeout = 1000);
        bool GammaSet(int[] nRArr, int[] nGArr, int[] nBArr, int nReceiveTimeout = 1000);
        bool GammaSave(int nNum, int[] nRArr, int[] nGArr, int[] nBArr, int nReceiveTimeout = 1000,int MCU=98);
        bool GammaApply(int nReceiveTimeout = 1000);
        bool GammaQuit(int nReceiveTimeout = 1000);
        bool GammaSaveMeasureData(List<GammaXYLv> GammaXYLvs, int nReceiveTimeout = 1000);
        List<GammaXYLv> GammaGetMeasureData(int nReceiveTimeout = 1000);
        // ----------Gamma----------

        // ----------WB----------
        bool WBSetUserPattern(int nR, int nG, int nB, int nReceiveTimeout = 1000);
        bool WBSave(int nNum, int nR, int nG, int nB, int nReceiveTimeout = 1000);
        bool WBQuit(int nReceiveTimeout = 1000);
        bool WBClose(int nReceiveTimeout = 1000);
        // ----------WB----------

        // ----------Lv----------
        int LvGetEBook(int nReceiveTimeout = 1000);
        bool LvSetEBook(int nBackLight, int nReceiveTimeout = 1000);
        bool LvSaveEBook(int nBackLight, int nReceiveTimeout = 1000);
        int LvGetMax(int nReceiveTimeout = 1000);
        int LvGet(int nReceiveTimeout = 1000);
        bool LvSet(int nBackLight, int nReceiveTimeout = 1000);
        bool LvSave(int nBackLight, int nReceiveTimeout = 1000);
        bool MaxLvSave(int nBackLight, int nReceiveTimeout = 1000);
        // ----------Lv----------
        // ----------EDID----------
        bool SetEDID_SN(string str, int nReceiveTimeout = 1000);
        bool SetEDID_ID(string str, int nReceiveTimeout = 1000);
        bool SetEDID_PRODUCT_CODE(int  scode, int nReceiveTimeout = 1000);
        bool SetEDID_DATE(int year,int week , int nReceiveTimeout = 1000);
        bool SetEDID_NAME(string str, int nReceiveTimeout = 1000);
        // ----------EDID----------
        //-----------Gamut---------
        bool SetColorGamut(int[] nuble, string methodName, int nReceiveTimeout = 1000);

        //----------Gamut----------
        // ----------color----------
        bool ColorSetPattern(ColorSpace.RGB rgb, int nReceiveTimeout = 1000);
        bool ColorSendData(string gamut, ColorSpace.RGB[] nRGBArr, int nReceiveTimeout = 1000);

        // ----------color----------
        // ----------Other----------
        bool SetSN_Display(int  SND, int nReceiveTimeout = 1000);
        string GetSystemInfo( int nReceiveTimeout = 1000);
        bool SetLanguage(int nuble, int nReceiveTimeout = 1000);
        bool SetMode(string nuble, int nReceiveTimeout = 1000);
         bool SetEnergyTips(int nuble ,int nReceiveTimeout = 1000);
        bool Set_OnOffFunct(int nuble, int nReceiveTimeout = 1000);
        bool Set_OSDdefLvValue(int nuble, int nReceiveTimeout = 1000);
        void SetWindowSize(int WindowSize);
        // ----------Other----------
    }
}
