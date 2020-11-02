using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using SmartRoadBridge.Alignment;
using System;
using System.IO;
using SRBA = SmartRoadBridge.Alignment;

[assembly: CommandClass(typeof(C3DTest.Class1))]
namespace C3DTest
{
    public class Class1
    {


        [CommandMethod("ICD")]
        public static void ReadICD()
        {
            // CAD指针
            CivilDocument civildoc = CivilApplication.ActiveDocument;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = doc.Database;
            Editor ed = doc.Editor;


            PromptOpenFileOptions opt = new PromptOpenFileOptions("\n请选择线型文件.");
            opt.Filter = "ICD File (*.icd)|*.icd|All files (*.*)|*.*";
            PromptFileNameResult res = ed.GetFileNameForOpen(opt);
            if (res.Status != PromptStatus.OK) { return; }
            SRBA.PM bill = new SRBA.PM(res.StringResult);

            // 转化平曲线
            ObjectId AlgID = Alignment.Create(civildoc, Path.GetFileNameWithoutExtension(res.StringResult), null, "0", "Basic", "All Labels");
            using (Transaction ts = acCurDb.TransactionManager.StartTransaction())
            {
                Alignment myAg = ts.GetObject(AlgID, OpenMode.ForWrite) as Alignment;
                AddICD(ref myAg, ref bill, ref ed);
                myAg.ReferencePointStation = bill.StartPK;
                //ed.WriteMessage("\n{0}读取成功.", res.StringResult);


                // 竖曲线
                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("\n是否加载竖曲线?");
                pKeyOpts.Keywords.Add("Y");
                pKeyOpts.Keywords.Add("N");
                pKeyOpts.Keywords.Default = "Y";
                pKeyOpts.AllowNone = true;
                PromptResult pKeyRes = doc.Editor.GetKeywords(pKeyOpts);
                switch (pKeyRes.Status)
                {
                    case PromptStatus.OK:
                        if (pKeyRes.StringResult == "Y")
                        {
                            SRBA.SQX kitty = new SRBA.SQX(Path.ChangeExtension(res.StringResult, "SQX"));
                            ObjectId layerId = myAg.LayerId;
                            ObjectId styleId = civildoc.Styles.ProfileStyles["Basic"];
                            ObjectId labelSetId = civildoc.Styles.LabelSetStyles.ProfileLabelSetStyles["Complete Label Set"];
                            ObjectId oProfileId = Profile.CreateByLayout(myAg.Name + "-Profile", myAg.ObjectId, layerId, styleId, labelSetId);
                            Profile oProfile = ts.GetObject(oProfileId, OpenMode.ForWrite) as Profile;

                            AddSQX(ref oProfile, ref kitty, ref ed);
                        }
                        break;
                    default:
                        break;
                }


                // 转化对比JD法
                //pKeyOpts = new PromptKeywordOptions("\n是否加载交点法平曲线?");
                //pKeyOpts.Keywords.Add("Y");
                //pKeyOpts.Keywords.Add("N");
                //pKeyOpts.Keywords.Default = "Y";
                //pKeyOpts.AllowNone = true;
                //pKeyRes = doc.Editor.GetKeywords(pKeyOpts);
                //switch (pKeyRes.Status)
                //{
                //    case PromptStatus.OK:
                //        if (pKeyRes.StringResult == "Y")
                //        {

                //            PQXnew kitty = new PQXnew("Test");
                //            kitty.ReadICDFile(res.StringResult);
                //            ObjectId layerId = myAg.LayerId;
                //            ObjectId styleId = civildoc.Styles.ProfileStyles["Basic"];
                //            ObjectId labelSetId = civildoc.Styles.LabelSetStyles.ProfileLabelSetStyles["Complete Label Set"];
                //            ObjectId oProfileId = Profile.CreateByLayout(myAg.Name + "-Profile", myAg.ObjectId, layerId, styleId, labelSetId);
                //            Profile oProfile = ts.GetObject(oProfileId, OpenMode.ForWrite) as Profile;

                //            AddPQX( 0,24000,500, ref acCurDb, ref kitty, ref ed);
                //        }
                //        break;
                //    default:
                //        break;
                //}










                ts.Commit();
            }





        }

        private static void AddPQX(double stpk,double edpk,double steppk,ref Database acCurDb, ref PQXnew kitty, ref Editor ed)
        {
            using (Transaction ts = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)ts.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)ts.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                Polyline Paa = new Polyline();

                double pkn = stpk;
                int i = 0;
                while (pkn<edpk)
                {                    
                    var cord=kitty.GetCoord(pkn);
                    Paa.AddVertexAt(i, new Point2d(cord[0],cord[1]), 0, 0, 0);
                    pkn += steppk;
                    i += 1;
                }



                ms.AppendEntity(Paa);
                ts.AddNewlyCreatedDBObject(Paa, true);

