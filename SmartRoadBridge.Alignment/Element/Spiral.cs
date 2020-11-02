using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System;

namespace SmartRoadBridge.Alignment
{

    public class Spiral : PQXElement
    {

        /// <summary>
        /// 跟据IE参数构建缓和曲线原型
        /// </summary>
        /// <param name="a"></param>
        /// <param name="sr"></param>
        /// <param name="er"></param>
        /// <param name="dir"></param>
        public Spiral(EITypeID idd, double a, double sr, double er, Point2D st, Angle sdir, LeftRightEnum dir)
            : base(idd, st, sdir, dir)
        {
            A = a;
            StartR = sr;
            EndR = er;
        }


        #region 属性
        public double A { set; get; }
        public double StartR { set; get; }
        public double EndR { set; get; }
        public override double Length { get { return Math.Abs(A * A / StartR - A * A / EndR); } }
        #endregion




        #region 方法

        double AB(double value, int i, int j, int k, int l)
        {
            double rhoA = 1 / StartR;
            double rhoB = 1 / EndR;
            double ConstA = rhoA;
            double ConstB = (rhoB - rhoA) / (2 * Length);

            return Math.Pow(ConstA, i) * Math.Pow(ConstB, j) / (double)k * Math.Pow(value, l);
        }
        public override Point2D GetPointOnCurve(double l)
        {
            double x = l - AB(l, 2, 0, 6, 3) - AB(l, 1, 1, 4, 4) - AB(l, 0, 2, 10, 5) + AB(l, 4, 0, 120, 5)
                + AB(l, 3, 1, 36, 6) + AB(l, 2, 2, 28, 7) + AB(l, 1, 3, 48, 8) + AB(l, 0, 4, 216, 9);

            double y = AB(l, 1, 0, 2, 2) + AB(l, 0, 1, 3, 3) - AB(l, 3, 0, 24, 4) - AB(l, 2, 1, 10, 5)
                        - AB(l, 1, 2, 12, 6) + AB(l, 5, 0, 720, 6) - AB(l, 0, 3, 42, 7) + AB(l, 4, 1, 168, 7)
                        + AB(l, 3, 2, 96, 8) + AB(l, 2, 3, 108, 9);

            y *= (int)LeftRight;

            Vector2D res = new Vector2D(y, x).Rotate(-StartAngle);


            return StartPoint + res;
        }
        protected override Angle UpdateEndAngle()
        {
            return StartAngle + (int)LeftRight * Angle.FromRadians(0.5 * A * A * Math.Abs(1.0 / (StartR * StartR) - 1.0 / (EndR * EndR)));

        }
        #endregion
    }
}
