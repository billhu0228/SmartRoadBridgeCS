using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace SmartRoadBridge.Alignment
{
    public class Straight : PQXElement
    {


        public Straight(double length, Point2D st, Angle sdir, EITypeID idd = EITypeID.Line, LeftRightEnum dir = LeftRightEnum.None) :
            base(idd, st, sdir, dir)
        {
            _length = length;
        }


        #region 字段
        double _length;
        #endregion

        #region 属性
        public override double Length { get { return _length; } }
        #endregion

        #region 方法

        public override Point2D GetPointOnCurve(double l)
        {
            double x = l;
            double y = 0;
            Vector2D res = new Vector2D(y, x).Rotate(-StartAngle);

            return StartPoint + res;
        }

        protected override Angle UpdateEndAngle()
        {
            return StartAngle;
        }

        #endregion
    }
}
