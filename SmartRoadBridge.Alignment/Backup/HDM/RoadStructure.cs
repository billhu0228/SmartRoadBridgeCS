using System;
using System.Collections.Generic;
using System.Xml;

namespace SmartRoadBridge.Alignment
{
    public class RoadStructure
    {
        string Name;



        internal List<Part> LeftParts;
        internal List<Part> RightParts;

        public RoadStructure(string name)
        {
            Name = name;
            LeftParts = new List<Part>();
            RightParts = new List<Part>();

        }

        internal void GenPartsFromNode(XmlNodeList xmlNodeList)
        {
            if (xmlNodeList.Count != 1)
            {
                throw new Exception();
            }
            var item = xmlNodeList[0];

            if (item.Name == "LeftPart")
            {
                LeftParts = ReadParts(item);
            }
            else if (item.Name == "RightPart")
            {
                RightParts = ReadParts(item);
            }




        }



        private List<Part> ReadParts(XmlNode item)
        {
            List<Part> res = new List<Part>();
            foreach (XmlElement Node in item.ChildNodes)
            {
                Part thePart = ReadPartFromNode(Node);
                res.Add(thePart);
            }
            return res;
        }

        private Part ReadPartFromNode(XmlElement node)
        {

            string name = node.Attributes.GetNamedItem("name").Value;
            int tp = int.Parse(node.Attributes.GetNamedItem("type").Value);
            string w = node.Attributes.GetNamedItem("w_express").Value;
            string h = node.Attributes.GetNamedItem("h_express").Value;
            string v = node.Attributes.GetNamedItem("v_transition").Value;
            TransitionEnum wt = (TransitionEnum)Enum.Parse(typeof(TransitionEnum), node.Attributes.GetNamedItem("w_transition").Value);
            TransitionEnum ht = (TransitionEnum)Enum.Parse(typeof(TransitionEnum), node.Attributes.GetNamedItem("h_transition").Value);
            TransitionEnum vt = (TransitionEnum)Enum.Parse(typeof(TransitionEnum), node.Attributes.GetNamedItem("v_transition").Value);
            //node.SelectSingleNode()

            //Part res = new Part(name,tp,w,h,wt,ht,vt);

            ;

            throw new NotImplementedException();
        }
    }

}
