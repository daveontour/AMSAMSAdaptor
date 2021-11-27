using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerGateUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "GateUpdatedNotification";
        public override string HandlerAction { get; } = "UpdateGate";
        public override string HandlerModel => "AMSAMSAdaptor.ModelGate";

        public HandlerGateUpdate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    public class HandlerGateCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "GateCreatedNotification";
        public override string HandlerAction { get; } = "CreateGate";
        public override string HandlerModel => "AMSAMSAdaptor.ModelGate";
        public HandlerGateCreate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    public class HandlerGateDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "GateDeletedNotification";
        public override string HandlerAction { get; } = "DeleteGate";
        public override string HandlerModel => "AMSAMSAdaptor.ModelGate";
        public HandlerGateDelete(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    internal class HandlerGate : HandlerAbstract
    {
        public override string MessageName { get; } = "Gate";
        public override string HandlerAction { get; } = "UpdateGate,CreateGate";
        public override string HandlerModel => "AMSAMSAdaptor.ModelGate";
        public HandlerGate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
}
