using System;
using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerAircraftTypeUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftTypeUpdatedNotification";
        public override string HandlerName { get; } = "HandlerAircraftTypeUpdate";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAircraftType apt = new ModelAircraftType(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAircraftType)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "UpdateAircraftType");
            } else
            {
                logger.Warn("AircraftType Update Message was null after passing through message transformers");
            }
        }
    }
    public class HandlerAircraftTypeCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftTypeCreatedNotification";
        public override string HandlerName { get; } = "HandlerAircraftTypeCreate";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAircraftType apt = new ModelAircraftType(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAircraftType)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "CreateAircraftType");
            }
            else
            {
                logger.Warn("AircraftType Create Message was null after passing through message transformers");
            }
        }
    }
    public class HandlerAircraftTypeDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftTypeDeletedNotification";
        public override string HandlerName { get; } = "HandlerAircraftTypeDelete";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAircraftType apt = new ModelAircraftType(node);
            foreach (ITransformer transformer in transformers)
            {
                apt = (ModelAircraftType)transformer.Transform(apt);
            }

            if (apt != null)
            {
                supervisor.SendBaseDataMessage(apt, "DeleteAircraftType");
            }
            else
            {
                logger.Warn("AircraftType Delete Message was null after passing through message transformers");
            }
        }
    }
    internal class HandlerAircraftType : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftType";
        public override string HandlerName { get; } = "HandlerAircraftType";

        public override void HandleMessage(XmlNode node)
        {
            base.HandleMessage(node);
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering");
                return;

            }

            // Pass the message through all the transformers
            ModelAircraftType flt = new ModelAircraftType(node);
            foreach (ITransformer transformer in transformers)
            {
                flt = (ModelAircraftType)transformer.Transform(flt);
            }

            if (flt != null)
            {
                supervisor.SendBaseDataMessage(flt, "UpdateAircraftType");
            }
            else
            {
                logger.Warn("AircraftType Update Message was null after passing through message transformers");
            }
        }
    }
}

