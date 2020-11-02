using CsvHelper;
using MathNet.Spatial.Units;
using MySql.Data.MySqlClient;
using SmartRoadBridge.Alignment;
using SmartRoadBridge.Public;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Spatial.Euclidean;
using SmartRoadBridge.Knowledge;
using netDxf;
using netDxf.Entities;
using System.Data;
using SmartRoadBridge.Structure;
using System.Text;

namespace SmartRoadBridge.Database
{
    public class KenyaDatabase
    {
        MySqlConnection CurConn;
        private string DbName;
        public string ConnectionStr;


        /// <summary>
        /// 恢复数据库
        /// </summary>
        /// <param name="nn"></param>
        //public KenyaDatabase(string nn)
        //{
        //    CurConn = new MySqlConnection("data source=localhost;user id=root;password=1234;charset=utf8");
        //    CurConn.Open();    
        //    MySqlCommand cmd = null;
        //    cmd = new MySqlCommand(string.Format("USE {0};", nn), CurConn);
        //    cmd.ExecuteNonQuery();
        //    Console.WriteLine("#  数据库连接成功..");
        //    DbName = nn;
        //}


        public KenyaDatabase(string address,string user,string password, string dbname,int port=3306,bool ReNew=true)
        {
            DbName = dbname;
            MySqlCommand cmd = null;
            try
            {
                CurConn = new MySqlConnection(string.Format("server={0};port={3};user id={1};password={2};charset=utf8", address,user,password,port));
                CurConn.Open();
                Console.WriteLine("#  数据库连接成功..");           

                if (ReNew)
                {
                    cmd = new MySqlCommand(string.Format("DROP DATABASE IF EXISTS {0};", DbName), CurConn);
                    cmd.ExecuteNonQuery();
                    cmd = new MySqlCommand(string.Format("CREATE DATABASE {0};", DbName), CurConn);
                    cmd.ExecuteNonQuery();
                    cmd = new MySqlCommand(string.Format("USE {0};", DbName), CurConn);
                    cmd.ExecuteNonQuery();
                    CurConn.Close();
                    string ColumnStr;
                    ColumnStr = "X Double,Y Double,H1 Double,H2 Double,Dia Double";
                    CreatTable("Pile_tbl", ColumnStr);
                    ColumnStr = "X Double,Y Double,PK Double,DM Double, SJ Double,Dia Double,Bridge VarChar(10),Line VarChar(10)";
                    CreatTable("Pier_tbl", ColumnStr);
                    ColumnStr = "X Double,Y Double,L Double,T Double,H Double,Angle Double";
                    CreatTable("Footing_tbl", ColumnStr);
                    ColumnStr = "ID int,Bridge VarChar(10),Station double,Type VarChar(50),Ang0 double,Ang1 double,BrH0 double,BrH1 double," +
                        "X0 Double,Y0 Double,H0 Double,X1 Double,Y1 Double,H1 Double,IsSide VarChar(3),Slop Double";
                    CreatTable("Box_tbl", ColumnStr);
                    ColumnStr = "Name VarChar(10),ICD Text,SQX Text,DMX Text, CG Text";
                    CreatTable("EI_tbl", ColumnStr);
                    ColumnStr = "ID int,Bridge VarChar(10),PK Double,Angle Double," +
                        "CB1 VarChar(50),CB2 VarChar(50),CB3 VarChar(50),CB4 VarChar(50)," +
                        "P1 VarChar(50),P2 VarChar(50),P3 VarChar(50),H1 VarChar(50),H2 VarChar(50),H3 VarChar(50)";
                    CreatTable("pier_tbl", ColumnStr);

                    ColumnStr = "ID int,Line VarChar(10),Bridge VarChar(10),Station double,Type VarChar(10),Angle double, " +
                        "LeftWidth double, RightWidth double, SpaceList VarChar(100),PierAngleList VarChar(100),FundAngleList VarChar(100), H0 double, H1 double,SlopLeft double,SlopRight double";
                    CreatTable("sub_tbl", ColumnStr);

                    Console.WriteLine("#  数据库初始化成功..");

                    

                }
                else
                {
                    cmd = new MySqlCommand(string.Format("USE {0};", DbName), CurConn);
                    cmd.ExecuteNonQuery();
                    CurConn.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            ConnectionStr = string.Format("data source={0};port={3};user id={1};password={2};Database={4};charset=utf8", address, user, password,port,dbname);
      
        }


        ~KenyaDatabase()
        {
      
        }



        #region 方法
        public void ReadSpanInfo(string csvpath,ref Dictionary<string, Align> CLList)
        {
            CurConn.Open();
            MySqlCommand cmd = null;
            List<BridgeINFO> tmp = new List<BridgeINFO>();
            string tabname = Path.GetFileNameWithoutExtension(csvpath);
            using (var reader = new StreamReader(csvpath))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<BridgeINFOMap>();
                tmp = csv.GetRecords<BridgeINFO>().ToList();
            }
            CurConn.Close();

            string ColumnStr = "Line VarChar(50),Bridge VarChar(50),Station Double,Span Double,Angle Double," +
                "WidthRight Double,WidthLeft Double,Offset Double, SJX Double, DMX Double," +
                "SubType VarChar(10),SupType VarChar(10),FundType VarChar(10)";
            CreatTable(tabname, ColumnStr);                      

            CurConn.Open();
            foreach (var item in tmp)
            {
               Align CL = CLList[item.Line];
                string RecString = string.Format("INSERT INTO {0} values('{1}','{2}',{3},{4},{5}," +
                    "{6},{7},{8},{9},{10}," +
                    "'{11}','{12}','{13}');",
                    tabname, item.Line, item.Bridge, item.PK, item.Span, item.Angle,
                    item.Width * 0.5, item.Width * 0.5, item.Offset, CL.curSQX.GetBG(item.PK), CL.curDMX.GetBG(item.PK),
                    item.SubType, item.SupType, item.FundType);
                cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();
            }
            CurConn.Close();

        }


        public void ReadSpanInfo(string csvpath, ref Align CL )
        {
            CurConn.Open();
            MySqlCommand cmd = null;
            List<BridgeINFO> tmp = new List<BridgeINFO>();
            string tabname = Path.GetFileNameWithoutExtension(csvpath);
            using (var reader = new StreamReader(csvpath))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<BridgeINFOMap>();
                tmp = csv.GetRecords<BridgeINFO>().ToList();
            }
            CurConn.Close();

            string ColumnStr = "Line VarChar(50),Bridge VarChar(50),Station Double,Span Double,Angle Double," +
                "WidthRight Double,WidthLeft Double,Offset Double, SJX Double, DMX Double,"+                
                "SubType VarChar(10),SupType VarChar(10),FundType VarChar(10)";
            CreatTable(tabname, ColumnStr);

            CurConn.Open();
            foreach (var item in tmp)
            {
                string RecString = string.Format("INSERT INTO {0} values('{1}','{2}',{3},{4},{5}," +
                    "{6},{7},{8},{9},{10}," +
                    "'{11}','{12}','{13}');",
                    tabname, item.Line, item.Bridge, item.PK, item.Span, item.Angle, 
                    item.Width*0.5, item.Width*0.5, item.Offset,CL.curSQX.GetBG(item.PK),CL.curDMX.GetBG(item.PK),
                    item.SubType, item.SupType, item.FundType);
                cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();
            }
            CurConn.Close();


        }


