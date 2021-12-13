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

                NameValueCollection appSettings = ConfigurationManager.AppSettings;


                string serviceName = string.IsNullOrEmpty(appSettings["serviceName"]) ? $"SITA MEA - AMS to AMS Adaptor" : appSettings["serviceName"];
                string serviceDisplayName = string.IsNullOrEmpty(appSettings["serviceDisplayName"]) ? $"SITA MEA - AMS to AMS Adaptor" : appSettings["serviceDisplayName"];
                string serviceDescription = string.IsNullOrEmpty(appSettings["serviceDescription"]) ? "Exchanges data from one instance of AMS to another" : appSettings["serviceDescription"];

                x.SetServiceName(serviceName);
                x.SetDisplayName(serviceDisplayName);
                x.SetDescription(serviceDescription);
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}
