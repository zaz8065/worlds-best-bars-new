using System;
using System.Linq;

using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [Serializable()]
    [JsonObject()]
    public class AdvertStat
    {
        [JsonProperty("period")]
        public DateTime Period { get; set; }

        [JsonProperty("clicks")]
        public int Clicks { get; set; }

        [JsonProperty("impressions")]
        public int Impressions { get; set; }

        public static AdvertStat Convert(Data.AdvertStat input)
        {
            if (input == null) { return null; }
            return new AdvertStat()
            {
                Period = input.Period,
                Clicks = input.Clicks,
                Impressions = input.Impressions
            };
        }

        public static AdvertStat[] GetByAd(Guid ad, DateTime? limit = null)
        {
            using (var context = new Data.Entities())
            {
                if (limit != null)
                {
                    return context.AdvertStats.Where(s => s.AdvertId == ad && (limit == null || s.Period > (DateTime)limit)).OrderByDescending(s => s.Period).ToList().Select(a => Convert(a)).ToArray();
                }
                else
                {
                    return context.AdvertStats.Where(s => s.AdvertId == ad).OrderByDescending(s => s.Period).ToList().Select(a => Convert(a)).ToArray();
                }
            }
        }
    }
}