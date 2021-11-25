
namespace AMSAMSAdaptor
{
    public class HandlerAreaUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "AreaUpdatedNotification";
        public override string HandlerAction { get; } = "UpdateArea";
        public override string HandlerModel => "AMSAMSAdaptor.ModelArea";

    }
    public class HandlerAreaCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "AreaCreatedNotification";
        public override string HandlerAction { get; } = "CreateArea";
        public override string HandlerModel => "AMSAMSAdaptor.ModelArea";
    }
    public class HandlerAreaDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "AreaDeletedNotification";
        public override string HandlerAction { get; } = "DeleteArea";
        public override string HandlerModel => "AMSAMSAdaptor.ModelArea";
    }
    internal class HandlerArea : HandlerAbstract
    {
        public override string MessageName { get; } = "Area";
        public override string HandlerAction { get; } = "UpdateArea,CreateArea";
        public override string HandlerModel => "AMSAMSAdaptor.ModelArea";
    }
}

