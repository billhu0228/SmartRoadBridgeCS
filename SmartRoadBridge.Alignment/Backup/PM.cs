using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartRoadBridge.Alignment
{


    public class PM
    {

        /// <summary>
        /// 使用ICD文件初始化平面线形.
        /// </summary>
        /// <param name="icdfile">icd文件</param>
        public PM(string icdfile)
        {
            // 初始化节点
            ICDList = new List<ICD>();
            string[] altext = File.ReadAllLines(icdfile);
            for (int i = 0; i < altext.Count(); i++)
            {
                string line = altext[i];
                if (i==0)
                {
                    StartPK = double.Parse(line);
                }
                else if(i==1)
                {
                    var xx = Regex.Split(line, ",");
                    StartX = double.Parse(xx[0]);
                    StartY = double.Parse(xx[1]);
                    StartAngInDeg = double.Parse(xx[2]);
                }
                else
                {
                    var xx = Regex.Split(line, ",");
                    if (line.StartsWith("//"))
                    {
                        // 注释
                        continue;
                    }
                    else if (xx.Count() == 3 && int.Parse(xx[2]) == 0)
                    {
                        // 结束
                        break;
                    }
                    else
                    {
                        ICD aa = new ICD(line);
                        ICDList.Add(aa);
                    }
                }
            } 
        }


        #region 属性
        public double StartPK;
        public double StartX, StartY;
        public double StartAngInDeg;
        public List<ICD> ICDList;
        #endregion



        #region 方法
        void test()
        {
            
        }
        #endregion





    }
}
