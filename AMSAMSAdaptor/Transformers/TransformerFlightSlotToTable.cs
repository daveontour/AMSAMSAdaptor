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
        public void SetConfig(XmlNode configNode)
        {
        }

        public object Transform(object input)
        {
            ModelFlight fl = (ModelFlight)input;
            XmlDocument doc = fl.node.OwnerDocument;

            XmlNode stateNode = doc.SelectSingleNode(".//amsx-messages:Flight/amsx-datatypes:FlightState", fl.nsmgr);
            XmlElement tv = doc.CreateElement(null, "TableValue", "http://www.sita.aero/ams6-xml-api-datatypes");
            tv.SetAttribute("propertyName", "StandSlots"); 
            stateNode.AppendChild(tv);

            foreach (XmlNode s in fl.node.SelectNodes(".//amsx-messages:Flight/amsx-datatypes:FlightState/amsx-datatypes:StandSlots/amsx-datatypes:StandSlot", fl.nsmgr))
            {
                XmlElement row = doc.CreateElement(null, "Row", "http://www.sita.aero/ams6-xml-api-datatypes");

                string StartTime = s.SelectSingleNode("./amsx-datatypes:Value[@propertyName = 'StartTime']", fl.nsmgr)?.InnerText;
                string EndTime = s.SelectSingleNode("./amsx-datatypes:Value[@propertyName = 'EndTime']", fl.nsmgr)?.InnerText;
                string StandName = s.SelectSingleNode("./amsx-datatypes:Stand/amsx-datatypes:Value[@propertyName = 'Name']", fl.nsmgr)?.InnerText;
                string StandExternalName = s.SelectSingleNode("./amsx-datatypes:Stand/amsx-datatypes:Value[@propertyName = 'ExternalName']", fl.nsmgr)?.InnerText;
                string Area = s.SelectSingleNode("./amsx-datatypes:Stand/amsx-datatypes:Area/amsx-datatypes:Value[@propertyName = 'Name']", fl.nsmgr)?.InnerText;

                if (StartTime != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "StartTime");
                    e.InnerText = StartTime;   
                    row.AppendChild(e);
                }
                if (EndTime != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "EndTime");
                    e.InnerText = EndTime;
                    row.AppendChild(e);
                }
                if (StandName != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "StandName");
                    e.InnerText = StandName;
                    row.AppendChild(e);
                }
                if (StandExternalName != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "StandExternalName");
                    e.InnerText = StandExternalName;
                    row.AppendChild(e);
                }
                if (Area != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "Area");
                    e.InnerText = Area;
                    row.AppendChild(e);
                }
                tv.AppendChild(row);
            }


            return fl;
        }
    }

    internal class TransformerCheckInSlotToTable : ITransformer
    {
        public void SetConfig(XmlNode configNode)
        {
        }

        public object Transform(object input)
        {
            ModelFlight fl = (ModelFlight)input;
            XmlDocument doc = fl.node.OwnerDocument;

            XmlNode stateNode = doc.SelectSingleNode(".//amsx-messages:Flight/amsx-datatypes:FlightState", fl.nsmgr);
            XmlElement tv = doc.CreateElement(null, "TableValue", "http://www.sita.aero/ams6-xml-api-datatypes");
            tv.SetAttribute("propertyName", "CheckInSlots");
            stateNode.AppendChild(tv);

            foreach (XmlNode s in fl.node.SelectNodes(".//amsx-messages:Flight/amsx-datatypes:FlightState/amsx-datatypes:CheckInSlots/amsx-datatypes:CheckInSlot", fl.nsmgr))
            {
                XmlElement row = doc.CreateElement(null, "Row", "http://www.sita.aero/ams6-xml-api-datatypes");

                string StartTime = s.SelectSingleNode("./amsx-datatypes:Value[@propertyName = 'StartTime']", fl.nsmgr)?.InnerText;
                string EndTime = s.SelectSingleNode("./amsx-datatypes:Value[@propertyName = 'EndTime']", fl.nsmgr)?.InnerText;
                string CheckInName = s.SelectSingleNode("./amsx-datatypes:CheckIn/amsx-datatypes:Value[@propertyName = 'Name']", fl.nsmgr)?.InnerText;
                string CheckInExternalName = s.SelectSingleNode("./amsx-datatypes:CheckIn/amsx-datatypes:Value[@propertyName = 'ExternalName']", fl.nsmgr)?.InnerText;
                string Area = s.SelectSingleNode("./amsx-datatypes:CheckIn/amsx-datatypes:Area/amsx-datatypes:Value[@propertyName = 'Name']", fl.nsmgr)?.InnerText;

                if (StartTime != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "StartTime");
                    e.InnerText = StartTime;
                    row.AppendChild(e);
                }
                if (EndTime != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "EndTime");
                    e.InnerText = EndTime;
                    row.AppendChild(e);
                }
                if (CheckInName != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "CheckInName");
                    e.InnerText = CheckInName;
                    row.AppendChild(e);
                }
                if (CheckInExternalName != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "CheckInExternalName");
                    e.InnerText = CheckInExternalName;
                    row.AppendChild(e);
                }
                if (Area != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "Area");
                    e.InnerText = Area;
                    row.AppendChild(e);
                }
                tv.AppendChild(row);
            }


            return fl;
        }
    }


    internal class TransformerGateSlotToTable : ITransformer
    {
        public void SetConfig(XmlNode configNode)
        {
        }

        public object Transform(object input)
        {
            ModelFlight fl = (ModelFlight)input;
            XmlDocument doc = fl.node.OwnerDocument;

            XmlNode stateNode = doc.SelectSingleNode(".//amsx-messages:Flight/amsx-datatypes:FlightState", fl.nsmgr);
            XmlElement tv = doc.CreateElement(null, "TableValue", "http://www.sita.aero/ams6-xml-api-datatypes");
            tv.SetAttribute("propertyName", "GateSlots");
            stateNode.AppendChild(tv);

            foreach (XmlNode s in fl.node.SelectNodes(".//amsx-messages:Flight/amsx-datatypes:FlightState/amsx-datatypes:GateSlots/amsx-datatypes:GateSlot", fl.nsmgr))
            {
                XmlElement row = doc.CreateElement(null, "Row", "http://www.sita.aero/ams6-xml-api-datatypes");

                string StartTime = s.SelectSingleNode("./amsx-datatypes:Value[@propertyName = 'StartTime']", fl.nsmgr)?.InnerText;
                string EndTime = s.SelectSingleNode("./amsx-datatypes:Value[@propertyName = 'EndTime']", fl.nsmgr)?.InnerText;
                string GateName = s.SelectSingleNode("./amsx-datatypes:Gate/amsx-datatypes:Value[@propertyName = 'Name']", fl.nsmgr)?.InnerText;
                string GateExternalName = s.SelectSingleNode("./amsx-datatypes:Gate/amsx-datatypes:Value[@propertyName = 'ExternalName']", fl.nsmgr)?.InnerText;
                string Area = s.SelectSingleNode("./amsx-datatypes:Gate/amsx-datatypes:Area/amsx-datatypes:Value[@propertyName = 'Name']", fl.nsmgr)?.InnerText;

                if (StartTime != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "StartTime");
                    e.InnerText = StartTime;
                    row.AppendChild(e);
                }
                if (EndTime != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "EndTime");
                    e.InnerText = EndTime;
                    row.AppendChild(e);
                }
                if (GateName != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "GateName");
                    e.InnerText = GateName;
                    row.AppendChild(e);
                }
                if (GateExternalName != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
                    e.SetAttribute("propertyName", "GateExternalName");
                    e.InnerText = GateExternalName;
                    row.AppendChild(e);
                }
                if (Area != null)
                {
                    XmlElement e = doc.CreateElement(null, "Value", "http://www.sita.aero/ams6-xml-api-datatypes");
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
