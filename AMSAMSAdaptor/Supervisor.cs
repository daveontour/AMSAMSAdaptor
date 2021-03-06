using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.ServiceModel;
using System.Threading;
using System.IO;
using System.Xml;
using System.Collections.Concurrent;
using WorkBridge.Modules.AMS.AMSIntegrationWebAPI.Srv;
using System.Timers;
using System.Text;
using NLog;

namespace AMSAMSAdaptor
{
    /*
     *
     * Main controlling class for the adaptor.
     * Reads the config file and instansiates all the different handlers
     * Initiates start-up  processing and back catalog processing
     *
     */

    public class Supervisor
    {
        public string queueName;
        private bool fromMSMQ = false;

        private readonly Logger logger = LogManager.GetLogger("consoleLogger");

        private static MessageQueue recvQueue;  // Queue to recieve update notifications on
        private bool startListenLoop = true;    // Flag controlling the execution of the update notificaiton listener

        public bool stopProcessing = false;
        private Thread startThread;
        private Thread receiveThread;           // Thread the notification listener runs in
        private readonly List<string> managedMessages = new List<string>();

        private readonly XmlDocument configDoc = new XmlDocument();

        public event Action<ModelFlight, string> SendFlightMessageHandler;

        public event Action<ModelFlight, string> SendPostFlightMessageHandler;

        public event Action<ModelBase, string> SendBaseDataMessageHandler;

        private readonly List<IInputMessageHandler> InputHandlers = new List<IInputMessageHandler>();
        private readonly List<IOutputMessageHandler> OutputHandlers = new List<IOutputMessageHandler>();
        private readonly List<IPostOutputMessageHandler> PostOutputHandlers = new List<IPostOutputMessageHandler>();

        public ConcurrentDictionary<string, HandlerDispatcher> Dispatchers = new ConcurrentDictionary<string, HandlerDispatcher>();
        private BasicHttpBinding binding;
        private EndpointAddress address;

        private string BackCatalogFrom { get; set; }
        private string BackCatalogTo { get; set; }
        private int BackCatalogBlockLength { get; set; }
        private int BackCatalogInterval { get; set; }
        private System.Timers.Timer bcTimer;
        private DateTime bcLastUpdate;

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
            // Read the main configuration file
            configDoc.Load("widget.config.xml");

            // Set the binding and address for use by the web services client (used by the web services interface)
            binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 20000000,
                MaxBufferSize = 20000000,
                MaxBufferPoolSize = 20000000
            };
            address = new EndpointAddress(Parameters.FROM_AMS_WEB_SERVICE_URI);

            logger.Info($"AMS-AMS Adaptor Service Starting ({Parameters.VERSION})");

            bool.TryParse(configDoc.SelectSingleNode(".//FromAMSQueue")?.Attributes["enabled"]?.Value, out fromMSMQ);

            // Identify and load all te configured message handlers

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            foreach (XmlNode node in configDoc.SelectNodes("//Handlers/Handler"))
            {
                string handlerClass = node.Attributes["class"].Value;
                string handlerMessageType = node.Attributes["messageType"].Value;
                string handlerName = node.Attributes["name"]?.Value;
                string handlerEnabled = node.Attributes["enabled"].Value;

                try
                {
                    if (handlerEnabled.ToUpper() == "TRUE")
                    {
                        if (!managedMessages.Contains(handlerMessageType))
                        {
                            managedMessages.Add(handlerMessageType);
                            AddDispatcher(handlerMessageType);
                        }
                        Type t = Type.GetType($"AMSAMSAdaptor.{handlerClass}");
                        IInputMessageHandler handler = (IInputMessageHandler)Activator.CreateInstance(t, this, node);
                        InputHandlers.Add(handler);

                        sb.AppendLine($"Loaded Handler: {handlerClass} ({handlerName}) to handle message type: {handlerMessageType}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    logger.Error($"Error loading handler class {handlerClass}");
                }
            }

            logger.Info(sb.ToString());

            // Reflective discovery of the ouput handlers
            var outtype = typeof(IOutputMessageHandler);
            var outtypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => outtype.IsAssignableFrom(p));

            // Set up the trggers to manage the different types
            sb.Clear();
            sb.AppendLine();
            foreach (var handler in outtypes)
            {
                try
                {
                    IOutputMessageHandler obj = (IOutputMessageHandler)Activator.CreateInstance(handler);
                    obj.SetSupervisor(this, configDoc);
                    sb.AppendLine($"Implemented Output Handler: {obj.GetDescription()}");
                    OutputHandlers.Add(obj);
                }
                catch (Exception)
                {
                    //logger.Error(ex.Message);
                }
            }
            logger.Info(sb.ToString());

            // Post out put handlers are for when the CRUD has been completed.
            var postouttype = typeof(IPostOutputMessageHandler);
            var postouttypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => postouttype.IsAssignableFrom(p));

