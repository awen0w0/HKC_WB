using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
namespace WhiteBalanceCorrection
{
    public static class ColorProcess
    {
        static Formpattern patternf = new Formpattern();
        public struct ColorItem
        {
            public ColorSpace.CIEXYZ XYZ;
            public double ColorTemputureK;
        }

        public static IHkcCommunication HkcCommunication
        {
            get;
            set;
        }

        public static IDisplayColorAnalyzer DisplayColorAnalyzer
        {
            get;
            set;
        }

        public static void Colorteststart()
        {
            patternf.Show();
            patternf.WindowState = FormWindowState.Normal;
            patternf.BackColor = Color.FromArgb(255, 255, 255);
            Application.DoEvents();
            patternf.Refresh();

        }
        public static bool ColortestPattern(ColorSpace.RGB rgb, int nReceiveTimeout)
        {

            if ((int)rgb.R < 0) rgb.R = 0;
            if ((int)rgb.G < 0) rgb.G = 0;
            if ((int)rgb.B < 0) rgb.B = 0;
            patternf.Show();
            Panel panel = patternf.panel1;
            patternf.WindowState = FormWindowState.Normal;
            patternf.BackColor = Color.FromArgb((int)rgb.R, (int)rgb.G, (int)rgb.B);
            panel.BackColor = Color.FromArgb((int)rgb.R, (int)rgb.G, (int)rgb.B);
            Application.DoEvents();
            patternf.Refresh();
            Thread.Sleep(100);
            return true;
        }
        public static ColorSpace.RGB[] GetTestRGBArray()
        {
            int gScope = 255 - 250 + 1;
            int bScope = 255 - 225 + 1;

            var rgb = new ColorSpace.RGB[gScope * bScope];

            int num = 0;
            for (int i = 0; i < gScope; ++i)
            {
                for (int j = 0; j < bScope; ++j)
                {
                    int index = i * gScope + j;

                    rgb[num].R = 255;
                    rgb[num].G = 250 + i;
                    rgb[num].B = 225 + j;
                    num++;
                }
            }

            return rgb;
        }

