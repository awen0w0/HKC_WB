using System;

namespace WhiteBalanceCorrection
{
    public class ColorSpace
    {

        public enum SpaceType
        {
            sRGB = 0,
            AdobeRGB,
            Display_P3,
            DCI_P3,
            BT_2020
        };
        public struct ALL
        {
            public float R;
            public float G;
            public float B;

            public float sx;
            public float sy;
            public float Lv;
            public float T;

            public double X;
            public double Y;
            public double Z;
            public float du;
            public float dv;

        };
        public struct RGB
        {
            public double R;
            public double G;
            public double B;

            public enum Type
            {
                R = 0,
                G,
                B,
            }

            public override string ToString() => $"{R},{G},{B}";

            public void Display() => Console.WriteLine(ToString());

        };

        public struct CIEXYZ
        {
            public double X;
            public double Y;
            public double Z;
            public override string ToString() => $"{X},{Y},{Z}";

            public void Display() => Console.WriteLine(ToString());
        };

        public struct CIELAB
        {
            public double L;
            public double A;
            public double B;
        };

        public struct Lvxy
        {
            public double Lv;
            public double x;
            public double y;
        };
        public struct CIExy
        {
    
            public double x;
            public double y;
        };

        public enum ColorTemperature
        {
            D45 = 0,
            D65,
            D93,
            D120
        };

        public enum ChromaticAdaptation
        {
            D65TD93 = 0,
            D65TD120,
            D93TD65,
            D93TD120,
            D120TD65,
            D120TD93
        };

        public enum Interpolation
        {
            linear = 0,
            PowerExponent,
            Hermite,
        };

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

        public struct POINT
        {
            public double x;
            public double y;
        };

        #region static method

        private static double ToLinear(SpaceType type, double value)
        {
            switch (type)
            {
                case SpaceType.sRGB:
                case SpaceType.Display_P3:
                    if (value <= 0.04045)
                    {
                        return value / 12.92;
                    }
                    else
                    {
                        return Math.Pow((value + 0.055) / 1.055, 2.4);
                    }
                case SpaceType.DCI_P3:
                    if (value <= 0.0)
                        return 0.0;
                    else
                        return Math.Pow(value, 2.2);
                case SpaceType.AdobeRGB:
                    if (value <= 0.0)
                    {
                        return 0.0;
                    }
                    else
                    {
                        return Math.Pow(value, 2.19921875);
                    }
                /*case SpaceType.BT_2020:
                    double m1 = 0.1593017578125;
                    double m2 = 78.84375;
                    double c2 = 18.8515625;
                    double c3 = 18.6875;
                    double c1 = c3 - c2 + 1;
                    double max = Math.Pow(value, 1 / m2) - c1 > 0 ? Math.Pow(value, 1 / m2) - c1 : 0;
                    return Math.Pow(max / (c2 - c3 * Math.Pow(value, 1 / m2)), 1 / m1);*/
    
                case SpaceType.BT_2020:
                    if (value <= 0.04045)
                    {
                        return value / 4.5;
                   }
                    else
                    {
                       return Math.Pow((value + 0.0993) / 1.0993, 2.4);
                    }
                default:
                    return 0.0;
            }

        }

        private static double ToGamma(SpaceType type, double value)
        {
            switch (type)
            {
                case SpaceType.sRGB:
                case SpaceType.Display_P3:
                    if (value <= 0.0031308)
                    {
                        return 12.92 * value;
                    }
                    else
                    {
                        return 1.055 * Math.Pow(value, 1 / 2.4) - 0.055;
                    }
                case SpaceType.DCI_P3:
                    if (value <= 0.0)
                        return 0.0;
                    return Math.Pow(value, 1 / 2.6);
                case SpaceType.AdobeRGB:
                    if (value <= 0.0)
                    {
                        return 0.0;
                    }
                    else
                    {
                        return Math.Pow(value, 1 / 2.19921875);
                    }
                /*case SpaceType.BT_2020:
                    if (value <= 0.0)
                    {
                        return 0.0;
                    }
                    else
                    {
                        double m1 = 0.1593017578125;
                        double m2 = 78.84375;
                        double c2 = 18.8515625;
                        double c3 = 18.6875;
                        double c1 = c3 - c2 + 1;
                        double ret = Math.Pow((c2 * Math.Pow(value, m1) + c1) / (1 + c3 * Math.Pow(value, m1)), m2);
                        return ret;
                    }*/
                case SpaceType.BT_2020:
                    if (value <= 0.018054)
                    {
                        return 4.5 * value;
                    }
                    else
                    {
                        return 1.0993 * Math.Pow(value, 1 / 2.4) - 0.0993;
                    }
                default:
                    return 0.0;
            }
        }

