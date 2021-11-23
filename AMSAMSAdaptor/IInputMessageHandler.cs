using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    public interface IInputMessageHandler
    {
        void SetSupervisor(Supervisor supervisor, XmlDocument configDoc);
        string GetMessageName();
    }

    public interface IOutputMessageHandler
    {
        void SetSupervisor(Supervisor supervisor, XmlDocument configDoc);
        string GetDescription();
    }
}
