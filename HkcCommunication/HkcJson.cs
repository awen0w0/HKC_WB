using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;
using System.Data.SqlTypes;

namespace WhiteBalanceCorrection
{
    public class HkcRequest
    {
        public string className { get; set; }
        public string methodName { get; set; }
    }

    public class HkcRequestObject
    {
        public string requestType { get; set; }
        public HkcRequest request { get; set; }
    }

    public class HkcRequest_IntNum
    {
        public string className { get; set; }
        public string methodName { get; set; }
        public int[] parameters { get; set; }
    }

    public class HkcRequestObject_IntNum
    {
        public string requestType { get; set; }
        public HkcRequest_IntNum request { get; set; }
    }

    public class HkcRequest_OneIntNum
    {
        public string className { get; set; }
        public string methodName { get; set; }
        public int[] parameters { get; set; }
    }

    public class HkcRequestObject_OneIntNum
    {
        public string requestType { get; set; }
        public HkcRequest_OneIntNum request { get; set; }
    }

    public class HkcRequest_StrNum
    {
        public string className { get; set; }
        public string methodName { get; set; }
        public string[] parameters { get; set; }
    }

    public class HkcRequestObject_StrNum
    {
        public string requestType { get; set; }
        public HkcRequest_StrNum request { get; set; }
    }

    public class HkcRequest_IntArr
    {
        public string className { get; set; }
        public string methodName { get; set; }
        public int[][] parameters { get; set; }
    }

    public class HkcRequestObject_IntArr
    {
        public string requestType { get; set; }
        public HkcRequest_IntArr request { get; set; }
    }

    public class HkcRequest_Objects
    {
        public string className { get; set; }
        public string methodName { get; set; }
        public object[] parameters { get; set; }
    }

    public class HkcRequestObject_Objects
    {
        public string requestType { get; set; }
        public HkcRequest_Objects request { get; set; }
    }

    public class HkcReturnObject
    {
        public string paramsJson { get; set; }
        public string requestName { get; set; }
        public string responseName { get; set; }
        public string status { get; set; }
    }

    public class HkcReturnObject_NumArr
    {
        public int[][] paramsJson { get; set; }
        public string requestName { get; set; }
        public string responseName { get; set; }
        public string status { get; set; }
    }
    public class HkcRequest_Str
    {
        public string className { get; set; }
        public string methodName { get; set; }
        public string[] parameters { get; set; }
    }

    public class HkcRequestObject_Str
    {
        public string requestType { get; set; }
        public HkcRequest_Str request { get; set; }
    }


    class HkcJson
    {
        public static string EncodeJsonCMD_String(string requestType, string className, string methodName, string str)
        {
            HkcRequestObject_Str sRB = new HkcRequestObject_Str();
            sRB.request = new HkcRequest_Str();
            sRB.requestType = requestType;
            sRB.request.className = className;
            sRB.request.methodName = methodName;
            sRB.request.parameters = new string[1];
            sRB.request.parameters[0]  = str;
            return JsonConvert.SerializeObject(sRB) + "\n";
        }

        public static string EncodeJsonCMD(string requestType, string className, string methodName)
        {
            HkcRequestObject sRB = new HkcRequestObject();
            sRB.request = new HkcRequest();
            sRB.requestType = requestType;
            sRB.request.className = className;
            sRB.request.methodName = methodName;
            return JsonConvert.SerializeObject(sRB) + "\n";
        }

