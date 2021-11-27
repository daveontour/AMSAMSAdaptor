

using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerAirlineUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "AirlineUpdatedNotification";
        public override string HandlerAction => "UpdateAirline";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraftType";
        public HandlerAirlineUpdate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }

    }
    public class HandlerAirlineCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "AirlineCreatedNotification";
        public override string HandlerAction => "CreateAirline";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraftType";

        public HandlerAirlineCreate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    public class HandlerAirlineDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "AirlineDeletedNotification";
        public override string HandlerAction => "DeleteAirline";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraftType";

        public HandlerAirlineDelete(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }

    internal class HandlerAirline : HandlerAbstract
    {
        public override string MessageName { get; } = "Airline";
        public override string HandlerAction => "UpdateAirline,CreateAirline";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAirline";
        public HandlerAirline(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
}

