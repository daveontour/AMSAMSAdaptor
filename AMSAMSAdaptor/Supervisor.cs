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
using IBM.WMQ;
using System.Collections;

namespace AMSAMSAdaptor
{
    public class Supervisor
    {
        private string qMgr;
        private string qSvrChan;
        private string qHost;
        private string qPort;
        private string qUser;
        private string qPass;
        private int getTimeout;
        public string queueName;
        private bool useSendLocking;
        private readonly Hashtable connectionParams = new Hashtable();

        private bool fromMSMQ = false;
        private bool fromIBMMQ = false;

        private bool OK_TO_RUN = true;


        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("consoleLogger");

        private static MessageQueue recvQueue;  // Queue to recieve update notifications on
        private bool startListenLoop = true;    // Flag controlling the execution of the update notificaiton listener

        private readonly static Random random = new Random();
        public bool stopProcessing = false;
        private Thread startThread;
        private Thread receiveThread;           // Thread the notification listener runs in 
        private List<string> managedMessages = new List<string>();


        XmlDocument configDoc = new XmlDocument();

        public event Action<ModelFlight, string> SendFlightMessageHandler;
        public event Action<ModelFlight, string> SendPostFlightMessageHandler;
        public event Action<ModelBase, string> SendBaseDataMessageHandler;

        private List<IInputMessageHandler> InputHandlers = new List<IInputMessageHandler>();
        private List<IOutputMessageHandler> OutputHandlers = new List<IOutputMessageHandler>();
        private List<IPostOutputMessageHandler> PostOutputHandlers = new List<IPostOutputMessageHandler>();

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

            //foreach (XmlNode node in configDoc.SelectNodes("//ProcessMessages/Message"))
            //{
            //    managedMessages.Add(node.InnerText);
            //    AddDispatcher(node.InnerText);
            //    logger.Info($"Managing Message Type: {node.InnerText}");
            //}

            // Set the binding and address for use by the web services client
            binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 20000000,
                MaxBufferSize = 20000000,
                MaxBufferPoolSize = 20000000
            };
            address = new EndpointAddress(Parameters.FROM_AMS_WEB_SERVICE_URI);

            logger.Info($"AMS-AMS Adaptor Service Starting ({Parameters.VERSION})");

            bool.TryParse(configDoc.SelectSingleNode(".//FromAMSQueue")?.Attributes["enabled"]?.Value, out fromMSMQ);
            bool.TryParse(configDoc.SelectSingleNode(".//FromIBMMQRequestQueue")?.Attributes["enabled"]?.Value, out fromIBMMQ);


            XmlNode defn = configDoc.SelectSingleNode(".//FromIBMMQRequestQueue");

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
                        //  return;
                    }

                    try
                    {
                        qMgr = defn.Attributes["queueMgr"].Value;
                    }
                    catch (Exception)
                    {
                        logger.Error($"Queue Manager not defined for {defn.Attributes["name"].Value}");
                        // return;
                    }
                    try
                    {
                        qSvrChan = defn.Attributes["channel"].Value;
                    }
                    catch (Exception)
                    {
                        logger.Error($"Channel not defined for {defn.Attributes["name"].Value}");
                        // return;
                    }

                    try
                    {
                        qHost = defn.Attributes["host"].Value;
                    }
                    catch (Exception)
                    {
                        logger.Error($"Queue  not defined for {defn.Attributes["name"].Value}");
                        //return;
                    }

                    try
                    {
                        qPort = defn.Attributes["port"].Value;
                    }
                    catch (Exception)
                    {
                        logger.Error($"Port not defined for {defn.Attributes["name"].Value}");
                        // return;
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
                        getTimeout = int.Parse(defn.Attributes["getTimeout"].Value);
                    }
                    catch (Exception)
                    {
                        getTimeout = 10;
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
                        //return;
                    }
                }
                catch (AccessViolationException ex)
                {
                    logger.Info(ex.Message);
                    logger.Info(ex.StackTrace);
                    //return;
                }
                catch (Exception ex)
                {
                    logger.Info("Error configuring MQ queue");
                    logger.Info(ex.Message);
                    logger.Info(ex.StackTrace);
                    Console.WriteLine($"Error configuring MQ access for {defn.Attributes["name"].Value}");
                    // return;
                }
            }

            foreach (XmlNode node in configDoc.SelectNodes("//Handlers/Handler"))
            {
                string handlerClass = node.Attributes["class"].Value;
                string handlerMessageType = node.Attributes["messageType"].Value;
                string handlerName = node.Attributes["name"]?.Value;
                string handlerEnabled = node.Attributes["enabled"].Value;

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

                    logger.Info($"Loaded Handler: {handlerClass} ({handlerName}) to handle message type: {handlerMessageType}");

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


            // Post out put handlers are for when the CRUD has been completed. Could be used for handling linking
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


            BaseDataInit baseDataInit = new BaseDataInit(configDoc, this);
            baseDataInit.Sync();
            UpdateFlights();

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

                if (fromIBMMQ) receiveThread = new Thread(this.ListenIBMMQ)
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
                    logger.Warn("No Messages to Process");
                }
                catch (Exception e)
                {
                    logger.Error($"Error in Reciveving and Processing Notification Message. {e.Message}");
                }
            }
            logger.Info("Queue Listener Stopped");
            receiveThread.Abort();
        }

        public void ListenIBMMQ()
        {


            while (startListenLoop)
            {
                try
                {
                    using (MQQueueManager queueManager = new MQQueueManager(qMgr, connectionParams))
                    {
                        var openOptions = MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING;
                        MQQueue queue = queueManager.AccessQueue(queueName, openOptions);

                        MQGetMessageOptions getOptions = new MQGetMessageOptions
                        {
                            WaitInterval = getTimeout,
                            Options = MQC.MQGMO_WAIT
                        };

                        MQMessage msg = new MQMessage
                        {
                            Format = MQC.MQFMT_STRING
                        };

                        queue.Get(msg, getOptions);
                        queue.Close();

                        ProcessMessage( msg.ReadString(msg.MessageLength));
                    }
                }
                catch (Exception ex)
                {
                    // Exception occurs on read timeout or on failure to connect
                    logger.Trace($"Unable to get message from: {queueName} { ex.Message }");
                    continue;
                }
            };
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
            logger.Debug($"Received Unmanaged Message Type");
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
            bool initFlights = false;
            bool.TryParse(configDoc.SelectSingleNode(".//InitFlights")?.InnerText, out initFlights);
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


    }
}