        private void CreatTable(string v1, string tableString)
        {
            CurConn.Open();
            MySqlCommand cmd;

            cmd = new MySqlCommand(string.Format("DROP TABLE IF EXISTS {0};", v1), CurConn);
            cmd.ExecuteNonQuery();

            string cmdstr = string.Format("CREATE TABLE {0} (",v1);
            //cmdstr += "ID INT,";
            cmdstr += tableString;
            cmdstr += ");";
            cmd = new MySqlCommand(cmdstr, CurConn);
            cmd.ExecuteNonQuery();
            CurConn.Close();
                                          
        }
















        public void Update()
        {
            
        }

        public void UpdatePier(ref Align CenterLine, string InfoTable)
        {
            MySqlCommand msqlCommand = new MySqlCommand();
            msqlCommand.Connection = CurConn;
            msqlCommand.CommandText = string.Format("SELECT * FROM {0}", InfoTable);
            MySqlDataReader msqlReader = msqlCommand.ExecuteReader();

            List<double[]> res = new List<double[]>();

            while (msqlReader.Read())
            {
                double curPK = msqlReader.GetDouble(msqlReader.GetOrdinal("PK"));
                double sj=CenterLine.curSQX.GetBG(curPK);
                double dm = CenterLine.curDMX.GetBG(curPK);
                res.Add(new[] { curPK, dm, sj  });
            }

            msqlReader.Close();
            foreach (var item in res)
            {
                string RecString = string.Format("INSERT INTO Pier_tbl values(0,0,{0},{1},{2},0,'{3}','{4}');", 
                    item[0],item[1],item[2], InfoTable,CenterLine.Name);
                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();                
            }
            

        }


        public void UpdateBox(string brname, ref Dictionary<string,Align> CLList, double C2C = 0.54)
        {
            string selectString = string.Format("SELECT * FROM {0} order by station", brname);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, ConnectionStr);
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

