﻿using Microsoft.VisualBasic.FileIO;
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
    internal class TransformerFlightLookupReMapper : ITransformer
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
                    FileName = property.Attributes["fileName"]?.Value,
                    IndexField = property.Attributes["indexField"]?.Value,
                    ValueField = property.Attributes["valueField"]?.Value
                };

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
                _remppers.Add(reMapper);
            }
        }

        public object Transform(object input)
        {
            ModelFlight fl = (ModelFlight)input;
            Dictionary<string, PropertyValue> newProperties = DeepClone(fl.FlightProperties);
            foreach (ReMapper mapper in _remppers)
            {
                try
                {
                    string newValue = LookupNewValue(mapper,fl);
                    newProperties[mapper.PropertyName].Value = newValue;
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
            string key = fl.FlightProperties[mapper.PropertyName].Value;
            if (!mapper.LookupDict.ContainsKey(key)) { return null; }
            return mapper.LookupDict[key];  
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