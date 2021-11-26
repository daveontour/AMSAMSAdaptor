using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class TransformerFlightRemoveCustomTable : ITransformer
    {
        private List<string> _tables = new List<string>();
        public void SetConfig(XmlNode configNode)
        {
            foreach (XmlNode property in configNode.SelectNodes(".//Property"))
            {
                _tables.Add(property.Attributes["tableName"]?.Value);
            }
        }

        public object Transform(object input)
        {
            ModelFlight fl = (ModelFlight)input;
            foreach(string table in _tables)
            {
                if (table != null)
                {
                    XmlNode childNode = fl.node.SelectSingleNode($".//TableValue[@propertyName='{table}']", fl.nsmgr);
                    if (childNode != null)
                    {
                        childNode.ParentNode.RemoveChild(childNode);
                    }
                }  
               
            }
            return fl;
        }
    }
}
