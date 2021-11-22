using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMSAMSAdaptor
{
    public interface ITransformer
    {
       object Transform(object input);
    }
}
