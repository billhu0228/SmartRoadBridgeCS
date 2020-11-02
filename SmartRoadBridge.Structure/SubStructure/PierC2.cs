using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System.Collections.Generic;

namespace SmartRoadBridge.Structure
{
    public class PierC2 : SubStructure
    {
        public double CantiLeft, CantiRight;

        public static double XBThick = 1.6;
        public static readonly double CAT01 = 7.45;
        public static readonly double CAT02 = 7.45;//8.45
        public static readonly double CAT03 = 7.55;
        public static readonly double W1 = 2.0;
        public static readonly double W2 = 2.0;

        public static new double CBHight => 2.2;

        public PierC2()
        {

        }

        public PierC2(int iD,string str, double station, double h0, double h1,
            Point2D center, Angle theta,
            double slopLeft, double slopRight,
            double cbleft, double cbright,
            double ctleft, double ctright
            ) :
            base(iD,str, station, h0, h1, center, theta, slopLeft, slopRight,cbleft,cbright)
        {
            CantiLeft = ctleft;
            CantiRight = ctright;
        }

        public static void GetParameter(double DeckWidth, bool LeftEnLarge, bool RightEnLarge,
            out string TypeName, out double DeckCantLeft, out double DeckCantRight, out double DeckToCBL, out double DeckToCBR)
        {
            TypeName = "";
            DeckCantLeft = 7.8;
            DeckCantRight = 7.8;

            if (DeckWidth <= 21.61)
            {
                TypeName = "C2MB01";
            }
            else
            {
                TypeName = "C2MB02";
            }
            //if (LeftEnLarge)
            //{
            //    //TypeName += "B";
            //}
            //else
            //{
            //    //TypeName += "A";
            //}
            //if (RightEnLarge)
            //{
            //    //TypeName += "B";
            //}
            //else
            //{
            //    //TypeName += "A";
            //}

            //if (DeckWidth <= 25.6)
            //{
            //    DeckCantLeft = LeftEnLarge ? 8.8 : 7.8;
            //    DeckCantRight = RightEnLarge ? 8.8 : 7.8;
            //}
            //else
            //{
            //    DeckCantLeft = LeftEnLarge ? 8.9 : 7.9;
            //    DeckCantRight = RightEnLarge ? 8.9 : 7.9;
            //}
            DeckToCBL = 0.35;
            DeckToCBR = 0.35;
        }


    }
}
