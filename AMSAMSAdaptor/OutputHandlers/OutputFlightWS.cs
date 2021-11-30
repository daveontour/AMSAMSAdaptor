using System;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Xml;
using WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes;


namespace AMSAMSAdaptor
{
    internal class OutputFlightWS : IOutputMessageHandler
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
                logger.Info($"Delete Flight: {flt.FlightProperties["AirlineIATA"]?.Value} {flt.FlightProperties["FlightNumber"]?.Value}");
                SendDeleteFlight(flt);
                supervisor.SendPostFlightMessage(flt, action);
            }
            if (action.Contains("CreateFlight"))
            {
                logger.Info($"Create Flight: {flt.FlightProperties["AirlineIATA"]?.Value} {flt.FlightProperties["FlightNumber"]?.Value}");
                SendCreateFlightExtended(flt);
                supervisor.SendPostFlightMessage(flt, action);
            }
            if (action.Contains("UpdateFlight"))
            {
                logger.Info($"Update Flight: {flt.FlightProperties["AirlineIATA"]?.Value} {flt.FlightProperties["FlightNumber"]?.Value}");
                SendUpdateFlightExtended(flt);
                supervisor.SendPostFlightMessage(flt, action);
            }
        }

        private void SendDeleteFlight(ModelFlight flt)
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    XmlElement res = client.DeleteFlight(Parameters.TOTOKEN, flt.GetFlightId());
                    logger.Trace(res.OuterXml);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error Deleting Flight");
                }
            }
        }
        //private void SendCreateFlight(ModelFlight flt)
        //{
        //    using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
        //    {
        //        try
        //        {
        //            PropertyValue[] propertyValues = flt.GetPropertValues();
        //            XmlElement res = client.CreateFlight(Parameters.TOTOKEN, flt.GetFlightId(), propertyValues);
        //            logger.Trace(res.OuterXml);
        //            ProcessLinking(flt);
        //        }
        //        catch (Exception e)
        //        {
        //            logger.Error(e, "Error Creating Flight");
        //        }
        //    }
        //}
        private void SendCreateFlightExtended(ModelFlight flt)
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    PropertyValue[] propertyValues = flt.GetPropertValues();
                    XmlElement res = client.CreateFlightExtended(Parameters.TOTOKEN, flt.GetFlightId(), flt.GetFlightUpdateInformation());
                    
                    if (res.OuterXml.Contains("<Status xmlns=\"http://www.sita.aero/ams6-xml-api-datatypes\">Success</Status>"))
                    {
                        logger.Info("Flight Creation Successful :)");
                        ProcessLinking(flt);
                    }
                    else
                    {
                        logger.Warn("Flight Creation Not Successful");
                        logger.Trace(PrintXML(res.OuterXml));
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error Creating Flight");
                }
            }
        }
        //private void SendUpdateFlight(ModelFlight flt)
        //{
        //    logger.Info($"Updating: {flt.ToString()}");

        //    using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
        //    {
        //        try
        //        {
        //            // First Check that the flight exist, if not, then create it.
        //            XmlElement existing = client.GetFlight(Parameters.TOTOKEN, flt.GetFlightId());
                    
        //            if (existing.OuterXml.Contains("<ErrorCode>FLIGHT_NOT_FOUND</ErrorCode>"))
        //            {
        //                SendCreateFlight(flt);
        //                return;
        //            }

        //            PropertyValue[] propertyValues = flt.GetPropertValues();
        //            XmlElement res = client.UpdateFlight(Parameters.TOTOKEN, flt.GetFlightId(), propertyValues);
        //            logger.Trace(PrintXML(res.OuterXml));
        //            ProcessLinking(flt);
        //        }
        //        catch (Exception e)
        //        {
        //            logger.Error(e, "Error Updating Flight");
        //        }
        //    }
        //}
        private void SendUpdateFlightExtended(ModelFlight flt)
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    XmlElement existing = client.GetFlight(Parameters.TOTOKEN, flt.GetFlightId());
                    
                    if (existing.OuterXml.Contains("<ErrorCode>FLIGHT_NOT_FOUND</ErrorCode>"))
                    {
                        logger.Trace("Flight for Update not Found, Will try Create instead");
                        SendCreateFlightExtended(flt);
                        return;
                    }
                    FlightId fltid = flt.GetFlightId();
                    XmlElement res = client.UpdateFlightExtended(Parameters.TOTOKEN, fltid, flt.GetFlightUpdateInformation());
                    logger.Trace("Result of Update:-");
                    logger.Trace(PrintXML(res.OuterXml));
                    if (res.OuterXml.Contains("<Status xmlns=\"http://www.sita.aero/ams6-xml-api-datatypes\">Success</Status>"))
                    {
                        ProcessLinking(flt);
                    } else
                    {
                        logger.Warn("Flight Update Not Successful");
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error Updating Flight");
                }
            }
        }

        private void ProcessLinking(ModelFlight flt)
        {
            try
            {
                FlightId fltId = flt.GetFlightId();
                FlightId lfltId = flt.GetLinkedFlightId();
                {

                    using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
                    {
                        try
                        {
                            if (lfltId != null)
                            {
                                // If the incoming flight message is linked, then link the flights
                                XmlElement res = client.LinkFlights(Parameters.TOTOKEN, fltId, lfltId);
                                logger.Trace("Result of Linking Operation:-");
                                logger.Trace(PrintXML(res.OuterXml));
                            }
                            else
                            {
                                // If the incoming flight message is not linked, then unlink the flight
                                XmlElement res = client.UnlinkFlight(Parameters.TOTOKEN, fltId);
                                logger.Trace("Result of UnLinking Operation:-");
                                logger.Trace(PrintXML(res.OuterXml));
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Error Processing Lininking for flight");
                        }
                    }
                }
            } catch(Exception e)
            {
                logger.Error(e,"Error is Linking/Unlinking Operation");
                logger.Error(e.Message);
            }
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            mStream.Close();
            writer.Close();

            return result;
        }
    }
}
