using System;
using System.Collections.Generic;
using System.Xml;

namespace AMSAMSAdaptor
{
    public abstract class HandlerAbstract : IInputMessageHandler, IDisposable
    {

        public Supervisor supervisor;
        public readonly NLog.Logger logger = NLog.LogManager.GetLogger("consoleLogger");
        public XmlNamespaceManager nsmgr;
        private XmlNode node;


        protected XmlNode config;
        protected List<string> passFilters = new List<string>();
        protected List<string> noPassFilters = new List<string>();
        protected List<string> transformerClass = new List<string>();
        protected List<ITransformer> transformers = new List<ITransformer>();

        public abstract string MessageName { get; }
        public virtual string HandlerName { get { return this.GetType().Name; } }
        public abstract string HandlerAction { get; }
        public abstract string HandlerModel { get; }
        public virtual string HandlerDestination { get; } = "BaseDataDistributor";
        public virtual void Dispose()
        {
            supervisor.Dispatchers[MessageName].TriggerFire -= HandleMessage;
        }

        public virtual string GetMessageName()
        {
            return MessageName;
        }
        public HandlerAbstract(Supervisor supervisor, XmlNode config)
        {
            this.supervisor = supervisor;
            this.config = config;

            foreach (XmlNode node in config.SelectNodes($"./PassFilter"))
            {
                passFilters.Add(node.InnerText);
            }
            foreach (XmlNode node in config.SelectNodes($"./NoPassFilter"))
            {
                noPassFilters.Add(node.InnerText);
            }
            foreach (XmlNode node in config.SelectNodes($"./Transformer"))
            {
                string className = node.Attributes["class"].Value;

                bool enabled = true;
                bool.TryParse(node.Attributes["enabled"]?.Value, out enabled); 
                
                if (!enabled)
                {
                    continue;
                }
                XmlNode configNode = node.SelectSingleNode(".//TransformerConfig");
                Type t = Type.GetType($"AMSAMSAdaptor.{className}");

                ITransformer transformer = (ITransformer)Activator.CreateInstance(t);
                transformer.SetConfig(configNode);
                transformers.Add(transformer);
            }

            supervisor.Dispatchers[MessageName].TriggerFire += HandleMessage;
        }

        public virtual void HandleMessage(XmlNode node)
        {

            if (MessageName == "Aircraft")
            {
                Console.WriteLine();
            }
            this.node = node;
            nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
            nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");
            nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");
            if (!CheckHandleMessage())
            {
                logger.Warn("Message did not pass filtering criteria");
                return;
            }
            // Pass the message through all the transformers
            Type t = Type.GetType(HandlerModel);
            object obj = Activator.CreateInstance(t, node);
            foreach (ITransformer transformer in transformers)
            {
                obj = transformer.Transform(obj);
            }

            if (obj != null)
            {
                if (HandlerDestination == "BaseDataDistributor")
                {
                    foreach (string action in HandlerAction.Split(','))
                    {
                        supervisor.SendBaseDataMessage((ModelBase)obj, action);
                    }
                }
                if (HandlerDestination == "FlightDataDistributor")
                {
                    foreach (string action in HandlerAction.Split(','))
                    {
                        supervisor.SendFlightMessage((ModelFlight)obj, HandlerAction);
                    }
                }
            }
            else
            {
                logger.Warn("Message was null after passing through message transformers");
            }
        }

        public bool CheckHandleMessage()
        {
            if (passFilters.Count != 0)
            {
                foreach (string pass in passFilters)
                {
                    if (node.SelectSingleNode(pass, nsmgr) != null)
                    {
                        return true;
                    }
                }
                return false;
            }

            if (noPassFilters.Count != 0)
            {
                foreach (string pass in noPassFilters)
                {
                    if (node.SelectSingleNode(pass, nsmgr) != null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