            // Set up the trggers to manage the different types
            foreach (var handler in outtypes)
            {
                try
                {
                    IPostOutputMessageHandler obj = (IPostOutputMessageHandler)Activator.CreateInstance(handler);
                    obj.SetSupervisor(this, configDoc);
                    logger.Info($"Implemented Post Output Handler: {obj.GetDescription()}");
                    PostOutputHandlers.Add(obj);
                }
                catch (Exception)
                {
                    //logger.Error(ex.Message);
                }
            }

            // Initiate start-up processing
            BaseDataInit baseDataInit = new BaseDataInit(configDoc, this);
            baseDataInit.Sync();
            //"Current" flights
            UpdateFlights();

            // Historic flights loaded at a lower priority
            BackCatalogProcessing();

            stopProcessing = false;
            startThread = new Thread(new ThreadStart(StartThread));
            startThread.Start();
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
                if (fromMSMQ) receiveThread = new Thread(this.ListenMSMQ)
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

        private void ListenMSMQ()
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
                    logger.Trace("No Messages to Process");
                }
                catch (Exception e)
                {
                    logger.Error($"Error in Reciveving and Processing Notification Message. {e.Message}");
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
            nsmgr.AddNamespace("amsx-messages", "http://www.sita.aero/ams6-xml-api-messages");
            nsmgr.AddNamespace("amsx-datatypes", "http://www.sita.aero/ams6-xml-api-datatypes");

            foreach (string messageType in managedMessages)
            {
                try
                {
                    if (newNode.SelectSingleNode($"//amsx-messages:Content/amsx-messages:{messageType}", nsmgr) != null || newNode.SelectSingleNode($"/amsx-datatypes:{messageType}", nsmgr) != null)
                    {
                        logger.Debug($"Received Managed Message Type: {messageType}");
                        logger.Trace(xml.PrintXML());
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
            logger.Trace($"Received Unmanaged Message Type");
        }

        public void SendFlightMessage(ModelFlight flt, string action)
        {
            SendFlightMessageHandler?.Invoke(flt, action);
        }

        public void SendBaseDataMessage(ModelBase basedata, string action)
        {
            SendBaseDataMessageHandler?.Invoke(basedata, action);
        }

        public void SendPostFlightMessage(ModelFlight flt, string action)
        {
            SendPostFlightMessageHandler?.Invoke(flt, action);
        }

        private void UpdateFlights()
        {
            bool.TryParse(configDoc.SelectSingleNode(".//InitFlights")?.InnerText, out bool initFlights);
            if (!initFlights)
            {
                logger.Info("Flight initialisation is NOT configured");
                return;
            }

            logger.Info("Flight initialisation commencing");

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

        private void BackCatalogProcessing()
        {
            bool.TryParse(configDoc.SelectSingleNode(".//enableBackCatalog")?.InnerText, out bool backCatalog);
            if (!backCatalog)
            {
                logger.Info("Back Catalog Flight processing is NOT configured");
                return;
            }

            BackCatalogFrom = configDoc.SelectSingleNode(".//backCatalogFrom").InnerText;
            BackCatalogTo = configDoc.SelectSingleNode(".//backCatalogTo").InnerText;

            BackCatalogBlockLength = Int32.Parse(configDoc.SelectSingleNode(".//backCatalogBlockLength").InnerText);
            BackCatalogInterval = Int32.Parse(configDoc.SelectSingleNode(".//backCatalogInterval").InnerText);

            bcLastUpdate = DateTime.Parse(BackCatalogFrom);
            bcTimer = new System.Timers.Timer
            {
                Interval = BackCatalogInterval * 1000,
                AutoReset = true,
                Enabled = true
            };
            bcTimer.Elapsed += BackCatalogIntervalProcessing;

            logger.Info("Flight initialisation commencing");
        }

        private void BackCatalogIntervalProcessing(object sender, ElapsedEventArgs e)
        {
            try
            {
                DateTime st = bcLastUpdate.AddDays(-1 * BackCatalogBlockLength);
                DateTime et = st.AddDays(BackCatalogBlockLength);

                if (et < DateTime.Parse(BackCatalogTo))
                {
                    logger.Info("Back Catalog Processing Completed");
                    bcTimer.Stop();
                    return;
                }

                string sts = $"{st:dd-MM-yyyy}%20{st:HH:mm}";
                string ets = $"{et:dd-MM-yyyy}%20{et:HH:mm}";
                bcLastUpdate = st;

                logger.Info($"Processing historic flight between {st} and {et}");

                try
                {
                    using (AMSIntegrationServiceClient client = new AMSIntegrationServiceClient(binding, address))
                    {
                        try
                        {
                            XmlElement flightsElement = client.GetFlights(Parameters.FROMTOKEN, st, et, Parameters.APT_CODE, AirportIdentifierType.IATACode);

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
                        catch (Exception ex1)
                        {
                            logger.Error(ex1.Message);
                            logger.Error(ex1);
                        }
                    }
                }
                catch (Exception ex1)
                {
                    logger.Error(ex1.Message);
                    logger.Error(ex1);
                }
            }
            catch (Exception)
            {
                //NO-OP
            }
        }
    }
}