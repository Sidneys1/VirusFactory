using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

// ReSharper disable InconsistentNaming
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace FortuneVoronoi {

    public abstract class MathTools {

        /// <summary>
        /// One static Random instance for use in the entire application
        /// </summary>
        public static readonly Random R = new Random((int)DateTime.Now.Ticks);

        public static double Dist(double x1, double y1, double x2, double y2) {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        public static IList Shuffle(IList s, Random rand, bool copy) {
            //			if(S.Rank>1)
            //				throw new Exception("Shuffle only defined on one-dimensional arrays!");
            var e = s;
            if (copy) {
                var cloneable = s as ICloneable;
                if (cloneable != null)
                    e = cloneable.Clone() as IList;
                else
                    throw new Exception("You want it copied, but it can't!");
            }
            int i;
            Debug.Assert(e != null, "e != null");
            for (i = 0; i < e.Count - 1; i++) {
                var r = i + rand.Next(e.Count - i);
                if (r == i)
                    continue;
                var temp = e[i];
                e[i] = e[r];
                e[r] = temp;
            }
            return e;
        }

        public static void ShuffleIList(IList a, Random r) {
            Shuffle(a, r, false);
        }

        public static void ShuffleIList(IList a) {
            Shuffle(a, new Random((int)DateTime.Now.Ticks), false);
        }

        public static IList Shuffle(IList a, bool copy) {
            return Shuffle(a, new Random((int)DateTime.Now.Ticks), copy);
        }

        public static IList Shuffle(IList a) {
            return Shuffle(a, new Random((int)DateTime.Now.Ticks), true);
        }

        public static int[] GetIntArrayRange(int a, int b) {
            var e = new int[b - a + 1];
            int i;
            for (i = a; i <= b; i++)
                e[i - a] = i;
            return e;
        }

        public static int[] GetIntArrayConst(int a, int n) {
            var e = new int[n];
            int i;
            for (i = 0; i < n; i++)
                e[i] = a;
            return e;
        }

        public static int[] GetIntArray(params int[] p) {
            return p;
        }

        public static object[] GetArray(params object[] p) {
            return p;
        }

        public static Array CopyToArray(ICollection l, Type T) {
            var erg = Array.CreateInstance(T, l.Count);
            l.CopyTo(erg, 0);
            return erg;
        }

        public static string[] HighLevelSplit(string s, params char[] chars) {
            var erg = new ArrayList();
            var currentBracket = new Stack();
            var pos = 0;
            int i;

            for (i = 0; i < s.Length; i++) {
                switch (s[i]) {
                    case '(':
                        currentBracket.Push(0);
                        continue;
                    case '[':
                        currentBracket.Push(1);
                        continue;
                    case '{':
                        currentBracket.Push(2);
                        continue;
                    case ')':
                        if ((int)currentBracket.Pop() != 0)
                            throw new Exception("Formatfehler!");
                        continue;
                    case ']':
                        if ((int)currentBracket.Pop() != 1)
                            throw new Exception("Formatfehler!");
                        continue;
                    case '}':
                        if ((int)currentBracket.Pop() != 2)
                            throw new Exception("Formatfehler!");
                        continue;
                }
                if (currentBracket.Count > 0)
                    continue;
                var c = Array.IndexOf(chars, s[i]);
                if (c == -1) continue;
                if (chars[c] == '\n') {
                    if (i - 2 >= pos)
                        erg.Add(s.Substring(pos, i - pos - 1));
                    pos = i + 1;
                } else {
                    if (i - 1 >= pos)
                        erg.Add(s.Substring(pos, i - pos));
                    pos = i + 1;
                }
            }
            if (currentBracket.Count > 0)
                throw new Exception("Formatfehler!");
            if (i - 1 >= pos)
                erg.Add(s.Substring(pos, i - pos));
            return (string[])CopyToArray(erg, typeof(string));
        }

        public static RectangleF MaxRectangleFit(RectangleF target, SizeF source) {
            // 1. Auf höhe probieren
            var h = target.Height;
            var w = target.Height / source.Height * source.Width;
            if (w <= target.Width) {
                return new RectangleF(target.X + target.Width / 2 - w / 2, target.Y, w, h);
            }
            // 2. Auf weite probieren
            w = target.Width;
            h = target.Width / source.Width * source.Height;
            return new RectangleF(target.X, target.Y + target.Height / 2 - h / 2, w, h);
        }

        public static double DaSkalar(double[] a, double[] b) {
            if (a.Length != b.Length)
                throw new Exception("Error in Skalar!");
            double e = 0;
            int i;
            for (i = 0; i < a.Length; i++) {
                e += a[i] * b[i];
            }
            return e;
        }

        public static double[] DaMult(double[] a, double r) {
            var e = new double[a.Length];
            int i;
            for (i = 0; i < e.Length; i++) {
                e[i] = a[i] * r;
            }
            return e;
        }

        public static double[] DaAdd(double[] a, double[] b) {
            if (a.Length != b.Length)
                throw new Exception("Error in Skalar!");
            var e = new double[a.Length];
            int i;
            for (i = 0; i < a.Length; i++) {
                e[i] += a[i] + b[i];
            }
            return e;
        }

        public static double DaDist(double[] a, double[] b) {
            if (a.Length != b.Length)
                throw new Exception("Unterschiedliche Längen!");
            int i;
            double e = 0;
            for (i = 0; i < a.Length; i++)
                e += (a[i] - b[i]) * (a[i] - b[i]);
            return e;
        }

        public static double DaMean(double[] a) {
            return a.Sum() / a.Length;
        }

        public static double DaStdv(double[] a, double m) {
            var erg = a.Sum(d => (m - d) * (m - d));
            return erg / a.Length;
        }

        /* 0: minimum, +: rising, -: falling, 1: maximum. */

        private static readonly char[][] HsbMap = {new[]{'1', '+', '0'},
                                            new[]{'-', '1', '0'},
                                            new[]{'0', '1', '+'},
                                            new[]{'0', '-', '1'},
                                            new[]{'+', '0', '1'},
                                            new[]{'1', '0', '-'}};

        public static double[] HSBtoRGB(int hue, int saturation, int brightness, double[] oldCol) {
            /* Clip hue at 360: */
            if (hue < 0)
                hue = 360 - (-hue % 360);
            hue = hue % 360;

            int i = (int)Math.Floor(hue / 60.0), j;
            double[] c;
            if (oldCol == null || oldCol.Length != 3)
                c = new double[3];
            else
                c = oldCol;

            var min = 127.0 * (240.0 - saturation) / 240.0;
            var max = 255.0 - 127.0 * (240.0 - saturation) / 240.0;
            if (brightness > 120) {
                min = min + (255.0 - min) * (brightness - 120) / 120.0;
                max = max + (255.0 - max) * (brightness - 120) / 120.0;
            }
            if (brightness < 120) {
                min = min * brightness / 120.0;
                max = max * brightness / 120.0;
            }

            for (j = 0; j < 3; j++) {
                switch (HsbMap[i][j]) {
                    case '0':
                        c[j] = min;
                        break;

                    case '1':
                        c[j] = max;
                        break;

                    case '+':
                        c[j] = (min + (hue % 60) / 60.0 * (max - min));
                        break;

                    case '-':
                        c[j] = (max - (hue % 60) / 60.0 * (max - min));
                        break;
                }
            }
            return c;
        }

        public static Color HSBtoRGB(int hue, int saturation, int brightness) {
            var c = HSBtoRGB(hue, saturation, brightness, null);
            return Color.FromArgb((int)c[0], (int)c[1], (int)c[2]);
        }

        public static double GetAngle(double x, double y) {
            if (x == 0) {
                if (y > 0)
                    return Math.PI / 2.0;
                if (y == 0)
                    return 0;
                if (y < 0)
                    return Math.PI * 3.0 / 2.0;
            }
            var atan = Math.Atan(y / x);
            if (x > 0 && y >= 0)
                return atan;
            if (x > 0 && y < 0)
                return 2 * Math.PI + atan;
            return Math.PI + atan;
        }

        public static double GetAngleTheta(double x, double y) {
            var dx = x; var ax = Math.Abs(dx);
            var dy = y; var ay = Math.Abs(dy);
            var t = (ax + ay == 0) ? 0 : dy / (ax + ay);
            if (dx < 0) t = 2 - t; else if (dy < 0) t = 4 + t;
            return t * 90.0;
        }

        public static int ccw(Point p0, Point p1, Point p2, bool plusOneOnZeroDegrees) {
            var dx1 = p1.X - p0.X; var dy1 = p1.Y - p0.Y;
            var dx2 = p2.X - p0.X; var dy2 = p2.Y - p0.Y;
            if (dx1 * dy2 > dy1 * dx2) return +1;
            if (dx1 * dy2 < dy1 * dx2) return -1;
            if ((dx1 * dx2 < 0) || (dy1 * dy2 < 0)) return -1;
            if ((dx1 * dx1 + dy1 * dy1) < (dx2 * dx2 + dy2 * dy2) && plusOneOnZeroDegrees)
                return +1;
            return 0;
        }

        public static int ccw(double p0X, double p0Y, double p1X, double p1Y, double p2X, double p2Y, bool plusOneOnZeroDegrees) {
            var dx1 = p1X - p0X; var dy1 = p1Y - p0Y;
            var dx2 = p2X - p0X; var dy2 = p2Y - p0Y;
            if (dx1 * dy2 > dy1 * dx2) return +1;
            if (dx1 * dy2 < dy1 * dx2) return -1;
            if ((dx1 * dx2 < 0) || (dy1 * dy2 < 0)) return -1;
            if ((dx1 * dx1 + dy1 * dy1) < (dx2 * dx2 + dy2 * dy2) && plusOneOnZeroDegrees)
                return +1;
            return 0;
        }

        public static bool Intersect(Point p11, Point p12, Point p21, Point p22) {
            return ccw(p11, p12, p21, true) * ccw(p11, p12, p22, true) <= 0
                && ccw(p21, p22, p11, true) * ccw(p21, p22, p12, true) <= 0;
        }

        public static PointF IntersectionPoint(Point p11, Point p12, Point p21, Point p22) {
            double kx = p11.X, ky = p11.Y, mx = p21.X, my = p21.Y;
            double lx = (p12.X - p11.X), ly = (p12.Y - p11.Y), nx = (p22.X - p21.X), ny = (p22.Y - p21.Y);
            double a = double.NaN, b = double.NaN;
            if (lx == 0) {
                if (nx == 0)
                    throw new Exception("No intersect!");
                b = (kx - mx) / nx;
            } else if (ly == 0) {
                if (ny == 0)
                    throw new Exception("No intersect!");
                b = (ky - my) / ny;
            } else if (nx == 0) {
                if (lx == 0)
                    throw new Exception("No intersect!");
                a = (mx - kx) / lx;
            } else if (ny == 0) {
                if (ly == 0)
                    throw new Exception("No intersect!");
                a = (my - ky) / ly;
            } else {
                b = (ky + mx * ly / lx - kx * ly / lx - my) / (ny - nx * ly / lx);
            }
            if (!double.IsNaN(a)) {
                return new PointF((float)(kx + a * lx), (float)(ky + a * ly));
            }
            if (!double.IsNaN(b)) {
                return new PointF((float)(mx + b * nx), (float)(my + b * ny));
            }
            throw new Exception("Error in IntersectionPoint");
        }
    }
}