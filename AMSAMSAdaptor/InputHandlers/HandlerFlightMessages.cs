using System;
using System.Xml;

namespace AMSAMSAdaptor
{

    internal class HandlerFlightUpdate : HandlerAbstract
    {
        public override string MessageName  => "FlightUpdatedNotification";
        public override string HandlerAction => "UpdateFlight";
        public override string HandlerModel => "AMSAMSAdaptor.ModelFlight";
        public override string HandlerDestination => "FlightDataDistributor";
        public HandlerFlightUpdate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    internal class HandlerFlightDelete : HandlerAbstract
    {
        public override string MessageName  => "FlightDeletedNotification";
        public override string HandlerAction => "DeleteFlight";
        public override string HandlerModel => "AMSAMSAdaptor.ModelFlight";
        public override string HandlerDestination => "FlightDataDistributor";
        public HandlerFlightDelete(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    internal class HandlerFlightCreate : HandlerAbstract, IDisposable
    {
        public override string MessageName  => "FlightCreatedNotification";
        public override string HandlerAction => "CreateFlight";
        public override string HandlerModel => "AMSAMSAdaptor.ModelFlight";
        public override string HandlerDestination => "FlightDataDistributor";
        public HandlerFlightCreate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }

    }
    internal class HandlerFlight : HandlerAbstract
    {
        public override string MessageName => "Flight";
        public override string HandlerAction => "UpdateFlight";
        public override string HandlerModel => "AMSAMSAdaptor.ModelFlight";
        public override string HandlerDestination => "FlightDataDistributor";
        public HandlerFlight(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
}
