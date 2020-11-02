using MathNet.Spatial.Euclidean;
using MySql.Data.MySqlClient;
using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace SmartRoadBridge.Public
{
    public static class PublicFun
    {
       

        public static string GetID(string line, double pK)
        {
            string Int = Math.Round(pK, 3, MidpointRounding.AwayFromZero).ToString("f3").PadLeft(9, '0');
            return line + string.Format("+{0}", Int);
        }


        public static List<double> GetNum(string inputtext,string sep)
        {
            var toX = Regex.Split(inputtext, sep);
            List<double> res = new List<double>();
            foreach (var item in toX)
            {
                res.Add(double.Parse(item));
            }

            return res;
        }
        static bool IsRectCross(Line L1, Line L2)
        {
            if (
                Math.Max(L1.StartPoint.X, L1.EndPoint.X) <= Math.Min(L2.StartPoint.X, L2.EndPoint.X) ||
                Math.Max(L2.StartPoint.X, L2.EndPoint.X) <= Math.Min(L1.StartPoint.X, L1.EndPoint.X) ||
                Math.Max(L1.StartPoint.Y, L1.EndPoint.Y) <= Math.Min(L2.StartPoint.Y, L2.EndPoint.Y) ||
                Math.Max(L2.StartPoint.Y, L2.EndPoint.Y) <= Math.Min(L1.StartPoint.Y, L1.EndPoint.Y)
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static double Length(this Line l)
        {
            return (l.StartPoint - l.EndPoint).Modulus();
        }
        public static Vector3 Product(this Vector3 v1, Vector3 v2)
        {
            double l = v1.X, m = v1.Y, n = v1.Z, o = v2.X, p = v2.Y, q = v2.Z;
            return new Vector3(m * q - n * p, n * o - l * q, l * p - m * o);
        }
        public static double Dot(this Vector3 v1, Vector3 v2)
        {
            double l = v1.X, m = v1.Y, n = v1.Z, o = v2.X, p = v2.Y, q = v2.Z;
            return l * o + m * p + n * q;
        }

        static bool IsSegmentCross(Line L1, Line L2)
        {
            Vector3 L1StL2St = L2.StartPoint - L1.StartPoint;
            Vector3 L1StL2Ed = L2.EndPoint - L1.StartPoint;
            Vector3 L2StL1St = L1.StartPoint - L2.StartPoint;
            Vector3 L2StL1Ed = L1.EndPoint - L2.StartPoint;
            Vector3 L1L1 = L1.EndPoint - L1.StartPoint;
            Vector3 L2L2 = L2.EndPoint - L2.StartPoint;

            if (L1StL2St.Product(L1L1).Dot(L1StL2Ed.Product(L1L1)) >= 0 ||
                L2StL1St.Product(L2L2).Dot(L2StL1Ed.Product(L2L2)) >= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 逆时针转动Rad度
        /// </summary>
        /// <param name="v"></param>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static Vector2 RotByZ(this Vector2 v, double rad)
        {
            double x1 = v.X * Math.Cos(rad) - v.Y * Math.Sin(rad);
            double y1 = v.X * Math.Sin(rad) + v.Y * Math.Cos(rad);
            return new Vector2(x1, y1);
        }
        public static Vector2 Convert2(this Vector2D vd)
        {
            return new Vector2(vd.X, vd.Y);
        }

        public static Vector2 Convert2D(this Vector3 vd)
        {
            return new Vector2(vd.X, vd.Y);
        }
        public static Vector3 Convert3D(this Vector2 vd)
        {
            return new Vector3(vd.X, vd.Y,0);
        }
        public static Vector2D Convert2DS(this Vector2 vd)
        {
            return new Vector2D(vd.X, vd.Y);
        }
        public static Vector3 SwapXY(this Vector3 vd)
        {
            return new Vector3(vd.Y, vd.X, 0);
        }

        public static Vector2 SwapXY(this Vector2 vd)
        {
            return new Vector2(vd.Y, vd.X);
        }

        public static Vector2? Intersectwith2(this Line master, Arc slaver)
        {
                        
            var verts = (from a in slaver.ToPolyline(60).Vertexes select a.Position).ToList();
            
            for (int i = 0; i < verts.Count-1; i++)
            {
                Line subLine = new Line(verts[i], verts[i + 1]).Flatten();
                var k= master.Intersectwith(subLine);
                if (k!=null)
                {
                    return k;
                }
            }
            return null;

        }

        public static Vector2? Intersectwith(this Line master, Arc slaver)
        {
            double[] res= IntersectSegCircle(slaver.Center.X, slaver.Center.Y, slaver.Radius, master.StartPoint.X, master.StartPoint.Y, master.EndPoint.X, master.EndPoint.Y);
            if (res==null || res.Count()==0)
            {
                return null;
            }
            else if (res.Count()==2)
            {
                Circle tmp = new Circle(slaver.Center, slaver.Radius);
                double IntPointAng = tmp.GetPointAngle(res[0], res[1]).Degrees;
                if (slaver.StartAngle <= slaver.EndAngle && (slaver.StartAngle <= IntPointAng && IntPointAng <= slaver.EndAngle))
                {
                    return new Vector2(res[0], res[1]);
                }
                else if (slaver.EndAngle<= slaver.StartAngle && !(slaver.EndAngle <= IntPointAng && IntPointAng <= slaver.StartAngle))
                {
                    return new Vector2(res[0], res[1]);
                }

            }
            else
            {
                Circle tmp = new Circle(slaver.Center, slaver.Radius);
                double IntPointAng = tmp.GetPointAngle(res[0], res[1]).Degrees;
                if (slaver.StartAngle <= slaver.EndAngle && (slaver.StartAngle <= IntPointAng && IntPointAng <= slaver.EndAngle))
                {
                    return new Vector2(res[0], res[1]);
                }
                else if (slaver.EndAngle <= slaver.StartAngle && !(slaver.EndAngle <= IntPointAng && IntPointAng <= slaver.StartAngle))
                {
                    return new Vector2(res[0], res[1]);
                }
                
                IntPointAng = tmp.GetPointAngle(res[2], res[3]).Degrees;
                if (slaver.StartAngle <= slaver.EndAngle && (slaver.StartAngle <= IntPointAng && IntPointAng <= slaver.EndAngle))
                {
                    return new Vector2(res[2], res[3]);
                }
                else if (slaver.EndAngle <= slaver.StartAngle && !(slaver.EndAngle <= IntPointAng && IntPointAng <= slaver.StartAngle))
                {
                    return new Vector2(res[2], res[3]);
                }
            }
            return null;
        }

        public static Vector2? Intersectwith2(this Line master, Circle slaver)
        {
            var verts = (from a in slaver.ToPolyline(360).Vertexes select a.Position).ToList();
            
            for (int i = 0; i < verts.Count - 1; i++)
            {
                Line subLine = new Line(verts[i], verts[i + 1]).Flatten();
                var k = master.Intersectwith(subLine);
                if (k != null)
                {
                    return k;
                }
            }
            return null;

        }

        public static Vector2? Intersectwith(this Line master, Circle slaver)
        {
            double[] res = IntersectSegCircle(slaver.Center.X, slaver.Center.Y, slaver.Radius, master.StartPoint.X, master.StartPoint.Y, master.EndPoint.X, master.EndPoint.Y);
            if (res == null)
            {
                return null;
            }
            else if (res.Count() == 2)
            {
                return new Vector2(res[0], res[1]);
            }
            return null;
        }


        public static Vector2? Intersectwith(this Line master, Line slaver)
        {

            if (!IsRectCross(master, slaver))
            {
                return null;
            }
            else if (!IsSegmentCross(master, slaver))
            {
                return null;
            }
            else
            {
                double x1 = master.StartPoint.X;
                double y1 = master.StartPoint.Y;
                double x2 = master.EndPoint.X;
                double y2 = master.EndPoint.Y;
                double x3 = slaver.StartPoint.X;
                double y3 = slaver.StartPoint.Y;
                double x4 = slaver.EndPoint.X;
                double y4 = slaver.EndPoint.Y;
                double b1 = (y2 - y1) * x1 + (x1 - x2) * y1;
                double b2 = (y4 - y3) * x3 + (x3 - x4) * y3;
                double D = (x2 - x1) * (y4 - y3) - (x4 - x3) * (y2 - y1);
                double D1 = b2 * (x2 - x1) - b1 * (x4 - x3);
                double D2 = b2 * (y2 - y1) - b1 * (y4 - y3);

                return new Vector2(D1 / D, D2 / D);

            }

        }

        public static Line Flatten(this Line l)
        {
            return new Line(l.StartPoint.Convert2D(), l.EndPoint.Convert2D());
        }

        public static Point3D Convert3DS(this Vector3 v,double x=0,double y=0,double z=0)
        {
            return new Point3D(v.X + x, v.Y + y, v.Z + z);

        }

        public static Point2D Convert2D(this Point3D v, double x = 0, double y = 0)
        {
            return new Point2D(v.X + x, v.Y + y);

        }


        public static Point3D Convert3D(this Point2D v, double x = 0, double y = 0, double z = 0)
        {
            return new Point3D(v.X + x, v.Y + y,  z);

        }
        public static string ToString2(this Point3D pt)
        {
            return string.Format("{0:F6},{1:F6},{2:F6}", pt.X, pt.Y, pt.Z);
        }
        public static string ToString2(this List<double> pt)
        {
            var st = (from a in pt select a.ToString()).ToList();
            string s = string.Join(",", st.ToArray());

            return s;
        }
        public static string ToString2(this List<string> pt)
        {
            
            string s = string.Join(",", pt.ToArray());

            return s;
        }

        static double[] IntersectSegCircle(double h,double k,double r,double x0,double y0,double x1,double y1)
        {
            double a = (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0);
            double b = 2 * (x1 - x0) * ((x0 - h)) + 2 * (y1 - y0) * (y0 - k);
            double c = (x0 - h) * (x0 - h) + (y0 - k) * (y0 - k) - r * r;
            double t1, t2;
            List<double> res = new List<double>();
            if (b * b - 4 * a * c<0)
            {
                return null;
            }
            else
            {
                t1 = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
                t2 = (-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
            }
            if (t1>=0&&t1<=1)
            {
                res.Add((x1 - x0) * t1 + x0);
                res.Add((y1 - y0) * t1 + y0);
            }
            if (t2>=0&&t2<=1)
            {
                res.Add((x1 - x0) * t2 + x0);
                res.Add((y1 - y0) * t2 + y0);
            }
            return res.ToArray();            
        }

        static MathNet.Spatial.Units.Angle GetPointAngle(this Circle cc, double x,double y)
        {
            Vector2D v1 = new Point2D(x, y ) - new Point2D(cc.Center.X, cc.Center.Y);
            Vector2D v0 = Vector2D.XAxis;
            return v0.SignedAngleTo(v1);
        }

        public static string GetSecName(string v)
        {
            List<string> SEC1 = new List<string>()
            {
                "A8L01","EBI01","JKI01","SBI05","MLI01","SEC101","SEC102","SEC103","SEC104","SGR01"
            };
            List<string> SEC2 = new List<string>()
            {
                "CCI01","CCI02","CCI03","CCI04","HSI01","HSI02","HSI03","HSI04","HSI05","HSI06",
                "MHI01","MHI02","MHI03","SEC201","SEC202","SEC203","SEC204L","SEC204R","SEC205","SEC206","SEC207","SEC208","SEC209","SEC210","WLI01",
            };
            foreach (var item in SEC1)
            {
                if (item.StartsWith(v,StringComparison.OrdinalIgnoreCase))
                {
                    return "SEC1";
                }
            }
            return "SEC2";
        }

        public static string GetOfficalBridgeName(string v)
        {
            if (v.StartsWith("SEC"))
            {
                return "ML" + v.Substring(4);
            }
            else
            {
                return v;
            }
        }

        public static DataTable SelectSQL(string selectString, ref MySqlConnection curConn)
        {

            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, curConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            return dataset.Tables[0];
        }

        public static void Info(int v, int count)
        {            
            int w = count.ToString().Length;
            string fz = v.ToString().PadLeft(w, '0');
            string strinfo = string.Format("{0}/{1}", fz, count);
            if (v!=1)
            {
                for (int k = 0; k < w*2+1; k++)
                {
                    Console.Write('\u0008');
                }            
            }
            Console.Write(strinfo);
        }
        public static DataRow[] Add(this DataRow[] A, DataRow[] B)
        {
            var x = A.ToList();
            foreach (var item in B)
            {                
                x.Add(item);
            }
            return x.ToArray();

        }

    }
}
