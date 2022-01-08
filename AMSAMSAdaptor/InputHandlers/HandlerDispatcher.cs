using NLog;
using System;
using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerDispatcher
    {
        public event Action<XmlNode> TriggerFire;

        public static readonly Logger logger = LogManager.GetLogger("consoleLogger");

        public void Fire(XmlNode args)
        {
            try
            {
                TriggerFire?.Invoke(args);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Internal Dispatcher Error");
                throw;
            }
        }
    }
}