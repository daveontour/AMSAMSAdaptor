using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerFlightRemoveCheckInSlots : ITransformer
    {
        public void SetConfig(XmlNode configNode)
        {
        }

        public object Transform(object input)
        {
            if (input == null) return null;

            ModelFlight fl = (ModelFlight)input;

            XmlNode childNode = fl.node.SelectSingleNode($".//amsx-datatypes:CheckInSlots", fl.nsmgr);
            if (childNode != null)
            {
                childNode.ParentNode.RemoveChild(childNode);
            }
            return fl;
        }
    }

    internal class TransformerFlightRemoveStandSlots : ITransformer
    {
        public void SetConfig(XmlNode configNode)
        {
        }

        public object Transform(object input)
        {
            ModelFlight fl = (ModelFlight)input;

            XmlNode childNode = fl.node.SelectSingleNode($".//amsx-datatypes:StandSlots", fl.nsmgr);
            if (childNode != null)
            {
                childNode.ParentNode.RemoveChild(childNode);
            }
            return fl;
        }
    }

    internal class TransformerFlightRemoveCarouselSlots : ITransformer
    {
        public void SetConfig(XmlNode configNode)
        {
        }

        public object Transform(object input)
        {
            ModelFlight fl = (ModelFlight)input;

            XmlNode childNode = fl.node.SelectSingleNode($".//amsx-datatypes:CarouselSlots", fl.nsmgr);
            if (childNode != null)
            {
                childNode.ParentNode.RemoveChild(childNode);
            }
            return fl;
        }
    }

    internal class TransformerFlightRemoveGateSlots : ITransformer
    {
        public void SetConfig(XmlNode configNode)
        {
        }

        public object Transform(object input)
        {
            ModelFlight fl = (ModelFlight)input;

            XmlNode childNode = fl.node.SelectSingleNode($".//amsx-datatypes:GateSlots", fl.nsmgr);
            if (childNode != null)
            {
                childNode.ParentNode.RemoveChild(childNode);
            }
            return fl;
        }
    }

    internal class TransformerFlightRemoveLoungeSlots : ITransformer
    {
        public void SetConfig(XmlNode configNode)
        {
        }

        public object Transform(object input)
        {
            ModelFlight fl = (ModelFlight)input;

            XmlNode childNode = fl.node.SelectSingleNode($".//amsx-datatypes:LoungeSlots", fl.nsmgr);
            if (childNode != null)
            {
                childNode.ParentNode.RemoveChild(childNode);
            }
            return fl;
        }
    }

    internal class TransformerFlightRemoveVehicleSlots : ITransformer
    {
        public void SetConfig(XmlNode configNode)
        {
        }

        public object Transform(object input)
        {
            ModelFlight fl = (ModelFlight)input;

            XmlNode childNode = fl.node.SelectSingleNode($".//amsx-datatypes:VehicleSlots", fl.nsmgr);
            if (childNode != null)
            {
                childNode.ParentNode.RemoveChild(childNode);
            }
            return fl;
        }
    }
}