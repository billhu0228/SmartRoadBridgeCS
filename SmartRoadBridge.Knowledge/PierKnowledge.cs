using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using netDxf;
using SmartRoadBridge.Alignment;
using SmartRoadBridge.Public;
using SmartRoadBridge.Structure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SmartRoadBridge.Knowledge
{
    public static class PierKnowledge
    {
        public static double SafeDist = 0.3;


        //public static SubStructure Arrange3(DataTable dt,int index,ref Align CL1,ref Align CL2 ,List<Vector2D> distListLeft, List<Vector2D> distListRight)
        //{
        //    DataRow Item = dt.Rows[index];

        //    SubStructure Result = new PierNone();

        //    string BrName = (string)Item["Bridge"];
        //    double curPK = (double)Item["Station"];
        //    Angle theta = Angle.FromDegrees((double)(Item["Angle"]));
        //    double WidthLeft= (double)Item["WidthLeft"];
        //    double WidthRight = (double)Item["WidthRight"];
        //    double slopLeft = CL1.curCG.GetHP(curPK)[0];
        //    double slopRight = CL1.curCG.GetHP(curPK)[1];
        //    Vector2D dir = new Vector2D(CL1.curPQX.GetDir(curPK)[0], CL1.curPQX.GetDir(curPK)[1]);
        //    Vector2D Cdir = dir.Rotate(theta);
        //    Point2D CenterP = new Point2D(CL1.curPQX.GetCoord(curPK)[0], CL1.curPQX.GetCoord(curPK)[1]);
        //    Point2D LeftP = CenterP + Cdir * WidthLeft;
        //    Point2D RightP = CenterP - Cdir * WidthRight;
        //    double LeftPK = CL1.curPQX.GetStation(LeftP.X, LeftP.Y);
        //    double RightPK = CL1.curPQX.GetStation(RightP.X, RightP.Y);
        //    Point2D LeftCenter = CL1.curPQX.GetPoint2D(LeftPK);
        //    Point2D RightCenter = CL1.curPQX.GetPoint2D(RightPK);

        //    double H0 = CL1.curSQX.GetBG(LeftPK) - BeamKnowledge.SupTotalH - CL1.curCG.GetHP(LeftPK)[0] * 0.01 * (LeftCenter - LeftP).Length;
        //    double H1 =CL1.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH;
        //    double H2 = CL1.curSQX.GetBG(RightPK) - BeamKnowledge.SupTotalH + CL1.curCG.GetHP(RightPK)[1] * 0.01 * (RightCenter - RightP).Length;
        //    Point3D P0 = new Point3D(LeftP.X, LeftP.Y, H0);
        //    Point3D P1 = new Point3D(CenterP.X,CenterP.Y, H1);
        //    Point3D P2 = new Point3D(RightP.X, RightP.Y, H2);

        //    Result = Arrange2((string)Item["align_name"],curPK, theta, WidthLeft, WidthRight, distListLeft, distListRight,ref CL1);


        //    Point3D E0 = (LeftP - Cdir * Result.DistList[0]).Convert3D();
        //    Point3D E1 = (LeftP - Cdir * Result.DistList[1]).Convert3D();
        //    Point3D E2 = new Point3D();
        //    if (Result.DistList[2]!=0)
        //    {
        //        E2 = (LeftP - Cdir * Result.DistList[2]).Convert3D();
        //    }

        //    Result.CapBeamCPList = new List<Point3D>() { P0, P1, P2, new Point3D() };
        //    Result.PierTopList = new List<Point3D>() { E0, E1, E2 };
        //    return Result;

        //}



        public static SubStructure Arrange2(string Line,double curPK, Angle theta, double LeftDeckWidth, double RightDeckWidth,
            List<Vector2D> distListLeft, List<Vector2D> distListRight, ref Align CL,double CenterAdjDist=0)
        {
            if (curPK == 42.370)
            {
                ;
            }
            // 边墩尺寸
            double FC1W = PierF.FC1W;
            double FC2W = PierF.FC2W;
            double C2W1 = PierC2.W1;
            double C2W2 = PierC2.W2;
            double C1W = PierC1.W;
            double F2W = PierF.F2W;
            double F3W = PierF.F3W;
            double F3WM = PierF.F3WM;//框架中间墩
            // 桥面超过盖梁边距
            double Cx = SubStructure.Cx;// 标准墩
            double Cx2 = SubStructure.Cx2;// 框架墩桥面超过盖梁
            double Fx = SubStructure.Fx;// 框架墩盖梁襟边
            // 真实大小悬臂
            double Cat01 = PierC2.CAT01;
            double Cat02 = PierC2.CAT02;
            // 桥面大小悬臂
            double DeckCat01 = Cat01 + Cx;
            double DeckCat02 = Cat02 + Cx;
            double DeckCantLeft, DeckCantRight;
            string C2SutTypeName;
            double DeckToCBLeft, DeckToCBRight;
            SubStructure Result = new PierNone();
            Point2D center = new Point2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);
            double slopLeft = CL.curCG.GetHP(curPK)[0];
            double slopRight = CL.curCG.GetHP(curPK)[1];
            double cbleft; //= widthLeft- Cx; //盖梁左净宽
            double cbright;// = widthRight- Cx;//盖梁右侧净宽
            Vector2D dir = new Vector2D(CL.curPQX.GetDir(curPK)[0], CL.curPQX.GetDir(curPK)[1]);
            Vector2D Cdir = dir.Rotate(theta);
            double h0, h1;
            center = center + Cdir * CenterAdjDist;

            // 一标段桥梁 或 空切 或分幅墩
            // 调整中心点
            if (curPK <= 15700 || distListLeft.Count+distListRight.Count==0)
            {
                cbleft = LeftDeckWidth - Cx; //盖梁左净宽
                cbright = RightDeckWidth - Cx;//盖梁右侧净宽
                center = AdjustC2CenterPoint(cbleft, cbright, center, Cdir);
                h0 = 0;// = CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH-PierC1.CBHight;
                h1 = CL.curDMX.GetBG(curPK) - 0.5;
                if ((CL.Name.StartsWith("L1K")||CL.Name.StartsWith("R1K")|| CL.Name.StartsWith("M3K")) && LeftDeckWidth+RightDeckWidth<21.61)
                {
                    string RampPierType= "R";
                    if (LeftDeckWidth + RightDeckWidth <= 9)
                    {
                        RampPierType = "RA01";
                        Result = new PierR(0, RampPierType, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright);
                        Result.DistList = new List<double>() { 0.5 * (LeftDeckWidth + RightDeckWidth), 0, 0, 0 };
                        //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0])-0.5, 0, 0, 0 };
                    }
                    else if (LeftDeckWidth + RightDeckWidth <=10)
                    {
                        RampPierType = "RA02";
                        Result = new PierR(0, RampPierType, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright);
                        Result.DistList = new List<double>() { 0.5 * (LeftDeckWidth + RightDeckWidth), 0, 0, 0 };
                        //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0])-0.5, 0, 0, 0 };
                    }

                    else if (LeftDeckWidth+RightDeckWidth<=11)
                    {
                        RampPierType = "RA03";
                        Result = new PierR(0, RampPierType, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright);
                        Result.DistList = new List<double>() { 0.5 * (LeftDeckWidth + RightDeckWidth), 0, 0, 0 };
                        //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0])-0.5, 0, 0, 0 };
                    }
                    else if (RightDeckWidth+LeftDeckWidth<=15)
                    {
                        RampPierType = "RA04";
                        Result = new PierR(0, RampPierType, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright);
                        Result.DistList = new List<double>() { 0.5 * (LeftDeckWidth + RightDeckWidth), 0, 0, 0 };
                        //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0])-0.5, 0, 0, 0 };
                    }
                    else if (LeftDeckWidth+RightDeckWidth<=17)
                    {
                        RampPierType = "MG01";
                        Result = new PierR(0, RampPierType, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright);
                        Result.DistList = new List<double>() { 0.5 * (LeftDeckWidth + RightDeckWidth), 0, 0, 0 };
                        //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0])-0.5, 0, 0, 0 };
                    }
                    else if (LeftDeckWidth + RightDeckWidth <= 18.6)
                    {
                        RampPierType = "MG02";
                        Result = new PierR(0, RampPierType, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright);
                        Result.DistList = new List<double>() { 0.5 * (LeftDeckWidth + RightDeckWidth), 0, 0, 0 };
                        //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0])-0.5, 0, 0, 0 };
                    }
                    else
                    {                        
                        Result = new PierC1(0, "MG03", curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright);
                        Result.DistList = new List<double>() { 0.5 * (LeftDeckWidth + RightDeckWidth), 0, 0, 0 };
                        //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0])-0.5, 0, 0, 0 };
                    }
                }
                else
                {
                    //string C2SubType = GetC2SubType(cbleft, cbright, Cat01, Cat01);
                    PierC2.GetParameter(LeftDeckWidth + RightDeckWidth, false, false, out C2SutTypeName, out DeckCantLeft, out DeckCantRight,out DeckToCBLeft,out DeckToCBRight);
                    Result = new PierC2(0, C2SutTypeName, curPK, h0, h1, center, theta, slopLeft, slopRight,
                        LeftDeckWidth - DeckToCBLeft, RightDeckWidth - DeckToCBRight, Cat01, Cat01);
                    Result.DistList = new List<double>() { DeckCantLeft, (LeftDeckWidth + RightDeckWidth) - DeckCantRight, 0, 0 };
                    //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[1]) - 0.5, 0, 0 };

                }

            }


            if (distListLeft.Count == 2 && distListRight.Count == 2)
            {
                #region 双柱墩

                // 双柱墩条件
                h1 = CL.curDMX.GetBG(curPK) - 0.5;
                h0 = 0;// CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH - PierC2.CBHight;

                if (LeftDeckWidth - DeckCat01 + C2W1*0.5 + SafeDist < distListLeft[0].Length)
                {
                    if (RightDeckWidth - DeckCat01 + C2W1 * 0.5 + SafeDist < distListRight[0].Length)
                    {
                        if (LeftDeckWidth+RightDeckWidth<=31.2)
                        {                      
                            // AA 悬臂标准条件
                            //string C2SubType = GetC2SubType(cbleft, cbright, Cat01, Cat01);
                            PierC2.GetParameter(LeftDeckWidth + RightDeckWidth, false, false, out C2SutTypeName, out DeckCantLeft, out DeckCantRight, out DeckToCBLeft, out DeckToCBRight);
                            Result = new PierC2(0, C2SutTypeName, curPK, h0, h1, center, theta, slopLeft, slopRight,
                                LeftDeckWidth - DeckToCBLeft, RightDeckWidth - DeckToCBRight, Cat01, Cat01);
                            //Result.DistList = new List<double>() { DeckCat01, (widthLeft + widthRight) - DeckCat01, 0, 0 };
                            Result.DistList = new List<double>() { DeckCantLeft, (LeftDeckWidth + RightDeckWidth) - DeckCantRight, 0, 0 };
                            //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[1]) - 0.5, 0, 0 };
                        }
  
                    }
                    else if (RightDeckWidth - DeckCat02 + C2W1 * 0.5 + SafeDist < distListRight[0].Length)
                    {
                        //if (widthLeft+widthRight>=22.6)
                        //{
                        //    // AB 悬臂标准条件                        
                        //    //string C2SubType = GetC2SubType(cbleft, cbright, Cat01, Cat02);
                        //    PierC2.GetParameter(widthLeft + widthRight, false, true, out C2SutTypeName, out DeckCantLeft, out DeckCantRight, out DeckToCBLeft, out DeckToCBRight);
                        //    Result = new PierC2(0, C2SutTypeName, curPK, h0, h1, center, theta, slopLeft, slopRight,
                        //        widthLeft - DeckToCBLeft, widthRight - DeckToCBRight, Cat01, Cat02);
                        //    //Result.DistList = new List<double>() { DeckCat01, (widthLeft + widthRight) - DeckCat02, 0, 0 };
                        //    Result.DistList = new List<double>() { DeckCantLeft, (widthLeft + widthRight) - DeckCantRight, 0, 0 };
                        //}

                    }
                }
                else if (LeftDeckWidth - DeckCat02 + C2W1 * 0.5 + SafeDist < distListLeft[0].Length)
                {
                    if (RightDeckWidth - DeckCat01 + C2W1 * 0.5 + SafeDist < distListRight[0].Length)
                    {
                        //if (widthLeft + widthRight >= 22.6)
                        //{
                        //    // BA 悬臂标准条件
                        //    //string C2SubType = GetC2SubType(cbleft, cbright, Cat02, Cat01);
                        //    PierC2.GetParameter(widthLeft + widthRight, true, false, out C2SutTypeName, out DeckCantLeft, out DeckCantRight, out DeckToCBLeft, out DeckToCBRight);
                        //    Result = new PierC2(0, C2SutTypeName, curPK, h0, h1, center, theta, slopLeft, slopRight,
                        //         widthLeft - DeckToCBLeft, widthRight - DeckToCBRight, Cat02, Cat01);
                        //    //   Result.DistList = new List<double>() { DeckCat02, (widthLeft + widthRight) - DeckCat01, 0, 0 };
                        //    Result.DistList = new List<double>() { DeckCantLeft, (widthLeft + widthRight) - DeckCantRight, 0, 0 };
                        //}
                    }
                    else if (RightDeckWidth - DeckCat02 + C2W1 * 0.5 + SafeDist < distListRight[0].Length)
                    {
                        //if (widthRight + widthLeft >= 23.6)
                        //{
                        //    // BB 悬臂标准条件                        
                        //    //string C2SubType = GetC2SubType(cbleft, cbright, Cat02, Cat02);
                        //    PierC2.GetParameter(widthLeft + widthRight, true, true, out C2SutTypeName, out DeckCantLeft, out DeckCantRight, out DeckToCBLeft, out DeckToCBRight);
                        //    Result = new PierC2(0, C2SutTypeName, curPK, h0, h1, center, theta, slopLeft, slopRight,
                        //         widthLeft - DeckToCBLeft, widthRight - DeckToCBRight, Cat02, Cat02);
                        //    //Result.DistList = new List<double>() { DeckCat02, (widthLeft + widthRight) - DeckCat02, 0, 0 };
                        //    Result.DistList = new List<double>() { DeckCantLeft, (widthLeft + widthRight) - DeckCantRight, 0, 0 };
                        //}

                    }
                }

                #endregion
                #region 独柱墩     
                // 独柱墩条件
                h1 = CL.curDMX.GetBG(curPK) - 0.5;
                h0 = 0;// CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH - PierC1.CBHight;

                if (LeftDeckWidth + RightDeckWidth <= 23.6 && Result.GetType() == typeof(PierNone))               
                {
                    cbleft = LeftDeckWidth - Cx; //盖梁左净宽
                    cbright = RightDeckWidth - Cx;//盖梁右侧净宽

                    double CCAdj =0.5*(LeftDeckWidth-RightDeckWidth);
                    if (distListLeft[0].Length-CCAdj>=SafeDist+0.5*C1W && distListRight[0].Length + CCAdj >= SafeDist + 0.5 * C1W)
                    {
                        string C1Type = RightDeckWidth + LeftDeckWidth <= 22.4 ? "C1MA01" : "C1MA02";
                        Result = new PierC1(0, C1Type, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright);
                        Result.DistList = new List<double>() { 0.5 * (LeftDeckWidth + RightDeckWidth), 0, 0, 0 };
                        //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0])-0.5, 0, 0, 0 };
                    }
                }
                #endregion
                #region FC系列和F3

                // 框架墩条件
                if (Result.GetType()==typeof(PierNone))
                {
                    if (LeftDeckWidth>RightDeckWidth)
                    {
                        // 左跨FC2条件

                        h1 = CL.curDMX.GetBG(curPK) - 0.5;
                        h0 = 0;// CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH - PierC2.CBHight;
                        if (RightDeckWidth - DeckCat01 + 0.5*C2W1 + SafeDist < distListRight[0].Length)
                        {
                            cbleft = LeftDeckWidth;
                            cbright = RightDeckWidth - Cx;
                            double Cantilever = LeftDeckWidth - distListLeft[0].Length + 0.5*C2W1 + SafeDist - Fx - 0.5*FC2W;
                            double W = distListLeft[1].Length - distListLeft[0].Length + SafeDist * 2 + 0.5 * FC2W+0.5*C2W1;
                            double tmp = Math.Max(W - Cantilever, 0);
                            Cantilever = Math.Max(Cantilever, W);
                            double DistBetweenC2 = (LeftDeckWidth + RightDeckWidth + tmp - Cantilever - DeckCat01 - Fx - 0.5 * FC2W);
                            if (DistBetweenC2>=6)
                            {
                                string typestr = "FC2";
                                if (cbleft + tmp + cbright<=32.45)
                                {
                                    typestr += "MD01";
                                }
                                else
                                {
                                    typestr += "MD02";
                                }

                                Result = new PierF(0, typestr, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft + tmp, cbright, 0, Cat01);
                                //Result.DistList = new List<double>() { Cx + 0.5 * FC2W-tmp, Cx + 0.5 * FC2W + Cantilever-tmp, Cx + 0.5 * FC2W + Cantilever + DistBetweenC2-tmp, 0 };
                                Result.DistList = new List<double>() { Fx + 0.5 * FC2W - tmp, Fx + 0.5 * FC2W + Cantilever + DistBetweenC2 - tmp-6, Fx + 0.5 * FC2W + Cantilever + DistBetweenC2 - tmp, 0 };
                                //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[1]) - 0.5, 0, 0 };                        
                            }        
                        }
                        //else if (widthRight - DeckCat02 + 0.5 * C2W1 + SafeDist < distListRight[0].Length)
                        //{
                        //    cbleft = widthLeft;
                        //    cbright = widthRight - Cx;
                        //    double Cantilever = widthLeft - distListLeft[0].Length + 0.5 * C2W1 + SafeDist - Fx - 0.5 * FC2W;
                        //    double W = distListLeft[1].Length - distListLeft[0].Length + SafeDist * 2 + 0.5 * FC2W + 0.5 * C2W1;
                        //    double tmp = Math.Max(W - Cantilever, 0);
                        //    Cantilever = Math.Max(Cantilever, W);
                        //    double DistBetweenC2 = (widthLeft + widthRight + tmp - Cantilever - DeckCat02 - Fx - 0.5 * FC2W);           
                            
                        //    if (DistBetweenC2 >= 6)
                        //    {
                        //        string typestr = "FC2";
                        //        if (cbleft + tmp + cbright <= 32.45)
                        //        {
                        //            typestr += "MD01";
                        //        }
                        //        else
                        //        {
                        //            typestr += "MD02";
                        //        }
                        //        Result = new PierF(0, typestr, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft + tmp, cbright, 0, Cat01);
                        //        Result.DistList = new List<double>() { Fx + 0.5 * FC2W-tmp, Fx + 0.5 * FC2W + Cantilever + DistBetweenC2 - tmp-6, Fx + 0.5 * FC2W + Cantilever + DistBetweenC2-tmp, 0 };
                        //        //Result.DistList = new List<double>() { Cx + 0.5 * FC2W - tmp, Cx + 0.5 * FC2W + Cantilever - tmp, Cx + 0.5 * FC2W + Cantilever + DistBetweenC2 - tmp, 0 };
                        //    }
                        //}

                        // 左跨FC1条件
                        h1 = CL.curDMX.GetBG(curPK) - 0.5;
                        h0 = 0;// CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH - PierC1.CBHight;
                        if (Result.GetType() == typeof(PierNone) && (RightDeckWidth - 10.8 + 0.5*C1W + SafeDist < distListRight[0].Length))
                        {
                            if (distListLeft[0].Length > 0.5 * C1W + SafeDist && distListRight[0].Length > 0.5 * C1W + SafeDist)
                            {                                
                                cbright = RightDeckWidth - Cx;
                                double Cantilever = RightDeckWidth + LeftDeckWidth - 10.8-Fx-0.5*FC1W-Cx2;
                                double W = distListLeft[1].Length + SafeDist + 0.5 * FC1W;
                                double tmp = Math.Max(W - Cantilever, 0);
                                Cantilever = Math.Max(Cantilever, W);


                                string typestr = "FC1";
                                if (Cantilever + 0.5 * FC1W + Fx + cbright <= 30.55)
                                {
                                    typestr += "MC01";
                                }
                                else
                                {
                                    typestr += "MC02";
                                }
                                Result = new PierF(0, typestr, curPK, h0, h1, center, theta, slopLeft, slopRight, Cantilever + 0.5 * FC1W + Fx, cbright, 0, 10.8-Cx);                                
                                
                                Result.DistList = new List<double>() { RightDeckWidth + LeftDeckWidth - 10.8 - Cantilever, RightDeckWidth + LeftDeckWidth - 10.8, 0, 0 };
                                //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[1]) - 0.5, 0, 0 };
                            }
                        }
                        // 右跨FC1条件
                        h1 = CL.curDMX.GetBG(curPK) - 0.5;
                        h0 = 0;// CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH - PierC1.CBHight;
                        if ((Result.GetType() == typeof(PierNone)) && (LeftDeckWidth - 10.8 + 0.5 * C1W + SafeDist < distListLeft[0].Length))
                        {
                            if (distListLeft[0].Length > 0.5 * C1W + SafeDist && distListRight[0].Length > 0.5 * C1W + SafeDist)
                            {
                                cbleft = LeftDeckWidth - Cx;
                                cbright = RightDeckWidth;
                                double Cantilever = RightDeckWidth + LeftDeckWidth - 10.8 - Fx - 0.5 * FC1W;
                                double W = distListRight[1].Length + SafeDist + 0.5 * FC1W;
                                double tmp = Math.Max(W - Cantilever, 0);
                                Cantilever = Math.Max(Cantilever, W);


                                string typestr = "FC1";
                                if (Cantilever + 0.5 * FC1W + Fx + 10.8 - Cx <= 30.55)
                                {
                                    typestr += "MC01";
                                }
                                else
                                {
                                    typestr += "MC02";
                                }
                                Result = new PierF(0, typestr, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, Cantilever + 0.5 * FC1W+Fx, 10.8 - Cx, 0);
                                Result.DistList = new List<double>() { 10.8, Cantilever + 10.8, 0, 0 };
                                //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[1]) - 0.5, 0, 0 };
                            }

                        }
                    }
                    else
                    {
                        // 右跨FC2条件
                        h1 = CL.curDMX.GetBG(curPK) - 0.5;
                        h0 = 0;// CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH - PierC2.CBHight;
                        if (LeftDeckWidth - DeckCat01 + 0.5 * C2W1 + SafeDist < distListLeft[0].Length)
                        {                           
                            double Cantilever = RightDeckWidth- distListRight[0].Length + 0.5*C2W1 + SafeDist - Fx - 0.5 * FC2W;
                            double W = distListRight[1].Length - distListRight[0].Length + SafeDist * 2 + 0.5 * FC2W + 0.5 * C2W1;
                            double tmp = Math.Max(W - Cantilever, 0);
                            Cantilever = Math.Max(Cantilever, W);
                            double DistBetweenC2 = (LeftDeckWidth + RightDeckWidth + tmp - Cantilever - DeckCat01 - Fx - 0.5 * FC2W);
                            if (DistBetweenC2 >= 6)
                            {
                                cbleft = LeftDeckWidth - Cx;
                                cbright = RightDeckWidth;

                                string typestr = "FC2";
                                if (cbleft + tmp + cbright <= 32.45)
                                {
                                    typestr += "MD01";
                                }
                                else
                                {
                                    typestr += "MD02";
                                }
                                Result = new PierF(0, typestr, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright + tmp, Cat01, 0);
                                Result.DistList = new List<double>() { DeckCat01, DeckCat01 + 6, DeckCat01 + Cantilever + DistBetweenC2, 0 };
                                //Result.DistList = new List<double>() { DeckCat01, DeckCat01 + DistBetweenC2, DeckCat01 + Cantilever+DistBetweenC2, 0 };
                                //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[1]) - 0.5, 0, 0 };
                            }
                        }
                        //else if (widthLeft - DeckCat02 + 0.5 * C2W1 + SafeDist < distListLeft[0].Length)
                        //{
                        //    double Cantilever = widthRight  - distListRight[0].Length + 0.5 * C2W1 + SafeDist - Fx - 0.5 * FC2W;
                        //    double W = distListRight[1].Length - distListRight[0].Length + SafeDist * 2 + 0.5 * FC2W + 0.5 * C2W1;
                        //    double tmp = Math.Max(W - Cantilever, 0);
                        //    Cantilever = Math.Max(Cantilever, W);
                        //    double DistBetweenC2 = ( widthLeft + widthRight + tmp - DeckCat02 - Cantilever - Fx - 0.5 * FC2W);
                        //    if (DistBetweenC2 >= 6)
                        //    {
                        //        cbleft = widthLeft - Cx;
                        //        cbright = widthRight;

                        //        string typestr = "FC2";
                        //        if (cbleft + tmp + cbright <= 32.45)
                        //        {
                        //            typestr += "MD01";
                        //        }
                        //        else
                        //        {
                        //            typestr += "MD02";
                        //        }

                        //        Result = new PierF(0, typestr, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright + tmp, Cat02, 0);
                        //        Result.DistList = new List<double>() { DeckCat02, DeckCat02 + 6, DeckCat02 + Cantilever+DistBetweenC2, 0 };
                        //        //Result.DistList = new List<double>() { DeckCat02, DeckCat02 + DistBetweenC2, DeckCat02 + Cantilever + DistBetweenC2, 0 };
                        //    }
                        //}

                        // 右跨FC1条件
                        h1 = CL.curDMX.GetBG(curPK) - 0.5;
                        h0 = 0;// CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH - PierC1.CBHight;
                        if ((Result.GetType() == typeof(PierNone)) && (LeftDeckWidth - 10.8 + 0.5*C1W + SafeDist < distListLeft[0].Length))
                        {
                            if (distListLeft[0].Length > 0.5 * C1W + SafeDist && distListRight[0].Length > 0.5 * C1W + SafeDist)
                            {
                                cbleft = LeftDeckWidth - Cx;
                                cbright = RightDeckWidth;
                                double Cantilever = RightDeckWidth + LeftDeckWidth - 10.8 - Fx - 0.5 * FC1W-Cx2;
                                double W = distListRight[1].Length + SafeDist + 0.5 * FC1W;
                                double tmp = Math.Max(W - Cantilever, 0);
                                Cantilever = Math.Max(Cantilever, W);


                                string typestr = "FC1";
                                if (Cantilever + 0.5 * FC1W + Fx+ 10.8 - Cx <= 30.55)
                                {
                                    typestr += "MC01";
                                }
                                else
                                {
                                    typestr += "MC02";
                                }

                                Result = new PierF(0, typestr, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, Cantilever + 0.5*FC1W+Fx, 10.8-Cx, 0);
                                Result.DistList = new List<double>() { 10.8, Cantilever + 10.8, 0, 0 };
                                //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[1]) - 0.5, 0, 0 };
                            }

                        }

                    }
                }
                //F3条件
                if (Result.GetType() == typeof(PierNone))
                {
                    h1 = CL.curDMX.GetBG(curPK) - 0.5;
                    h0 = 0;//CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH - PierF.F3CBHight;
                    //if (distListLeft[0].Length + distListRight[0].Length > 2 * SafeDist + F3W)
                    if (distListLeft[0].Length + distListRight[0].Length > 1.0) // 3.12修改：只要中间还有点点位置
                    {
                        double CantiRight = RightDeckWidth-Cx2 -Fx- F3W * 0.5;
                        double WRight = distListRight[1].Length + SafeDist + F3W * 0.5;
                        double tmpRight = Math.Max(WRight - CantiRight, 0);
                        CantiRight = Math.Max(CantiRight, WRight);

                        double CantiLeft = LeftDeckWidth - Cx2 - Fx - F3W * 0.5;
                        double WLeft = distListLeft[1].Length + SafeDist + F3W * 0.5;
                        double tmpLeft = Math.Max(WLeft - CantiLeft, 0);
                        CantiLeft = Math.Max(CantiLeft, WLeft);

                        double ccadj = distListLeft[0].Length - distListRight[0].Length;

                        string typestr = "F3";
                        double D1 = Cx2 + Fx + 0.5 * F3W - tmpLeft;
                        double D2 = Cx2 + CantiLeft - 0.5 * ccadj + Fx + 0.5 * F3W - tmpLeft;
                        double D3 = Cx2 + CantiLeft + CantiRight + Fx + 0.5 * F3W - tmpLeft;
                        if (D2 - D1 <= 15.4 && D3 - D2 <= 15.4)
                        {
                            typestr += "MF01";
                        }
                        else if (D2 - D1 <= 15.4 && D3 - D2 > 15.4)
                        {
                            typestr += "MF03";
                        }
                        else if (D2 - D1 > 15.4 && D3 - D2 <= 15.4)
                        {
                            typestr += "MF02";
                        }
                        else
                        {
                            typestr += "MF04";
                        }

                        Result = new PierF(0, typestr, curPK, h0, h1, center, theta, slopLeft, slopRight, CantiLeft + 0.5 * F3W+Fx, CantiRight + 0.5 * F3W + Fx, 0, 0);
                        Result.DistList = new List<double>() { Cx2+Fx + 0.5*F3W-tmpLeft, Cx2 + CantiLeft -0.5* ccadj+ Fx + 0.5*F3W-tmpLeft, Cx2 + CantiLeft +CantiRight+ Fx + 0.5*F3W-tmpLeft, 0 };
                        //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[1]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[2]) - 0.5, 0 };
                    }
                }

                #endregion
            }
            #region F2框架

            if (curPK==19205)
            {
                ;
            }
            //F2 条件
            h1 = CL.curDMX.GetBG(curPK) - 0.5;
            bool LeftDeckControl, RightDeckControl;
            //h0 = CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH - PierF.F2CBHight;
            if (Result.GetType() == typeof(PierNone) && distListLeft.Count + distListRight.Count == 2)
            {
                double CantiLeft = LeftDeckWidth - Cx2 - Fx - 0.5 * F2W;
                double CantiRight = RightDeckWidth - Cx2 - Fx - 0.5 * F2W;
                if (distListRight.Count == 0)
                {
                    RightDeckControl = true;
                    double W = distListLeft[1].Length + SafeDist + 0.5 * F2W;
                    double tmp = Math.Max(W - CantiLeft, 0);
                    LeftDeckControl = (tmp < 0.3);
                    CantiLeft = Math.Max(CantiLeft, W);
                    string typestr = "F2";
                    if (CantiLeft + 0.5 * F2W + Fx+ CantiRight + 0.5 * F2W + Fx <= 17)
                    {
                        typestr += "ME01";
                    }
                    else if (CantiLeft + 0.5 * F2W + Fx + CantiRight + 0.5 * F2W + Fx <= 22)
                    {
                        typestr += "ME02";
                    }
                    else
                    {
                        typestr += "ME04";
                    }


                    Result = new PierF(0, typestr, curPK, 0, h1, center, theta, slopLeft, slopRight, CantiLeft + 0.5 * F2W + Fx, CantiRight + 0.5 * F2W + Fx, 0, 0);
                    Result.DistList = new List<double>() { Cx2 + Fx + 0.5 * F2W - tmp, Cx2 + CantiLeft + CantiRight + Fx + 0.5 * F2W - tmp, 0, 0 };



                    if (curPK>=18660 && curPK<=20040)
                    {
                        if (LeftDeckControl)
                        {
                            Result.CapBeamLeft += 0.5;
                            Result.DistList[0] = Result.DistList[0] - 0.5;
                        }
                        if (RightDeckControl)
                        {
                            Result.CapBeamRight += 0.5;
                            Result.DistList[1] = Result.DistList[1] + 0.5;
                        }
                    }

                    if (typestr == "F2ME01")
                    {
                        Result.DistList[0] = Result.DistList[0] - 0.1;
                        Result.DistList[1] = Result.DistList[1] + 0.1;
                    }
                }
                else if (distListRight.Count == 1)
                {
                    double W = distListLeft[0].Length + SafeDist + 0.5 * F2W;
                    double tmp = Math.Max(W - CantiLeft, 0);
                    LeftDeckControl = (tmp <0.3);
                    CantiLeft = Math.Max(CantiLeft, W);

                    double WR = distListRight[0].Length + SafeDist + 0.5 * F2W;
                    double tmpR = Math.Max(WR - CantiRight, 0);
                    RightDeckControl = (tmpR < 0.3);
                    CantiRight = Math.Max(CantiRight, WR);


                    string typestr = "F2";
                    if (CantiLeft + 0.5 * F2W + Fx + CantiRight + 0.5 * F2W + Fx <= 17)
                    {
                        typestr += "ME01";
                    }
                    else if (CantiLeft + 0.5 * F2W + Fx + CantiRight + 0.5 * F2W + Fx <= 22)
                    {
                        typestr += "ME02";
                    }
                    else
                    {
                        typestr += "ME04";
                    }
                    Result = new PierF(0, typestr, curPK, 0, h1, center, theta, slopLeft, slopRight, CantiLeft + 0.5 * F2W + Fx, CantiRight + 0.5 * F2W + Fx, 0, 0);
                    Result.DistList = new List<double>() { Cx2 + Fx + 0.5 * F2W - tmp, Cx2 + CantiLeft + CantiRight + Fx + 0.5 * F2W - tmp, 0, 0 };

                    if (curPK >= 18660 && curPK <= 20040)
                    {
                        if (LeftDeckControl)
                        {
                            Result.CapBeamLeft += 0.5;
                            Result.DistList[0] = Result.DistList[0] - 0.5;
                        }
                        if (RightDeckControl)
                        {
                            Result.CapBeamRight += 0.5;
                            Result.DistList[1] = Result.DistList[1] + 0.5;
                        }
                    }

                    if (typestr == "F2ME01")
                    {
                        Result.DistList[0] = Result.DistList[0] - 0.1;
                        Result.DistList[1] = Result.DistList[1] + 0.1;
                    }
                }
                else
                {
                    LeftDeckControl = true;
                    string typestr = "F2";
                    if (CantiLeft + 0.5 * F2W + Fx + CantiRight + 0.5 * F2W + Fx <= 17)
                    {
                        typestr += "ME01";
                    }
                    else if (CantiLeft + 0.5 * F2W + Fx + CantiRight + 0.5 * F2W + Fx <= 22)
                    {
                        typestr += "ME02";
                    }
                    else
                    {
                        typestr += "ME04";
                    }
                    double WR = distListRight[1].Length + SafeDist + 0.5 * F2W;
                    double tmpR = Math.Max(WR - CantiRight, 0);
                    RightDeckControl = (tmpR < 0.3);
                    CantiRight = Math.Max(CantiRight, WR);
                    Result = new PierF(0, typestr, curPK, 0, h1, center, theta, slopLeft, slopRight, CantiLeft + 0.5 * F2W + Fx, CantiRight + 0.5 * F2W + Fx, 0, 0);
                    Result.DistList = new List<double>() { Cx2 + Fx + 0.5 * F2W, Cx2 + CantiLeft + CantiRight + Fx + 0.5 * F2W, 0, 0 };

                    if (curPK >= 18660 && curPK <= 20040)
                    {
                        if (LeftDeckControl)
                        {
                            Result.CapBeamLeft += 0.5;
                            Result.DistList[0] = Result.DistList[0] - 0.5;
                        }
                        if (RightDeckControl)
                        {
                            Result.CapBeamRight += 0.5;
                            Result.DistList[1] = Result.DistList[1] + 0.5;
                        }
                    }

                    if (typestr == "F2ME01")
                    {
                        Result.DistList[0] = Result.DistList[0] - 0.1;
                        Result.DistList[1] = Result.DistList[1] + 0.1;
                    }


                }
                //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[1]) - 0.5, 0, 0 };
            }
            #endregion

            #region F3框架
            //F3框架
            h1 = CL.curDMX.GetBG(curPK) - 0.5;
            h0 = 0;//CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH - PierF.F3CBHight;
            if (Result.GetType() == typeof(PierNone) && distListLeft.Count + distListRight.Count == 4)
            {
                if (distListRight.Count == 1)
                {
                    double CantiRight = RightDeckWidth-Cx2-Fx-0.5*F3W;
                    double WR = distListRight[0].Length + SafeDist + 0.5*F3W;
                    double tmpR = Math.Max(WR - CantiRight, 0);
                    CantiRight = Math.Max(CantiRight, WR);

                    double CantiLeft = LeftDeckWidth - Cx2 - Fx - 0.5 * F3W;
                    double WL = distListLeft[2].Length + SafeDist + 0.5 * F3W;
                    double tmpL = Math.Max(WL - CantiLeft, 0);
                    CantiLeft = Math.Max(CantiLeft, WL);


                  
                    double CC2CC = 0.5 * (distListLeft[1].Length + distListLeft[0].Length);

                    string typestr = "F3";
                    double D1 = Cx2 + Fx + 0.5 * F3W - tmpL;
                    double D2 = Cx2 + CantiLeft + Fx + 0.5 * F3W - CC2CC - tmpL;
                    double D3 = Cx2 + CantiLeft + CantiRight + Fx + 0.5 * F3W - tmpL;
                    if (D2-D1<= 15.4 && D3-D2<=15.4)
                    {
                        typestr += "MF01";
                    }
                    else if (D2 - D1 <= 15.4 && D3 - D2 >15.4)
                    {
                        typestr += "MF03";
                    }
                    else if (D2 - D1 >15.4 && D3 - D2 <= 15.4)
                    {
                        typestr += "MF02";
                    }
                    else
                    {
                        typestr += "MF04";
                    }


                    Result = new PierF(0, typestr, curPK, h0, h1, center, theta, slopLeft, slopRight, CantiLeft + 0.5 * F3W + Fx, CantiRight + 0.5 * F3W + Fx, 0, 0);
                    Result.DistList = new List<double>() { Cx2+Fx + 0.5 * F3W-tmpL, Cx2 + CantiLeft + Fx + 0.5 * F3W-CC2CC-tmpL, Cx2 + CantiLeft + CantiRight + Fx + 0.5 * F3W - tmpL, 0 };                                                                                
                }
              
                else
                {
                    double CantiLeft = LeftDeckWidth - Cx2 - Fx - 0.5 * F3W;
                    double WL = distListLeft[0].Length + SafeDist + 0.5 * F3W;
                    double tmpL = Math.Max(WL - CantiLeft, 0);
                    CantiLeft = Math.Max(CantiLeft, WL);

                    double CantiRight = RightDeckWidth - Cx2 - Fx - 0.5 * F3W;
                    double WR = distListRight[2].Length + SafeDist + 0.5 * F3W;
                    double tmpR = Math.Max(WR - CantiRight, 0);
                    CantiRight = Math.Max(CantiRight, WR);

                    double CC2CC = 0.5 * (distListRight[1].Length + distListRight[0].Length);

                    string typestr = "F3";
                    double D1 = Cx2 + Fx + 0.5 * F3W - tmpL;
                    double D2 = Cx2 + CantiLeft + Fx + 0.5 * F3W - CC2CC - tmpL;
                    double D3 = Cx2 + CantiLeft + CantiRight + Fx + 0.5 * F3W - tmpL;
                    if (D2 - D1 <= 15.4 && D3 - D2 <= 15.4)
                    {
                        typestr += "MF01";
                    }
                    else if (D2 - D1 <= 15.4 && D3 - D2 > 15.4)
                    {
                        typestr += "MF03";
                    }
                    else if (D2 - D1 > 15.4 && D3 - D2 <= 15.4)
                    {
                        typestr += "MF02";
                    }
                    else
                    {
                        typestr += "MF04";
                    }

                    Result = new PierF(0, typestr, curPK, h0, h1, center, theta, slopLeft, slopRight, CantiLeft + 0.5 * F3W + Fx, CantiRight + 0.5 * F3W + Fx, 0, 0);
                    Result.DistList = new List<double>() { Cx2 + Fx + 0.5 * F3W-tmpL, Cx2 + CantiLeft +Fx + 0.5 * F3W + CC2CC-tmpL, Cx2 + Fx + 0.5 * F3W+ CantiRight + CantiLeft -tmpL, 0 };
                }
                //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[0]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[1]) - 0.5, CL.GetGroundBG(curPK, -LeftDeckWidth + Result.DistList[2]) - 0.5, 0 };
            }
            #endregion

            #region 取消
            //if (Result.TypeStr=="C1")
            //{
            //    Result.DistList[0] = Result.DistList[0] - CenterAdjDist;
            //}
            //else if (Result.TypeStr=="F3" || Result.TypeStr=="FC2")
            //{
            //    Result.DistList[0] = Result.DistList[0] - CenterAdjDist;
            //    Result.DistList[1] = Result.DistList[1] - CenterAdjDist;
            //    Result.DistList[2] = Result.DistList[2] - CenterAdjDist;
            //}
            //else
            //{
            //    Result.DistList[0] = Result.DistList[0] - CenterAdjDist;
            //    Result.DistList[1] = Result.DistList[1] - CenterAdjDist;
            //}
            #endregion

            Result.CapBeamLeft -= CenterAdjDist;
            Result.CapBeamRight += CenterAdjDist;
            Result.SpanName = PublicFun.GetID(Line, curPK);
            if (Result.TypeStr.StartsWith("FC2")||Result.TypeStr.StartsWith("F3"))
            {
                Result.FundHList = new List<double>() { Result.H1, Result.H1, Result.H1, 0 };
            }
            else if (Result.TypeStr.StartsWith("FC1")|| Result.TypeStr.StartsWith("C2")|| Result.TypeStr.StartsWith("F2"))
            {
                Result.FundHList = new List<double>() { Result.H1, Result.H1, 0, 0 };
            }
            else
            {
                Result.FundHList = new List<double>() { Result.H1,0, 0, 0 };
            }
            
            return Result;
        }

        public static double GetCapBeamSlop(double capBeamSlop)
        {
            double ress = Math.Round(capBeamSlop, 1, MidpointRounding.AwayFromZero);
            return ress;            
        }

        public static SubStructure ArrangeRamp(double curPK, Angle theta, ref Align CL,DataRow cur, DataRow pre)
        {           


            double h1 = CL.curDMX.GetBG(curPK) - 0.5;
            double h0 = 0;// CL.curSQX.GetBG(curPK) - BeamKnowledge.SupTotalH - PierR.CBHight;


            Point2D center = new Point2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);
            double slopLeft = CL.curCG.GetHP(curPK)[0];
            double slopRight = CL.curCG.GetHP(curPK)[1];
            double cbleft, cbright;
            string curSup = (string)cur["BeamType"];
            string preSup = (string)pre["BeamType"];
            double bk_wl = (double)cur["back_wl"];
            double bk_wr = (double)cur["back_wr"];
            double ft_wl = (double)cur["front_wl"];
            double ft_wr = (double)cur["front_wr"];
            double WidthLeft = Math.Max(bk_wl, ft_wl);
            double WidthRight = Math.Max(bk_wr, ft_wr);
            string strType;
            cbleft = WidthLeft - 0.35;
            cbright = WidthRight - 0.35;
            strType = "R1";

            if (WidthLeft+WidthRight<=9)
            {
                strType = "RA01";
            }
            else if (WidthLeft + WidthRight <= 10)
            {
                strType = "RA02";
            }
            else if (WidthLeft + WidthRight <= 11)
            {
                strType = "RA03";
            }
            else if (WidthLeft + WidthRight <= 15)
            {
                strType = "RA04";
            }
            else if (WidthLeft + WidthRight <= 17)
            {
                strType = "MG01";
            }
            else if (WidthLeft + WidthRight <= 18.6)
            {
                strType = "MG02";
            }
            else if (WidthLeft + WidthRight <= 21.6)
            {
                strType = "MG03";
            }
            else
            {
                ;
            }
            Vector2D dir = new Vector2D(CL.curPQX.GetDir(curPK)[0], CL.curPQX.GetDir(curPK)[1]);
            Vector2D Cdir = dir.Rotate(theta);


            SubStructure Result = new PierR(0, strType, curPK, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright);            
            Result.DistList = new List<double>() { 0.5 * (WidthLeft + WidthRight), 0, 0, 0 };
            //Result.FundHList = new List<double>() { CL.GetGroundBG(curPK, -WidthLeft + Result.DistList[0])-0.5, 0, 0, 0 };
            Result.SpanName = PublicFun.GetID((string)cur["align_name"], curPK);
            return Result;
        }


        //private static void GetC2SubType2(double DeckWidth,bool LeftEnLarge,bool RightEnLarge,
        //    out string TypeName, out double DeckCantLeft, out double DeckCantRight, out double DeckToCBL, out double DeckToCBR)
        //{
        //    TypeName = "";
        //    DeckCantLeft = 0;
        //    DeckCantRight = 0;

        //    if (DeckWidth<=25)
        //    {
        //        TypeName = "C2S";
        //    }
        //    else
        //    {
        //        TypeName = "C2X";
        //    }
        //    if (LeftEnLarge)
        //    {
        //        TypeName += "B";
        //    }
        //    else
        //    {
        //        TypeName += "A";
        //    }
        //    if (RightEnLarge)
        //    {
        //        TypeName += "B";
        //    }
        //    else
        //    {
        //        TypeName += "A";
        //    }

        //    if (DeckWidth <= 25)
        //    {
        //        DeckCantLeft = LeftEnLarge ? 8.8 : 7.8;
        //        DeckCantRight = RightEnLarge ? 8.8 : 7.8;                
        //    }
        //    else
        //    {
        //        DeckCantLeft = LeftEnLarge ? 8.9 : 7.9;
        //        DeckCantRight = RightEnLarge ? 8.9 : 7.9;                
        //    }
        //    DeckToCBL = 0.35;
        //    DeckToCBR = 0.35;
        //}

        private static string GetC2SubType(double cbleft, double cbright,double sideLeft,double sideRight)
        {
            string LeftStr = sideLeft == 7.05 ? "A" : "B";
            string RightStr = sideRight == 7.05 ? "A" : "B";
            string MidStr;
            if ((cbleft + cbright-sideLeft-sideRight) <= 6)
            {
                MidStr = "S";
            }
            else if ((cbleft + cbright - sideLeft - sideRight) <= 8.4)
            {
                MidStr = "S";
            }
            else if ((cbleft + cbright - sideLeft - sideRight) <= 12.4)
            {
                MidStr = "M";
            }
            else
            {
                MidStr = "X";
            }
            return "C2" + MidStr + LeftStr + RightStr;
        }

        private static Point2D AdjustC2CenterPoint(double cbleft, double cbright, Point2D center, Vector2D cdir)
        {
            int AdjDir = cbleft > cbright ? +1 : -1;
            double AdjVal = Math.Abs((cbleft + cbright) * 0.5 - cbleft);
            Point2D res = center + cdir * AdjDir * AdjVal;
            return res;
        }

        public static double GetC2KJW(double refw)
        {

            return Math.Ceiling(refw)-0.2;
        }


        public static double[] GetCTWL(List<List<double>>pileLayout, List<double> refDia, int idx)
        {
            var PileLayoutOnCt = pileLayout[idx];
            double Dia = refDia[idx];
            double W = Dia * 1.6 + PileLayoutOnCt[0];
            double L = Dia * 1.6 + PileLayoutOnCt[1];

            W = Math.Ceiling(W * 10) * 0.1;
            L = Math.Ceiling(L * 10) * 0.1;

            return new[] { W, L };
        }

        public static double[] GetCTWL(List<List<List<double>>> pileLayout, List<double> refDia,int idx)
        {
            var PileLayoutOnCt = pileLayout[idx];
            double Dia = refDia[idx];
            double refWDist=0, refLDist=0;
            if (PileLayoutOnCt[0].Count ==1)
            {
                refWDist = 0;
            }
            else 
            {
                refWDist=Math.Abs(PileLayoutOnCt[0][0]) + Math.Abs(PileLayoutOnCt[0][1]); 
            }
            if (PileLayoutOnCt[1].Count == 1)
            {
                refLDist = 0;
            }
            else
            {
                refLDist = Math.Abs(PileLayoutOnCt[0][0]) + Math.Abs(PileLayoutOnCt[0][1]);
            }
            double W = Math.Max(Dia*1.6,1+Dia)+ refWDist;
            double L = Math.Max(Dia*1.6,1+Dia)+ refLDist;

            W = Math.Round(W, 2);
            L = Math.Round(L, 2);

            return new[] { W, L };            
        }

    }
}
