﻿using System;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class HandlerFlightDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "FlightDeletedNotification";
        public override string HandlerName { get; } = "HandlerFlightDelete";
        public override void SetSupervisor(Supervisor supervisor, XmlDocument configDoc)
        {
            base.SetSupervisor(supervisor, configDoc);
            
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
                supervisor.SendSoapMessage(flt.GetDeleteSoapMessage(), "http://www.sita.aero/ams6-xml-api-webservice/IAMSIntegrationService/UpdateFlight");
            } else
            {
                logger.Warn("Flight Delete Message was null after passing through message transformers");
            }
        }
    }
}