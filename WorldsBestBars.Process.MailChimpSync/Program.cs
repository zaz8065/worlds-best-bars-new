#define TRACE

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace WorldsBestBars.Process.MailChimpSync
{
    class Program
    {
        static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            _log.Info("www.worldsbestbars.com - MailChimp Synchronisation");
            _log.Debug("Mailchimp API Base: {0}", GetUri("helpers/ping"));
            _log.Debug("Mailchimp API Key: {0}", ApiKey);

            _log.Info("Getting a list of users subscribed in database.");

            var local = GetLocalUsers();

            _log.Info("Getting a list of users already on MailChimp.");

            var mailchimp = GetMailChimpUsers();

            _log.Info("Calculating which users to add/remove.");

            var localEmails = local.Where(e => !string.IsNullOrEmpty(e.Email)).Select(e => e.Email.ToLower());
            var mailchimpEmails = mailchimp.Select(e => e.Email.ToLower());

            var addEmails = localEmails.Except(mailchimpEmails);

            var add = new List<Services.Models.UserSummary>();

            foreach (var email in addEmails)
            {
                var entity = local.FirstOrDefault(e => e.Email == email);
                if (entity != null)
                {
                    add.Add(entity);
                }
            }

            _log.Info("{0:N0} users to add", add.Count());

            if (add.Any())
            {
                _log.Info("Subscribing...");
                BatchSubscribe(add);
            }
            else
            {
                _log.Info("No new users to subscribe.");
            }

#if DEBUG
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
#endif
        }

        #region Private Methods

        static Uri GetUri(string action)
        {
            var ret = new UriBuilder();

            ret.Scheme = "https";
            ret.Host = string.Format("{0}.api.mailchimp.com", ApiKey.Substring(ApiKey.Length - 3));
            ret.Path = string.Format("2.0/{0}.json", action);

            return ret.Uri;
        }

        static IEnumerable<Services.Models.UserSummary> GetLocalUsers()
        {
            var service = new Services.Users.GetSubscribed();

            return service.Execute();
        }

        static IEnumerable<MailChimpUser> GetMailChimpUsers()
        {
            var url = GetUri("lists/members");

            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values.Add("apikey", ApiKey);
                values.Add("id", ListId);

                var resp = Encoding.UTF8.GetString(client.UploadValues(url, values));

                var data = JsonConvert.DeserializeObject<MailChimpMemberListsResponse>(resp);

                return data.Users;
            }
        }

        static void BatchSubscribe(IEnumerable<Services.Models.UserSummary> users)
        {
            const int BatchSize = 5000;
            var mc = users.Select(e => new MailChimpBatchSubscribe(e));
            var batches = mc.Select((e, i) => new { Index = i, E = e }).GroupBy(e => (int)(e.Index / BatchSize)).Select(g => g.Select(e => e.E));

            foreach (var batch in batches)
            {
                var url = GetUri("lists/batch-subscribe");

                var reqdata = new
                {
                    apikey = ApiKey,
                    id = ListId,
                    double_optin = false,
                    update_existing = true,
                    replace_interests = false,
                    batch = batch
                };

                using (var client = new WebClient())
                {
                    try
                    {
                        var resp = client.UploadString(url, JsonConvert.SerializeObject(reqdata));

                        var data = JsonConvert.DeserializeObject(resp);
                    }
                    catch (WebException wex)
                    {
                        var content = new System.IO.StreamReader(wex.Response.GetResponseStream()).ReadToEnd();

                        _log.Info("Request data: " + JsonConvert.SerializeObject(reqdata));

                        _log.Error(content);

                        throw;
                    }
                }
            }
        }

        static void BatchUnsubscribe(IEnumerable<MailChimpUser> users)
        {
            var url = GetUri("lists/batch-unsubscribe");

            var values = new NameValueCollection();
            values.Add("apikey", ApiKey);
            values.Add("id", ListId);
            values.Add("delete_member", "true");
            values.Add("send_goodbye", "false");
            values.Add("send_notify", "false");
            values.Add("batch", JsonConvert.SerializeObject(users));

            using (var client = new WebClient())
            {
                var response = Encoding.UTF8.GetString(client.UploadValues(url, values));

                var data = JsonConvert.DeserializeObject(response);
            }
        }

        #endregion

        #region Properties

        static string ApiKey { get { return ConfigurationManager.AppSettings["mailchimp:api_key"]; } }
        static string ListId { get { return ConfigurationManager.AppSettings["mailchimp:list_id"]; } }

        #endregion

        #region Classes

        class MailChimpMemberListsResponse
        {
            [JsonProperty("total")]
            public int Total { get; set; }

            [JsonProperty("data")]
            public IEnumerable<MailChimpUser> Users { get; set; }
        }

        class MailChimpUser
        {
            [JsonProperty("euid")]
            public string Id { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }
        }

        class MailChimpEmail
        {
            [JsonProperty("email")]
            public string Email { get; set; }
        }

        class MailChimpBatchSubscribe
        {
            [JsonProperty("email")]
            public MailChimpEmail Email { get; set; }

            [JsonProperty("merge_vars")]
            public Dictionary<string, object> MergeVars { get; set; }

            public MailChimpBatchSubscribe(Services.Models.UserSummary user)
            {
                MergeVars = new Dictionary<string, object>();
#if DEBUG
                Email = new MailChimpEmail { Email = string.Format("hello+{0}@brendanmckenzie.com", user.Email.Replace("@", "_at_").Replace("+", "_plus_").Replace(".", "_dot_")) };
#else
                Email = new MailChimpEmail { Email = user.Email };
#endif
                var name = user.Name.Split(' ');
                MergeVars.Add("FNAME", RemoveDiacritics(name[0]));
                if (name.Length > 1)
                {
                    MergeVars.Add("LNAME", RemoveDiacritics(string.Join(" ", name.Skip(1))));
                }

                var groupings = GetGroupings(user);
                if (groupings != null)
                {
                    MergeVars.Add("groupings", groupings);
                }
            }

            static string[] GetGroupings(Services.Models.UserSummary user)
            {
                var location = Services.Users.Helper.GetNormalisedCity(user.City);
                if (location == null) { return null; }

                return new string[] { location };
            }

            static string RemoveDiacritics(string text)
            {
                var normalizedString = text.Normalize(NormalizationForm.FormD);
                var stringBuilder = new StringBuilder();

                foreach (var c in normalizedString)
                {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    {
                        stringBuilder.Append(c);
                    }
                }

                var ret = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

                var re = new Regex("[^a-zA-Z0-9 -]");
                ret = re.Replace(ret, string.Empty);

                return ret;
            }
        }

        #endregion
    }
}