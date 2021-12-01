using IBM.WMQ;
using System;
using System.Collections;
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

        private string qMgr;
        private string qSvrChan;
        private string qHost;
        private string qPort;
        private string qUser;
        private string qPass;
        private int putTimeout;
        public string queueName;
        private bool useSendLocking;
        private readonly Hashtable connectionParams = new Hashtable();
        private readonly object sendLock = new object();
        public static MQQueueManager queueManager;

        public bool toMSMQ = false;
        public bool toIBMMQ = false;

        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("consoleLogger");


        public void SetSupervisor(Supervisor supervisor, XmlDocument configDoc)
        {
            this.supervisor = supervisor;
            supervisor.SendBaseDataMessageHandler += Sender;

            token = configDoc.SelectSingleNode(".//ToToken")?.InnerText;
            toqueue = configDoc.SelectSingleNode(".//ToAMSRequestQueue")?.InnerText;

            bool.TryParse(configDoc.SelectSingleNode(".//ToAMSRequestQueue")?.Attributes["enabled"]?.Value, out toMSMQ);
            bool.TryParse(configDoc.SelectSingleNode(".//ToIBMMQRequestQueue")?.Attributes["enabled"]?.Value, out toIBMMQ);

            XmlNode defn = configDoc.SelectSingleNode(".//ToIBMMQRequestQueue");

            if (defn != null)
            {
                try
                {
                    try
                    {
                        queueName = defn.Attributes["queue"].Value;
                    }
                    catch (Exception)
                    {
                        logger.Error($"No Queue defined for {defn.Attributes["name"].Value}");
                        return;
                    }

                    try
                    {
                        qMgr = defn.Attributes["queueMgr"].Value;
                    }
                    catch (Exception)
                    {
                        logger.Error($"Queue Manager not defined for {defn.Attributes["name"].Value}");
                        return;
                    }
                    try
                    {
                        qSvrChan = defn.Attributes["channel"].Value;
                    }
                    catch (Exception)
                    {
                        logger.Error($"Channel not defined for {defn.Attributes["name"].Value}");
                        return;
                    }

                    try
                    {
                        qHost = defn.Attributes["host"].Value;
                    }
                    catch (Exception)
                    {
                        logger.Error($"Queue  not defined for {defn.Attributes["name"].Value}");
                        return;
                    }

                    try
                    {
                        qPort = defn.Attributes["port"].Value;
                    }
                    catch (Exception)
                    {
                        logger.Error($"Port not defined for {defn.Attributes["name"].Value}");
                        return;
                    }

                    try
                    {
                        qUser = defn.Attributes["username"].Value;
                    }
                    catch (Exception)
                    {
                        qUser = null;
                        logger.Info($"No username defined for {defn.Attributes["name"].Value}");
                    }

                    try
                    {
                        putTimeout = int.Parse(defn.Attributes["putTimeout"].Value);
                    }
                    catch (Exception)
                    {
                        putTimeout = 10;
                        logger.Info("MQ Message Put Timeout set to default");
                    }

                    try
                    {
                        qPass = defn.Attributes["password"].Value;
                    }
                    catch (Exception)
                    {
                        qPass = null;
                        logger.Info($"No password defined for {defn.Attributes["name"].Value}");
                    }
                    try
                    {
                        useSendLocking = bool.Parse(defn.Attributes["useSendLocking"].Value);
                    }
                    catch (Exception)
                    {
                        useSendLocking = false;
                    }

                    try
                    {
                        // Set the connection parameter
                        connectionParams.Add(MQC.CHANNEL_PROPERTY, qSvrChan);
                        connectionParams.Add(MQC.HOST_NAME_PROPERTY, qHost);
                        connectionParams.Add(MQC.PORT_PROPERTY, qPort);

                        if (qUser != null)
                        {
                            connectionParams.Add(MQC.USER_ID_PROPERTY, qUser);
                        }
                        if (qPass != null)
                        {
                            connectionParams.Add(MQC.PASSWORD_PROPERTY, qPass);
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
                catch (AccessViolationException ex)
                {
                    logger.Info(ex.Message);
                    logger.Info(ex.StackTrace);
                    return;
                }
                catch (Exception ex)
                {
                    logger.Info("Error configuring MQ queue");
                    logger.Info(ex.Message);
                    logger.Info(ex.StackTrace);
                    Console.WriteLine($"Error configuring MQ access for {defn.Attributes["name"].Value}");
                    return;
                }
            }

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
            if (toIBMMQ) SendMQ(xml);
            if (toMSMQ) SendMS(xml);
        }
        private void SendMS(string xml)
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

        private void SendMQ(string xml)
        {
            string messageXML = xml;
            bool sent = false;

            try
            {
                var openOptions = MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING;
                MQQueue queue = queueManager.AccessQueue(queueName, openOptions);

                logger.Trace($"MQ Accessed Queue:  {queueName}");

                var message = new MQMessage
                {
                    CharacterSet = 1208 // UTF-8
                };
                message.WriteString(messageXML);
                message.Format = MQC.MQFMT_STRING;

                MQPutMessageOptions putOptions = new MQPutMessageOptions
                {
                    Timeout = putTimeout
                };

                logger.Trace($"MQ Putting Message to Queue:  {queueName} with timeout {putTimeout}");
                
                queue.Put(message, putOptions);
                logger.Trace($"MQ Message Sent to {queueName}");

                queue.Close();
                logger.Trace($"MQ Queue Closed {queueName}");

                sent = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending MQ Message: {ex.Message}");
                logger.Trace($"Error sending MQ Message: {ex.Message}");
                return;
            }
            if (!sent)
            {
                logger.Trace($"Warning: Message NOT Sent to  {queueName}");
                return;
            }
        }
    }
}
