using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMSAMSAdaptor
{
    public class Parameters
    {

        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static string FROMTOKEN;
        public static string TOTOKEN;
        public static string AMS_REST_SERVICE_URI;
        public static string FROM_AMS_WEB_SERVICE_URI;
        public static string TO_AMS_WEB_SERVICE_URI;
        public static string APT_CODE;
        public static string RECVQ;
        public static string TOAMSREQQ;
        public static string FLIGHTALERTFIELD;
        public static string APT_CODE_ICAO;
        public static int REFRESH_INTERVAL;
        public static int GRACE_PERIOD;
        public static double FROM_HOURS;
        public static double TO_HOURS;
        public static int RESTSERVER_RETRY_INTERVAL;
        public static bool STARTUP_FLIGHT_PROCESSING;
        public static bool STARTUP_STAND_PROCESSING;
        public static string VERSION = "Version 1.0.0, 20211123";
        public static bool DEEPTRACE;
        public static bool ALERT_FLIGHT;
        public static bool ALERT_STAND;

        public static string STANDALERTFIELD;
        public static string STANDTOWIDFIELD;
        public static int LOADCHUNKHOURS;
        public static int READ_MESSAGE_LOOP_INTERVAL;

        public static int START_FROM_HOURS;
        public static int START_TO_HOURS;

        static Parameters()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("widget.config.xml");
                FROMTOKEN = doc.SelectSingleNode(".//FromToken")?.InnerText;
                TOTOKEN = doc.SelectSingleNode(".//ToToken")?.InnerText;
                RECVQ = doc.SelectSingleNode(".//FromAMSQueue")?.InnerText;
                TOAMSREQQ = doc.SelectSingleNode(".//ToAMSRequestQueue")?.InnerText;
                FROM_AMS_WEB_SERVICE_URI = doc.SelectSingleNode(".//FromAMSWebServiceURL")?.InnerText;
                TO_AMS_WEB_SERVICE_URI = doc.SelectSingleNode(".//ToAMSWebServiceURL")?.InnerText;
                APT_CODE = doc.SelectSingleNode(".//AirportCode")?.InnerText;

                if (!int.TryParse(doc.SelectSingleNode(".//ReadMessageLoopInterval")?.InnerText, out READ_MESSAGE_LOOP_INTERVAL))
                {
                    READ_MESSAGE_LOOP_INTERVAL = 5;
                }
                if (!int.TryParse(doc.SelectSingleNode(".//StartFromHours")?.InnerText, out START_FROM_HOURS))
                {
                    START_FROM_HOURS = -24;
                }
                if (!int.TryParse(doc.SelectSingleNode(".//StartToHours")?.InnerText, out START_TO_HOURS))
                {
                    START_TO_HOURS = 25;
                }

                //APT_CODE = (string)ConfigurationManager.AppSettings["IATAAirportCode"];
                //APT_CODE_ICAO = (string)ConfigurationManager.AppSettings["ICAOAirportCode"];
                //AMS_REST_SERVICE_URI = (string)ConfigurationManager.AppSettings["AMSRestServiceURI"];
                //AMS_WEB_SERVICE_URI = (string)ConfigurationManager.AppSettings["AMSWebServiceURI"];
                //GRACE_PERIOD = Int32.Parse((string)ConfigurationManager.AppSettings["GracePeriod"]);
                //REFRESH_INTERVAL = Int32.Parse((string)ConfigurationManager.AppSettings["RefreshInterval"]);
                //RESTSERVER_RETRY_INTERVAL = Int32.Parse((string)ConfigurationManager.AppSettings["ResetServerRetryInterval"]);
                //FROM_HOURS = double.Parse((string)ConfigurationManager.AppSettings["FromHours"]);
                //TO_HOURS = double.Parse((string)ConfigurationManager.AppSettings["ToHours"]);

                //STARTUP_FLIGHT_PROCESSING = bool.Parse((string)ConfigurationManager.AppSettings["StartUpFlightProcessing"]);
                //STARTUP_STAND_PROCESSING = bool.Parse((string)ConfigurationManager.AppSettings["StartUpStandProcessing"]);

                //ALERT_FLIGHT = bool.Parse((string)ConfigurationManager.AppSettings["AlertFlight"]);
                //ALERT_STAND = bool.Parse((string)ConfigurationManager.AppSettings["AlertStand"]);


                //LOADCHUNKHOURS = Int32.Parse((string)ConfigurationManager.AppSettings["LoadChunkHours"]);

                //if (!int.TryParse(doc.SelectSingleNode(".//ReadMessageLoopInterval")?.InnerText, out READ_MESSAGE_LOOP_INTERVAL)){
                //    READ_MESSAGE_LOOP_INTERVAL = 5;
                //}

                //try
                //{
                //    DEEPTRACE = bool.Parse((string)ConfigurationManager.AppSettings["DeepTrace"]);
                //}
                //catch (Exception)
                //{
                //    DEEPTRACE = false;
                //}
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
    }
}
