

namespace AMSAMSAdaptor
{
    public class HandlerAirportUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "AirportUpdatedNotification";
        public override string HandlerAction => "UpdateAirport";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAirport";

    }
    public class HandlerAirportCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "AirportCreatedNotification";
        public override string HandlerAction => "CreateAirport";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAirport";
    }
    public class HandlerAirportDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "AirportDeletedNotification";
        public override string HandlerAction => "DeleteAirport";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAirport";
    }

    internal class HandlerAirport : HandlerAbstract
    {
        public override string MessageName { get; } = "Airport";
        public override string HandlerAction => "UpdateAirport";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAirport";

    }
}

