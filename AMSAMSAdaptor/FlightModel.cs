using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    [Serializable]
    public class PropertyValue
    {
        public bool CoreProperty { get; set; } = false;
        public string Value { get; set; }
        public string PropertyName { get; set; }

        private string propertyCodeContext = "IATA";
        public string PropertyCodeContext
        {
            get { return propertyCodeContext; }
            set
            {
                propertyCodeContext = value;
                if (value != null) {
                    PropertyCodeContextSpecified = true; }
                else {
                    PropertyCodeContextSpecified = false; }
            }
        }
        public bool PropertyCodeContextSpecified { get; set; } = false;

        public PropertyValue() { }

        public PropertyValue(XmlNode node)
        {
            this.PropertyName = node.Attributes["propertyName"]?.Value;
            this.PropertyCodeContext = node.Attributes["codeContext"]?.Value;
            this.Value = node.InnerText;
        }
        public override string ToString()
        {
            string template = @"<wor:PropertyValue>
					<wor:codeContextField>@contextCode@</wor:codeContextField>
					<wor:codeContextFieldSpecified>@contextCodeSpecified@</wor:codeContextFieldSpecified>
					<wor:propertyNameField>@propertyNameField@</wor:propertyNameField>
					<wor:valueField>@valueField@</wor:valueField>
				</wor:PropertyValue>";
            string val = template.Replace("@contextCode@", PropertyCodeContext)
                .Replace("@contextCode@", PropertyCodeContext)
                .Replace("@contextCodeSpecified@", PropertyCodeContextSpecified.ToString())
                .Replace("@propertyNameField@", PropertyName)
                .Replace("@valueField@", Value);

            return val;
        }
    }

    [Serializable]
    public class FlightModel
    {
  //      [NonSerialized]
        public XmlNamespaceManager nsmgr;
        private XmlNode node;

        protected XmlDocument configDoc;


        public Dictionary<string, PropertyValue> FlightProperties { get; set; } = new Dictionary<string, PropertyValue>();
        public List<string> Routes { get; set; } = new List<string>();

        //[System.Runtime.Serialization.OnDeserialized]
        //private void OnDeserialized()
        //{
        //    nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
        //    nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");
        //}
        public FlightModel(XmlNode node)
        {
            this.node = node;
            nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");

            SetCoreValue("Nature", ".//ams:FlightId/ams:FlightKind");
            SetCoreValue("AirlineIATA", ".//ams:AirlineDesignator[@codeContext = 'IATA']");
            SetCoreValue("AirlineICAO", ".//ams:AirlineDesignator[@codeContext='ICAO']");
            SetCoreValue("FlightNumber", ".//ams:FlightId/ams:FlightNumber");
            SetCoreValue("ScheduledDate", ".//ams:FlightId/ams:ScheduledDate");
            SetCoreValue("AirportIATA", ".//ams:FlightId/ams:AirportCode[@codeContext='IATA']");
            SetCoreValue("AirportICAO", ".//ams:FlightId/ams:AirportCode[@codeContext='ICAO']");


            SetCoreValue("LinkedNature", ".//ams:LinkedFlight/ams:FlightId/ams:FlightKind");
            SetCoreValue("LinkedAirlineIATA", ".//ams:LinkedFlight/ams:AirlineDesignator[@codeContext = 'IATA']");
            SetCoreValue("LinkedAirlineICAO", ".//ams:LinkedFlight/ams:AirlineDesignator[@codeContext='ICAO']");
            SetCoreValue("LinkedFlightNumber", ".//ams:LinkedFlight/ams:FlightId/ams:FlightNumber");
            SetCoreValue("LinkedScheduledDate", ".//ams:LinkedFlight/ams:FlightId/ams:ScheduledDate");
            SetCoreValue("LinkedAirportIATA", ".//ams:LinkedFlight/ams:FlightId/ams:AirportCode[@codeContext='IATA']");
            SetCoreValue("LinkedAirportICAO", ".//ams:LinkedFlight/ams:FlightId/ams:AirportCode[@codeContext='ICAO']");

            SetCoreValue("ScheduledTime", ".//ams:FlightState/ams:ScheduledTime");

            SetCoreValue("AircraftTypeCode", ".//ams:FlightState/ams:AircraftType/ams:AircraftTypeId/ams:AircraftTypeCode[@codeContext='IATA']");
            SetCoreValue("AircraftTypeCodeICAO", ".//ams:FlightState/ams:AircraftType/ams:AircraftTypeId/ams:AircraftTypeCode[@codeContext='ICAO']");
            SetCoreValue("AircraftRegistration", ".//ams:FlightState/ams:Aircraft/ams:AircraftId/ams:Registration");

            foreach (XmlNode v in node.SelectNodes(".//ams:FlightState/ams:Route/ams:ViaPoints/ams:RouteViaPoint/ams:AirportCode[@codeContext='IATA']", nsmgr))
            {
                Routes.Add(v.InnerText);
            }

            foreach (XmlNode v in node.SelectNodes(".//ams:FlightState/ams:Value", nsmgr))
            {
                PropertyValue pv = new PropertyValue(v);
                FlightProperties.Add(pv.PropertyName, pv); 
            }
        }

        private void SetCoreValue(string name, string xpath)
        {
            string value = node.SelectSingleNode(xpath, nsmgr)?.InnerText;
            if (value != null)
            {
                PropertyValue pv = new PropertyValue()
                {
                    Value = value,
                    PropertyName = name,
                    CoreProperty = true
                };
                FlightProperties.Add(pv.PropertyName, pv);
            }
        }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();
            foreach(PropertyValue v in FlightProperties.Values)
            {
                if (!v.CoreProperty) continue;
                sb.AppendLine($"{v.PropertyName}: {v.Value}");
            }
            sb.AppendLine($"Route: {string.Join(",",Routes)}");

            foreach (PropertyValue v in FlightProperties.Values)
            {
                if (v.CoreProperty) continue;
                sb.AppendLine($"{v.PropertyName}: {v.Value}");
            }



            return sb.ToString();  
        }

        public string GetFlightID()
        {
            string template = @"
         <ams6:flightIdentifier>
            <wor:_hasAirportCodes>true</wor:_hasAirportCodes>
            <wor:_hasFlightDesignator>true</wor:_hasFlightDesignator>
            <wor:_hasScheduledTime>true</wor:_hasScheduledTime>
            <wor:airlineDesignatorField>
               <wor:LookupCode>
                  <wor:codeContextField>IATA</wor:codeContextField>
                  <wor:valueField>@@airline@@</wor:valueField>
               </wor:LookupCode>
            </wor:airlineDesignatorField>
            <wor:airportCodeField>
               <wor:LookupCode>
                  <wor:codeContextField>IATA</wor:codeContextField>
                  <wor:valueField>@@airport@@</wor:valueField>
               </wor:LookupCode>
            </wor:airportCodeField>
            <wor:flightKindField>@@nature@@</wor:flightKindField>
            <wor:flightNumberField>@@fltNum@@</wor:flightNumberField>
            <wor:scheduledDateField>@@schedDate@@</wor:scheduledDateField>
         </ams6:flightIdentifier>";

            string flightID = template.Replace("@@airline@@", FlightProperties["AirlineIATA"].Value)
                .Replace("@@airport@@", FlightProperties["AirportIATA"].Value)
                .Replace("@@nature@@", FlightProperties["Nature"].Value)
                .Replace("@@fltNum@@", FlightProperties["FlightNumber"].Value)
                .Replace("@@schedDate@@", FlightProperties["ScheduledDate"].Value);

            return flightID;
        }

        public string GetUpdateSoapMessage()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<soapenv:Envelope xmlns:soapenv = ""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ams6 = ""http://www.sita.aero/ams6-xml-api-webservice"" xmlns:wor = ""http://schemas.datacontract.org/2004/07/WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes"" >
    <soapenv:Header/>
    <soapenv:Body>
        <ams6:UpdateFlight>
            <ams6:sessionToken>@@TOKEN@@</ams6:sessionToken>");

            sb.AppendLine(GetFlightID());
            sb.AppendLine(GetUpdates());

            sb.AppendLine("</ams6:UpdateFlight>");
            sb.AppendLine("</soapenv:Body>");
            sb.AppendLine("</soapenv:Envelope>");
            return sb.ToString();
        }
        public string GetCreateSoapMessage()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<soapenv:Envelope xmlns:soapenv = ""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ams6 = ""http://www.sita.aero/ams6-xml-api-webservice"" xmlns:wor = ""http://schemas.datacontract.org/2004/07/WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes"" >
    <soapenv:Header/>
    <soapenv:Body>
        <ams6:CreateFlight>
            <ams6:sessionToken>@@TOKEN@@</ams6:sessionToken>");

            sb.AppendLine(GetFlightID());
            sb.AppendLine(GetUpdates());

            sb.AppendLine("</ams6:CreateFlight>");
            sb.AppendLine("</soapenv:Body>");
            sb.AppendLine("</soapenv:Envelope>");
            return sb.ToString();
        }

        public string GetDeleteSoapMessage()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<soapenv:Envelope xmlns:soapenv = ""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ams6 = ""http://www.sita.aero/ams6-xml-api-webservice"" xmlns:wor = ""http://schemas.datacontract.org/2004/07/WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes"" >
    <soapenv:Header/>
    <soapenv:Body>
        <ams6:DeleteFlight>
            <ams6:sessionToken>@@TOKEN@@</ams6:sessionToken>");

            sb.AppendLine(GetFlightID());
            sb.AppendLine("</ams6:DeleteFlight>");
            sb.AppendLine("</soapenv:Body>");
            sb.AppendLine("</soapenv:Envelope>");
            return sb.ToString();
        }
        public string GetUpdates()
        {
            StringBuilder sb = new StringBuilder();
            if (FlightProperties.ContainsKey("ScheduledTime")) sb.AppendLine(FlightProperties["ScheduledTime"]?.ToString());
            if (FlightProperties.ContainsKey("AircraftRegisitration")) sb.AppendLine(FlightProperties["AircraftRegisitration"]?.ToString());
            if (FlightProperties.ContainsKey("AircraftTypeCode")) sb.AppendLine(FlightProperties["AircraftTypeCode"]?.ToString());
            if (Routes.Count > 0)
            {
                string template = @"<wor:PropertyValue>
					<wor:codeContextField>IATA</wor:codeContextField>
					<wor:codeContextFieldSpecified>true</wor:codeContextFieldSpecified>
					<wor:propertyNameField>Route</wor:propertyNameField>
					<wor:valueField>@@route@@</wor:valueField>
				</wor:PropertyValue>";
                sb.AppendLine(template.Replace("@@route@@", string.Join(",",Routes)));
            }
            sb.AppendLine("<ams6:updates>");
            foreach (PropertyValue v in FlightProperties.Values)
            {
                if (v.CoreProperty) continue;
                sb.AppendLine(v.ToString());
            }
            sb.AppendLine("</ams6:updates>");
            return sb.ToString();
        }        
    }
}