        public static ColorSpace.RGB[] GetRGBArrayFromCSVFile(string filepath)
        {
            string[] lines = File.ReadAllLines(filepath);
            var rgbs = new ColorSpace.RGB[lines.Length];

            for (int i = 0; i < lines.Length; ++i)
            {
                string[] values = lines[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                rgbs[i].R = int.Parse(values[0]);
                rgbs[i].G = int.Parse(values[1]);
                rgbs[i].B = int.Parse(values[2]);
            }

            return rgbs;
        }

        public static byte[] RGBArrayToBinary(ColorSpace.RGB[] rgbs)
        {
            //定义数据大小，两行数据转成9字节，注意奇数行补0，0，0
            byte[] result = new byte[(rgbs.Length / 2 + rgbs.Length % 2) * 9];
            int index = 0;

            int[] data = new int[6] { 0, 0, 0, 0, 0, 0 };
            for (int i = 0, j = 0; j < rgbs.Length; ++j)
            {
                data[i++] = (int)rgbs[j].R;
                data[i++] = (int)rgbs[j].G;
                data[i++] = (int)rgbs[j].B;

                if (i == 6 || j == rgbs.Length - 1)
                {
                    if (i == 3)
                    {
                        data[3] = 0;
                        data[4] = 0;
                        data[5] = 0;
                    }

                    for (i = 0; i < 6; i += 2)
                    {
                        //两个数据用三个字节，6个数据用9个字节
                        int combine = data[i] | (data[i + 1] << 12);
                        result[index++] = (byte)(combine & 0xff);
                        result[index++] = (byte)((combine >> 8) & 0xff);
                        result[index++] = (byte)((combine >> 16) & 0xff);
                    }

                    i = 0;
                }
            }

            return result;
        }

        public static ColorSpace.RGB[] GetSingleChannelRGBArray(uint step)
        {
            ColorSpace.RGB[] result = new ColorSpace.RGB[(255 / step) * 3 + 3 + (255 % step == 0 ? 0 : 3)];
            int index1 = 0, index2 = result.Length / 3, index3 = index2 * 2;

            uint i;
            for (i = 0; ; i += step)
            {
                if (i > 255)
                {
                    i = 255;
                }

                result[index1].R = i;
                result[index1].G = 0;
                result[index1].B = 0;
                ++index1;

                result[index2].R = 0;
                result[index2].G = i;
                result[index2].B = 0;
                ++index2;

                result[index3].R = 0;
                result[index3].G = 0;
                result[index3].B = i;
                ++index3;

                if (i == 255)
                {
                    break;
                }
            }

            return result;
        }



        public static ColorItem SetPatternAndMeasure(ColorSpace.RGB rgb, int interval)
        {
            HkcCommunication.ColorSetPattern(rgb);
            Thread.Sleep(interval);
            var p = DisplayColorAnalyzer.Measure();

            ColorItem item;
            item.XYZ.X = p.X;
            item.XYZ.Y = p.Y;
            item.XYZ.Z = p.Z;
            item.ColorTemputureK = p.T;

            return item;
        }

        private static void SetPatternAndMeasure(ColorSpace.RGB[] rgbs, int interval, ConcurrentQueue<ColorItem> outputs)
        {
            for (int i = 0; i < rgbs.Length; ++i)
            {
                HkcCommunication.ColorSetPattern(rgbs[i]);
                Thread.Sleep(interval);
                var p = DisplayColorAnalyzer.Measure();
                //var p = ColorSpace.ColorSpaceToXYZ(ColorSpace.SpaceType.sRGB, rgbs[i]);
                ColorItem item;
                item.XYZ.X = p.X;
                item.XYZ.Y = p.Y;
                item.XYZ.Z = p.Z;
                item.ColorTemputureK = p.T;
                outputs.Enqueue(item);
            }
        }

        public static IEnumerable<ColorItem> SetPatternAndMeasure(ColorSpace.RGB[] rgbs, int interval)
        {
            var outputs = new ConcurrentQueue<ColorItem>();
            var task = new Task(new Action(() =>
            {
               // SetPatternAndMeasure(rgbs, interval, outputs);
            }));
            task.Start();

            for (int i = 0; i < rgbs.Length; ++i)
            {
                ColorItem item;
                while (outputs.TryDequeue(out item) == false)
                {
                    Thread.Yield();
                }
                yield return item;
            }
        }

        public static ColorItem[] SetPatternAndMeasureAll(ColorSpace.RGB[] rgbs, int interval)
        {
            var results = new ColorItem[rgbs.Length];

            for (int i = 0; i < rgbs.Length; ++i)
            {
                HkcCommunication.ColorSetPattern(rgbs[i]);
              //  ColortestPattern(rgbs[i],1000);
                Thread.Sleep(interval);
                var p = DisplayColorAnalyzer.Measure();
                results[i].XYZ.X = p.X;
                results[i].XYZ.Y = p.Y;
                results[i].XYZ.Z = p.Z;
                results[i].ColorTemputureK = p.T;
            }

            return results;
        }

        public static ProbeStruct Measure()
        {
            ProbeStruct Probe = DisplayColorAnalyzer.MeasureEx();
            return Probe;
        }

        public static ColorSpace.RGB[] CalTargetRGB(int point, ColorSpace.RGB[] targetRgbs, ColorSpace.RGB[] rgbs)
        {
            //形参targetRgbs和rgbs都是归一化的参数
            var result = new ColorSpace.RGB[rgbs.Length];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i].R = rgbs[i].R * targetRgbs[0].R;
                result[i].G = rgbs[i].G * targetRgbs[0].G;
                result[i].B = rgbs[i].B * targetRgbs[0].B;
            }
            return result;
        }

        private static double[] n1 = new double[4];
        private static double[] n2 = new double[4];
        private static double[] n3 = new double[4];
        private static double[] n1_standard = new double[4];
        private static double[] n2_standard = new double[4];
        private static double[] n3_standard = new double[4];

        public static void CalPlanePoint(ColorSpace.RGB white, ColorSpace.RGB rg, ColorSpace.RGB rb, ColorSpace.RGB gb)
        {
            //
            //ColorSpace.RGB result = originRGB;


            #region 求校正的三个平面的方程
            //空间平面方程采用点法式方程
            double[] M1 = new double[3];
            double[] M2 = new double[3];
            double[] M3 = new double[3];

            //计算white rg rb三点的平面
            M1[0] = white.R;
            M1[1] = white.G;
            M1[2] = white.B;
            M2[0] = rg.R;
            M2[1] = rg.G;
            M2[2] = rg.B;
            M3[0] = rb.R;
            M3[1] = rb.G;
            M3[2] = rb.B;
            n1 = CalPlaneEquation(M1, M2, M3);

            //计算white rg gb三点的平面
            M3[0] = gb.R;
            M3[1] = gb.G;
            M3[2] = gb.B;
            n2 = CalPlaneEquation(M1, M2, M3);

            //计算white gb rb三点的平面
            M2[0] = rb.R;
            M2[1] = rb.G;
            M2[2] = rb.B;
            n3 = CalPlaneEquation(M1, M2, M3);

            #endregion
            #region 求标准立方体的三个平面方程
            //double[] n1_standard = new double[4];
            //double[] n2_standard = new double[4];
            //double[] n3_standard = new double[4];
            for (int i = 0; i < 4; i++)
            {
                n1_standard[i] = 0;
                n2_standard[i] = 0;
                n3_standard[i] = 0;
            }
            n1_standard[0] = 255;
            n2_standard[1] = 255;
            n3_standard[2] = 255;
            #endregion


            //return result;

        }

