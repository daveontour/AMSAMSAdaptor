using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Topshelf;

namespace AMSAMSAdaptor
{
    class Program
    {

        // Primarily skeleton code for defining the ConsoleApp/Service to be managed by TopShelf
        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {

                x.Service<Supervisor>(s =>
                {
                    s.ConstructUsing(core => new Supervisor());
                    s.WhenStarted(core => core.Start());
                    s.WhenStopped(core => core.Stop());
                });

                x.RunAsLocalSystem();
                x.StartAutomatically();
                x.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1); // restart the service after 1 minute
                });


                XmlDocument doc = new XmlDocument();
                doc.Load("widget.config.xml");

                string serviceName = doc.SelectSingleNode(".//ServiceName")?.InnerText;
                string serviceDisplayName = doc.SelectSingleNode(".//ServiceDisplayName")?.InnerText;
                string serviceDescription = doc.SelectSingleNode(".//ServiceDescription")?.InnerText;

                serviceName = string.IsNullOrEmpty(serviceName) ? $"SITA MEA - AMS to AMS Adaptor" : serviceName;
                serviceDisplayName = string.IsNullOrEmpty(serviceDisplayName) ? $"SITA MEA - AMS to AMS Adaptor" : serviceDisplayName;
                serviceDescription = string.IsNullOrEmpty(serviceDescription) ? "Exchanges data from one instance of AMS to another" : serviceDescription;

                x.SetServiceName(serviceName);
                x.SetDisplayName(serviceDisplayName);
                x.SetDescription(serviceDescription);
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }

}
