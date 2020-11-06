using CsvHelper;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using Microsoft.Office.Core;
using MySql.Data.MySqlClient;
using netDxf;
using netDxf.Entities;
using SmartRoadBridge.Alignment;
using SmartRoadBridge.Database;
using SmartRoadBridge.Knowledge;
using SmartRoadBridge.Public;
using SmartRoadBridge.Structure;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartRoadBridge
{
    /// <summary>
    /// NepMain3 ：为SEC204 重做重定义的模型。--20201106
    /// </summary>
    public class NepMain3
    {
        static Align M1K, L1K, R1K, M2K;
        static Align HSA, HSB, HSC, HSE, HSF;
        MySqlConnection CurConn;

        static Dictionary<string, Align> AlignDict;

        static DxfDocument MainRoad, A8Road, LKRoad, RKRoad, A8RoadR, A8RoadL, EmptyRoad;
        static string RootPart = @"G:\";
        static string EJFilePath;
        static string MainDatabaseName;

        Dictionary<string, string> BridgeCSVDict, BridgeToLine, BridgeTitle;



        Dictionary<string, string> NextSpanDict, PreBridgeDict;

        List<string> RampTypeA, RampTypeB, RampTypeC, RampTypeD;

        #region 方法
        /// <summary>
        /// 数据初始化
        /// </summary>
        public NepMain3(string server, string port, string user, string pw, string db)
        {
            MainDatabaseName = db;
            CurConn = new MySqlConnection(string.Format("server={0};port={3};user id={1};password={2};charset=utf8", server, user, pw, port));
            CurConn.Open();
            Console.WriteLine("#  数据库连接成功..");
            MySqlCommand cmd;
            if (!ExcistDB(db))
            {
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                cmd = new MySqlCommand(string.Format("CREATE DATABASE {0};", db), CurConn);
                cmd.ExecuteNonQuery();
            }
            //MySqlCommand cmd = new MySqlCommand(string.Format("DROP DATABASE IF EXISTS {0};", db), CurConn);
            //cmd.ExecuteNonQuery();

            CurConn = new MySqlConnection(string.Format("server={0};port={3};user id={1};password={2};database={4};charset=utf8", server, user, pw, port, db));
            CurConn.Open();
            cmd = new MySqlCommand(string.Format("USE {0};", db), CurConn);
            cmd.ExecuteNonQuery();
            CurConn.Close();

            M1K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\M1K-0312");
            M2K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\M2K-0312");
            L1K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\L1K-0926");
            R1K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\R1K-0312");
            HSA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\A");
            HSB = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\B");
            HSC = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\C");
            HSE = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\E");
            HSF = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\F");


            MainRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "主线边线.dxf")[0]);
            A8Road = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "A8边线.dxf")[0]);
            A8RoadR = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "A8边线右线.dxf")[0]);
            A8RoadL = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "A8边线左线.dxf")[0]);
            LKRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "左线边线.dxf")[0]);
            RKRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "右线边线.dxf")[0]);
            EmptyRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "空线.dxf")[0]);


            AlignDict = new Dictionary<string, Align>()
            {
                {"M1K",M1K},{"M2K",M2K},
                {"L1K",L1K},{"R1K",R1K},
                {"HSA",HSA},{"HSB",HSB},{"HSC",HSC},{"HSE",HSE},{"HSF",HSF},
            };

            NextSpanDict = new Dictionary<string, string>
            {
                { "SEC201","M1K+16160.000" },
                { "SEC202","M1K+17490.000" },
                { "SEC203","M1K+18635.000" },
                { "SEC204L","M2K+20071.000"},
                { "SEC205","M2K+21056.000" },
                { "SEC206","M2K+22657.000" },
                { "SEC207","M2K+22957.000" },
                { "SEC208","M2K+23287.000" },
            };
            PreBridgeDict = new Dictionary<string, string>
            {
                { "SEC202","SEC201" },
                { "SEC203","SEC202" },
                { "SEC204L","SEC203"},
                { "SEC205","SEC204L" },
                { "SEC206","SEC205" },
                { "SEC207","SEC206" },
                { "SEC208","SEC207" },
                { "SEC209","SEC208" },
            };

            BridgeCSVDict = new Dictionary<string, string>()
            {
                //{"HSI02","HSI02.csv"},
                {"HSI01","HSI01.csv"},{"HSI03","HSI03.csv"},{"HSI04","HSI04.csv"},{"HSI05","HSI05.csv"},{"HSI06","HSI06.csv"},
                {"SEC204R","SEC204R.csv"},{"SEC204L","SEC204L.csv"},
            };
            BridgeToLine = new Dictionary<string, string>()
            {

                {"HSI01","HSA"},
                {"HSI02","HSB"},
                {"HSI03","HSC"},
                {"HSI04","HSC"},
                {"HSI05","HSE"},
                {"HSI06","HSF"},
                {"SEC204R","R1K"},
                {"SEC204L","L1K"},

            };
            EJFilePath = string.Format(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\{0}", "EJ.csv");

            BridgeTitle = new Dictionary<string, string>()
            {
                {"HSI01","SEC2(AK0+135.134 - AK0+485.600) HAILE SELASSIE INTERCHANGE - HSI01"},
                {"HSI03","SEC2(CK0+395.480 - CK0+445.480) HAILE SELASSIE INTERCHANGE - HSI03"},
                {"HSI04","SEC2(CK0+245.000 - CK0+275.000) HAILE SELASSIE INTERCHANGE - HSI04"},
                {"HSI05","SEC2(EK0+091.946 - EK0+182.000) HAILE SELASSIE INTERCHANGE - HSI05"},
                {"HSI06","SEC2(FK0+413.600 - FK0+503.731) HAILE SELASSIE INTERCHANGE - HSI06"},
                {"SEC204L","SEC2(M1K18+635 - M2K20+071) MAINLINE BRIDGE - ML04L"},
                {"SEC204R","SEC2(M1K18+635 - M2K20+071) MAINLINE BRIDGE - ML04R"},
            };

            RampTypeD = new List<string>() { "CCI04", "HSI05", "MHI03" };
            RampTypeA = new List<string>() { "CCI01", "HSI01", "MHI01" };
            RampTypeC = new List<string>() { "CCI03", "HSI03", "MHI02", "WLI01" };
            RampTypeB = new List<string>() { "CCI02", "HSI06" };
        }



        public void ResetDamYou()
        {
            CurConn.Open();
            MySqlCommand cmd = new MySqlCommand(string.Format("DROP DATABASE IF EXISTS {0};", CurConn.Database), CurConn);
            cmd.ExecuteNonQuery();
            cmd = new MySqlCommand(string.Format("CREATE DATABASE {0};", CurConn.Database), CurConn);
            cmd.ExecuteNonQuery();
            cmd = new MySqlCommand(string.Format("USE {0};", CurConn.Database), CurConn);
            cmd.ExecuteNonQuery();
            CurConn.Close();
        }

        public bool RefreshEITbl(List<string> CLtoWrite = null)
        {
            if (!ExcistTbl("EI_TBL"))
            {
                string ColumnStr = "Name VarChar(10),ICD Text,SQX Text,DMX Text, CG Text,HDX Text";
                CreatTableWithPriKey("EI_tbl", ColumnStr, "Name");
            }

            if (CLtoWrite == null)
            {
                CLtoWrite = (from key in AlignDict.Keys select key).ToList();
            }
            foreach (string clname in CLtoWrite)
            {
                try
                {
                    WriteEIData(clname, AlignDict[clname].WorkDir);
                }
                catch (Exception)
                {
                    Console.WriteLine(clname + "写入错误");
                }

            }
            return true;
        }

        public bool RefreshBridgeTbl(List<string> BridgeToWrite = null)
        {
            if (BridgeToWrite == null)
            {
                BridgeToWrite = (from key in BridgeCSVDict.Keys select key).ToList();
            }

            MySqlCommand cmd;
            string CreatStr;
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }

            if (!ExcistTbl("bridge_tbl"))
            {
                CreatStr = "Name VarChar(10),align_name VarChar(10), title_name varchar(120)";
                CreatTableWithPriKey("bridge_tbl", CreatStr, "Name");
                CreatForeignKey("bridge_tbl", "align_name", "ei_tbl", "Name");
            }
            foreach (string br in BridgeToWrite)
            {
                cmd = new MySqlCommand(string.Format("insert ignore into bridge_tbl values ('{0}','{1}','{2}')", br, BridgeToLine[br], BridgeTitle[br]), CurConn);
                cmd.ExecuteNonQuery();
            }

            if (!ExcistTbl("span_tbl"))
            {
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                CreatStr = "Name VarChar(17),align_name VarChar(10),bridge_name VarChar(10),Station double,Angle double," +
                    "deck_wl double, deck_wr double,back_wl double, back_wr double,front_wl double ,front_wr double," +
                    "BeamType VarChar(1),PierType VarChar(1),DeckType VarChar(2),cut_to VarChar(17),cut_by VarChar(17),HPL double ,HPR double";
                CreatTableWithPriKey("span_tbl", CreatStr, "Name");
                CreatForeignKey("span_tbl", "align_name", "ei_tbl", "Name");
                CreatForeignKey("span_tbl", "bridge_name", "bridge_tbl", "Name");
            }



            foreach (string br in BridgeToWrite)
            {

                string csvpath = string.Format(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\{0}", BridgeCSVDict[br]);

                List<BridgeINFO> tmp = new List<BridgeINFO>();

                using (var reader = new StreamReader(csvpath))
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.RegisterClassMap<BridgeINFOMap>();
                    tmp = csv.GetRecords<BridgeINFO>().ToList();
                }


                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                foreach (var item in tmp)
                {
                    if (!item.Bridge.StartsWith(br, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    Align CL = AlignDict[item.Line];

                    string cut_to = (item.FundType == "C2F" || item.FundType == "A1F") ? "None" : item.FundType;


                    string subtype = item.SubType.StartsWith("A") ? "A" : "C";
                    string beamtype = item.SupType;
                    string RecString = string.Format("INSERT ignore INTO {0} values('{1}','{2}','{3}',{4},{5}," +
                        "{6},{7},{8},{9},{10},{11}," +
                        "'{12}','{13}','CT','{14}','None',0,0);",
                        "span_tbl", GetID(item.Line, item.PK), item.Line, item.Bridge, item.PK, item.Angle,
                        item.Width * 0.5, item.Width * 0.5, item.Width * 0.5, item.Width * 0.5, item.Width * 0.5, item.Width * 0.5,
                        beamtype, subtype, cut_to);
                    cmd = new MySqlCommand(RecString, CurConn);
                    cmd.ExecuteNonQuery();
                }


                DxfDocument dxftocut = GetSideLine(br);
                if (dxftocut != null)
                {
                    UpdateWidthNew(br, ref dxftocut);
                }

                CurOther(br);
            }

            foreach (string br in BridgeToWrite)
            {
                MakeEJ(br);
            }

            foreach (string br in BridgeToWrite)
            {
                MakeHP(br);
            }

            CurConn.Close();
            return true;
        }

        //public void RefreshSubTbl2(bool reset = false)
        //{
        //    if (reset)
        //    {
        //        if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
        //        MySqlCommand cmd = new MySqlCommand("DROP TABLE IF EXISTS sub_tbl;", CurConn);
        //        cmd.ExecuteNonQuery();
        //    }
        //    if (!ExcistTbl("sub_tbl"))
        //    {
        //        string ColumnStr = "id VarChar(11),PierType VarChar(10),FundType VarChar(10),Angle double, " +
        //            "CBLeftWidth double, CBRightWidth double, SpaceList VarChar(100),PierAngleList VarChar(100),FundAngleList VarChar(100)," +
        //            "PierTopH double,PierBotH double";
        //        CreatTableWithPriKey("sub_tbl", ColumnStr, "id");
        //    }

        //}

        public bool RefreshSubTbl(List<string> BridgeToWrite = null, bool reset = false)
        {
            if (reset)
            {
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                MySqlCommand cmd = new MySqlCommand("DROP TABLE IF EXISTS sub_tbl;", CurConn);
                cmd.ExecuteNonQuery();
            }
            if (!ExcistTbl("SUB_TBL"))
            {
                string ColumnStr = "Name VarChar(15),span_name varchar(17),align_name VarChar(10),bridge_name VarChar(10),Station double,Type VarChar(10),Angle double, " +
                    "LeftWidth double, RightWidth double, SpaceList VarChar(100),PierAngleList VarChar(100),FundAngleList VarChar(100)," +
                    " H0 double, H1 double,SlopLeft double,SlopRight double,SlopReal double,H1List VarChar(100)";
                CreatTableWithPriKey("sub_tbl", ColumnStr, "Name");
                CreatForeignKey("sub_tbl", "align_name", "ei_tbl", "Name");
                CreatForeignKey("sub_tbl", "bridge_name", "bridge_tbl", "Name");
                CreatForeignKey("sub_tbl", "span_name", "span_tbl", "Name");
            }
            if (!ExcistTbl("abut_tbl"))
            {
                string ColumnStr = "Name VarChar(15),span_name varchar(17),align_name VarChar(10),bridge_name VarChar(10),Station double," +
                    "Type VarChar(10),Angle double,deck_wl double, deck_wr double, H0 double, H1 double,SlopLeft double,SlopRight double";
                CreatTableWithPriKey("abut_tbl", ColumnStr, "Name");
                CreatForeignKey("abut_tbl", "align_name", "ei_tbl", "Name");
                CreatForeignKey("abut_tbl", "bridge_name", "bridge_tbl", "Name");
                CreatForeignKey("abut_tbl", "span_name", "span_tbl", "Name");
            }

            if (BridgeToWrite == null)
            {
                BridgeToWrite = (from key in BridgeCSVDict.Keys select key).ToList();
            }
            var brList = BridgeToWrite;
            foreach (var item in brList)
            {
                DxfDocument side = GetConsiderLine(item);
                UpdateColumn(item, ref side, ref AlignDict);
            }
            CurConn.Close();

            return true;

        }

        public void RefreshBoxTblPlan(List<string> BridgeToWrite = null)
        {

            if (!ExcistTbl("box_tbl"))
            {
                string ColumnStr = "Name varchar(18), span_name varchar(17),align_name VarChar(10),bridge_name VarChar(10),Station double,Type VarChar(10),Ang0 double,Ang1 double,BrH0 double,BrH1 double," +
                    "X0 Double,Y0 Double,H0 Double,X1 Double,Y1 Double,H1 Double,IsSide VarChar(3),Slop Double,endspan_name varchar(17)";
                CreatTableWithPriKey("box_tbl", ColumnStr, "Name");
                CreatForeignKey("box_tbl", "align_name", "ei_tbl", "Name");
                CreatForeignKey("box_tbl", "bridge_name", "bridge_tbl", "Name");
                CreatForeignKey("box_tbl", "span_name", "span_tbl", "Name");
                CreatForeignKey("box_tbl", "endspan_name", "span_tbl", "Name");
            }
            if (BridgeToWrite == null)
            {
                BridgeToWrite = (from key in BridgeCSVDict.Keys select key).ToList();
            }
            foreach (string br in BridgeToWrite)
            {
                UpdateBox(br, ref AlignDict);

            }

        }

        public void RefreshBoxTblEleAndPlinth()
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }

            if (!ExcistTbl("plinth_tbl"))
            {
                string ColumnStr = "Name varchar(15), back_row int,front_row int," +
                    "back_splist varchar(100),front_splist varchar(100)," +
                    "back_height varchar(100),front_height varchar(100)," +
                    "back_stum varchar(150),front_stum varchar(150),back_bearing varchar(250),front_bearing varchar(250)," +
                    "back_pad varchar(50),front_pad varchar(50)," +
                    "back_padA varchar(100),front_padA varchar(100),back_LRB varchar(50),front_LRB varchar(50)"
                    ;
                CreatTableWithPriKey("plinth_tbl", ColumnStr, "Name");
                CreatForeignKey("plinth_tbl", "Name", "sub_tbl", "Name");
            }

            string selectString = string.Format("SELECT * FROM sub_tbl");

            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            DataRow[] tmp2 = GetRowByKeyName("span_tbl", "bridge_name", "SEC201", ref CurConn, CurConn.Database);
            tmp2 = tmp2.Add(GetRowByKeyName("span_tbl", "bridge_name", "SEC202", ref CurConn, CurConn.Database));
            tmp2 = tmp2.Add(GetRowByKeyName("span_tbl", "bridge_name", "SEC203", ref CurConn, CurConn.Database));
            tmp2 = tmp2.Add(GetRowByKeyName("span_tbl", "bridge_name", "SEC204L", ref CurConn, CurConn.Database));
            tmp2 = tmp2.Add(GetRowByKeyName("span_tbl", "bridge_name", "SEC205", ref CurConn, CurConn.Database));
            tmp2 = tmp2.Add(GetRowByKeyName("span_tbl", "bridge_name", "SEC206", ref CurConn, CurConn.Database));
            tmp2 = tmp2.Add(GetRowByKeyName("span_tbl", "bridge_name", "SEC207", ref CurConn, CurConn.Database));
            tmp2 = tmp2.Add(GetRowByKeyName("span_tbl", "bridge_name", "SEC208", ref CurConn, CurConn.Database));
            tmp2 = tmp2.Add(GetRowByKeyName("span_tbl", "bridge_name", "SEC209", ref CurConn, CurConn.Database));
            List<DataRow> Sect2BridgeList = tmp2.ToList();
            Sect2BridgeList.Sort((x, y) => ((double)x["Station"]).CompareTo((double)y["Station"]));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                PublicFun.Info(i + 1, dt.Rows.Count);
                DataRow item = dt.Rows[i];
                string pierName = (string)item["Name"];
                if (pierName == "SEC1/SGR01/P02")
                {
                    ;
                }
                List<string> ListSpan = GetAllRelatedSpan((string)item["span_name"]);
                string ej = GetDeckType((string)item["span_name"]);

                string line = (string)item["align_name"];       //表明这个下部结构是按照那条线设计的         
                double Station = (double)item["Station"];
                double slop = (double)item["SlopReal"];
                double Ang = (double)item["Angle"];

                Align CL = AlignDict[line];
                double X0 = CL.curPQX.GetCoord(Station)[0];
                double Y0 = CL.curPQX.GetCoord(Station)[1];
                double Z0 = (double)item["H0"];

                Point2D CapCenter = new Point2D(CL.curPQX.GetCoord(Station)[0], CL.curPQX.GetCoord(Station)[1]);
                Vector2D Dir = new Vector2D(CL.curPQX.GetDir(Station)[0], CL.curPQX.GetDir(Station)[1]);
                Vector2D CBDirPier = Dir.Rotate(Angle.FromDegrees(Ang));
                Vector2D NormalofPier = CBDirPier.Rotate(Angle.FromDegrees(-90.0));
                Point3D CC = new Point3D(X0, Y0, Z0);
                Vector3D Normal = new Vector3D(0, 0, 1);
                Normal = Normal.Rotate(new Vector3D(NormalofPier.X, NormalofPier.Y, 0), -Angle.FromRadians(Math.Atan(slop * 0.01)));
                Plane CapTop = new Plane(CC, Normal.Normalize());

                #region 垫石参数表
                int bk = 0;
                int ft = 0;
                List<double> bkheight = new List<double>();
                List<double> ftheight = new List<double>();
                List<double> bkstum = new List<double>();
                List<double> ftstum = new List<double>();
                List<double> bksplist = new List<double>();
                List<double> ftsplist = new List<double>();
                List<string> bkBearingName = new List<string>();
                List<string> ftBearingName = new List<string>();
                List<double> bkpad = new List<double>();
                List<double> bkpadA = new List<double>();
                List<double> ftpad = new List<double>();
                List<double> ftpadA = new List<double>();
                List<double> bklrb = new List<double>();
                List<double> ftlrb = new List<double>();

                #endregion

                foreach (string span in ListSpan)
                {
                    selectString = string.Format("(SELECT * FROM box_tbl where span_name='{0}')", span);
                    selectString += "UNION";
                    selectString += string.Format("(SELECT * FROM box_tbl where endspan_name='{0}')", span);
                    adapter = new MySqlDataAdapter(selectString, CurConn);
                    DataSet dataset2 = new DataSet();
                    adapter.Fill(dataset2);
                    adapter.Dispose();
                    DataTable tmp = dataset2.Tables[0];

                    var allbeamname = (from DataRow bb in tmp.Rows select Regex.Split(((string)bb["Name"]), "/G")[0]).ToList();

                    if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }

                    List<double> bklrbPart = new List<double>();
                    List<double> ftlrbPart = new List<double>();

                    foreach (DataRow beam in tmp.Rows)
                    {
                        #region 更新

                        #endregion
                        string beamName = (string)beam["Name"];
                        string StSpanName = (string)beam["span_name"];
                        string EdSpanName = (string)beam["endspan_name"];
                        string BeamType = (string)beam["Type"];
                        string key = "";
                        Point3D PinOnBeam, A, B, PinOnCB;

                        int beamID = int.Parse(beamName.Substring(beamName.Count() - 2, 2));

                        if (beamName == "SEC1/SGR01/S01/G01")
                        {
                            ;

                        }
                        if (StSpanName == span)
                        {
                            key = "BrH0";
                            PinOnBeam = new Point3D((double)beam["X0"], (double)beam["Y0"], (double)beam["H0"]);
                            A = new Point3D((double)beam["X0"], (double)beam["Y0"], 8000);
                            B = new Point3D((double)beam["X0"], (double)beam["Y0"], -8000);

                        }
                        else
                        {
                            PinOnBeam = new Point3D((double)beam["X1"], (double)beam["Y1"], (double)beam["H1"]);
                            A = new Point3D((double)beam["X1"], (double)beam["Y1"], 8000);
                            B = new Point3D((double)beam["X1"], (double)beam["Y1"], -8000);
                            key = "BrH1";
                        }
                        Line3D PinLine = new Line3D(A, B);
                        try
                        {
                            PinOnCB = (Point3D)PinLine.IntersectionWith(CapTop);
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        #region 判断支座类型及支座高度
                        double BearingH = 0;
                        string BearingName = "";
                        if (ej == "EJ")
                        {
                            GetGroupLength((string)item["span_name"], out double bkl, out double ftl, ref Sect2BridgeList);
                            double LengthToConsider = key == "BrH0" ? ftl : bkl;
                            if (BeamType.StartsWith("BE03"))
                            {
                                BearingName = "GBZY500x110(CR)";
                                BearingH = 0.110;
                            }
                            else if (BeamType.StartsWith("BE04"))
                            {
                                if (LengthToConsider > 120)
                                {
                                    BearingName = "GBZJH350x600x102(CR)";
                                    BearingH = 0.140;
                                }
                                else
                                {
                                    BearingName = "GBZJ350x600x99(CR)";
                                    BearingH = 0.099;
                                }

                            }
                            else
                            {
                                if (LengthToConsider <= 120)
                                {
                                    BearingName = "GBZJ350x550x99(CR)";
                                    BearingH = 0.099;
                                }
                                else
                                {
                                    BearingName = "GBZJH350x550x102(CR)";
                                    BearingH = 0.140;
                                }

                            }
                        }
                        else
                        {
                            if (BeamType.StartsWith("BE03"))
                            {
                                BearingName = "GBZY500x110(CR)";
                                BearingH = 0.110;
                            }
                            else if (BeamType.StartsWith("BE04"))
                            {
                                BearingName = "GBZJ350x600x99(CR)";
                                BearingH = 0.099;
                            }
                            else
                            {
                                BearingName = "GBZJ350x550x99(CR)";
                                BearingH = 0.099;
                            }

                        }
                        double PlinthHeight;
                        if (BearingName.StartsWith("GBZJH"))
                        {
                            PlinthHeight = PinOnCB.DistanceTo(PinOnBeam) - BearingH - 0.025;
                        }
                        else
                        {
                            PlinthHeight = PinOnCB.DistanceTo(PinOnBeam) - BearingH - 0.05;
                        }

                        #endregion



                        string res = string.Format("UPDATE box_tbl set {0}={1} where Name='{2}';", key, PlinthHeight, beamName);
                        MySqlCommand cmd = new MySqlCommand(res, CurConn);
                        cmd.ExecuteNonQuery();

                        #region 处理横坡

                        #endregion

                        #region 处理垫石表
                        Point2D BearPt = new Point2D(PinOnCB.X, PinOnCB.Y);
                        Vector2D refline = BearPt - CapCenter;
                        Angle RefA = CBDirPier.SignedAngleTo(refline);
                        Line2D CCLine = new Line2D(CapCenter + NormalofPier, CapCenter - NormalofPier);
                        Line2D BearingLine = new Line2D(BearPt + CBDirPier, BearPt - CBDirPier);
                        Point2D XPT = (Point2D)CCLine.IntersectWith(BearingLine);
                        int sign = (BearPt - XPT).SignedAngleTo(NormalofPier).Degrees < 180.0 ? +1 : -1;
                        double dist = sign * XPT.DistanceTo(BearPt);
                        if (RefA.Degrees <= 180.0)
                        {
                            bk += 1;
                            bkheight.Add(Math.Round(1000.0 * PlinthHeight, 0, MidpointRounding.AwayFromZero));
                            bkstum.Add(Math.Round(PinOnCB.Z + PlinthHeight, 3, MidpointRounding.AwayFromZero));
                            bksplist.Add(Math.Round(dist, 3, MidpointRounding.AwayFromZero));
                            bkBearingName.Add(BearingName);
                            double padtype, LRB;
                            double padang;
                            BeamKnowledge.MakePadPara(ref allbeamname, beam, span, beamID, out padtype, out padang, out LRB);
                            string CommonPierType = GetGDDType(span);
                            if (ListSpan.Count != 1 && (CommonPierType.EndsWith("C") || CommonPierType.EndsWith("D")))
                            {
                                bkpad.Add(padtype * -1);
                            }
                            else
                            {
                                bkpad.Add(padtype);
                            }
                            bkpadA.Add(padang);
                            bklrbPart.Add(LRB);
                        }
                        else
                        {
                            ft += 1;
                            ftheight.Add(Math.Round(1000.0 * PlinthHeight, 0, MidpointRounding.AwayFromZero));
                            ftstum.Add(Math.Round(PinOnCB.Z + PlinthHeight, 3, MidpointRounding.AwayFromZero));
                            ftsplist.Add(Math.Round(dist, 3, MidpointRounding.AwayFromZero));
                            ftBearingName.Add(BearingName);
                            double padtype, LRB;
                            double padang;
                            BeamKnowledge.MakePadPara(ref allbeamname, beam, span, beamID, out padtype, out padang, out LRB);
                            string CommonPierType = GetGDDType(span);
                            if (ListSpan.Count != 1 && (CommonPierType.EndsWith("C") || CommonPierType.EndsWith("D")))
                            {
                                ftpad.Add(padtype * -1);
                            }
                            else
                            {
                                ftpad.Add(padtype);
                            }
                            ftpadA.Add(padang);
                            ftlrbPart.Add(LRB);
                        }
                        #endregion
                    }

                    if (bklrbPart.Contains(1))
                    {
                        for (int jj = 0; jj < bklrbPart.Count() - 1; jj++)
                        {
                            bklrbPart[jj] = 1;
                        }
                        bklrbPart[bklrbPart.Count() - 1] = 0;
                    }

                    if (ftlrbPart.Contains(1))
                    {
                        for (int jj = 0; jj < ftlrbPart.Count() - 1; jj++)
                        {
                            ftlrbPart[jj] = 1;
                        }
                        ftlrbPart[ftlrbPart.Count() - 1] = 0;
                    }

                    foreach (var mk in bklrbPart)
                    {
                        bklrb.Add(mk);
                    }
                    foreach (var mk in ftlrbPart)
                    {
                        ftlrb.Add(mk);
                    }

                }
                if (bk != 0 || ft != 0)
                {
                    //if (bklrb.Contains(1))
                    //{
                    //    for (int jj = 0; jj < bk-1; jj++)
                    //    {
                    //        bklrb[jj] = 1;
                    //    }
                    //    bklrb[bk - 1] = 0;
                    //}

                    //if (ftlrb.Contains(1))
                    //{
                    //    for (int jj = 0; jj < ft-1; jj++)
                    //    {
                    //        ftlrb[jj] = 1;
                    //    }
                    //    ftlrb[ft - 1] = 0;
                    //}

                    string sqlstr = string.Format("replace into plinth_tbl values('{0}',{1},{2},'{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}');",
                        pierName, bk, ft, bksplist.ToString2(), ftsplist.ToString2(), bkheight.ToString2(), ftheight.ToString2(), bkstum.ToString2(), ftstum.ToString2(),
                        bkBearingName.ToString2(), ftBearingName.ToString2(), bkpad.ToString2(), ftpad.ToString2(), bkpadA.ToString2(), ftpadA.ToString2(), bklrb.ToString2(), ftlrb.ToString2()
                        );
                    MySqlCommand cmd2 = new MySqlCommand(sqlstr, CurConn);
                    cmd2.ExecuteNonQuery();
                }
            }
            Console.WriteLine("#  更新垫石高度..");
        }



        public void NewRefreshCapBeamTbl()
        {
            string selectString = string.Format("SELECT * FROM sub_tbl");
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            DataRow item;
            double CapBeamSlop = 0;
            double CapBeamCenterH = 0;
            double PinC2C = 0.54;
            Console.Write("#  更新盖梁参数..");

            for (int i = 0; i < dt.Rows.Count; i++)
            {

                #region 更新下部结构标高，横坡
                PublicFun.Info(i + 1, dt.Rows.Count);
                item = dt.Rows[i];
                string Name = (string)item["Name"];
                if (Name == "SEC2/ML04L/P41")
                {
                    ;
                }
                string line = (string)item["align_name"];
                string br = (string)item["bridge_name"];
                double pk = (double)item["Station"];
                string SpanName = (string)item["span_name"];

                Align CLofPier = AlignDict[line];
                double Ang = (double)item["Angle"];
                Point2D CapCenter = new Point2D(CLofPier.curPQX.GetCoord(pk)[0], CLofPier.curPQX.GetCoord(pk)[1]);
                Vector2D Dir = new Vector2D(CLofPier.curPQX.GetDir(pk)[0], CLofPier.curPQX.GetDir(pk)[1]);
                Vector2D CBDirPier = Dir.Rotate(Angle.FromDegrees(Ang));
                Point2D CBLeftExtrem = CapCenter + (double)item["LeftWidth"] * CBDirPier;
                var AdditionalSpan = GetAllRelatedSpan(SpanName);
                DataTable BeamsToCalculate = new DataTable();
                foreach (string sp in AdditionalSpan)
                {
                    selectString = string.Format("SELECT * FROM box_tbl where span_name='{0}'", sp);
                    var tb1 = PublicFun.SelectSQL(selectString, ref CurConn);
                    selectString = string.Format("SELECT * FROM box_tbl where endspan_name='{0}'", sp);
                    var tb2 = PublicFun.SelectSQL(selectString, ref CurConn);
                    if (tb1.Rows.Count != 0)
                    {
                        tb1.Columns.Add("which", typeof(string));
                        tb1.Columns.Add("align", typeof(string));
                        tb1.Columns.Add("side", typeof(string));
                        foreach (DataRow it in tb1.Rows)
                        {

                            it["which"] = "0";
                            it["align"] = GetCLBySpanName(sp);

                            Vector2D beamVec = new Vector2D((double)it["X1"] - (double)it["X0"], (double)it["Y1"] - (double)it["Y0"]);
                            var ABC = CBDirPier.SignedAngleTo(beamVec);
                            it["side"] = ABC.Degrees >= 180 ? "F" : "B";
                        }

                        BeamsToCalculate.Merge(tb1, true);
                    }
                    if (tb2.Rows.Count != 0)
                    {
                        tb2.Columns.Add("which", typeof(string));
                        tb2.Columns.Add("align", typeof(string));
                        tb2.Columns.Add("side", typeof(string));
                        foreach (DataRow it in tb2.Rows)
                        {
                            it["which"] = "1";
                            it["align"] = GetCLBySpanName(sp);
                            Vector2D beamVec = new Vector2D((double)it["X0"] - (double)it["X1"], (double)it["Y0"] - (double)it["Y1"]);
                            var ABC = CBDirPier.SignedAngleTo(beamVec);
                            it["side"] = ABC.Degrees >= 180 ? "F" : "B";
                        }
                        BeamsToCalculate.Merge(tb2, true);
                    }
                }

                if (BeamsToCalculate.Rows.Count == 0)
                {
                    continue;
                }


                List<PlinthStruc> Beams = new List<PlinthStruc>();

                foreach (DataRow row in BeamsToCalculate.Rows)
                {
                    string locstr = (string)row["which"];
                    string otherstr = locstr == "0" ? "1" : "0";
                    int Sign = locstr == "0" ? 1 : -1;
                    Point2D OnTheCap = new Point2D((double)row["X" + locstr], (double)row["Y" + locstr]);
                    Point2D NotOntTheCap = new Point2D((double)row["X" + otherstr], (double)row["Y" + otherstr]);
                    Vector2D BeamDir = Sign * (NotOntTheCap - OnTheCap).Normalize();
                    Point2D ControlPoint = OnTheCap - Sign * PinC2C * BeamDir;
                    Align CL = AlignDict[(string)row["align"]];



                    PlinthStruc pp = new PlinthStruc();
                    pp.Loc = ControlPoint;
                    pp.BackSide = (string)row["Side"] == "B";
                    pp.Hineed = CL.GetSurfaceBG(ControlPoint.X, ControlPoint.Y) - 0.17 - 1.6 - 0.3;
                    if (((string)row["Type"]).StartsWith("BE04", StringComparison.OrdinalIgnoreCase))
                    {
                        pp.Hineed = CL.GetSurfaceBG(ControlPoint.X, ControlPoint.Y) - 0.17 - 1.8 - 0.3;
                    }
                    pp.PinLoc = OnTheCap;
                    pp.Name = (string)row["Name"];
                    Beams.Add(pp);

                }

                var beamgroup1 = (from PlinthStruc a in Beams where a.BackSide select a).ToList();
                var beamgroup2 = (from PlinthStruc a in Beams where !(a.BackSide) select a).ToList();

                var CPParas1 = GetCapParameters(CBDirPier, CapCenter, beamgroup1);
                var CPParas2 = GetCapParameters(CBDirPier, CapCenter, beamgroup2);

                double CPH1 = CPParas1[1];
                double CPH2 = CPParas2[1];

                if (CPH1 < CPH2)
                {
                    CapBeamCenterH = CPH1;
                    CapBeamSlop = CPParas1[0];
                }
                else
                {
                    CapBeamCenterH = CPH2;
                    CapBeamSlop = CPParas2[0];
                }

                if (Name == "SEC2/ML02/P38")//增加特例
                {
                    CapBeamCenterH = CPH2;
                    CapBeamSlop = CPParas2[0];
                }
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                string RecString = string.Format("UPDATE sub_tbl set SlopReal={0},H0={1} where Name='{2}';", CapBeamSlop, CapBeamCenterH, Name);
                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();
                #endregion

                #region 根据边梁类型加宽盖梁

                var typelist = (from a in BeamsToCalculate.AsEnumerable() select ((string)a["Type"]).Last()).ToList();

                if (typelist.Contains('D'))
                {
                    var sub = GetRowByKeyName("sub_tbl", "Name", Name, ref CurConn, CurConn.Database);
                    double cbleft = (double)sub[0]["LeftWidth"];
                    double cbright = (double)sub[0]["RightWidth"];
                    string type = (string)sub[0]["Type"];
                    if (type.StartsWith("MG") || type.StartsWith("C1") || type.StartsWith("R"))
                    {
                        UpdataRowByKeyName("sub_tbl", "Name", Name, "LeftWidth", cbleft + 0.45, ref CurConn, CurConn.Database);
                        UpdataRowByKeyName("sub_tbl", "Name", Name, "RightWidth", cbright + 0.45, ref CurConn, CurConn.Database);
                    }
                    else
                    {
                        //Console.WriteLine(Name);
                    }

                }

                ;
                #endregion

                #region 更新下部结构类型
                if (Name == "SEC1/EBI01/P08")
                {
                    ;
                }
                var CurSub = GetRowByKeyName("sub_tbl", "Name", Name, ref CurConn, CurConn.Database).First();
                string OldType = (string)CurSub["Type"];
                string RealPierType = OldType;
                double LeftCBW = (double)CurSub["LeftWidth"];
                double RightCBW = (double)CurSub["RightWidth"];
                double NominalCB2D = Math.Round(LeftCBW + RightCBW, 3, MidpointRounding.AwayFromZero);
                List<double> DistList = (from a in ((string)CurSub["SpaceList"]).Split(',') select double.Parse(a)).ToList();
                double RealCB2D = LeftCBW + RightCBW;
                double AdjLeftLength = 0;
                double AdjRightLength = 0;
                if (OldType.StartsWith("C1"))
                {
                    if (NominalCB2D <= 20.90)
                    {
                        RealPierType = "C1MA01";
                        AdjLeftLength = 0.5 * (20.90 - RealCB2D);
                        AdjRightLength = 0.5 * (20.90 - RealCB2D);
                    }
                    else if (NominalCB2D <= 22.901)
                    {
                        RealPierType = "C1MA02";
                        AdjLeftLength = 0.5 * (NominalCB2D - RealCB2D);
                        AdjRightLength = 0.5 * (NominalCB2D - RealCB2D);
                    }
                    else
                    {
                        RealPierType = "C1MA02X";
                    }
                }
                else if (OldType.StartsWith("C2"))
                {
                    if (DistList[1] - DistList[0] <= 5.9)
                    {
                        RealPierType = "C2MB03";
                        AdjLeftLength = 0.5 * (NominalCB2D - RealCB2D);
                        AdjRightLength = 0.5 * (NominalCB2D - RealCB2D);
                    }
                    else if (NominalCB2D <= 20.90)
                    {
                        RealPierType = "C2MB01";
                        AdjLeftLength = 0.5 * (20.90 - RealCB2D);
                        AdjRightLength = 0.5 * (20.90 - RealCB2D);
                    }
                    else if (NominalCB2D <= 29.301)
                    {
                        RealPierType = "C2MB02";
                        AdjLeftLength = 0.5 * (NominalCB2D - RealCB2D);
                        AdjRightLength = 0.5 * (NominalCB2D - RealCB2D);
                    }
                    else
                    {
                        RealPierType = "C2MB02X";
                    }
                }
                else if (OldType.StartsWith("F2"))
                {

                }
                else if (OldType.StartsWith("F3"))
                {

                }
                else if (OldType.StartsWith("FC1"))
                {

                }
                else if (OldType.StartsWith("FC2"))
                {

                }
                else if (OldType.StartsWith("RA") || OldType.StartsWith("MG"))
                {
                    if (NominalCB2D <= 6.301)
                    {
                        RealPierType = "RA01";
                        AdjLeftLength = 0.5 * (6.3 - RealCB2D);
                        AdjRightLength = 0.5 * (6.3 - RealCB2D);
                    }
                    else if (NominalCB2D <= 8.301)
                    {
                        RealPierType = "RA02";
                        AdjLeftLength = 0.5 * (8.3 - RealCB2D);
                        AdjRightLength = 0.5 * (8.3 - RealCB2D);
                    }
                    else if (NominalCB2D <= 9.301)
                    {
                        RealPierType = "RA03";
                        AdjLeftLength = 0.5 * (9.3 - RealCB2D);
                        AdjRightLength = 0.5 * (9.3 - RealCB2D);
                    }
                    else if (NominalCB2D <= 11.20 - 0.001)
                    {
                        RealPierType = "RA03X";
                        Console.WriteLine(Name + ":" + RealPierType + "\n");
                    }
                    else if (NominalCB2D <= 12.201)
                    {
                        RealPierType = "RA04";
                        AdjLeftLength = 0.5 * (NominalCB2D - RealCB2D);
                        AdjRightLength = 0.5 * (NominalCB2D - RealCB2D);
                    }
                    else if (NominalCB2D <= 14.300 - 0.001)
                    {
                        RealPierType = "RA04X";
                        Console.WriteLine(Name + ":" + RealPierType + "\n");
                    }
                    else if (NominalCB2D <= 16.001)
                    {
                        RealPierType = "MG01";
                        AdjLeftLength = 0.5 * (NominalCB2D - RealCB2D);
                        AdjRightLength = 0.5 * (NominalCB2D - RealCB2D);
                    }
                    else if (NominalCB2D <= 17.601)
                    {
                        RealPierType = "MG02";
                        AdjLeftLength = 0.5 * (NominalCB2D - RealCB2D);
                        AdjRightLength = 0.5 * (NominalCB2D - RealCB2D);
                    }
                    else if (NominalCB2D <= 20.301)
                    {
                        RealPierType = "MG03";
                        AdjLeftLength = 0.5 * (NominalCB2D - RealCB2D);
                        AdjRightLength = 0.5 * (NominalCB2D - RealCB2D);
                    }
                    else if (NominalCB2D <= 20.901)
                    {
                        RealPierType = "C1MA01";
                        AdjLeftLength = 0.5 * (20.90 - RealCB2D);
                        AdjRightLength = 0.5 * (20.90 - RealCB2D);
                    }
                    else if (NominalCB2D <= 22.90)
                    {
                        RealPierType = "C1MA02";
                        AdjLeftLength = 0.5 * (NominalCB2D - RealCB2D);
                        AdjRightLength = 0.5 * (NominalCB2D - RealCB2D);
                    }
                    else
                    {
                        RealPierType = "MG99";
                    }
                }

                else
                {
                    throw new Exception("无此墩柱类型");
                }


                UpdataRowByKeyName("sub_tbl", "Name", Name, "LeftWidth", LeftCBW + AdjLeftLength, ref CurConn, CurConn.Database);
                UpdataRowByKeyName("sub_tbl", "Name", Name, "RightWidth", RightCBW + AdjRightLength, ref CurConn, CurConn.Database);
                UpdataRowByKeyName("sub_tbl", "Name", Name, "Type", RealPierType, ref CurConn, CurConn.Database);


                #endregion

            }
            Console.WriteLine();
        }

        public void RefreshCapBeamTbl()
        {
            string selectString = string.Format("SELECT * FROM sub_tbl");
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            double HL = 8000, HR = 8000;
            DataRow item;
            string Xstr, Ystr, Hstr;
            double PinC2C;


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = dt.Rows[i];
                PinC2C = 0.54;
                string Name = (string)item["Name"];


                if (Name == "SEC2/ML04L/P17")
                {
                    ;
                }
                string line = (string)item["align_name"];
                string br = (string)item["bridge_name"];
                double curPK = (double)item["Station"];
                string SpanName = (string)item["span_name"];
                var AdditionalSpan = GetAllRelatedSpan(SpanName);
                Align CL = AlignDict[line];

                DataTable BackBeamsToCalculate, FrontBeamsToCalculate;
                selectString = string.Format("SELECT * FROM box_tbl where span_name='{0}'", SpanName);
                FrontBeamsToCalculate = PublicFun.SelectSQL(selectString, ref CurConn);
                selectString = string.Format("SELECT * FROM box_tbl where endspan_name='{0}'", SpanName);
                BackBeamsToCalculate = PublicFun.SelectSQL(selectString, ref CurConn);
                int FrontBeams = FrontBeamsToCalculate.Rows.Count;
                int BackBeams = BackBeamsToCalculate.Rows.Count;

                if (FrontBeams == 0 && BackBeams == 0)
                {
                    continue;
                }

                Point2D FrontLeftPin = new Point2D(), FrontRightPin = new Point2D();
                Vector2D FrontLeftDir = new Vector2D(), FrontRightDir = new Vector2D();
                if (FrontBeams != 0)
                {
                    FrontLeftPin = new Point2D((double)FrontBeamsToCalculate.Rows[0]["X0"], (double)FrontBeamsToCalculate.Rows[0]["Y0"]);
                    FrontRightPin = new Point2D((double)FrontBeamsToCalculate.Rows[FrontBeams - 1]["X0"], (double)FrontBeamsToCalculate.Rows[FrontBeams - 1]["Y0"]);
                    FrontLeftDir = new Point2D((double)FrontBeamsToCalculate.Rows[0]["X1"], (double)FrontBeamsToCalculate.Rows[0]["Y1"]) - FrontLeftPin;
                    FrontRightDir = new Point2D((double)FrontBeamsToCalculate.Rows[FrontBeams - 1]["X1"], (double)FrontBeamsToCalculate.Rows[FrontBeams - 1]["Y1"]) - FrontRightPin;
                }

                Point2D BackLeftPin = new Point2D(), BackRightPin = new Point2D();
                Vector2D BackLeftDir = new Vector2D(), BackRightDir = new Vector2D();
                if (BackBeams != 0)
                {
                    BackLeftPin = new Point2D((double)BackBeamsToCalculate.Rows[0]["X1"], (double)BackBeamsToCalculate.Rows[0]["Y1"]);
                    BackRightPin = new Point2D((double)BackBeamsToCalculate.Rows[BackBeams - 1]["X1"], (double)BackBeamsToCalculate.Rows[BackBeams - 1]["Y1"]);
                    BackLeftDir = BackLeftPin - new Point2D((double)BackBeamsToCalculate.Rows[0]["X0"], (double)BackBeamsToCalculate.Rows[0]["Y0"]);
                    BackRightDir = BackRightPin - new Point2D((double)BackBeamsToCalculate.Rows[BackBeams - 1]["X0"], (double)BackBeamsToCalculate.Rows[BackBeams - 1]["Y0"]);
                }

                Point2D CCPin = new Point2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);
                Point2D BCLeftPt = new Point2D(), BCRightPt = new Point2D();
                Point2D FTLeftPt = new Point2D(), FTRightPt = new Point2D();
                double BackHL = 8000, BackHR = 8000;
                double FrontHL = 8000, FrontHR = 8000;
                if (FrontBeams != 0)
                {
                    FTLeftPt = FrontLeftPin - FrontLeftDir.Normalize() * PinC2C;
                    FTRightPt = FrontRightPin - FrontRightDir.Normalize() * PinC2C;
                    FrontHL = CL.GetSurfaceBG(FTLeftPt.X, FTLeftPt.Y) - (BeamKnowledge.SurfaceH + BeamKnowledge.BeamH + BeamKnowledge.BearingTotalH);
                    FrontHR = CL.GetSurfaceBG(FTRightPt.X, FTRightPt.Y) - (BeamKnowledge.SurfaceH + BeamKnowledge.BeamH + BeamKnowledge.BearingTotalH);
                }
                if (BackBeams != 0)
                {
                    BCLeftPt = BackLeftPin + BackLeftDir.Normalize() * PinC2C;
                    BCRightPt = BackRightPin + BackRightDir.Normalize() * PinC2C;
                    BackHL = CL.GetSurfaceBG(BCLeftPt.X, BCLeftPt.Y) - (BeamKnowledge.SurfaceH + BeamKnowledge.BeamH + BeamKnowledge.BearingTotalH);
                    BackHR = CL.GetSurfaceBG(BCRightPt.X, BCRightPt.Y) - (BeamKnowledge.SurfaceH + BeamKnowledge.BeamH + BeamKnowledge.BearingTotalH);
                }


                Point2D LeftPt = new Point2D(), RightPt = new Point2D();
                if (BackHL < FrontHL)
                {
                    HL = BackHL;
                    LeftPt = BCLeftPt;
                }
                else
                {
                    HL = FrontHL;
                    LeftPt = FTLeftPt;
                }

                if (BackHR < FrontHR)
                {
                    HR = BackHR;
                    RightPt = BCRightPt;
                }
                else
                {
                    HR = FrontHR;
                    RightPt = FTRightPt;
                }


                double ConsiderDist = LeftPt.DistanceTo(RightPt);
                double MiddleToLeft = LeftPt.DistanceTo(CCPin);

                double CapBeamSlop = (HR - HL) / ConsiderDist * 100.0;
                CapBeamSlop = PierKnowledge.GetCapBeamSlop(CapBeamSlop);

                double CapBeamCenterH = HL + CapBeamSlop / 100.0 * MiddleToLeft;

                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }

                string RecString = string.Format("UPDATE sub_tbl set SlopReal={0},H0={1} where Name='{2}';", CapBeamSlop, CapBeamCenterH, Name);

                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();


            }

        }



        public void RefreshH1(string csvpath)
        {
            string selectString = string.Format("SELECT * FROM sub_tbl");
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            DataRow item;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                PublicFun.Info(i + 1, dt.Rows.Count);
                item = dt.Rows[i];
                string Name = (string)item["Name"];
                if (Name == "SEC2/ML09/P01")
                {
                    ;
                }
                string line = (string)item["align_name"];
                string br = (string)item["bridge_name"];
                double pk = (double)item["Station"];
                string SpanName = (string)item["span_name"];
                string PierType = (string)item["Type"];
                Align CLofPier = AlignDict[line];
                double Ang = (double)item["Angle"];
                Point2D CapCenter = new Point2D(CLofPier.curPQX.GetCoord(pk)[0], CLofPier.curPQX.GetCoord(pk)[1]);
                Vector2D Dir = new Vector2D(CLofPier.curPQX.GetDir(pk)[0], CLofPier.curPQX.GetDir(pk)[1]);
                Vector2D CBDirPier = Dir.Rotate(Angle.FromDegrees(Ang));
                DataRow dr = GetRowByKeyName("span_tbl", "Name", SpanName, ref CurConn)[0];
                Point2D DeckLeftExtrem = CapCenter + (double)dr["deck_wl"] * CBDirPier;
                var distlist = (from a in ((string)item["SpaceList"]).Split(',') select double.Parse(a)).ToList();
                List<double> H1List = new List<double>();
                if (PierType.StartsWith("C1") && br == "SEC209")
                {
                    double d0 = distlist[0];
                    double dle = d0 - 3.5;
                    double dri = d0 + 3.5;
                    distlist[0] = dle;
                    distlist[1] = d0;
                    distlist[2] = dri;
                    distlist[3] = 0;
                }

                foreach (double dst in distlist)
                {
                    if (dst == 0 && distlist.IndexOf(dst) != 0)
                    {

                        H1List.Add(0);
                        continue;
                    }
                    Point2D CCofPier = DeckLeftExtrem + CBDirPier * dst * -1;
                    double St = CLofPier.curPQX.GetStationNew(CCofPier.X, CCofPier.Y);
                    Point2D RefCenter = new Point2D(CLofPier.curPQX.GetCoord(St)[0], CLofPier.curPQX.GetCoord(St)[1]);
                    Vector2D RefV = CCofPier - RefCenter;
                    int sign = RefV.SignedAngleTo(Dir) > Angle.FromDegrees(180) ? -1 : 1;
                    double H = CLofPier.GetGroundBG(St, RefV.Length * sign);
                    if (H < 1600)
                    {
                        ;
                    }
                    if (pk <= 17360 && pk >= 16880)
                    {
                        H1List.Add(H - 1.2);
                    }
                    else
                    {
                        H1List.Add(H - 0.5);
                    }

                }
                if ((string)item[3] == "SEC201" || ((string)item[3] == "SEC203" && (double)item[4] <= 17920))
                {
                    // 先建段
                    double Hreal = (double)item[13];
                    H1List[0] = Hreal;
                    for (int ii = 0; ii < 3; ii++)
                    {
                        if (H1List[ii + 1] != 0)
                        {
                            H1List[ii + 1] = Hreal;
                        }
                    }
                    ;
                }
                else
                {
                    // 非先建段
                    if (PierType.Contains("C2"))
                    {
                        if (PierType.StartsWith("FC2"))
                        {
                            if ((double)item[7] > (double)item[8])
                            {
                                // 左跨
                                double Hreal = Math.Min(H1List[1], H1List[2]);
                                H1List[1] = Hreal;
                                H1List[2] = Hreal;

                            }
                            else
                            {
                                // 右跨
                                double Hreal = Math.Min(H1List[0], H1List[1]);
                                H1List[0] = Hreal;
                                H1List[1] = Hreal;
                            }
                            ;
                        }
                        else
                        {
                            double Hreal = Math.Min(H1List[0], H1List[1]);
                            H1List[0] = Hreal;
                            H1List[1] = Hreal;
                            ;
                        }
                    }
                    else if (PierType.StartsWith("C1"))
                    {
                        if (H1List[1] != 0)
                        {
                            if (H1List[0] * H1List[1] * H1List[2] == 0)
                            {
                                throw new Exception("C1墩柱埋深数据有误");
                            }
                            double Hreal = Math.Min(Math.Min(H1List[0], H1List[1]), H1List[2]);
                            H1List[0] = Hreal;
                            H1List[1] = 0;
                            H1List[2] = 0;
                            H1List[3] = 0;
                        }

                    }
                }


                ;
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                //CLofPier.GetGroundBG()
                string cmdstr = "";
                cmdstr += UpdateMySql(pk, br, H1List.ToString2(), "", "H1List");
                MySqlCommand cmd = new MySqlCommand(cmdstr, CurConn);
                cmd.ExecuteNonQuery();
            }
            CurConn.Close();

            OverrideH1List(csvpath);

        }



        public void OverrideColumn(string csvpath)
        {
            List<SubINFO> tmp = new List<SubINFO>();
            using (var reader = new StreamReader(csvpath))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.AllowComments = true;
                    csv.Configuration.Comment = '#';
                    csv.Configuration.RegisterClassMap<SubINFOMap>();
                    tmp = csv.GetRecords<SubINFO>().ToList();
                }
            }
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            foreach (var item in tmp)
            {

                string typestr = item.Type;
                var dd = (from a in item.SpaceList select a.ToString()).ToArray();
                var SpaceList = string.Join(",", dd);
                var aa = (from a in item.PierAngleList select a.ToString()).ToArray();
                var PierAngleList = string.Join(",", aa);
                var ff = (from a in item.FundAngleList select a.ToString()).ToArray();
                var FundAngleList = string.Join(",", ff);

                string cmdstr = "";

                cmdstr += UpdateMySql(item.Station, item.Bridge, item.Type, "", "Type");
                cmdstr += UpdateMySql(item.Station, item.Bridge, item.Angle, 90, "Angle");
                cmdstr += UpdateMySql(item.Station, item.Bridge, item.LeftWidth, 0, "LeftWidth");
                cmdstr += UpdateMySql(item.Station, item.Bridge, item.RightWidth, 0, "RightWidth");
                cmdstr += UpdateMySql(item.Station, item.Bridge, SpaceList, "0,0,0,0", "SpaceList");
                cmdstr += UpdateMySql(item.Station, item.Bridge, PierAngleList, "0,0,0,0", "PierAngleList");
                cmdstr += UpdateMySql(item.Station, item.Bridge, FundAngleList, "0,0,0,0", "FundAngleList");



                MySqlCommand cmd = new MySqlCommand(cmdstr, CurConn);
                cmd.ExecuteNonQuery();
            }
            CurConn.Close();

            Console.WriteLine("#  下部结构写入自定义配置");
        }

        private void OverrideH1List(string csvpath)
        {
            List<H1INFO> tmp = new List<H1INFO>();
            using (var reader = new StreamReader(csvpath))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.AllowComments = true;
                    csv.Configuration.Comment = '#';
                    csv.Configuration.RegisterClassMap<H1INFOMap>();
                    tmp = csv.GetRecords<H1INFO>().ToList();
                }
            }
            foreach (var item in tmp)
            {
                string typestr = item.Name;
                if (item.H1List.ToString2() != "")
                {

                    UpdataRowByKeyName("sub_tbl", "Name", typestr, "H1List", item.H1List.ToString2(), ref CurConn, CurConn.Database);
                }
            }

            Console.WriteLine("#  下部结构写入埋深自定义配置");
        }

        public void OverrideCapBeamTbl(string csvpath)
        {
            List<CapINFO> tmp = new List<CapINFO>();
            using (var reader = new StreamReader(csvpath))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.RegisterClassMap<CapINFOMap>();
                    tmp = csv.GetRecords<CapINFO>().ToList();
                }
            }
            foreach (var item in tmp)
            {

                string typestr = item.Name;
                if (item.H0 != "")
                {
                    UpdataRowByKeyName("sub_tbl", "Name", typestr, "H0", double.Parse(item.H0), ref CurConn, CurConn.Database);
                }
                if (item.Slope != "")
                {
                    UpdataRowByKeyName("sub_tbl", "Name", typestr, "SlopReal", double.Parse(item.Slope), ref CurConn, CurConn.Database);
                }
            }

            Console.WriteLine("#  下部结构写入自定义配置");
        }



        /// <summary>
        /// 生成预制梁参数表
        /// </summary>
        public void RefreshBeamParaTbl()
        {
            if (!ExcistTbl("beampara_tbl"))
            {
                string ColumnStr = "Name varchar(18),bridge_name varchar(10)," +
                    "StSub_name varchar(18),EdSub_name varchar(18)," +
                    "GirderType varchar(10),GirderWidth double," +
                    "D1 double,D2 double,E1 double,E2 double,NominalLenght double,GirderLength double," +
                    "A1 double,A2 double,Slope double,LSlope double,DistList VarChar(100)";
                CreatTableWithPriKey("beampara_tbl", ColumnStr, "Name");
                CreatForeignKey("beampara_tbl", "bridge_name", "bridge_tbl", "Name");
            }
            string selectString = string.Format("SELECT * FROM box_tbl");
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];

            Console.Write("#  输出预制梁参数   ");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow Beam = dt.Rows[i];
                string Name = (string)Beam["Name"];

                if (Name == "SEC2/ML04L/S06/G04")
                {
                    ;
                }
                string brname = (string)Beam["bridge_name"];
                string spstart = GetSubFromSpan((string)Beam["span_name"]);
                string spend = GetSubFromSpan((string)Beam["endspan_name"]);

                int Width;
                double D1 = 800;
                double D2 = 800;
                int E1 = 40;
                int E2 = 40;

                BeamKnowledge.GetWetJointD(Name, ref CurConn, out D1, out D2);
                double A1 = Math.Round(90.0 + (double)Beam["Ang0"], 1, MidpointRounding.AwayFromZero);
                double A2 = Math.Round(90.0 - (double)Beam["Ang1"], 1, MidpointRounding.AwayFromZero);

                double aa0 = (double)Beam["Ang0"];
                double aa1 = (double)Beam["Ang1"];

                double deltStart = 0.5 / Math.Cos(aa0 / 180.0 * Math.PI);
                double deltEnd = 0.5 / Math.Cos(aa1 / 180.0 * Math.PI);
                double GirderL = 1000 * (deltStart + deltEnd + Math.Sqrt(Math.Pow((double)Beam["X0"] - (double)Beam["X1"], 2) + Math.Pow((double)Beam["Y0"] - (double)Beam["Y1"], 2)));

                double deltStartN = 0.54 / Math.Cos(aa0 / 180.0 * Math.PI);
                double deltEndN = 0.54 / Math.Cos(aa1 / 180.0 * Math.PI);
                double NominalL = 1000 * (deltStartN + deltEndN + Math.Sqrt(Math.Pow((double)Beam["X0"] - (double)Beam["X1"], 2) + Math.Pow((double)Beam["Y0"] - (double)Beam["Y1"], 2)));

                GirderL = Math.Round(GirderL, 0, MidpointRounding.AwayFromZero);
                NominalL = Math.Round(NominalL, 0, MidpointRounding.AwayFromZero);


                double DH = (double)Beam["H1"] - (double)Beam["H0"];
                double LSlope = 100.0 * DH / (Math.Sqrt(Math.Pow((double)Beam["X0"] - (double)Beam["X1"], 2) + Math.Pow((double)Beam["Y0"] - (double)Beam["Y1"], 2)));
                LSlope = Math.Round(LSlope, 1, MidpointRounding.AwayFromZero);

                double Slope = 2.5;
                string BeamType = (string)Beam["Type"];
                if (BeamType.EndsWith("A"))
                {
                    Width = 1650 + 1200;
                }
                else
                {
                    Width = 2400;
                }
                if (BeamType.EndsWith("C"))
                {
                    Slope = 0.0;
                }
                else
                {
                    Slope = (double)Beam["Slop"] > 0 ? 2.5 : -2.5;
                }

                string CurveDist = "";
                if (BeamType.EndsWith("A") || BeamType.EndsWith("D"))
                {
                    DxfDocument dxftocut = GetSideLine(brname);
                    CurveDist = BeamKnowledge.GetCurveDist(Beam, ref CurConn, ref dxftocut);
                }
                string RecString = string.Format("INSERT ignore INTO beampara_tbl values('{0}','{1}','{2}','{3}','{4}',{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},'{16}');",
                        Name, brname, spstart, spend, Beam["Type"], Width, D1, D2, E1, E2, NominalL, GirderL, A1, A2, Slope, LSlope, CurveDist);
                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();

                PublicFun.Info(i + 1, dt.Rows.Count);
            }
        }

        public void RefreshTmpFundTbl()
        {
            CurConn.ChangeDatabase("nep_geo");

            if (!ExcistTbl("fund_tbl"))
            {
                string ColumnStr = "Name varchar(15),Type varchar(10)," +
                    "Depth varchar(20),Size varchar(50)," +
                    "Pile varchar(50),PileLength varchar(50),PileDia varchar(50)";
                CreatTableWithPriKey("fund_tbl", ColumnStr, "Name");
                CreatForeignKey("fund_tbl", "Name", "sub_tbl", "Name");
            }
            string selectString = string.Format("SELECT * FROM sub_tbl");
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            Console.Write("#  输出基础参数   ");



            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow pier = dt.Rows[i];
                string Name = (string)pier["Name"];
                string Type = "KJ";
                string SpaseList = (string)pier["SpaceList"];
                double H1 = (double)pier["H1"];
                List<double[]> Size = new List<double[]>();
                List<double> Depth = new List<double>();
                if (((string)pier["Type"]).StartsWith("C2"))
                {
                    var spList = (from a in SpaseList.Split(',') select double.Parse(a)).ToList();
                    double CC = spList[1] - spList[0];
                    double[] SizeOne = new double[] { CC + 4, 1.8 + 4 };
                    Size.Add(SizeOne);
                    Depth.Add(H1);
                }
                PublicFun.Info(i + 1, dt.Rows.Count);
            }
            CurConn.ChangeDatabase(MainDatabaseName);
        }

        public void RefreshFundTbl()
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            if (!ExcistTbl("fund_tbl"))
            {
                string ColumnStr = "Name varchar(19),sub_name varchar(15)," +
                    "Type varchar(4),X double,Y double,Z double,AngOfNorth double," +
                    "W double, L double,  H double,RealType varchar(4)";
                CreatTableWithPriKey("fund_tbl", ColumnStr, "Name");
                CreatForeignKey("fund_tbl", "sub_name", "sub_tbl", "Name");
            }

            if (!ExcistTbl("pile_tbl"))
            {
                string ColumnStr = "Name varchar(19),sub_name varchar(15),fund_name varchar(19)," +
                    "Type varchar(4),X double,Y double,Z double," +
                    "D double, Length double,Info varchar(20)";
                CreatTableWithPriKey("pile_tbl", ColumnStr, "Name");
                CreatForeignKey("pile_tbl", "sub_name", "sub_tbl", "Name");
                CreatForeignKey("pile_tbl", "fund_name", "fund_tbl", "Name");
            }

            string selectString = string.Format("SELECT * FROM sub_tbl");
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            Console.Write("#  输出基础参数   ");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow pier = dt.Rows[i];
                string PierName = (string)pier["Name"];
                if (PierName == "SEC2/ML02/P20")
                {
                    ;

                }
                Align CL = AlignDict[(string)pier["align_name"]];
                double curPK = (double)pier["Station"];
                Vector2D Dir = new Vector2D(CL.curPQX.GetDir(curPK)[0], CL.curPQX.GetDir(curPK)[1]);
                Vector2D CBDir = Dir.Rotate(Angle.FromDegrees((double)pier["Angle"]));
                Point2D CC = new Point2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);

                DataRow SpanRow = GetRowByPriKeyRow("span_tbl", (string)pier["span_name"]);

                Point2D RefLeft = CC + CBDir * (double)SpanRow["deck_wl"];
                double refTotalWidht = (double)SpanRow["deck_wl"] + (double)SpanRow["deck_wr"];
                string Type = "KJ";
                string RealType = "";
                //double H1 = (double)pier["H1"];
                var SpaseList = (from a in ((string)pier["SpaceList"]).Split(',') select double.Parse(a)).ToList();
                var FundAngList = (from a in ((string)pier["FundAngleList"]).Split(',') select double.Parse(a)).ToList();
                var FundHList = (from a in ((string)pier["H1List"]).Split(',') select double.Parse(a)).ToList();
                string FundName = "";
                double W = 0;
                double L = 0;
                double H = 0;
                double X = 0;
                double Y = 0;
                double Z = 0;
                double AngFromNorth = 0;
                string AddtionalInfo = "";

                if (((string)pier["Type"]).StartsWith("C2"))
                {
                    DataRow FundMap = GetRowByKeyName("stdfundmap_tbl", "Name", PierName, ref CurConn, "nep_geo")[0];
                    //DataRow FundMap = GetRowByPriKeyRow("fundmap_tbl", PierName, "nep_geo");
                    double refDepth = double.Parse((string)FundMap["Depth"]);
                    FundName = PierName + "/F01";
                    AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[0])).Degrees;
                    double CCofPier = SpaseList[1] - SpaseList[0];
                    double pierD = 0;
                    double H1 = FundHList[0];
                    if ((string)FundMap["Type"] == "KJ")
                    {
                        Type = "KJ";
                        Point2D CCofKJ = RefLeft - CBDir * (SpaseList[0] + 0.5 * CCofPier);
                        X = CCofKJ.X;
                        Y = CCofKJ.Y;


                        H = 2;
                        W = PierKnowledge.GetC2KJW(CCofPier + 1.8 + H * 2);
                        H = W >= 13.8 ? 2.5 : 2;
                        W = PierKnowledge.GetC2KJW(CCofPier + 1.8 + H * 2);
                        L = 5.8;

                        if (W < 11.8)
                        {
                            throw new Exception("错误C2扩基");
                        }
                        else if (W == 11.8)
                        {
                            RealType = "SF05";
                        }
                        else if (W == 12.8)
                        {
                            RealType = "SF06";
                        }
                        else if (W == 13.8)
                        {
                            RealType = "SF07";
                        }
                        else if (W == 14.8)
                        {
                            RealType = "SF08";
                        }
                        else if (W == 15.8)
                        {
                            RealType = "SF09";
                        }
                        else if (W == 16.8)
                        {
                            RealType = "SF10";
                        }
                        else if (W == 17.8)
                        {
                            RealType = "SF11";
                        }
                        else
                        {
                            throw new Exception("超过最大C2扩基宽度?");
                        }

                        if (refTotalWidht > 27.6)
                        {
                            //按桩基做         
                            throw new Exception("不可做扩基");

                        }

                        Z = H1 - (refDepth - H - 0.5);
                        AddOneFund(FundName, PierName, Type, RealType, X, Y, Z, AngFromNorth, W, L, H);
                        //if (Type == "ZJ")
                        //{
                        //    Point2D PierA = RefLeft - CBDir * SpaseList[0];
                        //    Point2D PierB = RefLeft - CBDir * SpaseList[1];
                        //    double HofPileTop = H1 - H;

                        //    AddOneZJ(PierName + "/PI01", PierName, FundName, "ZJ", PierA.X, PierA.Y, HofPileTop, pierD, 0);
                        //    AddOneZJ(PierName + "/PI02", PierName, FundName, "ZJ", PierB.X, PierB.Y, HofPileTop, pierD, 0);
                        //}

                    }
                    else
                    {
                        pierD = CCofPier > 12 ? 2.2 : 2.0;
                        Type = (string)FundMap["Type"];
                        double Length = double.Parse((string)FundMap["PileLength"]);
                        Point2D CCofKJ = RefLeft - CBDir * (SpaseList[0] + 0.5 * CCofPier);
                        X = CCofKJ.X;
                        Y = CCofKJ.Y;
                        W = Math.Round(CCofPier + pierD * 1.6, 3);
                        L = pierD * 1.6;
                        H = 2.0;
                        Z = H1 - (refDepth - Length - H - 0.5);
                        double HofPileTop = Z - H;
                        AddtionalInfo = Type == "MCZ" ? (string)FundMap["PileRe"] : "";
                        RealType = pierD == 2.2 ? "PF02" : "PF01";
                        AddOneFund(FundName, PierName, Type, RealType, X, Y, Z, AngFromNorth, W, L, H);
                        Point2D PierA = RefLeft - CBDir * SpaseList[0];
                        Point2D PierB = RefLeft - CBDir * SpaseList[1];
                        AddOneZJ(PierName + "/PI01", PierName, FundName, Type, PierA.X, PierA.Y, HofPileTop, pierD, Length, AddtionalInfo);
                        AddOneZJ(PierName + "/PI02", PierName, FundName, Type, PierB.X, PierB.Y, HofPileTop, pierD, Length, AddtionalInfo);
                    }

                }
                else if (((string)pier["Type"]).StartsWith("C1"))
                {
                    DataRow FundMap = GetRowByKeyName("stdfundmap_tbl", "Name", PierName, ref CurConn, "nep_geo")[0];
                    if (FundMap == null)
                    {
                        Console.WriteLine(PierName + "找不到基础配置\n");
                        continue;
                    }
                    double refDepth = double.Parse((string)FundMap["Depth"]);
                    FundName = PierName + "/F01";
                    AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[0])).Degrees;
                    Point2D CCofKJ = RefLeft - CBDir * (SpaseList[0]);
                    X = CCofKJ.X;
                    Y = CCofKJ.Y;
                    double H1 = FundHList[0];

                    if ((string)FundMap["Type"] == "KJ")
                    {
                        if (refTotalWidht <= 22.4)
                        {
                            W = 8;
                            RealType = "SF03";
                        }
                        else
                        {
                            W = 9;
                            RealType = "SF04";
                        }
                        Type = "KJ";
                        H = 2;
                        L = 5.8;
                        Z = H1 - (refDepth - H - 0.5);
                        AddOneFund(FundName, PierName, Type, RealType, X, Y, Z, AngFromNorth, W, L, H);
                    }
                    else
                    {
                        Type = (string)FundMap["Type"];
                        double Length = double.Parse((string)FundMap["PileLength"]);
                        double PileDia = 2.0;
                        double CCofPile = Type == "MCZ" ? 2.5 * PileDia : 2.0 * PileDia;
                        W = CCofPile + 1.6 * PileDia;
                        L = 3.2;
                        H = 2.5;
                        Z = H1 - (refDepth - H - Length - 0.5);
                        double HofPileTop = H1 - H;

                        if (CCofPile == 4)
                        {
                            if (Type != "MCZ")
                            {
                                RealType = "PF03";
                            }
                            else
                            {
                                RealType = "PF06";
                            }
                        }
                        else if (CCofPile == 5)
                        {
                            RealType = "PF04";
                        }
                        else if (CCofPile == 3.2)
                        {
                            RealType = "PF05";
                        }
                        AddOneFund(FundName, PierName, Type, RealType, X, Y, Z, AngFromNorth, W, L, H);
                        Point2D PierA = CCofKJ + CBDir * CCofPile * 0.5;
                        Point2D PierB = CCofKJ - CBDir * CCofPile * 0.5;
                        AddtionalInfo = Type == "MCZ" ? (string)FundMap["PileRe"] : "";
                        AddOneZJ(PierName + "/PI01", PierName, FundName, Type, PierA.X, PierA.Y, HofPileTop, PileDia, Length, AddtionalInfo);
                        AddOneZJ(PierName + "/PI02", PierName, FundName, Type, PierB.X, PierB.Y, HofPileTop, PileDia, Length, AddtionalInfo);
                    }
                }
                else if (((string)pier["Type"]).StartsWith("RA"))
                {
                    DataRow FundMap = GetRowByKeyName("stdfundmap_tbl", "Name", PierName, ref CurConn, "nep_geo")[0];
                    //DataRow FundMap = GetRowByPriKeyRow("fundmap_tbl", PierName, "nep_geo");
                    if (FundMap == null)
                    {
                        Console.WriteLine(PierName);
                        continue;
                    }
                    double refDepth = double.Parse((string)FundMap["Depth"]);
                    double H1 = FundHList[0];
                    FundName = PierName + "/F01";
                    AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[0])).Degrees;
                    Point2D CCofKJ = RefLeft - CBDir * (SpaseList[0]);
                    X = CCofKJ.X;
                    Y = CCofKJ.Y;

                    if ((string)FundMap["Type"] == "KJ")
                    {
                        Type = "KJ";
                        H = 2;
                        W = 6.5;
                        L = 5.5;
                        Z = H1 - (refDepth - H - 0.5);
                        RealType = "SF01";
                        AddOneFund(FundName, PierName, Type, RealType, X, Y, Z, AngFromNorth, W, L, H);
                    }
                    else
                    {
                        double PileLength = double.Parse((string)FundMap["PileLength"]);
                        Type = (string)FundMap["Type"];
                        double PileDia = 1.6;
                        double CCofPile = Type == "MCZ" ? 2.5 * PileDia : 2.0 * PileDia;
                        W = CCofPile + PileDia * 1.0 + 1;
                        L = 2.6;
                        H = 2;
                        Z = H1 - (refDepth - H - PileLength - 0.5);
                        double HofPileTop = H1 - H;
                        RealType = Type == "MCZ" ? "PF06" : "PF05";
                        AddtionalInfo = Type == "MCZ" ? (string)FundMap["PileRe"] : "";

                        AddOneFund(FundName, PierName, Type, RealType, X, Y, Z, AngFromNorth, W, L, H);
                        Point2D PierA = CCofKJ + CBDir * CCofPile * 0.5;
                        Point2D PierB = CCofKJ - CBDir * CCofPile * 0.5;
                        AddOneZJ(PierName + "/PI01", PierName, FundName, Type, PierA.X, PierA.Y, HofPileTop, PileDia, PileLength, AddtionalInfo);
                        AddOneZJ(PierName + "/PI02", PierName, FundName, Type, PierB.X, PierB.Y, HofPileTop, PileDia, PileLength, AddtionalInfo);
                    }
                }
                else if (((string)pier["Type"]).StartsWith("MG"))
                {
                    DataRow FundMap = GetRowByKeyName("stdfundmap_tbl", "Name", PierName, ref CurConn, "nep_geo")[0];
                    double refDepth = double.Parse((string)FundMap["Depth"]);
                    FundName = PierName + "/F01";
                    AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[0])).Degrees;
                    Point2D CCofKJ = RefLeft - CBDir * (SpaseList[0]);
                    X = CCofKJ.X;
                    Y = CCofKJ.Y;
                    double H1 = FundHList[0];
                    if ((string)FundMap["Type"] == "KJ")
                    {
                        Type = "KJ";
                        H = 2;
                        if (((string)pier["Type"]).StartsWith("MG01"))
                        {
                            W = 7.5;
                            RealType = "SF02";
                        }
                        else if ((string)pier["Type"] == "MG02")
                        {
                            W = 7.5;
                            RealType = "SF02";
                        }
                        else if ((string)pier["Type"] == "MG03")
                        {
                            W = 8;
                            RealType = "SF03";
                        }
                        L = 5.8;
                        Z = H1 - (refDepth - H - 0.5);
                        AddOneFund(FundName, PierName, Type, RealType, X, Y, Z, AngFromNorth, W, L, H);
                    }
                    else
                    {
                        Type = (string)FundMap["Type"];
                        double PileDia = 2.0;
                        double CCofPile = Type == "MCZ" ? 2.5 * PileDia : 2.0 * PileDia;
                        double Length = double.Parse((string)FundMap["PileLength"]);
                        W = CCofPile + 1.6 * 2.0;
                        L = 3.2;
                        H = 2.5;
                        Z = H1 - (refDepth - H - Length - 0.5);
                        double HofPileTop = H1 - H;


                        if (CCofPile == 4)
                        {
                            if (Type != "MCZ")
                            {
                                RealType = "PF03";
                            }
                            else
                            {
                                RealType = "PF06";
                            }
                        }
                        else if (CCofPile == 5)
                        {
                            RealType = "PF04";
                        }
                        else if (CCofPile == 3.2)
                        {
                            RealType = "PF05";
                        }
                        AddOneFund(FundName, PierName, Type, RealType, X, Y, Z, AngFromNorth, W, L, H);
                        Point2D PierA = CCofKJ + CBDir * CCofPile * 0.5;
                        Point2D PierB = CCofKJ - CBDir * CCofPile * 0.5;
                        AddtionalInfo = Type == "MCZ" ? (string)FundMap["PileRe"] : "";
                        AddOneZJ(PierName + "/PI01", PierName, FundName, Type, PierA.X, PierA.Y, HofPileTop, PileDia, Length);
                        AddOneZJ(PierName + "/PI02", PierName, FundName, Type, PierB.X, PierB.Y, HofPileTop, PileDia, Length);
                    }
                }
                else
                {
                    // 框架墩基础配置
                    DataRow FPier = GetRowByPriKeyRow("framfund_tbl", PierName, "nep_geo");

                    if (FPier == null)
                    {
                        Console.WriteLine(PierName + "未查到基础配置\n");
                        continue;
                    }
                    Type = (string)FPier["Type"];
                    string depthtmp = (string)FPier["AllDepth"];
                    depthtmp = depthtmp.Replace("[", "");
                    depthtmp = depthtmp.Replace("]", "");
                    var AllDepth = (from a in depthtmp.Split(',') select double.Parse(a)).ToList();

                    depthtmp = (string)FPier["PileTopDepth"];
                    depthtmp = depthtmp.Replace("[", "");
                    depthtmp = depthtmp.Replace("]", "");
                    var PileTopDepth = (from a in depthtmp.Split(',') select double.Parse(a)).ToList();

                    //double cover = 0.5;// double.Parse((string)FPier["Cover"]);

                    string diatmp = (string)FPier["PileDia"];
                    diatmp = diatmp.Replace("[", "");
                    diatmp = diatmp.Replace("]", "");
                    var refDia = (from a in diatmp.Split(',') select double.Parse(a)).ToList();

                    string ratmp = (string)FPier["PileRe"];
                    ratmp = ratmp.Replace("[", "");
                    ratmp = ratmp.Replace("]", "");

                    var refRa = Type == "MCZ" ? (from a in ratmp.Split(',') select double.Parse(a)).ToList() : null;



                    var tmp = (
                        Regex.Split((string)FPier["Pile"], @"]], \[\["));
                    var tmp1 = from string a in tmp select a.Replace("[[[", "").Replace("]]]", "");
                    var PileLayout = (from string a in tmp1
                                      select
         ((from b in Regex.Split(a, @"], \[")
           select
(from c in b.Split(',') select double.Parse(c)).ToList()
           ).ToList())).ToList();

                    if (((string)pier["Type"]).StartsWith("F2"))
                    {
                        int FundIdx = 0;
                        double H1 = FundHList[0];
                        AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[FundIdx])).Degrees;
                        Point2D CCofKJ = RefLeft - CBDir * SpaseList[FundIdx];
                        X = CCofKJ.X;
                        Y = CCofKJ.Y;
                        Z = H1;
                        H = 2;
                        W = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[0];
                        L = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[1];
                        string AddInfo = Type == "MCZ" ? refRa[FundIdx].ToString() : "";
                        if (refDia[FundIdx] == 2.0)
                        {
                            RealType = "PF07";
                        }
                        else
                        {
                            RealType = "PF08";
                        }
                        AddOneFund(PierName + "/F01", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                        AddOneZJ(PierName + "/PI01", PierName, PierName + "/F01", Type, X, Y, H1 - H, refDia[FundIdx], AllDepth[FundIdx] - PileTopDepth[FundIdx], AddInfo);

                        FundIdx = 1;
                        H1 = FundHList[1];
                        AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[FundIdx])).Degrees;
                        CCofKJ = RefLeft - CBDir * SpaseList[FundIdx];
                        X = CCofKJ.X;
                        Y = CCofKJ.Y;
                        Z = H1;
                        H = 2;
                        W = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[0];
                        L = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[1];
                        AddInfo = Type == "MCZ" ? refRa[FundIdx].ToString() : "";
                        if (refDia[FundIdx] == 2.0)
                        {
                            RealType = "PF07";
                        }
                        else
                        {
                            RealType = "PF08";
                        }
                        AddOneFund(PierName + "/F02", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                        AddOneZJ(PierName + "/PI02", PierName, PierName + "/F02", Type, X, Y, H1 - H, refDia[FundIdx], AllDepth[FundIdx] - PileTopDepth[FundIdx], AddInfo);
                    }
                    else if (((string)pier["Type"]).StartsWith("F3"))
                    {
                        int FundIdx = 0;
                        double H1 = FundHList[FundIdx];
                        AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[FundIdx])).Degrees;
                        Point2D CCofKJ = RefLeft - CBDir * SpaseList[FundIdx];
                        X = CCofKJ.X;
                        Y = CCofKJ.Y;
                        H = 2;
                        Z = H1;
                        W = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[0];
                        L = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[1];
                        if (refDia[FundIdx] == 2.0)
                        {
                            RealType = "PF07";
                        }
                        else
                        {
                            RealType = "PF08";
                        }
                        string AddInfo = Type == "MCZ" ? refRa[FundIdx].ToString() : "";
                        AddOneFund(PierName + "/F01", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                        AddOneZJ(PierName + "/PI01", PierName, PierName + "/F01", Type, X, Y, H1 - H, refDia[FundIdx], AllDepth[FundIdx] - PileTopDepth[FundIdx], AddInfo);

                        FundIdx = 1;
                        H1 = FundHList[FundIdx];
                        AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[FundIdx])).Degrees;
                        CCofKJ = RefLeft - CBDir * SpaseList[FundIdx];
                        X = CCofKJ.X;
                        Y = CCofKJ.Y;
                        Z = H1;
                        H = 2;
                        W = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[0];
                        L = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[1];
                        if (refDia[FundIdx] == 2.0)
                        {
                            RealType = "PF07";
                        }
                        else
                        {
                            RealType = "PF08";
                        }
                        AddOneFund(PierName + "/F02", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                        AddInfo = Type == "MCZ" ? refRa[FundIdx].ToString() : "";
                        AddOneZJ(PierName + "/PI02", PierName, PierName + "/F02", Type, X, Y, H1 - H, refDia[FundIdx], AllDepth[FundIdx] - PileTopDepth[FundIdx], AddInfo);

                        FundIdx = 2;
                        H1 = FundHList[FundIdx];
                        AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[FundIdx])).Degrees;
                        CCofKJ = RefLeft - CBDir * SpaseList[FundIdx];
                        X = CCofKJ.X;
                        Y = CCofKJ.Y;
                        Z = H1;
                        H = 2;
                        W = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[0];
                        L = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[1];
                        if (refDia[FundIdx] == 2.0)
                        {
                            RealType = "PF07";
                        }
                        else
                        {
                            RealType = "PF08";
                        }
                        AddOneFund(PierName + "/F03", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                        AddInfo = Type == "MCZ" ? refRa[FundIdx].ToString() : "";
                        AddOneZJ(PierName + "/PI03", PierName, PierName + "/F03", Type, X, Y, H1 - H, refDia[FundIdx], AllDepth[FundIdx] - PileTopDepth[FundIdx], AddInfo);
                    }
                    else if (((string)pier["Type"]).StartsWith("FC1"))
                    {
                        double H1;
                        if (Math.Abs(SpaseList[0] - 10.8) < 1)
                        {
                            //先主墩
                            int FundIdx = 0;
                            H1 = FundHList[FundIdx];
                            AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[FundIdx])).Degrees;
                            Point2D CCofKJ = RefLeft - CBDir * SpaseList[FundIdx];
                            X = CCofKJ.X;
                            Y = CCofKJ.Y;
                            Z = H1;
                            H = 2.5;
                            W = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[0];
                            L = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[1];
                            double PileDia = refDia[FundIdx];
                            double CCofPile = Type == "MCZ" ? 2.5 * PileDia : 2.0 * PileDia;

                            if (CCofPile == 4)
                            {
                                if (Type != "MCZ")
                                {
                                    RealType = "PF03";
                                }
                                else
                                {
                                    RealType = "PF06";
                                }
                            }
                            else if (CCofPile == 5)
                            {
                                RealType = "PF04";
                            }
                            else
                            {
                                throw new Exception("FC1错误间距");
                            }
                            AddOneFund(PierName + "/F01", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                            Point2D CCofZ = CCofKJ + CBDir * CCofPile * 0.5;
                            string AddInfo = Type == "MCZ" ? refRa[0].ToString() : "";
                            AddOneZJ(PierName + "/PI01", PierName, PierName + "/F01", Type, CCofZ.X, CCofZ.Y, H1 - H, refDia[FundIdx], AllDepth[FundIdx] - PileTopDepth[FundIdx], AddInfo);
                            CCofZ = CCofKJ - CBDir * CCofPile * 0.5;
                            AddOneZJ(PierName + "/PI02", PierName, PierName + "/F01", Type, CCofZ.X, CCofZ.Y, H1 - H, refDia[FundIdx], AllDepth[FundIdx] - PileTopDepth[FundIdx], AddInfo);

                            FundIdx = 1;
                            H1 = FundHList[FundIdx];
                            AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[FundIdx])).Degrees;
                            CCofKJ = RefLeft - CBDir * SpaseList[FundIdx];
                            X = CCofKJ.X;
                            Y = CCofKJ.Y;
                            Z = H1;
                            H = 2.0;
                            W = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[0];
                            L = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[1];
                            if (refDia[FundIdx] == 2.0)
                            {
                                RealType = "PF07";
                            }
                            else
                            {
                                RealType = "PF08";
                            }
                            AddOneFund(PierName + "/F02", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                            AddInfo = Type == "MCZ" ? refRa[1].ToString() : "";
                            AddOneZJ(PierName + "/PI03", PierName, PierName + "/F02", Type, CCofKJ.X, CCofKJ.Y, H1 - H, refDia[FundIdx], AllDepth[FundIdx] - PileTopDepth[FundIdx], AddInfo);
                        }
                        else
                        {
                            int FundIdx = 0;
                            H1 = FundHList[FundIdx];
                            AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[FundIdx])).Degrees;
                            Point2D CCofKJ = RefLeft - CBDir * SpaseList[FundIdx];
                            X = CCofKJ.X;
                            Y = CCofKJ.Y;
                            H = 2;
                            W = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[0];
                            L = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[1];
                            if (refDia[FundIdx] == 2.0)
                            {
                                RealType = "PF07";
                            }
                            else
                            {
                                RealType = "PF08";
                            }
                            AddOneFund(PierName + "/F01", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                            string AddInfo = Type == "MCZ" ? refRa[0].ToString() : "";
                            AddOneZJ(PierName + "/PI01", PierName, PierName + "/F01", Type, CCofKJ.X, CCofKJ.Y, H1 - H, refDia[FundIdx], AllDepth[FundIdx] - PileTopDepth[FundIdx], AddInfo);

                            FundIdx = 1;
                            H1 = FundHList[FundIdx];
                            AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[FundIdx])).Degrees;
                            CCofKJ = RefLeft - CBDir * SpaseList[1];
                            X = CCofKJ.X;
                            Y = CCofKJ.Y;
                            H = 2.5;
                            double PileDia = refDia[FundIdx];
                            double CCofPile = Type == "MCZ" ? 2.5 * PileDia : 2.0 * PileDia;
                            W = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[0];
                            L = PierKnowledge.GetCTWL(PileLayout, refDia, FundIdx)[1];

                            if (CCofPile == 4)
                            {
                                if (Type != "MCZ")
                                {
                                    RealType = "PF03";
                                }
                                else
                                {
                                    RealType = "PF06";
                                }
                            }
                            else if (CCofPile == 5)
                            {
                                RealType = "PF04";
                            }
                            else
                            {
                                throw new Exception("FC1错误间距");
                            }
                            AddOneFund(PierName + "/F02", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                            AddInfo = Type == "MCZ" ? refRa[1].ToString() : "";
                            Point2D CCofZ = CCofKJ + CBDir * CCofPile * 0.5;
                            AddOneZJ(PierName + "/PI02", PierName, PierName + "/F02", Type, CCofZ.X, CCofZ.Y, H1 - H, refDia[FundIdx], AllDepth[FundIdx] - PileTopDepth[FundIdx], AddInfo);
                            CCofZ = CCofKJ - CBDir * CCofPile * 0.5;
                            AddOneZJ(PierName + "/PI03", PierName, PierName + "/F02", Type, CCofZ.X, CCofZ.Y, H1 - H, refDia[FundIdx], AllDepth[FundIdx] - PileTopDepth[FundIdx], AddInfo);
                        }
                    }
                    else if (((string)pier["Type"]).StartsWith("FC2"))
                    {
                        double H1;
                        if (Math.Abs((SpaseList[1] - SpaseList[0]) - 6) < 1)
                        {
                            H1 = FundHList[0];
                            AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[0])).Degrees;
                            Point2D CCofKJ = RefLeft - CBDir * 0.5 * (SpaseList[0] + SpaseList[1]);
                            double CCofPile = SpaseList[1] - SpaseList[0];
                            double PileDia = refDia[0];
                            X = CCofKJ.X;
                            Y = CCofKJ.Y;
                            H = 2.0;
                            W = Math.Round(CCofPile + PileDia * 1.6, 3);
                            L = PileDia * 1.6;
                            if (refDia[0] == 2)
                            {
                                RealType = "PF01";
                            }
                            else
                            {
                                RealType = "PF02";
                            }
                            AddOneFund(PierName + "/F01", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);

                            string AddInfo = Type == "MCZ" ? refRa[0].ToString() : "";
                            Point2D CCofZ = CCofKJ + CBDir * CCofPile * 0.5;
                            AddOneZJ(PierName + "/PI01", PierName, PierName + "/F01", Type, CCofZ.X, CCofZ.Y, H1 - H, refDia[0], AllDepth[0] - PileTopDepth[0], AddInfo);
                            CCofZ = CCofKJ - CBDir * CCofPile * 0.5;
                            AddOneZJ(PierName + "/PI02", PierName, PierName + "/F01", Type, CCofZ.X, CCofZ.Y, H1 - H, refDia[0], AllDepth[0] - PileTopDepth[0], AddInfo);

                            H1 = FundHList[2];
                            AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[1])).Degrees;
                            CCofKJ = RefLeft - CBDir * SpaseList[2];
                            X = CCofKJ.X;
                            Y = CCofKJ.Y;
                            H = 2;
                            W = PierKnowledge.GetCTWL(PileLayout, refDia, 1)[0];
                            L = PierKnowledge.GetCTWL(PileLayout, refDia, 1)[1];
                            if (refDia[1] == 2)
                            {
                                RealType = "PF07";
                            }
                            else
                            {
                                RealType = "PF08";
                            }
                            AddOneFund(PierName + "/F02", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                            PileDia = refDia[1];
                            CCofPile = Type == "MCZ" ? 2.5 * PileDia : 2.0 * PileDia;
                            AddOneZJ(PierName + "/PI03", PierName, PierName + "/F02", Type, CCofKJ.X, CCofKJ.Y, H1 - H, refDia[1], AllDepth[1] - PileTopDepth[1], AddInfo);

                        }
                        else
                        {
                            H1 = FundHList[0];
                            AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[0])).Degrees;
                            Point2D CCofKJ = RefLeft - CBDir * SpaseList[0];
                            X = CCofKJ.X;
                            Y = CCofKJ.Y;
                            H = 2;
                            W = PierKnowledge.GetCTWL(PileLayout, refDia, 0)[0];
                            L = PierKnowledge.GetCTWL(PileLayout, refDia, 0)[1];
                            if (refDia[0] == 2)
                            {
                                RealType = "PF07";
                            }
                            else
                            {
                                RealType = "PF08";
                            }
                            AddOneFund(PierName + "/F01", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                            double PileDia = refDia[0];
                            string AddInfo = Type == "MCZ" ? refRa[0].ToString() : "";
                            AddOneZJ(PierName + "/PI01", PierName, PierName + "/F01", Type, CCofKJ.X, CCofKJ.Y, H1 - H, refDia[0], AllDepth[0] - PileTopDepth[0], AddInfo);

                            H1 = FundHList[1];
                            AngFromNorth = (Vector2D.YAxis.SignedAngleTo(CBDir) + Angle.FromDegrees(FundAngList[1])).Degrees;
                            CCofKJ = RefLeft - CBDir * 0.5 * (SpaseList[1] + SpaseList[2]);
                            X = CCofKJ.X;
                            Y = CCofKJ.Y;
                            H = 2.0;
                            PileDia = refDia[1];
                            double CCofPile = SpaseList[2] - SpaseList[1];
                            W = Math.Round(CCofPile + PileDia * 1.6, 3);
                            L = PileDia * 1.6;
                            if (refDia[1] == 2)
                            {
                                RealType = "PF01";
                            }
                            else
                            {
                                RealType = "PF02";
                            }
                            AddOneFund(PierName + "/F02", PierName, Type, RealType, X, Y, H1, AngFromNorth, W, L, H);
                            AddInfo = Type == "MCZ" ? refRa[1].ToString() : "";
                            Point2D CCofZ = CCofKJ + CBDir * CCofPile * 0.5;
                            AddOneZJ(PierName + "/PI02", PierName, PierName + "/F02", Type, CCofZ.X, CCofZ.Y, H1 - H, refDia[1], AllDepth[1] - PileTopDepth[1], AddInfo);
                            CCofZ = CCofKJ - CBDir * CCofPile * 0.5;
                            AddOneZJ(PierName + "/PI03", PierName, PierName + "/F02", Type, CCofZ.X, CCofZ.Y, H1 - H, refDia[1], AllDepth[1] - PileTopDepth[1], AddInfo);
                        }
                    }
                    PublicFun.Info(i + 1, dt.Rows.Count);
                }

            }
        }

        internal void CreateBlkTbl()
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            if (!ExcistTbl("blk_tbl"))
            {
                string ColumnStr = "Name varchar(19),sub_name varchar(15)," +
                    "Type varchar(4),ParaList varchar(100)";
                CreatTableWithPriKey("blk_tbl", ColumnStr, "Name");
                CreatForeignKey("blk_tbl", "sub_name", "sub_tbl", "Name");
            }
        }

        private void AddOneZJ(string zjName, string pierName, string fundName, string typestr,
            double x, double y, double z, double dia, double l, string info = "")
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string RecString = string.Format("replace  INTO pile_tbl values('{0}','{1}','{2}','{3}',{4},{5},{6},{7},{8},'{9}');",
               zjName, pierName, fundName, typestr, x, y, z, dia, l, info);
            MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
            cmd.ExecuteNonQuery();
        }

        private void AddOneFund(string fundName, string pierName, string typestr, string realtypestr, double x, double y, double z, double angOfNorth, double w, double l, double h)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string RecString = string.Format("insert ignore INTO fund_tbl values('{0}','{1}','{2}',{3},{4},{5},{6},{7},{8},{9},'{10}');",
               fundName, pierName, typestr, x, y, z, angOfNorth, w, l, h, realtypestr);
            MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
            cmd.ExecuteNonQuery();
        }

        public void Shutdown()
        {
            Console.WriteLine("\n 按任意键继续..");
            Console.ReadKey();
        }
        #endregion


        #region 私有方法
        int UpdataRowByKeyName(string tblname, string KeyRowName, string KeyValue, string rowname, double rowvalue, ref MySqlConnection CurConn, string dbname = "")
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string olddbname = CurConn.Database;
            if (dbname != "")
            {
                CurConn.ChangeDatabase(dbname);
            }

            string RecString = string.Format("UPDATE {0} set {1}='{2}' where {3}='{4}';", tblname, rowname, rowvalue, KeyRowName, KeyValue);
            MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
            return cmd.ExecuteNonQuery();
        }

        int UpdataRowByKeyName(string tblname, string KeyRowName, string KeyValue, string rowname, string rowvalue, ref MySqlConnection CurConn, string dbname = "")
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string olddbname = CurConn.Database;
            if (dbname != "")
            {
                CurConn.ChangeDatabase(dbname);
            }

            string RecString = string.Format("UPDATE {0} set {1}='{2}' where {3}='{4}';", tblname, rowname, rowvalue, KeyRowName, KeyValue);
            MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
            return cmd.ExecuteNonQuery();
        }


        internal static DataRow[] GetRowByKeyName(string tblname, string rowname, string rowvalue, ref MySqlConnection CurConn, string dbname = "")
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string olddbname = CurConn.Database;
            if (dbname != "")
            {
                CurConn.ChangeDatabase(dbname);
            }

            string RecString = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", tblname, rowname, rowvalue);
            MySqlDataAdapter adapter = new MySqlDataAdapter(RecString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            System.Data.DataTable dt = dataset.Tables[0];

            if (dbname != "")
            {
                CurConn.ChangeDatabase(olddbname);
            }

            if (dt.Rows.Count != 0)
            {
                var res = (from DataRow a in dt.Rows select a).ToArray();
                return res;
            }
            else
            {
                return null;
            }


        }

        private DataRow GetRowByPriKeyRow(string tblname, string name, string DbNameInUse = "")
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            if (DbNameInUse != "")
            {
                CurConn.ChangeDatabase(DbNameInUse);
            }

            string RecString = string.Format("SELECT * FROM {1} WHERE Name='{0}'", name, tblname);
            MySqlDataAdapter adapter = new MySqlDataAdapter(RecString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            if (DbNameInUse != "")
            {
                CurConn.ChangeDatabase(MainDatabaseName);
            }
            if (dt.Rows.Count == 0)
            {
                return null;
            }
            return dt.Rows[0];
        }


        private string GetSubFromSpan(string span_name)
        {

            List<string> tmp = GetAllRelatedSpan(span_name);
            List<string> res = new List<string>();
            foreach (string spname in tmp)
            {
                string RecString = string.Format("SELECT Name FROM sub_tbl WHERE span_name='{0}'", spname);
                MySqlDataAdapter adapter = new MySqlDataAdapter(RecString, CurConn);
                DataSet dataset = new DataSet();
                adapter.Fill(dataset);
                adapter.Dispose();
                DataTable dt = dataset.Tables[0];
                foreach (DataRow item in dt.Rows)
                {
                    res.Add((string)item["Name"]);

                }
            }
            if (res.Count > 1)
            {
                throw new Exception("桥墩桥跨匹配错误");
            }
            else if (res.Count == 0)
            {
                string RecString = string.Format("SELECT Name FROM abut_tbl WHERE span_name='{0}'", span_name);
                MySqlDataAdapter adapter = new MySqlDataAdapter(RecString, CurConn);
                DataSet dataset = new DataSet();
                adapter.Fill(dataset);
                adapter.Dispose();
                DataTable dt = dataset.Tables[0];
                foreach (DataRow item in dt.Rows)
                {
                    res.Add((string)item["Name"]);
                }
            }
            if (res.Count == 0)
            {
                throw new Exception("桥墩桥跨匹配错误");
            }
            return res[0];
        }


        /// <summary>
        /// 根据桥跨名称获取前后联长
        /// </summary>
        private void GetGroupLength(string spanName, out double bkLength, out double frLength, ref List<DataRow> sec2MainBridge)
        {
            bkLength = 0;
            frLength = 0;
            var thisSpan = GetRowByKeyName("span_tbl", "Name", spanName, ref CurConn, CurConn.Database).First();
            string brname = (string)thisSpan["bridge_name"];
            List<DataRow> AllSpanWithEJ = new List<DataRow>();

            if (brname.StartsWith("SEC20") && brname != "SEC204R")
            {
                AllSpanWithEJ = sec2MainBridge;
                AllSpanWithEJ = (from a in AllSpanWithEJ where (string)a["DeckType"] == "EJ" select a).ToList();
            }
            else
            {
                var tmp = GetRowByKeyName("span_tbl", "bridge_name", brname, ref CurConn, CurConn.Database);
                AllSpanWithEJ = (from a in tmp where (string)a["DeckType"] == "EJ" select a).ToList();


            }


            var idx = (from a in AllSpanWithEJ where (string)a["Name"] == (string)thisSpan["Name"] select AllSpanWithEJ.IndexOf(a)).ToList()[0];
            try
            {
                bkLength = (double)AllSpanWithEJ[idx]["Station"] - (double)AllSpanWithEJ[idx - 1]["Station"];
                frLength = (double)AllSpanWithEJ[idx + 1]["Station"] - (double)AllSpanWithEJ[idx]["Station"];
                if (bkLength >= 160 || frLength >= 160)
                {
                    ;
                }
            }
            catch
            {
                ;
            }


            return;
        }

        private string GetDeckType(string spanName)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string RecString = string.Format("SELECT DeckType FROM span_tbl WHERE Name='{0}'", spanName);
            MySqlDataAdapter adapter = new MySqlDataAdapter(RecString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            return (string)dt.Rows[0]["DeckType"];
        }


        static string GetID(string line, double pK)
        {
            string Int = Math.Round(pK, 3, MidpointRounding.AwayFromZero).ToString("f3").PadLeft(9, '0');
            return line + string.Format("+{0}", Int);
        }

        private void UpdateCapBeamParameters(string brname)
        {
            string selectString = string.Format("SELECT * FROM sub_tbl where bridge_name='{0}' order by station", brname);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            double HL = 8000, HR = 8000;
            DataRow item;
            string Xstr, Ystr, Hstr;
            double PinC2C;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = dt.Rows[i];
                PinC2C = 0.54;

                string line = (string)item["align_name"];
                string br = (string)item["bridge_name"];
                double curPK = (double)item["Station"];
                string SpanName = (string)item["Name"];

                Align CL = AlignDict[line];

                DataTable BeamsToCalculate;
                selectString = string.Format("SELECT * FROM box_tbl where span_name='{0}'", SpanName);
                adapter = new MySqlDataAdapter(selectString, CurConn);
                dataset = new DataSet();
                adapter.Fill(dataset);
                adapter.Dispose();
                BeamsToCalculate = dataset.Tables[0];
                int numBeams = BeamsToCalculate.Rows.Count;
                bool UseSmallBeam = true;
                if (numBeams != 0)
                {
                    UseSmallBeam = false;
                }
                else
                {
                    if (i == 0)
                    {
                        //被动切割墩
                        continue;
                    }
                    double prePK = (double)dt.Rows[i - 1]["Station"];
                    selectString = string.Format("SELECT * FROM box_tbl where station={0} and bridge='{1}'", prePK, br);
                    adapter = new MySqlDataAdapter(selectString, CurConn);
                    dataset = new DataSet();
                    adapter.Fill(dataset);
                    adapter.Dispose();
                    BeamsToCalculate = dataset.Tables[0];
                    numBeams = BeamsToCalculate.Rows.Count;
                    if (numBeams != 0)
                    {
                        UseSmallBeam = true;
                    }
                    else
                    {
                        continue;
                    }
                }
                Point2D LeftStPin = new Point2D((double)BeamsToCalculate.Rows[0]["X0"], (double)BeamsToCalculate.Rows[0]["Y0"]);
                Point2D LeftEdPin = new Point2D((double)BeamsToCalculate.Rows[0]["X1"], (double)BeamsToCalculate.Rows[0]["Y1"]);
                Point2D RightStPin = new Point2D((double)BeamsToCalculate.Rows[numBeams - 1]["X0"], (double)BeamsToCalculate.Rows[numBeams - 1]["Y0"]);
                Point2D RightEdPin = new Point2D((double)BeamsToCalculate.Rows[numBeams - 1]["X1"], (double)BeamsToCalculate.Rows[numBeams - 1]["Y1"]);
                Point2D CCPin = new Point2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);
                Point2D LeftPt, RightPt;
                if (UseSmallBeam)
                {
                    LeftPt = LeftEdPin + (LeftEdPin - LeftStPin).Normalize() * PinC2C;
                    RightPt = RightEdPin + (RightEdPin - RightStPin).Normalize() * PinC2C;
                }
                else
                {
                    LeftPt = LeftStPin + (LeftStPin - LeftEdPin).Normalize() * PinC2C;
                    RightPt = RightStPin + (RightStPin - RightEdPin).Normalize() * PinC2C;
                }

                HL = CL.GetSurfaceBG(LeftPt.X, LeftPt.Y) - (BeamKnowledge.SurfaceH + BeamKnowledge.BeamH + BeamKnowledge.BearingTotalH);
                HR = CL.GetSurfaceBG(RightPt.X, RightPt.Y) - (BeamKnowledge.SurfaceH + BeamKnowledge.BeamH + BeamKnowledge.BearingTotalH);

                double ConsiderDist = LeftPt.DistanceTo(RightPt);
                double MiddleToLeft = LeftPt.DistanceTo(CCPin);

                double CapBeamSlop = (HR - HL) / ConsiderDist * 100.0;
                double CapBeamCenterH = HL + CapBeamSlop / 100.0 * MiddleToLeft;
                int idd = (int)(curPK * 100);

                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                string RecString = string.Format("INSERT INTO CapBeam_tbl values({0},'{7}','{1}',{2},{3},{4},{5},{6})" +
                    " ON DUPLICATE KEY UPDATE X={3},Y={4},H={5},Slop={6};"
                    , idd, brname, curPK, CCPin.X, CCPin.Y, CapBeamCenterH, CapBeamSlop, line);
                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();

            }

            Console.WriteLine("#  {0} 已配置盖梁参数...", brname);
        }

        private string GetCLBySpanName(string sp)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string RecString = string.Format("SELECT align_name FROM span_tbl WHERE Name='{0}'", sp);
            MySqlDataAdapter adapter = new MySqlDataAdapter(RecString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            return (string)dt.Rows[0]["align_name"];
        }



        private double[] GetCapParameters(Vector2D CPDir, Point2D CapCenter, List<PlinthStruc> beams)
        {
            var ij = (from PlinthStruc a in beams where a.Name.EndsWith("01") select beams.IndexOf(a)).ToList();

            if (beams.Count == 0)
            {
                return new double[] { 100000000, 100000000 };
            }
            if (ij.Count == 1)
            {
                List<PlinthStruc> beamsGroupBy = new List<PlinthStruc>() { beams[0], beams.Last() };
                return GetGroupBeamPara(beamsGroupBy, CPDir, CapCenter);

            }
            else if (ij.Count == 2)
            {
                // 当具有两组桥面
                List<PlinthStruc> beams1 = beams.GetRange(0, ij[1]);
                List<PlinthStruc> beams2 = beams.GetRange(ij[1], beams.Count - ij[1]);

                List<CBDeterminID> slopTuple = GetCBParameterList(beams1, beams2, CPDir, CapCenter);

                slopTuple = (from a in slopTuple where Math.Abs(a.SlopValue) <= 5 select a).ToList();

                slopTuple.Sort((x, y) => x.H.CompareTo(y.H));
                int point = 0;


                bool PassTest = false;

                foreach (var item in slopTuple)
                {
                    //判别条件：每个都不是负值
                    PassTest = true;
                    foreach (var beam in beams1)
                    {
                        int sign = (beam.PinLoc - CapCenter).AngleTo(CPDir).Degrees <= 90.0 ? -1 : 1;
                        double HinReal = item.H + sign * (beam.Loc.DistanceTo(CapCenter)) * item.SlopValue * 0.01;
                        if (HinReal - beam.Hineed > 0.1)
                        {
                            PassTest = false;
                            break;
                        }
                    }
                    if (PassTest)
                    {
                        foreach (var beam in beams2)
                        {
                            int sign = (beam.PinLoc - CapCenter).AngleTo(CPDir).Degrees <= 90.0 ? -1 : 1;
                            double HinReal = item.H + sign * (beam.Loc.DistanceTo(CapCenter)) * item.SlopValue * 0.01;
                            if (HinReal - beam.Hineed > 0.1)
                            {
                                PassTest = false;
                                break;
                            }
                        }
                    }



                    if (PassTest)
                    {
                        break;
                    }
                    point += 1;
                }

                List<PlinthStruc> beamsGroupBy = new List<PlinthStruc>();
                if (PassTest)
                {
                    beamsGroupBy = new List<PlinthStruc>() { beams1[slopTuple[point].A], beams2[slopTuple[point].B] };
                }
                else
                {      //特殊处理双幅桥面
                    ;
                    beamsGroupBy = new List<PlinthStruc>() { beams1[0], beams1.Last() };

                }


                //beams1.Sort((x, y) => x.Hineed.CompareTo(y.Hineed));
                //beams2.Sort((x, y) => x.Hineed.CompareTo(y.Hineed));


                return GetGroupBeamPara(beamsGroupBy, CPDir, CapCenter);

            }
            else
            {
                // 三组桥面特殊处理
                ;

                if (beams[0].Name == "SEC2/ML04L/S41/G01" && beams[10].Name == "SEC2/ML04R/S35/G04")
                {
                    List<PlinthStruc> beamsGroupBy = new List<PlinthStruc>() { beams[0], beams[7] };

                    return GetGroupBeamPara(beamsGroupBy, CPDir, CapCenter);

                }
                if (beams[0].Name == "SEC2/ML04L/S40/G01" && beams[10].Name == "SEC2/ML04R/S34/G01")
                {
                    List<PlinthStruc> beamsGroupBy = new List<PlinthStruc>() { beams[0], beams[10] };

                    return GetGroupBeamPara(beamsGroupBy, CPDir, CapCenter);

                }
                throw new Exception();

            }





        }

        private List<CBDeterminID> GetCBParameterList(List<PlinthStruc> beams1, List<PlinthStruc> beams2, Vector2D CapDir, Point2D CapCenter)
        {
            List<CBDeterminID> res = new List<CBDeterminID>();

            foreach (var i in beams1)
            {
                foreach (var j in beams2)
                {
                    CBDeterminID item = new CBDeterminID();
                    var pp = GetGroupBeamPara(new List<PlinthStruc>() { i, j }, CapDir, CapCenter);
                    item.SlopValue = pp[0];
                    item.A = beams1.IndexOf(i);
                    item.B = beams2.IndexOf(j);
                    item.H = pp[1];
                    res.Add(item);

                }

            }

            return res;
        }

        private double[] GetGroupBeamPara(List<PlinthStruc> beams, Vector2D CPDir, Point2D CapCenter)
        {

            beams.Sort((x, y) => x.Hineed.CompareTo(y.Hineed));

            double dist = beams[0].Loc.DistanceTo(beams[1].Loc);
            double H_delt = beams[1].Hineed - beams[0].Hineed;
            Vector2D dirF = beams[0].Loc - beams[1].Loc;
            int sign = 1;
            if (CPDir.AngleTo(dirF).Degrees >= 90.0)
            {
                sign = -1;
            }

            double SlopInReal = sign * H_delt / dist * 100.0;
            SlopInReal = PierKnowledge.GetCapBeamSlop(SlopInReal);

            double cc2loc = CapCenter.DistanceTo(beams[0].Loc);

            Vector2D control2loc = beams[0].Loc - CapCenter;
            int slopSign = 1;
            if (CPDir.AngleTo(control2loc).Degrees >= 90.0)
            {
                slopSign = -1;
            }

            double H0InReal = slopSign * cc2loc * SlopInReal / 100.0 + beams[0].Hineed;

            return new double[] { SlopInReal, H0InReal };
        }

        private List<string> GetAllRelatedSpan(string spanName)
        {
            List<string> res = new List<string>() { spanName };
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string RecString = string.Format("SELECT Name FROM span_tbl WHERE cut_by='{0}'", spanName);
            MySqlDataAdapter adapter = new MySqlDataAdapter(RecString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];

            foreach (DataRow item in dt.Rows)
            {
                res.Add((string)item["Name"]);
            }

            RecString = string.Format("SELECT cut_by FROM span_tbl WHERE Name='{0}'", spanName);
            adapter = new MySqlDataAdapter(RecString, CurConn);
            dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            dt = dataset.Tables[0];

            foreach (DataRow item in dt.Rows)
            {
                if ((string)item["cut_by"] != "None")
                {
                    res.Add((string)item["cut_by"]);
                }
            }

            return res.Distinct().ToList();
        }

        private DxfDocument GetConsiderLine(string item)
        {
            if (!item.StartsWith("sec", StringComparison.OrdinalIgnoreCase))
            {
                return EmptyRoad;
            }
            else if (item.StartsWith("sec210", StringComparison.OrdinalIgnoreCase))
            {
                return EmptyRoad;
            }
            else if (item.StartsWith("sec204r", StringComparison.OrdinalIgnoreCase))
            {
                return A8RoadR;
            }
            else if (item.StartsWith("sec204l", StringComparison.OrdinalIgnoreCase))
            {
                return A8RoadL;
            }
            else
            {
                return A8Road;
            }
        }

        /// <summary>
        /// 重写数据
        /// </summary>
        /// <param name="v"></param>

        private string UpdateMySql(double station, string bridge, double value, double condition, string key, bool force = false)
        {
            string res = string.Empty;
            if (force)
            {
                res = string.Format("UPDATE sub_tbl set {0}={1} where Station={2} and bridge_name='{3}';", key, value, station, bridge);
            }
            else
            {
                if (value != condition)
                {
                    res = string.Format("UPDATE sub_tbl set {0}={1} where Station={2} and bridge_name='{3}';", key, value, station, bridge);
                }
            }

            return res;
        }

        private string UpdateMySql(double station, string bridge, string value, string condition, string key, bool force = false)
        {
            string res = string.Empty;
            if (force)
            {
                res = string.Format("UPDATE sub_tbl set {0}='{1}' where Station={2} and bridge_name='{3}';", key, value, station, bridge);
            }
            else
            {
                if (value != condition)
                {
                    res = string.Format("UPDATE sub_tbl set {0}='{1}' where Station={2} and bridge_name='{3}';", key, value, station, bridge);
                }
            }

            return res;
        }

        private string UpdateBoxHRecorder(string tbname, int id, string bridge, string column, double value)
        {
            string res = string.Format("UPDATE {0} set {1}={2} where (ID,Bridge)=({3},'{4}');", tbname, column, value, id, bridge);
            return res;
        }


        private string UpdateRecorder(string tbname, int id, double station, string bridge, string column, string value)
        {
            string res = string.Format("UPDATE '{0}' set '{1}'='{2}' where Station={3} and Bridge='{4}' and id={5};", tbname, column, value, station, bridge, id);
            return res;
        }
        private string UpdateRecorder(string tbname, int id, double station, string bridge, string column, double value)
        {
            string res = string.Format("UPDATE '{0}' set '{1}'={2} where Station={3} and Bridge='{4}' and id={5};", tbname, column, value, station, bridge, id);
            return res;
        }

        private DxfDocument GetSideLine(string br)
        {
            DxfDocument dxftocut = null;
            string dxfilename = "";
            if (br == "SEC204R")
            {
                dxftocut = RKRoad;
            }
            else if (br == "SEC204L")
            {
                dxftocut = LKRoad;
            }
            else if (br.StartsWith("SEC"))
            {
                dxftocut = MainRoad;
            }
            //else if (new string[] { "SBI05" }.Contains(br))
            //{
            //    return null;
            //}
            else
            {
                dxfilename = string.Format("{0}边线.dxf", br);
                dxftocut = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", dxfilename)[0]);
            }
            return dxftocut;
        }


        /// <summary>
        /// 更新横断信息
        /// </summary>
        /// <param name="br"></param>
        private void MakeHP(string br)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string selectString = string.Format("SELECT * FROM span_tbl where bridge_name='{0}'", br);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();

            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            foreach (DataRow item in dt.Rows)
            {
                string id = (string)item["Name"];
                Align CL = AlignDict[(string)item["align_name"]];
                double st = (double)item["Station"];
                var res = CL.curCG.GetHP(st);

                string RecString = string.Format("UPDATE span_tbl set HPL={0},HPR={1} where name='{2}'", res[0], res[1], id);
                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();



            }
        }


        /// <summary>
        /// 更新伸缩缝信息
        /// </summary>
        /// <param name="br"></param>
        private void MakeEJ(string br)
        {
            if (br == "CCI01")
            {
                ;
            }
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string selectString = string.Format("SELECT * FROM span_tbl where bridge_name='{0}'", br);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();

            string altexticd = File.ReadAllText(EJFilePath, Encoding.Default);
            List<string> EJList = altexticd.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
            int i = 0;
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            bool ShouldUpdate = false;
            foreach (DataRow item in dt.Rows)
            {
                ShouldUpdate = false;
                string id = (string)item["Name"];
                if ((string)item["PierType"] == "A")
                {
                    ShouldUpdate = true;
                }
                if (EJList.Contains(id))
                {
                    ShouldUpdate = true;
                }
                if (ShouldUpdate)
                {
                    string RecString = string.Format("UPDATE span_tbl set DeckType='EJ' where name='{0}'", id);
                    MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                    cmd.ExecuteNonQuery();
                }

                i++;
            }

        }


        /// <summary>
        /// 切割其他路线
        /// </summary>
        /// <param name="br"></param>
        private void CurOther(string br)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string selectString = string.Format("SELECT * FROM span_tbl where bridge_name='{0}'", br);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();

            foreach (DataRow item in dt.Rows)
            {
                string tmp = (string)item["cut_to"];
                var RampNameList = tmp.Split('|');
                foreach (string RampName in RampNameList)
                {
                    if (RampName != "None")
                    {
                        DxfDocument RampSideLine = GetSideLine(RampName);
                        Align CL = AlignDict[(string)item["align_name"]];
                        Align RampCL = AlignDict[BridgeToLine[RampName]];


                        double curPK = (double)item["Station"];
                        Point2D MLCC = new Point2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);
                        Vector2D Dir = new Vector2D(CL.curPQX.GetDir(curPK)[0], CL.curPQX.GetDir(curPK)[1]);
                        Vector2D Cdir = Dir.Rotate(Angle.FromDegrees((double)item["Angle"]));

                        if (curPK == 19806)
                        {
                            ;
                        }
                        double pk0 = RampCL.curPQX.GetStation(MLCC.X, MLCC.Y, (MLCC + Cdir).X, (MLCC + Cdir).Y, 0);
                        //double ttpk0 = RampCL.curPQX.GetStation2(MLCC.X, MLCC.Y, (MLCC + Cdir).X, (MLCC + Cdir).Y, 0);

                        List<Vector2> AList, BList;
                        Vector2 RampCC = new Vector2(RampCL.curPQX.GetCoord(pk0)[0], RampCL.curPQX.GetCoord(pk0)[1]);

                        CutDxf(new Vector2(MLCC.X, MLCC.Y), new Vector2(Cdir.X, Cdir.Y), ref RampSideLine, out AList, out BList);

                        double WidthLeft, WidthRight;
                        List<double> DistList = new List<double>();
                        Angle angle;
                        DistList.Add(new Point2D(RampCC.X, RampCC.Y).DistanceTo(MLCC));
                        foreach (var pt in AList)
                        {
                            DistList.Add(new Point2D(pt.X, pt.Y).DistanceTo(MLCC));
                        }
                        foreach (var pt in BList)
                        {
                            DistList.Add(new Point2D(pt.X, pt.Y).DistanceTo(MLCC));
                        }
                        DistList.Sort();
                        if (AList.Count == 0 && BList.Count == 2)
                        {
                            WidthLeft = DistList[1] - DistList[0];
                            WidthRight = DistList[2] - DistList[1];
                            angle = RampCL.curPQX.GetDirVector2D(pk0).SignedAngleTo(MLCC - new Point2D(RampCC.X, RampCC.Y));
                        }
                        else if (AList.Count == 2 && BList.Count == 0)
                        {
                            WidthLeft = DistList[2] - DistList[1];
                            WidthRight = DistList[1] - DistList[0];
                            angle = RampCL.curPQX.GetDirVector2D(pk0).SignedAngleTo(new Point2D(RampCC.X, RampCC.Y) - MLCC);
                        }
                        else if (AList.Count == 3 && BList.Count == 0)
                        {
                            WidthLeft = DistList[3] - DistList[1];
                            WidthRight = DistList[1] - DistList[0];
                            angle = RampCL.curPQX.GetDirVector2D(pk0).SignedAngleTo(new Point2D(RampCC.X, RampCC.Y) - MLCC);
                        }
                        else
                        {

                            throw new Exception("切割状态有误");
                        }
          ;

                        if (angle.Degrees >= 180)
                        {
                            angle = Angle.FromDegrees(angle.Degrees - 180.0);
                        }
                        string beamtype = "B";
                        //if (curPK==19766||curPK==19806)
                        //{
                        //    beamtype = "C";
                        //}
                        //if (curPK == 350.6 &&br.StartsWith("HSI01",StringComparison.OrdinalIgnoreCase))
                        //{
                        //    beamtype = "X";                            
                        //}
                        var flist = new List<string>() { "CCB+00538.075", "CCC+00592.351", "HSC+00445.480", "HSF+00503.731", "MUB+00458.721", "R1K+19939.178" };
                        var thisName = GetID(BridgeToLine[RampName], pk0);
                        if (flist.Contains(thisName))
                        {
                            beamtype = "N";
                        }

                        double deck_wl = WidthLeft;
                        double deck_wr = WidthRight;
                        double back_wl = WidthLeft;
                        double back_wr = WidthRight;
                        double front_wl = WidthLeft;
                        double front_wr = WidthRight;
                        if (thisName == "HSA+00350.582")
                        {
                            front_wl = 4.5103;
                        }

                        // 调整逆方向介入匝道的左右宽度定义;

                        var RampToChange = new List<string>() { "CCC+00592.351", "MUB+00458.721" };
                        if (RampToChange.Contains(thisName))
                        {
                            deck_wr = WidthLeft;
                            deck_wl = WidthRight;
                            back_wr = WidthLeft;
                            back_wl = WidthRight;
                            front_wr = WidthLeft;
                            front_wl = WidthRight;

                        }


                        string RecString = string.Format("INSERT ignore INTO span_tbl values('{0}','{1}','{2}',{3},{4}," +
                            "{5},{6},{7},{8},{9},{10}," +
                            "'{11}','C','CT','None','{12}',0,0);",
                            thisName, BridgeToLine[RampName], RampName, pk0, angle.Degrees,
                             deck_wl, deck_wr, back_wl, back_wr, front_wl, front_wr, beamtype, GetID(BridgeToLine[br], curPK));

                        MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            CurConn.Close();

        }


        /// <summary>
        /// 指定宽度
        /// </summary>
        private void CutDxf(Vector2 Center, Vector2 Cdir, ref DxfDocument SideLineToCut, out List<Vector2> AList, out List<Vector2> BList)
        {
            AList = new List<Vector2>();
            BList = new List<Vector2>();

            Line A = new Line(Center, Center + Cdir * 50);
            Line B = new Line(Center, Center - Cdir * 50);
            foreach (Line line in SideLineToCut.Lines)
            {
                var f = A.Intersectwith(line.Flatten());
                if (f != null)
                {
                    Vector2 pt = (Vector2)f;
                    AList.Add(pt);
                }
                var g = B.Intersectwith(line.Flatten());
                if (g != null)
                {
                    Vector2 pt = (Vector2)g;
                    BList.Add(pt);
                }
            }
            foreach (netDxf.Entities.Arc arc in SideLineToCut.Arcs)
            {
                var a = A.Intersectwith(arc);
                //var test = A.Intersectwith2(arc);
                if (a != null)
                {
                    Vector2 pt = (Vector2)a;
                    AList.Add(pt);
                }
                var b = B.Intersectwith(arc);
                if (b != null)
                {
                    Vector2 pt = (Vector2)b;
                    BList.Add(pt);
                }
            }
        }


        private string UpdateWidthNew(string br, ref DxfDocument SideLineToCut)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }

            string selectString = string.Format("SELECT * FROM span_tbl where bridge_name='{0}'", br);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();


            List<double> res = new List<double>();
            List<double[]> AList = new List<double[]>();
            List<double[]> BList = new List<double[]>();

            List<string> CommPier = new List<string>() { // 共用墩位置
                "L1K+18798.000", "L1K+18828.000", "L1K+18858.000", "L1K+18888.000", "L1K+18914.000",
                "L1K+18940.000", "L1K+18970.000", "L1K+19000.000", "L1K+19030.000", "L1K+19060.000", "L1K+19085.000",
                "L1K+19841.000","L1K+19871.000", "L1K+19901.000" ,  };
            List<string> ComeTogether = new List<string>() {//入口位置
                "HSA+00210.600",
                "M1K+16895.000", "M1K+17255.000", "M2K+22507.000", "L1K+19931.000" ,"R1K+19495.000"};
            List<string> SpecialStation = new List<string>() {//特殊位置
                "L1K+19556.000"};

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow item = dt.Rows[i];
                Align CL = AlignDict[(string)item["align_name"]];
                double curPK = (double)item["Station"];
                Angle Theta = Angle.FromDegrees((double)item["Angle"]);

                res.Add(curPK);

                if (curPK == 21296)
                {
                    ;

                }
                Vector2 Center = new Vector2(CL.curPQX.GetCoord(curPK));
                Vector2 dir = new Vector2(CL.curPQX.GetDir(curPK));
                Vector2 Cdir = dir.RotByZ(Theta.Radians);

                Line A = new Line(Center, Center + Cdir * 50);
                Line B = new Line(Center, Center - Cdir * 50);
                List<double> DistA = new List<double>();
                List<double> DistB = new List<double>();
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
                if (DistA.Count == 1)
                {
                    DistA.Add(DistA[0]);
                }
                if (DistB.Count == 1)
                {
                    DistB.Add(DistB[0]);
                }
                DistA.Sort();
                DistB.Sort();
                AList.Add(DistA.ToArray());
                BList.Add(DistB.ToArray());
            }
            for (int k = 0; k < res.Count; k++)
            {
                DataRow item = dt.Rows[k];
                string Name = GetID((string)item["align_name"], res[k]);
                if (Name == "M2K+21296.000")
                {
                    ;
                }
                double deck_wl = Math.Round(AList[k][1], 12, MidpointRounding.AwayFromZero);
                double deck_wr = Math.Round(BList[k][1], 12, MidpointRounding.AwayFromZero);

                double front_wl = Math.Round(AList[k][0], 12, MidpointRounding.AwayFromZero);
                double front_wr = Math.Round(BList[k][0], 12, MidpointRounding.AwayFromZero);

                double back_wl = Math.Round(AList[k][1], 12, MidpointRounding.AwayFromZero);
                double back_wr = Math.Round(BList[k][1], 12, MidpointRounding.AwayFromZero);

                if (CommPier.Contains(Name))
                {
                    back_wl = Math.Round(AList[k][0], 12, MidpointRounding.AwayFromZero);
                    back_wr = Math.Round(BList[k][0], 12, MidpointRounding.AwayFromZero);
                }
                else if (ComeTogether.Contains(Name))
                {
                    back_wl = Math.Round(AList[k][0], 12, MidpointRounding.AwayFromZero);
                    back_wr = Math.Round(BList[k][0], 12, MidpointRounding.AwayFromZero);
                    front_wl = Math.Round(AList[k][1], 12, MidpointRounding.AwayFromZero);
                    front_wr = Math.Round(BList[k][1], 12, MidpointRounding.AwayFromZero);
                }
                else if (SpecialStation.Contains(Name))
                {
                    back_wl = Math.Round(AList[k][1], 12, MidpointRounding.AwayFromZero);
                    back_wr = Math.Round(BList[k][0], 12, MidpointRounding.AwayFromZero);
                    front_wl = Math.Round(AList[k][0], 12, MidpointRounding.AwayFromZero);
                    front_wr = Math.Round(BList[k][1], 12, MidpointRounding.AwayFromZero);
                }


                //if (Name == "HSA+00210.600")
                //{
                //    front_wl += 4;
                //}

                string RecString = string.Format("UPDATE span_tbl set deck_wl={1},deck_wr={2}, back_wl={3},back_wr={4},front_wl={5},front_wr={6} where Name='{0}';",
                   GetID((string)item["align_name"], res[k]), deck_wl, deck_wr, back_wl, back_wr, front_wl, front_wr);
                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();
            }
            Console.WriteLine("#  " + br + "  已更新桥宽度...");



            return "";
        }
        /// <summary>
        /// 切割宽度
        /// </summary>
        private void UpdateWidth(string v, ref DxfDocument SideLineToCut)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }

            string selectString = string.Format("SELECT * FROM {0}", v);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();

            List<double> res = new List<double>();
            List<double[]> AList = new List<double[]>();
            List<double[]> BList = new List<double[]>();


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow item = dt.Rows[i];
                Align CL = AlignDict[(string)item["Line"]];
                double curPK = (double)item["Station"];
                Angle Theta = Angle.FromDegrees((double)item["Angle"]);

                res.Add(curPK);
                Vector2 Center = new Vector2(CL.curPQX.GetCoord(curPK));
                Vector2 dir = new Vector2(CL.curPQX.GetDir(curPK));
                Vector2 Cdir = dir.RotByZ(Theta.Radians);

                Line A = new Line(Center, Center + Cdir * 50);
                Line B = new Line(Center, Center - Cdir * 50);
                List<double> DistA = new List<double>();
                List<double> DistB = new List<double>();
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
                if (DistA.Count == 1)
                {
                    DistA.Add(DistA[0]);
                }
                if (DistB.Count == 1)
                {
                    DistB.Add(DistB[0]);
                }
                DistA.Sort();
                DistB.Sort();
                AList.Add(DistA.ToArray());
                BList.Add(DistB.ToArray());
            }
            for (int k = 0; k < res.Count; k++)
            {

                double dl = Math.Round(AList[k][1], 12, MidpointRounding.AwayFromZero);
                double dr = Math.Round(BList[k][1], 12, MidpointRounding.AwayFromZero);
                double dlreal = Math.Round(AList[k][0], 12, MidpointRounding.AwayFromZero);
                double drreal = Math.Round(BList[k][0], 12, MidpointRounding.AwayFromZero);
                string RecString = string.Format("UPDATE {0} set WidthRight={1},WidthLeft={2},RealRight={3},RealLeft={4} where Station={5};",
                    v, dr, dl, drreal, dlreal, res[k]);
                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();
            }
            Console.WriteLine("#  " + v + "  已更新桥宽度...");
            CurConn.Close();
        }
        public void UpdateColumn(string brname, ref DxfDocument SideLine, ref Dictionary<string, Align> CLList)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string selectString = string.Format("SELECT * FROM span_tbl where bridge_name='{0}' order by Station", brname);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();
            int PierCount = 1;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow item = dt.Rows[i];


                string LineName = (string)item["align_name"];
                string BridgeName = (string)item["bridge_name"];
                Align CL = CLList[(string)item["align_name"]];
                string trName = (string)item["bridge_name"];// == "SEC2-10" ? "SEC210" : string.Join("0", ((string)item["Bridge"]).Split('-').ToArray());

                double deck_wl = (double)item["deck_wl"];
                double deck_wr = (double)item["deck_wr"];
                double curPK = (double)item["Station"];
                double WidthLeft = deck_wl;
                double WidthRight = deck_wr;
                Angle Theta = Angle.FromDegrees((double)item["Angle"]);
                //Align RefCL = GetCLByName((string)item["FundType"]);

                Vector2D Center = new Vector2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);
                Vector2D dir = new Vector2D(CL.curPQX.GetDir(curPK)[0], CL.curPQX.GetDir(curPK)[1]);
                Vector2D Cdir = dir.Rotate(Theta);

                SubStructure thisSB = new PierNone();



                if ((string)item["PierType"] == "A")
                {
                    MySqlCommand cmdA;
                    if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                    string SecNameA = PublicFun.GetSecName(brname);
                    string OfficalBrNameA = PublicFun.GetOfficalBridgeName(brname);
                    string A1 = i == dt.Rows.Count - 1 ? "1" : "0";
                    string abutname = SecNameA + "/" + OfficalBrNameA + "/A" + A1;
                    double H0 = CL.curSQX.GetBG(curPK);
                    double H1 = CL.curDMX.GetBG(curPK);
                    var HP = CL.curCG.GetHP(curPK);
                    string cmdstr = string.Format("replace into abut_tbl values('{0}','{1}','{2}','{3}',{4},'{5}',{6},{7},{8},{9},{10},{11},{12});",
                        abutname, item["Name"], item["align_name"], item["bridge_name"], curPK, "Abut", Theta.Degrees,
                        deck_wl, deck_wr, H0, H1, HP[0], HP[1]);
                    cmdA = new MySqlCommand(cmdstr, CurConn);
                    cmdA.ExecuteNonQuery();
                    continue;
                }
                if ((string)item["cut_by"] != "None")
                {
                    continue;
                }

                if (brname.StartsWith("SEC", StringComparison.OrdinalIgnoreCase) || brname == "MLI01" || brname == "SGR01")
                {
                    double DistLR = 0;
                    if (LineName == "L1K" && (curPK <= 19100 || curPK >= 19750))
                    {
                        Align RK = AlignDict["R1K"];

                        double RKPK = RK.curPQX.GetStation(Center.X, Center.Y, (Center + Cdir).X, (Center + Cdir).Y);
                        Vector2D CenterofRK = new Vector2D(RK.curPQX.GetCoord(RKPK)[0], RK.curPQX.GetCoord(RKPK)[1]);
                        DistLR = (Center - CenterofRK).Length;
                        Center = Center - Cdir * 0.5 * DistLR;

                        WidthLeft += 0.5 * DistLR;
                        WidthRight -= 0.5 * DistLR;
                    }
                    Line A = new Line(Center.Convert2(), Center.Convert2() + Cdir.Convert2() * 50);
                    Line B = new Line(Center.Convert2(), Center.Convert2() - Cdir.Convert2() * 50);

                    List<Vector2D> DistListLeft = new List<Vector2D>();
                    List<Vector2D> DistListRight = new List<Vector2D>();

                    foreach (Line line in SideLine.Lines)
                    {
                        var f = A.Intersectwith(line.Flatten());
                        if (f != null)
                        {
                            Vector2D pt = ((Vector2)f).Convert2DS();
                            DistListLeft.Add(pt - Center);
                        }
                        var g = B.Intersectwith(line.Flatten());
                        if (g != null)
                        {
                            Vector2D pt = ((Vector2)g).Convert2DS();
                            DistListRight.Add(pt - Center);
                        }
                    }
                    foreach (netDxf.Entities.Arc line in SideLine.Arcs)
                    {
                        var f = A.Intersectwith(line);
                        if (f != null)
                        {
                            Vector2D pt = ((Vector2)f).Convert2DS();
                            DistListLeft.Add(pt - Center);
                        }
                        var g = B.Intersectwith(line);
                        if (g != null)
                        {
                            Vector2D pt = ((Vector2)g).Convert2DS();
                            DistListRight.Add(pt - Center);
                        }
                    }

                    foreach (Circle cc in SideLine.Circles)
                    {
                        var f = A.Intersectwith(cc);
                        if (f != null)
                        {
                            Vector2D pt = ((Vector2)f).Convert2DS();
                            DistListLeft.Add(pt - Center);
                        }
                        var g = B.Intersectwith(cc);
                        if (g != null)
                        {
                            Vector2D pt = ((Vector2)g).Convert2DS();
                            DistListRight.Add(pt - Center);
                        }
                    }


                    DistListLeft.Sort((x, y) => x.Length.CompareTo(y.Length));
                    DistListRight.Sort((x, y) => x.Length.CompareTo(y.Length));

                    //SubStructure thisSB = PierKnowledge.Arrange3(dt, i, ref CL, ref RefCL,DistListLeft,DistListRight);
                    thisSB = PierKnowledge.Arrange2(LineName, curPK, Theta, WidthLeft, WidthRight, DistListLeft, DistListRight, ref CL, 0.5 * DistLR);

                }
                else
                {
                    DataRow Pre = item;
                    if (i != 0)
                    {
                        Pre = dt.Rows[i - 1];
                    }
                    thisSB = PierKnowledge.ArrangeRamp(curPK, Theta, ref CL, item, Pre);
                }

                // -----------------------------------------------------------------------------------


                MySqlCommand cmd;
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                string SecName = PublicFun.GetSecName(brname);
                string OfficalBrName = PublicFun.GetOfficalBridgeName(brname);

                string piername = SecName + "/" + OfficalBrName + "/P" + (PierCount).ToString().PadLeft(2, '0');

                string tmp = string.Format("replace into sub_tbl values('{15}','{0}','{1}','{2}',{3},'{4}',{5},{6},{7},'{8}','{9}','{10}',{11},{12},{13},{14},0,'{16}');",
                    thisSB.SpanName, item["align_name"], item["bridge_name"], curPK, thisSB.TypeStr, thisSB.Theta.Degrees, thisSB.CapBeamLeft, thisSB.CapBeamRight,
                    thisSB.DistList.ToString2(), thisSB.PierAngList.ToString2(), thisSB.FundAngList.ToString2(), thisSB.H0, thisSB.H1, thisSB.SlopLeft, thisSB.SlopRight,
                    piername, thisSB.FundHList.ToString2());

                cmd = new MySqlCommand(tmp, CurConn);
                cmd.ExecuteNonQuery();
                PierCount++;
            }
            CurConn.Close();
            Console.WriteLine("#  {0} 已配置下部结构...", brname);
        }

        private Align GetCLByName(string RampName)
        {
            string EIName = RampName;
            if (RampName == "SEC204R")
            {
                EIName = "R1K";
            }
            if (RampName != "C2F")
            {
                Align refRampCL = AlignDict[BridgeToLine[RampName]];
                return refRampCL;
            }
            else
            {
                return null;
            }
        }
        private bool ExcistDB(string dbname)
        {

            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string selectString = string.Format("select count(*) as A from information_schema.schemata where schema_name = '{0}';", dbname);
            MySqlCommand cmd = new MySqlCommand(selectString, CurConn);
            int result = Convert.ToInt32(cmd.ExecuteScalar());
            bool res = false;
            if (result != 0)
            {
                res = true;
            }
            CurConn.Close();
            return res;
        }
        private bool ExcistTbl(string tb)
        {

            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string selectString = string.Format("select count(*) as A from information_schema.tables where table_name = '{0}' and table_schema ='{1}';",
                tb, CurConn.Database);
            MySqlCommand cmd = new MySqlCommand(selectString, CurConn);
            int result = Convert.ToInt32(cmd.ExecuteScalar());
            bool res = false;
            if (result != 0)
            {
                res = true;
            }
            return res;
        }
        private void CreatTableWithPriAndForeignKey(string tblName, string creatString, string priKeyString, string forKeyString, string refTbl, string refKey)
        {

            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            MySqlCommand cmd;

            cmd = new MySqlCommand(string.Format("DROP TABLE IF EXISTS {0};", tblName), CurConn);
            cmd.ExecuteNonQuery();

            string cmdstr = string.Format("CREATE TABLE {0} (", tblName);
            //cmdstr += "ID INT,";
            cmdstr += creatString;
            cmdstr += ");";
            cmd = new MySqlCommand(cmdstr, CurConn);
            cmd.ExecuteNonQuery();
            cmdstr = string.Format("ALTER TABLE {0} ADD PRIMARY KEY(", tblName);
            cmdstr += priKeyString;
            cmdstr += ")";
            cmd = new MySqlCommand(cmdstr, CurConn);
            cmd.ExecuteNonQuery();
            cmdstr = string.Format("ALTER TABLE {0} ADD constraint FK_ID foreign KEY({1}) references {2}({3})", tblName, forKeyString, refTbl, refKey);
            cmdstr += " on delete cascade";
            cmdstr += " on update cascade";
            cmd = new MySqlCommand(cmdstr, CurConn);
            cmd.ExecuteNonQuery();

        }
        private void CreatForeignKey(string tblName, string keyName, string refTbl, string refName)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string cmdstr = string.Format("ALTER TABLE {0} ADD constraint foreign KEY({1}) references {2}({3})", tblName, keyName, refTbl, refName);
            cmdstr += " on delete cascade";
            cmdstr += " on update cascade";
            MySqlCommand cmd = new MySqlCommand(cmdstr, CurConn);
            cmd.ExecuteNonQuery();
        }

        private void CreatTableWithPriKey(string tblName, string creatString, string priKeyString)
        {

            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            MySqlCommand cmd;

            cmd = new MySqlCommand(string.Format("DROP TABLE IF EXISTS {0};", tblName), CurConn);
            cmd.ExecuteNonQuery();

            string cmdstr = string.Format("CREATE TABLE {0} (", tblName);
            //cmdstr += "ID INT,";
            cmdstr += creatString;
            cmdstr += ");";
            cmd = new MySqlCommand(cmdstr, CurConn);
            cmd.ExecuteNonQuery();
            cmdstr = string.Format("ALTER TABLE {0} ADD PRIMARY KEY(", tblName);
            cmdstr += priKeyString;
            cmdstr += ")";
            cmd = new MySqlCommand(cmdstr, CurConn);
            cmd.ExecuteNonQuery();

        }
        private void CreatTable(string v1, string tableString)
        {

            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            MySqlCommand cmd;

            cmd = new MySqlCommand(string.Format("DROP TABLE IF EXISTS {0};", v1), CurConn);
            cmd.ExecuteNonQuery();

            string cmdstr = string.Format("CREATE TABLE {0} (", v1);
            //cmdstr += "ID INT,";
            cmdstr += tableString;
            cmdstr += ");";
            cmd = new MySqlCommand(cmdstr, CurConn);
            cmd.ExecuteNonQuery();
            CurConn.Close();

        }
        private void CreatTable(string v, string columnStr, string index)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            MySqlCommand cmd;

            cmd = new MySqlCommand(string.Format("DROP TABLE IF EXISTS {0};", v), CurConn);
            cmd.ExecuteNonQuery();

            string cmdstr = string.Format("CREATE TABLE {0} (", v);
            //cmdstr += "ID INT,";
            cmdstr += columnStr;
            cmdstr += ",";
            //cmdstr += string.Format("unique key {0}({0})",index);
            cmdstr += index;
            cmdstr += ");";
            cmd = new MySqlCommand(cmdstr, CurConn);
            cmd.ExecuteNonQuery();
            CurConn.Close();
        }

        private void WriteEIData(string EIName, string workDir)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            var dd = new DirectoryInfo(workDir);
            string Name = dd.Name;
            string altexticd = File.ReadAllText(dd.FullName + "\\" + Name + ".ICD", Encoding.Default);
            string altextsqx = File.ReadAllText(dd.FullName + "\\" + Name + ".SQX", Encoding.Default);
            string altextcg = File.ReadAllText(dd.FullName + "\\" + Name + ".CG", Encoding.Default);
            string altextdmx = File.ReadAllText(dd.FullName + "\\" + Name + ".DMX", Encoding.Default);
            string altexthdx = File.ReadAllText(dd.FullName + "\\" + Name + ".HDX", Encoding.Default);
            string RecString = string.Format("REPLACE INTO EI_tbl values('{0}','{1}','{2}','{3}','{4}','{5}');", EIName, altexticd, altextsqx, altextdmx, altextcg, altexthdx);
            string utf8_string = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(RecString));
            MySqlCommand cmd = new MySqlCommand(utf8_string, CurConn);
            cmd.ExecuteNonQuery();
            Console.WriteLine("#  " + EIName + "  已更新EI数据...");
            CurConn.Close();
        }


        private void UpdateBox(string brname, ref Dictionary<string, Align> CLList)
        {

            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }

            string selectString = string.Format("(SELECT * FROM span_tbl where bridge_name='{0}')", brname);
            if (NextSpanDict.Keys.Contains(brname))
            {
                selectString += "union";
                selectString += string.Format("(select * from span_tbl where name='{0}')", NextSpanDict[brname]);
            }
            selectString += "ORDER BY Station";
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow item = dt.Rows[i];

                if ((string)item["BeamType"] != "B" || i == dt.Rows.Count - 1)
                {
                    continue;
                }

                DataRow next = dt.Rows[i + 1];
                string line = (string)item["align_name"];
                double curPK = (double)item["Station"];
                double leftW = (double)item["front_wl"];
                double rightW = (double)item["front_wr"];
                double angInDeg = (double)item["Angle"];
                string SubType = (string)item["PierType"];
                string span_name = (string)item["Name"];
                Align CL = CLList[line];
                Vector2 dir = new Vector2(CL.curPQX.GetDir(curPK));
                Vector2 Cdir = dir.RotByZ(angInDeg / 180.0 * Math.PI);
                Vector2 Center = new Vector2(CL.curPQX.GetCoord(curPK));//+dir*C2C;

                Line A0 = new Line(Center, Center + Cdir * leftW);
                Line B0 = new Line(Center, Center - Cdir * rightW);
                if (curPK == 15710)
                {
                    ;
                }

                string Nextline = (string)next["align_name"];
                double nexPK = (double)next["Station"];
                double NleftW = (double)next["back_wl"];
                double NrightW = (double)next["back_wr"];
                double NangInDeg = (double)next["Angle"];
                string NSubType = (string)next["PierType"];
                string Nspan_name = (string)next["Name"];
                Align NCL = CLList[Nextline];

                Vector2 Ndir = new Vector2(NCL.curPQX.GetDir(nexPK));
                Vector2 NCdir = Ndir.RotByZ(NangInDeg / 180.0 * Math.PI);
                Vector2 NCenter = new Vector2(NCL.curPQX.GetCoord(nexPK));//- Ndir* C2C;
                Line A1 = new Line(NCenter, NCenter + NCdir * NleftW);
                Line B1 = new Line(NCenter, NCenter - NCdir * NrightW);

                double StartC2C = 0.54, EndC2C = 0.54;
                if (SubType.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                {
                    StartC2C = 0.54;
                }
                if (NSubType.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                {
                    EndC2C = 0.54;
                }

                double StTotalW, EdTotalW;
                StTotalW = A0.Length() + B0.Length();
                EdTotalW = A1.Length() + B1.Length();
                Line[] StLines = new Line[] { new Line(A0.EndPoint, B0.EndPoint) };
                Line[] EdLines = new Line[] { new Line(A1.EndPoint, B1.EndPoint) };

                List<BoxBeam> beamList = BeamKnowledge.ArrangePlan2(curPK, nexPK, StTotalW, EdTotalW, StLines, EdLines, StartC2C, EndC2C, angInDeg, NangInDeg, ref CL, ref NCL);

                string RecString;
                MySqlCommand cmd;
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                foreach (BoxBeam beam in beamList)
                {
                    double BeamNomialH = beam.Length >= 32 ? 1.8 : 1.6;

                    double StZ = CL.GetSurfaceBG(beam.StartPin.X, beam.StartPin.Y) - HSumUp(beam.StartPin.X, beam.StartPin.Y, BeamNomialH, ref CL) + BeamKnowledge.BearingTotalH;
                    double EdZ = NCL.GetSurfaceBG(beam.EndPin.X, beam.EndPin.Y) - HSumUp(beam.EndPin.X, beam.EndPin.Y, BeamNomialH, ref NCL) + BeamKnowledge.BearingTotalH;

                    string SecName = PublicFun.GetSecName((string)item["bridge_name"]);
                    string BriName = PublicFun.GetOfficalBridgeName((string)item["bridge_name"]);
                    string beamname = SecName + "/" + BriName + "/S" + (i + 1).ToString().PadLeft(2, '0') + "/G" + beam.ID.ToString().PadLeft(2, '0');

                    //string BoxTypeString = beam.Length <= 25 ? "BE02" : "BE01";
                    string IsSide = beam.IsSideBeam ? "Y" : "N";
                    RecString = string.Format("INSERT ignore INTO box_tbl values('{0}','{1}','{2}','{3}',{4},'{5}',{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},'{16}',{17},'{18}');",
                        beamname,
                        span_name, line, brname, curPK,
                        beam.BeamType, beam.StartA.Degrees, beam.EndA.Degrees, beam.StartBearingH, beam.EndBearingH,
                        beam.StartPin.X, beam.StartPin.Y, StZ,
                        beam.EndPin.X, beam.EndPin.Y, EdZ, IsSide, beam.DeckSlope, Nspan_name
                        );
                    cmd = new MySqlCommand(RecString, CurConn);
                    cmd.ExecuteNonQuery();
                }
                CurConn.Close();
            }
            Console.WriteLine("#  {0} 已配置上部结构...", brname);
        }

        private string GetGDDType(string span)
        {
            var br = (string)(GetRowByPriKeyRow("span_tbl", span)["bridge_name"]);
            if (this.RampTypeA.Contains(br))
            {
                return "TypeA";
            }
            else if (RampTypeB.Contains(br))
            {
                return "TypeB";
            }
            else if (RampTypeC.Contains(br))
            {
                return "TypeC";
            }
            else if (RampTypeD.Contains(br))
            {
                return "TypeD";
            }
            else
            {
                return "Type0";
            }

        }

        /// <summary>
        /// 考虑纵坡的上部建筑总高（含支座区总高）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="cL"></param>
        /// <returns></returns>
        private double HSumUp(double x, double y, double beamH, ref Align cL)
        {
            var st = cL.curPQX.GetStationNew(x, y);
            var zp = cL.curSQX.GetZP(st);
            return BeamKnowledge.SurfaceH + beamH / Math.Cos(Math.Atan(zp / 100.00)) + BeamKnowledge.BearingTotalH;
        }

        #endregion
    }
}
