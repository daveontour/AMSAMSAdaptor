using System;
using System.Collections.Generic;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerFlightAddProperties : ITransformer
    {
        private readonly List<ReMapper> _remppers = new List<ReMapper>();

        public void SetConfig(XmlNode configNode)
        {
            foreach (XmlNode property in configNode.SelectNodes(".//Property"))
            {
                ReMapper reMapper = new ReMapper()
                {
                    PropertyName = property.Attributes["propertyName"]?.Value,
                    PropertyFixedValue = property.Attributes["value"]?.Value,
                    DatetimeFormat = property.Attributes["dateTimeFormat"]?.Value,
                    AltrernatePropertyName = property.Attributes["alternatePropertyName"]?.Value,
                    Type = property.Attributes["type"]?.Value,
                };
                _remppers.Add(reMapper);
            }
        }

        public object Transform(object input)
        {
            if (input == null) return null;

            ModelFlight fl = (ModelFlight)input;
            foreach (ReMapper mapper in _remppers)
            {
                string value = null;
                if (mapper.Type == "fixed")
                {
                    value = mapper.PropertyFixedValue;
                }
                if (mapper.Type == "dateTime")
                {
                    value = DateTime.Now.ToString(mapper.DatetimeFormat);
                }
                if (mapper.Type == "alternateField")
                {
                    value = fl.FlightProperties[mapper.AltrernatePropertyName].Value;
                }

                FlightPropertyValue pv = new FlightPropertyValue()
                {
                    PropertyName = mapper.PropertyName,
                    Value = value
                };
                fl.FlightProperties.Add(pv.PropertyName, pv);
            }
            return fl;
        }
    }
}