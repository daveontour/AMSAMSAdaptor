using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerFlightOnlyProperties : ITransformer
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
            ModelFlight fl = (ModelFlight)input;
            foreach(string property in fl.FlightProperties.Keys)
            {
                if (_properties.Contains(property))
                {
                    continue;
                }
                if (fl.FlightProperties[property].CoreProperty)
                {
                    continue;
                } 
                fl.FlightProperties.Remove(property);
            }
            return fl;
        }
    }
}
