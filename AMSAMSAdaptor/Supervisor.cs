using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Xml;

namespace AMSAMSAdaptor
{
    public class Supervisor
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("consoleLogger");

        private static MessageQueue recvQueue;  // Queue to recieve update notifications on
        private bool startListenLoop = true;    // Flag controlling the execution of the update notificaiton listener
 

        private readonly static Random random = new Random();
        public bool stopProcessing = false;
        private Thread startThread;
        private Thread receiveThread;           // Thread the notification listener runs in 
        private BasicHttpBinding binding;
        private EndpointAddress address;
        private List<string> managedMessages = new List<string>();

       
        XmlDocument configDoc = new XmlDocument();

        public event Action<XmlNode> FlightCreatedTrigger;
        public event Action<XmlNode> FlightUpdatedTrigger;
        public event Action<XmlNode> FlightDeletedTrigger;

        public event Action<string, string> SendSoapMessageHandler;

        private List<IInputMessageHandler> InputHandlers = new List<IInputMessageHandler>();

        public bool Start()
        {
            
            configDoc.Load("widget.config.xml");

            foreach(XmlNode node in configDoc.SelectNodes("//ProcessMessages/Message"))
            {
                managedMessages.Add(node.InnerText);
                logger.Info($"Managing Message Type: {node.InnerText}");
            }


            logger.Info($"AMS-AMS Adaptor Service Starting ({Parameters.VERSION})");

            // Set the binding and address for use by the web services client
            binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 20000000,
                MaxBufferSize = 20000000,
                MaxBufferPoolSize = 20000000
            };
            //address = new EndpointAddress(Parameters.AMS_WEB_SERVICE_URI);
            stopProcessing = false;
            startThread = new Thread(new ThreadStart(StartThread));
            startThread.Start();

            // Find all the classes that manage the different message types
            var type = typeof(IInputMessageHandler);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));

            // Set up the trggers to manage the different types
            foreach(var handler in types)
            {
                try
                {
                   
                    IInputMessageHandler obj = (IInputMessageHandler)Activator.CreateInstance(handler);
                    obj.SetSupervisor(this, configDoc);
                    InputHandlers.Add(obj);
                    logger.Info($"Implemented Handler for {obj.GetMessageName()}");
                } catch (Exception)
                {
                    //logger.Error(ex.Message);
                }
            }

            logger.Info($"AMS-AMS Adaptor Started");

            return true;
        }
        public void StartThread()
        {

            //Start Listener for incoming towing notifications
            recvQueue = new MessageQueue(Parameters.RECVQ);
            StartMQListener();
            logger.Info($"Started Notification Queue Listener on queue: {Parameters.RECVQ}");

            


            // Optionally process flights 
            if (Parameters.STARTUP_FLIGHT_PROCESSING)
            {
                logger.Warn(">>>>>>> Not Implemented - STARTUP Flight Processing");
            }
        }

        // Start the thread to listen to incoming update notifications
        public void StartMQListener()
        {
            try
            {
                this.startListenLoop = true;
                receiveThread = new Thread(this.ListenToQueue)
                {
                    IsBackground = false
                };
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                logger.Error("Error starting notification queue listener");
                logger.Error(ex.Message);
            }
        }
        public void Stop()
        {
            logger.Info("AMS-AMS Adaptor Service Stopping");
            stopProcessing = true;
            startListenLoop = false;
            logger.Info("AMS-AMS Adaptor Service Stopped");
        }

        private void ListenToQueue()
        {

            while (startListenLoop)
            {
                //Put it in a Try/Catch so on bad message or reading problem dont stop the system
                try
                {
                   using (Message msg = recvQueue.Receive(new TimeSpan(0, 0, Parameters.READ_MESSAGE_LOOP_INTERVAL)))
                    {
                        string xml;
                        using (StreamReader reader = new StreamReader(msg.BodyStream))
                        {
                            xml = reader.ReadToEnd();
                        }
                        if (CheckManagedMessage(xml))
                        {
                            logger.Debug("Received Managed Message Type");
                            logger.Trace(xml);
                            ProcessMessage(xml);
                        } else
                        {
                            logger.Debug("Received Unmanaged Message Type");
                        }
                    }
                }
                catch (MessageQueueException)
                {
                    logger.Warn("No Messages to Process");
                }
                catch (Exception e)
                {
                    logger.Error("Error in Reciveving and Processing Notification Message");
                    logger.Error(e.Message);
                    Thread.Sleep(Parameters.RESTSERVER_RETRY_INTERVAL);
                }
            }
            logger.Info("Queue Listener Stopped");
            receiveThread.Abort();
        }

        private bool CheckManagedMessage(string xml)
        {
            foreach(string messageType in managedMessages)
            {
                if (xml.Contains(messageType)){
                    return true;
                }
            }
            return false;
        }
        public void ProcessMessage(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode newNode = doc.DocumentElement;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");


            if (newNode.SelectSingleNode("//ams:FlightCreatedNotification", nsmgr) != null)
            {
                FlightCreatedTrigger?.Invoke(newNode);
            }
            if (newNode.SelectSingleNode("//ams:FlightUpdatedNotification", nsmgr) != null)
            {
                FlightUpdatedTrigger?.Invoke(newNode);
            }
            if (newNode.SelectSingleNode("//ams:FlightDeletedNotification", nsmgr) != null)
            {
                FlightDeletedTrigger?.Invoke(newNode);
            }
        }

        public void SendSoapMessage(string message, string soapAction)
        {
            SendSoapMessageHandler?.Invoke(message, soapAction);
        }
    }
}
