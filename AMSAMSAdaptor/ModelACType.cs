using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{

    public class ModelAircraftTypeId
    {
        private XmlNamespaceManager nsmgr;


        private string AircraftTypeCodeIATA { get; set; }
        private string AircraftTypeCodeICAO { get; set; }

        public ModelAircraftTypeId(XmlNode node)
        {
            nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");

            AircraftTypeCodeIATA = node.SelectSingleNode(".//AircraftTypeCode[@codeContext = 'IATA']", nsmgr)?.InnerText;
            AircraftTypeCodeICAO = node.SelectSingleNode(".//AircraftTypeCode[@codeContext = 'ICAO']", nsmgr)?.InnerText;
        }
    }

    public class ModelAircraftType
    {
        ModelAircraftTypeId ModelAircraftTypeId { get; set; }
        Dictionary<string, string> AircraftTypeState { get; set; }
        private XmlNamespaceManager nsmgr;

        public ModelAircraftType(XmlNode node)
        {
            nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");

            ModelAircraftTypeId = new ModelAircraftTypeId(node);
            foreach(XmlNode n in node.SelectNodes(".//Value"))
            {
                AircraftTypeState.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }   
        }
    }

    public class ModelAircraft
    {
        ModelAircraftType ModelAircraftType { get; set; }
        string AircraftRegistration { get; set; }
        Dictionary<string, string> AircraftState { get; set; }
        private XmlNamespaceManager nsmgr;

        public ModelAircraft(XmlNode node)
        {
            nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");

            ModelAircraftType = new ModelAircraftType(node.SelectSingleNode(".//AircraftType"));
            AircraftRegistration = node.SelectSingleNode(".//Registration")?.InnerText;
            foreach (XmlNode n in node.SelectNodes(".//AircraftState/Value"))
            {
                AircraftState.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }
        }
    }

    public class ModelAirlineId
    {
        private XmlNamespaceManager nsmgr;


        private string AirlineCodeIATA { get; set; }
        private string AirlineCodeICAO { get; set; }

        public ModelAirlineId(XmlNode node)
        {
            nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");

            AirlineCodeIATA = node.SelectSingleNode(".//AirlineCode[@codeContext = 'IATA']", nsmgr)?.InnerText;
            AirlineCodeICAO = node.SelectSingleNode(".//AirlineCode[@codeContext = 'ICAO']", nsmgr)?.InnerText;
        }
    }

    public class ModelAirline
    {
        ModelAirlineId ModelAirlineId { get; set; }
        Dictionary<string, string> AirlineState { get; set; }
        private XmlNamespaceManager nsmgr;

        public ModelAirline(XmlNode node)
        {
            nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");

            ModelAirlineId = new ModelAirlineId(node);
            foreach (XmlNode n in node.SelectNodes(".//Value"))
            {
                AirlineState.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }
        }
    }
}
