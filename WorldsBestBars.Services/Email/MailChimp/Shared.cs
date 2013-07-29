using System;
using System.Configuration;

namespace WorldsBestBars.Services.Email.MailChimp
{
    internal class Shared
    {
        #region Internal Methods

        internal static Uri GetUri(string action)
        {
            var ret = new UriBuilder();

            ret.Scheme = "https";
            ret.Host = string.Format("{0}.api.mailchimp.com", ApiKey.Substring(ApiKey.Length - 3));
            ret.Path = string.Format("2.0/{0}.json", action);

            return ret.Uri;
        }

        #endregion

        #region Properties

        internal static string ApiKey { get { return ConfigurationManager.AppSettings["mailchimp:api_key"]; } }
        internal static string ListId { get { return ConfigurationManager.AppSettings["mailchimp:list_id"]; } }

        #endregion
    }
}
