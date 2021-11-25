using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes;

namespace AMSAMSAdaptor
{
    internal class OutputFlightSOAP : IOutputMessageHandler
    {
        private BasicHttpBinding binding;
        private EndpointAddress address;
        private Supervisor supervisor;

        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("consoleLogger");


        public void SetSupervisor(Supervisor supervisor, XmlDocument configDoc)
        {
            this.supervisor = supervisor;
            supervisor.SendFlightMessageHandler += Sender;
            // Set the binding and address for use by the web services client
            binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 20000000,
                MaxBufferSize = 20000000,
                MaxBufferPoolSize = 20000000
            };
            address = new EndpointAddress(Parameters.TO_AMS_WEB_SERVICE_URI);
        }

        public string GetDescription()
        {
            return "Send Flight Messages via Web Services SOAP Interface";
        }

        private void Sender(ModelFlight flt, string action)
        {
            if (action.Contains("DeleteFlight"))
            {
                //               Console.WriteLine(PrintXML(flt.GetDeleteSoapMessage()));
                logger.Info($"Delete Flight: {flt.FlightProperties["AirlineIATA"]?.Value} {flt.FlightProperties["FlightNumber"]?.Value}");
                SendDeleteFlight(flt);
                supervisor.SendPostFlightMessage(flt,action);
            }
            if (action.Contains("CreateFlight"))
            {
                //               Console.WriteLine(PrintXML(flt.GetCreateSoapMessage()));
                logger.Info($"Create Flight: {flt.FlightProperties["AirlineIATA"]?.Value} {flt.FlightProperties["FlightNumber"]?.Value}");
                SendCreateFlight(flt);
                supervisor.SendPostFlightMessage(flt, action);
            }
            if (action.Contains("UpdateFlight"))
            {
                //               Console.WriteLine(PrintXML(flt.GetUpdateSoapMessage()));
                logger.Info($"Update Flight: {flt.FlightProperties["AirlineIATA"]?.Value} {flt.FlightProperties["FlightNumber"]?.Value}");
                SendUpdateFlight(flt);
                supervisor.SendPostFlightMessage(flt, action);
            }
        }

        private void SendDeleteFlight(ModelFlight flt)
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    FlightId flightId = GetFlightId(flt);
                    
                    XmlElement res = client.DeleteFlight(Parameters.TOTOKEN, flightId);
                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                }
            }
        }
        private void SendCreateFlight(ModelFlight flt)
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    FlightId flightId = GetFlightId(flt);
                    WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes.PropertyValue[] propertyValues = GetPropertValues(flt);
                    XmlElement res = client.CreateFlight(Parameters.TOTOKEN, flightId, propertyValues);
                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                }
            }
        }
        private void SendUpdateFlight(ModelFlight flt)
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    FlightId flightId = GetFlightId(flt);
                    WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes.PropertyValue[] propertyValues = GetPropertValues(flt);
                    XmlElement res = client.UpdateFlight(Parameters.TOTOKEN, flightId, propertyValues);

                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                }
            }
        }


        private FlightId GetFlightId(ModelFlight flt)
        {
            LookupCode apCode = new LookupCode();
            apCode.codeContextField = CodeContext.ICAO;
            apCode.valueField = flt.FlightProperties["AirportICAO"].Value;
            LookupCode[] ap = { apCode };

            LookupCode apCode2 = new LookupCode();
            apCode2.codeContextField = CodeContext.IATA;
            apCode2.valueField = flt.FlightProperties["AirportIATA"].Value;
            ap.Append(apCode2);


            LookupCode alCode = new LookupCode();
            alCode.codeContextField = CodeContext.IATA;
            alCode.valueField = flt.FlightProperties["AirlineIATA"].Value;
            LookupCode[] al = { alCode };

            FlightId flightID = new FlightId();
            flightID.flightKindField = flt.FlightProperties["Nature"].Value == "Arrival" ? FlightKind.Arrival : FlightKind.Departure;
            flightID.airportCodeField = ap;
            flightID.airlineDesignatorField = al;
            flightID.scheduledDateField = Convert.ToDateTime(flt.FlightProperties["ScheduledDate"].Value);
            flightID.flightNumberField = flt.FlightProperties["FlightNumber"].Value;

            return flightID;
        }

        private WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes.PropertyValue[] GetPropertValues(ModelFlight flt)
        {

            WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes.PropertyValue[] val = { };

            foreach (PropertyValue liPV in flt.FlightProperties.Values)
            {
                if (liPV.FlightIdProp) continue;
                WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes.PropertyValue pv = new WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes.PropertyValue()
                {
                    propertyNameField = liPV.PropertyName,
                    valueField = liPV.Value,
                    codeContextField = (liPV.PropertyCodeContext == "ICAO") ? CodeContext.ICAO : CodeContext.IATA
                };
            }

            return val;
        }
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
}
