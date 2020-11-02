using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System.Collections.Generic;

namespace SmartRoadBridge.Structure
{
    public class PierR : SubStructure
    {

        public static new readonly double CBHight = 1.6;
        public static readonly double W = 2.4;

        public PierR()
        {
        }

        public PierR(int iD, string str, double station, double h0, double h1, Point2D center, Angle theta, double slopLeft, double slopRight, double cbleft, double cbright) 
            : base(iD, str, station, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright)
        {
        }
    }
    
}
