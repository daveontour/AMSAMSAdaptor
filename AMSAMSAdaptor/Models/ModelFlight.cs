using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes;

namespace AMSAMSAdaptor
{
    [Serializable]
    public class FlightPropertyValue
    {
        public bool CoreProperty { get; set; } = false;
        public bool FlightIdProp { get; set; } = false;
        public string Value { get; set; }
        public string PropertyName { get; set; }

        private string propertyCodeContext = "IATA";
        public string PropertyCodeContext
        {
            get { return propertyCodeContext; }
            set
            {
                propertyCodeContext = value;
                if (value != null)
                {
                    PropertyCodeContextSpecified = true;
                }
                else
                {
                    PropertyCodeContextSpecified = false;
                }
            }
        }
        public bool PropertyCodeContextSpecified { get; set; } = false;

        public FlightPropertyValue() { }

        public FlightPropertyValue(XmlNode node)
        {
            this.PropertyName = node.Attributes["propertyName"]?.Value;
            this.PropertyCodeContext = node.Attributes["codeContext"]?.Value;
            this.Value = node.InnerText;
        }
    }

    public class ModelFlight
    {

        public XmlNamespaceManager nsmgr;
        public XmlNode node;

        protected XmlDocument configDoc;


        public Dictionary<string, FlightPropertyValue> FlightProperties { get; set; } = new Dictionary<string, FlightPropertyValue>();
        public List<string> Routes { get; set; } = new List<string>();


        public ModelFlight(XmlNode node)
        {
            this.node = node;
            nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
            nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");

            SetCoreValue("Nature", ".//amsx-datatypes:FlightId/amsx-datatypes:FlightKind", true);
            SetCoreValue("AirlineIATA", ".//amsx-datatypes:AirlineDesignator[@codeContext = 'IATA']", true);
            SetCoreValue("AirlineICAO", ".//amsx-datatypes:AirlineDesignator[@codeContext='ICAO']", true);
            SetCoreValue("FlightNumber", ".//amsx-datatypes:FlightId/amsx-datatypes:FlightNumber", true);
            SetCoreValue("ScheduledDate", ".//amsx-datatypes:FlightId/amsx-datatypes:ScheduledDate", true);
            SetCoreValue("AirportIATA", ".//amsx-datatypes:FlightId/amsx-datatypes:AirportCode[@codeContext='IATA']", true);
            SetCoreValue("AirportICAO", ".//amsx-datatypes:FlightId/amsx-datatypes:AirportCode[@codeContext='ICAO']", true);


            SetCoreValue("LinkedNature", ".//amsx-messages:Flight/amsx-datatypes:FlightState/amsx-datatypes:LinkedFlight/amsx-datatypes:FlightId/amsx-datatypes:FlightKind", true);
            SetCoreValue("LinkedAirlineIATA", ".//amsx-messages:Flight/amsx-datatypes:FlightState/amsx-datatypes:LinkedFlight/amsx-datatypes:FlightId/amsx-datatypes:AirlineDesignator[@codeContext = 'IATA']", true);
            SetCoreValue("LinkedAirlineICAO", ".//amsx-messages:Flight/amsx-datatypes:FlightState/amsx-datatypes:LinkedFlight/amsx-datatypes:FlightId/amsx-datatypes:AirlineDesignator[@codeContext='ICAO']", true);
            SetCoreValue("LinkedFlightNumber", ".//amsx-messages:Flight/amsx-datatypes:FlightState/amsx-datatypes:LinkedFlight/amsx-datatypes:FlightId/amsx-datatypes:FlightNumber", true); ;
            SetCoreValue("LinkedScheduledDate", ".//amsx-messages:Flight/amsx-datatypes:FlightState/amsx-datatypes:LinkedFlight/amsx-datatypes:FlightId/amsx-datatypes:ScheduledDate", true);
            SetCoreValue("LinkedAirportIATA", ".//amsx-messages:Flight/amsx-datatypes:FlightState/amsx-datatypes:LinkedFlight/amsx-datatypes:FlightId/amsx-datatypes:AirportCode[@codeContext='IATA']", true);
            SetCoreValue("LinkedAirportICAO", ".//amsx-messages:Flight/amsx-datatypes:FlightState/amsx-datatypes:LinkedFlight/amsx-datatypes:FlightId/amsx-datatypes:AirportCode[@codeContext='ICAO']", true);

            SetCoreValue("ScheduledTime", ".//amsx-datatypes:FlightState/amsx-datatypes:ScheduledTime");

            SetCoreValue("AircraftTypeCode", ".//amsx-datatypes:FlightState/amsx-datatypes:AircraftType/amsx-datatypes:AircraftTypeId/amsx-datatypes:AircraftTypeCode[@codeContext='IATA']");
//            SetCoreValue("AircraftTypeCodeICAO", ".//amsx-datatypes:FlightState/amsx-datatypes:AircraftType/amsx-datatypes:AircraftTypeId/amsx-datatypes:AircraftTypeCode[@codeContext='ICAO']");
            SetCoreValue("AircraftRegistration", ".//amsx-datatypes:FlightState/amsx-datatypes:Aircraft/amsx-datatypes:AircraftId/amsx-datatypes:Registration");

            foreach (XmlNode v in node.SelectNodes(".//amsx-datatypes:FlightState/amsx-datatypes:Route/amsx-datatypes:ViaPoints/amsx-datatypes:RouteViaPoint/amsx-datatypes:AirportCode[@codeContext='IATA']", nsmgr))
            {
                Routes.Add(v.InnerText);
            }
            if (Routes.Count > 0)
            {
                FlightPropertyValue pv = new FlightPropertyValue()
                {
                    Value = string.Join(",", Routes),
                    PropertyName = "Route",
                    CoreProperty = true,
                    FlightIdProp = false
                };
                FlightProperties.Add(pv.PropertyName, pv);

            }
            foreach (XmlNode v in node.SelectNodes(".//amsx-datatypes:FlightState/amsx-datatypes:Value", nsmgr))
            {
                FlightPropertyValue pv = new FlightPropertyValue(v);
                FlightProperties.Add(pv.PropertyName, pv);
            }
        }

