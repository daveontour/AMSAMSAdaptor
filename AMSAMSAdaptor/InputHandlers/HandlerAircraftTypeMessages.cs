

namespace AMSAMSAdaptor
{
    public class HandlerAircraftTypeUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftTypeUpdatedNotification";
        public override string HandlerAction => "UpdateAircraftType";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraftType";
    }
    public class HandlerAircraftTypeCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftTypeCreatedNotification";
        public override string HandlerAction => "CreateAircraftType";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraftType";
    }
    public class HandlerAircraftTypeDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftTypeDeletedNotification";
        public override string HandlerAction => "DeleteAircraftType";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraftType";
    }
    internal class HandlerAircraftType : HandlerAbstract
    {
        public override string MessageName { get; } = "AircraftType";
        public override string HandlerAction => "UpdateAircraftType,CreateAircraftType";
        public override string HandlerModel => "AMSAMSAdaptor.ModelAircraftType";
    }
}

