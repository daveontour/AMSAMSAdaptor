using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerFlightAddProperties : ITransformer
    {
        private List<ReMapper> _remppers = new List<ReMapper>();
        public void SetConfig(XmlNode configNode)
        {
            foreach (XmlNode property in configNode.SelectNodes(".//Property"))
            {
                ReMapper reMapper = new ReMapper() {
                    PropertyName = property.Attributes["propertyName"]?.Value,
                    PropertyFixedValue = property.Attributes["value"]?.Value,
                    DatetimeFormat = property.Attributes["dateTimeFormat"]?.Value,
                    Type = property.Attributes["type"]?.Value,
                };
                _remppers.Add(reMapper);
            }
        }

        public object Transform(object input)
        {
            FlightModel fl = (FlightModel)input;
            foreach(ReMapper mapper in _remppers)
            {
                string value = null;
                if (mapper.IsFixedValue)
                {
                    value = mapper.PropertyFixedValue;
                }
                if (mapper.IsDatetime)
                {
                    value = DateTime.Now.ToString(mapper.DatetimeFormat);
                }

                PropertyValue pv = new PropertyValue() {
                    PropertyName = mapper.PropertyName,
                    Value = value
                };
                fl.FlightProperties.Add(pv.PropertyName, pv);
            }
            return fl;
        }
    }

    public class ReMapper
    {
        public string Type { get; set;}
        public string PropertyName { get; set; }
        public bool IsFixedValue { get { return (Type == "fixed") ; }  }
        public bool IsDatetime { get { return (Type == "dateTime"); } }
        public bool IsAlternateProperty { get { return (Type == "alternateProperty"); } }
        public string AltrernatePropertyName { get; set; }
        public string DatetimeFormat { get; set; }
        public string PropertyFixedValue { get; set; }
    }
}
