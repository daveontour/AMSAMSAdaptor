using System;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class UpdateFlightHandler : HandlerAbstract, IDisposable
    {
        public override string MessageName { get; } = "FlightUpdatedNotification";
        public override void SetSupervisor(Supervisor supervisor, XmlDocument configDoc)
        {
            base.SetSupervisor(supervisor, configDoc);
            supervisor.FlightUpdatedTrigger += HandleMessage;
        }
        public override void Dispose()
        {
            supervisor.FlightUpdatedTrigger -= HandleMessage;
        }

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            FlightModel flt = new FlightModel(node);
            foreach (ITransformer transformer in transformers)
            {
                flt = (FlightModel)transformer.Transform(flt);
            }

            if (flt != null)
            {
                supervisor.SendSoapMessage(flt.GetUpdateSoapMessage(), "http://www.sita.aero/ams6-xml-api-webservice/IAMSIntegrationService/UpdateFlight");
            } else
            {
                logger.Warn("Flight Update Message was null after passing through message transformers");
            }
        }
    }
}
