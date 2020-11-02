using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartRoadBridge.Alignment
{
    public class PQX
    {
        public JD StartPoint;
        public JD EndPoint;
        public List<JD> JDList;
        public List<Tuple<int,EnumKP, double,double,double>> PKList;

        public PQX(string jdfile)
        {
            // 初始化节点
            JDList = new List<JD>();            
            string[] altext = File.ReadAllLines(jdfile);
            foreach (string line in altext)
            {
                if (line.StartsWith("//"))
                {
                    continue;
                }
                var xx = Regex.Split(line, @"\s+");
                if (xx[0] == "-1")
                {
                    StartPoint = new JD(double.Parse(xx[1]), double.Parse(xx[2]), double.Parse(xx[3]));
                    StartPoint.TypeID = EnumTypeID.ST;
                }
                else if (xx[0] == "-2")
                {
                    EndPoint = new JD(double.Parse(xx[1]), double.Parse(xx[2]));
                    EndPoint.TypeID = EnumTypeID.ED;
                }
                else if (xx[0] == "3")
                {
                    JD a = new JD(line);
                    a.TypeID = EnumTypeID.JD3;
                    JDList.Add(a);
                }
                else
                {
                    throw new Exception();
                }
            }
            // 方向角
            for (int i = 0; i < JDList.Count; i++)
            {
                double v1, v2;
                if (i==0)
                {
                    v1 = StartPoint.GetXAngleTo(JDList[i]);
                    v2 = JDList[i].GetXAngleTo(JDList[i + 1]);
                }
                else if (i == JDList.Count - 1)
                {
                    v1 = JDList[i - 1].GetXAngleTo(JDList[i]);
                    v2 = JDList[i].GetXAngleTo(EndPoint);
                }
                else
                {
                    v1 = JDList[i - 1].GetXAngleTo(JDList[i]);
                    v2 = JDList[i].GetXAngleTo(JDList[i + 1]);
                }
                double aa = (v2 - v1);
                JDList[i].alpha = aa;
                JDList[i].Dir = aa > 0 ? LeftRightEnum.Left :LeftRightEnum.Right;
                JDList[i].ak1 = ((int)JDList[i].Dir / JDList[i].R - 1 / JDList[i].R1) / JDList[i].L1;
                JDList[i].ak2 = ((int)JDList[i].Dir / JDList[i].R - 1 / JDList[i].R2) / JDList[i].L2;

            }                
            // 积木法里程
            ScanTheLine();
        }

        public double[] GetCoord(double pk0)
        {
            // 插值
            double x0, y0;
            double x1, y1;
            double xres, yres;

            if (PKList.Count==0)
            {

                if (pk0 < StartPoint.PK || pk0 > StartPoint.DistTo(EndPoint)+StartPoint.PK)
                {
                    throw new ArgumentOutOfRangeException("里程不在设计范围内");
                }
                else
                {
                    double dd = pk0 - StartPoint.PK;
                    double LL = EndPoint.DistTo(StartPoint);
                    xres = StartPoint.X + dd / LL * (EndPoint.X - StartPoint.X);
                    yres = StartPoint.Y + dd / LL * (EndPoint.Y - StartPoint.Y);
                }
                return new double[] { xres, yres };
            }



            else if (pk0 < PKList.First().Item3 || pk0 > PKList.Last().Item3)
            {
                throw new ArgumentOutOfRangeException("里程不在设计范围内");
            }
            else if (PKList.Exists(x => x.Item3 == pk0))
            {
                var ff = from a in PKList select a.Item3;
                var tmp = ff.ToList();
                int kk = tmp.IndexOf(pk0);
                xres = PKList[kk].Item4;
                yres = PKList[kk].Item5;
            }
            else
            {
                var ff = from a in PKList select a.Item3;
                var tmp = ff.ToList();
                tmp.Add(pk0);
                tmp.Sort((x, y) => x.CompareTo(y));
                int kk = tmp.IndexOf(pk0);
                var before = PKList[kk - 1];
                var after = PKList[kk];

                x0 = before.Item4;
                y0 = before.Item5;
                x1 = after.Item4;
                y1 = after.Item5;

                JD A, B;

                double L0 = pk0 - before.Item3;

                if (before.Item2 == EnumKP.QD)
                {
                    var vec = Vector(StartPoint, JDList[after.Item1]);
                    double dd = pk0 - before.Item3;
                    xres = x0 + vec[0] * dd;
                    yres = y0 + vec[1] * dd;
                }
                else if (before.Item2 == EnumKP.ZH)
                {
                    if (before.Item1==0)
                    {
                        A = StartPoint;
                        B = JDList[before.Item1];
                    }
                    else
                    {
                        A = JDList[before.Item1 - 1];
                        B = JDList[before.Item1];
                    }
                    var vec = Vector(A, B);

                    double uf = XSeries(B.R, L0, B.ak1);
                    double vf = YSeries(B.R, L0, B.ak1);
                    xres = x0 + uf * vec[0] + vf * NomralVec(vec, B.Dir)[0];
                    yres = y0 + uf * vec[1] + vf * NomralVec(vec, B.Dir)[1];
                    
                }
                else if (before.Item2 == EnumKP.HY)
                {
                    if (before.Item1 == 0)
                    {
                        A = StartPoint;
                        B = JDList[before.Item1];
                    }
                    else
                    {
                        A = JDList[before.Item1 - 1];
                        B = JDList[before.Item1];
                    }
                    var vec = Vector(A, B);
                    vec = RotVec(vec,(int)B.Dir* B.beta1);
                    double uf = B.R * Math.Sin(L0 / B.R);
                    double vf = B.R - B.R * Math.Cos(L0 / B.R);
                    xres = x0 + uf * vec[0] + vf * NomralVec(vec, B.Dir)[0];
                    yres = y0 + uf * vec[1] + vf * NomralVec(vec, B.Dir)[1];

                }
                else if (before.Item2 == EnumKP.YH)
                {

                    if (before.Item1 == JDList.Count-1)
                    {                        
                        A = JDList[before.Item1];
                        B = EndPoint;
                    }
                    else
                    {
                        A = JDList[before.Item1];
                        B = JDList[before.Item1+1];
                    }

                    var vec = Vector(A, B);

                    double uf = XSeries(A.R, A.L2-L0, A.ak2);
                    double vf = YSeries(A.R, A.L2-L0, A.ak2);
                    xres = x1 - uf * vec[0] + vf * NomralVec(vec, A.Dir)[0];
                    yres = y1 - uf * vec[1] + vf * NomralVec(vec, A.Dir)[1];
                }
                else
                {
                    if (before.Item1 == JDList.Count - 1)
                    {
                        A = JDList[before.Item1];
                        B = EndPoint;
                    }
                    else
                    {
                        A = JDList[before.Item1];
                        B = JDList[before.Item1 + 1];
                    }

                    var vec = Vector(A, B);
                    double dd = pk0 - before.Item3;
                    xres = x0 + vec[0] * dd;
                    yres = y0 + vec[1] * dd;
                }
            }
            return new double[] { xres,yres };
        }

        /// <summary>
        /// 主店清单
        /// </summary>
        void ScanTheLine()
        {            
            PKList = new List<Tuple<int,EnumKP, double,double,double>>();
            Tuple<int,EnumKP, double,double,double> cp;
            JD beforeJD,curJD,afterJD;

            if (JDList.Count == 0) { return; }

            // 准备起步数据

            double x0 = StartPoint.X;
            double y0 = StartPoint.Y;            
            double pk0 = StartPoint.PK;
            double pkZH, pkHY, pkYH, pkHZ;
            double Ts1,Ts2,pTs2=0,nTs1=0;
            double q1, q2, p1, p2,np1,nq1;
            double dist1,dist2;


            
            cp = new Tuple<int,EnumKP, double,double,double>(0,EnumKP.QD, pk0, StartPoint.X, StartPoint.Y);
            PKList.Add(cp);
            
            for (int i = 0; i < JDList.Count; i++)
            {

                curJD = JDList[i];
                beforeJD = i == 0 ? StartPoint : JDList[i - 1];
                afterJD = i == JDList.Count - 1 ? EndPoint : JDList[i + 1];
                q1 = QSeries(curJD.R, curJD.L1);
                p1 = PSeries(curJD.R, curJD.L1);
                Ts1 = (curJD.R + p1) * Math.Tan(Math.Abs(curJD.alpha) / 2) + q1;
                q2 = QSeries(curJD.R, curJD.L2);
                p2 = PSeries(curJD.R, curJD.L2);
                Ts2 = (curJD.R + p2) * Math.Tan(Math.Abs(curJD.alpha) / 2) + q2;

                if (i!=JDList.Count-1)
                {
                    nq1 = QSeries(afterJD.R, afterJD.L1);
                    np1 = PSeries(afterJD.R, afterJD.L1);
                    nTs1 = (afterJD.R + np1) * Math.Tan(Math.Abs(afterJD.alpha) / 2) + nq1;
                }
                else
                {
                    nTs1 =0;
                }
                                             

                var V1 = Vector(beforeJD, curJD);
                var V2 = Vector(curJD, afterJD);
                dist1 = curJD.DistTo(beforeJD);
                dist2 = curJD.DistTo(afterJD);
                pkZH = pk0 + dist1 - Ts1 - pTs2;
                pkHY = pkZH + curJD.L1;
                pkYH = pkHY + curJD.R * (Math.Abs(curJD.alpha) - curJD.beta1 - curJD.beta2);
                pkHZ = pkYH + curJD.L2;

                double xZH=x0+V1[0] * (dist1 - Ts1 - pTs2);
                double yZH = y0 + V1[1] * (dist1 - Ts1 - pTs2);
                double uf = XSeries(curJD.R, curJD.L1, curJD.ak1);
                double vf = YSeries(curJD.R, curJD.L1, curJD.ak1);
                double xHY = xZH + uf * V1[0] + vf * NomralVec(V1, curJD.Dir)[0];
                double yHY = yZH + uf * V1[1] + vf * NomralVec(V1, curJD.Dir)[1];


                double xHZ = afterJD.X - V2[0] * (dist2 - Ts2);
                double yHZ = afterJD.Y - V2[1] * (dist2 - Ts2);
                uf = XSeries(curJD.R, curJD.L2, curJD.ak2);
                vf = YSeries(curJD.R, curJD.L2, curJD.ak2);
                double xYH = xHZ - uf * V2[0] + vf * NomralVec(V2, curJD.Dir)[0];
                double yYH = yHZ - uf * V2[1] + vf * NomralVec(V2, curJD.Dir)[1];

                cp = new Tuple<int, EnumKP, double, double, double>(i, EnumKP.ZH, pkZH, xZH, yZH);
                PKList.Add(cp);
                cp = new Tuple<int, EnumKP, double, double, double>(i, EnumKP.HY, pkHY, xHY, yHY);
                PKList.Add(cp);                                             
                cp = new Tuple<int, EnumKP, double, double, double>(i, EnumKP.YH, pkYH, xYH,yYH);
                PKList.Add(cp);
                cp = new Tuple<int, EnumKP, double, double, double>(i,EnumKP.HZ, pkHZ,xHZ ,yHZ);
                PKList.Add(cp);

                x0 = afterJD.X - V2[0] * (dist2 - Ts2);
                y0 = afterJD.Y - V2[1] * (dist2 - Ts2);
                pk0 = pkHZ;
                pTs2 = Ts2;

            }
            var V = Vector(JDList.Last(), EndPoint);
            var dist = EndPoint.DistTo(JDList.Last()) - pTs2;
            cp = new Tuple<int, EnumKP, double, double, double>(JDList.Count-1,EnumKP.ZD, pk0+dist, x0 + V[0] * (dist), y0 + V[1] * (dist));
            PKList.Add(cp);

       
        }



        double[] RotVec(double[] v,double rad)
        {
            double x1 = v[0] * Math.Cos(rad) - v[1] * Math.Sin(rad);
            double y1 = v[0] * Math.Sin(rad) + v[1] * Math.Cos(rad);
            return new double[] { x1, y1 };
        }



        double[] NomralVec(double[] V, LeftRightEnum dir)
        {
            if (dir == LeftRightEnum.Left)
            {
                return new double[] { -V[1], V[0] };
            }
            else
            {
                return new double[] { V[1], -V[0] };
            }

        }


        double[] Vector(JD i, JD j, double factor = 1)
        {
            double dx = j.X - i.X;
            double dy = j.Y - i.Y;
            double dist = i.DistTo(j);
            return new double[] { dx / dist*factor, dy / dist*factor };
        }


        public static int Fab(int n)
        {
            return (n > 1) ? (n * Fab(n - 1)) : ((n < 0) ? 0 : 1);
        }

        public static double XSeries(double R, double Ls,double a, int n = 2)
        {
            double fz, fm;
            double res = 0;
            for (int i = 0; i <= n; i++)
            {
                fz = Math.Pow(-1, i) *Math.Pow(a,2*i)* Math.Pow(Ls, 4 * i + 1);
                fm = Fab(2 * i) * (4 * i + 1) * Math.Pow(2, 2 * i);
                res += fz / fm;
            }
            return Math.Abs(res);
        }
        public static double YSeries(double R, double Ls, double a, int n = 2)
        {
            double fz, fm;
            double res = 0;
            for (int i = 0; i <= n; i++)
            {
                fz = Math.Pow(-1, i) * Math.Pow(a, 2 * i + 1) * Math.Pow(Ls, 4 * i + 3);
                fm = Fab(2 * i + 1) * (4 * i + 3) * Math.Pow(2, 2 * i + 1);
                res += fz / fm;
            }
            return Math.Abs(res);
        }

        public static double PSeries(double R,double Ls,int n=2)
        {
            double fz,fm;
            double res=0;
            for (int i = 0; i <= n; i++)
            {
                fz = Math.Pow(-1, i) * Math.Pow(Ls, 2 * i + 2);
                fm = Fab(2 * i + 2) * (4 * i + 3) * Math.Pow(2, 2 * i + 2) * Math.Pow(R, 2 * i + 1);
                res += fz / fm;
            }
            return res;        
        }
        public static double QSeries(double R,double Ls,int n = 2)
        {
            double fz, fm;
            double res = 0;
            for (int i = 0; i <= n; i++)
            {
                fz = Math.Pow(-1, i) * Math.Pow(Ls, 2 * i + 1);
                fm = Fab(2 * i + 1) * (4 * i + 1) * Math.Pow(2, 2 * i + 1) * Math.Pow(R, 2 * i );
                res += fz / fm;
            }
            return res;
        }


    }
    
}