                Align CL = CLList[line];
                Vector2 dir = new Vector2(CL.curPQX.GetDir(curPK));
                Vector2 Cdir = dir.RotByZ(angInDeg / 180.0 * Math.PI);
                Vector2 Center = new Vector2(CL.curPQX.GetCoord(curPK));//+dir*C2C;

                Line A0 = new Line(Center, Center + Cdir * leftW);
                Line B0 = new Line(Center, Center - Cdir * rightW);


                string Nextline = (string)next["Line"];
                double nexPK = (double)next["Station"];
                double NleftW = (double)next["WidthLeft"];
                double NrightW = (double)next["WidthRight"];
                double NangInDeg = (double)next["Angle"];
                Align NCL = CLList[Nextline];

                Vector2 Ndir = new Vector2(NCL.curPQX.GetDir(nexPK));
                Vector2 NCdir = Ndir.RotByZ(NangInDeg / 180.0 * Math.PI);
                Vector2 NCenter = new Vector2(NCL.curPQX.GetCoord(nexPK));//- Ndir* C2C;
                Line A1 = new Line(NCenter, NCenter + NCdir * NleftW);
                Line B1 = new Line(NCenter, NCenter - NCdir * NrightW);


                List<BoxBeam> beamList = BeamKnowledge.Arrange(curPK, nexPK, A0, B0, A1, B1, ref CL,ref NCL, C2C,C2C);