        private void SetCoreValue(string name, string xpath, bool flightIDProp = false)
        {
            string value = node.SelectSingleNode(xpath, nsmgr)?.InnerText;
            if (value != null)
            {
                FlightPropertyValue pv = new FlightPropertyValue()
                {
                    Value = value,
                    PropertyName = name,
                    CoreProperty = true,
                    FlightIdProp = flightIDProp
                };
                FlightProperties.Add(pv.PropertyName, pv);
            }
        }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();
            foreach (FlightPropertyValue v in FlightProperties.Values)
            {
                if (!v.CoreProperty) continue;
                sb.AppendLine($"{v.PropertyName}: {v.Value}");
            }

            foreach (FlightPropertyValue v in FlightProperties.Values)
            {
                if (v.CoreProperty) continue;
                sb.AppendLine($"{v.PropertyName}: {v.Value}");
            }



            return sb.ToString();
        }

        public FlightId GetFlightId()
        {
            try
            {
                LookupCode[] ap = { };
                //if (FlightProperties.ContainsKey("AirportICAO"))
                //{
                //    LookupCode apCode = new LookupCode();
                //    apCode.codeContextField = CodeContext.ICAO;
                //    apCode.valueField = FlightProperties["AirportICAO"]?.Value;
                //    if (apCode.valueField != null)
                //    {
                //        ap = ap.Append<LookupCode>(apCode).ToArray();
                //    }
                //}
                if (FlightProperties.ContainsKey("AirportIATA"))
                {
                    LookupCode apCode2 = new LookupCode();
                    apCode2.codeContextField = CodeContext.IATA;
                    apCode2.valueField = FlightProperties["AirportIATA"].Value;
                    if (apCode2.valueField != null)
                    {
                        ap = ap.Append<LookupCode>(apCode2).ToArray();
                    }
                }

                LookupCode alCode = new LookupCode();
                alCode.codeContextField = CodeContext.IATA;
                alCode.valueField = FlightProperties["AirlineIATA"].Value;
                LookupCode[] al = { alCode };

                FlightId flightID = new FlightId();
                flightID.flightKindField = FlightProperties["Nature"].Value == "Arrival" ? FlightKind.Arrival : FlightKind.Departure;
                flightID.airportCodeField = ap;
                flightID.airlineDesignatorField = al;
                flightID.scheduledDateField = Convert.ToDateTime(FlightProperties["ScheduledDate"].Value);
                flightID.flightNumberField = FlightProperties["FlightNumber"].Value;

                return flightID;
            } catch (Exception ex)
            {
                return null;
            }
        }

