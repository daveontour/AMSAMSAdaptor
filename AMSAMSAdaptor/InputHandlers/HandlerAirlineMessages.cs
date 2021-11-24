using System;
using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerAirlineUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "AirlineUpdatedNotification";
        public override string HandlerName { get; } = "HandlerAirlineUpdate";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAirline apt = new ModelAirline(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAirline)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "UpdateAirline");
            } else
            {
                logger.Warn("Airline Update Message was null after passing through message transformers");
            }
        }
    }
    public class HandlerAirlineCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "AirlineCreatedNotification";
        public override string HandlerName { get; } = "HandlerAirlineCreate";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAirline apt = new ModelAirline(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAirline)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "CreateAirline");
            }
            else
            {
                logger.Warn("Airline Create Message was null after passing through message transformers");
            }
        }
    }
    public class HandlerAirlineDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "AirlineDeletedNotification";
        public override string HandlerName { get; } = "HandlerAirlineDelete";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAirline apt = new ModelAirline(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAirline)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "DeleteAirline");
            }
            else
            {
                logger.Warn("Airline Delete Message was null after passing through message transformers");
            }
        }
    }

    internal class HandlerAirline : HandlerAbstract
    {
        public override string MessageName { get; } = "Airline";
        public override string HandlerName { get; } = "HandlerAirline";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAirline flt = new ModelAirline(node);
            foreach (ITransformer transformer in transformers)
            {
                flt = (ModelAirline)transformer.Transform(flt);
            }

            if (flt != null)
            {
                supervisor.SendBaseDataMessage(flt, "UpdateAirport");
            }
            else
            {
                logger.Warn("Airline Update Message was null after passing through message transformers");
            }
        }
    }
}

