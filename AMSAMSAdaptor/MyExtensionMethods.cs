using System;
using System.IO;
using System.Text;
using System.Xml;

namespace AMSAMSAdaptor
{
    public static class MyExtensionMethods
    {
        public static string PrintXML(this XmlNode node)
        {
            return node.OuterXml.PrintXML();
        }

        public static string PrintXML(this StringBuilder sb)
        {
            return sb.ToString().PrintXML();
        }

        public static string PrintXML(this string xml)
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
            catch (Exception)
            {
                // NO-OP
            }

            mStream.Close();
            writer.Close();

            return result;
        }
    }
}