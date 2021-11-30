using System.Xml;

namespace AMSAMSAdaptor
{
    public class HandlerCustomsTypeUpdate : HandlerAbstract
    {
        public override string MessageName { get; } = "CustomsTypeUpdatedNotification";
        public override string HandlerAction { get; } = "UpdateCustomsType";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCustomsType";

        public HandlerCustomsTypeUpdate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    public class HandlerCustomsTypeCreate : HandlerAbstract
    {
        public override string MessageName { get; } = "CustomsTypeCreatedNotification";
        public override string HandlerAction { get; } = "CreateCustomsType";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCustomsType";
        public HandlerCustomsTypeCreate(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    public class HandlerCustomsTypeDelete : HandlerAbstract
    {
        public override string MessageName { get; } = "CustomsTypeDeletedNotification";
        public override string HandlerAction { get; } = "DeleteCustomsType";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCustomsType";
        public HandlerCustomsTypeDelete(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
    internal class HandlerCustomsType : HandlerAbstract
    {
        public override string MessageName { get; } = "CustomsType";
        public override string HandlerAction { get; } = "UpdateCustomsType,CreateCustomsType";
        public override string HandlerModel => "AMSAMSAdaptor.ModelCustomsType";
        public HandlerCustomsType(Supervisor supervisor, XmlNode config) : base(supervisor, config) { }
    }
}
