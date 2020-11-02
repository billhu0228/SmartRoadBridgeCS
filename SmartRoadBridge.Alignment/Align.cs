using MathNet.Spatial.Euclidean;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SmartRoadBridge.Alignment
{
    public class Align
    {
        public string Name;
        public string WorkDir;
        public PQX curPQX;
        public DMX curDMX;
        public SQX curSQX;
        public CG curCG;
        public HDX curHDX;


        public Align(string name, string ICDtext, string SQXtext, string DMXtext, string CGtext = null, bool isReadVert = true)
        {

            Name = name;
            curPQX = new PQX(Name + "_PQX");
            curPQX.ReadICDFile(ICDtext);

            if (isReadVert)
            {
                curDMX = new DMX(Regex.Split(ICDtext, "\\s+"));
                curSQX = new SQX(Regex.Split(SQXtext, "\\s+"));
                curCG = new CG(Regex.Split(DMXtext, "\\s+"));
            }

            WorkDir = "";

        }

        public Align(string name, string[] ICDtext, string[] SQXtext, string[] DMXtext, string[] CGtext = null, string[] HDXtext = null, bool isReadVert = true)
        {



            Name = name;
            curPQX = new PQX(Name + "_PQX");
            curPQX.ReadICDFile(ICDtext);

            if (isReadVert)
            {
                curDMX = new DMX(DMXtext);
                curSQX = new SQX(SQXtext);
                if (CGtext!=null)
                {
                    curCG = new CG(CGtext);
                }
                if (HDXtext!=null)
                {
                    curHDX = new HDX(HDXtext);
                }
            }

            WorkDir = "";

        }

        public Align(string workDir, bool isReadVert = true)
        {

            var dd = new DirectoryInfo(workDir);
            Name = dd.Name;

            curPQX = new PQX(Name + "_PQX");
            curPQX.ReadICDFile(dd.FullName + "\\" + Name + ".ICD");
            if (isReadVert)
            {
                curDMX = new DMX(dd.FullName + "\\" + Name + ".DMX");
                curSQX = new SQX(dd.FullName + "\\" + Name + ".SQX");
                curCG = new CG(dd.FullName + "\\" + Name + ".CG");
                curHDX = new HDX(dd.FullName + "\\" + Name + ".HDX");
            }

            WorkDir = workDir;

        }

        public double GetSurfaceBG(double x, double y)
        {
            double st = curPQX.GetStationNew(x, y);
            double x0 = curPQX.GetCoord(st)[0];
            double y0 = curPQX.GetCoord(st)[1];
            Vector2D Cdir = new Vector2D(x - x0, y - y0);
            Vector2D dir = new Vector2D(curPQX.GetDir(st)[0], curPQX.GetDir(st)[1]);
            var HP = curCG.GetHP(st);
            double a = Cdir.SignedAngleTo(dir).Degrees;
            double CC = 0;
            if (a < 180)
            {
                CC = curSQX.GetBG(st) + (Cdir.Length) * HP[1] * 0.01;
            }
            else
            {
                CC = curSQX.GetBG(st) - (Cdir.Length) * HP[0] * 0.01;

            }
            return CC;

        }


        public double GetSurfaceDist(double x, double y)
        {
            double st = curPQX.GetStationNew(x, y);
            double x0 = curPQX.GetCoord(st)[0];
            double y0 = curPQX.GetCoord(st)[1];
            Vector2D Cdir = new Vector2D(x - x0, y - y0);
            return Cdir.Length;

        }


        /// <summary>
        /// 根据桥面坐标求所在点横坡
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double GetSurfaceHP(double x, double y)
        {
            //double st = curPQX.GetStation(x, y);
            double st = curPQX.GetStationNew(x, y, 20);
            double x0 = curPQX.GetCoord(st)[0];
            double y0 = curPQX.GetCoord(st)[1];
            Vector2D dir = new Vector2D(curPQX.GetDir(st)[0], curPQX.GetDir(st)[1]);
            Vector2D Cdir = new Vector2D(x - x0, y - y0);
            var HP = curCG.GetHP(st);
            double a = Cdir.SignedAngleTo(dir).Degrees;
            if (a < 180)
            {
                return HP[1];
            }
            else
            {
                return HP[0];
            }
        }

        /// <summary>
        /// 获取地面高程
        /// </summary>
        /// <param name="pk">里程</param>
        /// <param name="x0">斜距，向右为正</param>
        /// <returns></returns>
        public double GetGroundBG(double pk, double x0)
        {
            if (curHDX.Method==HDXMethod.None)
            {
                Debug.WriteLine("\n{0}无横地线信息.",this.Name);
                return this.curDMX.GetBG(pk);
            }
            else if (curHDX.Method==HDXMethod.ABS)
            {
                return this.curHDX.GetdBG(pk, x0);

            }
            else
            {
                return this.curDMX.GetBG(pk) + this.curHDX.GetdBG(pk, x0);
            }
            
        }

        public double GetGroundBGByCoord(double x0,double y0)
        {
            double[] loc = new double[] { x0, y0 };
            double pk = this.curPQX.GetStationNew(loc[0], loc[1]);
            Point2D cc =new Point2D(this.curPQX.GetCoord(pk)[0], this.curPQX.GetCoord(pk)[1]);
            Point2D pt = new Point2D(loc[0], loc[1]);
            int f = this.curPQX.GetSide(loc[0], loc[1]);
            return GetGroundBG(pk, f * cc.DistanceTo(pt));
        }


    }
}
