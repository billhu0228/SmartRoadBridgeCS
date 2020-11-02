using SmartRoadBridge.Alignment;
using MySql.Data.MySqlClient;
using SmartRoadBridge.Database;
using netDxf;
using System.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CsvHelper;
using MathNet.Spatial.Units;
using SmartRoadBridge.Public;
using netDxf.Entities;
using MathNet.Spatial.Euclidean;
using SmartRoadBridge.Structure;
using SmartRoadBridge.Knowledge;

namespace SmartRoadBridge
{
    public class NepMain
    {
        static Align M1K, L1K, R1K, M2K, M3K;
        static Align SBE, EBA;
        static Align JKA, MLA;
        static Align CCA, CCB, CCC, CCD;
        static Align HSA, HSB, HSC, HSE, HSF;
        static Align MUA, MUB, MUC;
        static Align WLR;
        static Align A8L2;
        MySqlConnection CurConn;
        static Dictionary<string, Align> AlignDict;

        static DxfDocument MainRoad, A8Road, LKRoad, RKRoad, A8RoadR, A8RoadL, EmptyRoad;
        static string RootPart = @"E:\";
        static string EJFilePath;
   

        Dictionary<string, string> BridgeCSVDict, BridgeToLine;





        #region 方法
        /// <summary>
        /// 数据初始化
        /// </summary>
        public NepMain(string server, string port, string user, string pw, string db)
        {
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

            HSA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\A");
            HSB = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\B");
            HSC = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\C");
            HSE = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\E");
            HSF = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\F");
            

            MUA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\06-Museum Interchange\A匝道");
            MUB = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\06-Museum Interchange\B匝道");
            MUC = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\06-Museum Interchange\C匝道");

            WLR = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\07-Westlands Interchange\Westland-RampR");
            A8L2 = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\09-A8\A8L2");


            MainRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "主线边线.dxf")[0]);
            A8Road = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "A8边线.dxf")[0]);
            A8RoadR = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "A8边线右线.dxf")[0]);
            A8RoadL = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "A8边线左线.dxf")[0]);
            LKRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "左线边线.dxf")[0]);
            RKRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "右线边线.dxf")[0]);
            EmptyRoad = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", "空线.dxf")[0]);


            AlignDict = new Dictionary<string, Align>()
            {
                {"M1K",M1K},{"L1K",L1K},{"R1K",R1K},{"M2K",M2K},{"M3K",M3K},
                {"MLA",MLA },{"JKA",JKA},{"SBE",SBE},{"EBA",EBA},
                {"CCA",CCA},{"CCB",CCB},{"CCC",CCC},{"CCD",CCD},
                {"HSA",HSA},{"HSB",HSB},{"HSC",HSC},{"HSE",HSE},{"HSF",HSF},
                {"MUA",MUA},{"MUB",MUB},{"MUC",MUC},{"WLR",WLR},{ "A8L2",A8L2}
            };

            BridgeCSVDict = new Dictionary<string, string>()
            {
                {"HSI02","HSI02.csv"}, {"HSI01","HSI01.csv"},{"HSI03","HSI03.csv"},{"HSI04","HSI04.csv"},{"HSI05","HSI05.csv"},{"HSI06","HSI06.csv"},
                {"MLI01","MLI01.csv"},{"JKI01","JKI01.csv"},{"CCI01","CCA.csv"},{"CCI02","CCB.csv"},{"CCI03","CCC.csv"},{"CCI04","CCD.csv"},
                {"EBI01","EBA.csv"},{"SBI05","SBE.csv"},{"WLI01","WLR.csv"},{"MHI01","MUA.csv"},{"MHI02","MUB.csv"},{"MHI03","MUC.csv"},
                {"SEC101","SEC101.csv"},{"SEC102","SEC102.csv"},{"SEC103","SEC103.csv"},{"SEC104","SEC104.csv"},{"SEC201","SEC201.csv"},{"SEC202","SEC202.csv"},
                {"SEC203","SEC203.csv"},{"SEC204R","SEC204R.csv"},{"SEC204L","SEC204L.csv"},{"SEC205","SEC205.csv"},{"SEC206","SEC206.csv"},{"SEC207","SEC207.csv"},
                {"SEC208","SEC208.csv"},{"SEC209","SEC209.csv"},{"SEC210","SEC210.csv"},{"A8L01","A8L01.csv"},
            };
            BridgeToLine = new Dictionary<string, string>()
            {
                {"MLI01","MLA"},
                {"JKI01","JKA"},
                {"CCI01","CCA"},
                {"CCI02","CCB"},
                {"CCI03","CCC"},
                {"CCI04","CCD"},                
                {"HSI01","HSA"},
                {"HSI02","HSB"},
                {"HSI03","HSC"},
                {"HSI05","HSE"},
                {"HSI06","HSF"},
                {"EBI01","EBA"},
                {"SBI05","SBE"},
                {"WLI01","WLR"},
                {"MHI01","MUA"},
                {"MHI02","MUB"},
                {"MHI03","MUC"},
                {"SEC101","M1K"},
                {"SEC102","M1K"},
                {"SEC103","M1K"},
                {"SEC104","M1K"},
                {"SEC201","M1K"},
                {"SEC202","M1K"},
                {"SEC203","M1K"},
                {"SEC204R","R1K"},
                {"SEC204L","L1K"},
                {"SEC205","M2K"},
                {"SEC206","M2K"},
                {"SEC207","M2K"},
                {"SEC208","M2K"},
                {"SEC209","M2K"},
                {"SEC210","M3K"},
                {"A8L01","A8L2"},
            };
            EJFilePath= string.Format(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\{0}", "EJ.csv");
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
                string ColumnStr = "Name VarChar(10),ICD Text,SQX Text,DMX Text, CG Text";
                CreatTable("EI_tbl", ColumnStr, "unique key Name(Name)");
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
            foreach (string br in BridgeToWrite)
            {
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                MySqlCommand cmd = new MySqlCommand(string.Format("DROP TABLE IF EXISTS {0};", br), CurConn);
                cmd.ExecuteNonQuery();

                string ColumnStr = "id varchar(17),Line VarChar(50),Bridge VarChar(50),Station Double,Span Double,Angle Double," +
                    "WidthRight Double,WidthLeft Double,RealRight Double,RealLeft Double, SJX Double, DMX Double," +
                    "SubType VarChar(20),SupType VarChar(10),FundType VarChar(20),DeckType VarChar(2)";
                CreatTable(br, ColumnStr);

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
                    Align CL = AlignDict[item.Line];
                    string RecString = string.Format("INSERT INTO {0} values('{15}','{1}','{2}',{3},{4},{5}," +
                        "{6},{7},{8},{9},{10},{11}," +
                        "'{12}','{13}','{14}','CT');",
                        br, item.Line, item.Bridge, item.PK, item.Span, item.Angle,
                        item.Width * 0.5, item.Width * 0.5, item.Width * 0.5, item.Width * 0.5, CL.curSQX.GetBG(item.PK), CL.curDMX.GetBG(item.PK),
                        item.SubType, item.SupType, item.FundType, GetID(item.Line, item.PK));
                    cmd = new MySqlCommand(RecString, CurConn);
                    cmd.ExecuteNonQuery();
                }


                DxfDocument dxftocut = GetSideLine(br);
                if (dxftocut != null)
                {
                    UpdateWidth(br, ref dxftocut);
                }

                CurOther(br);
            }
            foreach (string br in BridgeToWrite)
            {
                MakeEJ(br);
            }


            CurConn.Close();
            return true;
        }



        private string GetID(string line, double pK)
        {
            string Int = Math.Round(pK, 3, MidpointRounding.AwayFromZero).ToString("f3").PadLeft(9,'0');
            return line + string.Format("+{0}", Int);
        }


        public void RefreshSubTbl2(bool reset = false)
        {
            if (reset)
            {
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                MySqlCommand cmd = new MySqlCommand("DROP TABLE IF EXISTS sub_tbl;", CurConn);
                cmd.ExecuteNonQuery();
            }
            if (!ExcistTbl("sub_tbl"))
            {
                string ColumnStr = "id VarChar(11),PierType VarChar(10),FundType VarChar(10),Angle double, " +
                    "CBLeftWidth double, CBRightWidth double, SpaceList VarChar(100),PierAngleList VarChar(100),FundAngleList VarChar(100)," +
                    "PierTopH double,PierBotH double";
                CreatTableWithPriKey("sub_tbl", ColumnStr, "id");
            }

        }

        public bool RefreshSubTbl(bool reset = false)
        {
            if (reset)
            {
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                MySqlCommand cmd = new MySqlCommand("DROP TABLE IF EXISTS sub_tbl;", CurConn);
                cmd.ExecuteNonQuery();
            }
            if (!ExcistTbl("SUB_TBL"))
            {
                string ColumnStr = "ID int,Line VarChar(10),Bridge VarChar(10),Station double,Type VarChar(10),Angle double, " +
                    "LeftWidth double, RightWidth double, SpaceList VarChar(100),PierAngleList VarChar(100),FundAngleList VarChar(100)," +
                    " H0 double, H1 double,SlopLeft double,SlopRight double";
                CreatTable("sub_tbl", ColumnStr, "unique key Name(Bridge,Station)");
            }

            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string selectString = string.Format("SELECT Table_name from information_schema.TABLES WHERE table_schema = '{0}'", CurConn.Database);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();

            var brList = (from DataRow a in dt.Rows select (string)a["Table_name"]).ToList();

            foreach (var item in brList)
            {
                if (item.StartsWith("MHI", StringComparison.OrdinalIgnoreCase))
                {
                    ;
                }
                if (!item.EndsWith("_tbl", StringComparison.OrdinalIgnoreCase))
                {
                    DxfDocument side = GetConsiderLine(item);
                    UpdateColumn(item, ref side, ref AlignDict);
                }
            }
            CurConn.Close();

            return true;
        }

        public void RefreshBoxTblPlan(List<string> BridgeToWrite = null)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            MySqlCommand cmd = new MySqlCommand("DROP TABLE IF EXISTS box_tbl;", CurConn);
            cmd.ExecuteNonQuery();
            
            if (!ExcistTbl("box_TBL"))
            {
                string ColumnStr = "ID int,Bridge VarChar(10),Station double,Type VarChar(50),Ang0 double,Ang1 double,BrH0 double,BrH1 double," +
                    "X0 Double,Y0 Double,H0 Double,X1 Double,Y1 Double,H1 Double,IsSide VarChar(3),Slop Double,StCBID int,EdCBID int,EdBridge VarChar(10)";                
                CreatTable("box_TBL", ColumnStr, "unique key Name(Bridge,ID)");
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

        public void RefreshCapBeamTbl(List<string> BridgeToWrite=null)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            MySqlCommand cmd = new MySqlCommand("DROP TABLE IF EXISTS CapBeam_tbl;", CurConn);
            cmd.ExecuteNonQuery();
            if (!ExcistTbl("CapBeam_tbl"))
            {
                string ColumnStr = "ID int,Line VarChar(10),Bridge VarChar(10),Station double," +
                    "X Double,Y Double,H Double,Slop Double";
                CreatTable("CapBeam_tbl", ColumnStr, "unique key Name(Bridge,ID)");
            }        
            if (BridgeToWrite == null)
            {
                BridgeToWrite = (from key in BridgeCSVDict.Keys select key).ToList();
            }
            foreach (string br in BridgeToWrite)
            {
                UpdateCapBeamParameters(br);
            }
        }

        public void RefreshBoxTblEle()
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }

            string selectString = string.Format("SELECT * FROM capbeam_tbl");
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow item = dt.Rows[i];

                string brname = (string)item["Bridge"];
                string line = (string)item["Line"];
                double CBID = (int)item["ID"];
                double Station = (double)item["Station"];
                double slop = (double)item["Slop"];

                Point3D CC = new Point3D((double)item["X"], (double)item["Y"], (double)item["H"]);
                Vector3D Normal = new Vector3D(0, 0, 1);
                Vector3D Dir = new Vector3D(AlignDict[line].curPQX.GetDir(Station)[0], AlignDict[line].curPQX.GetDir(Station)[1], 0);
                Normal = Normal.Rotate(Dir, -Angle.FromRadians(Math.Atan(slop * 0.01)));
                Plane CapTop = new Plane(CC, Normal.Normalize());


       


                selectString = string.Format("SELECT * FROM box_tbl where (StCBID={0} or EdCBID={0}) and Bridge='{1}'",CBID,brname);
                adapter = new MySqlDataAdapter(selectString, CurConn);
                DataSet dataset2 = new DataSet();
                adapter.Fill(dataset2);
                adapter.Dispose();
                DataTable tmp = dataset2.Tables[0];

                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                foreach (DataRow beam in tmp.Rows)
                {
     
                    int BeamID=(int)beam["ID"];
                    string BridgeName=(string)beam["Bridge"];
                    int StCapBeam = (int)beam["StCBID"];
                    int EdCapBeam = (int)beam["EdCBID"];
                    string key="";
                    Point3D PinOnBeam,A, B,PinOnCB;
                    if (StCapBeam == CBID)
                    {
                        key =  "BrH0" ;
                        PinOnBeam= new Point3D((double)beam["X0"], (double)beam["Y0"], (double)beam["H0"]);
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
                    double PlinthHeight=PinOnCB.DistanceTo(PinOnBeam)- BeamKnowledge.BearingAH - BeamKnowledge.BearingPadH;
                    string res = string.Format("UPDATE box_tbl set {0}={1} where ID={2} and Bridge='{3}';",  key, PlinthHeight, BeamID, BridgeName);


                    MySqlCommand cmd = new MySqlCommand(res, CurConn);
                    cmd.ExecuteNonQuery();

                }
                

                



            }

            Console.WriteLine("#  更新垫石高度..");
        }

        public void OverrideColumn(string csvpath)
        {
            List<SubINFO> tmp = new List<SubINFO>();
            using (var reader = new StreamReader(csvpath))
            {
                using (var csv = new CsvReader(reader))
                {
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


        public void Shutdown()
        {
            Console.WriteLine("\n 按任意键继续..");
            Console.ReadKey();
        }


        #endregion




        #region 私有方法

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
                res = string.Format("UPDATE sub_tbl set {0}={1} where Station={2} and Bridge='{3}';", key, value, station, bridge);
            }
            else
            {
                if (value != condition)
                {
                    res = string.Format("UPDATE sub_tbl set {0}={1} where Station={2} and Bridge='{3}';", key, value, station, bridge);
                }
            }

            return res;
        }

        private string UpdateMySql(double station, string bridge, string value, string condition, string key, bool force = false)
        {
            string res = string.Empty;
            if (force)
            {
                res = string.Format("UPDATE sub_tbl set {0}='{1}' where Station={2} and Bridge='{3}';", key, value, station, bridge);
            }
            else
            {
                if (value != condition)
                {
                    res = string.Format("UPDATE sub_tbl set {0}='{1}' where Station={2} and Bridge='{3}';", key, value, station, bridge);
                }
            }

            return res;
        }

        private string UpdateBoxHRecorder(string tbname,int id,  string bridge, string column, double value)
        {
            string res = string.Format("UPDATE {0} set {1}={2} where (ID,Bridge)=({3},'{4}');", tbname, column, value, id, bridge);
            return res;
        }

        private void UpdateCapBeamParameters(string brname)
        {
            string selectString = string.Format("SELECT * FROM {0} order by station", brname);
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

                string line = (string)item["Line"];
                string br = (string)item["Bridge"];
                double curPK = (double)item["Station"];
                if (br!=brname)
                {
                    continue;
                }
                if (curPK==16835)
                {
                    ;
                }

                Align CL = AlignDict[line];

                DataTable BeamsToCalculate;
                selectString = string.Format("SELECT * FROM box_tbl where station={0} and bridge='{1}'", curPK, br);
                adapter = new MySqlDataAdapter(selectString, CurConn);
                dataset = new DataSet();
                adapter.Fill(dataset);
                adapter.Dispose();
                BeamsToCalculate = dataset.Tables[0];
                int numBeams = BeamsToCalculate.Rows.Count;
                bool UseSmallBeam=true;
                if (numBeams!=0)
                {
                    UseSmallBeam = false;
                }
                else
                {
                    if (i==0)
                    {
                        //被动切割墩
                        continue;
                    }
                    double prePK = (double)dt.Rows[i-1]["Station"];
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

                HL = CL.GetSurfaceBG(LeftPt.X, LeftPt.Y) - (BeamKnowledge.SurfaceH+BeamKnowledge.BeamH+BeamKnowledge.BearingTotalH);
                HR = CL.GetSurfaceBG(RightPt.X, RightPt.Y) - (BeamKnowledge.SurfaceH + BeamKnowledge.BeamH + BeamKnowledge.BearingTotalH);

                double ConsiderDist = LeftPt.DistanceTo(RightPt);
                double MiddleToLeft = LeftPt.DistanceTo(CCPin);

                double CapBeamSlop = (HR - HL) / ConsiderDist * 100.0;
                double CapBeamCenterH = HL + CapBeamSlop / 100.0 * MiddleToLeft;
                int idd = (int)(curPK * 100);

                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                string RecString = string.Format("INSERT INTO CapBeam_tbl values({0},'{7}','{1}',{2},{3},{4},{5},{6})" +
                    " ON DUPLICATE KEY UPDATE X={3},Y={4},H={5},Slop={6};"
                    , idd,brname,curPK, CCPin.X, CCPin.Y,CapBeamCenterH,CapBeamSlop,line);
                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();

            }

            Console.WriteLine("#  {0} 已配置盖梁参数...", brname);
        }

        private string UpdateRecorder(string tbname, int id, double station, string bridge, string column, string value)
        {
            string res = string.Format("UPDATE '{0}' set '{1}'='{2}' where Station={3} and Bridge='{4}' and id={5};", tbname, column, value, station, bridge,id);
            return res;
        }
        private string UpdateRecorder(string tbname, int id, double station, string bridge, string column, double value)
        {
            string res = string.Format("UPDATE '{0}' set '{1}'={2} where Station={3} and Bridge='{4}' and id={5};", tbname, column, value, station, bridge,id);
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
            else if (new string[] { "MLI01", "JKI01", "EBI01", "SBI05" }.Contains(br))
            {
                return null;
            }
            else
            {
                dxfilename = string.Format("{0}边线.dxf", br);
                dxftocut = DxfDocument.Load(Directory.GetFiles(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\05 数据库\dxf\", dxfilename)[0]);
            }
            return dxftocut;
        }

        /// <summary>
        /// 更新伸缩缝信息
        /// </summary>
        /// <param name="br"></param>
        private void MakeEJ(string br)
        {
            if (br=="CCI01")
            {
                ;
            }
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string selectString = string.Format("SELECT * FROM {0}", br);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();

            string altexticd = File.ReadAllText(EJFilePath, Encoding.Default);
            List<string> EJList = altexticd.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
            int i =0;
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            bool ShouldUpdate = false;
            foreach (DataRow item in dt.Rows)
            {                
                ShouldUpdate = false;
                string id = (string)item["id"];
                string subtype = (string)item["SubType"];
                string suptype = (string)item["SupType"];

                if (subtype.StartsWith("A"))
                {
                    ShouldUpdate = true;
                }
                else if (EJList.Contains(id))
                {
                    ShouldUpdate = true;
                }
                if (ShouldUpdate)
                {
                    string RecString = string.Format("UPDATE {0} set DeckType='EJ' where id='{1}'", br, id);
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
            string selectString = string.Format("SELECT * FROM {0}", br);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();

            foreach (DataRow item in dt.Rows)
            {
                string tmp = (string)item["FundType"];
                var RampNameList= tmp.Split('|');
                foreach (string RampName in RampNameList)
                {
                    if (RampName != "C2F" && RampName != "A1F")
                    {
                        DxfDocument RampSideLine = GetSideLine(RampName);
                        Align CL = AlignDict[(string)item["Line"]];
                        Align RampCL = AlignDict[BridgeToLine[RampName]];


                        double curPK = (double)item["Station"];
                        Point2D MLCC = new Point2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);
                        Vector2D Dir = new Vector2D(CL.curPQX.GetDir(curPK)[0], CL.curPQX.GetDir(curPK)[1]);
                        Vector2D Cdir = Dir.Rotate(Angle.FromDegrees((double)item["Angle"]));

                        if (curPK==19295)
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
                        else
                        {
                            throw new Exception("切割状态有误");
                        }
          ;

                        if (angle.Degrees>=180)
                        {
                            angle = Angle.FromDegrees(angle.Degrees - 180.0);
                        }
                        string beamtype = "B";
                        if (curPK==19766||curPK==19806||curPK==18855||curPK==18900)
                        {
                            beamtype = "C";
                        }
                        if (curPK == 350.6 &&br.StartsWith("HSI01",StringComparison.OrdinalIgnoreCase))
                        {
                            beamtype = "X";                            
                        }
                        string RecString = string.Format("INSERT INTO {0} values('{13}','{1}','{2}',{3},30,{4},{5},{6},{7},{8},{9},{10},'{11}','{12}','C2F','CT');",
                            RampName, BridgeToLine[RampName], RampName, pk0, angle.Degrees, WidthRight, WidthLeft, WidthRight, WidthLeft, 
                            RampCL.curSQX.GetBG(pk0), RampCL.curDMX.GetBG(pk0), GetID(BridgeToLine[br] , curPK),beamtype,GetID(BridgeToLine[RampName], pk0));

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
        private void CutDxf(Vector2 Center, Vector2 Cdir, ref DxfDocument SideLineToCut, out List<Vector2> AList,out List<Vector2> BList)
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
                if (DistA.Count==1)
                {
                    DistA.Add(DistA[0]);
                }
                if (DistB.Count==1)
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
                    v, dr, dl,drreal,dlreal, res[k]);
                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();
            }
            Console.WriteLine("#  " + v + "  已更新桥宽度...");
            CurConn.Close();
        }
        public void UpdateColumn(string brname, ref DxfDocument SideLine, ref Dictionary<string, Align> CLList)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
            string selectString = string.Format("SELECT * FROM {0} order by Station asc", brname);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow item = dt.Rows[i];
                string LineName = (string)item["Line"];
                string BridgeName = (string)item["Bridge"];
                Align CL = CLList[(string)item["Line"]];
                string trName = (string)item["Bridge"];// == "SEC2-10" ? "SEC210" : string.Join("0", ((string)item["Bridge"]).Split('-').ToArray());
                if (!trName.Equals(brname,StringComparison.OrdinalIgnoreCase) || ((string)item["SubType"])[0] != 'C')
                {
                    continue;
                }
                double curPK = (double)item["Station"];
                double WidthLeft = (double)item["WidthLeft"];
                double WidthRight = (double)item["WidthRight"];
                Angle Theta = Angle.FromDegrees((double)item["Angle"]);
                //Align RefCL = GetCLByName((string)item["FundType"]);

                Vector2D Center = new Vector2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);
                Vector2D dir = new Vector2D(CL.curPQX.GetDir(curPK)[0], CL.curPQX.GetDir(curPK)[1]);
                Vector2D Cdir = dir.Rotate(Theta);

                SubStructure thisSB = new PierNone();


                if (brname.StartsWith("SEC",StringComparison.OrdinalIgnoreCase))
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
                    thisSB = PierKnowledge.Arrange2(LineName,curPK, Theta, WidthLeft, WidthRight, DistListLeft, DistListRight, ref CL, 0.5 * DistLR);

                }
                else
                {
                    DataRow Pre = item;
                    if (i!=0)
                    {
                        Pre = dt.Rows[i - 1];
                    }                     
                    thisSB = PierKnowledge.ArrangeRamp(curPK, Theta, ref CL, item, Pre);
                }

                // -----------------------------------------------------------------------------------


                MySqlCommand cmd;
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }

                string tmp = string.Format("replace into sub_tbl values({0},'{1}','{2}',{3},'{4}',{5},{6},{7},'{8}','{9}','{10}',{11},{12},{13},{14});",
                    thisSB.ID, item["Line"], item["Bridge"], curPK, thisSB.TypeStr, thisSB.Theta.Degrees, thisSB.CapBeamLeft, thisSB.CapBeamRight,
                    thisSB.DistList.ToString2(), thisSB.PierAngList.ToString2(), thisSB.FundAngList.ToString2(), thisSB.H0, thisSB.H1, thisSB.SlopLeft, thisSB.SlopRight);

                cmd = new MySqlCommand(tmp, CurConn);
                cmd.ExecuteNonQuery();
                CurConn.Close();
            }
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
                tb,CurConn.Database);
            MySqlCommand cmd = new MySqlCommand(selectString, CurConn);
            int result = Convert.ToInt32(cmd.ExecuteScalar());
            bool res = false;
            if (result!= 0)
            {
                res=true;                
            }
            CurConn.Close();
            return res;
        }
        private void CreatTableWithPriAndForeignKey(string tblName, string creatString, string priKeyString,string forKeyString,string refTbl,string refKey)
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
            cmdstr = string.Format("ALTER TABLE {0} ADD constraint FK_ID foreign KEY({1}) references {2}({3})", tblName,forKeyString,refTbl,refKey);
            cmdstr += " on delete cascade";
            cmdstr += " on update cascade";
            cmd = new MySqlCommand(cmdstr, CurConn);
            cmd.ExecuteNonQuery();
            CurConn.Close();
        }
        private void CreatTableWithPriKey(string tblName, string creatString,string priKeyString)
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
            CurConn.Close();
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
        private void CreatTable(string v, string columnStr,string index)
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
            string RecString = string.Format("REPLACE INTO EI_tbl values('{0}','{1}','{2}','{3}','{4}');", EIName, altexticd, altextsqx, altextdmx, altextcg);
            string utf8_string = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(RecString));
            MySqlCommand cmd = new MySqlCommand(utf8_string, CurConn);
            cmd.ExecuteNonQuery();
            Console.WriteLine("#  " + EIName + "  已更新EI数据...");
            CurConn.Close();
        }


        private void UpdateBox(string brname, ref Dictionary<string, Align> CLList)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }

            string selectString = string.Format("SELECT * FROM {0} order by station", brname);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow item = dt.Rows[i];


                if ((string)item["SupType"] != "B" || i == dt.Rows.Count - 1)
                {
                    continue;
                }

                DataRow next = dt.Rows[i + 1];
                string line = (string)item["Line"];
                double curPK = (double)item["Station"];
                double leftW = (double)item["WidthLeft"];
                double rightW = (double)item["WidthRight"];
                double angInDeg = (double)item["Angle"];
                string SubType = (string)item["SubType"];

                Align CL = CLList[line];
                Vector2 dir = new Vector2(CL.curPQX.GetDir(curPK));
                Vector2 Cdir = dir.RotByZ(angInDeg / 180.0 * Math.PI);
                Vector2 Center = new Vector2(CL.curPQX.GetCoord(curPK));//+dir*C2C;

                Line A0 = new Line(Center, Center + Cdir * leftW);
                Line B0 = new Line(Center, Center - Cdir * rightW);
                if (curPK == 19085)
                {
                    ;
                }

                string Nextline = (string)next["Line"];
                double nexPK = (double)next["Station"];
                double NleftW = (double)next["WidthLeft"];
                double NrightW = (double)next["WidthRight"];
                double NangInDeg = (double)next["Angle"];
                string NSubType = (string)next["SubType"];
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

                bool isStPublicPier = !((string)item["FundType"] == "C2F" || (string)item["FundType"] == "A1F");
                bool isEdPublicPier = !((string)next["FundType"] == "C2F" || (string)next["FundType"] == "A1F");
                Line[] StLines = new Line[] { };
                Line[] EdLines = new Line[] { };
                double StTotalW, EdTotalW;
                StTotalW = A0.Length() + B0.Length();
                EdTotalW = A1.Length() + B1.Length();
                if ((string)item["FundType"] == "C2F"|| (string)item["FundType"] == "A1F")
                {
                    if ((string)item["SubType"] == "C2"|| (string)item["SubType"] == "A1")
                    {
                        // 普通墩
                        StLines = new Line[] { new Line(A0.EndPoint, B0.EndPoint) };
                        
                    }
                    else
                    {
                        // 被动共用墩
                        StLines = new Line[] { new Line(A0.EndPoint, B0.EndPoint) };
                        //continue;
                    }
                }                    
                else
                {
                    // 主动共用墩
                    StLines = new Line[2];
                    if (curPK == 19556)
                    {
                        // 三线共用段19556                        
                        selectString = string.Format("SELECT WidthRight+WidthLeft FROM {0} where SubType='{1}'", "HSI06", GetID((string)item["Line"], curPK));
                    }
                    else
                    {
                        // 正常情况
                        selectString = string.Format("SELECT WidthRight+WidthLeft FROM {0} where SubType='{1}'", (string)item["FundType"], GetID((string)item["Line"], curPK));
                    }

                    adapter = new MySqlDataAdapter(selectString, CurConn);
                    DataSet dataset2 = new DataSet();
                    adapter.Fill(dataset2);
                    adapter.Dispose();
                    DataTable tmp = dataset2.Tables[0];

                    double RampWidth = (double)tmp.Rows[0][0];
                    double RealMainWidth = (double)item["RealLeft"] + (double)item["RealRight"];


                    if (leftW > rightW)
                    {
                        StLines[0] = new Line(A0.EndPoint.Convert2D(), A0.EndPoint.Convert2D() - Cdir * RampWidth);
                        StLines[1] = new Line(B0.EndPoint.Convert2D() + Cdir * RealMainWidth, B0.EndPoint.Convert2D());
                    }
                    else
                    {
                        StLines[0] = new Line(A0.EndPoint.Convert2D(), A0.EndPoint.Convert2D() - Cdir * RealMainWidth);
                        StLines[1] = new Line(B0.EndPoint.Convert2D() + Cdir * RampWidth, B0.EndPoint.Convert2D());
                    }

                    if (curPK==19556)
                    {
                        StLines = new Line[1];
                        StLines[0] = new Line(Center + Cdir * 6.5, B0.EndPoint.Convert2D());
                    }
                }

                if ((string)next["FundType"] == "C2F" || (string)next["FundType"] == "A1F")
                {
                    if ((string)next["SubType"] == "C2" || (string)next["SubType"] == "A1")
                    {
                        // 普通墩
                        EdLines = new Line[] { new Line(A1.EndPoint, B1.EndPoint) };
                    }
                    else
                    {
                        // 被动共用墩
                        ;
                        EdLines = new Line[] { new Line(A1.EndPoint, B1.EndPoint) };
                        //continue;
                    }
                }
                else
                {
                    // 主动共用墩
                    EdLines = new Line[2];
                    if (nexPK==19556)
                    {
                        // 三线共用段19556                        
                        selectString = string.Format("SELECT WidthRight+WidthLeft FROM {0} where SubType='{1}'", "HSI01",GetID((string)next["Line"],nexPK));
                    }
                    else
                    {
                        // 正常情况
                        selectString = string.Format("SELECT WidthRight+WidthLeft FROM {0} where SubType='{1}'", (string)next["FundType"], GetID((string)next["Line"], nexPK));
                    }
                    
                    adapter = new MySqlDataAdapter(selectString, CurConn);
                    DataSet dataset2 = new DataSet();
                    adapter.Fill(dataset2);
                    adapter.Dispose();
                    DataTable tmp = dataset2.Tables[0];

                    double RampWidth = (double)tmp.Rows[0][0];
                    double RealMainWidth = (double)next["RealLeft"] + (double)next["RealRight"];

                    if (NleftW > NrightW)
                    {
                        EdLines[0] = new Line(A1.EndPoint.Convert2D(), A1.EndPoint.Convert2D() - NCdir * RampWidth);
                        EdLines[1] = new Line(B1.EndPoint.Convert2D() + NCdir * RealMainWidth, B1.EndPoint.Convert2D());
                    }
                    else
                    {
                        EdLines[0] = new Line(A1.EndPoint.Convert2D(), A1.EndPoint.Convert2D() - NCdir * RealMainWidth);
                        EdLines[1] = new Line(B1.EndPoint.Convert2D() + NCdir * RampWidth, B1.EndPoint.Convert2D());
                    }

                    if (nexPK == 19556)
                    {
                        EdLines = new Line[1];
                        EdLines[0] = new Line(A1.EndPoint.Convert2D(), NCenter-NCdir*4.5);
                    }

                    if (nexPK == 19766)
                    {
                        var ff = EdLines[0];
                        EdLines = new Line[1];
                        EdLines[0] = ff;
                    }

                    if (brname=="SEC204R" && nexPK==19295)
                    {
                        EdLines = new Line[1];
                        EdLines[0] =new Line(A1.EndPoint.Convert2D(),B1.EndPoint.Convert2D());

                    }


                }

                List<BoxBeam> beamList = BeamKnowledge.ArrangePlan2(curPK, nexPK,StTotalW,EdTotalW,StLines,EdLines,StartC2C,EndC2C,angInDeg,NangInDeg,ref CL,ref NCL);
                
                //List<BoxBeam> beamList = BeamKnowledge.ArrangePlan(curPK, nexPK, A0, B0, A1, B1, ref CL, ref NCL, StartC2C, EndC2C, isStPublicPier, isEdPublicPier);


                string RecString;
                MySqlCommand cmd;
                if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }
                foreach (BoxBeam beam in beamList)
                {
                    double StZ = CL.GetSurfaceBG(beam.StartPin.X, beam.StartPin.Y) - HSumUp(beam.StartPin.X, beam.StartPin.Y, ref CL)+BeamKnowledge.BearingTotalH;
                    double EdZ = NCL.GetSurfaceBG(beam.EndPin.X, beam.EndPin.Y) - HSumUp(beam.EndPin.X, beam.EndPin.Y, ref NCL) + BeamKnowledge.BearingTotalH;

                    string BoxTypeString = beam.Length <= 25 ? "B25" : "B30";
                    string IsSide = beam.IsSideBeam ? "Y" : "N";
                    RecString = string.Format("INSERT INTO box_tbl values({11},'{12}',{13},'{0}',{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},'{14}',{15},{16},{17},'{18}');",
                        BoxTypeString, beam.StartA.Degrees, beam.EndA.Degrees, beam.StartBearingH, beam.EndBearingH,
                        beam.StartPin.X, beam.StartPin.Y, StZ,
                        beam.EndPin.X, beam.EndPin.Y, EdZ,
                        beam.ID, brname, curPK, IsSide, beam.DeckSlope,
                      (int)(curPK * 100), (int)(nexPK*100), (string)next["Bridge"]
                        );
                    cmd = new MySqlCommand(RecString, CurConn);
                    cmd.ExecuteNonQuery();
                }
                CurConn.Close();
            }
            Console.WriteLine("#  {0} 已配置上部结构...", brname);
        }
        private void UpdateBoxElevation(string brname)
        {
            if (CurConn.State == ConnectionState.Closed) { CurConn.Open(); }

            string selectString = string.Format("SELECT * FROM {0} order by station", brname);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, CurConn);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            adapter.Dispose();
            DataTable dt = dataset.Tables[0];
            double HsmL=8000, HsmR = 8000, HbigL = 8000, HbigR = 8000;
            double HLeft, HRight;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow item = dt.Rows[i];
                if ((string)item["SupType"] != "B" || i == dt.Rows.Count - 1)
                {
                    continue;
                }



                string line = (string)item["Line"];
                string br = (string)item["Bridge"];
                double curPK = (double)item["Station"];
                //double leftW = (double)item["WidthLeft"];
                //double rightW = (double)item["WidthRight"];
                //double angInDeg = (double)item["Angle"];
                string SubType = (string)item["SubType"];

                

                DataTable BigBeams, SmallBeams;

                selectString = string.Format("SELECT * FROM box_tbl where station={0} and bridge='{1}'",curPK,br);
                adapter = new MySqlDataAdapter(selectString, CurConn);
                dataset = new DataSet();
                adapter.Fill(dataset);
                adapter.Dispose();
                BigBeams = dataset.Tables[0];
                Align CL = AlignDict[line];

                double ConsiderDist = 0;
                double MiddleToLeft = 0;
                Point2D MidllePinAtBigStation;
                Point2D MidllePinAtSmallStation;

                Vector2D dir = new Vector2D(CL.curPQX.GetDir(curPK)[0], CL.curPQX.GetDir(curPK)[1]);

                if (SubType.StartsWith("A"))
                {
                    continue;                    
                }
                else
                {

                    DataRow pre = dt.Rows[i - 1];
                    string Pline = (string)pre["Line"];
                    string Pbr = (string)pre["Bridge"];
                    double prePK = (double)pre["Station"];
                    string PSubType = (string)pre["SubType"];
                    selectString = string.Format("SELECT * FROM box_tbl where station={0} and bridge='{1}'", prePK, Pbr);
                    adapter = new MySqlDataAdapter(selectString, CurConn);
                    dataset = new DataSet();
                    adapter.Fill(dataset);
                    adapter.Dispose();
                    SmallBeams = dataset.Tables[0];
                    Align PCL = AlignDict[Pline];

        
                    Point2D BigLeftPin=new Point2D((double)BigBeams.Rows[0]["X0"], (double)BigBeams.Rows[0]["Y0"]);
                    Point2D BigRightPin = new Point2D((double)BigBeams.Rows[BigBeams.Rows.Count - 1]["X0"], (double)BigBeams.Rows[BigBeams.Rows.Count - 1]["Y0"]);
                    MidllePinAtBigStation = new Point2D(CL.curPQX.GetCoord(curPK + 0.54)[0], CL.curPQX.GetCoord(curPK + 0.54)[1]);
                    MidllePinAtSmallStation = new Point2D(CL.curPQX.GetCoord(curPK - 0.54)[0], CL.curPQX.GetCoord(curPK - 0.54)[1]);
                    HbigL = CL.GetSurfaceBG(BigLeftPin.X,BigLeftPin.Y)  -HSumUp(BigLeftPin.X, BigLeftPin.Y, ref CL);
                    HbigR = CL.GetSurfaceBG(BigRightPin.X, BigRightPin.Y) - HSumUp(BigRightPin.X, BigRightPin.Y, ref CL);

                    Point2D SmLeftPin = new Point2D((double)SmallBeams.Rows[0]["X1"], (double)SmallBeams.Rows[0]["Y1"]);
                    Point2D SmRightPin = new Point2D((double)SmallBeams.Rows[SmallBeams.Rows.Count - 1]["X1"], (double)SmallBeams.Rows[SmallBeams.Rows.Count - 1]["Y1"]);


                    HsmL = PCL.GetSurfaceBG(SmLeftPin.X, SmLeftPin.Y) - HSumUp(SmLeftPin.X, SmLeftPin.Y, ref PCL);
                    HsmR = PCL.GetSurfaceBG(SmRightPin.X, SmRightPin.Y) - HSumUp(SmRightPin.X, SmRightPin.Y, ref PCL);
                    ConsiderDist = BigLeftPin.DistanceTo(BigRightPin);
                    MiddleToLeft = BigLeftPin.DistanceTo(MidllePinAtBigStation);
                }

                HLeft = Math.Min(HsmL, HbigL);
                HRight = Math.Min(HsmR, HbigR);

                double CapBeamSlop = (HRight - HLeft) / ConsiderDist * 100.0;
                double CapBeamCenterH = HLeft + CapBeamSlop/100.0 * MiddleToLeft;

                foreach (DataRow bm in SmallBeams.Rows)
                {
                    double x = (double)bm["X1"];
                    double y = (double)bm["Y1"];
                    Point2D CurPin = new Point2D(x,y);
                    double dist = CurPin.DistanceTo(MidllePinAtSmallStation);
                    int direction = dir.SignedAngleTo(CurPin - MidllePinAtSmallStation) < Angle.FromDegrees(180.0) ? -1 : 1;
                    double HofBot = CapBeamCenterH + direction * dist * CapBeamSlop*0.01;
                    double PlinthH = CL.GetSurfaceBG(x, y) - HSumUp(x, y, ref CL)-HofBot+BeamKnowledge.BearingTotalH;
                    double z = CL.GetSurfaceBG(x, y) - HSumUp(x, y, ref CL)+BeamKnowledge.BearingTotalH;

                    string cmdstr = "";
                    cmdstr += UpdateBoxHRecorder("Box_tbl",(int)bm["ID"], (string)bm["Bridge"],"H1",z);
                    cmdstr += UpdateBoxHRecorder("Box_tbl", (int)bm["ID"], (string)bm["Bridge"], "BrH1", PlinthH);
                    MySqlCommand cmd = new MySqlCommand(cmdstr, CurConn);
                    cmd.ExecuteNonQuery();
                }

                foreach (DataRow bm in BigBeams.Rows)
                {
                    double x = (double)bm["X0"];
                    double y = (double)bm["Y0"];
                    Point2D CurPin = new Point2D(x, y);
                    double dist = CurPin.DistanceTo(MidllePinAtBigStation);
                    int direction = dir.SignedAngleTo(CurPin - MidllePinAtBigStation) < Angle.FromDegrees(180.0) ? -1 : 1;
                    double HofBot = CapBeamCenterH + direction * dist * CapBeamSlop * 0.01;
                    double PlinthH = CL.GetSurfaceBG(x, y) - HSumUp(x, y, ref CL) - HofBot + BeamKnowledge.BearingTotalH;
                    double z = CL.GetSurfaceBG(x, y) - HSumUp(x, y, ref CL) + BeamKnowledge.BearingTotalH;

                    string cmdstr = "";
                    cmdstr += UpdateBoxHRecorder("Box_tbl", (int)bm["ID"], (string)bm["Bridge"], "H0", z);
                    cmdstr += UpdateBoxHRecorder("Box_tbl", (int)bm["ID"], (string)bm["Bridge"], "BrH0", PlinthH);
                    MySqlCommand cmd = new MySqlCommand(cmdstr, CurConn);
                    cmd.ExecuteNonQuery();
                }




            }
        }

        private void UpdateOneBeam(string v)
        {
            
        }

        private double HSumUp(double x, double y, ref Align cL)
        {
            var st=cL.curPQX.GetStationNew(x, y);
            var zp = cL.curSQX.GetZP(st);
            return BeamKnowledge.SurfaceH + BeamKnowledge.BeamH / Math.Cos(Math.Atan(zp / 100.00))+BeamKnowledge.BearingTotalH;
        }

        #endregion
    }
}
