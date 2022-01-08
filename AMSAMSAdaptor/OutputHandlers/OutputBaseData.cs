using System;
using System.Messaging;
using System.Text;
using System.Xml;
using NLog;

namespace AMSAMSAdaptor
{
    internal class OutputBaseData : IOutputMessageHandler
    {
        private string token;
        private string toqueue;
        private bool toMSMQ = false;
        private readonly Logger logger = LogManager.GetLogger("consoleLogger");

        public void SetSupervisor(Supervisor supervisor, XmlDocument configDoc)
        {
            supervisor.SendBaseDataMessageHandler += Sender;

            token = configDoc.SelectSingleNode(".//ToToken")?.InnerText;
            toqueue = configDoc.SelectSingleNode(".//ToAMSRequestQueue")?.InnerText;

            bool.TryParse(configDoc.SelectSingleNode(".//ToAMSRequestQueue")?.Attributes["enabled"]?.Value, out toMSMQ);
        }

        public string GetDescription()
        {
            return "Send Base Data Messages via Request Queue";
        }

        private void Sender(ModelBase data, string action)
        {
            logger.Trace($"Base Data output handler received message type {action}");
            string xml = null;
            if (action.Contains("Create")) xml = data.CreateRequest();
            if (action.Contains("Update")) xml = data.UpdateRequest();
            if (action.Contains("Delete")) xml = data.DeleteRequest();

            xml = xml.Replace("TOKEN", token);

            Send(xml);
        }

        private void Send(string xml)
        {
            if (toMSMQ)
            {
                SendMS(xml);
            }
            else
            {
                logger.Warn($"Output to target queue is not enabled. Is <ToAMSRequestQueue  enabled=\"true\">  set?");
            }
        }

        private void SendMS(string xml)
        {
            logger.Debug("Sending base data message to destination queue");
            logger.Trace(xml.PrintXML());
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
                    logger.Error($"Error sending message to destination queue: {ex.Message}. Destination queue: {toqueue}");
                    return;
                }
            }
        }
    }
}