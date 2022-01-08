using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerBaseDataRemoveProperties : ITransformer
    {
        private List<string> _properties = new List<string>();

        public void SetConfig(XmlNode configNode)
        {
            foreach (XmlNode property in configNode.SelectNodes(".//Property"))
            {
                _properties.Add(property.Attributes["propertyName"]?.Value);
            }
        }

        public object Transform(object input)
        {
            if (input == null) return null;

            ModelBase fl = (ModelBase)input;

            foreach (string property in _properties)
            {
                if (property != null)
                    fl.State.Remove(property);
            }
            return fl;
        }
    }
}