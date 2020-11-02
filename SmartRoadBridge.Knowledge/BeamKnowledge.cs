using netDxf.Entities;
using SmartRoadBridge.Public;
using SmartRoadBridge.Structure;
using System;
using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using System.Linq;
using netDxf;
using SmartRoadBridge.Alignment;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace SmartRoadBridge.Knowledge
{

    public static class BeamKnowledge
    {
        public static double BeamH = 1.6;
        public static double BearingTotalH = 0.3;
        public static double SurfaceH = 0.17;
        private static double NormalSide=1.65;
        private static double SpecialSide = 1.2;
        /// <summary>
        /// 连续墩支座高度
        /// </summary>
        public static double BearingAH=0.099;

        /// <summary>
        /// 过渡墩支座高度
        /// </summary>
        public static double BearingBH = 0.102;
        public static double BearingPadH=0.05;

        //public static double SupTotalH { get { return BeamH + SurfaceH + BearingTotalH; } }



        public static List<int> AvilibleBeamNum(double Width, double Ang,double BeamWdith, double Min, double Max, double Side, out double SideReal)
        {

            double LFactor = 1.0/Math.Cos(Ang / 180.0 * Math.PI);
            SideReal = Side * LFactor;
            double BeamWdithReal = BeamWdith * LFactor;
            double MinReal = Min * LFactor;
            double MaxReal = Max * LFactor;            

            List<int> res = new List<int>();

            for (int k = 1; k < 12; k++)
            {
                double va = (Width - 2 * SideReal) / (k - 1) - BeamWdithReal;
                if (va >= MinReal && va <= MaxReal)
                {
                    res.Add(k);
                }
            }
            return res;
        }



        public static List<int> AvilibleBeamNum(double Width,double BeamWdith=2.4,double Min=0.4,double Max=1.0,double Side = 1.65)
        {


            List<int> res = new List<int>();

            for (int k = 1; k < 12; k++)
            {
                double va = (Width -2* Side) / (k-1)- BeamWdith;
                if (va>=Min&& va<=Max)
                {
                    res.Add(k);
                }
            }
      
            return res;
        }


        /// <summary>
        /// 平面布梁函数
        /// </summary>
        /// <param name="StStation">起点桩号</param>
        /// <param name="EdStation">终点桩号</param>
        /// <param name="StLines">起点布梁线组，从左到右</param>
        /// <param name="EdLines">终点布梁线组，从左到右</param>
        /// <param name="StartC2C">起点桩号-支座线偏移</param>
        /// <param name="EndC2C">终点点桩号-支座线偏移</param>
        /// <returns>该跨梁表</returns>
        public static List<BoxBeam> ArrangePlan2(double StStation, double EdStation,double StW,double EdW,
            Line[] StLines, Line[] EdLines, double StartC2C, double EndC2C,double StartAng,double EndAng,
            ref Align StAlign, ref Align EdAlign)
        {
            if (StStation == 402)
            {
               ;
            }

            List<BoxBeam> res = new List<BoxBeam>();

            int BeamNum = 0;
            int[] BeamNums = null;
            double DeltSt = 0, DeltEd = 0;
            double[] DeltEds;
            double[] DeltSts;
            if (StLines.Count() == 1 && EdLines.Count() == 1)
            {
                // 普通起终点
                double SideNominal = NormalSide;                
                double SideSt = SideNominal; 
                double SideEd = SideNominal;
                double StartA = Math.Abs(StartAng - 90.0);
                double EndA = Math.Abs(EndAng - 90.0);
                Line Stline = StLines[0];
                Line EdLine = EdLines[0];
                if (StStation == 402)
                {
                    // MUB 调整加宽 
                    Vector3 Vst = Stline.Direction;
                    Vector3 Ved = EdLine.Direction;
                    Vst.Normalize();
                    Ved.Normalize();
                    Stline.StartPoint = Stline.EndPoint + Vst * -8.9;
                    EdLine.StartPoint = EdLine.EndPoint + Ved * -8.9;
                }
                if (StStation == 432.5)
                {
                    // MUB 调整加宽 
                    Vector3 Vst = Stline.Direction;
                    Vector3 Ved = EdLine.Direction;
                    Vst.Normalize();
                    Ved.Normalize();
                    Stline.StartPoint = Stline.EndPoint + Vst * -8.9;
                    EdLine.StartPoint = EdLine.EndPoint + Ved * -8.9;
                }
                bool NoNumApp = true;
                double StLineLength = StLines[0].Length();
                double EdLineLength = EdLines[0].Length();
              
                var Num1 = AvilibleBeamNum(StLineLength, StartA, 2.4, 0.35, 1, SideNominal,out SideSt);
                var Num2 = AvilibleBeamNum(EdLineLength, EndA, 2.4, 0.35, 1, SideNominal, out SideEd);
                var Num = (from a in Num1 where Num2.Contains(a) select a).ToList();
                NoNumApp = Num.Count == 0;
                if (NoNumApp)
                {
                    SideNominal = NormalSide;
                    Num1 = AvilibleBeamNum(StLineLength, StartA, 2.4, 0.35, 1.1, SideNominal, out SideSt);
                    Num2 = AvilibleBeamNum(EdLineLength, EndA, 2.4, 0.35, 1.1, SideNominal, out SideEd);
                    Num = (from a in Num1 where Num2.Contains(a) select a).ToList();
                    NoNumApp = Num.Count == 0;
                }

                if (NoNumApp)
                {
                    SideNominal = NormalSide;
                    Num1 = AvilibleBeamNum(StLineLength, StartA, 2.4, 0.35, 1.2, SideNominal, out SideSt);
                    Num2 = AvilibleBeamNum(EdLineLength, EndA, 2.4, 0.35, 1.2, SideNominal, out SideEd);
                    Num = (from a in Num1 where Num2.Contains(a) select a).ToList();
                    NoNumApp = Num.Count == 0;
                }

                if (NoNumApp)
                {
                    SideNominal = SpecialSide;
                    Num1 = AvilibleBeamNum(StLineLength, StartA, 2.4, 0.4, 1, SideNominal, out SideSt);
                    Num2 = AvilibleBeamNum(EdLineLength, EndA, 2.4, 0.4, 1, SideNominal, out SideEd);
                    Num = (from a in Num1 where Num2.Contains(a) select a).ToList();
                    NoNumApp = Num.Count == 0;
                }

                if (NoNumApp)
                {
                    SideNominal = SpecialSide;
                    Num1 = AvilibleBeamNum(StLineLength, StartA, 2.4, 0.4, 1.1, SideNominal, out SideSt);
                    Num2 = AvilibleBeamNum(EdLineLength, EndA, 2.4, 0.4, 1.1, SideNominal, out SideEd);
                    Num = (from a in Num1 where Num2.Contains(a) select a).ToList();
                    NoNumApp = Num.Count == 0;
                }


                if (NoNumApp)
                {
                    SideNominal = SpecialSide;
                    Num1 = AvilibleBeamNum(StLineLength, StartA, 2.4, 0.3, 1.2, SideNominal, out SideSt);
                    Num2 = AvilibleBeamNum(EdLineLength, EndA, 2.4, 0.3, 1.2, SideNominal, out SideEd);
                    Num = (from a in Num1 where Num2.Contains(a) select a).ToList();
                    NoNumApp = Num.Count == 0;
                }

                if (NoNumApp)
                {
                    SideNominal = SpecialSide;
                    Num1 = AvilibleBeamNum(StLineLength, StartA, 2.4, 0.25, 1.2, SideNominal, out SideSt);
                    Num2 = AvilibleBeamNum(EdLineLength, EndA, 2.4, 0.25, 1.2, SideNominal, out SideEd);
                    Num = (from a in Num1 where Num2.Contains(a) select a).ToList();
                    NoNumApp = Num.Count == 0;
                }
                if (NoNumApp)
                {
                    SideNominal = NormalSide;
                    Num1 = AvilibleBeamNum(StLineLength, StartA, 2.4, 0.4, 1.4, SideNominal, out SideSt);
                    Num2 = AvilibleBeamNum(EdLineLength, EndA, 2.4, 0.4, 1.4, SideNominal, out SideEd);
                    Num = (from a in Num1 where Num2.Contains(a) select a).ToList();
                    NoNumApp = Num.Count == 0;
                }
                if (NoNumApp)
                {
                    SideNominal = NormalSide;
                    Num1 = AvilibleBeamNum(StLineLength, StartA, 2.4, 0.4, 1.45, SideNominal, out SideSt);
                    Num2 = AvilibleBeamNum(EdLineLength, EndA, 2.4, 0.4, 1.45, SideNominal, out SideEd);
                    Num = (from a in Num1 where Num2.Contains(a) select a).ToList();
                    NoNumApp = Num.Count == 0;
                }

                if (NoNumApp)
                {
                    SideNominal = NormalSide;
                    Num1 = AvilibleBeamNum(StLineLength, StartA, 2.4, 0.3, 1.45, SideNominal, out SideSt);
                    Num2 = AvilibleBeamNum(EdLineLength, EndA, 2.4, 0.3, 1.45, SideNominal, out SideEd);
                    Num = (from a in Num1 where Num2.Contains(a) select a).ToList();
                    NoNumApp = Num.Count == 0;
                }

                if (NoNumApp)
                {
                    SideNominal = NormalSide;
                    Num1 = AvilibleBeamNum(StLineLength, StartA, 2.4, 0.38, 1.7, SideNominal, out SideSt);
                    Num2 = AvilibleBeamNum(EdLineLength, EndA, 2.4, 0.38, 1.7, SideNominal, out SideEd);
                    Num = (from a in Num1 where Num2.Contains(a) select a).ToList();
                    NoNumApp = Num.Count == 0;
                }
                if (!NoNumApp)
                {
                    BeamNum = Num.Min();
                    DeltSt = (StLineLength - 2 * SideSt) / (BeamNum - 1);
                    DeltEd = (EdLineLength - 2 * SideEd) / (BeamNum - 1);

                    AppendBeam(ref res, Stline, EdLine, BeamNum, DeltSt, DeltEd, StartC2C, EndC2C, SideSt, SideEd, ref StAlign,ref EdAlign);
                }
            }         
            else
            {
                // 错误
                throw new Exception("布置参数错误");
            }

            for (int i = 0; i < res.Count; i++)
            {
                BoxBeam bb = res[i];
                bb.ID =i+1;                
            }


            return res;
        }
        //private static void AppendBeam(ref List<BoxBeam> res, Line LineStart, Line[] LinesEnd,int[] bmNums, double deltSt, double[] deltEd, double StartC2C, double EndC2C, 
        //    double[] Side)
        //{
        //    BoxBeam cc;
        //    Vector2 stend=new Vector2(), edend=new Vector2();
        //    Vector2 stpin, edpin;
        //    Vector2 Cdir0 = LineStart.Direction.Convert2D();
        //    Cdir0.Normalize();
        //    Vector2 Cdir1 = LinesEnd[0].Direction.Convert2D();
        //    Cdir1.Normalize();
        //    Vector2 BeamDir;
        //    Vector2D BeamAxis;
        //    for (int i = 0; i < bmNums[0]; i++)
        //    {
        //        stend = LineStart.StartPoint.Convert2D() + Cdir0 * Side[0] + Cdir0 * (i) * deltSt;
        //        edend = LinesEnd[0].StartPoint.Convert2D() + Cdir1 * Side[0] + Cdir1 * (i) * deltEd[0];

        //        BeamDir = edend - stend;
        //        BeamDir.Normalize();

        //        stpin = stend + BeamDir * StartC2C;
        //        edpin = edend - BeamDir * EndC2C;

        //        BeamAxis = (edpin - stpin).Convert2DS();

        //        double startA = 90 - Cdir0.Convert2DS().SignedAngleTo(BeamAxis).Degrees;
        //        double endA = 90 - Cdir1.Convert2DS().SignedAngleTo(BeamAxis).Degrees;

        //        cc = new BoxBeam(stpin.Convert3D().Convert3DS(0, 0, 0), edpin.Convert3D().Convert3DS(0, 0, 0), 0, 0, 0.5, 0.5, startA, endA, false, 0);
        //        cc.ID = i;
        //        res.Add(cc);
        //    }
        //    for (int i = 0; i < bmNums[1]; i++)
        //    {

        //        stend = stend + Cdir0 * deltSt;
        //        edend = LinesEnd[1].StartPoint.Convert2D() + Cdir1 * Side[2] + Cdir1 * (i) * deltEd[1];

        //        BeamDir = edend - stend;
        //        BeamDir.Normalize();

        //        stpin = stend + BeamDir * StartC2C;
        //        edpin = edend - BeamDir * EndC2C;

        //        BeamAxis = (edpin - stpin).Convert2DS();

        //        double startA = 90 - Cdir0.Convert2DS().SignedAngleTo(BeamAxis).Degrees;
        //        double endA = 90 - Cdir1.Convert2DS().SignedAngleTo(BeamAxis).Degrees;

        //        cc = new BoxBeam(stpin.Convert3D().Convert3DS(0, 0, 0), edpin.Convert3D().Convert3DS(0, 0, 0), 0, 0, 0.5, 0.5, startA, endA, false, 0);
        //        cc.ID = i;
        //        res.Add(cc);
        //    }

        //}

        /// <summary>
        /// 向列表添加一孔梁
        /// </summary>
        /// <param name="res">列表</param>
        /// <param name="LineStart">起点线</param>
        /// <param name="LineEnd">终点线</param>
        /// <param name="beamNum">梁片数</param>
        /// <param name="deltSt">起点间距</param>
        /// <param name="deltEd">终点间距</param>
        /// <param name="StartC2C">起点支座跨径距</param>
        /// <param name="EndC2C">终点支座跨径距</param>
        /// <param name="Side">边梁边距</param>
        /// <param name="StartCL">起点设计线</param>
        /// <param name="EndCL">终点设计线</param>
        private static void AppendBeam(ref List<BoxBeam> res, Line LineStart, Line LineEnd,
            int beamNum, double deltSt, double deltEd,double StartC2C,double EndC2C,double SideSt,double SideEd,
            ref Align StartCL,ref Align EndCL
            )
        {
            BoxBeam cc;
            Vector2 stend, edend;
            Vector2 stpin, edpin;

            for (int i = 0; i < beamNum; i++)
            {
                Vector2 Cdir0 = LineStart.Direction.Convert2D();
                Cdir0.Normalize();
                Vector2 Cdir1 = LineEnd.Direction.Convert2D();
                Cdir1.Normalize();

                stend = LineStart.StartPoint.Convert2D() + Cdir0 * SideSt + Cdir0 * (i) * deltSt;
                edend = LineEnd.StartPoint.Convert2D() + Cdir1 * SideEd + Cdir1 * (i) * deltEd;

                Vector2 BeamDir = edend - stend;
                BeamDir.Normalize();

                Vector2D BeamAxis = (edend - stend).Convert2DS();
                double startA = 90 - Cdir0.Convert2DS().SignedAngleTo(BeamAxis).Degrees;
                double endA = 90 - Cdir1.Convert2DS().SignedAngleTo(BeamAxis).Degrees;

                stpin = stend + BeamDir * StartC2C / Math.Cos((startA) / 180.0 * Math.PI);
                edpin = edend - BeamDir * EndC2C / Math.Cos((endA) / 180.0 * Math.PI);

                //var VecSt = (stpin.Convert2DS() - new Vector2D(Left0.StartPoint.X, Left0.StartPoint.Y));
                //var VecEd = (edpin.Convert2DS() - new Vector2D(Left1.StartPoint.X, Left1.StartPoint.Y));

                //增加桥面横坡值
                double StartDeckHp = StartCL.GetSurfaceHP(stend.X, stend.Y);
                double EndDeckHp = EndCL.GetSurfaceHP(edend.X, edend.Y);
                
                double StPK = StartCL.curPQX.GetStationNew(stend.X, stend.Y);
                double EdPK = EndCL.curPQX.GetStationNew(edend.X, edend.Y);

                if (StartCL.curCG.GetHP(StPK)[0]==2.5&& StartCL.curCG.GetHP(StPK)[1] == -2.5)
                {
                    if (StartCL.GetSurfaceDist(stend.X, stend.Y)<=0.5)
                    {
                        StartDeckHp = 0.0;
                    }
                }
                if (EndCL.curCG.GetHP(EdPK)[0] == 2.5 && EndCL.curCG.GetHP(EdPK)[1] == -2.5)
                {
                    if (EndCL.GetSurfaceDist(edend.X, edend.Y) <= 0.5)
                    {
                        EndDeckHp = 0.0;
                    }
                }
                double NominalHP = 0.5 * (StartDeckHp + EndDeckHp);

                //

                cc = new BoxBeam(stpin.Convert3D().Convert3DS(0, 0, 0), edpin.Convert3D().Convert3DS(0, 0, 0),
                    0, 0, 0.5, 0.5, startA, endA, false, NominalHP);
                cc.ID = i;

                #region 判断梁类别编号
                string typestr = "BE";
                if (cc.Length2D<=20)
                {
                    typestr += "05";
                }
                else if (cc.Length2D <= 27.0)
                {
                    typestr += "01";
                }
                else if (cc.Length2D <= 31.5)
                {
                    if (Math.Abs(startA)>=29)
                    {
                        typestr += "03";
                    }
                    else
                    {
                        typestr += "02";
                    }
                    
                }
                else if (cc.Length2D <= 38.0)
                {
                    typestr += "04";
                }
                else 
                {
                    typestr += "99";
                }
                if (i == 0 || i == beamNum - 1)
                {
                    if (Math.Max(SideSt,SideEd) < NormalSide)
                    {
                        typestr += "D";
                    }
                    else
                    {
                        typestr += "A";
                    }
                }
                else 
                {
                    if (beamNum>=7)
                    {
                        if (Math.Abs(NominalHP)<=1.0)
                        {
                            typestr += "C";
                        }
                        else
                        {
                            typestr += "B";
                        }
                    }
                    else
                    {
                        typestr += "B";
                    }
                }
                cc.BeamType = typestr;
                #endregion
                res.Add(cc);
            }
            var tst = (from a in res select a.BeamType.Substring(0, 4)).ToList().Distinct().ToList();
            if (tst.Count!=1 && tst.Contains("BE04"))
            {
                for (int i = 0; i < res.Count; i++)
                {
                    string part = res[i].BeamType.Substring(2, 2);
                    res[i].BeamType = res[i].BeamType.Replace(part, "04");

                }
            }


            // 增加s型加强02梁
            if (res[0].BeamType[3]=='2')
            {                
                if (res.Count==3)
                {
                    res[1].BeamType = "BE02Bs";
                }
                else if (res.Count>= 4)
                {
                    if (Math.Max(deltEd,deltSt)>2.4+0.85)
                    {
                        res[1].BeamType = "BE02Bs";
                        res[res.Count - 2].BeamType = "BE02Bs";
                    }
                }
            }
            // 增加s型加强01梁
            else if (res[0].BeamType[3] == '1')
            {
                if (res.Count == 3)
                {
                    res[1].BeamType = "BE01Bs";
                }
                else if (res.Count >= 4)
                {
                    if (Math.Max(deltEd, deltSt) > 2.4 + 0.85)
                    {
                        res[1].BeamType = "BE01Bs";
                        res[res.Count - 2].BeamType = "BE01Bs";
                    }
                }

            }


        }

        public static List<BoxBeam> ArrangePlan(double StartStation, double EndStation,
            Line Left0, Line Right0, Line Left1, Line Right1, ref Align StAlign, ref Align EdAlign, double StartC2C, double EndC2C,
            bool isStPub,bool isEdPub, double Side = 1.65)
        {
      
            if (StartStation == 19645)
            {
                ;
            }
            List<BoxBeam> res = new List<BoxBeam>();
            if (isStPub || isEdPub)
            {
                return res;

            }
            Line LineStart = new Line(Left0.EndPoint, Right0.EndPoint);
            Line LineEnd = new Line(Left1.EndPoint, Right1.EndPoint);
            var xx = LineEnd.Length();
            if ((StAlign.Name.StartsWith("R1K") || StAlign.Name.StartsWith("L1K")) || (EdAlign.Name.StartsWith("R1K") || EdAlign.Name.StartsWith("L1K")))
            {
                Side = 1.2;
            }

            var Num1 = AvilibleBeamNum(LineStart.Length(), 2.4, 0.4, 1, Side);
            var Num2 = AvilibleBeamNum(LineEnd.Length(), 2.4, 0.4, 1, Side);
            var tmp = (from a in Num1 where Num2.Contains(a) select a).ToList();

            if (tmp.Count == 0)
            {
                Num1 = AvilibleBeamNum(LineStart.Length(), 2.4, 0.3, 1.1, Side);
                Num2 = AvilibleBeamNum(LineEnd.Length(), 2.4, 0.3, 1.1, Side);
                tmp = (from a in Num1 where Num2.Contains(a) select a).ToList();
            }


            if (tmp.Count == 0)
            {
                Num1 = AvilibleBeamNum(LineStart.Length(), 2.4, 0.3, 1.35, Side);
                Num2 = AvilibleBeamNum(LineEnd.Length(), 2.4, 0.3, 1.35, Side);
                tmp = (from a in Num1 where Num2.Contains(a) select a).ToList();
            }

            int aa;

            double delt0 = 0;// = (LineStart.Length() - 2 * Side) / (Num1.Max() - 1);
            double delt1 = 0;// = (LineEnd.Length() - 2 * Side) / (Num2.Max() - 1);

            if (Num1.Count == 0 || Num2.Count == 0)
            {
                return res;
            }
            if (tmp.Count == 0)
            {
                if (Num1.Max() > Num2.Max())
                {
                    // 匝道分出
                    if ((Left0.Length() - Left1.Length()) > (Right0.Length() - Right1.Length()))
                    {
                        // 左侧分出
                        LineStart = new Line(Right0.EndPoint, Left0.EndPoint);
                        LineEnd = new Line(Right1.EndPoint, Left1.EndPoint);
                    }
                    aa = Num2.Max();

                    delt0 = (LineStart.Length() - 2 * Side) / (aa + 2);
                    delt1 = (LineEnd.Length() - 2 * Side) / (aa - 1);
                }
                else
                {
                    // 匝道汇入
                    if ((Left1.Length() - Left0.Length()) > (Right1.Length() - Right0.Length()))
                    {
                        // 左侧匝道汇入
                        LineStart = new Line(Right0.EndPoint, Left0.EndPoint);
                        LineEnd = new Line(Right1.EndPoint, Left1.EndPoint);
                    }
                    aa = Num1.Max();


                    delt0 = (LineStart.Length() - 2 * Side) / (aa - 1);
                    delt1 = (LineEnd.Length() - 2 * Side) / (aa + 2);
                }

            }
            else
            {
                aa = tmp.Max();
                delt0 = (LineStart.Length() - 2 * Side) / (aa - 1);
                delt1 = (LineEnd.Length() - 2 * Side) / (aa - 1);
            }

            BoxBeam cc;
            Vector2 stend, edend;
            Vector2 stpin, edpin;

            for (int i = 0; i < aa; i++)
            {                
                Vector2 Cdir0 = LineStart.Direction.Convert2D();
                Cdir0.Normalize();
                Vector2 Cdir1 = LineEnd.Direction.Convert2D();
                Cdir1.Normalize();

                stend = LineStart.StartPoint.Convert2D() + Cdir0 * Side + Cdir0 * (i) * delt0;
                edend = LineEnd.StartPoint.Convert2D() + Cdir1 * Side + Cdir1 * (i) * delt1;

                Vector2 BeamDir = edend - stend;
                BeamDir.Normalize();

                stpin = stend + BeamDir * StartC2C;
                edpin = edend - BeamDir * EndC2C;
                


                var VecSt = (stpin.Convert2DS() - new Vector2D(Left0.StartPoint.X, Left0.StartPoint.Y));
                var VecEd = (edpin.Convert2DS() - new Vector2D(Left1.StartPoint.X, Left1.StartPoint.Y));

                
                Vector2D BeamAxis = (edpin - stpin).Convert2DS();

                double startA = 90 - Cdir0.Convert2DS().SignedAngleTo(BeamAxis).Degrees;
                double endA = 90 - Cdir1.Convert2DS().SignedAngleTo(BeamAxis).Degrees;

                cc = new BoxBeam(stpin.Convert3D().Convert3DS(0, 0, 0), edpin.Convert3D().Convert3DS(0, 0, 0), 0, 0, 0.5, 0.5, startA, endA, false, 0);
                cc.ID = i;
                res.Add(cc);
            }
            return res;
        }

        public static List<BoxBeam> Arrange(double StartStation,double EndStation,
            Line Left0, Line Right0,Line Left1,Line Right1,ref Align StAlign,ref Align EdAlign, double StartC2C, double EndC2C,
            double Side=1.65,double BearingH=0.3,double BeamH=1.6,double SurfaceH=0.170)
        {

            if (StartStation==19645)
            {
                ;
            }
            List<BoxBeam> res = new List<BoxBeam>();

            Line LineStart = new Line(Left0.EndPoint, Right0.EndPoint);
            Line LineEnd = new Line(Left1.EndPoint, Right1.EndPoint);

            if( (StAlign.Name.StartsWith("R1K")|| StAlign.Name.StartsWith("L1K")) || (EdAlign.Name.StartsWith("R1K") || EdAlign.Name.StartsWith("L1K")))
            {
                Side = 1.2;
            }

            var Num1 = AvilibleBeamNum(LineStart.Length(), 2.4, 0.4, 1, Side);
            var Num2 = AvilibleBeamNum(LineEnd.Length(), 2.4, 0.4, 1, Side);
            var tmp = (from a in Num1 where Num2.Contains(a) select a).ToList();

            if (tmp.Count==0)
            {
                Num1 = AvilibleBeamNum(LineStart.Length(),2.4,0.3,1.1,Side);
                Num2 = AvilibleBeamNum(LineEnd.Length(),2.4,0.3,1.1,Side);
                tmp = (from a in Num1 where Num2.Contains(a) select a).ToList();
            }


            if (tmp.Count == 0)
            {
                Num1 = AvilibleBeamNum(LineStart.Length(), 2.4, 0.3, 1.35, Side);
                Num2 = AvilibleBeamNum(LineEnd.Length(), 2.4, 0.3, 1.35, Side);
                tmp = (from a in Num1 where Num2.Contains(a) select a).ToList();
            }

            int aa;

            double delt0 = 0;// = (LineStart.Length() - 2 * Side) / (Num1.Max() - 1);
            double delt1 = 0;// = (LineEnd.Length() - 2 * Side) / (Num2.Max() - 1);

            if (Num1.Count==0||Num2.Count==0)
            {
                return res;
            }
            if (tmp.Count == 0)
            {
                if (Num1.Max() > Num2.Max())
                {
                    // 匝道分出
                    if ((Left0.Length() - Left1.Length()) > (Right0.Length() - Right1.Length()))
                    {
                        // 左侧分出
                        LineStart = new Line(Right0.EndPoint, Left0.EndPoint);
                        LineEnd = new Line(Right1.EndPoint, Left1.EndPoint);
                    }
                    aa = Num2.Max();

                    delt0 = (LineStart.Length() - 2 * Side) / (aa+2);
                    delt1 = (LineEnd.Length() - 2 * Side) / (aa - 1);
                }
                else
                {
                    // 匝道汇入
                    if ((Left1.Length() - Left0.Length()) > (Right1.Length() - Right0.Length()))
                    {
                        // 左侧匝道汇入
                        LineStart = new Line(Right0.EndPoint, Left0.EndPoint);
                        LineEnd = new Line(Right1.EndPoint, Left1.EndPoint);
                    }
                    aa = Num1.Max();


                    delt0 = (LineStart.Length() - 2 * Side) / (aa - 1);
                    delt1 = (LineEnd.Length() - 2 * Side) / (aa+2);
                }

            }
            else
            {
                aa = tmp.Max();
                delt0 = (LineStart.Length() - 2 * Side) / (aa - 1);
                delt1 = (LineEnd.Length() - 2 * Side) / (aa - 1);
            }

            BoxBeam cc;
            Vector2 stend, edend;
            Vector2 stpin, edpin;

            double StandardH = SurfaceH + BeamH + BearingH;
            double H0_St = StAlign.curSQX.GetBG(StartStation) - StandardH;
            double H0_Ed = EdAlign.curSQX.GetBG(EndStation) - StandardH;
            List<double[]> HPListCB =new List<double[]>(){ StAlign.curCG.GetHP(StartStation), EdAlign.curCG.GetHP(EndStation) } ;
            double HLeftSt = H0_St - HPListCB[0][0] * Left0.Length()*0.01;
            double HLeftEd = H0_Ed - HPListCB[1][0] * Left1.Length() * 0.01;
            double slop;
            for (int i = 0; i < aa; i++)
            {
                bool isSide = i == 0 || i == aa - 1;
                Vector2 Cdir0 = LineStart.Direction.Convert2D();
                Cdir0.Normalize();
                Vector2 Cdir1 = LineEnd.Direction.Convert2D();
                Cdir1.Normalize();

                stend = LineStart.StartPoint.Convert2D() + Cdir0 * Side + Cdir0 * (i) * delt0;
                edend = LineEnd.StartPoint.Convert2D() + Cdir1 * Side + Cdir1 * (i) * delt1;

                Vector2 BeamDir = edend - stend;
                BeamDir.Normalize();

                stpin = stend + BeamDir * StartC2C;
                edpin = edend - BeamDir * EndC2C;

                double Hbot_st= StAlign.GetSurfaceBG(stpin.X, stpin.Y)-SurfaceH-BeamH;
                double Hbot_ed= EdAlign.GetSurfaceBG(edpin.X, edpin.Y)-SurfaceH-BeamH;
                double HCB_st = 0;
                double HCB_ed = 0;

                var VecSt = (stpin.Convert2DS()-new Vector2D(Left0.StartPoint.X, Left0.StartPoint.Y));
                var VecEd = (edpin.Convert2DS()-new Vector2D(Left1.StartPoint.X, Left1.StartPoint.Y) );
                Vector2D dirForwoard = new Vector2D(StAlign.curPQX.GetDir(StartStation)[0], StAlign.curPQX.GetDir(StartStation)[1]);
                if (VecSt.SignedAngleTo(dirForwoard).Degrees<=180)
                {
                    // 使用右坡
                    HCB_st = H0_St + HPListCB[0][1] * 0.01 * VecSt.Length;
                    slop = -2.5;
                }
                else
                {
                    // 左坡
                    HCB_st = H0_St - HPListCB[0][0] * 0.01 * VecSt.Length;
                    slop = 2.5;
                }
                if (aa%2==1 && i==(int)(0.5*aa))
                {
                    slop = 0;
                }
                dirForwoard = new Vector2D(EdAlign.curPQX.GetDir(EndStation)[0], EdAlign.curPQX.GetDir(EndStation)[1]);
                if (VecEd.SignedAngleTo(dirForwoard).Degrees <= 180)
                {
                    HCB_ed = H0_Ed + HPListCB[1][1] * 0.01 * VecEd.Length;
                }
                else
                {
                    HCB_ed = H0_Ed - HPListCB[1][0] * 0.01 * VecEd.Length;
                }
                Vector2D BeamAxis = (edpin - stpin).Convert2DS();

                double startA =90- Cdir0.Convert2DS().SignedAngleTo(BeamAxis).Degrees;
                double endA =90- Cdir1.Convert2DS().SignedAngleTo(BeamAxis).Degrees;
                
                cc = new BoxBeam(stpin.Convert3D().Convert3DS(0,0,Hbot_st), edpin.Convert3D().Convert3DS(0,0,Hbot_ed),
                    (Hbot_st-HCB_st), (Hbot_ed - HCB_ed),0.5,0.5,startA,endA,isSide,slop);

                //(int)(StartStation*100)+i
                cc.ID = (int)(StartStation * 100) + i;
                res.Add(cc);
            }

            


            return res;
        }

        public static void GetWetJointD(string name, ref MySqlConnection CurConn, out double d1, out double d2)
        {

            if (name== "SEC2/ML01/S01/G02")
            {
                ;
            }
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            int BeamID = int.Parse(name.Substring(name.Length - 2, 2));
            d1 = 0;
            d2 = 0;

            if (BeamID==1)
            {
                return;  
            }

            string pre =name.Substring(0,name.Length-2)+(BeamID - 1).ToString().PadLeft(2, '0');

            string RecString = string.Format("SELECT * FROM box_tbl WHERE Name='{0}'", name);
            MySqlDataAdapter adapter = new MySqlDataAdapter(RecString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable thisBeam = dataset.Tables[0];

            RecString = string.Format("SELECT * FROM box_tbl WHERE Name='{0}'", pre);
            adapter = new MySqlDataAdapter(RecString, CurConn);
            dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable preBeam = dataset.Tables[0];

            Point2D thisStPin = new Point2D((double)thisBeam.Rows[0]["X0"], (double)thisBeam.Rows[0]["Y0"]);
            Point2D thisEdPin = new Point2D((double)thisBeam.Rows[0]["X1"], (double)thisBeam.Rows[0]["Y1"]);
            Point2D preStPin = new Point2D((double)preBeam.Rows[0]["X0"], (double)preBeam.Rows[0]["Y0"]);
            Point2D preEdPin = new Point2D((double)preBeam.Rows[0]["X1"], (double)preBeam.Rows[0]["Y1"]);
            
            Vector2D thisV = new Vector2D(
                (double)thisBeam.Rows[0]["X1"] - (double)thisBeam.Rows[0]["X0"],
                (double)thisBeam.Rows[0]["Y1"] - (double)thisBeam.Rows[0]["Y0"]);

            Vector2D preV = new Vector2D(
                (double)preBeam.Rows[0]["X1"] - (double)preBeam.Rows[0]["X0"],
                (double)preBeam.Rows[0]["Y1"] - (double)preBeam.Rows[0]["Y0"]);

            Line2D thisAxis = new Line2D(thisStPin - thisV.Normalize() * 0.5, thisEdPin + thisV.Normalize() * 0.5);
            Line2D preAxis = new Line2D(preStPin - preV.Normalize() * 0.5, preEdPin + preV.Normalize() * 0.5);

            double f10 = 1.0 / Math.Cos((double)thisBeam.Rows[0]["Ang0"] / 180.0 * Math.PI);
            double f11 = 1.0 / Math.Cos((double)thisBeam.Rows[0]["Ang1"] / 180.0 * Math.PI);
            double f20 = 1.0 / Math.Cos((double)preBeam.Rows[0]["Ang0"] / 180.0 * Math.PI);
            double f21 = 1.0 / Math.Cos((double)preBeam.Rows[0]["Ang1"] / 180.0 * Math.PI);

            d1 = thisAxis.StartPoint.DistanceTo(preAxis.StartPoint) - 1.2*f10-1.2*f20;
            d2 = thisAxis.EndPoint.DistanceTo(preAxis.EndPoint) - 1.2*f11-1.2*f21;

            d1 = Math.Round(d1 * 1000, 0, MidpointRounding.AwayFromZero);
            d2 = Math.Round(d2 * 1000, 0, MidpointRounding.AwayFromZero);
            return;
        }

        public static string GetCurveDist(DataRow thisBeam, ref MySqlConnection curConn, ref DxfDocument SideLineToCut)
        {
            int ed = 16;

            List<string> BeamExcept1650 = new List<string> {
                "SEC2/MHI02/S05/G01", "SEC2/MHI02/S06/G01" ,
                "SEC2/HSI01/S04/G01", "SEC2/HSI01/S04/G05" ,
            };
            if (BeamExcept1650.Contains((string)thisBeam["Name"]))
            {
                List<double> tmp = new List<double>();
                for (int i = 0; i < ed+1; i++)
                {
                    tmp.Add(1650);

                }
                return tmp.ToString2();

            }
            List<string> BeamExcept1250 = new List<string> {
                "SEC2/HSI01/S08/G01", "SEC2/HSI01/S08/G05" ,
            };
            if (BeamExcept1250.Contains((string)thisBeam["Name"]))
            {
                List<double> tmp = new List<double>();
                for (int i = 0; i < ed + 1; i++)
                {
                    tmp.Add(1200);

                }
                return tmp.ToString2();

            }


            Vector2 thisStPin = new Vector2((double)thisBeam["X0"], (double)thisBeam["Y0"]);
            Vector2 thisEdPin = new Vector2((double)thisBeam["X1"], (double)thisBeam["Y1"]);

            Vector2 thisV = new Vector2(
                (double)thisBeam["X1"] - (double)thisBeam["X0"],
                (double)thisBeam["Y1"] - (double)thisBeam["Y0"]);
            
            thisV.Normalize();
            Line thisAxis = new Line(thisStPin - thisV * 0.5, thisEdPin + thisV * 0.5);
            
            double BeamLen = thisAxis.Length();
            double step = BeamLen / ed;
            List<double> res = new List<double>();
            Vector2 Center = thisAxis.StartPoint.Convert2D();
            Vector2 Cdir = thisV.RotByZ(0.5*Math.PI);
            for (int i = 0; i < ed+1; i++)
            {
                Center = (thisAxis.StartPoint.Convert2D() + step*i*thisV);
                List<double> DistA = new List<double>();
                List<double> DistB = new List<double>();
                Line A = new Line(Center, Center + Cdir * 50);
                Line B = new Line(Center, Center - Cdir * 50);

                foreach (Line line in SideLineToCut.Lines)
                {
                    var f = A.Intersectwith(line.Flatten());
                    if (f != null)
                    {
                        Vector2 pt = (Vector2)f;
                        DistA.Add((pt - Center).Modulus());
                    }
                    var g = B.Intersectwith(line.Flatten());
                    if (g != null)
                    {
                        Vector2 pt = (Vector2)g;
                        DistB.Add((pt - Center).Modulus());
                    }
                }
                foreach (netDxf.Entities.Arc line in SideLineToCut.Arcs)
                {
                    var f = A.Intersectwith(line);
                    if (f != null)
                    {
                        Vector2 pt = (Vector2)f;
                        DistA.Add((pt - Center).Modulus());
                    }
                    var g = B.Intersectwith(line);
                    if (g != null)
                    {
                        Vector2 pt = (Vector2)g;
                        DistB.Add((pt - Center).Modulus());
                    }
                }

                res.Add(Math.Min(DistA.Min(), DistB.Min()));
            }
            var vv = (from a in res select Math.Round(a * 1000, 0, MidpointRounding.AwayFromZero)).ToList();           
            
            return vv.ToString2();
        }

        public static void MakePadPara(ref List<string> allbeamname, DataRow beam, string span, int beamID, out double padtype, out double padang,out double LBlk)
        {            
            padtype = 0;
            padang = 0.0;
            //string loc = ((string)beam["Name"]).Split('G')[0];
            string loc = Regex.Split(((string)beam["Name"]), "/G")[0];
            int beamcount=allbeamname.FindAll((x) => x.StartsWith(loc)).Count();
            bool isft =(string)(beam["span_name"]) == span;
            if (beamID == 1)
            {
                padtype = -1;
                if (isft)
                {
                    padang = Math.Round((double)beam["Ang0"] * -1,4,MidpointRounding.AwayFromZero);
                }
                else
                {
                    padang = Math.Round((double)beam["Ang1"] * -1,4,MidpointRounding.AwayFromZero);
                }
            }
            else if (beamID == beamcount)
            {
                padtype = 1;

                if (isft)
                {
                    padang =Math.Round( (double)beam["Ang0"] * -1,4,MidpointRounding.AwayFromZero);
                }
                else
                {
                    padang =Math.Round( (double)beam["Ang1"] * -1,4,MidpointRounding.AwayFromZero);
                }
            }

            // 纵向挡块
            LBlk = 0;
            double DH = (double)beam["H1"] - (double)beam["H0"];
            double LSlope = 100.0 * DH / (Math.Sqrt(Math.Pow((double)beam["X0"] - (double)beam["X1"], 2) + Math.Pow((double)beam["Y0"] - (double)beam["Y1"], 2)));
            LSlope = Math.Round(LSlope, 1, MidpointRounding.AwayFromZero);
            if (LSlope>=5.0 && (!isft))
            {
                LBlk = 1;
            }
            else if (LSlope<=-5.0 && isft )
            {
                LBlk = 1;
            }  
        }
    }
}