                ts.Commit();

            }


        }

        private static void AddICD(ref Alignment myAg, ref SRBA.PM bill, ref Editor ed)
        {
            double curAngInDeg = bill.StartAngInDeg;
            Point3d St = new Point3d(bill.StartY, bill.StartX, 0);
            Point3d Ed, TmpPt;

            //AlignmentLine RefStartLine;
            //AlignmentArc curArc;
            AlignmentSpiral curSpiral;
            for (int i = 0; i < bill.ICDList.Count; i++)
            {
                SRBA.ICD item = bill.ICDList[i];
                switch (item.TypeID)
                {
                    case 1:
                        Ed = new Point3d(St.X + Math.Sin(curAngInDeg) * item.Length, St.Y + Math.Cos(curAngInDeg) * item.Length, 0);
                        AlignmentLine l = myAg.Entities.AddFixedLine(St, Ed);
                        St = Ed;
                        curAngInDeg = l.Direction;
                        break;
                    case 2:
                        if (myAg.Entities.LastEntity != 0)
                        {
                            AlignmentArc a = myAg.Entities.AddFloatingCurve(myAg.Entities.LastEntity, item.R, item.Length, CurveParamType.CurveLength, item.isClookWise);
                            Ed = new Point3d(a.EndPoint.X, a.EndPoint.Y, 0);
                            myAg.Entities.Remove(a);
                            AlignmentArc b = myAg.Entities.AddFixedCurve(St, Ed, item.R, item.isClookWise);
                            St = Ed;
                            curAngInDeg = b.EndDirection;
                        }
                        else
                        {
                            Ed = new Point3d(St.X - Math.Sin(curAngInDeg) * 10, St.Y - Math.Cos(curAngInDeg) * 10, 0);
                            AlignmentLine ll = myAg.Entities.AddFixedLine(Ed, St);
                            AlignmentArc a = myAg.Entities.AddFloatingCurve(myAg.Entities.LastEntity, item.R, item.Length, CurveParamType.CurveLength, item.isClookWise);
                            Ed = new Point3d(a.EndPoint.X, a.EndPoint.Y, 0);
                            myAg.Entities.Remove(a);
                            AlignmentArc b = myAg.Entities.AddFixedCurve(St, Ed, item.R, item.isClookWise);
                            St = Ed;
                            curAngInDeg = b.EndDirection;
                            myAg.Entities.Remove(ll);
                        }
                        break;
                    case 3:
                        if (myAg.Entities.LastEntity != 0)
                        {
                            AlignmentLine tp = myAg.Entities.AddFloatingLine(myAg.Entities.LastEntity, 10.0);
                            Point3d spii = new Point3d(tp.EndPoint.X, tp.EndPoint.Y, 0);
                            myAg.Entities.Remove(tp);
                            AlignmentSpiral s = myAg.Entities.AddFixedSpiral(myAg.Entities.LastEntity, St, spii, item.EndR, item.Length, SpiralCurveType.InCurve, item.isClookWise, SpiralType.Clothoid);
                            Ed = new Point3d(s.EndPoint.X, s.EndPoint.Y, 0);
                            St = Ed;
                            curAngInDeg = s.EndDirection;
                        }
                        else
                        {
                            TmpPt = new Point3d(St.X + Math.Sin(curAngInDeg) * 10, St.Y + Math.Cos(curAngInDeg) * 10, 0);
                            curSpiral = myAg.Entities.AddFixedSpiral(-1, St, TmpPt, item.EndR, item.Length, SpiralCurveType.InCurve, item.isClookWise, SpiralType.Clothoid);
                            Ed = new Point3d(curSpiral.EndPoint.X, curSpiral.EndPoint.Y, 0);
                            St = Ed;
                            curAngInDeg = curSpiral.EndDirection;
                        }

                        break;
                    case 4:

                        if (myAg.Entities.LastEntity != 0)
                        {
                            AlignmentLine tmp = myAg.Entities.AddFloatingLine(myAg.Entities.LastEntity, 10.0);
                            Point3d SPI = new Point3d(tmp.EndPoint.X, tmp.EndPoint.Y, 0);
                            myAg.Entities.Remove(tmp);
                            AlignmentSpiral sp = myAg.Entities.AddFixedSpiral(myAg.Entities.LastEntity, St, SPI,
                                item.StartR, item.Length, SpiralCurveType.OutCurve, item.isClookWise, Autodesk.Civil.SpiralType.Clothoid);
                            Ed = new Point3d(sp.EndPoint.X, sp.EndPoint.Y, 0);
                            St = Ed;
                            curAngInDeg = sp.EndDirection;
                        }
                        else
                        {
                            TmpPt = new Point3d(St.X + Math.Sin(curAngInDeg) * 10, St.Y + Math.Cos(curAngInDeg) * 10, 0);
                            curSpiral = myAg.Entities.AddFixedSpiral(-1, St, TmpPt, item.StartR, item.Length, SpiralCurveType.OutCurve, item.isClookWise, SpiralType.Clothoid);
                            Ed = new Point3d(curSpiral.EndPoint.X, curSpiral.EndPoint.Y, 0);
                            St = Ed;
                            curAngInDeg = curSpiral.EndDirection;
                        }

                        break;
                    case 5:
                        AlignmentLine tt5 = myAg.Entities.AddFloatingLine(myAg.Entities.LastEntity, 10.0);
                        Point3d Sip5 = new Point3d(tt5.EndPoint.X, tt5.EndPoint.Y, 0);
                        myAg.Entities.Remove(tt5);
                        AlignmentSpiral ss5 = myAg.Entities.AddFixedSpiral(myAg.Entities.LastEntity, St, Sip5, item.StartR, item.EndR, item.Length, item.isClookWise, Autodesk.Civil.SpiralType.Clothoid);
                        Ed = new Point3d(ss5.EndPoint.X, ss5.EndPoint.Y, 0);
                        St = Ed;
                        curAngInDeg = ss5.EndDirection;
                        break;
                    case 6://R小-R大

                        if (myAg.Entities.LastEntity != 0)
                        {
                            AlignmentLine tt = myAg.Entities.AddFloatingLine(myAg.Entities.LastEntity, 10.0);
                            Point3d Sip = new Point3d(tt.EndPoint.X, tt.EndPoint.Y, 0);
                            myAg.Entities.Remove(tt);
                            AlignmentSpiral ss = myAg.Entities.AddFixedSpiral(myAg.Entities.LastEntity, St, Sip, item.StartR, item.EndR, item.Length, item.isClookWise, Autodesk.Civil.SpiralType.Clothoid);
                            Ed = new Point3d(ss.EndPoint.X, ss.EndPoint.Y, 0);
                            St = Ed;
                            curAngInDeg = ss.EndDirection;
                        }

                        else
                        {
                            TmpPt = new Point3d(St.X + Math.Sin(curAngInDeg) * 10, St.Y + Math.Cos(curAngInDeg) * 10, 0);
                            curSpiral = myAg.Entities.AddFixedSpiral(-1, St, TmpPt, item.StartR, item.EndR, item.Length, item.isClookWise, Autodesk.Civil.SpiralType.Clothoid);
                            Ed = new Point3d(curSpiral.EndPoint.X, curSpiral.EndPoint.Y, 0);
                            St = Ed;
                            curAngInDeg = curSpiral.EndDirection;
                        }



                        break;
                    default:
                        break;
                }

            }


        }

        private static void AddSQX(ref Profile myPr, ref SRBA.SQX kitty, ref Editor ed)
        {
            Point2d St = new Point2d(kitty.BPDList[0].PK, kitty.BPDList[0].H), Ed;
            Point2d A = new Point2d(), B = new Point2d(), C = new Point2d();
            SRBA.BPD preBPD = new SRBA.BPD(), curBPD = new SRBA.BPD(), nextBPD = new SRBA.BPD();

            if (kitty.BPDList.Count <= 2)
            {
                curBPD = kitty.BPDList[0];
                nextBPD = kitty.BPDList[1];
                B = new Point2d(curBPD.PK, curBPD.H);
                C = new Point2d(nextBPD.PK, nextBPD.H);
                myPr.Entities.AddFixedTangent(B, C);
                return;
            }

            for (int i = 0; i < kitty.BPDList.Count; i++)
            {
                curBPD = kitty.BPDList[i];
                if (i != 0)
                {
                    Ed = new Point2d(curBPD.PK, curBPD.H);
                    myPr.Entities.AddFixedTangent(St, Ed);
                    St = Ed;
                }
            }

            for (int i = 1; i < kitty.BPDList.Count - 2; i++)
            {
                curBPD = kitty.BPDList[i];
                if (curBPD.R == 0)
                {
                    continue;
                }
                ProfileTangent ptA = (ProfileTangent)myPr.Entities[i];
                ProfileTangent ptB = (ProfileTangent)myPr.Entities[i + 1];
                VerticalCurveType ct = ptA.Grade < ptB.Grade ? VerticalCurveType.Sag : VerticalCurveType.Crest;
                double dr = 0.01;
                double RR = curBPD.R;
                while (true)
                {
                    try
                    {
                        myPr.Entities.AddFreeSymmetricParabolaByRadius((uint)i, (uint)(i + 1), ct, RR);
                        break;
                    }
                    catch
                    {
                        RR = RR - dr;
                        continue;
                    }
                }

            }


            //oProfile.Entities.AddFreeSymmetricParabolaByLength(oTangent1.EntityId, oTangent2.EntityId, VerticalCurveType.Sag, 900.1, true);
        }       

    }
}
