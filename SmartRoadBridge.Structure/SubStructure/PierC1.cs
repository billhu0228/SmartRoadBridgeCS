using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System.Collections.Generic;

namespace SmartRoadBridge.Structure
{
    public class PierC1 : SubStructure
    {
        public static new readonly double CBHight=2.5;
        public static readonly double W=4.0;

        public PierC1()
        {

        }

        public PierC1(int iD, string str, double station, double h0, double h1,
            Point2D center, Angle theta,
            double slopLeft, double slopRight,
            double cbleft, double cbright
            ) :
            base(iD, str, station, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright)
        {
            DistList = new List<double>() { cbleft, 0, 0, 0 };
        }

        
    }
}
