using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace WhiteBalanceCorrection
{
    public enum LogLevel
    {
        DEBUG = 0,
        INFO = 1,
        WARN = 2,
        ERROR = 3,
        FATAL = 4
    };

    public static class LogHelper
    {
        private static readonly string DirectoryName = Directory.GetCurrentDirectory() + "\\Log\\txt_log";
        private static readonly string CorrectLogsDirectoryName = Directory.GetCurrentDirectory() + "\\Log\\csv_log";
        private const string Extension = ".txt";
        private static readonly string LogFileName = Path.Combine(DirectoryName, DateTime.Now.ToString("yyyy-MM-dd") + Extension);
        private static readonly string CorrectLogFileName = Path.Combine(CorrectLogsDirectoryName, DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
        private static string _logBuffer;

        static LogHelper()
        {
            try
            {
                if (!Directory.Exists(DirectoryName))
                {
                    Directory.CreateDirectory(DirectoryName);
                }
                if (!Directory.Exists(CorrectLogsDirectoryName))
                {
                    Directory.CreateDirectory(CorrectLogsDirectoryName);
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("创建日志文件夹失败");
                throw;
            }
        }

        private static string FormatContextString(string sContent, LogLevel logLevel)
        {
            string sFormatContent = "[" + DateTime.Now.ToLongTimeString() + "]";
            switch (logLevel)
            {
                case LogLevel.DEBUG:
                    sFormatContent += "[DEBUG]";
                    break;
                case LogLevel.INFO:
                    sFormatContent += "[INFO]";
                    break;
                case LogLevel.WARN:
                    sFormatContent += "[WARN]";
                    break;
                case LogLevel.ERROR:
                    sFormatContent += "[ERROR]";
                    break;
                case LogLevel.FATAL:
                    sFormatContent += "[FATAL]";
                    break;
                default:
                    sFormatContent += "[INFO]";
                    break;
            }
            sFormatContent += sContent + Environment.NewLine;
            return sFormatContent;
        }

        public static void WriteToLog(string sContent = "", LogLevel logLevel = LogLevel.INFO)
        {
            sContent = FormatContextString(sContent, logLevel);
            Console.WriteLine(sContent);
            WriteStringToFile(LogFileName, sContent);
        }

        public static void WriteToCorrectLog(string content = "", bool addNewLineAtEnd = false, bool addNewLineAtBegin = false)
        {
            content = content + Environment.NewLine;
            if (!File.Exists(CorrectLogFileName))
            {
                try
                {
                    var file = new FileStream(CorrectLogFileName, FileMode.Create);
                    var contextByte =
                        Encoding.Default.GetBytes(
                            "DT,NO,Sx,Sy,CT,O-Lv,Lv,ML,S-R,S-G,S-B,S-Sx,S-Sy,S-CT,S-Lv,C-R,C-G,C-B,C-Sx,C-Sy,C-CT,C-Lv,W-R,W-G,W-B,W-Sx,W-Sy,W-CT,W-Lv,Ts" + Environment.NewLine);
                    file.Write(contextByte, 0, contextByte.Length);
                    file.Flush();
                    file.Close();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            WriteStringToFile(CorrectLogFileName, content);
        }
        
        public static void WriteToBuffer(string sContent = "", LogLevel logLevel = LogLevel.INFO)
        {
            sContent = FormatContextString(sContent, logLevel);
            Console.WriteLine(sContent);
            _logBuffer += sContent;
        }

        public static void WriteBufferToLogFile()
        {
            WriteStringToFile(LogFileName, _logBuffer);
            _logBuffer = string.Empty;
        }

        private static void WriteStringToFile(string fileName, string content)
        {
            try
            {
                var file = new FileStream(fileName, FileMode.Append);
                var contextByte = Encoding.Default.GetBytes(content);
                file.Write(contextByte, 0, contextByte.Length);
                file.Flush();
                file.Close();
            }
            catch (Exception)
            {
                return;
            }
        }

        public static bool OpenCorrectLogsFolder()
        {
            if (!Directory.Exists(CorrectLogsDirectoryName))
            {
                System.Windows.Forms.MessageBox.Show("无法打开日志文件夹");
                return false;
            }
            System.Diagnostics.Process.Start("Explorer.exe", CorrectLogsDirectoryName);
            return true;
        }
    }

    public class RecordResult
    {
        /// <summary>
        /// 日期
        /// </summary>
        public string DT { get; set; }
        /// <summary>
        /// 机器序列号
        /// </summary>
        public string NO { get; set; }
        /// <summary>
        /// 亮度调整前的Sx
        /// </summary>
        public string Sx { get; set; }
        /// <summary>
        /// 亮度调整前的Sy
        /// </summary>
        public string Sy { get; set; }
        /// <summary>
        /// 亮度调整前的色温
        /// </summary>
        public string CT { get; set; }
        /// <summary>
        /// 亮度调整前的亮度
        /// </summary>
        public string OLv { get; set; }
        /// <summary>
        /// 亮度调整后的亮度
        /// </summary>
        public string Lv { get; set; }
        /// <summary>
        /// 亮度调整后的MaxLight
        /// </summary>
        public string ML { get; set; }
        /// <summary>
        /// 标着色温下校正后的R
        /// </summary>
        public string S_R { get; set; }
        /// <summary>
        /// 标着色温下校正后的G
        /// </summary>
        public string S_G { get; set; }
        /// <summary>
        /// 标着色温下校正后的B
        /// </summary>
        public string S_B { get; set; }
        /// <summary>
        /// 标着色温下校正后的Sx
        /// </summary>
        public string S_Sx { get; set; }
        /// <summary>
        /// 标着色温下校正后的Sy
        /// </summary>
        public string S_Sy { get; set; }
        /// <summary>
        /// 标着色温下校正后的色温
        /// </summary>
        public string S_CT { get; set; }
        /// <summary>
        /// 标着色温下校正后的亮度
        /// </summary>
        public string S_Lv { get; set; }
        public string C_R { get; set; }
        public string C_G { get; set; }
        public string C_B { get; set; }
        public string C_Sx { get; set; }
        public string C_Sy { get; set; }
        public string C_CT { get; set; }
        public string C_Lv { get; set; }
        public string W_R { get; set; }
        public string W_G { get; set; }
        public string W_B { get; set; }
        public string W_Sx { get; set; }
        public string W_Sy { get; set; }
        public string W_CT { get; set; }
        public string W_Lv { get; set; }
        /// <summary>
        /// 总耗时
        /// </summary>
        public string Ts { get; set; }
        public string Result { get; set; }
    }
}
