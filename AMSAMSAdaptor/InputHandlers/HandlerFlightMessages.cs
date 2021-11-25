using System;


namespace AMSAMSAdaptor
{
    internal class HandlerFlight : HandlerAbstract
    {
        public override string MessageName  => "Flight";
        public override string HandlerAction => "UpdateFlight";
        public override string HandlerModel => "AMSAMSAdaptor.ModeFlight";
        public override string HandlerDestination => "FlightDataDistributor";

    }
    internal class HandlerFlightUpdate : HandlerAbstract
    {
        public override string MessageName  => "FlightUpdatedNotification";
        public override string HandlerAction => "UpdateFlight";
        public override string HandlerModel => "AMSAMSAdaptor.ModeFlight";
        public override string HandlerDestination => "FlightDataDistributor";
    }
    internal class HandlerFlightDelete : HandlerAbstract
    {
        public override string MessageName  => "FlightDeletedNotification";
        public override string HandlerAction => "DeleteFlight";
        public override string HandlerModel => "AMSAMSAdaptor.ModeFlight";
        public override string HandlerDestination => "FlightDataDistributor";
    }
    internal class HandlerFlightCreate : HandlerAbstract, IDisposable
    {
        public override string MessageName  => "FlightCreatedNotification";
        public override string HandlerAction => "CreateFlight";
        public override string HandlerModel => "AMSAMSAdaptor.ModeFlight";
        public override string HandlerDestination => "FlightDataDistributor";

    }
}
