using System.Xml;

namespace AMSAMSAdaptor
{
    public interface IInputMessageHandler
    {
        void SetSupervisor(Supervisor supervisor, XmlDocument configDoc);
        string GetMessageName();
        string HandlerName { get; }
        string HandlerAction { get; }
    }

    public interface IOutputMessageHandler
    {
        void SetSupervisor(Supervisor supervisor, XmlDocument configDoc);
        string GetDescription();
    }

    public interface IPostOutputMessageHandler
    {
        void SetSupervisor(Supervisor supervisor, XmlDocument configDoc);
        string GetDescription();
    }
}
