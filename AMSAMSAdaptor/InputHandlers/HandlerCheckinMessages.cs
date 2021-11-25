namespace AMSAMSAdaptor
{
    public class HandlerCheckInUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "CheckInUpdatedNotification";
        public override string HandlerAction { get; } = "UpdateCheckIn";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCheckIn";

    }
    public class HandlerCheckInCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "CheckInCreatedNotification";
        public override string HandlerAction { get; } = "CreateCheckIn";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCheckIn";
    }
    public class HandlerCheckInDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "CheckInDeletedNotification";
        public override string HandlerAction { get; } = "DeleteCheckIn";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCheckIn";
    }
    internal class HandlerCheckIn : HandlerAbstract
    {
        public override string MessageName { get; } = "CheckIn";
        public override string HandlerAction { get; } = "UpdateCheckIn,CreateCheckin";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCheckIn";
    }
}
