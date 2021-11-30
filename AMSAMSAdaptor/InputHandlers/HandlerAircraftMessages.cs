
using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerAircraftUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftUpdatedNotification";
        public override string HandlerAction { get; } = "UpdateAircraft,CreateAircraft";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraft";
        public HandlerAircraftUpdate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }

    }
    public class HandlerAircraftCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftCreatedNotification";
        public override string HandlerAction { get; } = "CreateAircraft";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraft";
        public HandlerAircraftCreate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    public class HandlerAircraftDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftDeletedNotification";
        public override string HandlerAction { get; } = "DeleteAircraft";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraft";
        public HandlerAircraftDelete(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    internal class HandlerAircraft : HandlerAbstract
    {
        public override string MessageName { get; } = "Aircraft";
        public override string HandlerAction { get; } = "UpdateAircraft,CreateAircraft";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraft";
        public HandlerAircraft(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
}

