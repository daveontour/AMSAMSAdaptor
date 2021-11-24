using System;
using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerAirportUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "AirportUpdatedNotification";
        public override string HandlerName { get; } = "HandlerAirportUpdate";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAirport apt = new ModelAirport(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAirport)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "UpdateAirport");
            } else
            {
                logger.Warn("Airport Update Message was null after passing through message transformers");
            }
        }
    }
    public class HandlerAirportCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "AirportCreatedNotification";
        public override string HandlerName { get; } = "HandlerAirportCreate";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAirport apt = new ModelAirport(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAirport)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "CreateAirport");
            }
            else
            {
                logger.Warn("Airport Create Message was null after passing through message transformers");
            }
        }
    }
    public class HandlerAirportDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "AirportDeletedNotification";
        public override string HandlerName { get; } = "HandlerAirportDelete";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAirport apt = new ModelAirport(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAirport)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "DeleteAirport");
            }
            else
            {
                logger.Warn("Airport Delete Message was null after passing through message transformers");
            }
        }
    }

    internal class HandlerAirport : HandlerAbstract
    {
        public override string MessageName { get; } = "Airport";
        public override string HandlerName { get; } = "HandlerAirport";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAirport flt = new ModelAirport(node);
            foreach (ITransformer transformer in transformers)
            {
                flt = (ModelAirport)transformer.Transform(flt);
            }

            if (flt != null)
            {
                supervisor.SendBaseDataMessage(flt, "UpdateAirport");
            }
            else
            {
                logger.Warn("Airport Update Message was null after passing through message transformers");
            }
        }
    }
}

