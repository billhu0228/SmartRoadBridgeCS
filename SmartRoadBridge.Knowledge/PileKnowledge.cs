using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoadBridge.Knowledge
{
    public static class PileKnowledge
    {
        public static double GetDia(double PK)
        {
            return 1000;
        }
        public static double GetLength(double PK)
        {
            return 30000;
        }

        public static void GetArr(double curPK,out int RowNum, out int ColNum,out double RowSpace,out double ColSpace)
        {
            RowNum = 1;
            RowSpace = 0;
            ColNum = 2;
            ColSpace = 8;
        }
    }
}
