using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WorkBridge.Modules.AMS.AMSIntegrationAPI.Mod.Intf.DataTypes;

namespace AMSAMSAdaptor
{
    internal class OutputBaseData : IOutputMessageHandler
    {
        private Supervisor supervisor;
        private string token;

        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("consoleLogger");


        public void SetSupervisor(Supervisor supervisor, XmlDocument configDoc)
        {
            this.supervisor = supervisor;
            supervisor.SendBaseDataMessageHandler += Sender;

            token = configDoc.SelectSingleNode(".//ToToken").InnerText;
        }

        public string GetDescription()
        {
            return "Send Base Data Messages VIA Request Queue";
        }

        private void Sender(ModelBase data, string action)
        {
            string xml = null;
            if (action.Contains("Create")) xml = data.CreateRequest();
            if (action.Contains("Update")) xml = data.UpdateRequest();
            if (action.Contains("Delete")) xml = data.DeleteRequest();

             
            xml = xml.Replace("TOKEN", token);
            Console.WriteLine(xml);
        }
    }
}
