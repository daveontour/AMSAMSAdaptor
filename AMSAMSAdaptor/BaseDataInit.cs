using System;
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
        private bool initAreas = false;
        private bool initCheckIns = false;
        private bool initStands = false;
        private bool initGates = false;
        private bool initRoutes = false;

        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("consoleLogger");

        public BaseDataInit(XmlNode config, Supervisor supervisor)
        {
            this.config = config;
            this.supervisor = supervisor;

            bool.TryParse(config.SelectSingleNode(".//InitAircraftTypes")?.InnerText, out initACTypes);
            bool.TryParse(config.SelectSingleNode(".//InitAircraft")?.InnerText, out initAC);
            bool.TryParse(config.SelectSingleNode(".//InitAirports")?.InnerText, out initAirports);
            bool.TryParse(config.SelectSingleNode(".//InitAirlines")?.InnerText, out initAirlines);
            bool.TryParse(config.SelectSingleNode(".//InitAreas")?.InnerText, out initAreas);
            bool.TryParse(config.SelectSingleNode(".//InitCheckIns")?.InnerText, out initCheckIns);
            bool.TryParse(config.SelectSingleNode(".//InitStands")?.InnerText, out initStands);
            bool.TryParse(config.SelectSingleNode(".//InitGates")?.InnerText, out initGates);
            bool.TryParse(config.SelectSingleNode(".//InitRoutes")?.InnerText, out initRoutes);

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
            if (initAreas) InitAreas();
            if (initCheckIns) InitCheckIns();
            if (initGates) InitGates();
            if (initStands) InitStands();
            if (initRoutes) InitRoutes();
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
                        logger.Debug("Startup AircraftType Update");
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

        public void InitAreas()
        {
            logger.Info("Populating Areas");

            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    XmlElement res = client.GetAreas(Parameters.FROMTOKEN, "IXE", WorkBridge.Modules.AMS.AMSIntegrationWebAPI.Srv.AirportIdentifierType.IATACode);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
                    nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");

                    XmlNodeList fls = res.SelectNodes("//amsx-datatypes:Areas/amsx-datatypes:Area", nsmgr);
                    foreach (XmlNode fl in fls)
                    {
                        logger.Warn("Startup Area Update");
                        supervisor.ProcessMessage(fl.OuterXml);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
            logger.Info("Populating Area Complete");
        }

        public void InitCheckIns()
        {
            logger.Info("Populating Areas");

            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    XmlElement res = client.GetCheckIns(Parameters.FROMTOKEN, "IXE", WorkBridge.Modules.AMS.AMSIntegrationWebAPI.Srv.AirportIdentifierType.IATACode);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
                    nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");

                    XmlNodeList fls = res.SelectNodes("//amsx-datatypes:CheckIns/amsx-datatypes:CheckIn", nsmgr);
                    foreach (XmlNode fl in fls)
                    {
                        logger.Warn("Startup Checkin Update");
                        supervisor.ProcessMessage(fl.OuterXml);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
            logger.Info("Populating CheckIns Complete");
        }

        public void InitGates()
        {
            logger.Info("Populating Areas");

            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    XmlElement res = client.GetGates(Parameters.FROMTOKEN, "IXE", WorkBridge.Modules.AMS.AMSIntegrationWebAPI.Srv.AirportIdentifierType.IATACode);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
                    nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");

                    XmlNodeList fls = res.SelectNodes("//amsx-datatypes:Gates/amsx-datatypes:Gate", nsmgr);
                    foreach (XmlNode fl in fls)
                    {
                        logger.Warn("Startup Gates Update");
                        supervisor.ProcessMessage(fl.OuterXml);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
            logger.Info("Populating Gates Complete");
        }

        public void InitStands()
        {
            logger.Info("Populating Areas");

            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    XmlElement res = client.GetStands(Parameters.FROMTOKEN, "IXE", WorkBridge.Modules.AMS.AMSIntegrationWebAPI.Srv.AirportIdentifierType.IATACode);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
                    nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");

                    XmlNodeList fls = res.SelectNodes("//amsx-datatypes:Stands/amsx-datatypes:Stand", nsmgr);
                    foreach (XmlNode fl in fls)
                    {
                        logger.Warn("Startup Stand Update");
                        supervisor.ProcessMessage(fl.OuterXml);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
            logger.Info("Populating Stand Complete");
        }

        public void InitRoutes()
        {
            logger.Info("Populating Routes");

            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {
                try
                {
                    XmlElement res = client.GetRoutes(Parameters.FROMTOKEN);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
                    nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");

                    XmlNodeList fls = res.SelectNodes("//amsx-datatypes:Routes/amsx-datatypes:Route", nsmgr);
                    foreach (XmlNode fl in fls)
                    {
                        logger.Warn("Startup Routes Update");
                        supervisor.ProcessMessage(fl.OuterXml);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
            logger.Info("Populating Routes Complete");
        }
    }
}