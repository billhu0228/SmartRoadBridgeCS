using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System.Collections.Generic;

namespace SmartRoadBridge.Structure
{    
    public abstract class SubStructure
    {
        public string Name;
        public string SpanName;
        public int ID;
        public string TypeStr;
        public double Station;
        public double H0, H1;
        public Point2D Center;
        public Angle Theta;
        public double SlopLeft, SlopRight;
        public List<double> DistList, PierAngList,FundAngList,FundHList;

        public double CapBeamLeft, CapBeamRight;

        public List<Point3D> CapBeamCPList;
        public List<Point3D> PierTopList;
        public List<Point3D> PierBotList;
        public static readonly double Cx = 0.35;
        public static readonly double Cx2 = 0.5;
        public static readonly double Fx = 0.5;

        public static double CBHight { get { return 0.0; } }

        public SubStructure()
        {
            Name = "";
            SpanName = "";
            DistList = new List<double>() { 0, 0, 0, 0 };
            PierAngList = new List<double>() { 0, 0, 0, 0 };
            FundAngList = new List<double>() { 0, 0, 0, 0 };
            FundHList = new List<double>() { 0, 0, 0, 0 };
            CapBeamCPList = new List<Point3D>() { new Point3D(), new Point3D(), new Point3D(), new Point3D() };
            PierTopList = new List<Point3D>() { new Point3D(), new Point3D(), new Point3D(), new Point3D() };
            PierBotList = new List<Point3D>() { new Point3D(), new Point3D(), new Point3D(), new Point3D() };
            Theta = Angle.FromDegrees(90.0);
        }

        protected SubStructure(int iD,string str, double station, double h0, double h1, 
            Point2D center, Angle theta, double slopLeft, double slopRight,double cbleft,double cbright)
        {
            ID = iD;
            TypeStr = str;
            Station = station;
            H0 = h0;
            H1 = h1;
            Center = center;
            Theta = theta;
            SlopLeft = slopLeft;
            SlopRight = slopRight;
            CapBeamLeft = cbleft;
            CapBeamRight = cbright;

            DistList = new List<double>() { 0, 0, 0, 0 };
            PierAngList = new List<double>() { 0, 0, 0, 0 };
            FundAngList = new List<double>() { 0, 0, 0, 0 };
            FundHList = new List<double>() { 0, 0, 0, 0 };
            CapBeamCPList = new List<Point3D>() { new Point3D(), new Point3D(), new Point3D(), new Point3D() };
            PierTopList = new List<Point3D>() { new Point3D(), new Point3D(), new Point3D(), new Point3D() };
            PierBotList = new List<Point3D>() { new Point3D(), new Point3D(), new Point3D(), new Point3D() };
            SpanName = "";
            Name = "";
        }
    }
}
