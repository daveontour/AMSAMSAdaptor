using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace AMSAMSAdaptor
{
    public abstract class ModelBase
    {
        public XmlNamespaceManager nsmgr;
        public static string token;
        public static readonly NLog.Logger logger = NLog.LogManager.GetLogger("consoleLogger");
        public string AMSXMessageHeader = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<amsx-messages:Envelope
xmlns:amsx-messages=""http://www.sita.aero/ams6-xml-api-messages""
xmlns:amsx-datatypes=""http://www.sita.aero/ams6-xml-api-datatypes""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
apiVersion=""2.8"">
<amsx-messages:Content>";

        public string AMSXMessageBottom = @"</amsx-messages:Content></amsx-messages:Envelope>";
        public XmlNode node;
        public Dictionary<string, string> State { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> KeyState { get; set; } = new Dictionary<string, string>();

        public ModelBase(XmlNode node)
        {
            if (node != null)
            {
                this.node = node;
                nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
                nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
                nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");
            }
        }

        public virtual string CreateRequest()
        { return null; }

        public virtual string UpdateRequest()
        { return null; }

        public virtual string DeleteRequest()
        { return null; }

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
            catch (Exception e)
            {
                logger.Error(e.Message);
            }

            mStream.Close();
            writer.Close();

            return result;
        }
    }

    public class ModelAircraftTypeId : ModelBase
    {
        public string AircraftTypeCodeIATA { get; set; }
        public string AircraftTypeCodeICAO { get; set; }

        public ModelAircraftTypeId(XmlNode node) : base(node)
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
        public ModelAircraftTypeId ModelAircraftTypeId { get; set; }

        public ModelAircraftType(XmlNode node) : base(node)
        {
            ModelAircraftTypeId = new ModelAircraftTypeId(node);
            foreach (XmlNode n in node.SelectNodes(".//amsx-datatypes:Value", nsmgr))
            {
                {
                    State.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
                }
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
        private ModelAircraftType ModelAircraftType { get; set; }
        private string AircraftRegistration { get; set; }

        public ModelAircraft(XmlNode node) : base(node)
        {
            XmlNode type = node.SelectSingleNode(".//amsx-datatypes:AircraftTypeId", nsmgr);
            if (type != null) ModelAircraftType = new ModelAircraftType(node.SelectSingleNode(".//amsx-datatypes:AircraftType", nsmgr));
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

            if (ModelAircraftType != null)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"AircraftTypeId\">{ModelAircraftType.ModelAircraftTypeId.AircraftTypeCodeIATA}</amsx-messages:Update>");
            }
            else
            {
                logger.Error($"Aircraft with no type. Rego {AircraftRegistration}");
            }
            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }
            sb.AppendLine("</amsx-messages:AircraftUpdates>");
            sb.AppendLine("</amsx-messages:AircraftCreateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString().Replace("&", "&amp;"));
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

            sb.AppendLine($"<amsx-messages:Update propertyName=\"AircraftTypeId\">{ModelAircraftType?.ModelAircraftTypeId?.AircraftTypeCodeIATA}</amsx-messages:Update>");
            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }
            sb.AppendLine("</amsx-messages:AircraftUpdates>");
            sb.AppendLine("</amsx-messages:AircraftUpdateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString().Replace("&", "&amp;"));
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
        private ModelAirlineId ModelAirlineId { get; set; }

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
            return PrintXML(sb.ToString().Replace("&", "&amp;"));
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
            return PrintXML(sb.ToString().Replace("&", "&amp;"));
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
        private ModelAirportId ModelAirportId { get; set; }

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
            return PrintXML(sb.ToString().Replace("&", "&amp;"));
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
            return PrintXML(sb.ToString().Replace("&", "&amp;"));
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

    public class ModelArea : ModelBase
    {
        private string AreaId { get; set; }
        private string AirportCode { get; set; }

        public ModelArea(XmlNode node) : base(node)
        {
            AreaId = node.SelectSingleNode(".//amsx-datatypes:AreaId/amsx-datatypes:ExternalName", nsmgr)?.InnerText;
            AirportCode = node.SelectSingleNode(".//amsx-datatypes:AreaId/amsx-datatypes:AirportCode[@codeContext = 'IATA']", nsmgr)?.InnerText;

            foreach (XmlNode n in node.SelectNodes(".//amsx-datatypes:AreaState/amsx-datatypes:Value", nsmgr))
            {
                State.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }
        }

        public override string CreateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AreaCreateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-datatypes:AreaId>");
            sb.AppendLine($"<amsx-datatypes:ExternalName>{AreaId}</amsx-datatypes:ExternalName>");
            sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{AirportCode}</amsx-datatypes:AirportCode>");
            sb.AppendLine("</amsx-datatypes:AreaId>");
            sb.AppendLine("<amsx-messages:AreaUpdates>");

            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }

            sb.AppendLine("</amsx-messages:AreaUpdates>");
            sb.AppendLine("</amsx-messages:AreaCreateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception ex)
            {
                logger.Error(ex,sb.ToString());
                return null;
            }
        }

        public override string UpdateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AreaUpdateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-datatypes:AreaId>");
            sb.AppendLine($"<amsx-datatypes:ExternalName>{AreaId}</amsx-datatypes:ExternalName>");
            sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{AirportCode}</amsx-datatypes:AirportCode>");
            sb.AppendLine("</amsx-datatypes:AreaId>");
            sb.AppendLine("<amsx-messages:AreaUpdates>");

            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }

            sb.AppendLine("</amsx-messages:AreaUpdates>");
            sb.AppendLine("</amsx-messages:AreaUpdateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                logger.Error(e,sb.ToString());
                return null;
            }
        }

        public override string DeleteRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:AreaDeleteRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-datatypes:AreaId>");
            sb.AppendLine($"<amsx-datatypes:ExternalName>{AreaId}</amsx-datatypes:ExternalName>");
            sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{AirportCode}</amsx-datatypes:AirportCode>");
            sb.AppendLine("</amsx-datatypes:AreaId>");
            sb.AppendLine("</amsx-messages:AreaDeleteRequest>");
            sb.AppendLine(AMSXMessageBottom);
            return PrintXML(sb.ToString());
        }
    }

    public class ModelGate : ModelBase
    {
        private string ExternalName { get; set; }
        private string AirportCode { get; set; }

        //private string AreaIdExternalName { get; set; }
        //private string AreaName { get; set; }
        public ModelGate(XmlNode node) : base(node)
        {
            ExternalName = node.SelectSingleNode(".//amsx-datatypes:GateId/amsx-datatypes:ExternalName", nsmgr)?.InnerText;
            AirportCode = node.SelectSingleNode(".//amsx-datatypes:GateId/amsx-datatypes:AirportCode[@codeContext = 'IATA']", nsmgr)?.InnerText;
            //AreaIdExternalName = node.SelectSingleNode(".//amsx-datatypes:AreaId/amsx-datatypes:ExternalName", nsmgr)?.InnerText;
            //AreaName = node.SelectSingleNode(".//amsx-datatypes:Area/amsx-datatypes:Value[@propertyName = 'Name']", nsmgr)?.InnerText;

            foreach (XmlNode n in node.SelectNodes(".//amsx-datatypes:ResourceState/amsx-datatypes:Value", nsmgr))
            {
                State.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }
        }

        public override string CreateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:GateCreateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-datatypes:GateId>");
            sb.AppendLine($"<amsx-datatypes:ExternalName>{ExternalName}</amsx-datatypes:ExternalName>");
            sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{AirportCode}</amsx-datatypes:AirportCode>");
            sb.AppendLine("</amsx-datatypes:GateId>");
            sb.AppendLine("<amsx-messages:GateUpdates>");

            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }

            sb.AppendLine("</amsx-messages:GateUpdates>");
            sb.AppendLine("</amsx-messages:GateCreateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                logger.Error(e,sb.ToString());
                return null;
            }
        }

        public override string UpdateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:GateUpdateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-datatypes:GateId>");
            sb.AppendLine($"<amsx-datatypes:ExternalName>{ExternalName}</amsx-datatypes:ExternalName>");
            sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{AirportCode}</amsx-datatypes:AirportCode>");
            sb.AppendLine("</amsx-datatypes:GateId>");
            sb.AppendLine("<amsx-messages:GateUpdates>");

            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }

            sb.AppendLine("</amsx-messages:GateUpdates>");
            sb.AppendLine("</amsx-messages:GateUpdateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                logger.Error(e,sb.ToString());
                return null;
            }
        }
    }

    public class ModelStand : ModelBase
    {
        private string ExternalName { get; set; }
        private string AirportCode { get; set; }

        //private string StandIdExternalName { get; set; }
        //private string StandAreaName { get; set; }
        public ModelStand(XmlNode node) : base(node)
        {
            ExternalName = node.SelectSingleNode(".//amsx-datatypes:GateId/amsx-datatypes:ExternalName", nsmgr)?.InnerText;
            AirportCode = node.SelectSingleNode(".//amsx-datatypes:GateId/amsx-datatypes:AirportCode[@codeContext = 'IATA']", nsmgr)?.InnerText;

            foreach (XmlNode n in node.SelectNodes(".//amsx-datatypes:ResourceState/amsx-datatypes:Value", nsmgr))
            {
                State.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }
        }

        public override string CreateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:StandCreateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-datatypes:StandId>");
            sb.AppendLine($"<amsx-datatypes:ExternalName>{ExternalName}</amsx-datatypes:ExternalName>");
            sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{AirportCode}</amsx-datatypes:AirportCode>");
            sb.AppendLine("</amsx-datatypes:StandId>");
            sb.AppendLine("<amsx-messages:StandUpdates>");

            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }

            sb.AppendLine("</amsx-messages:StandUpdates>");
            sb.AppendLine("</amsx-messages:StandCreateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                logger.Error(e,sb.ToString());
                return null;
            }
        }

        public override string UpdateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:StandUpdateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-datatypes:StandId>");
            sb.AppendLine($"<amsx-datatypes:ExternalName>{ExternalName}</amsx-datatypes:ExternalName>");
            sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{AirportCode}</amsx-datatypes:AirportCode>");
            sb.AppendLine("</amsx-datatypes:StandId>");
            sb.AppendLine("<amsx-messages:StandUpdates>");

            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }

            sb.AppendLine("</amsx-messages:StandUpdates>");
            sb.AppendLine("</amsx-messages:StandUpdateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                logger.Error(e,sb.ToString());
                return null;
            }
        }
    }

    public class ModelCheckIn : ModelBase
    {
        private string ExternalName { get; set; }
        private string AirportCode { get; set; }

        //private string CheckInIdExternalName { get; set; }
        //private string CheckInAreaName { get; set; }
        public ModelCheckIn(XmlNode node) : base(node)
        {
            ExternalName = node.SelectSingleNode(".//amsx-datatypes:GateId/amsx-datatypes:ExternalName", nsmgr)?.InnerText;
            AirportCode = node.SelectSingleNode(".//amsx-datatypes:GateId/amsx-datatypes:AirportCode[@codeContext = 'IATA']", nsmgr)?.InnerText;
            //AreaIdExternalName = node.SelectSingleNode(".//amsx-datatypes:AreaId/amsx-datatypes:ExternalName", nsmgr)?.InnerText;
            //AreaName = node.SelectSingleNode(".//amsx-datatypes:Area/amsx-datatypes:Value[@propertyName = 'Name']", nsmgr)?.InnerText;

            foreach (XmlNode n in node.SelectNodes(".//amsx-datatypes:ResourceState/amsx-datatypes:Value", nsmgr))
            {
                State.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }
        }

        public override string CreateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:CheckInCreateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-datatypes:CheckInId>");
            sb.AppendLine($"<amsx-datatypes:ExternalName>{ExternalName}</amsx-datatypes:ExternalName>");
            sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{AirportCode}</amsx-datatypes:AirportCode>");
            sb.AppendLine("</amsx-datatypes:CheckInId>");
            sb.AppendLine("<amsx-messages:CheckInUpdates>");

            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }

            sb.AppendLine("</amsx-messages:CheckInUpdates>");
            sb.AppendLine("</amsx-messages:CheckInCreateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                logger.Error(e,sb.ToString());
                return null;
            }
        }

        public override string UpdateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:CheckInUpdateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-datatypes:CheckInId>");
            sb.AppendLine($"<amsx-datatypes:ExternalName>{ExternalName}</amsx-datatypes:ExternalName>");
            sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{AirportCode}</amsx-datatypes:AirportCode>");
            sb.AppendLine("</amsx-datatypes:CheckInId>");
            sb.AppendLine("<amsx-messages:CheckInUpdates>");

            foreach (KeyValuePair<string, string> n in State)
            {
                sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
            }

            sb.AppendLine("</amsx-messages:CheckInUpdates>");
            sb.AppendLine("</amsx-messages:CheckInUpdateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception)
            {
                logger.Error(sb.ToString());
                return null;
            }
        }
    }

    public class ModelRoute : ModelBase
    {
        private struct RouteViaPoint
        {
            public string sequenceNumber;
            public string iataAirport;
            public string icaoAirport;
        }

        private List<RouteViaPoint> viaPoints = new List<RouteViaPoint>();

        public ModelRoute(XmlNode node) : base(node)
        {
            KeyState.Add("CustomTypeCode", node.SelectSingleNode(".//amsx-datatypes:CustomsTypeCode", nsmgr)?.InnerText);

            foreach (XmlNode n in node.SelectNodes(".//amsx-datatypes:RouteViaPoint", nsmgr))
            {
                RouteViaPoint point;
                point.sequenceNumber = n.Attributes["sequenceNumber"]?.Value;
                point.icaoAirport = n.SelectSingleNode(".//amsx-datatypes:AirportCode[@codeContext='ICAO']", nsmgr)?.InnerText;
                point.iataAirport = n.SelectSingleNode(".//amsx-datatypes:AirportCode[@codeContext='IATA']", nsmgr)?.InnerText;

                viaPoints.Add(point);
            }

            foreach (XmlNode n in node.SelectNodes(".//amsx-datatypes:ResourceState/amsx-datatypes:Value", nsmgr))
            {
                State.Add(n.Attributes["propertyName"]?.Value, n.InnerText);
            }
        }

        public override string CreateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:RouteCreateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-messages:RouteId>");
            sb.AppendLine("<amsx-datatypes:ViaPoints>");

            foreach (RouteViaPoint point in viaPoints)
            {
                sb.AppendLine($"<amsx-datatypes:RouteViaPoint sequenceNumber=\"{point.sequenceNumber}\">");
                sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{point.iataAirport}</amsx-datatypes:AirportCode>");
                sb.AppendLine("</amsx-datatypes:RouteViaPoint>");
            }

            sb.AppendLine("</amsx-datatypes:ViaPoints>");
            sb.AppendLine("</amsx-messages:RouteId>");

            if (KeyState["CustomTypeCode"] != null || State.Count > 0)
            {
                sb.AppendLine("<amsx-messages:RouteUpdates>");
                if (KeyState["CustomTypeCode"] != null) sb.AppendLine($"<amsx-messages:Update propertyName=\"CustomsTypeCode\">{KeyState["CustomTypeCode"]}</amsx-messages:Update>");
                foreach (KeyValuePair<string, string> n in State)
                {
                    sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
                }
                sb.AppendLine("</amsx-messages:RouteUpdates>");
            }

            sb.AppendLine("</amsx-messages:RouteCreateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception)
            {
                logger.Error(sb.ToString());
                return null;
            }
        }

        public override string UpdateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:RouteUpdateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-messages:RouteId>");
            sb.AppendLine("<amsx-datatypes:ViaPoints>");

            foreach (RouteViaPoint point in viaPoints)
            {
                sb.AppendLine($"<amsx-datatypes:RouteViaPoint sequenceNumber=\"{point.sequenceNumber}\">");
                sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{point.iataAirport}</amsx-datatypes:AirportCode>");
                sb.AppendLine("</amsx-datatypes:RouteViaPoint>");
            }

            sb.AppendLine("</amsx-datatypes:ViaPoints>");
            sb.AppendLine("</amsx-messages:RouteId>");

            if (KeyState["CustomTypeCode"] != null || State.Count > 0)
            {
                sb.AppendLine("<amsx-messages:RouteUpdates>");
                if (KeyState["CustomTypeCode"] != null) sb.AppendLine($"<amsx-messages:Update propertyName=\"CustomsTypeCode\">{KeyState["CustomTypeCode"]}</amsx-messages:Update>");
                foreach (KeyValuePair<string, string> n in State)
                {
                    sb.AppendLine($"<amsx-messages:Update propertyName=\"{n.Key}\">{n.Value}</amsx-messages:Update>");
                }
                sb.AppendLine("</amsx-messages:RouteUpdates>");
            }
            sb.AppendLine("</amsx-messages:RouteUpdateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                logger.Error(e,sb.ToString());
                return null;
            }
        }

        public override string DeleteRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:RouteDeleteRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-messages:RouteId>");
            sb.AppendLine("<amsx-datatypes:ViaPoints>");

            foreach (RouteViaPoint point in viaPoints)
            {
                sb.AppendLine($"<amsx-datatypes:RouteViaPoint sequenceNumber=\"{point.sequenceNumber}\">");
                sb.AppendLine($"<amsx-datatypes:AirportCode codeContext=\"IATA\">{point.iataAirport}</amsx-datatypes:AirportCode>");
                sb.AppendLine("</amsx-datatypes:RouteViaPoint");
            }

            sb.AppendLine("<amsx-datatypes:ViaPoints>");
            sb.AppendLine("</amsx-messages:RouteId>");
            sb.AppendLine("</amsx-messages:RouteDeleteRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                logger.Error(e,sb.ToString());
                return null;
            }
        }
    }

    public class ModelCustomsType : ModelBase
    {


        public string CustomTypeCode;
        public string CustomTypeName;
        public string CustomTypePriority;

        public ModelCustomsType(XmlNode node) : base(node)
        {
            CustomTypeCode = node.SelectSingleNode(".//amsx-datatypes:CustomsTypeCode", nsmgr)?.InnerText;
            CustomTypeName = node.SelectSingleNode(".//amsx-datatypes:CustomsTypeState/amsx-datatypes:Value[@propertyName='Name']", nsmgr)?.InnerText;
            CustomTypePriority = node.SelectSingleNode(".//amsx-datatypes:CustomsTypeState/amsx-datatypes:Value[@propertyName='Priority']", nsmgr)?.InnerText;
        }

        public override string CreateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:CustomsTypeCreateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-messages:CustomsTypeId>");
            sb.AppendLine($"<amsx-datatypes:CustomsTypeCode>{CustomTypeCode}</amsx-datatypes:CustomsTypeCode>");
            sb.AppendLine("</amsx-messages:CustomsTypeId>");
            sb.AppendLine("<amsx-messages:CustomsTypeUpdates>");
            sb.AppendLine($"<amsx-messages:Update propertyName=\"Name\">{CustomTypeName}</amsx-messages:Update>");
            sb.AppendLine($"<amsx-messages:Update propertyName=\"Priority\">{CustomTypePriority}</amsx-messages:Update>");
            sb.AppendLine("</amsx-messages:CustomsTypeUpdates>");
            sb.AppendLine("</amsx-messages:CustomsTypeCreateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                logger.Error(e,sb.ToString());
                return null;
            }
        }

        public override string UpdateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:CustomsTypeUpdateRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-messages:CustomsTypeId>");
            sb.AppendLine($"<amsx-datatypes:CustomsTypeCode>{CustomTypeCode}</amsx-datatypes:CustomsTypeCode>");
            sb.AppendLine("</amsx-messages:CustomsTypeId>");
            sb.AppendLine("<amsx-messages:CustomsTypeUpdates>");
            sb.AppendLine($"<amsx-messages:Update propertyName=\"Name\">{CustomTypeName}</amsx-messages:Update>");
            sb.AppendLine($"<amsx-messages:Update propertyName=\"Priority\">{CustomTypePriority}</amsx-messages:Update>");
            sb.AppendLine("</amsx-messages:CustomsTypeUpdates>");
            sb.AppendLine("</amsx-messages:CustomsTypeUpdateRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                logger.Error(e,sb.ToString());
                return null;
            }
        }

        public override string DeleteRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AMSXMessageHeader);
            sb.AppendLine("<amsx-messages:CustomsTypeDeleteRequest>");
            sb.AppendLine("<amsx-datatypes:Token>TOKEN</amsx-datatypes:Token>");
            sb.AppendLine("<amsx-messages:CustomsTypeId>");
            sb.AppendLine($"<amsx-datatypes:CustomsTypeCode>{CustomTypeCode}</amsx-datatypes:CustomsTypeCode>");
            sb.AppendLine("</amsx-messages:CustomsTypeId>");
            sb.AppendLine("</amsx-messages:CustomsTypeDeleteRequest>");
            sb.AppendLine(AMSXMessageBottom);
            try
            {
                return PrintXML(sb.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                logger.Error(e,sb.ToString());
                return null;
            }
        }
    }
    }