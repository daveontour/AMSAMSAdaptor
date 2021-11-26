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
        private string Table { get; set; }
        public void SetConfig(XmlNode configNode)
        {
            Table = configNode.SelectSingleNode(".//Property")?.Attributes["table"]?.Value;
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

                tv.AppendChild(row);
            }


            return fl;
        }
    }
}