        public static string EncodeJsonCMD_RGB(string requestType, string className, string methodName, int nR, int nG, int nB)
        {
            HkcRequestObject_IntNum sRB = new HkcRequestObject_IntNum();
            sRB.request = new HkcRequest_IntNum();
            sRB.requestType = requestType;
            sRB.request.className = className;
            sRB.request.methodName = methodName;
            sRB.request.parameters = new int[3];
            sRB.request.parameters[0] = nR;
            sRB.request.parameters[1] = nG;
            sRB.request.parameters[2] = nB;
            return JsonConvert.SerializeObject(sRB) + "\n";
        }
        public static string EncodeJsonCMD_NubleIntNum(string requestType, string className, string methodName, int[] nNum)
        {
            HkcRequestObject_OneIntNum sRB = new HkcRequestObject_OneIntNum();
            sRB.request = new HkcRequest_OneIntNum();
            sRB.requestType = requestType;
            sRB.request.className = className;
            sRB.request.methodName = methodName;
            sRB.request.parameters = new int[nNum.Length];
            sRB.request.parameters = nNum;
            return JsonConvert.SerializeObject(sRB) + "\n";
        }
        public static string EncodeJsonCMD_OneIntNum(string requestType, string className, string methodName, int nNum)
        {
            HkcRequestObject_OneIntNum sRB = new HkcRequestObject_OneIntNum();
            sRB.request = new HkcRequest_OneIntNum();
            sRB.requestType = requestType;
            sRB.request.className = className;
            sRB.request.methodName = methodName;
            sRB.request.parameters = new int[1];
            sRB.request.parameters[0] = nNum;
            return JsonConvert.SerializeObject(sRB) + "\n";
        }
        public static string EncodeJsonCMD_TwoIntNum(string requestType, string className, string methodName, int nNum1, int nNum2)
        {
            HkcRequestObject_OneIntNum sRB = new HkcRequestObject_OneIntNum();
            sRB.request = new HkcRequest_OneIntNum();
            sRB.requestType = requestType;
            sRB.request.className = className;
            sRB.request.methodName = methodName;
            sRB.request.parameters = new int[2];
            sRB.request.parameters[0] = nNum1;
            sRB.request.parameters[1] = nNum2;
            return JsonConvert.SerializeObject(sRB) + "\n";
        }

        public static string EncodeJsonCMD_NRGB(string requestType, string className, string methodName, string sNum, int nR, int nG, int nB)
        {
            HkcRequestObject_StrNum sRB = new HkcRequestObject_StrNum();
            sRB.request = new HkcRequest_StrNum();
            sRB.requestType = requestType;
            sRB.request.className = className;
            sRB.request.methodName = methodName;
            sRB.request.parameters = new string[4];
            sRB.request.parameters[0] = sNum;
            sRB.request.parameters[1] = nR.ToString();
            sRB.request.parameters[2] = nG.ToString();
            sRB.request.parameters[3] = nB.ToString();
            return JsonConvert.SerializeObject(sRB) + "\n";
        }

        public static string EncodeJsonCMD_RGBArr(string requestType, string className, string methodName, int[] nRArr, int[] nGArr, int[] nBArr)
        {
            HkcRequestObject_IntArr sRB = new HkcRequestObject_IntArr();
            sRB.request = new HkcRequest_IntArr();
            sRB.requestType = requestType;
            sRB.request.className = className;
            sRB.request.methodName = methodName;
            sRB.request.parameters = new int[3][];
            int nCurveNum = 257;
            sRB.request.parameters[0] = new int[nCurveNum];
            sRB.request.parameters[1] = new int[nCurveNum];
            sRB.request.parameters[2] = new int[nCurveNum];
            for (int i = 0; i < nCurveNum; i++)
            {
                sRB.request.parameters[0][i] = nRArr[i];
                sRB.request.parameters[1][i] = nGArr[i];
                sRB.request.parameters[2][i] = nBArr[i];
            }
            return JsonConvert.SerializeObject(sRB) + "\n";
        }

        public static string EncodeJsonCMD_RGBArrAndNum(string requestType, string className, string methodName, int nNum, int[] nRArr, int[] nGArr, int[] nBArr)
        {
            HkcRequestObject_Objects sRB = new HkcRequestObject_Objects();
            sRB.request = new HkcRequest_Objects();
            sRB.requestType = requestType;
            sRB.request.className = className;
            sRB.request.methodName = methodName;
            sRB.request.parameters = new object[4];
            sRB.request.parameters[0] = nNum;
            sRB.request.parameters[1] = nRArr;
            sRB.request.parameters[2] = nGArr;
            sRB.request.parameters[3] = nBArr;
            return JsonConvert.SerializeObject(sRB) + "\n";
        }
        public static string EncodeJsonCMD_RGBArrAndNumData(string requestType, string className, string methodName, int nNum, int DataN, int[] nRGBArr)
        {
            HkcRequestObject_Objects sRB = new HkcRequestObject_Objects();
            sRB.request = new HkcRequest_Objects();
            sRB.requestType = requestType;
            sRB.request.className = className;
            sRB.request.methodName = methodName;
            sRB.request.parameters = new object[3];
            sRB.request.parameters[0] = nNum;
            sRB.request.parameters[1] = DataN;
            sRB.request.parameters[2] = nRGBArr;
            return JsonConvert.SerializeObject(sRB) + "\n";
        }
        public static string EncodeJsonCMD_GammaXYLvs(string requestType, string className, string methodName, List<GammaXYLv> GammaXYLvs)
        {
            HkcRequestObject_Objects sRB = new HkcRequestObject_Objects();
            sRB.request = new HkcRequest_Objects();
            sRB.requestType = requestType;
            sRB.request.className = className;
            sRB.request.methodName = methodName;
            sRB.request.parameters = new object[GammaXYLvs.Count];
            for (int i = 0; i < GammaXYLvs.Count; i++)
            {
                int[] nArr = new int[3];
                nArr[0] = Convert.ToInt32(GammaXYLvs[i].fX * 1000);
                nArr[1] = Convert.ToInt32(GammaXYLvs[i].fY * 1000);
                nArr[2] = Convert.ToInt32(GammaXYLvs[i].fLv);
                sRB.request.parameters[i] = nArr;
            }
            return JsonConvert.SerializeObject(sRB) + "\n";
        }



