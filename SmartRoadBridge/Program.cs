using SmartRoadBridge.Alignment;
using MySql.Data.MySqlClient;
using SmartRoadBridge.Database;
using netDxf;
using System.IO;
using System;
using System.Collections.Generic;

namespace SmartRoadBridge
{
    class Program
    {
        static Align M1K, L1K, R1K, M2K,M3K;
        static Align SBE, EBA;
        static Align JKA,MLA;
        static Align CCA,CCB,CCC,CCD;
        static Align MUA, MUB, MUC;
        static Align WLR;

        static Dictionary<string, Align> AlignDict;
        static KenyaDatabase MainDB;
        static DxfDocument MainRoad,A8Road,LKRoad,RKRoad, A8RoadR,A8RoadL ,EmptyRoad;
        static string RootPart = @"G:\";

                
        static void Main(string[] args)
        {
            NepMain2 Worker = new NepMain2("cdb-2ashfo5g.bj.tencentcdb.com", "10033", "bill", "0okmMKO)", "nep_dev");
            Worker.RefreshEITbl();
            Worker.RefreshBridgeTbl();
            Worker.RefreshSubTbl();
            Worker.OverrideColumn(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SubOverride.csv");
            Worker.RefreshH1(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\H1Override.csv");
            Worker.RefreshBoxTblPlan();
            Worker.NewRefreshCapBeamTbl();
            Worker.OverrideCapBeamTbl(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\CBOverride.csv");
            Worker.RefreshBoxTblEleAndPlinth();
            Worker.RefreshBeamParaTbl();
            Worker.RefreshFundTbl();
            Worker.CreateBlkTbl();
            Worker.Shutdown();
        }

        ///// <summary>
        ///// 原方案
        ///// </summary>
        ///// <param name="args"></param>
        //static void Main2(string[] args)
        //{
        //    NepMain Worker = new NepMain("cdb-2ashfo5g.bj.tencentcdb.com",  "10033","bill", "0okmMKO)", "NEP2020");
        //    //Worker.RefreshEITbl();            
        //    //Worker.RefreshBridgeTbl();
        //    //Worker.RefreshSubTbl(true);
        //    //Worker.OverrideColumn(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SubOverride.csv");
        //    //Worker.RefreshBoxTblPlan(new List<string>() {"SEC202"});
        //    //Worker.RefreshCapBeamTbl(new List<string>() { "SEC202" });
        //    Worker.RefreshBoxTblPlan();
        //    Worker.RefreshCapBeamTbl();
        //    Worker.RefreshBoxTblEle();
        //    Worker.Shutdown();
        //}

        #region 方法

        /// <summary>
        /// 数据初始化
        /// </summary>
        static void DataInitialize()
        {
            M1K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\M1K-0312");
            L1K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\L1K-0312");
            R1K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\R1K-0312");
            M2K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\M2K-0312");
            M3K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\M3K-0312");

            MLA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\08-Mlolongo Interchange\2+400-TQ");
            JKA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\01-JKIA Interchange\JKIA-RAMP A");
            EBA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\02-Eastern Bypass\A");
            SBE = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\03-Southern Bypass\E");

            CCA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\04-Capital Center\A4-3");
            CCB = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\04-Capital Center\B4-3");
            CCC = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\04-Capital Center\C4");
            CCD = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\04-Capital Center\D4");           

            MUA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\06-Museum Interchange\A匝道");
            MUB = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\06-Museum Interchange\B匝道");
            MUC = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\06-Museum Interchange\C匝道");
            
            WLR = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\07-Westlands Interchange\Westland-RampR");

            MainRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "主线边线.dxf")[0]);
            A8Road = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "A8边线.dxf")[0]);
            A8RoadR = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "A8边线右线.dxf")[0]);
            A8RoadL = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "A8边线左线.dxf")[0]);
            LKRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "左线边线.dxf")[0]);
            RKRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "右线边线.dxf")[0]);            
            EmptyRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "空线.dxf")[0]);
            //CCCRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "CCC边线.dxf")[0]);

            AlignDict = new Dictionary<string, Align>()
            {
                {"M1K",M1K},{"L1K",L1K},{"R1K",R1K},{"M2K",M2K},{"M3K",M3K},
                {"MLA",MLA },{"JKA",JKA},{"SBE",SBE},{"EBA",EBA},
                {"CCA",CCA},{"CCB",CCB},{"CCC",CCC},{"CCD",CCD},
                {"MUA",MUA},{"MUB",MUB},{"MUC",MUC},{"WLR",WLR},

            };
        }



        static void WriteEIInfo(bool debug = false)
        {
            MainDB.ReadEIData("M1K",M1K.WorkDir);
            MainDB.ReadEIData("L1K",L1K.WorkDir);
            MainDB.ReadEIData("R1K",R1K.WorkDir);
            MainDB.ReadEIData("M2K",M2K.WorkDir);
            MainDB.ReadEIData("M3K", M3K.WorkDir);
            MainDB.ReadEIData("MLA", MLA.WorkDir);
            MainDB.ReadEIData("JKA", JKA.WorkDir);
            MainDB.ReadEIData("EBA", EBA.WorkDir);
            MainDB.ReadEIData("SBE", SBE.WorkDir);
            MainDB.ReadEIData("CCA",CCA.WorkDir);
            MainDB.ReadEIData("CCB", CCB.WorkDir);
            MainDB.ReadEIData("CCC", CCC.WorkDir);
            MainDB.ReadEIData("CCD", CCD.WorkDir);
            MainDB.ReadEIData("MUA", MUA.WorkDir);
            MainDB.ReadEIData("MUB", MUB.WorkDir);
            MainDB.ReadEIData("MUC", MUC.WorkDir);            
            MainDB.ReadEIData("WLR", WLR.WorkDir);
        }

        /// <summary>
        /// 桥跨信息
        /// </summary>
        /// <param name="debug">调试模式</param>
        static void WriteSpanInfo(bool debug = false)
        {
            if (debug)
            {
                MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\MLI01.csv", ref AlignDict);                
                return;
            }

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\MLI01.csv", ref AlignDict);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\JKA.csv", ref AlignDict);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\CCA.csv", ref AlignDict);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\CCB.csv", ref AlignDict);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\CCC.csv", ref AlignDict);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\CCD.csv", ref AlignDict);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\EBA.csv", ref AlignDict);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SBE.csv", ref AlignDict);            
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\WLR.csv", ref AlignDict);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\MUA.csv", ref AlignDict);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\MUB.csv", ref AlignDict);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\MUC.csv", ref AlignDict);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC101.csv", ref M1K);
            MainDB.UpdateWidth("SEC101", ref MainRoad, ref M1K);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC102.csv", ref M1K);
            MainDB.UpdateWidth("SEC102", ref MainRoad, ref M1K);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC103.csv", ref M1K);
            MainDB.UpdateWidth("SEC103", ref MainRoad, ref M1K);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC104.csv", ref M1K);
            MainDB.UpdateWidth("SEC104", ref MainRoad, ref M1K);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC201.csv", ref M1K);
            MainDB.UpdateWidth("SEC201", ref MainRoad, ref M1K);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC202.csv", ref M1K);
            MainDB.UpdateWidth("SEC202", ref MainRoad, ref M1K);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC203.csv", ref M1K);
            MainDB.UpdateWidth("SEC203", ref MainRoad, ref M1K);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC204R.csv", ref AlignDict);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC204L.csv", ref AlignDict);
            MainDB.UpdateWidth("SEC204R", ref RKRoad, ref AlignDict);
            MainDB.UpdateWidth("SEC204L", ref LKRoad, ref AlignDict);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC205.csv", ref M2K);
            MainDB.UpdateWidth("SEC205", ref MainRoad, ref M2K);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC206.csv", ref M2K);
            MainDB.UpdateWidth("SEC206", ref MainRoad, ref M2K);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC207.csv", ref M2K);
            MainDB.UpdateWidth("SEC207", ref MainRoad, ref M2K);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC208.csv", ref M2K);
            MainDB.UpdateWidth("SEC208", ref MainRoad, ref M2K);

            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC209.csv", ref M2K);
            MainDB.UpdateWidth("SEC209", ref MainRoad, ref M2K);
            MainDB.ReadSpanInfo(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SEC210.csv", ref M3K);
            MainDB.UpdateWidth("SEC210", ref MainRoad, ref M3K);

        }


        private static void WriteBeamInfo(bool debug = false)
        {
            if (debug)
            {
                MainDB.UpdateBox("MLI01", ref AlignDict);
                return;
            }
            MainDB.UpdateBox("SEC101", ref AlignDict);
            MainDB.UpdateBox("SEC102", ref AlignDict);
            MainDB.UpdateBox("SEC103", ref AlignDict);
            MainDB.UpdateBox("SEC104", ref AlignDict);
            MainDB.UpdateBox("SEC201", ref AlignDict);
            MainDB.UpdateBox("SEC202", ref AlignDict);
            MainDB.UpdateBox("SEC203", ref AlignDict);
            MainDB.UpdateBox("SEC204R", ref AlignDict);
            MainDB.UpdateBox("SEC204L", ref AlignDict);
            MainDB.UpdateBox("SEC205", ref AlignDict);
            MainDB.UpdateBox("SEC206", ref AlignDict);
            MainDB.UpdateBox("SEC207", ref AlignDict);
            MainDB.UpdateBox("SEC208", ref AlignDict);
            MainDB.UpdateBox("SEC209", ref AlignDict);
            MainDB.UpdateBox("SEC210", ref AlignDict);
            MainDB.UpdateBox("MLI01", ref AlignDict);
            MainDB.UpdateBox("JKA", ref AlignDict);
            MainDB.UpdateBox("EBA", ref AlignDict);
            MainDB.UpdateBox("SBE", ref AlignDict);
            MainDB.UpdateBox("CCA", ref AlignDict);
            MainDB.UpdateBox("CCB", ref AlignDict);
            MainDB.UpdateBox("CCC", ref AlignDict);
            MainDB.UpdateBox("CCD", ref AlignDict);
            MainDB.UpdateBox("MUA", ref AlignDict);
            MainDB.UpdateBox("MUB", ref AlignDict);
            MainDB.UpdateBox("MUC", ref AlignDict);
            MainDB.UpdateBox("WLR", ref AlignDict);
            //MainDB.UpdateBox("WLL", ref AlignDict);
        }


        private static void WriteColumnInfo(bool debug = false)
        {      

            if (debug)
            {
                MainDB.UpdateColumn("MLI01", ref EmptyRoad, ref AlignDict);
                MainDB.OverrideColumn(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SubOverride.csv");
                return;
            }
            MainDB.UpdateColumn("SEC101", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC102", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC103", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC104", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC201", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC202", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC203", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC204R", ref A8RoadR, ref AlignDict);
            MainDB.UpdateColumn("SEC204L", ref A8RoadL, ref AlignDict);
            MainDB.UpdateColumn("SEC205", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC206", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC207", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC208", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC209", ref A8Road, ref AlignDict);
            MainDB.UpdateColumn("SEC210", ref EmptyRoad, ref AlignDict);

            MainDB.UpdateColumn("MLI01", ref EmptyRoad, ref AlignDict);
            MainDB.UpdateColumn("JKA", ref EmptyRoad, ref AlignDict);
            MainDB.UpdateColumn("EBA", ref EmptyRoad, ref AlignDict);
            MainDB.UpdateColumn("SBE", ref EmptyRoad, ref AlignDict);

            MainDB.UpdateColumn("CCA", ref EmptyRoad, ref AlignDict);
            MainDB.UpdateColumn("CCB", ref EmptyRoad, ref AlignDict);
            MainDB.UpdateColumn("CCC", ref EmptyRoad, ref AlignDict);
            MainDB.UpdateColumn("CCD", ref EmptyRoad, ref AlignDict);

            MainDB.UpdateColumn("MUA", ref EmptyRoad, ref AlignDict);
            MainDB.UpdateColumn("MUB", ref EmptyRoad, ref AlignDict);
            MainDB.UpdateColumn("MUC", ref EmptyRoad, ref AlignDict);

            MainDB.UpdateColumn("WLR", ref EmptyRoad, ref AlignDict);


            MainDB.OverrideColumn(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\SubOverride.csv");
            return;
        }

        #endregion
    }
}
