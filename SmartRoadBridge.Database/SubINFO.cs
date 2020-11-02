using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartRoadBridge.Database
{

    public class SubINFO
    {
        public int ID { get; set; }
        public string Line { get; set; }
        public string Bridge { get; set; }
        public double Station { get; set; }
        public string Type { get; set; }
        public double Angle { set; get; }
        public double LeftWidth { set; get; }
        public double RightWidth { set; get; }
        public List<double> SpaceList { set; get; }
        public List<double> PierAngleList { set; get; }
        public List<double> FundAngleList { set; get; }
    }


    public sealed class SubINFOMap : ClassMap<SubINFO>
    {
        public SubINFOMap()
        {
            Map(m => m.ID).Index(0).Default(0);
            Map(m => m.Line).Index(1);
            Map(m => m.Bridge).Index(2);
            Map(m => m.Station).Index(3).Default(0.0);
            Map(m => m.Type).Index(4);
            Map(m => m.Angle).Index(5).Default(90.0);
            Map(m => m.LeftWidth).Index(6).Default(0.0);
            Map(m => m.RightWidth).Index(7).Default(0.0);
            Map(m => m.SpaceList).Index(8).TypeConverter<DoubleListConverter<string>>(); 
            Map(m => m.PierAngleList).Index(9).TypeConverter<DoubleListConverter<string>>();
            Map(m => m.FundAngleList).Index(10).TypeConverter<DoubleListConverter<string>>();
        }
    }

    public class DoubleListConverter<T> : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            var tmp = (from a in text.Split('/') select double.Parse(a)).ToList();
            return tmp;
        }
    }

}
