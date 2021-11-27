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
                SendCreateFlight(flt);
                supervisor.SendPostFlightMessage(flt, action);
            }
            if (action.Contains("UpdateFlight"))
            {
                logger.Info($"Update Flight: {flt.FlightProperties["AirlineIATA"]?.Value} {flt.FlightProperties["FlightNumber"]?.Value}");
                //try{flt.FlightProperties.Remove("ScheduledTime");
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
                    XmlElement res = client.DeleteFlight(Parameters.TOTOKEN, flt.GetFlightId());
                    logger.Trace(res.OuterXml);

                   
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error Deleting Flight");
                }
            }
        }
        private void SendCreateFlight(ModelFlight flt)
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes.PropertyValue[] propertyValues = flt.GetPropertValues();
                    XmlElement res = client.CreateFlight(Parameters.TOTOKEN, flt.GetFlightId(), propertyValues);
                    logger.Trace(res.OuterXml);
                    ProcessLinking(flt);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error Creating Flight");
                }
            }
        }
        private void SendUpdateFlight(ModelFlight flt)
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes.PropertyValue[] propertyValues = flt.GetPropertValues();
                    XmlElement res = client.UpdateFlight(Parameters.TOTOKEN, flt.GetFlightId(), propertyValues);
                    logger.Trace(res.OuterXml);
                    ProcessLinking(flt);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error Updating Flight");
                }
            }
        }
        private void SendUpdateFlightExtended(ModelFlight flt)
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    XmlElement res = client.UpdateFlightExtended(Parameters.TOTOKEN, flt.GetFlightId(), flt.GetFlightUpdateInformation());

                    logger.Trace(res.OuterXml);
                    ProcessLinking(flt);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error Updating Flight");
                }
            }
        }

        private void ProcessLinking(ModelFlight flt)
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
                            logger.Trace(res.OuterXml);
                        }
                        else
                        {
                            // If the incoming flight message is not linked, then unlink the flight
                            XmlElement res = client.UnlinkFlight(Parameters.TOTOKEN, fltId);
                            logger.Trace(res.OuterXml);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Error Processing Lininking for flight");
                    }
                }
            }
        }
    }
}
