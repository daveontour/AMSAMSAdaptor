using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    public interface ITransformer
    {
       object Transform(object input);
        void SetConfig(XmlNode configNode);
    }
}