        public static ColorSpace.RGB CalTargetRGB222(ColorSpace.RGB originRGB)
        {
            ColorSpace.RGB result = originRGB;
            #region 求RGB三者最大值,确定平面
            int maxIndex = 0; //初始化R最大
            double R, G, B;
            R = originRGB.R;
            G = originRGB.G;
            B = originRGB.B;
            if (G > R)
            {
                if (B > G)
                {
                    maxIndex = 2;
                }
                else
                {
                    maxIndex = 1;
                }

            }
            else
            {
                if (B > G)
                {
                    maxIndex = 2;
                }
            }
            #endregion

            #region originRGB到平面距离
            double[] point1 = new double[3];
            double[] point2 = new double[3];
            point1[0] = 0;
            point1[1] = 0;
            point1[2] = 0;
            point2[0] = originRGB.R;
            point2[1] = originRGB.G;
            point2[2] = originRGB.B;
            double[] Point = new double[3];
            double[] Point_standard = new double[3];
            switch (maxIndex)
            {
                case 0:
                    Point = CalLineAndPlaneCrossoverPoint(point1, point2, n1);
                    Point_standard = CalLineAndPlaneCrossoverPoint(point1, point2, n1_standard);
                    break;
                case 1:
                    Point = CalLineAndPlaneCrossoverPoint(point1, point2, n2);
                    Point_standard = CalLineAndPlaneCrossoverPoint(point1, point2, n2_standard);
                    break;
                case 2:
                    Point = CalLineAndPlaneCrossoverPoint(point1, point2, n3);
                    Point_standard = CalLineAndPlaneCrossoverPoint(point1, point2, n3_standard);
                    break;
                default:
                    break;
            }
            double distance = Math.Sqrt(Math.Pow(Point[0], 2) + Math.Pow(Point[1], 2) + Math.Pow(Point[2], 2));
            double distance_standard = Math.Sqrt(Math.Pow(Point_standard[0], 2) + Math.Pow(Point_standard[1], 2) + Math.Pow(Point_standard[2], 2));
            result.R = Point[0] * distance / distance_standard;
            result.G = Point[1] * distance / distance_standard;
            result.B = Point[2] * distance / distance_standard;
            #endregion
            return result;

        }
        public static double[] CalPlaneEquation(double[] M1, double[] M2, double[] M3)
        {
            //空间平面方程采用点法式方程
            double[] n = new double[4];
            double[] M1M2 = new double[3];
            double[] M1M3 = new double[3];
            //先求法向量
            M1M2[0] = M2[0] - M1[0];
            M1M2[1] = M2[1] - M1[1];
            M1M2[2] = M2[1] - M1[2];

            M1M3[0] = M3[1] - M1[0];
            M1M3[1] = M3[1] - M1[1];
            M1M3[2] = M3[1] - M1[2];

            n[0] = M1M2[1] * M1M3[2] - M1M2[2] * M1M3[1];
            n[1] = M1M2[2] * M1M3[0] - M1M2[0] * M1M3[2];
            n[2] = M1M2[0] * M1M3[1] - M1M2[1] * M1M3[0];

            n[3] = -n[0] * M1[0] - n[1] * M1[1] - n[2] * M1[2];
            return n;
        }