        public FlightId GetLinkedFlightId()
        {
            try
            {
                if (!FlightProperties.ContainsKey("LinkedAirportIATA"))
                {
                    return null;
                }

               //LookupCode[] ap = { };
                //LookupCode apCode = new LookupCode();
                //apCode.codeContextField = CodeContext.ICAO;
                //apCode.valueField = FlightProperties["LinkedAirportICAO"].Value;
                //LookupCode[] ap = { apCode };

                LookupCode apCode2 = new LookupCode();
                apCode2.codeContextField = CodeContext.IATA;
                apCode2.valueField = FlightProperties["LinkedAirportIATA"].Value;
                LookupCode[] ap = { apCode2 };


                LookupCode alCode = new LookupCode();
                alCode.codeContextField = CodeContext.IATA;
                alCode.valueField = FlightProperties["LinkedAirlineIATA"].Value;
                LookupCode[] al = { alCode };

                FlightId flightID = new FlightId();
                flightID.flightKindField = FlightProperties["LinkedNature"].Value == "Arrival" ? FlightKind.Arrival : FlightKind.Departure;
                flightID.airportCodeField = ap;
                flightID.airlineDesignatorField = al;
                flightID.scheduledDateField = Convert.ToDateTime(FlightProperties["LinkedScheduledDate"].Value);
                flightID.flightNumberField = FlightProperties["LinkedFlightNumber"].Value;

                return flightID;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public FlightUpdateInformation GetFlightUpdateInformation()
        {
            FlightUpdateInformation flightUpdateInformation = new FlightUpdateInformation();

            flightUpdateInformation.updateField = GetPropertValues();
            flightUpdateInformation.tableValueUpdateField = GetTableValue();
            flightUpdateInformation.activityUpdateField = GetActivityUpdate();
            flightUpdateInformation.eventUpdateField = GetEventUpdate();

            return flightUpdateInformation;
        }


        public PropertyValue[] GetPropertValues()
        {

            PropertyValue[] val = { };

            foreach (FlightPropertyValue liPV in FlightProperties.Values)
            {
                if (liPV.FlightIdProp) continue;
                PropertyValue pv = new PropertyValue()
                {
                    propertyNameField = liPV.PropertyName,
                    valueField = liPV.Value,
                    codeContextField = (liPV.PropertyCodeContext == "ICAO") ? CodeContext.ICAO : CodeContext.IATA
                };

                val = val.Append(pv).ToArray();
            }

            return val;
        }

        public TableValueUpdate[] GetTableValue()
        {

            TableValueUpdate[] val = { };

            foreach (XmlNode table in node.SelectNodes("//amsx-datatypes:TableValue", nsmgr))
            {
                TableValueUpdate tableValueUpdate = new TableValueUpdate();
                tableValueUpdate.propertyNameField = table.Attributes["propertyName"]?.Value;
                TableRow[] rows = { };

                foreach (XmlNode row in table.SelectNodes(".//amsx-datatypes:Row", nsmgr))
                {
                    TableRow tr = new TableRow();
                    PropertyValue[] valueField = { };

                    foreach (XmlNode field in row.SelectNodes(".//amsx-datatypes:Value", nsmgr))
                    {
                       PropertyValue pv = new PropertyValue();

                        pv.propertyNameField = field.Attributes["propertyName"]?.Value;
                        pv.valueField = field.InnerText;

                        valueField = valueField.Append(pv).ToArray();
                    }

                    tr.valueField = valueField;

                    rows = rows.Append(tr).ToArray();
                }

                tableValueUpdate.rowField = rows;

                val = val.Append(tableValueUpdate).ToArray();
            }

            return val;
        }
        public EventUpdate[] GetEventUpdate()
        {
            EventUpdate[] val = { };
            foreach (XmlNode e in node.SelectNodes("//amsx-datatypes:Event", nsmgr))
            {
                EventUpdate eu = new EventUpdate();
                eu.codeField = e.Attributes["code"]?.Value;
                PropertyValue[] valueField = { };
                foreach (XmlNode v in e.SelectNodes(".///amsx-datatypes:Value"))
                {
                    PropertyValue pv = new PropertyValue();
                    pv.propertyNameField = v.Attributes["propertyName"]?.Value;
                    pv.valueField = v.InnerText;
                    valueField = valueField.Append(pv).ToArray();
                }
                eu.updateField = valueField;
                val = val.Append(eu).ToArray();
            }

            return val;
        }
        public ActivityUpdate[] GetActivityUpdate()
        {
            ActivityUpdate[] val = { };
            foreach (XmlNode e in node.SelectNodes("//amsx-datatypes:Activity", nsmgr))
            {
                ActivityUpdate eu = new ActivityUpdate();
                eu.codeField = e.Attributes["code"]?.Value;
                PropertyValue[] valueField = { };
                foreach (XmlNode v in e.SelectNodes(".///amsx-datatypes:Value"))
                {
                    PropertyValue pv = new PropertyValue();
                    pv.propertyNameField = v.Attributes["propertyName"]?.Value;
                    pv.valueField = v.InnerText;
                    valueField = valueField.Append(pv).ToArray();
                }
                eu.updateField = valueField;
                val = val.Append(eu).ToArray();
            }
            return val;
        }
    }
}
