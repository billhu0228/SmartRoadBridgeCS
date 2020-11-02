using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace SmartRoadBridge.Database
{
    public class BridgeINFO
    {
        //public string Id { get; set; }
        public string Line { get; set; }
        public string Bridge { get; set; }
        public double PK { get; set; }
        public double Span { get; set; }
        public double Angle { set; get; }
        public double Width { set; get; }
        public double Offset { set; get; }
        public string SubType { set; get; }
        public string SupType { set; get; }
        public string FundType { set; get; }
    }

    public sealed class BridgeINFOMap : ClassMap<BridgeINFO>
    {
        public BridgeINFOMap()
        {
            //Map(m => m.Id).Index(0);
            Map(m => m.Line).Index(1);
            Map(m => m.Bridge).Index(2);
            Map(m => m.PK).Index(3);
            Map(m => m.Span).Index(4);
            Map(m => m.Angle).Index(5);
            Map(m => m.Width).Index(6);
            Map(m => m.Offset).Index(7);
            Map(m => m.SubType).Index(8);
            Map(m => m.SupType).Index(9);
            Map(m => m.FundType).Index(10);            
        }
    }

    public class PKConverter<T> : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            string pattern = @"\d+\.?\d*";
            var mt = (from Match m in  Regex.Matches(text, pattern) select m.Value).ToList();
            
            if (mt.Count == 2) { return int.Parse(mt[0]) * 1000 + double.Parse(mt[1]); }
            else { return -1; }                
        }
    }

    public class DesConverter<T> : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            string res = "";
            var tt=text.Split('+');
            foreach (var toPlus in tt)
            {
                var toX=Regex.Split(toPlus, "[x|×]");
                if (toX.Count()==1)
                {
                    res += GetNUMFromString(toX[0]);
                    res += ",";
                }
                else if(toX.Count()==2)
                {
                    for (int k = 0; k < int.Parse(toX[0]); k++)
                    {
                        res += GetNUMFromString(toX[1]);
                        res += ",";
                    }
                }
                else
                {
                    throw new Exception("#  分割数量不正确.");
                }
            }
            int l = res.Length;
            return res.Remove(l-1,1);
        }

        private string GetNUMFromString(string v)
        {
            string pattern = @"\d+\.?\d*";
            var mt = (from Match m in Regex.Matches(v, pattern) select m.Value).ToList();
            return mt[0];
        }
    }
}
