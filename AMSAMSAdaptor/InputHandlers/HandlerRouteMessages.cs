using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerRouteUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "RouteUpdatedNotification";
        public override string HandlerAction { get; } = "UpdateRoute";
        public override string HandlerModel => "AMSAMSAdaptor.ModelRoute";

        public HandlerRouteUpdate(Supervisor supervisor, XmlNode config) : base(supervisor, config)
        {
        }
    }

    public class HandlerRouteCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "RouteCreatedNotification";
        public override string HandlerAction { get; } = "CreateRoute";
        public override string HandlerModel => "AMSAMSAdaptor.ModelRoute";

        public HandlerRouteCreate(Supervisor supervisor, XmlNode config) : base(supervisor, config)
        {
        }
    }

    public class HandlerRouteDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "RouteDeletedNotification";
        public override string HandlerAction { get; } = "DeleteRoute";
        public override string HandlerModel => "AMSAMSAdaptor.ModelRoute";

        public HandlerRouteDelete(Supervisor supervisor, XmlNode config) : base(supervisor, config)
        {
        }
    }

    internal class HandlerRoute : HandlerAbstract
    {
        public override string MessageName { get; } = "Route";
        public override string HandlerAction { get; } = "UpdateRoute,CreateRoute";
        public override string HandlerModel => "AMSAMSAdaptor.ModelRoute";

        public HandlerRoute(Supervisor supervisor, XmlNode config) : base(supervisor, config)
        {
        }
    }
}