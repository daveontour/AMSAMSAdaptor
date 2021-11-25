using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
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
        private string toqueue;

        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("consoleLogger");


        public void SetSupervisor(Supervisor supervisor, XmlDocument configDoc)
        {
            this.supervisor = supervisor;
            supervisor.SendBaseDataMessageHandler += Sender;

            token = configDoc.SelectSingleNode(".//ToToken").InnerText;
            toqueue = configDoc.SelectSingleNode(".//ToAMSRequestQueue").InnerText;
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
            //Console.WriteLine(xml);
            Send(xml);
        }

        private void Send(string xml)
        {
            logger.Debug("Sending base data message to destination queue");
            using (MessageQueue msgQueue = new MessageQueue(toqueue))
            {
                try
                {
                    var body = Encoding.ASCII.GetBytes(xml);
                    Message myMessage = new Message(body, new ActiveXMessageFormatter());
                    msgQueue.Send(myMessage);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error sending message to destination queue: {ex.Message}");
                    return;
                }
            }
        }
    }
}
