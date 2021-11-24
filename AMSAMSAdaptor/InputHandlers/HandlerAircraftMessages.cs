using System;
using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerAircraftUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftUpdatedNotification";
        public override string HandlerName { get; } = "HandlerAircraftUpdate";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAircraft apt = new ModelAircraft(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAircraft)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "UpdateAircraft");
            } else
            {
                logger.Warn("Aircraft Update Message was null after passing through message transformers");
            }
        }
    }
    public class HandlerAircraftCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftCreatedNotification";
        public override string HandlerName { get; } = "HandlerAircraftCreate";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAircraft apt = new ModelAircraft(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAircraft)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "CreateAircraft");
            }
            else
            {
                logger.Warn("Aircraft Create Message was null after passing through message transformers");
            }
        }
    }
    public class HandlerAircraftDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftDeletedNotification";
        public override string HandlerName { get; } = "HandlerAircraftDelete";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAircraft apt = new ModelAircraft(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAircraft)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "DeleteAircraft");
            }
            else
            {
                logger.Warn("Aircraft Delete Message was null after passing through message transformers");
            }
        }
    }
    internal class HandlerAircraft : HandlerAbstract
    {
        public override string MessageName { get; } = "Aircraft";
        public override string HandlerName { get; } = "HandlerAircraft";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAircraft flt = new ModelAircraft(node);
            foreach (ITransformer transformer in transformers)
            {
                flt = (ModelAircraft)transformer.Transform(flt);
            }

            if (flt != null)
            {
                supervisor.SendBaseDataMessage(flt, "UpdateAircraft");
            }
            else
            {
                logger.Warn("Aircraft Update Message was null after passing through message transformers");
            }
        }
    }
}

