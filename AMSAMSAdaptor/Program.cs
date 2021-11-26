using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Topshelf;
using WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes;

namespace AMSAMSAdaptor
{
    class Program
    {

        // Primarily skeleton code for defining the ConsoleApp/Service to be managed by TopShelf
        static void Main(string[] args)
        {
            try
            {
                XmlDocument configDoc = new XmlDocument();
                configDoc.Load("C:/Users/dave_/Desktop/flightUpdate.xml");
                XmlNode newNode = configDoc.DocumentElement;

                ModelFlight flt = new ModelFlight(newNode);

                FlightUpdateInformation x = flt.GetFlightUpdateInformation();
                Console.WriteLine(x);

                BasicHttpBinding binding = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 20000000,
                    MaxBufferSize = 20000000,
                    MaxBufferPoolSize = 20000000
                };
                EndpointAddress address = new EndpointAddress(Parameters.TO_AMS_WEB_SERVICE_URI);
                using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
                {
                    try
                    {
                        XmlElement res = client.UpdateFlightExtended(Parameters.TOTOKEN, flt.GetFlightId(), flt.GetFlightUpdateInformation());
                        Console.WriteLine(PrintXML(res.OuterXml));

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);  
            }
            Console.ReadLine();
            //var exitCode = HostFactory.Run(x =>
            //{

            //    x.Service<Supervisor>(s =>
            //    {
            //        s.ConstructUsing(core => new Supervisor());
            //        s.WhenStarted(core => core.Start());
            //        s.WhenStopped(core => core.Stop());
            //    });

            //    x.RunAsLocalSystem();
            //    x.StartAutomatically();
            //    x.EnableServiceRecovery(rc =>
            //    {
            //        rc.RestartService(1); // restart the service after 1 minute
            //    });


            //    XmlDocument doc = new XmlDocument();
            //    doc.Load("widget.config.xml");

            //    string serviceName = doc.SelectSingleNode(".//ServiceName")?.InnerText;
            //    string serviceDisplayName = doc.SelectSingleNode(".//ServiceDisplayName")?.InnerText;
            //    string serviceDescription = doc.SelectSingleNode(".//ServiceDescription")?.InnerText;

            //    serviceName = string.IsNullOrEmpty(serviceName) ? $"SITA MEA - AMS to AMS Adaptor" : serviceName;
            //    serviceDisplayName = string.IsNullOrEmpty(serviceDisplayName) ? $"SITA MEA - AMS to AMS Adaptor" : serviceDisplayName;
            //    serviceDescription = string.IsNullOrEmpty(serviceDescription) ? "Exchanges data from one instance of AMS to another" : serviceDescription;

            //    x.SetServiceName(serviceName);
            //    x.SetDisplayName(serviceDisplayName);
            //    x.SetDescription(serviceDescription);
            //});

            //int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            //Environment.ExitCode = exitCodeValue;
        }

        public static string PrintXML(string xml)
        {

            string result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXml = sReader.ReadToEnd();

                result = formattedXml;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            mStream.Close();
            writer.Close();

            return result;
        }

    }

}
