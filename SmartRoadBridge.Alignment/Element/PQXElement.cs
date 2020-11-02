using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace SmartRoadBridge.Alignment
{

    public enum EITypeID { None = 0, Line = 1, Arc = 2, ZHY = 3, YHZ = 4, BigRtoSmallR = 5, SmallRtoBigR = 6 }
    public enum LeftRightEnum { None = 0, Left = -1, Right = 1 }
    public enum EnumTypeID { ST = -1, ED = -2, JD3 = 3 }
    public enum EnumKP { QD, ZH, HY, YH, HZ, ZD }

    /// <summary>
    /// 平曲线要素基类
    /// </summary>
    public abstract class PQXElement
    {
        public PQXElement()
        {
            TypeID = 0;
            StartPoint = new Point2D();
            StartAngle = Angle.FromDegrees(0);
            LeftRight = LeftRightEnum.None;
        }
        public PQXElement(EITypeID idd, Point2D stpoint, Angle stangle, LeftRightEnum lr)
        {
            TypeID = idd;
            StartPoint = stpoint;
            StartAngle = stangle;
            LeftRight = lr;
        }

        #region 共有属性
        public EITypeID TypeID { set; get; }
        public Point2D StartPoint { set; get; }
        public Angle StartAngle { set; get; }
        public LeftRightEnum LeftRight { set; get; }
        // 抽象继承
        public Angle EndAngle { get { return UpdateEndAngle(); } }
        public Point2D EndPoint { get { return UpdateEndPoint(); } }
        public abstract double Length { get; }

        #endregion


        public abstract Point2D GetPointOnCurve(double l);

        Point2D UpdateEndPoint()
        {
            return GetPointOnCurve(Length);
        }
        protected abstract Angle UpdateEndAngle();



    }
}
