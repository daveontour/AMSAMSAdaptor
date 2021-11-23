using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    internal class OutputFlightSOAP : IOutputMessageHandler
    {

        public void SetSupervisor(Supervisor supervisor, XmlDocument configDoc)
        {
            supervisor.SendFlightMessageHandler += Sender;
        }

        public string GetDescription()
        {
            return "Send Flight Messages via Web Services SOAP Interface";
        }

        private void Sender(FlightModel flt, string action)
        {
            if (action.Contains("CreateFlight"))
                Console.WriteLine(PrintXML(flt.GetCreateSoapMessage()));
            if (action.Contains("UpdateFlight"))
                Console.WriteLine(PrintXML(flt.GetUpdateSoapMessage()));
            if (action.Contains("DeleteFlight"))
                Console.WriteLine(PrintXML(flt.GetDeleteSoapMessage()));
        }

        public static string PrintXML(string xml)
        {
            string result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXml = sReader.ReadToEnd();

                result = formattedXml;
            }
            catch (XmlException)
            {
                // Handle the exception
            }

            mStream.Close();
            writer.Close();

            return result;
        }
    }
}
