using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerFlightPropertyRemapper : ITransformer
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

                bool timeOffest = false;
                bool.TryParse(property.Attributes["timeOffset"]?.Value, out timeOffest);

                reMapper.TimeOffset = timeOffest;

                if (reMapper.Type == "lookup") {
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

            if (mapper.TimeFormat != null)
            {
                return DateTime.Parse(referenceValue).AddMinutes(off).ToString(mapper.TimeFormat);
            }
            else
            {
                return DateTime.Parse(referenceValue).AddMinutes(off).ToString();
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

        //public static string PrintXML(string xml)
        //{
        //    string result = "";

        //    MemoryStream mStream = new MemoryStream();
        //    XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
        //    XmlDocument document = new XmlDocument();

        //    try
        //    {
        //        // Load the XmlDocument with the XML.
        //        document.LoadXml(xml);

        //        writer.Formatting = Formatting.Indented;

        //        // Write the XML into a formatting XmlTextWriter
        //        document.WriteContentTo(writer);
        //        writer.Flush();
        //        mStream.Flush();

        //        // Have to rewind the MemoryStream in order to read
        //        // its contents.
        //        mStream.Position = 0;

        //        // Read MemoryStream contents into a StreamReader.
        //        StreamReader sReader = new StreamReader(mStream);

        //        // Extract the text from the StreamReader.
        //        string formattedXml = sReader.ReadToEnd();

        //        result = formattedXml;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }

        //    mStream.Close();
        //    writer.Close();

        //    return result;
        //}
    }
}
