using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

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
            mergeVars.Add("FNAME", name[0]);
            if (name.Length > 1)
            {
                mergeVars.Add("LNAME", string.Join(" ", name.Skip(1)));
            }

            var groupings = GetGroupings(user);
            if (groupings != null)
            {
                mergeVars.Add("groupings", groupings);
            }

            values.Add("apikey", Shared.ApiKey);
            values.Add("id", Shared.ListId);
            values.Add("email", user.Email);
            values.Add("merge_vars", JsonConvert.SerializeObject(mergeVars));
            values.Add("double_optin", "false");
            values.Add("update_existing", "false");
            values.Add("replace_interests", "false");
            values.Add("send_welcome", "false");
        }

        #endregion

        #region Private Methods

        static string[] GetGroupings(Services.Models.UserSummary user)
        {
            var location = Services.Users.Helper.GetNormalisedCity(user.City);
            if (location == null) { return null; }

            return new string[] { location };
        }

        #endregion
    }
}