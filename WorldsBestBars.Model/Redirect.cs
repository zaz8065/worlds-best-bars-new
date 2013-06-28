using System;
using System.Linq;

using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [Serializable()]
    [JsonObject()]
    public class Redirect
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("inbound")]
        public string Inbound { get; set; }

        [JsonProperty("outbound")]
        public string Outbound { get; set; }

        public static Redirect Convert(Data.Redirect input)
        {
            if (input == null) { return null; }
            return new Redirect()
            {
                Id = input.Id,
                Inbound = input.Inbound,
                Outbound = input.Outbound
            };
        }

        public static Redirect[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                return context.Redirects.ToList().Select(r => Convert(r)).ToArray();
            }
        }

        public static Redirect GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                return Convert(context.Redirects.SingleOrDefault(r => r.Id == id));
            }
        }

        public static string GetByInbound(string inbound, bool trackHit = false)
        {
            using (var context = new Data.Entities())
            {
                var redirect = context.Redirects.SingleOrDefault(r => r.Inbound == inbound);
                
                if (redirect == null)
                {
                    return null;
                }

                if (trackHit)
                {
                    redirect.Hits++;
                    redirect.LastHit = DateTime.Now;

                    context.SaveChanges();
                }

                return redirect.Outbound;
            }
        }
    }
}