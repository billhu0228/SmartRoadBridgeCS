using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoadBridge.Structure
{
    class Abut : SubStructure
    {
        public Abut()
        {
        }

        public Abut(int iD, string str, double station, double h0, double h1, Point2D center, Angle theta, double slopLeft, double slopRight, double cbleft, double cbright) : base(iD, str, station, h0, h1, center, theta, slopLeft, slopRight, cbleft, cbright)
        {
        }
    }
}
