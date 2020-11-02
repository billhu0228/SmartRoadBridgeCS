using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartRoadBridge.Alignment
{

    public class JD
    {


        public double X, Y;
        public double PK;
        public double R1, R2, R;
        public double L1 { set; get; }
        public double L2 { set; get; }
        public double A1 { set; get; }
        public double A2 { set; get; }
        public double alpha,beta1,beta2;
        public double ak1, ak2;
        public LeftRightEnum Dir;
        public EnumTypeID TypeID;


        /// <summary>
        /// 三要素交点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="r"></param>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        public JD(double x, double y, double r, double l1, double l2, LeftRightEnum dir)
        {
            X = x;
            Y = y;
            R = r;
            L1 = l1;
            L2 = l2;
            R1 = 10000000000.0;
            R2 = 10000000000.0;
            Dir = dir;
            TypeID = EnumTypeID.JD3;
            ak1 = ((int)dir / R - 1 / R1) / L1;
            ak2 = ((int)dir / R - 1 / R2) / L2;
            A1 = 1 / Math.Sqrt(Math.Abs(ak1));
            A2 = 1 / Math.Sqrt(Math.Abs(ak2));
            beta1 = L1 / (2.0 * R);
            beta2 = L2 / (2.0 * R);
        }

        /// <summary>
        /// 起点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pk0"></param>
        public JD(double x, double y, double pk0 = 0)
        {
            if (pk0==0)
            {
                TypeID = EnumTypeID.ED;
            }
            else
            {
                TypeID = EnumTypeID.ST;
            }            
            X = x;
            Y = y;
            PK = pk0;
        }

        /// <summary>
        /// JD文件记录
        /// </summary>
        /// <param name="RcdLine"></param>
        public JD(string RcdLine)
        {
            var xx = Regex.Split(RcdLine, @"\s+");
            if (xx[0] == "3")
            {
                X = double.Parse(xx[1]);
                Y = double.Parse(xx[2]);
                if (xx[4].StartsWith("L"))
                {
                    L1 = double.Parse(xx[4].Remove(0, 1));
                    if (Math.Abs(L1-Math.Round(L1))<0.0001)
                    {
                        L1 = Math.Round(L1);
                    }
                }
                else
                {
                    A1 = double.Parse(xx[4]);
                }
                R = double.Parse(xx[5]);
                if (Math.Abs(R - Math.Round(R)) < 0.0001)
                {
                    R = Math.Round(R);
                }
                if (xx[6].StartsWith("L"))
                {
                    L2 = double.Parse(xx[6].Remove(0, 1));
                    if (Math.Abs(L2 - Math.Round(L2)) < 0.0001)
                    {
                        L2 = Math.Round(L2);
                    }
                }
                else
                {
                    A2 = double.Parse(xx[6]);
                }
                R1 = double.Parse(xx[3]);
                R2 = double.Parse(xx[7]);
            }

            beta1 = L1 / (2.0 * R);
            beta2 = L2 / (2.0 * R);

        }



        public double GetXAngleTo(JD TowardsJD)
        {
            var dy = TowardsJD.Y - Y;
            var dx = TowardsJD.X - X;
            var value = dy / dx;
            if (value>0)
            {
                if (dy >= 0)
                {
                    return Math.Atan(dy / dx);
                }
                else
                {
                    return Math.Atan(dy / dx)+Math.PI;
                }
            }
            else
            {
                if (dy >= 0)
                {
                    return Math.Atan(dy / dx)+Math.PI;
                }
                else
                {
                    return Math.Atan(dy / dx) + Math.PI*2;

                }
            }
     
            

        }
        public double DistTo(JD TowardsJD)
        {
            var dy = TowardsJD.Y - Y;
            var dx = TowardsJD.X - X;
            return Math.Sqrt(dy * dy + dx * dx);
        }
    }





}
