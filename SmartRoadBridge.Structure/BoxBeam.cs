using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using SmartRoadBridge.Public;

namespace SmartRoadBridge.Structure
{

    public class BoxBeam
    {
        public int ID;
        public string Name;
        public Point3D StartPin, EndPin;
        public double StartL, EndL;
        public Angle StartA, EndA;
        public double StartBearingH, EndBearingH;
        public bool IsSideBeam;
        public double DeckSlope;
        public string BeamType;

        public double Length { get { return StartPin.DistanceTo(EndPin) + StartL + EndL; } }
        public double Length2D { get { return StartPin.Convert2D().DistanceTo(EndPin.Convert2D()) + StartL + EndL; } }

        public BoxBeam(Point3D startPin, Point3D endPin,
            double startH = 0.3, double endH = 0.3,
            double startL=0.5, double endL=0.5, double startA=90, double endA =90,bool isside=false,
            double deckSlope=0.0,
            string typestring="None"
            )
        {
            StartPin = startPin;
            EndPin = endPin;
            StartL = startL;
            EndL = endL;
            StartA =Angle.FromDegrees( startA);
            EndA = Angle.FromDegrees( endA);
            StartBearingH = startH;
            EndBearingH = endH;
            IsSideBeam = isside;
            DeckSlope = deckSlope;
            BeamType = typestring;
        }
    }
}
