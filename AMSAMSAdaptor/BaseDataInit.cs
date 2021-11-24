﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class BaseDataInit
    {
        private XmlNode config;
        private Supervisor supervisor;
        private BasicHttpBinding binding;
        private EndpointAddress address;
        private bool initACTypes = false;
        private bool initAC = false;
        private bool initAirports = false;
        private bool initAirlines = false;

        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("consoleLogger");

        public BaseDataInit(XmlNode config, Supervisor supervisor)
        {
            this.config = config;
            this.supervisor = supervisor;

            bool.TryParse(config.SelectSingleNode(".//InitAircraftTypes")?.InnerText, out initACTypes);
            bool.TryParse(config.SelectSingleNode(".//InitAircraft")?.InnerText, out initAC);
            bool.TryParse(config.SelectSingleNode(".//InitAirports")?.InnerText, out initAirports);
            bool.TryParse(config.SelectSingleNode(".//InitAirlines")?.InnerText, out initAirlines);

            binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 20000000,
                MaxBufferSize = 20000000,
                MaxBufferPoolSize = 20000000
            };
            address = new EndpointAddress(Parameters.TO_AMS_WEB_SERVICE_URI);
        }

        public void Sync()
        {
            if (initACTypes) InitAircraftTypes();
            if (initAC) InitAircrafts();
            if (initAirports) InitAirports();
            if (initAirlines) InitAirlines();
        }

        public void InitAirports()
        {
            logger.Info("Populating Airports");
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {

                try
                {
                    XmlElement res = client.GetAirports(Parameters.FROMTOKEN);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
                    nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");

                    XmlNodeList fls = res.SelectNodes("//amsx-datatypes:Airport", nsmgr);
                    foreach (XmlNode fl in fls)
                    {
                        logger.Warn("Startup Airport Update");
                        supervisor.ProcessMessage(fl.OuterXml);
                    }

                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
            logger.Info("Populating Airports Complete");

        }

        public void InitAircrafts()
        {
            logger.Info("Populating Aircraft");

            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    XmlElement res = client.GetAircrafts(Parameters.FROMTOKEN);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
                    nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");

                    XmlNodeList fls = res.SelectNodes("//amsx-datatypes:Aircraft", nsmgr);
                    foreach (XmlNode fl in fls)
                    {
                        logger.Warn("Startup Aircraft Update");
                        supervisor.ProcessMessage(fl.OuterXml);
                    }

                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
            logger.Info("Populating Aircraft Complete");

        }

        public void InitAircraftTypes()
        {
            logger.Info("Populating Aircraft Types");

            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {

                try
                {
                    XmlElement res = client.GetAircraftTypes(Parameters.FROMTOKEN);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
                    nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");

                    XmlNodeList fls = res.SelectNodes("//amsx-datatypes:AircraftType", nsmgr);
                    foreach (XmlNode fl in fls)
                    {
                        logger.Warn("Startup AircraftType Update");
                        supervisor.ProcessMessage(fl.OuterXml);
                    }

                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
            logger.Info("Populating AircraftType Complete");
        }

        public void InitAirlines()
        {
            logger.Info("Populating Airlines Types");
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {

                try
                {
                    XmlElement res = client.GetAirlines(Parameters.FROMTOKEN);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
                    nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");

                    XmlNodeList fls = res.SelectNodes("//amsx-datatypes:Airline", nsmgr);
                    foreach (XmlNode fl in fls)
                    {
                        logger.Warn("Startup Airline Update");
                        supervisor.ProcessMessage(fl.OuterXml);
                    }

                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
            logger.Info("Populating Airlines Complete");
        }
    }
}