        public static string DecodeJsonCMD(string jsonText, string requestName, string responseName)
        {
            HkcReturnObject sRB = JsonConvert.DeserializeObject<HkcReturnObject>(jsonText);
            try
            {
                if (sRB.status == "SUCCESS" && sRB.requestName == requestName && sRB.responseName == responseName)
                {
                    return sRB.paramsJson;
                }
                return "false";
            }
            catch (System.Exception ex)
            {
                return "false";
            }
        }

        public static int[][] DecodeJsonCMD_NumArr(string jsonText, string requestName, string responseName)
        {
            HkcReturnObject_NumArr sRB = JsonConvert.DeserializeObject<HkcReturnObject_NumArr>(jsonText);
            try
            {
                if (sRB.status == "SUCCESS" && sRB.requestName == requestName && sRB.responseName == responseName)
                {
                    return sRB.paramsJson;
                }
                return null;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        // ----------Gamma----------
        public static string EncodeJsonCMD_GammaStart()
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD("WB", "autotest.Gamma", "start");
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_GammaStart(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Gamma.start", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public static string EncodeJsonCMD_GammaSetPattern(int nR, int nG, int nB)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_RGB("WB", "autotest.Gamma", "setPattern", nR, nG, nB);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_GammaSetPattern(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Gamma.setPattern", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public static string EncodeJsonCMD_GammaSet(int[] nRArr, int[] nGArr, int[] nBArr)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_RGBArr("WB", "autotest.Gamma", "set", nRArr, nGArr, nBArr);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_GammaSet(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Gamma.set", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public static string EncodeJsonCMD_GammaSave(int nNum, int DataN,int[] nRArr, int[] nGArr, int[] nBArr)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_RGBArrAndNum("WB", "autotest.Gamma", "save", nNum,  nRArr, nGArr, nBArr);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }
        public static bool DecodeJsonCMD_GammaSave(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Gamma.save", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        public static string EncodeJsonCMD_GammaSaveDataN(int nNum, int DataN, int[] nRGBArr)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_RGBArrAndNumData("WB", "autotest.Gamma", "senddata", nNum, DataN ,nRGBArr);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }
        public static bool DecodeJsonCMD_GammaDataN(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Gamma.senddata", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        public static string EncodeJsonCMD_GammaCRC(int nNum)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_OneIntNum("WB", "autotest.Gamma", "save",nNum);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static string  DecodeJsonCMD_GammaCRC(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Gamma.save", "WB");
                if (paramsJson != "false")
                {
                    return paramsJson;
                }
            }
            catch (System.Exception ex)
            {
                return string.Empty; ;
            }
            return string.Empty;
        }


        public static string EncodeJsonCMD_ColorSaveDataN(int nNum, int DataN, int[] nRGBArr)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_RGBArrAndNumData("WB", "autotest.3DLUT", "data", nNum, DataN, nRGBArr);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }
        public static bool DecodeJsonCMD_ColorDataN(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.3DLUT.data", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        public static string EncodeJsonCMD_ColorCRC(string nNum)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_String("WB", "autotest.3DLUT", "save", nNum);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }
        public static string DecodeJsonCMD_ColorCRC(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.3DLUT.save", "WB");
                if (paramsJson != "false")
                {
                    return paramsJson;
                }
            }
            catch (System.Exception ex)
            {
                return string.Empty; ;
            }
            return string.Empty;
        }
        public static string EncodeJsonCMD_GammaApply()
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD("WB", "autotest.Gamma", "apply");
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_GammaApply(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Gamma.apply", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public static string EncodeJsonCMD_GammaQuit()
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD("WB", "autotest.Gamma", "quit");
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_GammaQuit(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Gamma.quit", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public static string EncodeJsonCMD_GammaSaveMeasureData(List<GammaXYLv> GammaXYLvs)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_GammaXYLvs("WB", "autotest.Gamma", "saveMeasureData", GammaXYLvs);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_GammaSaveMeasureData(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Gamma.saveMeasureData", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public static string EncodeJsonCMD_GammaGetMeasureData()
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD("WB", "autotest.Gamma", "getMeasureDate");
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static List<GammaXYLv> DecodeJsonCMD_GammaGetMeasureData(string jsonText)
        {
            List<GammaXYLv> GammaXYLvs = new List<GammaXYLv>();
            try
            {
                int[][] paramsJson = HkcJson.DecodeJsonCMD_NumArr(jsonText, "autotest.Gamma.getMeasureDate", "WB");
                for (int i = 0; i < paramsJson.Length; i++)
                {
                    if (paramsJson[i].Length == 3)
                    {
                        GammaXYLv gTmp = new GammaXYLv();
                        gTmp.fX = float.Parse(paramsJson[i][0].ToString()) / 1000;
                        gTmp.fY = float.Parse(paramsJson[i][1].ToString()) / 1000;
                        gTmp.fLv = float.Parse(paramsJson[i][2].ToString());
                        GammaXYLvs.Add(gTmp);
                    }
                }
            }
            catch (System.Exception ex)
            {
            	
            }
            return GammaXYLvs;
        }
        // ----------Gamma----------

        // ----------WB----------

        public static string EncodeJsonCMD_WBSetUserPattern(int nR, int nG, int nB)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_RGB("WB", "autotest.WB", "setPatternByUser", nR, nG, nB);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_WBSetUserPattern(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.WB.setPatternByUser", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public static string EncodeJsonCMD_WBSet(int nR, int nG, int nB)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_RGB("WB", "autotest.WB", "set", nR, nG, nB);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_WBSet(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.WB.set", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public static string EncodeJsonCMD_WBSave(string sNum, int nR, int nG, int nB)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_NRGB("WB", "autotest.WB", "save", sNum, nR, nG, nB);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_WBSave(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.WB.save", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public static string EncodeJsonCMD_WBQuit()
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD("WB", "autotest.WB", "quit");
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_WBQuit(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.WB.quit", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        public static string EncodeJsonCMD_WBClose()
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD("WB", "autotest.WB", "close");
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_WBClose(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.WB.close", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        // ----------WB----------

        // ----------LV----------
        public static string EncodeJsonCMD_GetEBookValue()
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD("WB", "autotest.BackLight", "getEbookLv");
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static int DecodeJsonCMD_GetEBookValue(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.BackLight.getEbookLv", "WB");
                int nBackLight = Common.IsValidInt(paramsJson) ? Int32.Parse(paramsJson) : 0;
                return nBackLight;
            }
            catch (System.Exception ex)
            {
                return 0;
            }
        }
        public static string EncodeJsonCMD_SetEBookValue(int nBackLight)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_OneIntNum("WB", "autotest.BackLight", "setEBookLv", nBackLight);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SetEBookValue(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.BackLight.setEBookLv", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }

        }
        public static string EncodeJsonCMD_SaveEBookValue(int nBackLight)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_OneIntNum("WB", "autotest.BackLight", "saveEbookLv", nBackLight);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SaveEBookValue(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.BackLight.saveEbookLv", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }

        }
        public static string EncodeJsonCMD_GetMaxBackLightValue()
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD("WB", "autotest.BackLight", "getMaxBackLightValue");
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static int DecodeJsonCMD_GetMaxBackLightValue(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.BackLight.getMaxBackLightValue", "WB");
                int nBackLight = Common.IsValidInt(paramsJson) ? Int32.Parse(paramsJson) : 0;
                return nBackLight;
            }
            catch (System.Exception ex)
            {
                return 0;
            }
        }

        public static string EncodeJsonCMD_GetBackLight()
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD("WB", "autotest.BackLight", "getBackLightValue");
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;

        }

        public static int DecodeJsonCMD_GetBackLight(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.BackLight.getBackLightValue", "WB");
                int nBackLight = Common.IsValidInt(paramsJson) ? Int32.Parse(paramsJson) : 0;
                return nBackLight;
            }
            catch (System.Exception ex)
            {
                return 0;
            }

        }

        public static string EncodeJsonCMD_SetBackLight(int nBackLight)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_OneIntNum("WB", "autotest.BackLight", "setBackLightValue", nBackLight);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SetBackLight(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.BackLight.setBackLightValue", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }

        }
        public static string EncodeJsonCMD_SaveBackLight(int nBackLight)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_OneIntNum("WB", "autotest.BackLight", "saveBackLightValue", nBackLight);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SaveBackLight(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.BackLight.saveBackLightValue", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }

        }
        public static string EncodeJsonCMD_SaveMaxBackLight(int nBackLight)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_OneIntNum("WB", "autotest.BackLight", "saveMaxBackLightValue", nBackLight);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SaveMaxBackLight(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.BackLight.saveMaxBackLightValue", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }

        }
        // ----------LV----------



        // ----------Other---------- 

        public static string EncodeJsonCMD_GetSystemInfo()
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD("WB", "autotest.Others", "getOriginalProductModel");
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static string DecodeJsonCMD_GetSystemInfo(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Others.getOriginalProductModel", "WB");
                if (paramsJson != "false")
                {
                    return paramsJson;
                }
                return string.Empty;
            }
            catch (System.Exception ex)
            {
                return string.Empty;
            }
        }
        public static string EncodeJsonCMD_SetMode(string methodname)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD("WB", "autotest.SetMode", methodname);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SetMode(string jsonText, string methodname)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.SetMode." + methodname, "WB");
                if (paramsJson != "false")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        // ----------EDID----------
        public static string EncodeJsonCMD_SetEDID_string(string MethodName, string sSN)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_String("WB", "autotest.EDID", MethodName, sSN);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SetEDID_string(string ClassMethodName, string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, ClassMethodName, "WB");
                if (paramsJson != "false")
                {
                    return true;
                }
                return false ;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        public static string EncodeJsonCMD_SeGamut(string MethodName, int [] nb)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_NubleIntNum("WB", "autotest.Gamut", MethodName, nb);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SeGamut(string ClassMethodName, string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, ClassMethodName, "WB");
                if (paramsJson != "false")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        public static string EncodeJsonCMD_SetSN_Display(int  sSND)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_OneIntNum("WB", "autotest.Others", "setSNDisplay", sSND);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SetSN_Display(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Others.setSNDisplay", "WB");
                if (paramsJson != "false")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        public static string EncodeJsonCMD_SetOthers(int sSND,string method)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_OneIntNum("WB", "autotest.Others", method, sSND);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SetOthers(string jsonText, string method)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Others."+ method, "WB");
                if (paramsJson != "false")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        public static string EncodeJsonCMD_SetLanguage(int sSND)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_OneIntNum("WB", "autotest.Others", "setLangaugeValue", sSND);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SetLanguage(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.Others.setLangaugeValue", "WB");
                if (paramsJson != "false")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        public static string EncodeJsonCMD_SetEDID_PRODUCT_CODE(int  scode)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_OneIntNum("WB", "autotest.EDID", "saveProductCode", scode);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SetEDID_PRODUCT_CODE(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.EDID.saveProductCode", "WB");
                if (paramsJson != "false")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        public static string EncodeJsonCMD_SetEDID_DATE(int year,int week)
        {
            string jsonText = string.Empty;
            try
            {
                jsonText = HkcJson.EncodeJsonCMD_TwoIntNum("WB", "autotest.EDID", "saveDate", year, week);
            }
            catch (System.Exception ex)
            {
                jsonText = string.Empty;
            }
            return jsonText;
        }

        public static bool DecodeJsonCMD_SetEDID_DATE(string jsonText)
        {
            string paramsJson = string.Empty;
            try
            {
                paramsJson = HkcJson.DecodeJsonCMD(jsonText, "autotest.EDID.saveDate", "WB");
                if (paramsJson == "true")
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }

        }
    }
}
