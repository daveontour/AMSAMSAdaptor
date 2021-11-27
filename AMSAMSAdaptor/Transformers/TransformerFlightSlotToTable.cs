using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerStandSlotToTable : ITransformer
    {
       // private string Table { get; set; }
        public void SetConfig(XmlNode configNode)
        {
           // Table = configNode.SelectSingleNode(".//Property")?.Attributes["tableName"]?.Value;
        }

        public object Transform(object input)
        {
            ModelFlight fl = (ModelFlight)input;
            XmlDocument doc = fl.node.OwnerDocument;
            XmlElement tv = doc.CreateElement("TableValue");
            tv.SetAttribute("propertyName", "StandSlots");
            foreach (XmlNode s in fl.node.SelectNodes(".//amsx-datatypes:StandSlot", fl.nsmgr))
            {
                XmlElement row = doc.CreateElement("Row");

                string StartTime = s.SelectSingleNode("./amsx-datatypes:Value[@property='StartTime']", fl.nsmgr)?.InnerText;
                string EndTime = s.SelectSingleNode("./amsx-datatypes:Value[@property='EndTime']", fl.nsmgr)?.InnerText;
                string StandName = s.SelectSingleNode("./amsx-datatypes:Stand/amsx-datatypes:Value[@property='Name']", fl.nsmgr)?.InnerText;
                string StandExternalName = s.SelectSingleNode("./amsx-datatypes:Stand/amsx-datatypes:Value[@property='ExternalName']", fl.nsmgr)?.InnerText;
                string Area = s.SelectSingleNode("./amsx-datatypes:Area/amsx-datatypes:Value[@property='Name']", fl.nsmgr)?.InnerText;

                if (StartTime != null)
                {
                    XmlElement e = doc.CreateElement("Value");
                    e.SetAttribute("propertyName", "StartTime");
                    e.InnerText = StartTime;   
                    row.AppendChild(e);
                }
                if (EndTime != null)
                {
                    XmlElement e = doc.CreateElement("Value");
                    e.SetAttribute("propertyName", "EndTime");
                    e.InnerText = EndTime;
                    row.AppendChild(e);
                }
                if (StandName != null)
                {
                    XmlElement e = doc.CreateElement("Value");
                    e.SetAttribute("propertyName", "StandName");
                    e.InnerText = StandName;
                    row.AppendChild(e);
                }
                if (StandName != null)
                {
                    XmlElement e = doc.CreateElement("Value");
                    e.SetAttribute("propertyName", "StandName");
                    e.InnerText = StandName;
                    row.AppendChild(e);
                }
                if (StandExternalName != null)
                {
                    XmlElement e = doc.CreateElement("Value");
                    e.SetAttribute("propertyName", "StandExternalName");
                    e.InnerText = StandExternalName;
                    row.AppendChild(e);
                }
                if (Area != null)
                {
                    XmlElement e = doc.CreateElement("Value");
                    e.SetAttribute("propertyName", "Area");
                    e.InnerText = Area;
                    row.AppendChild(e);
                }
                tv.AppendChild(row);
            }


            return fl;
        }
    }
}
