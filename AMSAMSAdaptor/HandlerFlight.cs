using System;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class HandlerFlight : HandlerAbstract
    {
        public override string MessageName { get; } = "Flight";
        public override string HandlerName { get; } = "HandlerFlight";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelFlight flt = new ModelFlight(node);
            foreach (ITransformer transformer in transformers)
            {
                flt = (ModelFlight)transformer.Transform(flt);
            }

            if (flt != null)
            {
                supervisor.SendFlightMessage(flt, "http://www.sita.aero/ams6-xml-api-webservice/IAMSIntegrationService/UpdateFlight");
            } else
            {
                logger.Warn("Flight Update Message was null after passing through message transformers");
            }
        }
    }
}
