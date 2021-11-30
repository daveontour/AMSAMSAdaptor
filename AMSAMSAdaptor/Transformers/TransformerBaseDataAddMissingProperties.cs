using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerBaseDataAddMissingProperties : ITransformer
    {
        struct Replacement
        {
            public string property;
            public string value;
        }

        private List <Replacement> _properties = new List<Replacement>();
        
        public void SetConfig(XmlNode configNode)
        {
            foreach (XmlNode property in configNode.SelectNodes(".//Property"))
            {
                Replacement r;
                r.property = property.Attributes["propertyName"]?.Value;
                r.value = property.Attributes["value"]?.Value;
                _properties.Add(r);
            }
        }

        public object Transform(object input)
        {
            ModelBase fl = (ModelBase)input;

            foreach(Replacement r in _properties)
            {
                if (!fl.KeyState.ContainsKey(r.property))
                {
                    fl.KeyState.Add(r.property, r.value);
                }
                if (fl.KeyState[r.property] == null)
                {
                    fl.KeyState[r.property] = r.value;
                }
            }
            return fl;
        }
    }
}
