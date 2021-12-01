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
        public string AltrernatePropertyName { get; set; }
        public string DatetimeFormat { get; set; }
        public string PropertyFixedValue { get; set; }
        public string LookupPropertyName { get; set; }
        public string FileName { get; set; }
        public string IndexField { get; set; }
        public string ValueField { get; set; }
        public bool TimeOffset { get; internal set; }
        public string TimeFormat { get; internal set; }
        public string TimeReferenceProperty { get; internal set; }

        public Dictionary<string, string> LookupDict = new Dictionary<string, string>();
    }
}
