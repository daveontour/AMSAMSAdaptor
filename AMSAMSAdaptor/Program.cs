using System;
using System.Collections.Specialized;
using System.Configuration;
using Topshelf;

namespace AMSAMSAdaptor
{
    internal class Program
    {
        // Primarily skeleton code for defining the ConsoleApp/Service to be managed by TopShelf
        private static void Main(string[] args)
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

                string serviceName = string.IsNullOrEmpty(appSettings["serviceName"]) ? $"SITAAMSAMSAdaptor" : appSettings["serviceName"];
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