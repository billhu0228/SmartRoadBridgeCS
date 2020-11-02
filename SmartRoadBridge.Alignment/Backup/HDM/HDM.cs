using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SmartRoadBridge.Alignment
{
    public class HDM
    {

        List<RoadStructure> RoadAssemblyList;

        public HDM(string XMLFilePath)
        {
            RoadAssemblyList = new List<RoadStructure>();

            FileInfo PathName = new FileInfo(XMLFilePath);


            XmlDocument doc = new XmlDocument();
            if (Directory.Exists(PathName.DirectoryName))
            {
                DirectoryInfo folder = new DirectoryInfo(PathName.DirectoryName);

                foreach (var item in folder.GetFiles("*.xml"))
                {
                    doc.Load(item.FullName);
                    if (doc.GetElementsByTagName("BaseDesign") == null)
                    {
                        ;
                    }
                    else
                    {
                        RoadStructure curRS = new RoadStructure(item.Name);

                        curRS.GenPartsFromNode(doc.GetElementsByTagName("LeftPart"));
                        curRS.GenPartsFromNode(doc.GetElementsByTagName("RightPart"));
                        ;
                    }







                }









            }

        }
    }
}
