using System;
using System.Configuration;
using System.Collections.Specialized;

namespace WorldsBestBars.Logic
{
    public static class Config
    {
        public static class Facebook
        {
            static Facebook()
            {
                var facebook = ConfigurationManager.GetSection("worldsbestbars/facebook") as NameValueCollection;

                AppId = facebook["AppId"];
                AppSecret = facebook["AppSecret"];
            }

            public static string AppId { get; private set; }
            public static string AppSecret { get; private set; }
        }
        public static class Google
        {
            static Google()
            {
                var google = ConfigurationManager.GetSection("worldsbestbars/google") as NameValueCollection;

                AnalyticsTrackingCode = google["AnalyticsTrackingCode"];
            }

            public static string AnalyticsTrackingCode { get; private set; }
        }
    }
}
