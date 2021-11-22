using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMSAMSAdaptor
{
    internal class TransformerFlightUpdate : ITransformer
    {
        public object Transform(object input)
        {
            FlightModel fl = (FlightModel)input;
           // fl.FlightProperties["Nature"].Value = "WHO REALLY CARES";
            return fl;
        }
    }
}
