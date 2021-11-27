namespace AMSAMSAdaptor
{
    public class HandlerStandUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "StandUpdatedNotification";
        public override string HandlerAction { get; } = "UpdateStand";
        public override string HandlerModel => "AMSAMSAdaptor.ModelStand";

    }
    public class HandlerStandCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "StandCreatedNotification";
        public override string HandlerAction { get; } = "CreateStand";
        public override string HandlerModel => "AMSAMSAdaptor.ModelStand";
    }
    public class HandlerStandDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "StandDeletedNotification";
        public override string HandlerAction { get; } = "DeleteStand";
        public override string HandlerModel => "AMSAMSAdaptor.ModelStand";
    }
    internal class HandlerStand : HandlerAbstract
    {
        public override string MessageName { get; } = "Stand";
        public override string HandlerAction { get; } = "UpdateStand,CreateStand";
        public override string HandlerModel => "AMSAMSAdaptor.ModelStand";
    }
}
