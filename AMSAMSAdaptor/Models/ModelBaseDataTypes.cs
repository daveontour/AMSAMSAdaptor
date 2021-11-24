using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{

    public abstract class ModelBase
    {
        public XmlNamespaceManager nsmgr;
        public static string token;
        public string AMSXMessageHeader = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<amsx-messages:Envelope
xmlns:amsx-messages=""http://www.sita.aero/ams6-xml-api-messages""
xmlns:amsx-datatypes=""http://www.sita.aero/ams6-xml-api-datatypes""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
apiVersion=""2.8"">
<amsx-messages:Content>";
        public string AMSXMessageBottom = @"</amsx-messages:Content></amsx-messages:Envelope>";

        public Dictionary<string, string> State { get; set; } = new Dictionary<string, string>();
        public ModelBase(XmlNode node)
        {
            if (node != null)
            {
                nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
                nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
                nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");
            }
        }

        public virtual string CreateRequest() { return null; }
        public virtual string UpdateRequest() { return null; }
        public virtual string DeleteRequest() { return null; }

        public static string PrintXML(string xml)
        {

            string result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXml = sReader.ReadToEnd();

                result = formattedXml;
            }
            catch (XmlException)
            {
                // Handle the exception
            }

            mStream.Close();
            writer.Close();

            return result;
        }

    }

    public class ModelAircraftTypeId : ModelBase
    {
        private string AircraftTypeCodeIATA { get; set; }
        private string AircraftTypeCodeICAO { get; set; }

        public ModelAircraftTypeId(XmlNode node):base(node)
        {
            AircraftTypeCodeIATA = node.SelectSingleNode(".//amsx-datatypes:AircraftTypeCode[@codeContext = 'IATA']", nsmgr)?.InnerText;
            AircraftTypeCodeICAO = node.SelectSingleNode(".//amsx-datatypes:AircraftTypeCode[@codeContext = 'ICAO']", nsmgr)?.InnerText;
        }

        public string GetTypeId()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<amsx-messages:AircraftTypeId>");
            if (AircraftTypeCodeIATA != null) sb.AppendLine($"<amsx-datatypes:AircraftTypeCode codeContext=\"IATA\">{AircraftTypeCodeIATA}</amsx-datatypes:AircraftTypeCode>");
            if (AircraftTypeCodeICAO != null) sb.AppendLine($"<amsx-datatypes:AircraftTypeCode codeContext=\"ICAO\">{AircraftTypeCodeICAO}</amsx-datatypes:AircraftTypeCode>");
            sb.AppendLine("</amsx-messages:AircraftTypeId>");
            return sb.ToString();
        }
    }

    public class ModelAircraftType : ModelBase
    {
        ModelAircraftTypeId ModelAircraftTypeId { get; set; }

        public ModelAircraftType(XmlNode node) : base(node)
        {
            ModelAircraftTypeId = new ModelAircraftTypeId(node);
            foreach (XmlNode n in node.SelectNodes(".//amsx-datatypes:Value",nsmgr))
            {
                State.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }
        }

        public override string CreateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AircraftTypeCreateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine(ModelAircraftTypeId.GetTypeId());
            sb.AppendLine("<amsx-messages:AircraftTypeUpdates>");
            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }
            sb.AppendLine("</amsx-messages:AircraftTypeUpdates>");
            sb.AppendLine("</amsx-messages:AircraftTypeCreateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
        public override string UpdateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AircraftTypeUpdateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine(ModelAircraftTypeId.GetTypeId());
            sb.AppendLine("<amsx-messages:AircraftTypeUpdates>");
            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }
            sb.AppendLine("</amsx-messages:AircraftTypeUpdates>");
            sb.AppendLine("</amsx-messages:AircraftTypeUpdateRequest>");
            sb.AppendLine(AMSXMessageBottom); 
            return PrintXML(sb.ToString());
        }
        public override string DeleteRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AircraftTypeDeleteRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine(ModelAircraftTypeId.GetTypeId());
            sb.AppendLine("</amsx-messages:AircraftTypeDeleteRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
    }

    public class ModelAircraft : ModelBase
    {
        ModelAircraftType ModelAircraftType { get; set; }
        string AircraftRegistration { get; set; }
        public ModelAircraft(XmlNode node) : base(node)
        {
            XmlNode type = node.SelectSingleNode(".//AircraftType", nsmgr);
            if (type != null) ModelAircraftType = new ModelAircraftType(node.SelectSingleNode(".//AircraftType", nsmgr));
            AircraftRegistration = node.SelectSingleNode(".//amsx-datatypes:Registration", nsmgr)?.InnerText;
            foreach (XmlNode n in node.SelectNodes(".//amsx-datatypes:AircraftState/amsx-datatypes:Value", nsmgr))
            {
                State.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }
        }

        public override string CreateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AircraftCreateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-messages:AircraftId>");
            sb.AppendLine($"<amsx-datatypes:Registration>{AircraftRegistration}</amsx-datatypes:Registration>");
            sb.AppendLine("</amsx-messages:AircraftId>");
            sb.AppendLine("<amsx-messages:AircraftUpdates>");
            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }
            sb.AppendLine("</amsx-messages:AircraftUpdates>");
            sb.AppendLine("</amsx-messages:AircraftCreateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
        public override string UpdateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AircraftUpdateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-messages:AircraftId>");
            sb.AppendLine($"<amsx-datatypes:Registration>{AircraftRegistration}</amsx-datatypes:Registration>");
            sb.AppendLine("</amsx-messages:AircraftId>");
            sb.AppendLine("<amsx-messages:AircraftUpdates>");
            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }
            sb.AppendLine("</amsx-messages:AircraftUpdates>");
            sb.AppendLine("</amsx-messages:AircraftUpdateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
        public override string DeleteRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AircraftDeleteRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-messages:AircraftId>");
            sb.AppendLine($"<amsx-datatypes:Registration>{AircraftRegistration}</amsx-datatypes:Registration>");
            sb.AppendLine("</amsx-messages:AircraftId>");
            sb.AppendLine("</amsx-messages:AircraftDeleteRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
    }

    public class ModelAirlineId : ModelBase
    {
        private string AirlineCodeIATA { get; set; }
        private string AirlineCodeICAO { get; set; }

        public ModelAirlineId(XmlNode node) : base(node)
        {
            AirlineCodeIATA = node.SelectSingleNode(".//amsx-datatypes:AirlineCode[@codeContext = 'IATA']", nsmgr)?.InnerText;
            AirlineCodeICAO = node.SelectSingleNode(".//amsx-datatypes:AirlineCode[@codeContext = 'ICAO']", nsmgr)?.InnerText;
        }
        public string GetTypeId()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<amsx-messages:AirlineId>");
            if (AirlineCodeIATA != null) sb.AppendLine($"<amsx-datatypes:AirlineCode codeContext=\"IATA\">{AirlineCodeIATA}</amsx-datatypes:AirlineCode>");
            if (AirlineCodeICAO != null) sb.AppendLine($"<amsx-datatypes:AirlineCode codeContext=\"ICAO\">{AirlineCodeICAO}</amsx-datatypes:AirlineCode>");
            sb.AppendLine("</amsx-messages:AirlineId>");
            return sb.ToString();
        }
    }

    public class ModelAirline : ModelBase
    {
        ModelAirlineId ModelAirlineId { get; set; }

        public ModelAirline(XmlNode node) : base(node)
        {
 
            ModelAirlineId = new ModelAirlineId(node);
            foreach (XmlNode n in node.SelectNodes(".//amsx-datatypes:Value", nsmgr))
            {
                State.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }
        }
        public override string CreateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AirlineCreateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine(ModelAirlineId.GetTypeId());
            sb.AppendLine("<amsx-messages:AirlineUpdates>");
            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }
            sb.AppendLine("</amsx-messages:AirlineUpdates>");
            sb.AppendLine("</amsx-messages:AirlineCreateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
        public override string UpdateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AirlineUpdateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine(ModelAirlineId.GetTypeId());
            sb.AppendLine("<amsx-messages:AirlineUpdates>");
            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }
            sb.AppendLine("</amsx-messages:AirlineUpdates>");
            sb.AppendLine("</amsx-messages:AirlineUpdateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
        public override string DeleteRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AirlineDeleteRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine(ModelAirlineId.GetTypeId());
            sb.AppendLine("</amsx-messages:AirlineDeleteRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
    }

    public class ModelAirportId : ModelBase
    {
        private string AirportCodeIATA { get; set; }
        private string AirportCodeICAO { get; set; }

        public ModelAirportId(XmlNode node) : base(node)
        {
            AirportCodeIATA = node.SelectSingleNode(".//amsx-datatypes:AirportCode[@codeContext = 'IATA']", nsmgr)?.InnerText;
            AirportCodeICAO = node.SelectSingleNode(".//amsx-datatypes:AirportCode[@codeContext = 'ICAO']", nsmgr)?.InnerText;
        }
        public string GetTypeId()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<amsx-messages:AirportId>");
            if (AirportCodeIATA != null) sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{AirportCodeIATA}</amsx-datatypes:AirportCode>");
            if (AirportCodeICAO != null) sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"ICAO\">{AirportCodeICAO}</amsx-datatypes:AirportCode>");
            sb.AppendLine("</amsx-messages:AirportId>");
            return sb.ToString();
        }
    }

    public class ModelAirport : ModelBase
    {
        ModelAirportId ModelAirportId { get; set; }

        public ModelAirport(XmlNode node) : base(node)
        {

            ModelAirportId = new ModelAirportId(node);
            foreach (XmlNode n in node.SelectNodes(".//amsx-datatypes:AirportState/amsx-datatypes:Value", nsmgr))
            {
                State.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }
        }
        public override string CreateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AirportCreateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine(ModelAirportId.GetTypeId());
            sb.AppendLine("<amsx-messages:AirportUpdates>");
            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }
            sb.AppendLine("</amsx-messages:AirportUpdates>");
            sb.AppendLine("</amsx-messages:AirportCreateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
        public override string UpdateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AirportUpdateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine(ModelAirportId.GetTypeId());
            sb.AppendLine("<amsx-messages:AirportUpdates>");
            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }
            sb.AppendLine("</amsx-messages:AirportUpdates>");
            sb.AppendLine("</amsx-messages:AirportUpdateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
        public override string DeleteRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AirportDeleteRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine(ModelAirportId.GetTypeId());
            sb.AppendLine("</amsx-messages:AirportDeleteRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
    }
}
