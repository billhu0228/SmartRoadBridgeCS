using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartRoadBridge.Alignment
{
    public struct CGData
    {
        public double PK, LeftSlop, RightSlop;
    }
    public class CG
    {
        List<CGData> CGDataList;
        int TypeID;

        //超高旋转轴至左右边线距离
        double dtl, dtr;

        double isSlop;

        //超高过渡方式标志。jd1=1，为线性过渡方式；jd1=3，则为三次抛物线过渡方式
        int jd1;

        //以变宽的中央分隔带边缘作为超高旋转轴，则axi值可输入为+0
        double axi;

        int jd2;



        public CG(string[] altext)
        {
            CGDataList = new List<CGData>();
            bool readfrist = false;
            foreach (string item in altext)
            {
                if (item.StartsWith("//") || item == "")
                {
                    continue;
                }

                string line = item.TrimEnd('\r');
                line = line.TrimEnd('\t');
                var xx = (Regex.Split(line, @"\s+")).ToList();
                xx.Remove("");

                if (!readfrist)
                {
                    TypeID = int.Parse(xx[0]);
                    dtl = double.Parse(xx[1]);
                    dtr = double.Parse(xx[2]);
                    isSlop = double.Parse(xx[3]);
                    jd1 = int.Parse(xx[4]);
                    axi = double.Parse(xx[5]);
                    jd2 = int.Parse(xx[6]);
                    readfrist = true;

                    if (axi != 0 || jd2 != 2)
                    {
                        throw new Exception("超高数据类型超过已知");
                    }
                }
                else
                {
                    CGData pt = new CGData();
                    try
                    {
                        if (xx.Count == 3)
                        {
                            pt.PK = double.Parse(xx[0]);
                            pt.LeftSlop = double.Parse(xx[1]);
                            pt.RightSlop = double.Parse(xx[2]);
                        }
                        else
                        {
                            pt.PK = double.Parse(xx[0]);
                            pt.LeftSlop = double.Parse(xx[1]);
                            pt.RightSlop = double.Parse(xx[1]);
                        }

                        CGDataList.Add(pt);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            CGDataList.Sort((x, y) => x.PK.CompareTo(y.PK));
        }

        /// <summary>
        /// 超高
        /// </summary>        
        /// <param name="inputs">输入资源</param>
        public CG(string cgfile)
        {
            string[] altext = File.ReadAllLines(cgfile);
            CGDataList = new List<CGData>();
            bool readfrist = false;
            foreach (string item in altext)
            {
                if (item.StartsWith("//"))
                {
                    continue;
                }

                string line = item.TrimEnd('\r');
                line = line.TrimEnd('\t');
                var xx = (Regex.Split(line, @"\s+")).ToList();
                xx.Remove("");

                if (!readfrist)
                {
                    TypeID = int.Parse(xx[0]);
                    dtl = double.Parse(xx[1]);
                    dtr = double.Parse(xx[2]);
                    isSlop = double.Parse(xx[3]);
                    jd1 = int.Parse(xx[4]);
                    axi = double.Parse(xx[5]);
                    jd2 = int.Parse(xx[6]);
                    readfrist = true;

                    if (axi != 0 || jd2 != 2)
                    {
                        throw new Exception("超高数据类型超过已知");
                    }
                }
                else
                {
                    CGData pt = new CGData();
                    try
                    {
                        if (xx.Count == 3)
                        {
                            pt.PK = double.Parse(xx[0]);
                            pt.LeftSlop = double.Parse(xx[1]);
                            pt.RightSlop = double.Parse(xx[2]);
                        }
                        else
                        {
                            pt.PK = double.Parse(xx[0]);
                            pt.LeftSlop = double.Parse(xx[1]);
                            pt.RightSlop = double.Parse(xx[1]);
                        }

                        CGDataList.Add(pt);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            CGDataList.Sort((x, y) => x.PK.CompareTo(y.PK));

        }






        public double[] GetHP(double pk)
        {
            double[] res = new double[] { 0, 0 };

            if (Math.Abs(pk) < 1e-4)
            {
                pk = 0;
            }

            if (pk < CGDataList.First().PK || pk > CGDataList.Last().PK)
            {
                throw new ArgumentOutOfRangeException("里程不在设计范围内");
            }
            else if (CGDataList.Exists(x => x.PK == pk))
            {
                if (pk == CGDataList.First().PK)
                {
                    res = new double[] { CGDataList.First().LeftSlop, CGDataList.First().RightSlop };
                }
                else if (pk == CGDataList.Last().PK)
                {
                    res = res = new double[] { CGDataList.Last().LeftSlop, CGDataList.Last().RightSlop };
                }
                else
                {
                    double l = (GetHP(pk + 0.000001)[0] + GetHP(pk - 0.000001)[0]) * 0.5;
                    double r = (GetHP(pk + 0.000001)[1] + GetHP(pk - 0.000001)[1]) * 0.5;
                    res = new double[] { l, r };
                }
            }
            else
            {

                CGData[] tmp = CGDataList.ToArray();
                List<CGData> tmpBPDList = tmp.ToList();
                CGData pt = new CGData { PK = pk };
                tmpBPDList.Add(pt);
                tmpBPDList.Sort((x, y) => x.PK.CompareTo(y.PK));
                int kk = tmpBPDList.IndexOf(pt);
                double CCLeft = (tmpBPDList.ElementAt(kk + 1).LeftSlop - tmpBPDList.ElementAt(kk - 1).LeftSlop) /
                    (tmpBPDList.ElementAt(kk + 1).PK - tmpBPDList.ElementAt(kk - 1).PK);

                double CCRight = (tmpBPDList.ElementAt(kk + 1).RightSlop - tmpBPDList.ElementAt(kk - 1).RightSlop) /
                    (tmpBPDList.ElementAt(kk + 1).PK - tmpBPDList.ElementAt(kk - 1).PK);

                if (CCLeft * CCRight > 0 && CCLeft != CCRight)
                {
                    // 超高等待
                    double l = 0;
                    double r = 0;
                    if (Math.Abs(CCLeft) > Math.Abs(CCRight))//左插值，右等待
                    {
                        l = (pk - tmpBPDList.ElementAt(kk - 1).PK) * CCLeft + tmpBPDList.ElementAt(kk - 1).LeftSlop;
                        r = tmpBPDList.ElementAt(kk - 1).RightSlop;
                        if (CCRight > 0)
                        {
                            r = Math.Min(l, tmpBPDList.ElementAt(kk + 1).RightSlop);
                        }
                        else
                        {
                            r = Math.Min(l, tmpBPDList.ElementAt(kk - 1).RightSlop);
                        }
                    }
                    else//右差值，左等待
                    {
                        r = (pk - tmpBPDList.ElementAt(kk - 1).PK) * CCRight + tmpBPDList.ElementAt(kk - 1).RightSlop;
                        l = tmpBPDList.ElementAt(kk - 1).LeftSlop;
                        if (CCLeft > 0)//左不动
                        {
                            l = Math.Max(r, tmpBPDList.ElementAt(kk - 1).LeftSlop);
                        }
                        else//左先动
                        {
                            l = Math.Max(r, tmpBPDList.ElementAt(kk + 1).LeftSlop);
                        }

                    }

                    res = new double[] { l, r };
                }
                else
                {
                    // 分别插值

                    double l = (pk - CGDataList.ElementAt(kk - 1).PK) * CCLeft + CGDataList.ElementAt(kk - 1).LeftSlop;
                    double r = (pk - CGDataList.ElementAt(kk - 1).PK) * CCRight + CGDataList.ElementAt(kk - 1).RightSlop;

                    res = new double[] { l, r };
                }
            }


            return res;
        }

    }
}
