using System.Collections.Generic;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerFlightOnlyProperties : ITransformer
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

            //Make a copy of the keys to iterate agains to avoid iteration modification problems
            List<string> properties = new List<string>(fl.FlightProperties.Keys);

            foreach (string property in properties)
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