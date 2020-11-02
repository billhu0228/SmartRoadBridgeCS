using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartRoadBridge.Alignment
{
    /// <summary>
    /// ICD节点
    /// </summary>
    public class ICD
    {

        /// <summary>
        /// 默认构造
        /// </summary>
        public ICD()
        {
            TypeID = 0;
            A = double.NaN;
            R = double.NaN;
            StartR = double.NaN;
            EndR = double.NaN;
            Length = double.NaN;
            DirEnum = 0;
            AngInDeg = double.NaN;
        }
        /// <summary>
        /// ICD文件记录
        /// </summary>
        /// <param name="RcdLine"></param>
        public ICD(string RcdLine)
        {
            var xx = Regex.Split(RcdLine, ",");
            try
            {
                TypeID = int.Parse(xx[0]);
            }
            catch
            {
                throw new Exception();
            }

            if (TypeID == 1)
            {
                // 直线，长度，方位角
                Length = double.Parse(xx[1]);
                if (xx.Count() == 3)
                {
                    AngInDeg = double.Parse(xx[2]);
                }
            }
            else if (TypeID == 2)
            {
                R = double.Parse(xx[1]);
                Length = double.Parse(xx[2]);
                DirEnum = int.Parse(xx[3]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;
            }
            else if (TypeID == 3)
            {
                A = double.Parse(xx[1]);
                EndR = double.Parse(xx[2]);
                DirEnum = int.Parse(xx[3]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;
                Length = A * A / EndR;
            }
            else if (TypeID == 4)
            {
                A = double.Parse(xx[1]);
                StartR = double.Parse(xx[2]);
                DirEnum = int.Parse(xx[3]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;
                Length = A * A / StartR;
            }
            else if (TypeID == 5)
            {
                A = double.Parse(xx[1]);
                StartR = double.Parse(xx[2]);
                EndR = double.Parse(xx[3]);
                DirEnum = int.Parse(xx[4]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;
                Length = A * A / EndR - A * A / StartR;
            }
            else if (TypeID == 6)
            {
                A = double.Parse(xx[1]);
                StartR = double.Parse(xx[2]);
                EndR = double.Parse(xx[3]);
                DirEnum = int.Parse(xx[4]) == -1 ? LeftRightEnum.Left : LeftRightEnum.Right;
                Length = A * A / StartR - A * A / EndR;
            }
            else
            {
                return;
            }
            isClookWise = (DirEnum == LeftRightEnum.Right);
        }


        #region 属性
        public int TypeID;
        public double A, R, StartR, EndR;
        public double Length, AngInDeg;
        public LeftRightEnum DirEnum;
        public bool isClookWise;
        #endregion

    }
}
