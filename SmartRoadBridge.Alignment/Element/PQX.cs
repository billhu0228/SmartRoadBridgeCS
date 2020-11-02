using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartRoadBridge.Alignment
{
    public class PQX
    {
        public PQX(string name)
        {
            Name = name;
            StartPK = 0;
            StartPoint = new Point2D();
            StartAngle = Angle.FromDegrees(0);
            ElemCollection = new List<PQXElement>();
        }

        public string Name { set; get; }
        public double StartPK { set; get; }
        public double EndPk { get { return StartPK + (ElemCollection.Count == 0 ? 0 : (from item in ElemCollection select item.Length).ToList().Sum()); } }
        public Point2D StartPoint;
        public Angle StartAngle;

        List<PQXElement> ElemCollection;



        #region 方法

        public void ReadICDFile(string[] altext)
        {
            // 初始化节点
            Point2D CurPoint = new Point2D();
            Angle CurAngle = Angle.FromDegrees(0);
            ElemCollection = new List<PQXElement>();
            for (int i = 0; i < altext.Count(); i++)
            {
                string line = altext[i];
                if (line == "")
                {
                    continue;
                }
                if (i == 0)
                {
                    StartPK = double.Parse(line);
                }
                else if (i == 1)
                {
                    var xx = Regex.Split(line, ",");
                    double StartX = double.Parse(xx[0]);
                    double StartY = double.Parse(xx[1]);
                    double StartAngInRad = double.Parse(xx[2]);

                    StartPoint = new Point2D(StartY, StartX);
                    StartAngle = Angle.FromRadians(StartAngInRad);
                    CurPoint = StartPoint;
                    CurAngle = StartAngle;
                }
                else
                {
                    var xx = Regex.Split(line, ",");
                    if (line.StartsWith("//"))
                    {
                        // 注释
                        continue;
                    }
                    else if (xx.Count() == 3 && int.Parse(xx[2]) == 0)
                    {
                        // 结束
                        break;
                    }
                    else
                    {
                        // 读取元素点
                        int TypeID;
                        double AngInDeg;
                        PQXElement item = null;
                        try
                        {
                            TypeID = int.Parse(xx[0]);
                        }
                        catch
                        {
                            throw new Exception();
                        }

                        if (TypeID == 1)
                        {
                            // 直线，长度，方位角
                            double ll = double.Parse(xx[1]);
                            if (xx.Count() == 3)
                            {
                                AngInDeg = double.Parse(xx[2]);
                            }
                            item = new Straight(ll, CurPoint, CurAngle);
                        }
                        else if (TypeID == 2)
                        {
                            double rr = double.Parse(xx[1]);
                            double ll = double.Parse(xx[2]);
                            LeftRightEnum sdir = int.Parse(xx[3]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;
                            item = new Arc(rr, ll, CurPoint, CurAngle, sdir);
                        }
                        else if (TypeID == 3)
                        {
                            EITypeID idd = (EITypeID)TypeID;
                            double aa = double.Parse(xx[1]);
                            double EndR = double.Parse(xx[2]);
                            LeftRightEnum sdir = int.Parse(xx[3]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;

                            item = new Spiral(idd, aa, double.MaxValue, EndR, CurPoint, CurAngle, sdir);

                        }
                        else if (TypeID == 4)
                        {
                            EITypeID idd = (EITypeID)TypeID;
                            double aa = double.Parse(xx[1]);
                            double StartR = double.Parse(xx[2]);
                            LeftRightEnum sdir = int.Parse(xx[3]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;
                            item = new Spiral(idd, aa, StartR, double.MaxValue, CurPoint, CurAngle, sdir);

                        }
                        else if (TypeID == 5 || TypeID == 6)
                        {
                            EITypeID idd = (EITypeID)TypeID;

                            double aa = double.Parse(xx[1]);
                            double StartR = double.Parse(xx[2]);
                            double EndR = double.Parse(xx[3]);
                            LeftRightEnum sdir = int.Parse(xx[4]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;
                            item = new Spiral(idd, aa, StartR, EndR, CurPoint, CurAngle, sdir);
                        }
                        else
                        {
                            throw new Exception("读取IDC文件错误!!");
                        }

                        ElemCollection.Add(item);
                        CurAngle = ElemCollection.Last().EndAngle;
                        CurPoint = ElemCollection.Last().EndPoint;
                    }
                }
            }
        }

        public void ReadICDFile(string icdfile)
        {
            // 初始化节点
            Point2D CurPoint = new Point2D();
            Angle CurAngle = Angle.FromDegrees(0);
            ElemCollection = new List<PQXElement>();
            string[] altext = File.ReadAllLines(icdfile);

            for (int i = 0; i < altext.Count(); i++)
            {
                string line = altext[i];
                if (i == 0)
                {
                    StartPK = double.Parse(line);
                }
                else if (i == 1)
                {
                    var xx = Regex.Split(line, ",");
                    double StartX = double.Parse(xx[0]);
                    double StartY = double.Parse(xx[1]);
                    double StartAngInRad = double.Parse(xx[2]);

                    StartPoint = new Point2D(StartY, StartX);
                    StartAngle = Angle.FromRadians(StartAngInRad);
                    CurPoint = StartPoint;
                    CurAngle = StartAngle;
                }
                else
                {
                    var xx = Regex.Split(line, ",");
                    if (line.StartsWith("//"))
                    {
                        // 注释
                        continue;
                    }
                    else if (xx.Count() == 3 && int.Parse(xx[2]) == 0)
                    {
                        // 结束
                        break;
                    }
                    else
                    {
                        // 读取元素点
                        int TypeID;
                        double AngInDeg;
                        PQXElement item = null;
                        try
                        {
                            TypeID = int.Parse(xx[0]);
                        }
                        catch
                        {
                            throw new Exception();
                        }

                        if (TypeID == 1)
                        {
                            // 直线，长度，方位角
                            double ll = double.Parse(xx[1]);
                            if (xx.Count() == 3)
                            {
                                AngInDeg = double.Parse(xx[2]);
                            }
                            item = new Straight(ll, CurPoint, CurAngle);
                        }
                        else if (TypeID == 2)
                        {
                            double rr = double.Parse(xx[1]);
                            double ll = double.Parse(xx[2]);
                            LeftRightEnum sdir = int.Parse(xx[3]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;
                            item = new Arc(rr, ll, CurPoint, CurAngle, sdir);
                        }
                        else if (TypeID == 3)
                        {
                            EITypeID idd = (EITypeID)TypeID;
                            double aa = double.Parse(xx[1]);
                            double EndR = double.Parse(xx[2]);
                            LeftRightEnum sdir = int.Parse(xx[3]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;

                            item = new Spiral(idd, aa, double.MaxValue, EndR, CurPoint, CurAngle, sdir);

                        }
                        else if (TypeID == 4)
                        {
                            EITypeID idd = (EITypeID)TypeID;
                            double aa = double.Parse(xx[1]);
                            double StartR = double.Parse(xx[2]);
                            LeftRightEnum sdir = int.Parse(xx[3]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;
                            item = new Spiral(idd, aa, StartR, double.MaxValue, CurPoint, CurAngle, sdir);

                        }
                        else if (TypeID == 5 || TypeID == 6)
                        {
                            EITypeID idd = (EITypeID)TypeID;

                            double aa = double.Parse(xx[1]);
                            double StartR = double.Parse(xx[2]);
                            double EndR = double.Parse(xx[3]);
                            LeftRightEnum sdir = int.Parse(xx[4]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;
                            item = new Spiral(idd, aa, StartR, EndR, CurPoint, CurAngle, sdir);
                        }
                        else
                        {
                            throw new Exception("读取IDC文件错误!!");
                        }

                        ElemCollection.Add(item);
                        CurAngle = ElemCollection.Last().EndAngle;
                        CurPoint = ElemCollection.Last().EndPoint;
                    }
                }
            }
        }
        public double[] GetDir(double pk, double det = 0.00001)
        {
            double x0 = GetCoord(pk - det)[0], y0 = GetCoord(pk - det)[1];
            double x1 = GetCoord(pk + det)[0], y1 = GetCoord(pk + det)[1];

            double ll = Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1));
            return new double[] { (x1 - x0) / ll, (y1 - y0) / ll };


        }
        /// <summary>
        /// 不用了
        /// </summary>
        /// <param name="pki"></param>
        /// <param name="delt"></param>
        /// <returns></returns>
        public Angle GetDirection(double pki, double delt = 1e-9)
        {
            if (pki == StartPK)
            {
                return StartAngle;
            }
            Point2D st, ed;
            try
            {
                st = new Point2D(GetCoord(pki - delt)[0], GetCoord(pki - delt)[1]);
            }
            catch (Exception)
            {
                st = new Point2D(GetCoord(pki)[0], GetCoord(pki)[1]);
            }
            try
            {
                ed = new Point2D(GetCoord(pki + delt)[0], GetCoord(pki + delt)[1]);
            }
            catch (Exception)
            {
                ed = new Point2D(GetCoord(pki)[0], GetCoord(pki)[1]);
            }

            return new LineSegment2D(st, ed).Direction.SignedAngleTo(Vector2D.YAxis);
        }

        public double[] GetCoord(double pkn)
        {

            if (pkn < StartPK)
            {
                return GetCoord(StartPK);
            }
            else if (pkn <= EndPk)
            {
                var tmp = (from item in ElemCollection select item.Length).ToList();
                List<double> LengthSumUp = new List<double>();
                LengthSumUp.Add(StartPK);
                foreach (var item in tmp)
                {
                    LengthSumUp.Add(LengthSumUp.Last() + item);
                }

                int aa = LengthSumUp.FindIndex((x) => x > pkn) - 1;

                double PrePK, NexPK;
                if (aa == -2)
                {
                    // 恰好等于终点里程
                    PrePK = LengthSumUp[LengthSumUp.Count - 2];
                    NexPK = LengthSumUp.Last();
                    aa = LengthSumUp.Count - 2;
                }
                else
                {
                    PrePK = LengthSumUp[aa];
                    NexPK = LengthSumUp[aa + 1];
                }
                double ll = pkn - PrePK;
                Point2D Res = ElemCollection[aa].GetPointOnCurve(ll);

                return new double[] { Res.X, Res.Y };



            }
            else
            {
                return GetCoord(EndPk);
            }
        }



        public double[] BinaryTest(double K1, double K2, Point2D Target, double delt)
        {
            double Kmid;
            do
            {
                var ret = SolveBetter(K1, K2, Target);
                K1 = ret[0];
                K2 = ret[1];
                Kmid = 0.5 * (K1 + K2);
            } while (Math.Abs(K1 - K2) > delt);
            double Res1 = Kmid;
            double Dist = SolveBetter(Res1, Res1, Target)[2];
            return new[] { Res1, Dist };
        }



        public double GetStationNew(double x, double y, int step = 20, double delt = 1e-9)
        {
            List<Tuple<double, double>> Res = new List<Tuple<double, double>>();
            double K1, K2;
            double DK = (EndPk - StartPK) / step;
            Point2D Target = new Point2D(x, y);
            for (int i = 0; i < step; i++)
            {
                K1 = StartPK + DK * i;
                K2 = K1 + DK;
                var r = BinaryTest(K1, K2, Target, delt);
                Tuple<double, double> ret = new Tuple<double, double>(r[0], r[1]);
                Res.Add(ret);
            }
            Res.Sort((xx, yy) => xx.Item2.CompareTo(yy.Item2));
            return Res[0].Item1;
        }

        /// <summary>
        /// 已废弃
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="delt"></param>
        /// <returns></returns>
        public double GetStation(double x, double y, double delt = 1e-9)
        {
            double K1 = StartPK, K2 = 0.5 * (StartPK + EndPk), Kmid = 0.5 * (K1 + K2);

            Point2D Target = new Point2D(x, y);
            //do
            //{
            //    var ret = SolveBetter(K1, K2, Target);
            //    K1 = ret[0];
            //    K2 = ret[1];
            //    Kmid = 0.5 * (K1 + K2);
            //} while (Math.Abs(K1-K2)>delt);
            double Res1 = BinaryTest(K1, K2, Target, delt)[0];

            K2 = EndPk;
            K1 = 0.5 * (StartPK + EndPk);
            //Kmid = 0.5 * (K1 + K2);            
            //do
            //{
            //    var ret = SolveBetter(K1, K2, Target);
            //    //var kk = SolveBetter(17490, 17479.68, Target);
            //    K1 = ret[0];
            //    K2 = ret[1];
            //    Kmid = 0.5 * (K1 + K2);
            //} while (Math.Abs(K1 - K2) > delt);
            //double Res2 = Kmid;
            double Res2 = BinaryTest(K1, K2, Target, delt)[0];

            K2 = Math.Max(Res1, Res2);
            K1 = Math.Min(Res1, Res2);
            double Res = BinaryTest(K1, K2, Target, delt)[0];
            return Res;
        }


        public double GetStation2(double x0, double y0, double x1, double y1, double offset = 0, double delt = 1e-9)
        {
            Point2D P0 = new Point2D(x0, y0);
            Point2D P1 = new Point2D(x1, y1);

            double K1 = StartPK, K2 = 0.5 * (StartPK + EndPk), Kmid = 0.5 * (K1 + K2);
            do
            {
                var ret = SolveBetter(K1, K2, P0, P1, offset);
                K1 = ret[0];
                K2 = ret[1];
                Kmid = 0.5 * (K1 + K2);
            } while (Math.Abs(K1 - K2) > delt);
            double Res1 = Kmid;

            K2 = EndPk;
            K1 = 0.5 * (StartPK + EndPk);
            Kmid = 0.5 * (K1 + K2);
            do
            {
                var ret = SolveBetter(K1, K2, P0, P1, offset);
                K1 = ret[0];
                K2 = ret[1];
                Kmid = 0.5 * (K1 + K2);
            } while (Math.Abs(K1 - K2) > delt);
            double Res2 = Kmid;

            K2 = Math.Max(Res1, Res2);
            K1 = Math.Min(Res1, Res2);
            Kmid = 0.5 * (K1 + K2);
            do
            {
                var ret = SolveBetter(K1, K2, P0, P1, offset);
                K1 = ret[0];
                K2 = ret[1];
                Kmid = 0.5 * (K1 + K2);
            } while (Math.Abs(K1 - K2) > delt);

            var tt = testBetter(Kmid, P0, P1);

            return Kmid;
        }

        public double GetStation(double x0, double y0, double x1, double y1, double offset = 0, double delt = 1e-9)
        {
            Point2D P0 = new Point2D(x0, y0);
            Point2D P1 = new Point2D(x1, y1);

            double K1 = StartPK, K2 = 0.5 * (StartPK + EndPk), Kmid = 0.5 * (K1 + K2);
            do
            {
                var ret = SolveBetter2(K1, K2, P0, P1, offset);
                K1 = ret[0];
                K2 = ret[1];
                Kmid = 0.5 * (K1 + K2);
            } while (Math.Abs(K1 - K2) > delt);
            double Res1 = Kmid;


            if (Math.Abs(testBetter(Res1, P0, P1)) < 1e-3)
            {
                return Res1;
            }

            K2 = EndPk;
            K1 = 0.5 * (StartPK + EndPk);
            Kmid = 0.5 * (K1 + K2);
            do
            {
                var ret = SolveBetter2(K1, K2, P0, P1, offset);
                K1 = ret[0];
                K2 = ret[1];
                Kmid = 0.5 * (K1 + K2);
            } while (Math.Abs(K1 - K2) > delt);
            double Res2 = Kmid;

            if (Math.Abs(testBetter(Res2, P0, P1)) < 1e-3)
            {
                return Res2;
            }

            K2 = Math.Max(Res1, Res2);
            K1 = Math.Min(Res1, Res2);
            Kmid = 0.5 * (K1 + K2);
            do
            {
                var ret = SolveBetter2(K1, K2, P0, P1, offset);
                K1 = ret[0];
                K2 = ret[1];
                Kmid = 0.5 * (K1 + K2);
            } while (Math.Abs(K1 - K2) > delt);

            var tt = testBetter(Kmid, P0, P1);

            if (Math.Abs(tt) > 1)
            {
                ;
            }

            return Kmid;
        }


        private double[] SolveBetter2(double k1, double k2, Point2D p0, Point2D p1, double offset = 0)
        {
            double km = 0.5 * (k1 + k2);
            Vector2D G = p1 - p0;
            Vector2D CDir1 = new Vector2D(GetDir(k1)[0], GetDir(k1)[1]).Rotate(Angle.FromDegrees(90));
            Vector2D CDir2 = new Vector2D(GetDir(k2)[0], GetDir(k2)[1]).Rotate(Angle.FromDegrees(90));
            Vector2D CDirM = new Vector2D(GetDir(km)[0], GetDir(km)[1]).Rotate(Angle.FromDegrees(90));
            Point2D p21 = new Point2D(GetCoord(k1)[0], GetCoord(k1)[1]) + CDir1 * offset;
            Point2D p22 = new Point2D(GetCoord(k2)[0], GetCoord(k2)[1]) + CDir2 * offset;
            Point2D p2m = new Point2D(GetCoord(km)[0], GetCoord(km)[1]) + CDirM * offset;
            Vector2D T1 = p21 - p0;
            Vector2D T2 = p22 - p0;
            Vector2D Tm = p2m - p0;

            Angle A1 = G.SignedAngleTo(T1);
            Angle A2 = G.SignedAngleTo(T2);
            Angle Am = G.SignedAngleTo(Tm);
            if (Math.Abs(A1.Degrees - G.AngleTo(T1).Degrees) > 1e-3)
            {
                A1 = -A1;
            }
            if (Math.Abs(A2.Degrees - G.AngleTo(T2).Degrees) > 1e-3)
            {
                A2 = A2 - Angle.FromDegrees(360.0);
            }
            if (Math.Abs(Am.Degrees - G.AngleTo(Tm).Degrees) > 1e-3)
            {
                Am = Am - Angle.FromDegrees(360.0);
            }
            if (Am.Degrees * A1.Degrees > 0)
            {
                return new double[] { km, k2 };
            }
            else
            {
                return new double[] { k1, km };
            }
        }

        private double[] SolveBetter(double k1, double k2, Point2D p0, Point2D p1, double offset = 0)
        {
            Vector2D G = p1 - p0;
            Vector2D CDir1 = new Vector2D(GetDir(k1)[0], GetDir(k1)[1]).Rotate(Angle.FromDegrees(90));
            Vector2D CDir2 = new Vector2D(GetDir(k2)[0], GetDir(k2)[1]).Rotate(Angle.FromDegrees(90));
            Point2D p21 = new Point2D(GetCoord(k1)[0], GetCoord(k1)[1]) + CDir1 * offset;
            Point2D p22 = new Point2D(GetCoord(k2)[0], GetCoord(k2)[1]) + CDir2 * offset;
            Vector2D T1 = p21 - p0;
            Vector2D T2 = p22 - p0;

            if (T1.DotProduct(T2) == 0)
            {
                throw new Exception("重合点无法求解");
            }

            if (Math.Abs(G.CrossProduct(T1)) < Math.Abs(G.CrossProduct(T2)))
            {
                return new[] { k1, 0.5 * (k1 + k2) };
            }
            else
            {
                return new[] { 0.5 * (k1 + k2), k2 };
            }
        }


        private double testBetter(double k1, Point2D p0, Point2D p1)
        {
            Vector2D G = p1 - p0;
            Point2D p21 = new Point2D(GetCoord(k1)[0], GetCoord(k1)[1]);
            Vector2D T1 = p21 - p0;

            return Math.Abs(G.CrossProduct(T1));
        }

        /// <summary>
        /// 哪个里程跟p更近
        /// </summary>
        /// <param name="k1"></param>
        /// <param name="k2"></param>
        /// <param name="p0"></param>
        /// <returns></returns>
        private double[] SolveBetter(double k1, double k2, Point2D p0)
        {
            Point2D p1 = new Point2D(GetCoord(k1)[0], GetCoord(k1)[1]);
            Point2D p2 = new Point2D(GetCoord(k2)[0], GetCoord(k2)[1]);
            if (p1.DistanceTo(p0) < p2.DistanceTo(p0))
            {
                return new[] { k1, 0.5 * (k1 + k2), p1.DistanceTo(p0) };
            }
            else
            {
                return new[] { 0.5 * (k1 + k2), k2, p2.DistanceTo(p0) };
            }

        }

        public Point2D GetPoint2D(double pk, double offset = 0.0)
        {
            Vector2D CDir = GetCrossDirVector2D(pk);
            return new Point2D(GetCoord(pk)[0], GetCoord(pk)[1]) + CDir * offset;
        }

        public Vector2D GetCrossDirVector2D(double pk)
        {
            return GetDirVector2D(pk).Rotate(Angle.FromDegrees(90.0));
        }

        public Vector2D GetDirVector2D(double pk)
        {
            return new Vector2D(GetDir(pk)[0], GetDir(pk)[1]);
        }
        public int GetSide(double x0,double y0)
        {
            double pk_center = GetStationNew(x0, y0);
            Point2D center = new Point2D(GetCoord(pk_center)[0], GetCoord(pk_center)[1]);
            Vector2D v_dir = GetDirVector2D(pk_center);
            Vector2D v_cross = new Point2D(x0, y0) - center;
            if (v_cross.Length == 0)
            {
                return 0;
            }
            else
            {
                Angle AA = v_dir.SignedAngleTo(v_cross);
                if (AA.Degrees < 180)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }

        #endregion
    }
}
