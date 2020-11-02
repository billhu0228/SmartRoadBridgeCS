using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System;

namespace SmartRoadBridge.Alignment
{
    public class Arc : PQXElement
    {
        public Arc(double r, double length, Point2D st, Angle sdir, LeftRightEnum dir, EITypeID idd = EITypeID.Arc) :
            base(idd, st, sdir, dir)
        {
            Radius = r;
            _length = length;

        }
        #region 字段
        private double _length;
        #endregion



        #region 属性
        public double Radius { get; }
        public override double Length { get { return _length; } }

        #endregion

        #region 方法
        public override Point2D GetPointOnCurve(double l)
        {
            double rad = l / Radius;
            double x = Radius * Math.Sin(rad);
            double y = Radius * (1 - Math.Cos(rad)) * (int)LeftRight;

            Vector2D res = new Vector2D(y, x).Rotate(-StartAngle);

            return StartPoint + res;


        }
        protected override Angle UpdateEndAngle()
        {
            return StartAngle + (int)LeftRight * Angle.FromRadians(Length / Radius);
        }

        #endregion
    }
}
