using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace WorldsBestBars.Services.Email.MailChimp
{
    public class Subscribe : BaseService
    {
        #region Public Methods

        public void Execute(Guid id)
        {
            var user = Services.Users.Helper.Get(id);

            var url = Shared.GetUri("lists/subscribe");
            var values = new NameValueCollection();
            var mergeVars = new Dictionary<string, object>();

            var name = user.Name.Split(' ');
            mergeVars.Add("FNAME", RemoveDiacritics(name[0]));
            if (name.Length > 1)
            {
                mergeVars.Add("LNAME", RemoveDiacritics(string.Join(" ", name.Skip(1))));
            }

            var groupings = GetGroupings(user);
            if (groupings != null)
            {
                mergeVars.Add("groupings", groupings);
            }

            var reqdata = new
            {
                apikey = Shared.ApiKey,
                id = Shared.ListId,
                email = new
                {
                    email = user.Email
                },
                merge_vars = mergeVars,
                double_optin = false,
                update_existing = false,
                replace_interests = false,
                send_welcome = false
            };

            using (var client = new WebClient())
            {
                client.UploadString(url, JsonConvert.SerializeObject(reqdata));
            }
        }

        #endregion

        #region Private Methods

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

        #endregion
    }
}