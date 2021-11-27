
using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerAreaUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "AreaUpdatedNotification";
        public override string HandlerAction { get; } = "UpdateArea";
        public override string HandlerModel => "AMSAMSAdaptor.ModelArea";
        public HandlerAreaUpdate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }

    }
    public class HandlerAreaCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "AreaCreatedNotification";
        public override string HandlerAction { get; } = "CreateArea";
        public override string HandlerModel => "AMSAMSAdaptor.ModelArea";
        public HandlerAreaCreate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    public class HandlerAreaDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "AreaDeletedNotification";
        public override string HandlerAction { get; } = "DeleteArea";
        public override string HandlerModel => "AMSAMSAdaptor.ModelArea";
        public HandlerAreaDelete(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    internal class HandlerArea : HandlerAbstract
    {
        public override string MessageName { get; } = "Area";
        public override string HandlerAction { get; } = "UpdateArea,CreateArea";
        public override string HandlerModel => "AMSAMSAdaptor.ModelArea";
        public HandlerArea(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
}

