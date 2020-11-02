using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace SmartRoadBridge.Structure
{
    public class PierF : SubStructure
    {
        public double CantiLeft, CantiRight;
        public static readonly double FC2W=1.8;
        public static readonly double FC1W=1.8;
        public static readonly double F3CBHight = 2.5;
        public static readonly double F3W=1.8;
        public static readonly double F3WM = 2.0;
        public static readonly double F2CBHight=2.5;
        public static readonly double F2W=1.8;

        public static new double CBHight => 2.0;
        public PierF()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="str"></param>
        /// <param name="station"></param>
        /// <param name="h0"></param>
        /// <param name="h1"></param>
        /// <param name="center"></param>
        /// <param name="theta"></param>
        /// <param name="slopLeft"></param>
        /// <param name="slopRight"></param>
        /// <param name="cbleft">左盖梁</param>
        /// <param name="cbright">右盖梁</param>
        /// <param name="ctleft"></param>
        /// <param name="ctright"></param>
        public PierF(int iD, string str, double station, double h0, double h1, 
            Point2D center, Angle theta, double slopLeft, double slopRight, double cbleft, double cbright,
            double ctleft,double ctright) : 
            base(iD, str, station, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright)
        {
            CantiLeft = ctleft;
            CantiRight = ctright;

        }
    }
}
