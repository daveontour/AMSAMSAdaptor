using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMSAMSAdaptor
{
    public class ReMapper
    {
        public string Type { get; set; }
        public string PropertyName { get; set; }
        public bool IsFixedValue { get { return (Type == "fixed"); } }
        public bool IsDatetime { get { return (Type == "dateTime"); } }
        public bool IsAlternateProperty { get { return (Type == "alternateProperty"); } }
        public string AltrernatePropertyName { get; set; }
        public string DatetimeFormat { get; set; }
        public string PropertyFixedValue { get; set; }
    }
}
