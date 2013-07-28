using Newtonsoft.Json;
using System;
using System.Collections.Specialized;

namespace WorldsBestBars.Services.Email.MailChimp
{
    public class Unsubscribe : BaseService
    {
        #region Public Methods

        public void Execute(Guid id)
        {
            var user = Services.Users.Helper.Get(id);

            var url = Shared.GetUri("lists/unsubscribe");
            var values = new NameValueCollection();

            values.Add("apikey", Shared.ApiKey);
            values.Add("id", Shared.ListId);
            values.Add("email", JsonConvert.SerializeObject(new { email = user.Email }));
            values.Add("delete_member", "false");
            values.Add("send_goodbye", "false");
            values.Add("send_notify", "false");
        }

        #endregion
    }
}