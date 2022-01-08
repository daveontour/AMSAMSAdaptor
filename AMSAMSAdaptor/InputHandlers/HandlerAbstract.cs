using NLog;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AMSAMSAdaptor
{
    public abstract class HandlerAbstract : IInputMessageHandler, IDisposable
    {
        public Supervisor supervisor;
        protected readonly Logger logger = LogManager.GetLogger("consoleLogger");
        protected XmlNamespaceManager nsmgr;
        protected XmlNode node;

        protected XmlNode config;
        protected List<string> passFilters = new List<string>();
        protected List<string> noPassFilters = new List<string>();
        protected List<string> transformerClass = new List<string>();
        protected List<ITransformer> transformers = new List<ITransformer>();

        private string passFilterType { get; set; } = "and";
        private string nopassFilterType { get; set; } = "and";

        public abstract string MessageName { get; }

        public virtual string HandlerName
        { get { return this.GetType().Name; } }

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

            XmlNode PassFiltersNode = config.SelectSingleNode($"./PassFilters");
            XmlNode NoPassFiltersNode = config.SelectSingleNode($"./NoPassFilters");

            if (PassFiltersNode != null)
            {
                passFilterType = PassFiltersNode.Attributes["type"].Value;
                foreach (XmlNode node in PassFiltersNode.SelectNodes($"./PassFilter"))
                {
                    bool.TryParse(node.Attributes["enabled"]?.Value, out bool enabled);

                    if (!enabled)
                    {
                        continue;
                    }
                    passFilters.Add(node.InnerText);
                }
            }
            if (NoPassFiltersNode != null)
            {
                nopassFilterType = NoPassFiltersNode.Attributes["type"].Value;
                foreach (XmlNode node in NoPassFiltersNode.SelectNodes($"./NoPassFilter"))
                {
                    bool.TryParse(node.Attributes["enabled"]?.Value, out bool enabled);

                    if (!enabled)
                    {
                        continue;
                    }
                    noPassFilters.Add(node.InnerText);
                }
            }
            foreach (XmlNode node in config.SelectNodes($"./Transformer"))
            {
                string className = node.Attributes["class"].Value;

                bool.TryParse(node.Attributes["enabled"]?.Value, out bool enabled);

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
            this.node = node;
            nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
            nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");
            nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");
            if (!CheckHandleMessage())
            {
                logger.Trace("Message did not pass filtering criteria");
                logger.Trace(node.PrintXML());
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
                        supervisor.SendFlightMessage((ModelFlight)obj, action);
                    }
                }
            }
            else
            {
                logger.Warn("Message was null after passing through message transformers");
                logger.Warn(node.PrintXML());
            }
        }

        public bool CheckHandleMessage()
        {
            if (passFilters.Count != 0)
            {
                if (passFilterType == "and")
                {
                    foreach (string pass in passFilters)
                    {
                        if (node.SelectSingleNode(pass, nsmgr) == null)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    // For an "or" operation
                    foreach (string pass in passFilters)
                    {
                        if (node.SelectSingleNode(pass, nsmgr) != null)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            if (noPassFilters.Count != 0)
            {
                if (nopassFilterType == "or")
                {
                    foreach (string pass in noPassFilters)
                    {
                        if (node.SelectSingleNode(pass, nsmgr) != null)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    foreach (string pass in noPassFilters)
                    {
                        if (node.SelectSingleNode(pass, nsmgr) == null)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            return true;
        }
    }
}