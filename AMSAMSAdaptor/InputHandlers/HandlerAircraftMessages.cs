
namespace AMSAMSAdaptor
{
    public class HandlerAircraftUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftUpdatedNotification";
        public override string HandlerAction { get; } = "UpdateAircraft";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraft";

    }
    public class HandlerAircraftCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftCreatedNotification";
        public override string HandlerAction { get; } = "CreateAircraft";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraft";
    }
    public class HandlerAircraftDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftDeletedNotification";
        public override string HandlerAction { get; } = "DeleteAircraft";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraft";
    }
    internal class HandlerAircraft : HandlerAbstract
    {
        public override string MessageName { get; } = "Aircraft";
        public override string HandlerAction { get; } = "UpdateAircraft,CreateAircraft";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraft";
    }
}

