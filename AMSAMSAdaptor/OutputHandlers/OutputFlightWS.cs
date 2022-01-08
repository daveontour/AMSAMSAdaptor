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
        private static readonly NLog.Logger emailLogger = NLog.LogManager.GetLogger("emailLogger");

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
                        string errorMessage = $"Flight Create Was Not Successful for Flight {flt.ToString()}";
                        errorMessage = errorMessage + $"Message from host was: \n {res.PrintXML()} ";
                        emailLogger.Error(errorMessage);
                    }
                }
                catch (Exception e)
                {
                    string errorMessage = $"Flight Update Was Not Successful for Flight {flt.ToString()}";
                    errorMessage = errorMessage + $"Error was: {e.Message} ";
                    emailLogger.Error(errorMessage);
                }
            }
        }

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
                    logger.Trace(res.PrintXML());
                    if (res.OuterXml.Contains("<Status xmlns=\"http://www.sita.aero/ams6-xml-api-datatypes\">Success</Status>"))
                    {
                        ProcessLinking(flt);
                    }
                    else
                    {
                        string errorMessage = $"Flight Update Was Not Successful for Flight {flt.ToString()}";
                        errorMessage = errorMessage + $"Message from host was: \n {res.PrintXML()} ";
                        emailLogger.Error(errorMessage);
                    }
                }
                catch (Exception e)
                {
                    string errorMessage = $"Flight Update Was Not Successful for Flight {flt.ToString()}";
                    errorMessage = errorMessage + $"Error was: {e.Message} ";
                    emailLogger.Error(errorMessage);
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
                                logger.Trace(res.PrintXML());
                            }
                            else
                            {
                                // If the incoming flight message is not linked, then unlink the flight
                                XmlElement res = client.UnlinkFlight(Parameters.TOTOKEN, fltId);
                                logger.Trace("Result of UnLinking Operation:-");
                                logger.Trace(res.PrintXML());
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Error Processing Lininking for flight");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error is Linking/Unlinking Operation");
                logger.Error(e.Message);
            }
        }
    }
}