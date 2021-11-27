namespace AMSAMSAdaptor
{
    public class HandlerGateUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "GateUpdatedNotification";
        public override string HandlerAction { get; } = "UpdateGate";
        public override string HandlerModel => "AMSAMSAdaptor.ModelGate";

    }
    public class HandlerGateCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "GateCreatedNotification";
        public override string HandlerAction { get; } = "CreateGate";
        public override string HandlerModel => "AMSAMSAdaptor.ModelGate";
    }
    public class HandlerGateDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "GateDeletedNotification";
        public override string HandlerAction { get; } = "DeleteGate";
        public override string HandlerModel => "AMSAMSAdaptor.ModelGate";
    }
    internal class HandlerGate : HandlerAbstract
    {
        public override string MessageName { get; } = "Gate";
        public override string HandlerAction { get; } = "UpdateGate,CreateGate";
        public override string HandlerModel => "AMSAMSAdaptor.ModelGate";
    }
}
