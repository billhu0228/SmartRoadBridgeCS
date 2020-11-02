using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoadBridge.Alignment
{
    public enum HDXMethod { TGF = 1, ABS = 2, None = 0 };
    /// <summary>
    /// ABS法节点
    /// </summary>
    public struct HDXData
    {
        public double x;
        public double h;

        public HDXData(double x, double h)
        {
            this.x = x;
            this.h = h;
        }
    }

    public class HDXElement
    {
        public double PK;        
        public List<HDXData> LeftLine, RightLine;
        public HDXMethod Type;

        public HDXElement()
        {
            LeftLine = new List<HDXData>();
            RightLine = new List<HDXData>();
            Type = 0;
            PK = 0;            
        }

        internal HDXElement ConvertABS()
        {
            HDXElement ret = new HDXElement();
            if (Type!=HDXMethod.TGF)
            {
                return this;
            }
            else
            {
                List<HDXData> newLeftLine = new List<HDXData>();
                for (int i = 0; i < LeftLine.Count; i++)
                {
                    var range = LeftLine.GetRange(0, i + 1);

                    newLeftLine.Add(new HDXData((from a in range select a.x).ToList().Sum(), (from a in range select a.h).ToList().Sum()));
                }
                ret.LeftLine = newLeftLine;

                List<HDXData> newRightLine = new List<HDXData>();
                for (int i = 0; i < RightLine.Count; i++)
                {
                    var range = RightLine.GetRange(0, i + 1);

                    newRightLine.Add(new HDXData((from a in range select a.x).ToList().Sum(), (from a in range select a.h).ToList().Sum()));
                }
                ret.RightLine = newRightLine;
                ret.Type = HDXMethod.ABS;
                ret.PK = PK;
                return ret;
            }
        }

        internal double GetBG(double x)
        {
            HDXElement NewElem = this.ConvertABS();

            HDXData t1, t2;
            double absx = Math.Abs(x);
            List<double> xlist = new List<double>() ;
            List<HDXData> RcdList = new List<HDXData>();
            if (x==0)
            {
                return 0;
            }
            else if (x>0)
            {
                xlist= (from a in NewElem.RightLine select a.x).ToList();
                RcdList = NewElem.RightLine;
            }
            else
            {
                xlist = (from a in NewElem.LeftLine select a.x).ToList();
                RcdList = NewElem.LeftLine;
            }
            
            if (xlist.Contains(absx))
            {
                return RcdList[xlist.IndexOf(absx)].h;
            }
            else
            {
                xlist.Add(absx);
                xlist.Sort();
                int i0 = xlist.IndexOf(absx);
                if (i0==0)
                {
                    if (this.Type==HDXMethod.ABS)
                    {
                        double dd = LeftLine[0].x + RightLine[0].x;
                        double cl = RightLine[0].x / dd;
                        double cr = LeftLine[0].x / dd;
                        t1 = new HDXData(0,cl*LeftLine[0].h+cr*RightLine[0].h);
                    }
                    else
                    {
                        t1 = new HDXData(0, 0);
                    }
                    
                    t2 = RcdList[i0];
                }
                else if (i0==RcdList.Count())
                {
                    t1 = RcdList[i0 - 1];
                    t2 = RcdList[i0 - 1];
                }
                else
                {
                    t1 = RcdList[i0 - 1];
                    t2 = RcdList[i0];
                }
                
                double f1 = (t2.x - absx) / (t2.x - t1.x);
                double f2 = (absx-t1.x) / (t2.x - t1.x);
                return t1.h*f1 + t2.h *f2;
            }
        }
    }
}
