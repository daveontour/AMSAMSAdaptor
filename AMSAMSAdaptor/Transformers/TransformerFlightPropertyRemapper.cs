using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using NLog;

namespace AMSAMSAdaptor
{
    public class ReMapper
    {
        public string Type { get; set; }
        public string PropertyName { get; set; }
        public string AltrernatePropertyName { get; set; }
        public string DatetimeFormat { get; set; }
        public string PropertyFixedValue { get; set; }
        public string LookupPropertyName { get; set; }
        public string FileName { get; set; }
        public string IndexField { get; set; }
        public string ValueField { get; set; }
        public bool TimeOffset { get; internal set; }
        public string TimeFormat { get; internal set; }
        public string TimeReferenceProperty { get; internal set; }
        public int Offset { get; set; } = 0;

        public Dictionary<string, string> LookupDict = new Dictionary<string, string>();
    }

    internal class TransformerFlightPropertyRemapper : ITransformer
    {
        private readonly Logger logger = LogManager.GetLogger("consoleLogger");
        private readonly List<ReMapper> _remppers = new List<ReMapper>();

        public void SetConfig(XmlNode configNode)
        {
            foreach (XmlNode property in configNode.SelectNodes(".//Property"))
            {
                ReMapper reMapper = new ReMapper()
                {
                    PropertyName = property.Attributes["propertyName"]?.Value,
                    LookupPropertyName = property.Attributes["lookupPropertyName"]?.Value,
                    FileName = property.Attributes["fileName"]?.Value,
                    IndexField = property.Attributes["indexField"]?.Value,
                    ValueField = property.Attributes["valueField"]?.Value,
                    TimeFormat = property.Attributes["timeFormat"]?.Value,
                    TimeReferenceProperty = property.Attributes["referencePropertyName"]?.Value,
                    PropertyFixedValue = property.Attributes["value"]?.Value,
                    DatetimeFormat = property.Attributes["dateTimeFormat"]?.Value,
                    AltrernatePropertyName = property.Attributes["alternatePropertyName"]?.Value,
                    Type = property.Attributes["type"]?.Value,
                };

                bool.TryParse(property.Attributes["timeOffset"]?.Value, out bool timeOffest);
                reMapper.TimeOffset = timeOffest;
                int.TryParse(property.Attributes["offset"]?.Value, out int offest);
                reMapper.Offset = offest;

                if (reMapper.Type == "lookup")
                {
                    // If a Lookup propertyname is not specified, then use the property name
                    if (reMapper.LookupPropertyName == null)
                    {
                        reMapper.LookupPropertyName = reMapper.PropertyName;
                    }

                    using (TextFieldParser parser = new TextFieldParser(reMapper.FileName))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");

                        while (!parser.EndOfData)
                        {
                            try
                            {
                                string[] fields = parser.ReadFields();
                                string keyValue = fields[int.Parse(reMapper.IndexField)];
                                string valueValue = fields[int.Parse(reMapper.ValueField)];
                                reMapper.LookupDict.Add(keyValue, valueValue);
                            }
                            catch
                            {
                                // Do nothing, try the next line
                            }
                        }
                    }
                }

                _remppers.Add(reMapper);
            }
        }

        public object Transform(object input)
        {
            if (input == null) return null;

            ModelFlight fl = (ModelFlight)input;
            Dictionary<string, FlightPropertyValue> newProperties = DeepClone(fl.FlightProperties);
            foreach (ReMapper mapper in _remppers)
            {
                try
                {
                    if (mapper.Type == "lookup")
                    {
                        string newValue = LookupNewValue(mapper, fl);
                        newProperties[mapper.PropertyName].Value = newValue;
                    }
                    if (mapper.Type == "alternateField")
                    {
                        newProperties[mapper.PropertyName].Value = fl.FlightProperties[mapper.AltrernatePropertyName].Value;
                        newProperties[mapper.PropertyName].PropertyCodeContext = fl.FlightProperties[mapper.AltrernatePropertyName].PropertyCodeContext;
                        newProperties[mapper.PropertyName].PropertyCodeContextSpecified = fl.FlightProperties[mapper.AltrernatePropertyName].PropertyCodeContextSpecified;
                    }
                    if (mapper.Type == "fixed")
                    {
                        newProperties[mapper.PropertyName].Value = mapper.PropertyFixedValue;
                    }
                    if (mapper.Type == "dateTime")
                    {
                        newProperties[mapper.PropertyName].Value = DateTime.Now.AddMinutes(mapper.Offset).ToString(mapper.DatetimeFormat);
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

        private string LookupNewValue(ReMapper mapper, ModelFlight fl)
        {
            string key = fl.FlightProperties[mapper.LookupPropertyName].Value;
            if (!mapper.LookupDict.ContainsKey(key)) { return null; }
            string lookupValue = mapper.LookupDict[key];
            if (!mapper.TimeOffset)
            {
                return lookupValue;
            }

            int off = int.Parse(lookupValue);
            string referenceValue = fl.FlightProperties[mapper.TimeReferenceProperty].Value;

            DateTime t = DateTime.Parse(referenceValue);
            DateTime r = t.AddMinutes(off);
            if (mapper.TimeFormat != null)
            {
                string s = r.ToString(mapper.TimeFormat);
                return s;
            }
            else
            {
                string s = r.ToString();
                return s;
            }
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