                string RecString;
                MySqlCommand cmd;
                CurConn.Open();
                foreach (BoxBeam beam in beamList)
                {
                    string BoxTypeString = beam.Length <= 25 ? "B25" : "B30";
                    string IsSide = beam.IsSideBeam ? "Y" : "N";
                    RecString = string.Format("INSERT INTO box_tbl values({11},'{12}',{13},'{0}',{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},'{14}',{15});",
                        BoxTypeString, beam.StartA.Degrees, beam.EndA.Degrees, beam.StartBearingH, beam.EndBearingH,
                        beam.StartPin.X, beam.StartPin.Y, beam.StartPin.Z,
                        beam.EndPin.X, beam.EndPin.Y, beam.EndPin.Z,
                        0, brname, curPK, IsSide, beam.DeckSlope
                        );
                    cmd = new MySqlCommand(RecString, CurConn);
                    cmd.ExecuteNonQuery();
                }
                CurConn.Close();
            }
            Console.WriteLine("#  {0} 已配置上部结构...", brname);
        }


        //public void UpdateBox(string brname, ref Align CL,double C2C=0.54)
        //{
        //    string selectString = string.Format("SELECT * FROM {0} order by station", brname);           
        //    MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, ConnectionStr);
        //    DataSet dataset = new DataSet();
        //    adapter.Fill(dataset);
        //    adapter.Dispose();
        //    DataTable dt = dataset.Tables[0];

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        DataRow item = dt.Rows[i];
        

        //        if ((string)item["SupType"]!="B"|| i==dt.Rows.Count-1)
        //        {
        //            continue;
        //        }

        //        DataRow next = dt.Rows[i + 1];
        //        double curPK = (double)item["Station"];
        //        double leftW = (double)item["WidthLeft"];
        //        double rightW = (double)item["WidthRight"];
        //        double angInDeg = (double)item["Angle"];

        //        Vector2 dir = new Vector2(CL.curPQX.GetDir(curPK));                
        //        Vector2 Cdir = dir.RotByZ(angInDeg/180.0*Math.PI);
        //        Vector2 Center = new Vector2(CL.curPQX.GetCoord(curPK));//+dir*C2C;

        //        Line A0 = new Line(Center, Center + Cdir * leftW);
        //        Line B0 = new Line(Center, Center - Cdir * rightW);


        //        double nexPK = (double)next["Station"];
        //        double NleftW = (double)next["WidthLeft"];
        //        double NrightW = (double)next["WidthRight"];
        //        double NangInDeg = (double)next["Angle"];

        //        Vector2 Ndir = new Vector2(CL.curPQX.GetDir(nexPK));                
        //        Vector2 NCdir = Ndir.RotByZ(NangInDeg / 180.0 * Math.PI);
        //        Vector2 NCenter = new Vector2(CL.curPQX.GetCoord(nexPK));//- Ndir* C2C;
        //        Line A1 = new Line(NCenter, NCenter + NCdir * NleftW);
        //        Line B1 = new Line(NCenter, NCenter - NCdir * NrightW);


        //        List<BoxBeam> beamList= BeamKnowledge.Arrange(curPK,nexPK,A0, B0,A1, B1,ref CL, C2C);


        //        string RecString;
        //        MySqlCommand cmd;
        //        CurConn.Open();
        //        foreach (BoxBeam beam in beamList)
        //        {
        //            string BoxTypeString= beam.Length <= 25 ? "B25" : "B30";
        //            string IsSide = beam.IsSideBeam ? "Y" : "N";
        //            RecString = string.Format("INSERT INTO box_tbl values({11},'{12}',{13},'{0}',{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},'{14}',{15});",
        //                BoxTypeString, beam.StartA.Degrees, beam.EndA.Degrees, beam.StartBearingH, beam.EndBearingH,
        //                beam.StartPin.X, beam.StartPin.Y, beam.StartPin.Z,
        //                beam.EndPin.X, beam.EndPin.Y, beam.EndPin.Z,
        //                0, brname, curPK, IsSide, beam.HP
        //                ) ;
        //            cmd = new MySqlCommand(RecString, CurConn);
        //            cmd.ExecuteNonQuery();
        //        }
        //        CurConn.Close();
        //    }




        //    Console.WriteLine("#  {0} 已配置上部结构...", brname);


        //}

        /// <summary>
        /// 重写数据
        /// </summary>
        /// <param name="v"></param>
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
            CurConn.Open();
            foreach (var item in tmp)
            {

                string typestr = item.Type;
                var dd = (from a in item.SpaceList select a.ToString()).ToArray();
                var SpaceList = string.Join(",", dd);
                var aa = (from a in item.PierAngleList select a.ToString()).ToArray();
                var PierAngleList = string.Join(",", aa);
                var ff = (from a in item.PierAngleList select a.ToString()).ToArray();
                var FundAngleList = string.Join(",", ff);

                string cmdstr = "";
                cmdstr += UpdateMySql(item.Station, item.Bridge, item.Type,"", "Type");
                cmdstr += UpdateMySql(item.Station, item.Bridge, item.Angle, 90, "Angle");
                cmdstr += UpdateMySql(item.Station, item.Bridge, item.LeftWidth, 0, "LeftWidth");
                cmdstr += UpdateMySql(item.Station, item.Bridge, item.RightWidth, 0, "RightWidth");
                cmdstr += UpdateMySql(item.Station, item.Bridge, SpaceList, "0,0,0,0", "SpaceList");
                cmdstr += UpdateMySql(item.Station, item.Bridge, PierAngleList, "0,0,0,0", "PierAngleList");
                cmdstr += UpdateMySql(item.Station, item.Bridge, FundAngleList, "0,0,0,0", "FundAngleList");


                //if (item.LeftWidth==0 && item.RightWidth==0)
                //{
                //    cmdstr = string.Format("UPDATE sub_tbl set Type='{4}',SpaceList='{0}',PierAngleList='{1}',FundAngleList='{5}' where Station={2} and Bridge='{3}';",
                //        SpaceList, PierAngleList, item.Station, item.Bridge, typestr,FundAngleList);
                //}
                //else if (item.LeftWidth != 0 && item.RightWidth != 0)
                //{
                //    cmdstr = string.Format("UPDATE sub_tbl set Type='{4}',SpaceList='{0}',PierAngleList='{1}',FundAngleList='{7}',RightWidth={5},LeftWidth={6} where Station={2} and Bridge='{3}';",
                //        SpaceList, PierAngleList, item.Station, item.Bridge, typestr,item.RightWidth,item.LeftWidth, FundAngleList);
                //}
                //else if (item.LeftWidth==0)
                //{
                //    cmdstr = string.Format("UPDATE sub_tbl set Type='{4}',SpaceList='{0}',PierAngleList='{1}',FundAngleList='{6}',RightWidth={5} where Station={2} and Bridge='{3}';",
                //        SpaceList, PierAngleList, item.Station, item.Bridge, typestr, item.RightWidth, FundAngleList);
                //}
                //else
                //{
                //    cmdstr = string.Format("UPDATE sub_tbl set Type='{4}',SpaceList='{0}',PierAngleList='{1}',FundAngleList='{6}',LeftWidth={5} where Station={2} and Bridge='{3}';",
                //        SpaceList, PierAngleList, item.Station, item.Bridge, typestr, item.LeftWidth, FundAngleList);
                //}


                MySqlCommand cmd = new MySqlCommand(cmdstr, CurConn);
                cmd.ExecuteNonQuery();
            }
            CurConn.Close();
            

        }



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

        private string UpdateMySql(double station, string bridge, string value, string condition,string key,bool force=false)
        {
            string res=string.Empty;
            if (force)
            {
                res = string.Format("UPDATE sub_tbl set {0}='{1}' where Station={2} and Bridge='{3}';",key, value, station, bridge);
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

        public void UpdateColumn(string brname,ref DxfDocument SideLine , ref Dictionary<string, Align> CLList)
        {
            string selectString = string.Format("SELECT * FROM {0} order by Station asc", brname);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, ConnectionStr);
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
                string trName = (string)item["Bridge"]=="SEC2-10"?"SEC210": string.Join("0", ((string)item["Bridge"]).Split('-').ToArray());                
                if (trName!=brname||((string)item["SubType"])[0]!='C')
                {
                    continue;
                }
                double curPK =(double)item["Station"];
                double WidthLeft = (double)item["WidthLeft"];
                double WidthRight = (double)item["WidthRight"];
                Angle Theta = Angle.FromDegrees((double)item["Angle"]);
                Align RefCL = GetCLByName((string)item["FundType"]);

                Vector2D Center = new Vector2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);
                Vector2D dir = new Vector2D(CL.curPQX.GetDir(curPK)[0], CL.curPQX.GetDir(curPK)[1]);                
                Vector2D Cdir = dir.Rotate(Theta);

                SubStructure thisSB = new PierNone();


                if (brname.StartsWith("SEC"))
                {
                    double DistLR = 0;
                    if (LineName=="L1K" && (curPK<=19100||curPK>=19750))
                    {
                        Align RK = GetCLByName("R1K");

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
                    thisSB = PierKnowledge.Arrange2("F",curPK, Theta, WidthLeft, WidthRight, DistListLeft, DistListRight, ref CL,0.5* DistLR);

                }
                else
                {
                    DataRow Pre = dt.Rows[i-1];
                    thisSB = PierKnowledge.ArrangeRamp(curPK, Theta,ref CL,item,Pre);
                }



                // -----------------------------------------------------------------------------------


                MySqlCommand cmd;
                CurConn.Open();


                string tmp = string.Format("Insert into sub_tbl values({0},'{1}','{2}',{3},'{4}',{5},{6},{7},'{8}','{9}','{10}',{11},{12},{13},{14});",
                    thisSB.ID, item["Line"], item["Bridge"], curPK, thisSB.TypeStr, thisSB.Theta.Degrees, thisSB.CapBeamLeft, thisSB.CapBeamRight,
                    thisSB.DistList.ToString2(), thisSB.PierAngList.ToString2(),thisSB.FundAngList.ToString2(), thisSB.H0, thisSB.H1,thisSB.SlopLeft,thisSB.SlopRight) ;

                cmd = new MySqlCommand(tmp, CurConn);
                cmd.ExecuteNonQuery();
                CurConn.Close();
            }
            Console.WriteLine("#  {0} 已配置下部结构...", brname);
        }

        private Align GetCLByName(string RampName)
        {
            string EIName = RampName;
            if (RampName=="SEC204R")
            {
                EIName = "R1K";
            }
            if (RampName != "C2F")
            {
                string selectString = string.Format("SELECT * FROM ei_tbl where Name='{0}'", EIName);
                MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, ConnectionStr);
                DataSet tmp = new DataSet();
                adapter.Fill(tmp);
                var EIdt = tmp.Tables[0];
                adapter.Dispose();

                var icd = ((string)EIdt.Rows[0]["ICD"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var dmx = ((string)EIdt.Rows[0]["DMX"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var cg = ((string)EIdt.Rows[0]["CG"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var sqx = ((string)EIdt.Rows[0]["SQX"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                Align refRampCL = new Align(RampName, icd, sqx, dmx, cg);

                return refRampCL;
            }
            else
            {
                return null;
            }
        }


        public void UpdateWidth(string v, ref DxfDocument MainRoad, ref Dictionary<string,Align> CLList)
        {
            CurConn.Open();

            string selectString = string.Format("SELECT * FROM {0}", v);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, ConnectionStr);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();

            List<double> res = new List<double>();
            List<double> AList = new List<double>();
            List<double> BList = new List<double>();


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                
                DataRow item = dt.Rows[i];
                Align CL = CLList[(string)item["Line"]];
                double curPK = (double)item["Station"];
                Angle Theta = Angle.FromDegrees((double)item["Angle"]);

                if (curPK==19806)
                {
                    ;

                }
                res.Add(curPK);
                Vector2 Center = new Vector2(CL.curPQX.GetCoord(curPK));
                Vector2 dir = new Vector2(CL.curPQX.GetDir(curPK));
                Vector2 Cdir = dir.RotByZ(Theta.Radians);

                Line A = new Line(Center, Center + Cdir * 50);
                Line B = new Line(Center, Center - Cdir * 50);
                double DistA = 100;
                double DistB = 100;
                foreach (Line line in MainRoad.Lines)
                {
                    var f = A.Intersectwith(line.Flatten());
                    if (f != null)
                    {
                        Vector2 pt = (Vector2)f;
                        DistA = Math.Min(DistA, (pt - Center).Modulus());
                    }
                    var g = B.Intersectwith(line.Flatten());
                    if (g != null)
                    {
                        Vector2 pt = (Vector2)g;
                        DistB = Math.Min(DistB, (pt - Center).Modulus());
                    }
                }
                foreach (netDxf.Entities.Arc line in MainRoad.Arcs)
                {
                    var f = A.Intersectwith(line);
                    if (f != null)
                    {
                        Vector2 pt = (Vector2)f;
                        DistA = Math.Min(DistA, (pt - Center).Modulus());
                    }
                    var g = B.Intersectwith(line);
                    if (g != null)
                    {
                        Vector2 pt = (Vector2)g;
                        DistB = Math.Min(DistB, (pt - Center).Modulus());
                    }
                }

                AList.Add(DistA);
                BList.Add(DistB);
            }



            if (res.Count != AList.Count || res.Count != BList.Count)
            {
                ;

            }

            for (int k = 0; k < res.Count; k++)
            {
                double dl = Math.Round(AList[k], 6, MidpointRounding.AwayFromZero);
                double dr = Math.Round(BList[k], 6, MidpointRounding.AwayFromZero);
                string RecString = string.Format("UPDATE {0} set WidthRight={1},WidthLeft={2} where Station={3};",
                    v, dr, dl, res[k]);
                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();


            }
            Console.WriteLine("#  " + v + "  已更新桥宽度...");
            CurConn.Close();


            // 切割匝道
            CurConn.Open();
            selectString = string.Format("SELECT * FROM {0}", v);
            adapter = new MySqlDataAdapter(selectString, ConnectionStr);
            dataset = new DataSet();
            adapter.Fill(dataset);
            dt = dataset.Tables[0];
            adapter.Dispose();

            foreach (DataRow item in dt.Rows)
            {
                Align CL = CLList[(string)item["Line"]];
                string RampName = (string)item["FundType"];

                if (RampName != "C2F")
                {
                    double rampleftwidth = 4.5;
                    double ramprightwidth = 4.5;
                    string eiline = RampName;
                    if (RampName=="SEC204R")
                    {
                        eiline = "R1K";
                        ramprightwidth = 6.5;
                    }
                    else if (RampName=="SEC204L")
                    {
                        eiline = "L1K";
                        rampleftwidth = 6.5;
                    }
                    else if (RampName == "MUB")
                    {
                        rampleftwidth = 3.5;
                        ramprightwidth = 3.5;
                    }
                    else if (RampName == "EBA"|| RampName == "SBE")
                    {
                        rampleftwidth = 5.0;
                        ramprightwidth = 5.0;
                    }
                    selectString = string.Format("SELECT * FROM ei_tbl where Name='{0}'", eiline);
                    adapter = new MySqlDataAdapter(selectString, ConnectionStr);
                    DataSet tmp = new DataSet();
                    adapter.Fill(tmp);
                    var EIdt = tmp.Tables[0];
                    adapter.Dispose();

                    //selectString = string.Format("SELECT WidthRight,WidthLeft FROM {0} LIMIT 1;", RampName);
                    //adapter = new MySqlDataAdapter(selectString, ConnectionStr);
                    //tmp = new DataSet();
                    //adapter.Fill(tmp);
                    //var WidhtList = (tmp.Tables[0].Rows[0].ItemArray);
                    //adapter.Dispose();


                    var icd = ((string)EIdt.Rows[0]["ICD"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var dmx = ((string)EIdt.Rows[0]["DMX"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var cg = ((string)EIdt.Rows[0]["CG"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var sqx = ((string)EIdt.Rows[0]["SQX"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    Align refRampCL = new Align(RampName, icd, sqx, dmx, cg);

                    double curPK = (double)item["Station"];
                    Point2D MLCC = new Point2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);
                    Vector2D Dir = new Vector2D(CL.curPQX.GetDir(curPK)[0], CL.curPQX.GetDir(curPK)[1]);
                    Vector2D Cdir = Dir.Rotate(Angle.FromDegrees((double)item["Angle"]));
                    double pkleft = refRampCL.curPQX.GetStation(MLCC.X, MLCC.Y, (MLCC + Cdir).X, (MLCC + Cdir).Y, rampleftwidth);
                    double pk0 = refRampCL.curPQX.GetStation(MLCC.X, MLCC.Y, (MLCC + Cdir).X, (MLCC + Cdir).Y, 0);
                    double pkright = refRampCL.curPQX.GetStation(MLCC.X, MLCC.Y, (MLCC + Cdir).X, (MLCC + Cdir).Y, -ramprightwidth);




                    Point2D LeftPt = refRampCL.curPQX.GetPoint2D(pkleft, rampleftwidth);
                    Point2D RPCC = refRampCL.curPQX.GetPoint2D(pk0, 0);
                    Point2D RightPt = refRampCL.curPQX.GetPoint2D(pkright, -ramprightwidth);

                    var angle = refRampCL.curPQX.GetDirVector2D(pk0).SignedAngleTo(LeftPt - RightPt);

                    string RecString = string.Format("INSERT INTO {0} values('{1}','{2}',{3},30,{4},{5},{6},0,{7},{8},'{9}','B','C2F');",
                        refRampCL.Name, eiline, RampName.Replace("0","-"), pk0, angle.Degrees, LeftPt.DistanceTo(RPCC), RightPt.DistanceTo(RPCC),
                        refRampCL.curSQX.GetBG(pk0), refRampCL.curDMX.GetBG(pk0),v);

                    MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                    cmd.ExecuteNonQuery();
                }
            }
            CurConn.Close();



        }

        public void UpdateWidth(string v, ref DxfDocument MainRoad, ref Align CL)
        {
            CurConn.Open();

            string selectString = string.Format("SELECT * FROM {0}", v);
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectString, ConnectionStr);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            DataTable dt = dataset.Tables[0];
            adapter.Dispose();

            List<double> res = new List<double>();
            List<double> AList = new List<double>();
            List<double> BList = new List<double>();


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow item = dt.Rows[i];
                double curPK = (double)item["Station"];
                Angle Theta = Angle.FromDegrees((double)item["Angle"]);

                res.Add(curPK);
                Vector2 Center = new Vector2(CL.curPQX.GetCoord(curPK));
                Vector2 dir = new Vector2(CL.curPQX.GetDir(curPK));
                Vector2 Cdir = dir.RotByZ(Theta.Radians);

                Line A = new Line(Center, Center + Cdir * 50);
                Line B = new Line(Center, Center - Cdir * 50);
                double DistA = 100;
                double DistB = 100;
                foreach (Line line in MainRoad.Lines)
                {
                    var f = A.Intersectwith(line.Flatten());
                    if (f != null)
                    {
                        Vector2 pt = (Vector2)f;
                        DistA = Math.Min(DistA, (pt - Center).Modulus());
                    }
                    var g = B.Intersectwith(line.Flatten());
                    if (g != null)
                    {
                        Vector2 pt = (Vector2)g;
                        DistB = Math.Min(DistB, (pt - Center).Modulus());
                    }
                }
                foreach (netDxf.Entities.Arc line in MainRoad.Arcs)
                {
                    var f = A.Intersectwith(line);
                    if (f != null)
                    {
                        Vector2 pt = (Vector2)f;
                        DistA = Math.Min(DistA, (pt - Center).Modulus());
                    }
                    var g = B.Intersectwith(line);
                    if (g != null)
                    {
                        Vector2 pt = (Vector2)g;
                        DistB = Math.Min(DistB, (pt - Center).Modulus());
                    }
                }

                AList.Add(DistA);
                BList.Add(DistB);
            }



            if (res.Count!=AList.Count||res.Count!=BList.Count)
            {
                ;

            }

            for (int k = 0; k < res.Count; k++)
            {
                double dl = Math.Round(AList[k], 6, MidpointRounding.AwayFromZero);
                double dr = Math.Round(BList[k], 6, MidpointRounding.AwayFromZero);
                string RecString = string.Format("UPDATE {0} set WidthRight={1},WidthLeft={2} where Station={3};",
                    v,dr , dl,res[k]);
                MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                cmd.ExecuteNonQuery();


            }            
            Console.WriteLine("#  "+v + "  已更新桥宽度...");
            CurConn.Close();


            // 切割匝道
            CurConn.Open();
            selectString = string.Format("SELECT * FROM {0}", v);
            adapter = new MySqlDataAdapter(selectString, ConnectionStr);
            dataset = new DataSet();
            adapter.Fill(dataset);
            dt = dataset.Tables[0];
            adapter.Dispose();

            foreach (DataRow item in dt.Rows)
            {
                string RampName = (string)item["FundType"];
                if (RampName != "C2F")
                {
                    selectString = string.Format("SELECT * FROM ei_tbl where Name='{0}'", RampName);
                    adapter = new MySqlDataAdapter(selectString, ConnectionStr);
                    DataSet tmp = new DataSet();
                    adapter.Fill(tmp);
                    var EIdt = tmp.Tables[0];
                    adapter.Dispose();

                    selectString = string.Format("SELECT WidthRight,WidthLeft FROM {0} LIMIT 1;",RampName);
                    adapter = new MySqlDataAdapter(selectString, ConnectionStr);
                    tmp = new DataSet();
                    
                    adapter.Fill(tmp);
                    var WidhtList = (tmp.Tables[0].Rows[0].ItemArray);
                    adapter.Dispose();


                    var icd=((string)EIdt.Rows[0]["ICD"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var dmx = ((string)EIdt.Rows[0]["DMX"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var cg = ((string)EIdt.Rows[0]["CG"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var sqx = ((string)EIdt.Rows[0]["SQX"]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    Align refRampCL = new Align(RampName, icd, sqx, dmx, cg);

                    double curPK = (double)item["Station"];
                    Point2D MLCC = new Point2D(CL.curPQX.GetCoord(curPK)[0], CL.curPQX.GetCoord(curPK)[1]);
                    Vector2D Dir = new Vector2D(CL.curPQX.GetDir(curPK)[0], CL.curPQX.GetDir(curPK)[1]);
                    Vector2D Cdir = Dir.Rotate(Angle.FromDegrees((double)item["Angle"]));
                    double pkleft= refRampCL.curPQX.GetStation(MLCC.X, MLCC.Y, (MLCC + Cdir).X, (MLCC + Cdir).Y, 4.5);
                    double pk0  = refRampCL.curPQX.GetStation(MLCC.X, MLCC.Y, (MLCC + Cdir).X, (MLCC + Cdir).Y, 0);
                    double pkright= refRampCL.curPQX.GetStation(MLCC.X, MLCC.Y, (MLCC + Cdir).X, (MLCC + Cdir).Y, -4.5);




                    Point2D LeftPt = refRampCL.curPQX.GetPoint2D(pkleft, (double)WidhtList[0]);
                    Point2D RPCC = refRampCL.curPQX.GetPoint2D(pk0, 0);
                    Point2D RightPt = refRampCL.curPQX.GetPoint2D(pkright, -(double)WidhtList[1]);

                    var angle=refRampCL.curPQX.GetDirVector2D(pk0).SignedAngleTo(LeftPt - RightPt);

                    string RecString = string.Format("INSERT INTO {0} values('{1}','{2}',{3},30,{4},{5},{6},0,{7},{8},'{9}','B','C2F');",
                        refRampCL.Name, refRampCL.Name, RampName,pk0, angle.Degrees, LeftPt.DistanceTo(RPCC), RightPt.DistanceTo(RPCC),
                        refRampCL.curSQX.GetBG(pk0), refRampCL.curDMX.GetBG(pk0), v);

                    MySqlCommand cmd = new MySqlCommand(RecString, CurConn);
                    cmd.ExecuteNonQuery();
                }
            }
            CurConn.Close();



        }              


        public void ReadEIData(string EIName,string workDir)
        {
            CurConn.Open();
            var dd = new DirectoryInfo(workDir);
            string Name = dd.Name;
            string altexticd = File.ReadAllText(dd.FullName + "\\" + Name + ".ICD", Encoding.Default);
            string altextsqx = File.ReadAllText(dd.FullName + "\\" + Name + ".SQX", Encoding.Default);
            string altextcg = File.ReadAllText(dd.FullName + "\\" + Name + ".CG", Encoding.Default);
            string altextdmx = File.ReadAllText(dd.FullName + "\\" + Name + ".DMX", Encoding.Default);


            string RecString = string.Format("INSERT INTO EI_tbl values('{0}','{1}','{2}','{3}','{4}');", EIName, altexticd, altextsqx, altextdmx, altextcg);

            string utf8_string = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(RecString));


            MySqlCommand cmd = new MySqlCommand(utf8_string, CurConn);
            cmd.ExecuteNonQuery();

            Console.WriteLine("#  " + Name + "  已更新EI数据...");
            CurConn.Close();

        }

        public void UpdatePile(ref Align CenterLine)
        {
            MySqlCommand msqlCommand = new MySqlCommand();
            msqlCommand.Connection = CurConn;
            msqlCommand.CommandText = "SELECT * FROM testdb";
            MySqlDataReader msqlReader = msqlCommand.ExecuteReader();

            while (msqlReader.Read())
            {
                double PK0 = msqlReader.GetDouble(msqlReader.GetOrdinal("StartPK"));
                var SpList = PublicFun.GetNum(msqlReader.GetString(msqlReader.GetOrdinal("SpanList")),",");
                var Ang = msqlReader.GetDouble(msqlReader.GetOrdinal("Angle"));
                double curPK=PK0;
                for (int i = 0; i < SpList.Count+1; i++)
                {
                    if (i==0)
                    {
                        curPK += 0;
                    }
                    else
                    {
                        curPK += SpList[i - 1];
                    }
                    Point2D c = new Point2D(CenterLine.curPQX.GetCoord(curPK)[0], CenterLine.curPQX.GetCoord(curPK)[1]);                    
                    Angle dir = CenterLine.curPQX.GetDirection(curPK);                    
                    int RN=0,CN=0;
                    double Rs=0, Cs=0;
                    PileKnowledge.GetArr(curPK, out RN,out CN, out Rs, out Cs);



                }
            }
        }






        #endregion
    }
}
