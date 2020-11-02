using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartRoadBridge.Alignment
{
    public struct RCD
    {
        public double PK, H;
    }

    public class DMX
    {
        public List<RCD> RcdList;

        public DMX(string[] altext)
        {
            RcdList = new List<RCD>();
            foreach (string item in altext)
            {
                if (item.StartsWith("//") || item == "")
                {
                    continue;
                }
                RCD pt = new RCD();
                try
                {
                    string line = item.TrimEnd('\r');
                    line = line.TrimEnd('\t');
                    var xx = Regex.Split(line, @"\s+");

                    pt.PK = double.Parse(xx[0]);

                    pt.H = double.Parse(xx[1]);
                    RcdList.Add(pt);
                }
                catch (Exception)
                {
                    throw;
                }

            }
            RcdList.Sort((x, y) => x.PK.CompareTo(y.PK));

        }


        public DMX(string dmxfile)
        {
            RcdList = new List<RCD>();
            string[] altext = File.ReadAllLines(dmxfile);

            foreach (string item in altext)
            {
                if (item.StartsWith("//") || item == "")
                {
                    continue;
                }
                RCD pt = new RCD();
                try
                {
                    string line = item.TrimEnd('\r');
                    line = line.TrimEnd('\t');
                    var xx = Regex.Split(line, @"\s+");

                    pt.PK = double.Parse(xx[0]);

                    pt.H = double.Parse(xx[1]);
                    RcdList.Add(pt);
                }
                catch (Exception)
                {
                    throw;
                }

            }
            RcdList.Sort((x, y) => x.PK.CompareTo(y.PK));
        }



        public double GetBG(double x0)
        {
            RCD t1, t2;
            var f = from a in RcdList select a.PK;
            var xlist = f.ToList();
            if (xlist.Contains(x0))
            {
                return RcdList[xlist.IndexOf(x0)].H;
            }
            else
            {
                xlist.Add(x0);
                xlist.Sort();
                int i0 = xlist.IndexOf(x0);
                t1 = RcdList[i0 - 1];
                t2 = RcdList[i0];

                return t1.H + (t2.H - t1.H) / (t2.PK - t1.PK) * (x0 - t1.PK);
            }
        }



    }
}
