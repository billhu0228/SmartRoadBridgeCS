using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartRoadBridge.Alignment
{
    public struct BPD
    {
        public double PK, H, R;
    }


    public class SQX
    {
        //public string Name
        //{
        //    get;
        //}
        public List<BPD> BPDList;


        public SQX(string[] altext)
        {
            BPDList = new List<BPD>();

            foreach (string item in altext)
            {
                if (item.StartsWith("//") || item == "")
                {
                    continue;
                }
                BPD pt = new BPD();
                try
                {
                    string line = item.TrimEnd('\r');
                    line = line.TrimEnd('\t');
                    var xx = Regex.Split(line, @"\s+");
                    pt.PK = double.Parse(xx[0]);
                    pt.H = double.Parse(xx[1]);
                    if (xx.Length == 3)
                    {
                        pt.R = double.Parse(xx[2]);
                    }
                    else
                    {
                        pt.R = -1;
                    }
                    BPDList.Add(pt);
                }
                catch (Exception)
                {
                    throw;
                }

            }
            BPDList.Sort((x, y) => x.PK.CompareTo(y.PK));
        }


        /// <summary>
        /// 竖曲线
        /// </summary>        
        /// <param name="inputs">输入资源</param>
        public SQX(string sqxfile)
        {
            string[] altext = File.ReadAllLines(sqxfile);
            BPDList = new List<BPD>();

            foreach (string item in altext)
            {
                if (item.StartsWith("//") || item == "")
                {
                    continue;
                }
                BPD pt = new BPD();
                try
                {
                    string line = item.TrimEnd('\r');
                    line = line.TrimEnd('\t');
                    var xx = Regex.Split(line, @"\s+");
                    pt.PK = double.Parse(xx[0]);
                    pt.H = double.Parse(xx[1]);
                    if (xx.Length == 3)
                    {
                        pt.R = double.Parse(xx[2]);
                    }
                    else
                    {
                        pt.R = -1;
                    }
                    BPDList.Add(pt);
                }
                catch (Exception)
                {
                    throw;
                }

            }
            BPDList.Sort((x, y) => x.PK.CompareTo(y.PK));

        }

        void GetAB(int k, out double begin, out double end, out int direct)
        {
            double i1, i2, L, T, w;
            BPD curBPD = BPDList.ElementAt(k);
            if (k == 0)
            {
                i2 = (BPDList.ElementAt(k + 1).H - curBPD.H) / (BPDList.ElementAt(k + 1).PK - curBPD.PK);
                i1 = -i2;
            }
            else if (k == BPDList.Count - 1)
            {
                i1 = (curBPD.H - BPDList.ElementAt(k - 1).H) / (curBPD.PK - BPDList.ElementAt(k - 1).PK);
                i2 = -i1;
            }
            else
            {
                i1 = (curBPD.H - BPDList.ElementAt(k - 1).H) / (curBPD.PK - BPDList.ElementAt(k - 1).PK);
                i2 = (BPDList.ElementAt(k + 1).H - curBPD.H) / (BPDList.ElementAt(k + 1).PK - curBPD.PK);
            }
            w = i2 - i1;
            L = curBPD.R * Math.Abs(w);
            T = curBPD.R * Math.Abs(w) * 0.5;
            begin = curBPD.PK - T;
            end = curBPD.PK + T;
            direct = w < 0 ? -1 : 1;

        }

        public double GetBG(double pk)
        {
            if (Math.Abs(pk) < 1e-4)
            {
                pk = 0;
            }
            double res = 0;
            if (pk < BPDList.First().PK || pk > BPDList.Last().PK)
            {
                //if (Math.Abs(pk- BPDList.First().PK)<0.001)
                //{
                //    return GetBG(BPDList.First().PK);
                //}
                throw new ArgumentOutOfRangeException("里程不在设计范围内");
            }
            else if (BPDList.Exists(x => x.PK == pk))
            {
                if (pk == BPDList.First().PK)
                {
                    res = BPDList.First().H;
                }
                else if (pk == BPDList.Last().PK)
                {
                    res = BPDList.Last().H;
                }
                else
                {
                    res = (GetBG(pk + 0.000001) + GetBG(pk - 0.000001)) * 0.5;
                }
            }
            else
            {

                BPD[] tmp = BPDList.ToArray();
                List<BPD> tmpBPDList = tmp.ToList();
                BPD pt = new BPD { PK = pk };
                tmpBPDList.Add(pt);
                tmpBPDList.Sort((x, y) => x.PK.CompareTo(y.PK));
                int kk = tmpBPDList.IndexOf(pt);
                double CC = (BPDList.ElementAt(kk).H - BPDList.ElementAt(kk - 1).H) / (BPDList.ElementAt(kk).PK - BPDList.ElementAt(kk - 1).PK);
                double y0 = (pk - BPDList.ElementAt(kk - 1).PK) * CC + BPDList.ElementAt(kk - 1).H;
                double dy = 0;

                GetAB(kk - 1, out double beginA, out double endA, out int dirA);
                GetAB(kk, out double beginB, out double endB, out int dirB);
                if (pk <= endA)
                {
                    dy = (endA - pk) * (endA - pk) / BPDList.ElementAt(kk - 1).R * 0.5 * dirA;
                }
                else if (pk >= beginB)
                {
                    dy = (pk - beginB) * (pk - beginB) / BPDList.ElementAt(kk).R * 0.5 * dirB;
                }
                else
                {
                    dy = 0;
                }
                res = y0 + dy;
            }

            return res;
        }

        public double GetZP(double curPK)
        {
            double zp1 = (GetBG(curPK) - GetBG(curPK - 0.001)) / 0.001 * 100.0;
            double zp2 = (GetBG(curPK + 0.001) - GetBG(curPK)) / 0.001 * 100.0;
            return (zp1 + zp2) * 0.5;
        }
    }
}