        private static double LabFunction(double value)
        {
            if (value > Math.Pow((6 / 29.0), 3))
            {
                return Math.Pow(value, 1 / 3.0);
            }
            else
            {
                return 1 / 3.0 * Math.Pow(29 / 6.0, 2) * value + 16 / 116.0;
            }
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

            if (Math.Abs(fabsA) < m_min2Zero)
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

        private static double[] Cal4MatrixInv(double[] num)
        {
            double a11, a12, a13, a14, a21, a22, a23, a24, a31, a32, a33, a34, a41, a42, a43, a44;
            a11 = num[0];
            a12 = num[1];
            a13 = num[2];
            a14 = num[3];
            a21 = num[4];
            a22 = num[5];
            a23 = num[6];
            a24 = num[7];
            a31 = num[8];
            a32 = num[9];
            a33 = num[10];
            a34 = num[11];
            a41 = num[12];
            a42 = num[13];
            a43 = num[14];
            a44 = num[15];

            double fabsA = a11 * a22 * a33 * a44 - a11 * a22 * a34 * a43 - a11 * a23 * a32 * a44 + a11 * a23 * a34 * a42
                        + a11 * a24 * a32 * a43 - a11 * a24 * a33 * a42 - a12 * a21 * a33 * a44 + a12 * a21 * a34 * a43
                        + a12 * a23 * a31 * a44 - a12 * a23 * a34 * a41 - a12 * a24 * a31 * a43 + a12 * a24 * a33 * a41
                        + a13 * a21 * a32 * a44 - a13 * a21 * a34 * a42 - a13 * a22 * a31 * a44 + a13 * a22 * a34 * a41
                        + a13 * a24 * a31 * a42 - a13 * a24 * a32 * a41 - a14 * a21 * a32 * a43 + a14 * a21 * a33 * a42
                        + a14 * a22 * a31 * a43 - a14 * a22 * a33 * a41 - a14 * a23 * a31 * a42 + a14 * a23 * a32 * a41;
            //代数余子式
            double m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44;
            m11 = Math.Pow(-1, 2) * (a22 * a33 * a44 + a23 * a34 * a42 + a24 * a32 * a43
                    - a24 * a33 * a42 - a23 * a32 * a44 - a22 * a34 * a43);
            m12 = Math.Pow(-1, 3) * (a21 * a33 * a44 + a23 * a34 * a41 + a24 * a31 * a43
                    - a24 * a33 * a41 - a23 * a31 * a44 - a21 * a34 * a43);
            m13 = Math.Pow(-1, 4) * (a21 * a32 * a44 + a22 * a34 * a41 + a24 * a31 * a42
                    - a24 * a32 * a41 - a22 * a31 * a44 - a21 * a34 * a42);
            m14 = Math.Pow(-1, 5) * (a21 * a32 * a43 + a22 * a33 * a41 + a23 * a31 * a42
                     - a23 * a32 * a41 - a22 * a31 * a43 - a21 * a33 * a42);

            m21 = Math.Pow(-1, 3) * (a12 * a33 * a44 + a13 * a34 * a42 + a14 * a32 * a43
                     - a14 * a33 * a42 - a13 * a32 * a44 - a12 * a34 * a43);
            m22 = Math.Pow(-1, 4) * (a11 * a33 * a44 + a13 * a34 * a41 + a14 * a31 * a43
                     - a14 * a33 * a41 - a13 * a31 * a44 - a11 * a34 * a43);
            m23 = Math.Pow(-1, 5) * (a11 * a32 * a44 + a12 * a34 * a41 + a14 * a31 * a42
                      - a14 * a32 * a41 - a12 * a31 * a44 - a11 * a34 * a42);
            m24 = Math.Pow(-1, 6) * (a11 * a32 * a43 + a12 * a33 * a41 + a13 * a31 * a42
                      - a13 * a32 * a41 - a12 * a31 * a43 - a11 * a33 * a42);

            m31 = Math.Pow(-1, 4) * (a12 * a23 * a44 + a13 * a24 * a42 + a14 * a22 * a43
                     - a14 * a23 * a42 - a13 * a22 * a44 - a12 * a24 * a43);
            m32 = Math.Pow(-1, 5) * (a11 * a23 * a44 + a13 * a24 * a41 + a14 * a21 * a43
                     - a14 * a23 * a41 - a13 * a21 * a44 - a11 * a24 * a43);
            m33 = Math.Pow(-1, 6) * (a11 * a22 * a44 + a12 * a24 * a41 + a14 * a21 * a42
                      - a14 * a22 * a41 - a12 * a21 * a44 - a11 * a24 * a42);
            m34 = Math.Pow(-1, 7) * (a11 * a22 * a43 + a12 * a23 * a41 + a13 * a21 * a42
                      - a13 * a22 * a41 - a12 * a21 * a43 - a11 * a23 * a42);

            m41 = Math.Pow(-1, 5) * (a12 * a23 * a34 + a13 * a24 * a32 + a14 * a22 * a33
                     - a14 * a23 * a32 - a13 * a22 * a34 - a12 * a24 * a33);
            m42 = Math.Pow(-1, 6) * (a11 * a23 * a34 + a13 * a24 * a31 + a14 * a21 * a33
                     - a14 * a23 * a31 - a13 * a21 * a34 - a11 * a24 * a33);
            m43 = Math.Pow(-1, 7) * (a11 * a22 * a34 + a12 * a24 * a31 + a14 * a21 * a32
                      - a14 * a22 * a31 - a12 * a21 * a34 - a11 * a24 * a32);
            m44 = Math.Pow(-1, 8) * (a11 * a22 * a33 + a12 * a23 * a31 + a13 * a21 * a32
                      - a13 * a22 * a31 - a12 * a21 * a33 - a11 * a23 * a32);

            if (Math.Abs(fabsA) < m_min2Zero)
            {
                throw new System.Exception(string.Format("fabsA = {0}", fabsA));
            }
            double derA = 1.0 / fabsA;
            double[] result = new double[16];
            result[0] = derA * m11;
            result[1] = derA * m21;
            result[2] = derA * m31;
            result[3] = derA * m41;
            result[4] = derA * m12;
            result[5] = derA * m22;
            result[6] = derA * m32;
            result[7] = derA * m42;
            result[8] = derA * m13;
            result[9] = derA * m23;
            result[10] = derA * m33;
            result[11] = derA * m43;
            result[12] = derA * m14;
            result[13] = derA * m24;
            result[14] = derA * m34;
            result[15] = derA * m44;
            return result;
        }

        private static void EdgeProcessing(double[] maxRGBLight, RGB rgb, ref RGB result)
        {
            var temp = new double[] { result.R, result.G, result.B };
            //比例压缩
            if (temp[0] > 1 || temp[0] < 0 || temp[1] > 1 || temp[1] < 0 || temp[2] > 1 || temp[2] < 0)
            {
                //求最大值
                double maxsum = maxRGBLight[0] + maxRGBLight[1] + maxRGBLight[2];
                double tempMax = 0;

                RGB.Type type = RGB.Type.R;

                if (temp[0] > 1 || temp[0] < 0)
                {
                    tempMax = (temp[0] - rgb.R) * maxRGBLight[0] / maxsum;
                    type = RGB.Type.R;
                }
                if (temp[1] > 1 || temp[1] < 0)
                {
                    if (Math.Abs(tempMax) < Math.Abs((temp[1] - rgb.G) * maxRGBLight[1] / maxsum))
                    {
                        tempMax = (temp[1] - rgb.G) * maxRGBLight[1] / maxsum;
                        type = RGB.Type.G;
                    }
                }
                if (temp[2] > 1 || temp[2] < 0)
                {

                    if (Math.Abs(tempMax) < Math.Abs((temp[2] - rgb.B) * maxRGBLight[2] / maxsum))
                    {
                        tempMax = (temp[2] - rgb.B) * maxRGBLight[2] / maxsum;
                        type = RGB.Type.B;
                    }
                }
                double tempValue;
               /*switch (type)
                {
                    case RGB.Type.R:
                        if (temp[0] > 1)
                        {
                            tempValue = temp[0] - 1;
                            temp[0] = 1;
                            //temp[1] = temp[1] - tempValue * (maxsum - maxRGBLight[1]) / maxsum;
                            //temp[2] = temp[2] - tempValue * (maxsum - maxRGBLight[2]) / maxsum;
                            //temp[1] = temp[1] - maxRGBLight[1] / maxsum * tempValue ;
                            //temp[2] = temp[2] - maxRGBLight[2] / maxsum * tempValue ;
                            //temp[1] = temp[1] - 1.024 / 1.107 * tempValue;
                            //temp[2] = temp[2] - 0.43 / 1.107 * tempValue;
                            temp[1] = temp[1] - tempValue * 1 /50;
                            temp[2] = temp[2] - tempValue * 1 / 50;
                        }
                        else
                        {
                            tempValue = temp[0] - 0;
                            temp[0] = 0;
                            //temp[1] = temp[1] - tempValue * (maxsum - maxRGBLight[1]) / maxsum;
                            //temp[2] = temp[2] - tempValue * (maxsum - maxRGBLight[2]) / maxsum;
                            //temp[1] = temp[1] - 1.024 / 1.107 * tempValue;
                            //temp[2] = temp[2] - 0.43 / 1.107 * tempValue;
                            temp[1] = temp[1] + tempValue * 1 / 50;
                            temp[2] = temp[2] + tempValue * 1 / 50;
                        }
                        break;
                    case RGB.Type.G:
                        if (temp[1] > 1)
                        {
                            tempValue = temp[1] - 1;
                            //temp[0] = temp[0] - tempValue * (maxsum - maxRGBLight[0]) / maxsum;
                            temp[1] = 1;
                            //temp[2] = temp[2] - tempValue * (maxsum - maxRGBLight[2]) / maxsum;
                            //temp[0] = temp[0] - maxRGBLight[0] / maxsum * tempValue ;
                            //temp[1] = 1;
                            //temp[2] = temp[2] - maxRGBLight[2] / maxsum * tempValue ;
                            //temp[0] = temp[0] -  1.107 / 1.024 * tempValue ;
                            // temp[1] = 1;
                            //temp[2] = temp[2] - 0.43 / 1.024 * tempValue ;
                            temp[0] = temp[0] - tempValue * 1 / 50;
                            temp[2] = temp[2] - tempValue * 1 / 50;
                        }
                        else
                        {
                            tempValue = temp[1] - 0;
                            //temp[0] = temp[0] - tempValue * (maxsum - maxRGBLight[0]) / maxsum;
                            temp[1] = 0;
                            //temp[2] = temp[2] - tempValue * (maxsum - maxRGBLight[2]) / maxsum;
                            //temp[0] = temp[0] - maxRGBLight[0] / maxsum * tempValue ;
                            // temp[1] = 0;
                            //temp[2] = temp[2] - maxRGBLight[2] / maxsum * tempValue ;
                            //temp[0] = temp[0] - 1.107 / 1.024 * tempValue;
                            //temp[1] = 1;
                            //temp[2] = temp[2] - 0.43 / 1.024 * tempValue;
                            temp[0] = temp[0] + tempValue * 1 / 50;
                            temp[2] = temp[2] + tempValue * 1 / 50;
                        }
                        break;
                    case RGB.Type.B:
                        if (temp[2] > 1)
                        {
                            tempValue = temp[2] - 1;
                            //temp[0] = temp[0] - tempValue * (maxsum - maxRGBLight[0]) / maxsum;
                            //temp[1] = temp[1] - tempValue * (maxsum - maxRGBLight[1]) / maxsum;
                            temp[2] = 1;
                            //temp[0] = temp[0] - maxRGBLight[0] / maxsum * tempValue;
                            //temp[1] = temp[1] - maxRGBLight[1] / maxsum * tempValue;
                            //temp[2] = 1;
                            //temp[0] = temp[0] - 1.107 / 0.43 * tempValue;
                            //temp[1] = temp[1] - 1.024 / 0.43 * tempValue;
                            //temp[2] = 1;
                            temp[0] = temp[0] - tempValue * 1 / 50;
                            temp[1] = temp[1] - tempValue * 1 / 50;
                        }
                        else
                        {
                            tempValue = temp[2] - 0;
                            double per = rgb.B / (rgb.B + tempValue);
                            //temp[0] = temp[0] - tempValue * (maxsum - maxRGBLight[0]) / maxsum;
                            //temp[1] = temp[1] - tempValue * (maxsum - maxRGBLight[1]) / maxsum;
                            temp[2] = 0;
                            //temp[0] = temp[0] - maxRGBLight[0] / maxsum * tempValue ;
                            //temp[1] = temp[1] - maxRGBLight[1] / maxsum * tempValue ;
                            //temp[2] = 0;
                            //temp[0] = temp[0] - 1.107 / 0.43 * tempValue;
                            //temp[1] = temp[1] - 1.024 / 0.43 * tempValue;
                            // temp[2] = 1;
                            temp[0] = temp[0] + (rgb.R - temp[0]) * (1 - per);
                            temp[1] = temp[1] + (rgb.B - temp[1]) * (1 - per);
                        }
                        break;
                    default:
                        break;
                }*/
            }

            for (int i = 0; i < 3; ++i)
            {
                if (temp[i] < 0)
                {
                    temp[i] = 0;
                }
                else if (temp[i] > 1.0)
                {
                    temp[i] = 1.0;
                }
            }

            result.R = temp[0];
            result.G = temp[1];
            result.B = temp[2];
        }

        public static CIEXYZ ColorSpaceToXYZ(ColorTemperature temperature, SpaceType type, RGB rgb)
        {
            CIEXYZ xyz;
            xyz.X = 0.0;
            xyz.Y = 0.0;
            xyz.Z = 0.0;

            double Rlin, Glin, Blin;
            Rlin = ToLinear(type, rgb.R);
            Glin = ToLinear(type, rgb.G);
            Blin = ToLinear(type, rgb.B);

            switch (temperature)
            {
                case ColorTemperature.D45:
                    switch (type)
                    {
                        case SpaceType.sRGB:
                            xyz.X = Rlin * 0.522840799 + Glin * 0.34313593 + Blin * 0.11034585;
                            xyz.Y = Rlin * 0.269589787 + Glin * 0.68627187 + Blin * 0.04413834;
                            xyz.Z = Rlin * 0.02450816 + Glin * 0.1143786 + Blin * 0.58115482;
                            break;
                        case SpaceType.AdobeRGB:
                            xyz.X = Rlin * 0.68048128 + Glin * 0.1780606 + Blin * 0.11778065;
                            xyz.Y = Rlin * 0.35087316 + Glin * 0.60201457 + Blin * 0.047112261;
                            xyz.Z = Rlin * 0.03189756 + Glin * 0.0678326 + Blin * 0.6203114;
                            break;
                        case SpaceType.Display_P3:
                        case SpaceType.DCI_P3:
                            xyz.X = Rlin * 0.59026175 + Glin * 0.2576514 + Blin * 0.128409398;
                            xyz.Y = Rlin * 0.2777702 + Glin * 0.670866 + Blin * 0.051363759;
                            xyz.Z = Rlin * 0.0 + Glin * 0.04375213 + Blin * 0.6762895;
                            break;

                        case SpaceType.BT_2020:
                            xyz.X = Rlin * 0.7247947 + Glin * 0.141281496 + Blin * 0.11024635;
                            xyz.Y = Rlin * 0.2989266 + Glin * 0.6623609 + Blin * 0.03871246;
                            xyz.Z = Rlin * 0.0 + Glin * 0.02742523 + Blin * 0.692616397;
                            break;
                        default:
                            break;
                    }
                    break;
                case ColorTemperature.D65:
                    switch (type)
                    {
                        case SpaceType.sRGB:
                            xyz.X = Rlin * 0.412391 + Glin * 0.357584 + Blin * 0.180481;
                            xyz.Y = Rlin * 0.212639 + Glin * 0.715169 + Blin * 0.072192;
                            xyz.Z = Rlin * 0.019331 + Glin * 0.119195 + Blin * 0.950532;
                            break;
                        case SpaceType.AdobeRGB:
                            xyz.X = Rlin * 0.576669 + Glin * 0.185558 + Blin * 0.188229;
                            xyz.Y = Rlin * 0.297345 + Glin * 0.627364 + Blin * 0.075291;
                            xyz.Z = Rlin * 0.027031 + Glin * 0.070689 + Blin * 0.991338;
                            break;
                        case SpaceType.Display_P3:
                        case SpaceType.DCI_P3:
                            xyz.X = Rlin * 0.486571 + Glin * 0.265668 + Blin * 0.198217;
                            xyz.Y = Rlin * 0.228975 + Glin * 0.691738 + Blin * 0.079287;
                            xyz.Z = Rlin * 0.0 + Glin * 0.045113 + Blin * 1.043945;
                            break;

                        case SpaceType.BT_2020:
                            xyz.X = Rlin * 0.636958 + Glin * 0.144617 + Blin * 0.168881;
                            xyz.Y = Rlin * 0.262700 + Glin * 0.677998 + Blin * 0.059302;
                            xyz.Z = Rlin * 0.0 + Glin * 0.028072 + Blin * 1.060985;
                            break;
                        default:
                            break;
                    }
                    break;
                case ColorTemperature.D93:
                    switch (type)
                    {
                        case SpaceType.sRGB:
                            xyz.X = Rlin * 0.369898 + Glin * 0.355095 + Blin * 0.247703;
                            xyz.Y = Rlin * 0.190729 + Glin * 0.710190 + Blin * 0.099081;
                            xyz.Z = Rlin * 0.017339 + Glin * 0.118365 + Blin * 1.304569;
                            break;
                        case SpaceType.AdobeRGB:
                            xyz.X = Rlin * 0.533033 + Glin * 0.184267 + Blin * 0.255397;
                            xyz.Y = Rlin * 0.274845 + Glin * 0.622996 + Blin * 0.102159;
                            xyz.Z = Rlin * 0.024986 + Glin * 0.070197 + Blin * 1.345090;
                            break;
                        case SpaceType.Display_P3:
                        case SpaceType.DCI_P3:
                            xyz.X = Rlin * 0.444734 + Glin * 0.262971 + Blin * 0.264991;
                            xyz.Y = Rlin * 0.209287 + Glin * 0.684717 + Blin * 0.105996;
                            xyz.Z = Rlin * 0.0 + Glin * 0.044655 + Blin * 1.395618;
                            break;
                        case SpaceType.BT_2020:
                            xyz.X = Rlin * 0.604596 + Glin * 0.143274 + Blin * 0.224827;
                            xyz.Y = Rlin * 0.249353 + Glin * 0.671700 + Blin * 0.078947;
                            xyz.Z = Rlin * 0.0 + Glin * 0.027812 + Blin * 1.412461;
                            break;
                        default:
                            break;
                    }
                    break;
                case ColorTemperature.D120:
                    switch (type)
                    {
                        case SpaceType.sRGB:
                            xyz.X = Rlin * 0.340855 + Glin * 0.355764 + Blin * 0.281799;
                            xyz.Y = Rlin * 0.175753 + Glin * 0.711527 + Blin * 0.112719;
                            xyz.Z = Rlin * 0.015978 + Glin * 0.118588 + Blin * 1.484140;
                            break;
                        case SpaceType.AdobeRGB:
                            xyz.X = Rlin * 0.504396 + Glin * 0.184613 + Blin * 0.289507;
                            xyz.Y = Rlin * 0.260028 + Glin * 0.624169 + Blin * 0.115803;
                            xyz.Z = Rlin * 0.023639 + Glin * 0.070329 + Blin * 1.524737;
                            break;
                        case SpaceType.Display_P3:
                        case SpaceType.DCI_P3:
                            xyz.X = Rlin * 0.416712 + Glin * 0.262830 + Blin * 0.298875;
                            xyz.Y = Rlin * 0.196100 + Glin * 0.684350 + Blin * 0.119550;
                            xyz.Z = Rlin * 0.0 + Glin * 0.044631 + Blin * 1.574073;
                            break;
                        case SpaceType.BT_2020:
                            xyz.X = Rlin * 0.582055 + Glin * 0.143129 + Blin * 0.253233;
                            xyz.Y = Rlin * 0.240057 + Glin * 0.671022 + Blin * 0.088921;
                            xyz.Z = Rlin * 0.0 + Glin * 0.027784 + Blin * 1.590921;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return xyz;
        }

        public static RGB XYZToColorSpace(ColorTemperature temperature, SpaceType type, CIEXYZ xyz)
        {
            RGB rgb;
            double Rlin = 0.0, Glin = 0.0, Blin = 0.0;

            switch (temperature)
            {
                case ColorTemperature.D45:
                    switch (type)
                    {
                        case SpaceType.sRGB:
                            Rlin = xyz.X * 2.556315779 + xyz.Y * -1.21261133 + xyz.Z * -0.39327935;
                            Glin = xyz.X * -1.0100555 + xyz.Y * 1.954958747 + xyz.Z * 0.0433048;
                            Blin = xyz.X * 0.0909881 + xyz.Y * -0.33362307 + xyz.Z * 1.7287741;
                            break;
                        case SpaceType.AdobeRGB:
                            Rlin = xyz.X * 1.7301292 + xyz.Y * -0.47881115 + xyz.Z * -0.2921401;
                            Glin = xyz.X * -1.0100555 + xyz.Y * 1.9549587 + xyz.Z * 0.0433048;
                            Blin = xyz.X * 0.02148569 + xyz.Y * -0.1891583 + xyz.Z * 1.6223803;
                            break;
                        case SpaceType.Display_P3:
                        case SpaceType.DCI_P3:
                            Rlin = xyz.X * 2.0554663 + xyz.Y * -0.7677682 + xyz.Z * -0.3319669;
                            Glin = xyz.X * -0.85529669 + xyz.Y * 1.8175054689 + xyz.Z * 0.024359716;
                            Blin = xyz.X * 0.05533289 + xyz.Y * -0.11758239 + xyz.Z * 1.477080767;
                            break;
                        case SpaceType.BT_2020:
                            Rlin = xyz.X * 1.50861303 + xyz.Y * -0.3125676 + xyz.Z * -0.2226612;
                            Glin = xyz.X * -0.68242359 + xyz.Y * 1.6546435 + xyz.Z * 0.0161408;
                            Blin = xyz.X * 0.02702163 + xyz.Y * -0.0655182 + xyz.Z * 1.4431615;
                            break;
                        default:
                            break;
                    }
                    break;
                case ColorTemperature.D65:
                    switch (type)
                    {
                        case SpaceType.sRGB:
                            Rlin = xyz.X * 3.240970 + xyz.Y * -1.537383 + xyz.Z * -0.498611;
                            Glin = xyz.X * -0.969244 + xyz.Y * 1.875968 + xyz.Z * 0.041555;
                            Blin = xyz.X * 0.055630 + xyz.Y * -0.203977 + xyz.Z * 1.056971;
                            break;
                        case SpaceType.AdobeRGB:
                            Rlin = xyz.X * 2.041588 + xyz.Y * -0.565007 + xyz.Z * -0.498611;
                            Glin = xyz.X * -0.969244 + xyz.Y * 1.875968 + xyz.Z * 0.041555;
                            Blin = xyz.X * 0.013444 + xyz.Y * -0.118362 + xyz.Z * 1.015175;
                            break;
                        case SpaceType.Display_P3:
                        case SpaceType.DCI_P3:
                            Rlin = xyz.X * 2.493497 + xyz.Y * -0.931384 + xyz.Z * -0.402711;
                            Glin = xyz.X * -0.829489 + xyz.Y * 1.762664 + xyz.Z * 0.023625;
                            Blin = xyz.X * 0.035846 + xyz.Y * -0.076172 + xyz.Z * 0.956884;
                            break;
                        case SpaceType.BT_2020:
                            Rlin = xyz.X * 1.716651 + xyz.Y * -0.355671 + xyz.Z * -0.253366;
                            Glin = xyz.X * -0.666684 + xyz.Y * 1.616481 + xyz.Z * 0.015769;
                            Blin = xyz.X * 0.017639 + xyz.Y * -0.042771 + xyz.Z * 0.942103;
                            break;
                        default:
                            break;
                    }
                    break;
                case ColorTemperature.D93:
                    switch (type)
                    {
                        case SpaceType.sRGB:
                            Rlin = xyz.X * 3.613284 + xyz.Y * -1.713994 + xyz.Z * -0.555890;
                            Glin = xyz.X * -0.976038 + xyz.Y * 1.889118 + xyz.Z * 0.041846;
                            Blin = xyz.X * 0.040533 + xyz.Y * -0.148621 + xyz.Z * 0.770128;
                            break;
                        case SpaceType.AdobeRGB:
                            Rlin = xyz.X * 2.208722 + xyz.Y * -0.611261 + xyz.Z * -0.372953;
                            Glin = xyz.X * -0.976038 + xyz.Y * 1.889118 + xyz.Z * 0.041846;
                            Blin = xyz.X * 0.009908 + xyz.Y * -0.087234 + xyz.Z * 0.748188;
                            break;
                        case SpaceType.Display_P3:
                        case SpaceType.DCI_P3:
                            Rlin = xyz.X * 2.728063 + xyz.Y * -1.019000 + xyz.Z * -0.440594;
                            Glin = xyz.X * -0.837995 + xyz.Y * 1.780740 + xyz.Z * 0.023867;
                            Blin = xyz.X * 0.026813 + xyz.Y * -0.056978 + xyz.Z * 0.715765;
                            break;
                        case SpaceType.BT_2020:
                            Rlin = xyz.X * 1.808539 + xyz.Y * -0.374708 + xyz.Z * -0.266928;
                            Glin = xyz.X * -0.672935 + xyz.Y * 1.631637 + xyz.Z * 0.015916;
                            Blin = xyz.X * 0.013250 + xyz.Y * -0.032128 + xyz.Z * 0.707671;
                            break;
                        default:
                            break;
                    }
                    break;
                case ColorTemperature.D120:
                    switch (type)
                    {
                        case SpaceType.sRGB:
                            Rlin = xyz.X * 3.240970 + xyz.Y * -1.537383 + xyz.Z * -0.498611;
                            Glin = xyz.X * -0.969244 + xyz.Y * 1.875968 + xyz.Z * 0.041555;
                            Blin = xyz.X * 0.055630 + xyz.Y * -0.203977 + xyz.Z * 1.056971;
                            break;
                        case SpaceType.AdobeRGB:
                            Rlin = xyz.X * 2.041588 + xyz.Y * -0.565007 + xyz.Z * -0.498611;
                            Glin = xyz.X * -0.969244 + xyz.Y * 1.875968 + xyz.Z * 0.041555;
                            Blin = xyz.X * 0.013444 + xyz.Y * -0.118362 + xyz.Z * 1.015175;
                            break;
                        case SpaceType.Display_P3:
                        case SpaceType.DCI_P3:
                            Rlin = xyz.X * 2.911514 + xyz.Y * -1.087524 + xyz.Z * -0.470222;
                            Glin = xyz.X * -0.838444 + xyz.Y * 1.781694 + xyz.Z * 0.023880;
                            Blin = xyz.X * 0.023773 + xyz.Y * -0.050518 + xyz.Z * 0.634617;
                            break;
                        case SpaceType.BT_2020:
                            Rlin = xyz.X * 1.878576 + xyz.Y * -0.389220 + xyz.Z * -0.277265;
                            Glin = xyz.X * -0.673615 + xyz.Y * 1.633287 + xyz.Z * 0.015932;
                            Blin = xyz.X * 0.011764 + xyz.Y * -0.028524 + xyz.Z * 0.628288;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            rgb.R = ToGamma(type, Rlin);
            rgb.G = ToGamma(type, Glin);
            rgb.B = ToGamma(type, Blin);
            rgb.R = rgb.R > 1 ? 1 : rgb.R;
            rgb.G = rgb.G > 1 ? 1 : rgb.G;
            rgb.B = rgb.B > 1 ? 1 : rgb.B;
            rgb.R = rgb.R < 0 ? 0 : rgb.R;
            rgb.G = rgb.G < 0 ? 0 : rgb.G;
            rgb.B = rgb.B < 0 ? 0 : rgb.B;

            return rgb;
        }

        public static CIELAB XYZToLAB(ColorTemperature temperature, CIEXYZ xyz)
        {
            CIELAB lab;
            //默认D93
            lab.L = 116 * LabFunction(xyz.Y / 1.0) - 16;
            lab.A = 500 * (LabFunction(xyz.X / 0.972696) - LabFunction(xyz.Y / 1.0));
            lab.B = 200 * (LabFunction(xyz.Y / 1.0) - LabFunction(xyz.Z / 1.440273));
            switch (temperature)
            {
                case ColorTemperature.D65:
                    lab.L = 116 * LabFunction(xyz.Y / 1.0) - 16;
                    lab.A = 500 * (LabFunction(xyz.X / 0.950456) - LabFunction(xyz.Y / 1.0));
                    lab.B = 200 * (LabFunction(xyz.Y / 1.0) - LabFunction(xyz.Z / 1.08906));
                    break;
                case ColorTemperature.D93:
                    lab.L = 116 * LabFunction(xyz.Y / 1.0) - 16;
                    lab.A = 500 * (LabFunction(xyz.X / 0.972696) - LabFunction(xyz.Y / 1.0));
                    lab.B = 200 * (LabFunction(xyz.Y / 1.0) - LabFunction(xyz.Z / 1.440273));
                    break;
                case ColorTemperature.D120:
                    lab.L = 116 * LabFunction(xyz.Y / 1.0) - 16;
                    lab.A = 500 * (LabFunction(xyz.X / 0.978417) - LabFunction(xyz.Y / 1.0));
                    lab.B = 200 * (LabFunction(xyz.Y / 1.0) - LabFunction(xyz.Z / 1.618705));
                    break;
                default:
                    break;
            }
            return lab;
        }

        public static CIEXYZ LvxyToXYZ(Lvxy lvxy)
        {
            CIEXYZ xyz;
            if (Math.Abs(lvxy.y) < m_min2Zero)
            {
                throw new Exception();
            }
            xyz.Y = lvxy.Lv;
            xyz.X = lvxy.x * lvxy.Lv / lvxy.y;
            xyz.Z = (1 - lvxy.x - lvxy.y) * lvxy.Lv / lvxy.y;

            return xyz;
        }

        public static Lvxy XYZToLvxy(CIEXYZ xyz)
        {
            Lvxy lvxy;
            double u = xyz.X + xyz.Y + xyz.Z;
            if (Math.Abs(u) < m_min2Zero)
            {
                throw new Exception();
            }
            lvxy.Lv = xyz.Y;
            lvxy.x = xyz.X / u;
            lvxy.y = xyz.Y / u;

            return lvxy;
        }

        public static void RGBNormalize(ref RGB rgb, int x = 255)
        {
            rgb.R /= x;
            rgb.G /= x;
            rgb.B /= x;
        }

        public static void RGBReNormalize(ref RGB rgb, int x = 255)
        {
            rgb.R = Math.Round(rgb.R * x);
            rgb.G = Math.Round(rgb.G * x);
            rgb.B = Math.Round(rgb.B * x);
        }

        public static void RGBNormalize(RGB[] rgb, int x = 255)
        {
            for(int i = 0; i < rgb.Length; ++i)
            {
                RGBNormalize(ref rgb[i], x);
            }
        }

        public static void RGBReNormalize(RGB[] rgb, int x = 255)
        {
            for (int i = 0; i < rgb.Length; ++i)
            {
                RGBReNormalize(ref rgb[i], x);
            }
        }

        private static readonly double m_rad2Degree = 180.0 / Math.Acos(-1.0);
        private static readonly double m_degree2Rad = Math.Acos(-1.0) / 180.0;
        private static readonly double m_min2Zero = 0.0;

        private static double CalDE2000(ColorTemperature temperature, CIEXYZ xyz1, CIEXYZ xyz2)
        {
            CIELAB lab1 = XYZToLAB(temperature, xyz1);
            CIELAB lab2 = XYZToLAB(temperature, xyz2);

            double deltaH, deltaL, deltaC;
            double averL, averC, averC_, averH;
            double KL, SL, KC, SC, KH, SH, RT;
            double a1, a2, c1, c2, h1, h2, deltah, T;
            double lx, cx, hx, chx;
            deltaL = lab2.L - lab1.L;
            averL = (lab1.L + lab2.L) / 2;
            averC = (Math.Sqrt(Math.Pow(lab1.A, 2) + Math.Pow(lab1.B, 2)) + Math.Sqrt(Math.Pow(lab2.A, 2) + Math.Pow(lab2.B, 2))) / 2;

            a1 = lab1.A + lab1.A / 2 * (1 - Math.Sqrt(Math.Pow(averC, 7) / (Math.Pow(averC, 7) + Math.Pow(25, 7))));
            a2 = lab2.A + lab2.A / 2 * (1 - Math.Sqrt(Math.Pow(averC, 7) / (Math.Pow(averC, 7) + Math.Pow(25, 7))));
            c1 = Math.Sqrt(Math.Pow(a1, 2) + Math.Pow(lab1.B, 2));
            c2 = Math.Sqrt(Math.Pow(a2, 2) + Math.Pow(lab1.B, 2));
            deltaC = c2 - c1;

            averC_ = (c1 + c2) / 2;
            h1 = Math.Atan2(lab1.B, a1) * m_rad2Degree % 360;
            h2 = Math.Atan2(lab2.B, a2) * m_rad2Degree % 360;


            if (Math.Abs(h1 - h2) <= 180)
            {
                deltah = h2 - h1;
            }
            else if (Math.Abs(h1 - h2) > 180 && h2 <= h1)
            {
                deltah = h2 - h1 + 360;
            }
            else
            {
                deltah = h2 - h1 - 360;
            }
            deltaH = 2 * Math.Sqrt(c1 * c2) * Math.Sin(deltah * m_degree2Rad / 2);
            if (Math.Abs(h1 - h2) > 180)
            {
                averH = (h1 + h2 + 360) / 2;
            }
            else
            {
                averH = (h1 + h2) / 2;
            }
            T = 1 - 0.17 * Math.Cos((averH - 30) * m_degree2Rad) + 0.24 * Math.Cos((2 * averH) * m_degree2Rad) +
                0.32 * Math.Cos((3 * averH + 6) * m_degree2Rad) - 0.20 * Math.Cos((4 * averH - 63) * m_degree2Rad);
            SL = 1 + 0.015 * Math.Pow((averL - 50), 2) / Math.Sqrt(20 + (Math.Pow(averL - 50, 2)));
            SC = 1 + 0.045 * averC_;
            SH = 1 + 0.015 * averC_ * T;
            RT = -2 * Math.Sqrt(Math.Pow(averC_, 7) / (Math.Pow(averC_, 7) + Math.Pow(25, 7))) *
                 Math.Sin(60 * Math.Exp(-Math.Pow((averH - 275) / 25, 2)) * m_degree2Rad);
            KL = 1;
            KC = 1;
            KH = 1;

            lx = deltaL / (KL * SL);
            cx = deltaC / (KC * SL);
            hx = deltaH / (KH * SL);
            chx = RT * cx * hx;
            return Math.Sqrt(Math.Pow(lx, 2) + Math.Pow(cx, 2) + Math.Pow(hx, 2) + chx);
        }

        public static double CalWhiteLight(ColorTemperature colorTemperature, ColorSpace.RGB[] rgb1, CIEXYZ[] xyz, CIEXYZ[] wxyz)
        {
            double sx, sy;
            switch (colorTemperature)
            {
                case ColorTemperature.D45:
                    sx = 0.3621;
                    sy = 0.37087;

                    break;
                case ColorTemperature.D65:
                    sx = 0.3127;
                    sy = 0.329;

                    break;
                case ColorTemperature.D93:
                    sx = 0.285;
                    sy = 0.293;

                    break;
                case ColorTemperature.D120:
                    sx = 0.272;
                    sy = 0.278;

                    break;
                default:
                    throw new Exception();
            }

            if (xyz.Length < 15)
            {
                throw new Exception();
            }

            CIEXYZ[] red = new CIEXYZ[5];
            CIEXYZ[] green = new CIEXYZ[5];
            CIEXYZ[] blue = new CIEXYZ[5];
            CIEXYZ[] wrgb = new CIEXYZ[5];
            ColorSpace.RGB[] rgb = new ColorSpace.RGB[5];

            int size = xyz.Length / 3;

            Array.Copy(rgb1, rgb1.Length / 3 - 5, rgb, 0, 5);
            Array.Copy(xyz, xyz.Length / 3 - 5, red, 0, 5);
            Array.Copy(xyz, 2 * xyz.Length / 3 - 5, green, 0, 5);
            Array.Copy(xyz, xyz.Length - 5, blue, 0, 5);
            Array.Copy(wxyz, wxyz.Length - 5, wrgb, 0, 5);

            var blackXYZ = xyz[0];
            for (int i = 0; i < 5; i++)
            {
                red[i].X = red[i].X - blackXYZ.X;
                red[i].Y = red[i].Y - blackXYZ.Y;
                red[i].Z = red[i].Z - blackXYZ.Z;

                green[i].X = green[i].X - blackXYZ.X;
                green[i].Y = green[i].Y - blackXYZ.Y;
                green[i].Z = green[i].Z - blackXYZ.Z;

                blue[i].X = blue[i].X - blackXYZ.X;
                blue[i].Y = blue[i].Y - blackXYZ.Y;
                blue[i].Z = blue[i].Z - blackXYZ.Z;

                wrgb[i].X = wrgb[i].X - blackXYZ.X;
                wrgb[i].Y = wrgb[i].Y - blackXYZ.Y;
                wrgb[i].Z = wrgb[i].Z - blackXYZ.Z;



  
                //加叠加补偿D93
                /*double deltaX = rgb[i].X - (red[i].X + green[i].X + blue[i].X);
                red[i].X = red[i].X + deltaX * red[i].X / (red[i].X + green[i].X + blue[i].X);
                green[i].X = green[i].X + deltaX * green[i].X / (red[i].X + green[i].X + blue[i].X);
                blue[i].X = blue[i].X + deltaX * blue[i].X / (red[i].X + green[i].X + blue[i].X);

                double deltaY = rgb[i].Y - (red[i].Y + green[i].Y + blue[i].Y);
                red[i].Y = red[i].Y + deltaY * red[i].Y / (red[i].Y + green[i].Y + blue[i].Y);
                green[i].Y = green[i].Y + deltaY * green[i].Y / (red[i].Y + green[i].Y + blue[i].Y);
                blue[i].Y = blue[i].Y + deltaY * blue[i].Y / (red[i].Y + green[i].Y + blue[i].Y);

                double deltaZ = rgb[i].Z - (red[i].Z + green[i].Z + blue[i].Z);
                red[i].Z = red[i].Z + deltaZ * red[i].Z / (red[i].Z + green[i].Z + blue[i].Z);
                green[i].Z = green[i].Z + deltaZ * green[i].Z / (red[i].Z + green[i].Z + blue[i].Z);
                blue[i].Z = blue[i].Z + deltaZ * blue[i].Z / (red[i].Z + green[i].Z + blue[i].Z);*/

                //加叠加补偿D65
                /*double deltaX = rgb[i].X - (red[i].X + green[i].X + blue[i].X);
                red[i].X = red[i].X + deltaX * 0.412391;
                green[i].X = green[i].X + deltaX * 0.357584;
                blue[i].X = blue[i].X + deltaX * 0.180481;

                double deltaY = rgb[i].Y - (red[i].Y + green[i].Y + blue[i].Y);
                red[i].Y = red[i].Y + deltaY * 0.212639;
                green[i].Y = green[i].Y + deltaY * 0.715169;
                blue[i].Y = blue[i].Y + deltaY * 0.072192;

                double deltaZ = rgb[i].Z - (red[i].Z + green[i].Z + blue[i].Z);
                red[i].Z = red[i].Z + deltaZ * 0.019331;
                green[i].Z = green[i].Z + deltaZ * 0.119195;
                blue[i].Z = blue[i].Z + deltaZ * 0.950532;*/

            }

            double xx, y;
            double rx, ry, rz, gx, gy, gz, bx, by, bz;
            int xy_num = 0;
            double maxerror = 100;
            int min_num = 0;
            double R, G, B;
            short num = 0;
            double x, x0, y0, x1, y1;
            double real_x, real_y;

            double red_XX;
            double light = 0;
            double templight;
            for (int r = 255; r > 200; r--)
            {
                for (int g = 255; g > 200; g--)
                {
                    //if(Math.Abs(r - g) > 10)
                    //{
                     //   break;
                    //}
                    for (int b = 255; b > 192; b--)
                    {
                       // if(Math.Abs(b - g) > 10)
                        //{
                          //  break;
                       // }
                        if (r <= rgb[1].R)
                        {
                            num = 0;
                            x0 = rgb[0].R;
                            x1 = rgb[1].R;
                        }
                        else if (r <= rgb[2].R)
                        {
                            num = 1;
                            x0 = rgb[1].R;
                            x1 = rgb[2].R;
                        }
                        else if (r <= rgb[3].R)
                        {
                            num = 2;
                            x0 = rgb[2].R;
                            x1 = rgb[3].R;
                        }
                        else
                        {
                            num = 3;
                            x0 = rgb[3].R;
                            x1 = rgb[4].R;
                        }

                        x = r;
                        y0 = red[num].X;
                        y1 = red[num + 1].X;
                        rx = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;
                        y0 = red[num].Y;
                        y1 = red[num + 1].Y;
                        ry = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;

                        y0 = red[num].Z;
                        y1 = red[num + 1].Z;
                        rz = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;


                        if (g <= rgb[1].R)
                        {
                            num = 0;
                            x0 = rgb[0].R;
                            x1 = rgb[1].R;
                        }
                        else if (g <= rgb[2].R)
                        {
                            num = 1;
                            x0 = rgb[1].R;
                            x1 = rgb[2].R;
                        }
                        else if (g <= rgb[3].R)
                        {
                            num = 2;
                            x0 = rgb[2].R;
                            x1 = rgb[3].R;
                        }
                        else
                        {
                            num = 3;
                            x0 = rgb[3].R;
                            x1 = rgb[4].R;
                        }
                        x = g;
                        y0 = green[num].X;
                        y1 = green[num + 1].X;
                        gx = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;
                        y0 = green[num].Y;
                        y1 = green[num + 1].Y;
                        gy = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;

                        y0 = green[num].Z;
                        y1 = green[num + 1].Z;
                        gz = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;

                        if (b <= rgb[1].R)
                        {
                            num = 0;
                            x0 = rgb[0].R;
                            x1 = rgb[1].R;
                        }
                        else if (b <= rgb[2].R)
                        {
                            num = 1;
                            x0 = rgb[1].R;
                            x1 = rgb[2].R;
                        }
                        else if (b <= rgb[3].R)
                        {
                            num = 2;
                            x0 = rgb[2].R;
                            x1 = rgb[3].R;
                        }
                        else
                        {
                            num = 3;
                            x0 = rgb[3].R;
                            x1 = rgb[4].R;
                        }
                        x = b;
                        y0 = blue[num].X;
                        y1 = blue[num + 1].X;
                        bx = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;
                        y0 = blue[num].Y;
                        y1 = blue[num + 1].Y;
                        by = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;
                        y0 = blue[num].Z;
                        y1 = blue[num + 1].Z;
                        bz = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;

                        double allxyz = rx + ry + rz + gx + gy + gz + bx + by + bz;
                        xx = (rx + gx + bx) / allxyz;
                        y = (ry + gy + by) / allxyz;

                        if(Math.Abs(xx-sx)<0.0015&& Math.Abs(y-sy)<0.0015)
                        {
                            light = ry + gy + by;
                            string str = "rgb= " + r.ToString() + ",  " + g.ToString() + ",  " + b.ToString();
                            LogHelper.WriteToLog(str, LogLevel.INFO);
                            Console.WriteLine(str);
                            return light;
                        }

                        double next = Math.Pow(xx - sx, 2) + Math.Pow(y - sy, 2);

                        templight = ry + gy + by;
                        if (maxerror > next)
                        {
                            maxerror = next;
                            min_num = xy_num;
                            R = r;
                            G = g;
                            B = b;
                            real_x = xx;
                            real_y = y;
                            red_XX = rx;
                            light = ry + gy + by;
                        }
                        xy_num++;
                    }
                }
            }

            return light;
        }

        #endregion

        #region non-static method

        private double[] m_transforMatrix = new double[9];
        private double[] m_invTransforMatrix = new double[9];
        private readonly MapRGB m_mapRGB = new MapRGB();
        private CIEXYZ m_blackXYZ;
        private readonly double[] m_maxRGBLight = new double[3];

        private ColorTemperature m_colorTemperature = ColorTemperature.D65;
        private SpaceType m_spaceType = SpaceType.sRGB;
        private Interpolation m_interpolation = Interpolation.linear;
        private double m_whiteLight;

        public ColorTemperature MColorTemperature
        {
            get
            {
                return m_colorTemperature;
            }
            set
            {
                m_colorTemperature = value;
            }
        }

        public SpaceType MSpaceType
        {
            get
            {
                return m_spaceType;
            }
            set
            {
                m_spaceType = value;
            }
        }

        public Interpolation MInterpolation
        {
            get
            {
                return m_interpolation;
            }
            set
            {
                m_interpolation = value;
            }
        }

        public double WhiteLight
        {
            get
            {
                return m_whiteLight;
            }
            set
            {
                m_whiteLight = value;
            }
        }



        public void setWRGBLvMap(CIEXYZ[] xyz)
        {
            for(int i = 0; i<xyz.Length; i++)
            {
                m_mapRGB.RGBX[i] = xyz[i].X;
                m_mapRGB.RGBLv[i] = xyz[i].Y;
                m_mapRGB.RGBZ[i] = xyz[i].Z;
            }
        }

        public void SetMapSize(int size)
        {
            if(size <= 0)
            {
                return;
            }
            m_mapRGB.size = size;
        }


        public void SetRGBLvMap(RGB[] rgb, CIEXYZ[] xyz, CIEXYZ[] wxyz)
        {
            
            if(rgb.Length == 0 || rgb.Length % 3 != 0 || rgb.Length != xyz.Length)
            {
                throw new Exception();
            }
            int size = rgb.Length / 3;

            m_blackXYZ = xyz[0];
            for(int i = 0; i< rgb.Length; i++)
            {
                xyz[i].X = xyz[i].X - m_blackXYZ.X;
                xyz[i].Y = xyz[i].Y - m_blackXYZ.Y;
                xyz[i].Z = xyz[i].Z - m_blackXYZ.Z;
            }
            for(int i = 0; i< wxyz.Length; i++)
            {
                wxyz[i].X = wxyz[i].X - m_blackXYZ.X;
                wxyz[i].Y = wxyz[i].Y - m_blackXYZ.Y;
                wxyz[i].Z = wxyz[i].Z - m_blackXYZ.Z;
                m_mapRGB.RGBLv[i] = wxyz[i].Y;
                m_mapRGB.RGBX[i] = wxyz[i].X;
                m_mapRGB.RGBZ[i] = wxyz[i].Z;
            }
            m_maxRGBLight[0] = xyz[size - 1].Y;
            m_maxRGBLight[1] = xyz[2 * size - 1].Y;
            m_maxRGBLight[2] = xyz[3 * size - 1].Y;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int index = i * size + j;
                    switch (i)
                    {
                        case 0:
                            m_mapRGB.R[j] = rgb[index].R;
                            m_mapRGB.RLv[j] = xyz[index].Y;
                            m_mapRGB.RX[j] = xyz[index].X;
                            m_mapRGB.RZ[j] = xyz[index].Z;
                            break;
                        case 1:
                            m_mapRGB.G[j] = rgb[index].G;
                            m_mapRGB.GLv[j] = xyz[index].Y;
                            m_mapRGB.GX[j] = xyz[index].X;
                            m_mapRGB.GZ[j] = xyz[index].Z;
                            break;
                        case 2:
                            m_mapRGB.B[j] = rgb[index].B;
                            m_mapRGB.BLv[j] = xyz[index].Y;
                            m_mapRGB.BX[j] = xyz[index].X;
                            m_mapRGB.BZ[j] = xyz[index].Z;
                            break;
                    }
                }
            }

            for(int i = 0; i < size - 1; i++)
            {
                double delta_light_R = m_mapRGB.RLv[i + 1] - m_mapRGB.RLv[i];
                double delta_light_G = m_mapRGB.GLv[i + 1] - m_mapRGB.GLv[i];
                double delta_light_B = m_mapRGB.BLv[i + 1] - m_mapRGB.BLv[i];
                if (delta_light_R < 0 || delta_light_G < 0 || delta_light_B < 0)
                {
                    throw new Exception();
                }
            }
        }

  
        public void CalTransforMatrix(CIEXYZ[] xyz)
        {
            int size = m_mapRGB.size - 1;
            /*for (int i=0;i<3;i++)
            {
                xyz[i].X = xyz[i].X - m_blackXYZ.X;
                xyz[i].Y = xyz[i].Y - m_blackXYZ.Y;
                xyz[i].Z = xyz[i].Z - m_blackXYZ.Z;
            }*/
            //加叠加补偿
            double deltaX = m_mapRGB.RGBX[size] - (xyz[0].X + xyz[1].X + xyz[2].X);
            xyz[0].X = xyz[0].X + deltaX * xyz[0].X / (xyz[0].X + xyz[1].X + xyz[2].X);
            xyz[1].X = xyz[1].X + deltaX * xyz[1].X / (xyz[0].X + xyz[1].X + xyz[2].X);
            xyz[2].X = xyz[2].X + deltaX * xyz[2].X / (xyz[0].X + xyz[1].X + xyz[2].X);
            double deltaY = m_mapRGB.RGBLv[size] - (xyz[0].Y + xyz[1].Y + xyz[2].Y);
            xyz[0].Y = xyz[0].Y + deltaY * xyz[0].Y / (xyz[0].Y + xyz[1].Y + xyz[2].Y);
            xyz[1].Y = xyz[1].Y + deltaY * xyz[1].Y / (xyz[0].Y + xyz[1].Y + xyz[2].Y);
            xyz[2].Y = xyz[2].Y + deltaY * xyz[2].Y / (xyz[0].Y + xyz[1].Y + xyz[2].Y);
            double deltaZ = m_mapRGB.RGBZ[size] - (xyz[0].Z + xyz[1].Z + xyz[2].Z);
            xyz[0].Z = xyz[0].Z + deltaZ * xyz[0].Z / (xyz[0].Z + xyz[1].Z + xyz[2].Z);
            xyz[1].Z = xyz[1].Z + deltaZ * xyz[1].Z / (xyz[0].Z + xyz[1].Z + xyz[2].Z);
            xyz[2].Z = xyz[2].Z + deltaZ * xyz[2].Z / (xyz[0].Z + xyz[1].Z + xyz[2].Z);

           // double deltaY = 0;// m_mapRGB.RGBLv[16] - (xyz[0].Y + xyz[1].Y + xyz[2].Y);

            
            double[] a = new double[9];
            a[0] = m_mapRGB.RLv[size] + deltaY * xyz[0].Y / (xyz[0].Y + xyz[1].Y + xyz[2].Y);
            a[1] = m_mapRGB.GLv[0];
            a[2] = m_mapRGB.BLv[0];
            a[3] = m_mapRGB.RLv[0];
            a[4] = m_mapRGB.GLv[size] + deltaY * xyz[1].Y / (xyz[0].Y + xyz[1].Y + xyz[2].Y);
            a[5] = m_mapRGB.BLv[0];
            a[6] = m_mapRGB.RLv[0];
            a[7] = m_mapRGB.GLv[0];
            a[8] = m_mapRGB.BLv[size] + deltaY * xyz[2].Y / (xyz[0].Y + xyz[1].Y + xyz[2].Y);

            double[] inv_a = Cal3MatrixInv(a);

            double[] matrix = new double[9];


            matrix[0] = inv_a[0] * xyz[0].X + inv_a[1] * xyz[1].X + inv_a[2] * xyz[2].X;
            matrix[1] = inv_a[3] * xyz[0].X + inv_a[4] * xyz[1].X + inv_a[5] * xyz[2].X;
            matrix[2] = inv_a[6] * xyz[0].X + inv_a[7] * xyz[1].X + inv_a[8] * xyz[2].X;
            matrix[3] = inv_a[0] * xyz[0].Y + inv_a[1] * xyz[1].Y + inv_a[2] * xyz[2].Y;
            matrix[4] = inv_a[3] * xyz[0].Y + inv_a[4] * xyz[1].Y + inv_a[5] * xyz[2].Y;
            matrix[5] = inv_a[6] * xyz[0].Y + inv_a[7] * xyz[1].Y + inv_a[8] * xyz[2].Y;
            matrix[6] = inv_a[0] * xyz[0].Z + inv_a[1] * xyz[1].Z + inv_a[2] * xyz[2].Z;
            matrix[7] = inv_a[3] * xyz[0].Z + inv_a[4] * xyz[1].Z + inv_a[5] * xyz[2].Z;
            matrix[8] = inv_a[6] * xyz[0].Z + inv_a[7] * xyz[1].Z + inv_a[8] * xyz[2].Z;


            /*matrix[0] = (xyz[0].X / xyz[0].Y);
            matrix[1] = (xyz[1].X / xyz[1].Y) ;
            matrix[2] = (xyz[2].X / xyz[2].Y) ;
            matrix[3] = (xyz[0].Y / xyz[0].Y) ;
            matrix[4] = (xyz[1].Y / xyz[1].Y) ;
            matrix[5] = (xyz[2].Y / xyz[2].Y) ;
            matrix[6] = (xyz[0].Z / xyz[0].Y) ;
            matrix[7] = (xyz[1].Z / xyz[1].Y) ;
            matrix[8] = (xyz[2].Z / xyz[2].Y) ;*/

            double[] invMatrix = Cal3MatrixInv(matrix);

            m_transforMatrix = matrix;
            m_invTransforMatrix = invMatrix;


            /*for(int i = 0; i< m_mapRGB.RLv.Length; i++)
            {
                m_mapRGB.R_a[i] = m_mapRGB.RX[i] - (matrix[0] * m_mapRGB.RLv[i] + matrix[1] * m_mapRGB.GLv[0] + matrix[2] * m_mapRGB.BLv[0]);
                m_mapRGB.R_b[i] = m_mapRGB.RLv[i] - (matrix[3] * m_mapRGB.RLv[i] + matrix[4] * m_mapRGB.GLv[0] + matrix[5] * m_mapRGB.BLv[0]);
                m_mapRGB.R_c[i] = m_mapRGB.RZ[i] - (matrix[6] * m_mapRGB.RLv[i] + matrix[7] * m_mapRGB.GLv[0] + matrix[8] * m_mapRGB.BLv[0]);

                m_mapRGB.G_a[i] = m_mapRGB.GX[i] - (matrix[0] * m_mapRGB.RLv[0] + matrix[1] * m_mapRGB.GLv[i] + matrix[2] * m_mapRGB.BLv[0]);
                m_mapRGB.G_b[i] = m_mapRGB.GLv[i] - (matrix[3] * m_mapRGB.RLv[0] + matrix[4] * m_mapRGB.GLv[i] + matrix[5] * m_mapRGB.BLv[0]);
                m_mapRGB.G_c[i] = m_mapRGB.GZ[i] - (matrix[6] * m_mapRGB.RLv[0] + matrix[7] * m_mapRGB.GLv[i] + matrix[8] * m_mapRGB.BLv[0]);

                m_mapRGB.B_a[i] = m_mapRGB.BX[i] - (matrix[0] * m_mapRGB.RLv[0] + matrix[1] * m_mapRGB.GLv[0] + matrix[2] * m_mapRGB.BLv[i]);
                m_mapRGB.B_b[i] = m_mapRGB.BLv[i] - (matrix[3] * m_mapRGB.RLv[0] + matrix[4] * m_mapRGB.GLv[0] + matrix[5] * m_mapRGB.BLv[i]);
                m_mapRGB.B_c[i] = m_mapRGB.BZ[i] - (matrix[6] * m_mapRGB.RLv[0] + matrix[7] * m_mapRGB.GLv[0] + matrix[8] * m_mapRGB.BLv[i]);

            }
            for (int i = 0; i < m_mapRGB.RLv.Length; i++)
            {
                m_mapRGB.R_a[i] = (invMatrix[0] * m_mapRGB.RX[i] + invMatrix[1] * m_mapRGB.RLv[i] + invMatrix[2] * m_mapRGB.RZ[0]) - m_mapRGB.RLv[i];
                m_mapRGB.R_b[i] = (invMatrix[3] * m_mapRGB.RX[i] + invMatrix[4] * m_mapRGB.RLv[0] + invMatrix[5] * m_mapRGB.RZ[0]) - m_mapRGB.GLv[0];
                m_mapRGB.R_c[i] = (invMatrix[6] * m_mapRGB.RX[i] + invMatrix[7] * m_mapRGB.RLv[0] + invMatrix[8] * m_mapRGB.RZ[0]) - m_mapRGB.BLv[0];

                m_mapRGB.G_a[i] = (invMatrix[0] * m_mapRGB.GX[i] + invMatrix[1] * m_mapRGB.GLv[i] + invMatrix[2] * m_mapRGB.GZ[0]) - m_mapRGB.RLv[0];
                m_mapRGB.G_b[i] = (invMatrix[3] * m_mapRGB.GX[i] + invMatrix[4] * m_mapRGB.GLv[0] + invMatrix[5] * m_mapRGB.GZ[0]) - m_mapRGB.GLv[i];
                m_mapRGB.G_c[i] = (invMatrix[6] * m_mapRGB.GX[i] + invMatrix[7] * m_mapRGB.GLv[0] + invMatrix[8] * m_mapRGB.GZ[0]) - m_mapRGB.BLv[0];

                m_mapRGB.B_a[i] = (invMatrix[0] * m_mapRGB.BX[i] + invMatrix[1] * m_mapRGB.BLv[i] + invMatrix[2] * m_mapRGB.BZ[0]) - m_mapRGB.RLv[0];
                m_mapRGB.B_b[i] = (invMatrix[3] * m_mapRGB.BX[i] + invMatrix[4] * m_mapRGB.BLv[0] + invMatrix[5] * m_mapRGB.BZ[0]) - m_mapRGB.GLv[0];
                m_mapRGB.B_c[i] = (invMatrix[6] * m_mapRGB.BX[i] + invMatrix[7] * m_mapRGB.BLv[0] + invMatrix[8] * m_mapRGB.BZ[0]) - m_mapRGB.BLv[i];

            }*/

            for (int i = 0; i <= size; i++)
            {
                m_mapRGB.R_a[i] = (invMatrix[0] * m_mapRGB.RX[i] + invMatrix[1] * m_mapRGB.RLv[i] + invMatrix[2] * m_mapRGB.RZ[i]) - m_mapRGB.RLv[i];
                m_mapRGB.R_b[i] = (invMatrix[3] * m_mapRGB.RX[i] + invMatrix[4] * m_mapRGB.RLv[i] + invMatrix[5] * m_mapRGB.RZ[i]) - m_mapRGB.GLv[0];
                m_mapRGB.R_c[i] = (invMatrix[6] * m_mapRGB.RX[i] + invMatrix[7] * m_mapRGB.RLv[i] + invMatrix[8] * m_mapRGB.RZ[i]) - m_mapRGB.BLv[0];

                m_mapRGB.G_a[i] = (invMatrix[0] * m_mapRGB.GX[i] + invMatrix[1] * m_mapRGB.GLv[i] + invMatrix[2] * m_mapRGB.GZ[i]) - m_mapRGB.RLv[0];
                m_mapRGB.G_b[i] = (invMatrix[3] * m_mapRGB.GX[i] + invMatrix[4] * m_mapRGB.GLv[i] + invMatrix[5] * m_mapRGB.GZ[i]) - m_mapRGB.GLv[i];
                m_mapRGB.G_c[i] = (invMatrix[6] * m_mapRGB.GX[i] + invMatrix[7] * m_mapRGB.GLv[i] + invMatrix[8] * m_mapRGB.GZ[i]) - m_mapRGB.BLv[0];

                m_mapRGB.B_a[i] = (invMatrix[0] * m_mapRGB.BX[i] + invMatrix[1] * m_mapRGB.BLv[i] + invMatrix[2] * m_mapRGB.BZ[i]) - m_mapRGB.RLv[0];
                m_mapRGB.B_b[i] = (invMatrix[3] * m_mapRGB.BX[i] + invMatrix[4] * m_mapRGB.BLv[i] + invMatrix[5] * m_mapRGB.BZ[i]) - m_mapRGB.GLv[0];
                m_mapRGB.B_c[i] = (invMatrix[6] * m_mapRGB.BX[i] + invMatrix[7] * m_mapRGB.BLv[i] + invMatrix[8] * m_mapRGB.BZ[i]) - m_mapRGB.BLv[i];

            }

            for (int i = 0; i<= size; i++)
            {
                m_mapRGB.RLv[i] = m_mapRGB.RLv[i] + m_mapRGB.R_a[i] + m_mapRGB.G_a[i] + m_mapRGB.B_a[i];
                m_mapRGB.GLv[i] = m_mapRGB.GLv[i] + m_mapRGB.R_b[i] + m_mapRGB.G_b[i] + m_mapRGB.B_b[i];
                m_mapRGB.BLv[i] = m_mapRGB.BLv[i] + m_mapRGB.R_c[i] + m_mapRGB.G_c[i] + m_mapRGB.B_c[i];
            }
            //加叠加补偿

            for (int i = 0; i <= size; i++)
            {
                double all = (m_mapRGB.RLv[i] + m_mapRGB.GLv[i] + m_mapRGB.BLv[i]);
                double deltaYY = m_mapRGB.RGBLv[i] - all; 
                m_mapRGB.RLv[i] = m_mapRGB.RLv[i] + deltaYY * m_mapRGB.RLv[i] / all;
                m_mapRGB.GLv[i] = m_mapRGB.GLv[i] + deltaYY * m_mapRGB.GLv[i] / all;
                m_mapRGB.BLv[i] = m_mapRGB.BLv[i] + deltaYY * m_mapRGB.BLv[i] / all;
            }


            //D65
            /*for (int i = 0; i < m_mapRGB.RLv.Length; i++)
            {
                double deltaYY = m_mapRGB.RGBLv[i] - (m_mapRGB.RLv[i] + m_mapRGB.GLv[i] + m_mapRGB.BLv[i]);
                m_mapRGB.RLv[i] = m_mapRGB.RLv[i] + deltaYY * 0.212639;
                m_mapRGB.GLv[i] = m_mapRGB.GLv[i] + deltaYY * 0.715169;
                m_mapRGB.BLv[i] = m_mapRGB.BLv[i] + deltaYY * 0.072192;
            }*/

        }

        private double LargrangeInterpolationRGB(RGB.Type type, double x)
        {
            double result;
            double x0, y0, x1, y1;
            int key = 0;
            double[] Ch, ChLv;
            switch (type)
            {
                case RGB.Type.R:
                    Ch = m_mapRGB.R;
                    ChLv = m_mapRGB.RLv;
                    break;
                case RGB.Type.G:
                    Ch = m_mapRGB.G;
                    ChLv = m_mapRGB.GLv;
                    break;
                case RGB.Type.B:
                    Ch = m_mapRGB.B;
                    ChLv = m_mapRGB.BLv;
                    break;
                default:
                    throw new System.Exception();
            }

            for (int i = 0; i < m_mapRGB.size; i++)
            {
                //存在非常接近的亮度则直接赋值，否者采用插值方法逼近
                if (Math.Abs(ChLv[i] - x) < m_min2Zero)
                {
                    result = Ch[i];
                    return result;
                }
                if (ChLv[i] > x)
                {
                    key = i;
                    break;
                }
                if(i == m_mapRGB.size-1)
                {
                    key = m_mapRGB.size-1;
                }
            }

            switch (key)
            {
                case 0:
                    y0 = Ch[key];
                    x0 = ChLv[key];
                    y1 = Ch[key + 1];
                    x1 = ChLv[key + 1];
                    break;
                default:
                    y0 = Ch[key - 1];
                    x0 = ChLv[key - 1];
                    y1 = Ch[key];
                    x1 = ChLv[key];
                    break;
            }

            if (Math.Abs(x1 - x0) < m_min2Zero)
            {
                throw new Exception();
            }
            switch (m_interpolation)
            {
                case Interpolation.linear:
                    result = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;
                    break;
                case Interpolation.PowerExponent:
                    if (key >= 1 && key < (m_mapRGB.size-1) && x0>0 && y0 > 0 && x1 > 0 && y1 > 0 )
                    {
                        double u0, v0, u1, v1;
                        u0 = Math.Log(x0);
                        v0 = Math.Log(y0);
                        u1 = Math.Log(x1);
                        v1 = Math.Log(y1);
                        double c1 = (v0 - v1) / (u0 - u1);
                        double c0 = v0 - c1 * u0;
                        double a = Math.Exp(c0);
                        double b = c1;
                        result = a * Math.Pow(x, b);
                    }
                    else
                    {
                        result = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;
                    }
                    break;
                //case Interpolation.Hermite:
                    //if (key >= 2 && key < 16)
                    //{
                    //    double k_forword, k_back;
                    //    k_forword = (y0 - y00) / (x0 - x00);
                    //    k_back = (y2 - y1) / (x2 - x1);
                    //    result = (1 + 2 * (x - x0) / (x1 - x0)) * Math.Pow((x - x1) / (x0 - x1), 2) * y0 +
                    //              (1 + 2 * (x - x1) / (x0 - x1)) * Math.Pow((x - x0) / (x1 - x0), 2) * y1 +
                    //              (x - x0) * Math.Pow((x - x1) / (x0 - x1), 2) * k_forword +
                    //              (x - x1) * Math.Pow((x - x0) / (x1 - x0), 2) * k_back;
                    //}
                    //else
                    //{
                    //    result = (x1 - x) / (x1 - x0) * y0 + (x - x0) / (x1 - x0) * y1;
                    //}
                    //break;
                default:
                    result = 0;
                    break;
            }

            return result;
        }

        public RGB CalLargrangeTargetRGB(RGB rgb)
        {
            RGB result;

            CIEXYZ targetXYZ = ColorSpaceToXYZ(m_colorTemperature, m_spaceType, rgb);
            double LR = m_invTransforMatrix[0] * targetXYZ.X + m_invTransforMatrix[1] * targetXYZ.Y + m_invTransforMatrix[2] * targetXYZ.Z;
            double LG = m_invTransforMatrix[3] * targetXYZ.X + m_invTransforMatrix[4] * targetXYZ.Y + m_invTransforMatrix[5] * targetXYZ.Z;
            double LB = m_invTransforMatrix[6] * targetXYZ.X + m_invTransforMatrix[7] * targetXYZ.Y + m_invTransforMatrix[8] * targetXYZ.Z;


                if (LR < m_mapRGB.RGBLv[0])
                {
                    result.R = (LR - m_mapRGB.RLv[0]) / (m_mapRGB.RLv[m_mapRGB.size-1] - m_mapRGB.RLv[0]);
                }
            else
            {
                result.R = LargrangeInterpolationRGB(RGB.Type.R, LR);
            }
                 if (LG < m_mapRGB.RGBLv[0])
                {
                    result.G = (LG - m_mapRGB.GLv[0]) / (m_mapRGB.GLv[m_mapRGB.size-1] - m_mapRGB.GLv[0]);
                }
            else
            {
                  result.G = LargrangeInterpolationRGB(RGB.Type.G, LG);

            }
            if (LB < m_mapRGB.RGBLv[0])
            {
                    result.B = (LB - m_mapRGB.BLv[0]) / (m_mapRGB.BLv[m_mapRGB.size-1] - m_mapRGB.BLv[0]);
            }
            else
            {
                result.B = LargrangeInterpolationRGB(RGB.Type.B, LB);
            }

            //result.R = LargrangeInterpolationRGB(RGB.Type.R, LR);
            //result.G = LargrangeInterpolationRGB(RGB.Type.G, LG);
            //result.B = LargrangeInterpolationRGB(RGB.Type.B, LB);
            string str = "RGB= " + result.R.ToString()+",  "+ result.G.ToString()+",  "+ result.B.ToString();

            //Console.WriteLine(str);

            EdgeProcessing(m_maxRGBLight, rgb, ref result);

            return result;
        }

        public RGB[] CalLargrangeTargetRGB(RGB[] rgbs)
        {
            var result = new RGB[rgbs.Length];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = CalLargrangeTargetRGB(rgbs[i]);
            }
            return result;
        }

        public void CIEXYZNormalize(ref CIEXYZ xyz)
        {
            xyz.X /= m_whiteLight;
            xyz.Y /= m_whiteLight;
            xyz.Z /= m_whiteLight;
        }

        public void CIEXYZNormalize(CIEXYZ[] xyz)
        {
            for(int i = 0; i < xyz.Length; ++i)
            {
                CIEXYZNormalize(ref xyz[i]);
            }
        }

        public double CalDE2000(RGB rgb1, RGB rgb2)
        {
            CIEXYZ xyz1 = ColorSpaceToXYZ(MColorTemperature, MSpaceType, rgb1);
            CIEXYZ xyz2 = ColorSpaceToXYZ(MColorTemperature, MSpaceType, rgb2);
            return CalDE2000(MColorTemperature, xyz1, xyz2);
        }

        public double CalDE2000(RGB rgb1, CIEXYZ xyz2)
        {
            xyz2.X = xyz2.X - m_blackXYZ.X;
            xyz2.Y = xyz2.Y - m_blackXYZ.Y;
            xyz2.Z = xyz2.Z - m_blackXYZ.Z;

            CIEXYZ xyz1 = ColorSpaceToXYZ(MColorTemperature, MSpaceType, rgb1);
            return CalDE2000(MColorTemperature, xyz1, xyz2);
        }

        //

        public double CalColorGamutCoverage(CIEXYZ red, CIEXYZ green, CIEXYZ blue)
        {
            double coveragePer;
            double u_red, v_red, u_green, v_green, u_blue, v_blue;
            double div_red, div_green, div_blue;
            div_red = red.X + 15 * red.Y + 3 * red.Z;
            div_green = green.X + 15 * green.Y + 3 * green.Z;
            div_blue = blue.X + 15 * blue.Y + 3 * blue.Z;
            if (div_red < m_min2Zero || div_green < m_min2Zero || div_blue < m_min2Zero)
            {
                throw new Exception();
            }
            u_red = 4 * red.X / div_red;
            v_red = 9 * red.Y / div_red;
            u_green = 4 * green.X / div_green;
            v_green = 9 * green.Y / div_green;
            u_blue = 4 * blue.X / div_blue;
            v_blue = 9 * blue.Y / div_blue;
            double S = 1 / 2 * ((u_red - u_blue) * (v_green - v_blue) - (u_green - u_blue) * (v_red - v_blue));
            coveragePer = (S / 0.1952) * 100;
            return coveragePer;
        }

        public CIEXYZ ChromaticAdaptationTranform(ChromaticAdaptation AdaptType, CIEXYZ xyz)
        {
            CIEXYZ ret_xyz;
            ret_xyz.X = 0;
            ret_xyz.Y = 0;
            ret_xyz.Z = 0;
            switch (AdaptType)
            {
                case ChromaticAdaptation.D65TD93:
                    ret_xyz.X = 0.967151245407572 * xyz.X - 0.012998284223708 * xyz.Y + 0.061024830742356 * xyz.Z;
                    ret_xyz.Y = -0.013061999512235 * xyz.X + 0.991137833408354 * xyz.Y + 0.019537088382847 * xyz.Z;
                    ret_xyz.Z = 0.012938918795789 * xyz.X - 0.022227816555079 * xyz.Y + 1.331612222261907 * xyz.Z;
                    break;
                case ChromaticAdaptation.D65TD120:
                    ret_xyz.X = 0.946219608701597 * xyz.X - 0.022007713219299 * xyz.Y + 0.092818388746250 * xyz.Z;
                    ret_xyz.Y = -0.023314708498996 * xyz.X + 0.989597625281912 * xyz.Y + 0.029899215008943 * xyz.Z;
                    ret_xyz.Z = 0.019424603263898 * xyz.X - 0.033219269182530 * xyz.Y + 1.499885257224812 * xyz.Z;
                    break;
                case ChromaticAdaptation.D93TD65:
                    ret_xyz.X = 1.034770177188749 * xyz.X + 0.012502891965650 * xyz.Y - 0.047604658447696 * xyz.Z;
                    ret_xyz.Y = 0.013830664028722 * xyz.X + 1.008776649668104 * xyz.Y - 0.015434352547181 * xyz.Z;
                    ret_xyz.Z = -0.009823717155332 * xyz.X + 0.016717425717376 * xyz.Y + 0.751174301444759 * xyz.Z;
                    break;
                case ChromaticAdaptation.D93TD120:
                    ret_xyz.X = 0.977903629270187 * xyz.X - 0.008818701145773 * xyz.Y + 0.025018001843524 * xyz.Z;
                    ret_xyz.Y = -0.010732294197129 * xyz.X + 0.998491313575745 * xyz.Y + 0.008295612054538 * xyz.Z;
                    ret_xyz.Z = 0.004906107077432 * xyz.X - 0.008193738982048 * xyz.Y + 1.126263276651203 * xyz.Z;
                    break;
                case ChromaticAdaptation.D120TD65:
                    ret_xyz.X = 1.058716421955348 * xyz.X + 0.021331262477989 * xyz.Y - 0.065942471233656 * xyz.Z;
                    ret_xyz.Y = 0.025340437463663 * xyz.X + 1.010346539728817 * xyz.Y - 0.021708745281384 * xyz.Z;
                    ret_xyz.Z = -0.013149909672851 * xyz.X + 0.022100772176047 * xyz.Y + 0.667090867697533 * xyz.Z;
                    break;
                case ChromaticAdaptation.D120TD93:
                    ret_xyz.X = 1.022807052806995 * xyz.X + 0.008846481465187 * xyz.Y - 0.022785059446563 * xyz.Z;
                    ret_xyz.Y = 0.011030001950674 * xyz.X + 1.001545836177697 * xyz.Y - 0.007622004995518 * xyz.Z;
                    ret_xyz.Z = -0.004375197225928 * xyz.X + 0.007247864281913 * xyz.Y + 0.887935666512606 * xyz.Z;
                    break;
                default:
                    break;
            }
            return ret_xyz;
        }

        private double[] m_chromaticAdaptation = new double[9];

        public CIEXYZ ChromaticAdaptationTranform(CIEXYZ xyz)
        {
            CIEXYZ ret_xyz;
            ret_xyz.X = m_chromaticAdaptation[0] * xyz.X + m_chromaticAdaptation[1] * xyz.Y + m_chromaticAdaptation[2] * xyz.Z;
            ret_xyz.Y = m_chromaticAdaptation[3] * xyz.X + m_chromaticAdaptation[4] * xyz.Y + m_chromaticAdaptation[5] * xyz.Z;
            ret_xyz.Z = m_chromaticAdaptation[6] * xyz.X + m_chromaticAdaptation[7] * xyz.Y + m_chromaticAdaptation[8] * xyz.Z;
            return ret_xyz;
        }

        private double[] CalchromaticAdaptationMatrix(CIEXYZ xyz_startWhite, CIEXYZ xyz_targetWhite)
        {
            double[] MA = new double[9] { 0.8951000, 0.2664000, -0.1614000, -0.7502000, 1.7135000, 0.0367000, 0.0389000, -0.0685000, 1.0296000 };
            double[] PYB_S = new double[3];
            PYB_S[0] = MA[0] * xyz_startWhite.X + MA[1] * xyz_startWhite.Y + MA[2] * xyz_startWhite.Z;
            PYB_S[1] = MA[3] * xyz_startWhite.X + MA[4] * xyz_startWhite.Y + MA[5] * xyz_startWhite.Z;
            PYB_S[2] = MA[6] * xyz_startWhite.X + MA[7] * xyz_startWhite.Y + MA[8] * xyz_startWhite.Z;
            double[] PYB_D = new double[3];
            PYB_D[0] = MA[0] * xyz_targetWhite.X + MA[1] * xyz_targetWhite.Y + MA[2] * xyz_targetWhite.Z;
            PYB_D[1] = MA[3] * xyz_targetWhite.X + MA[4] * xyz_targetWhite.Y + MA[5] * xyz_targetWhite.Z;
            PYB_D[2] = MA[6] * xyz_targetWhite.X + MA[7] * xyz_targetWhite.Y + MA[8] * xyz_targetWhite.Z;

            double[] inv_MA = Cal3MatrixInv(MA);
            double[] temp = new double[9];
            temp[0] = inv_MA[0] * PYB_D[0] / PYB_S[0];
            temp[1] = inv_MA[1] * PYB_D[1] / PYB_S[1];
            temp[2] = inv_MA[2] * PYB_D[2] / PYB_S[2];
            temp[3] = inv_MA[3] * PYB_D[0] / PYB_S[0];
            temp[4] = inv_MA[4] * PYB_D[1] / PYB_S[1];
            temp[5] = inv_MA[5] * PYB_D[2] / PYB_S[2];
            temp[6] = inv_MA[6] * PYB_D[0] / PYB_S[0];
            temp[7] = inv_MA[7] * PYB_D[1] / PYB_S[1];
            temp[8] = inv_MA[8] * PYB_D[2] / PYB_S[2];

            double[] chromaticAdaptation = new double[9];
            chromaticAdaptation[0] = temp[0] * MA[0] + temp[1] * MA[3] + temp[2] * MA[6];
            chromaticAdaptation[1] = temp[0] * MA[1] + temp[1] * MA[4] + temp[2] * MA[7];
            chromaticAdaptation[2] = temp[0] * MA[2] + temp[1] * MA[5] + temp[2] * MA[8];
            chromaticAdaptation[3] = temp[3] * MA[0] + temp[4] * MA[3] + temp[5] * MA[6];
            chromaticAdaptation[4] = temp[3] * MA[1] + temp[4] * MA[4] + temp[5] * MA[7];
            chromaticAdaptation[5] = temp[3] * MA[2] + temp[4] * MA[5] + temp[5] * MA[8];
            chromaticAdaptation[6] = temp[6] * MA[0] + temp[7] * MA[3] + temp[8] * MA[6];
            chromaticAdaptation[7] = temp[6] * MA[1] + temp[7] * MA[4] + temp[8] * MA[7];
            chromaticAdaptation[8] = temp[6] * MA[2] + temp[7] * MA[5] + temp[8] * MA[8];
            m_chromaticAdaptation = chromaticAdaptation;
            return chromaticAdaptation;
        }

        public double CalChromaCoverageRatio(POINT red_real, POINT green_real, POINT blue_real)
        {
            //总体思路：根据跟面积分布的点，然后计算点在三角形内的数据/总的点数即为大致的覆盖率，步长影响精度
            POINT red, green, blue;
            //不同色域下的红绿蓝的点位置不一致
            switch (m_spaceType)
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

            //先避免倾斜角为90°情况
            //理论上不存在倾斜角为90°
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
            return percent;
        }


        #endregion
    }
}
