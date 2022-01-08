using System.Collections.Generic;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerFlightRemoveProperties : ITransformer
    {
        private readonly List<string> _properties = new List<string>();

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

            ModelFlight fl = (ModelFlight)input;

            foreach (string property in _properties)
            {
                if (property != null)
                    fl.FlightProperties.Remove(property);
            }
            return fl;
        }
    }
}