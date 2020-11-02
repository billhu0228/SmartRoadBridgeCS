using CsvHelper.Configuration;
using System.Collections.Generic;

namespace SmartRoadBridge.Database
{
    public class CapINFO
    {        
        public string Name { get; set; }
        public string H0 { get; set; }
        public string Slope { get; set; }

    }


    public sealed class CapINFOMap : ClassMap<CapINFO>
    {
        public CapINFOMap()
        {            
            Map(m => m.Name).Index(0);
            Map(m => m.H0).Index(1).Default("");
            Map(m => m.Slope).Index(2).Default("");
        }
    }

}
