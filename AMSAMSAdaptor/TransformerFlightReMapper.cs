using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerFlightReMapper : ITransformer
    {
        private XmlNode config;
        private List<ReMapper> _remppers = new List<ReMapper>();
        public void SetConfig(XmlNode configNode)
        {
            foreach (XmlNode property in configNode.SelectNodes(".//Property"))
            {
                ReMapper reMapper = new ReMapper()
                {
                    PropertyName = property.Attributes["propertyName"]?.Value,
                    AltrernatePropertyName = property.Attributes["alternatePropertyName"]?.Value,
                };
                _remppers.Add(reMapper);
            }
        }

        public object Transform(object input)
        {
            FlightModel fl = (FlightModel)input;
            Dictionary<string, PropertyValue> newProperties = DeepClone(fl.FlightProperties);
            foreach (ReMapper mapper in _remppers)
            {
                newProperties[mapper.PropertyName].Value = fl.FlightProperties[mapper.AltrernatePropertyName].Value;
                newProperties[mapper.PropertyName].PropertyCodeContext = fl.FlightProperties[mapper.AltrernatePropertyName].PropertyCodeContext;
                newProperties[mapper.PropertyName].PropertyCodeContextSpecified = fl.FlightProperties[mapper.AltrernatePropertyName].PropertyCodeContextSpecified;
            }

            fl.FlightProperties = newProperties;
            return fl;
        }

        public T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