        public static double[] CalLineAndPlaneCrossoverPoint(double[] point1, double[] point2, double[] planePara)
        {
            //空间直线两点式（x - x1）/ (x2 - x1) = （y - y1）/ (y2 - y1) = （z - z1）/ (z2 - z1)
            double[] result = new double[3];
            double x1 = point1[0];
            double y1 = point1[1];
            double z1 = point1[2];
            double x2 = point2[0];
            double y2 = point2[1];
            double z2 = point2[2];
            double A = planePara[0];
            double B = planePara[1];
            double C = planePara[2];
            double D = planePara[3];
            double x, y, z;
            double divisor, dividend;
            if (x2 - x1 == 0 || y2 - y1 == 0 || z2 - z1 == 0)
            {
                if (x2 - x1 == 0)
                {
                    x = x1;
                    if (y2 - y1 == 0)
                    {
                        y = y1;
                        if (z2 - z1 == 0)
                        {
                            z = z1;
                        }
                        else
                        {
                            z = (-A * x - B * y - D) / C;
                        }
                    }
                    else
                    {
                        if (z2 - z1 == 0)
                        {
                            z = z1;
                            y = (-A * x - C * z - D) / B;
                        }
                        else
                        {
                            divisor = B * (y2 - y1) + C * (z2 - z1);
                            dividend = -A * x1 * (y2 - y1) + C * y1 * (z2 - z1) - (C * z1 + D) * (y2 - y1);
                            y = dividend / divisor;
                            z = (y - y1) * (z2 - z1) / (y2 - y1) + z1;
                        }
                    }
                }
                if (y2 - y1 == 0)
                {
                    y = y1;
                    if (x2 - x1 == 0)
                    {
                        x = x1;
                        if (z2 - z1 == 0)
                        {
                            z = z1;
                        }
                        else
                        {
                            z = (-A * x - B * y - D) / C;
                        }
                    }
                    else
                    {
                        if (z2 - z1 == 0)
                        {
                            z = z1;
                            x = (-B * y - C * z - D) / A;

                        }
                        else
                        {
                            divisor = A * (x2 - x1) + C * (z2 - z1);
                            dividend = -B * y1 * (x2 - x1) + C * x1 * (z2 - z1) - (C * z1 + D) * (x2 - x1);
                            x = dividend / divisor;
                            z = (x - x1) * (z2 - z1) / (x2 - x1) + z1;
                        }
                    }
                }
                if (z2 - z1 == 0)
                {
                    z = z1;
                    if (x2 - x1 == 0)
                    {
                        x = x1;
                        if (y2 - y1 == 0)
                        {
                            y = y1;
                        }
                        else
                        {
                            y = (-A * x - C * z - D) / C;
                        }
                    }
                    else
                    {
                        if (y2 - y1 == 0)
                        {
                            y = y1;
                            x = (-B * y - C * z - D) / A;

                        }
                        else
                        {
                            divisor = A * (x2 - x1) + B * (y2 - y1);
                            dividend = B * x1 * (y2 - y1) - B * y1 * (x2 - x1) - (C * z1 + D) * (x2 - x1);
                            x = dividend / divisor;
                            y = (x - x1) * (y2 - y1) / (x2 - x1) + y1;
                        }
                    }
                }
            }
            divisor = A * (x2 - x1) + B * (y2 - y1) + C * (z2 - z1);
            dividend = -(B * y1 + C * z1 + D) * (x2 - x1) + x1 * (B * (y2 - y1) + C * (z2 - z1));
            x = dividend / divisor;
            y = (x - x1) * (y2 - y1) / (x2 - x1) + y1;
            z = (x - x1) * (z2 - z1) / (x2 - x1) + z1;

            return result;
        }



        public static void creat_3D_lut(string lutCSVFilePath, int originR, int originG, int originB)
        {
            //生成3d_lut文件和bin文件
            // 由于在ReadConfigFile_Color方法里做了参数的验证，所以一定会进入到这里

            // 计算3dlut.bin
            // 放在这里是为了尽可能的利用线程资源，缩短测试时间
            // 还未对比测试过

            var base64Bin = string.Empty; // bin文件内容转base64字符串
            var lutRGBs = ColorProcess.GetRGBArrayFromCSVFile(lutCSVFilePath);
            var normalizedLutRGBs = (ColorSpace.RGB[])lutRGBs.Clone();
            ColorSpace.RGBNormalize(normalizedLutRGBs, 1023);
            ColorSpace.RGB[] targetRGB = new ColorSpace.RGB[64];
            targetRGB[0].R = originR / 255.0;
            targetRGB[0].G = originG / 255.0;
            targetRGB[0].B = originB / 255.0;
            ColorSpace.RGB[] newLutRGBs = null;
            newLutRGBs = ColorProcess.CalTargetRGB(1, targetRGB, normalizedLutRGBs);
            ColorSpace.RGBReNormalize(newLutRGBs, 1023);
            var singleBin = ColorProcess.RGBArrayToBinary(newLutRGBs);
            base64Bin = Convert.ToBase64String(singleBin);
            #region LOG
            try
            {
                StringBuilder csvString = new StringBuilder();

                using (var CSVLog = new StreamWriter($"./Log/csv_log/{DateTime.Now:MM-dd-HH-mm}.csv", false))
                {
                    for (int index = 0; index < newLutRGBs.Length; ++index)
                    {
                        csvString.Append($"{newLutRGBs[index]}\r\n");
                    }
                    CSVLog.Write(csvString.ToString());
                    CSVLog.Close();
                }
            }
            catch (Exception info)
            {
                MessageBox.Show($"无法写入CSV日志：{info.Message}");
            }
            try
            {
                using (var TXTLog = new StreamWriter($"./Log/csv_log/{DateTime.Now:MM-dd-HH-mm}.txt", false))
                {
                    TXTLog.Write(base64Bin);
                    TXTLog.Close();
                    System.Windows.Forms.MessageBox.Show("校正完成，3D_lut文件生成完毕");

                }
            }
            catch (Exception info)
            {
                MessageBox.Show($"无法写入TXT日志：{info.Message}");
            }
            #endregion

        }


    }
}
