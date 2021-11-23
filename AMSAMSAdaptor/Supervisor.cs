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

        public event Action<string, string> SendSoapMessageHandler;

        private List<IInputMessageHandler> InputHandlers = new List<IInputMessageHandler>();
        public ConcurrentDictionary<string, HandlerDispatcher> Dispatchers = new ConcurrentDictionary<string,HandlerDispatcher>();

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

            foreach(XmlNode node in configDoc.SelectNodes("//ProcessMessages/Message"))
            {
                managedMessages.Add(node.InnerText);
                AddDispatcher(node.InnerText);
                logger.Info($"Managing Message Type: {node.InnerText}");
            }


            logger.Info($"AMS-AMS Adaptor Service Starting ({Parameters.VERSION})");

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
                    if (this.managedMessages.Contains(obj.GetMessageName())){
                        InputHandlers.Add(obj);
                        logger.Info($"Implemented Handler for {obj.GetMessageName()}");
                    } else
                    {
                        logger.Warn($"Found Handler for {obj.GetMessageName()} but the messages type is not configured to be handled");
                    }
                } catch (Exception)
                {
                    //logger.Error(ex.Message);
                }
            }

            //Set Up Sender for testing
            SendSoapMessageHandler += TestSender;


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

            foreach (string messageType in managedMessages)
            {
                try
                {
                    if (newNode.SelectSingleNode($"//ams:{messageType}", nsmgr) != null)
                    {
                        logger.Debug("Received Managed Message Type");
                        Dispatchers[messageType].Fire(newNode);
                        return;
                    }
                } catch (Exception e)
                {
                    logger.Error($"Error firing dispatcher for {messageType}. Error: {e.Message}");
                    return;
                }
            }
            logger.Debug("Received Unmanaged Message Type");
        }

        public void SendSoapMessage(string message, string soapAction)
        {
            SendSoapMessageHandler?.Invoke(message, soapAction);
        }

        private void TestSender(string arg1, string arg2)
        {
            Console.WriteLine(PrintXML(arg1));
        }


        public static string PrintXML(string xml)
        {
            string result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXml = sReader.ReadToEnd();

                result = formattedXml;
            }
            catch (XmlException)
            {
                // Handle the exception
            }

            mStream.Close();
            writer.Close();

            return result;
        }
    }
}
