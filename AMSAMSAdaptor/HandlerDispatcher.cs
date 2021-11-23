using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerDispatcher
    {
        public event Action<XmlNode> TriggerFire;

        public static readonly Logger sourceLogger = LogManager.GetLogger("sourceLogger");

        public void Fire(XmlNode args)
        {
            try
            {
                TriggerFire?.Invoke(args);
            }
            catch (Exception ex)
            {
                sourceLogger.Error(ex, "Internal Dispatcher Error");
                throw;
            }
        }
    }
}
