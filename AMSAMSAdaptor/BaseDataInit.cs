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
        private BasicHttpBinding binding;
        private EndpointAddress address;
        public BaseDataInit(XmlNode config)
        {
            this.config = config;


            binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 20000000,
                MaxBufferSize = 20000000,
                MaxBufferPoolSize = 20000000
            };
            address = new EndpointAddress(Parameters.TO_AMS_WEB_SERVICE_URI);
        }

        public string GetAirports()
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {

                try
                {
                    XmlElement res = client.GetAirports(Parameters.FROMTOKEN);
                    return res.OuterXml;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return null;
        }

        public void InitAircrafts()
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {


                try
                {
                    XmlElement res = client.GetAircrafts(Parameters.FROMTOKEN);

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(res.OwnerDocument.NameTable);
                    nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");

                    XmlNodeList acs = res.SelectNodes("//ams:Flight", nsmgr);
                    foreach (XmlNode fl in acs)
                    {


                    }



                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public string GetAircraftTypes()
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {

                try
                {
                    XmlElement res = client.GetAircraftTypes(Parameters.FROMTOKEN);
                    return res.OuterXml;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return null;
        }

        public string GetAirlines()
        {
            using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
            {

                try
                {
                    XmlElement res = client.GetAirlines(Parameters.FROMTOKEN);


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return null;
        }


    }
}
