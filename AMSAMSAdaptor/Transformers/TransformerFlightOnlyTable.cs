using System.Collections.Generic;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerFlightOnlyTable : ITransformer
    {
        private List<string> _tables = new List<string>();

        public void SetConfig(XmlNode configNode)
        {
            foreach (XmlNode property in configNode.SelectNodes(".//Property"))
            {
                _tables.Add(property.Attributes["tableName"]?.Value);
            }
        }

        public object Transform(object input)
        {
            if (input == null) return null;

            ModelFlight fl = (ModelFlight)input;
            foreach (XmlNode childNode in fl.node.SelectSingleNode($".//amsx-datatypes:TableValue", fl.nsmgr))
            {
                if (childNode != null)
                {
                    if (!_tables.Contains(childNode.Attributes["propertyName"]?.Value))
                    {
                        childNode.ParentNode.RemoveChild(childNode);
                    }
                }
            }
            return fl;
        }

        internal class TransformerFlightRemoveActivities : ITransformer
        {
            public void SetConfig(XmlNode configNode)
            {
            }

            public object Transform(object input)
            {
                ModelFlight fl = (ModelFlight)input;

                XmlNode childNode = fl.node.SelectSingleNode($".//amsx-datatypes:Activity", fl.nsmgr);
                if (childNode != null)
                {
                    childNode.ParentNode.RemoveChild(childNode);
                }

                return fl;
            }
        }

        internal class TransformerFlightRemoveEvents : ITransformer
        {
            public void SetConfig(XmlNode configNode)
            {
            }

            public object Transform(object input)
            {
                ModelFlight fl = (ModelFlight)input;

                XmlNode childNode = fl.node.SelectSingleNode($".//amsx-datatypes:Event", fl.nsmgr);
                if (childNode != null)
                {
                    childNode.ParentNode.RemoveChild(childNode);
                }
                return fl;
            }
        }
    }
}