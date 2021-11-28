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
        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("consoleLogger");
        private List<ReMapper> _remppers = new List<ReMapper>();
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
            ModelFlight fl = (ModelFlight)input;
            Dictionary<string, FlightPropertyValue> newProperties = DeepClone(fl.FlightProperties);
            foreach (ReMapper mapper in _remppers)
            {
                try
                {
                    if (mapper.IsAlternateProperty)
                    {
                        newProperties[mapper.PropertyName].Value = fl.FlightProperties[mapper.AltrernatePropertyName].Value;
                        newProperties[mapper.PropertyName].PropertyCodeContext = fl.FlightProperties[mapper.AltrernatePropertyName].PropertyCodeContext;
                        newProperties[mapper.PropertyName].PropertyCodeContextSpecified = fl.FlightProperties[mapper.AltrernatePropertyName].PropertyCodeContextSpecified;
                    }
                    if (mapper.IsFixedValue)
                    {
                        newProperties[mapper.PropertyName].Value = mapper.PropertyFixedValue;
                    }
                    if (mapper.IsDatetime)
                    {
                        newProperties[mapper.PropertyName].Value = DateTime.Now.ToString(mapper.DatetimeFormat);
                    }
                }
                catch (KeyNotFoundException)
                {
                    logger.Warn($"Error in TransformerReMapper. Property Not Found in incoming message. Key was {mapper.PropertyName}");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error in TransformerReMapper. {ex.Message}. Key was {mapper.PropertyName}");
                }
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
