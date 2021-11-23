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
using System.Collections.Concurrent;
using WorkBridge.Modules.AMS.AMSIntegrationWebAPI.Srv;

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
        private List<string> managedMessages = new List<string>();


        XmlDocument configDoc = new XmlDocument();

        public event Action<FlightModel, string> SendFlightMessageHandler;
        public event Action<FlightModel, string> SendPostFlightMessageHandler;

        private List<IInputMessageHandler> InputHandlers = new List<IInputMessageHandler>();
        private List<IOutputMessageHandler> OutputHandlers = new List<IOutputMessageHandler>();
        private List<IOutputMessageHandler> PostOutputHandlers = new List<IOutputMessageHandler>();
        public ConcurrentDictionary<string, HandlerDispatcher> Dispatchers = new ConcurrentDictionary<string, HandlerDispatcher>();
        private BasicHttpBinding binding;
        private EndpointAddress address;
        // One place to hold all the dispatchers for each trigger
        public HandlerDispatcher AddDispatcher(string messageType)
        {
            if (Dispatchers.ContainsKey(messageType))
            {
                return Dispatchers[messageType];
            }
            else
            {
                HandlerDispatcher dispatcher = new HandlerDispatcher();
                Dispatchers.TryAdd(messageType, dispatcher);
                return dispatcher;
            }
        }

        public bool Start()
        {

            configDoc.Load("widget.config.xml");

            foreach (XmlNode node in configDoc.SelectNodes("//ProcessMessages/Message"))
            {
                managedMessages.Add(node.InnerText);
                AddDispatcher(node.InnerText);
                logger.Info($"Managing Message Type: {node.InnerText}");
            }

            // Set the binding and address for use by the web services client
            binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 20000000,
                MaxBufferSize = 20000000,
                MaxBufferPoolSize = 20000000
            };
            address = new EndpointAddress(Parameters.FROM_AMS_WEB_SERVICE_URI);

            logger.Info($"AMS-AMS Adaptor Service Starting ({Parameters.VERSION})");

            BaseDataInit baseDataInit = new BaseDataInit(configDoc);
            baseDataInit.Sync();

            stopProcessing = false;
            startThread = new Thread(new ThreadStart(StartThread));
            startThread.Start();

            // Find all the classes that manage the different message types
            var type = typeof(IInputMessageHandler);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));

            // Set up the trggers to manage the different types
            foreach (var handler in types)
            {
                try
                {

                    IInputMessageHandler obj = (IInputMessageHandler)Activator.CreateInstance(handler);
                    obj.SetSupervisor(this, configDoc);
                    if (this.managedMessages.Contains(obj.GetMessageName()))
                    {
                        InputHandlers.Add(obj);
                        logger.Info($"Implemented Message Handler for {obj.GetMessageName()}");
                    }
                    else
                    {
                        logger.Warn($"Found Message Handler for {obj.GetMessageName()} but the messages type is not configured to be handled");
                    }
                }
                catch (Exception)
                {
                    //logger.Error(ex.Message);
                }
            }

            var outtype = typeof(IOutputMessageHandler);
            var outtypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => outtype.IsAssignableFrom(p));

            // Set up the trggers to manage the different types
            foreach (var handler in outtypes)
            {
                try
                {

                    IOutputMessageHandler obj = (IOutputMessageHandler)Activator.CreateInstance(handler);
                    obj.SetSupervisor(this, configDoc);
                    logger.Info($"Implemented Output Handler: {obj.GetDescription()}");
                    OutputHandlers.Add(obj);
                }
                catch (Exception)
                {
                    //logger.Error(ex.Message);
                }
            }


            UpdateFlights();    


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
                        ProcessMessage(xml);
                    }
                }
                catch (MessageQueueException)
                {
                    logger.Warn("No Messages to Process");
                }
                catch (Exception e)
                {
                    logger.Error("Error in Reciveving and Processing Notification Message");
                }
            }
            logger.Info("Queue Listener Stopped");
            receiveThread.Abort();
        }


        public void ProcessMessage(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode newNode = doc.DocumentElement;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-messages");
            nsmgr.AddNamespace("amsdata", "http://www.sita.aero/ams6-xml-api-datatypes");

            foreach (string messageType in managedMessages)
            {
                try
                {
                    if (newNode.SelectSingleNode($"//ams:{messageType}", nsmgr) != null || newNode.SelectSingleNode($"//amsdata:{messageType}", nsmgr) != null)
                    {
                        logger.Debug("Received Managed Message Type");
                        Dispatchers[messageType].Fire(newNode);
                        return;
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Error firing dispatcher for {messageType}. Error: {e.Message}");
                    return;
                }
            }
            logger.Debug("Received Unmanaged Message Type");
        }

        public void SendFlightMessage(FlightModel flt, string action)
        {
            SendFlightMessageHandler?.Invoke(flt, action);
        }
        public void SendPostFlightMessage(FlightModel flt, string action)
        {
            SendPostFlightMessageHandler?.Invoke(flt, action);
        }

        private void UpdateFlights()
        {
            try
            {
                using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
                {

                    try
                    {
                        XmlElement flightsElement = client.GetFlights(Parameters.FROMTOKEN, DateTime.Now.AddHours(Parameters.START_FROM_HOURS), DateTime.Now.AddHours(Parameters.START_TO_HOURS), Parameters.APT_CODE, AirportIdentifierType.IATACode);

                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(flightsElement.OwnerDocument.NameTable);
                        nsmgr.AddNamespace("ams", "http://www.sita.aero/ams6-xml-api-datatypes");

                        XmlNodeList fls = flightsElement.SelectNodes("//ams:Flight", nsmgr);
                        foreach (XmlNode fl in fls)
                        {
                            if (stopProcessing)
                            {
                                logger.Trace("Stop requested while still processing flights");
                                break;
                            }

                            logger.Warn("Startup Flight Update");
                            ProcessMessage(fl.OuterXml);

                        }

                    }
                    catch (Exception e)
                    {
                        logger.Error(e.Message);
                        logger.Error(e);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                logger.Error(e);
            }

        }
    }
}
