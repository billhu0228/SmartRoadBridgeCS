using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartRoadBridge.Alignment
{
    
    public class HDX
    {
        List<HDXElement> HDXDataList;
        public HDXMethod Method;

        public HDX(string[] hdxtext)
        {
            string[] altext = hdxtext;
            HDXDataList = new List<HDXElement>();
            Method = 0;
            if (altext.Count() <=1)
            {
                return;
            }

            int i = 0;
            int n = 0;
            HDXElement curData = new HDXElement();
            foreach (string item in altext)
            {

                if (item.StartsWith("//")||item=="")
                {
                    if (i != 0)
                    {
                        i++;
                        continue;
                    }
                    if (item.Contains("ABS"))
                    {
                        Method = HDXMethod.ABS;
                        curData.Type = HDXMethod.ABS;
                    }
                    else if (item.Contains("抬杆法"))
                    {
                        Method = HDXMethod.TGF;
                        curData.Type = HDXMethod.TGF;
                    }
                    else
                    {
                        throw new Exception("地面线数据未说明类型");
                    }
                    i++;
                    continue;
                }
                string line = item.TrimEnd('\r');
                line = line.TrimEnd('\t');
                if (line == "ABS")
                {
                    i++;
                    continue;
                }
                var xx = (Regex.Split(line, @"\s+")).ToList();
                xx.Remove("");
                if (n == 0)
                {
                    curData = new HDXElement();
                    curData.PK = double.Parse(xx[0]);
                    curData.Type = Method;
                    n = 1;
                }
                else if (n == 1)
                {
                    var dataline = (from a in xx select double.Parse(a)).ToList();
                    HDXData dt = new HDXData();
                    for (int ii = 0; ii < dataline.Count; ii++)
                    {
                        if (ii % 2 == 0)
                        {
                            dt = new HDXData();
                            dt.x = dataline[ii];
                        }
                        else
                        {
                            dt.h = dataline[ii];
                            curData.LeftLine.Add(dt);
                        }
                    }
                    n = 2;
                }
                else if (n == 2)
                {
                    var dataline = (from a in xx select double.Parse(a)).ToList();
                    HDXData dt = new HDXData();
                    for (int ii = 0; ii < dataline.Count; ii++)
                    {
                        if (ii % 2 == 0)
                        {
                            dt = new HDXData();
                            dt.x = dataline[ii];
                        }
                        else
                        {
                            dt.h = dataline[ii];
                            curData.RightLine.Add(dt);
                        }
                    }
                    ;
                    HDXDataList.Add(curData);
                    n = 0;
                }




            }
            ;
        }

        public HDX(string hdxfile)
        {
            string[] altext = File.ReadAllLines(hdxfile,Encoding.Default);            
            HDXDataList = new List<HDXElement>();
            Method = 0;
            if (altext.Count()==0)
            {
                return;
            }

            int i = 0;
            int n = 0;
            HDXElement curData = new HDXElement();
            foreach (string item in altext)
            {
                
                if (item.StartsWith("//"))
                {
                    if (i != 0)
                    {
                        i++;
                        continue;
                    }
                    if (item.Contains("ABS"))
                    {
                        Method = HDXMethod.ABS;
                        curData.Type = HDXMethod.ABS;
                    }
                    else if (item.Contains("抬杆法"))
                    {
                        Method = HDXMethod.TGF;
                        curData.Type = HDXMethod.TGF;
                    }
                    else
                    {
                        throw new Exception("地面线数据未说明类型");
                    }
                    i++;
                    continue;
                }
                string line = item.TrimEnd('\r');
                line = line.TrimEnd('\t');
                if (line=="ABS")
                {
                    i++;
                    continue;
                }
                var xx = (Regex.Split(line, @"\s+")).ToList();
                xx.Remove("");
                if (n == 0)
                {
                    curData = new HDXElement();
                    curData.PK = double.Parse(xx[0]);
                    curData.Type = Method;                    
                    n = 1;
                }
                else if (n == 1)
                {
                    var dataline = (from a in xx select double.Parse(a)).ToList();
                    HDXData dt = new HDXData();
                    for (int ii = 0; ii < dataline.Count; ii++)
                    {
                        if (ii % 2 == 0)
                        {
                            dt = new HDXData();
                            dt.x = dataline[ii];
                        }
                        else
                        {
                            dt.h = dataline[ii];
                            curData.LeftLine.Add(dt);
                        }
                    }
                    n = 2;
                }
                else if (n == 2)
                {
                    var dataline = (from a in xx select double.Parse(a)).ToList();
                    HDXData dt = new HDXData();
                    for (int ii = 0; ii < dataline.Count; ii++)
                    {
                        if (ii % 2 == 0)
                        {
                            dt = new HDXData();
                            dt.x = dataline[ii];
                        }
                        else
                        {
                            dt.h = dataline[ii];
                            curData.RightLine.Add(dt);
                        }
                    }
                    ;
                    HDXDataList.Add(curData);
                    n =0;
                }




            }
            ;
        }

        /// <summary>
        /// 横向地面线标高差
        /// </summary>
        /// <param name="pk">里程</param>
        /// <param name="x">斜距，向右为正</param>
        internal double GetdBG(double pk,double x)
        {
            HDXElement t1, t2;
            var f = from a in HDXDataList select a.PK;
            var xlist = f.ToList();
            if (xlist.Contains(pk))
            {
                t1 = HDXDataList[xlist.IndexOf(pk)];
                t2 = HDXDataList[xlist.IndexOf(pk)];
            }
            else
            {

                xlist.Add(pk);
                xlist.Sort();
                int i0 = xlist.IndexOf(pk);
                if (i0==HDXDataList.Count)
                {
                    t1 = HDXDataList[i0 - 1];
                    t2 = HDXDataList[i0 - 1];
                }
                else if (i0==0)
                {
                    t1 = HDXDataList[i0];
                    t2 = HDXDataList[i0];
                }
                else
                {
                    t1 = HDXDataList[i0 - 1];
                    t2 = HDXDataList[i0];
                }
                
            }

            t1 = t1.ConvertABS();
            t2 = t2.ConvertABS();
            double f1 = (t2.PK - pk) / (t2.PK - t1.PK);
            double f2 = ( pk- t1.PK) / (t2.PK - t1.PK);
            if (t2.PK-t1.PK==0)
            {
                f1 = 0.5;
                f2 = 0.5;
            }

            double dh= f1 * t1.GetBG(x) + f2 * t2.GetBG(x);
            return dh;
            ;

        }
    }
}
