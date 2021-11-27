using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerCheckInUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "CheckInUpdatedNotification";
        public override string HandlerAction { get; } = "UpdateCheckIn";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCheckIn";
        public HandlerCheckInUpdate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    public class HandlerCheckInCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "CheckInCreatedNotification";
        public override string HandlerAction { get; } = "CreateCheckIn";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCheckIn";
        public HandlerCheckInCreate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    public class HandlerCheckInDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "CheckInDeletedNotification";
        public override string HandlerAction { get; } = "DeleteCheckIn";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCheckIn";
        public HandlerCheckInDelete(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    internal class HandlerCheckIn : HandlerAbstract
    {
        public override string MessageName { get; } = "CheckIn";
        public override string HandlerAction { get; } = "UpdateCheckIn,CreateCheckin";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCheckIn";
        public HandlerCheckIn(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
}
