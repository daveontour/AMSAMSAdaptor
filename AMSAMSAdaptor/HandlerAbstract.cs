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


        protected XmlDocument configDoc;
        protected List<string> passFilters = new List<string>();
        protected List<string> noPassFilters = new List<string>();
        protected List<string> transformerClass = new List<string>();
        protected List<ITransformer> transformers = new List<ITransformer>();

        public virtual string MessageName { get; }
        public virtual string HandlerName { get; }

        public virtual void Dispose()
        {
            supervisor.Dispatchers[MessageName].TriggerFire -= HandleMessage;
        }

        public virtual string GetMessageName()
        {
            return MessageName;
        }

        public virtual void SetSupervisor(Supervisor supervisor, XmlDocument configDoc)
        {
            this.supervisor = supervisor;
            this.configDoc = configDoc;

            foreach (XmlNode node in configDoc.SelectNodes($"//{HandlerName}/PassFilter"))
            {
                passFilters.Add(node.InnerText);
            }
            foreach (XmlNode node in configDoc.SelectNodes($"//{HandlerName}/NoPassFilter"))
            {
                noPassFilters.Add(node.InnerText);
            }
            foreach (XmlNode node in configDoc.SelectNodes($"//{HandlerName}/Transformer"))
            {
                string className = node.SelectSingleNode(".//TransformerClass")?.InnerText;
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
            nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");
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
