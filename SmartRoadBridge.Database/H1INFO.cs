using CsvHelper.Configuration;
using System.Collections.Generic;

namespace SmartRoadBridge.Database
{
    public class H1INFO
    {        
        public string Name { get; set; }        
        public List<double> H1List { set; get; }
    }


    public sealed class H1INFOMap : ClassMap<H1INFO>
    {
        public H1INFOMap()
        {            
            Map(m => m.Name).Index(0);            
            Map(m => m.H1List).Index(1).TypeConverter<DoubleListConverter<string>>();
        }
    }


}
