using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [Serializable()]
    [JsonObject()]
    public class Opening
    {
        [JsonProperty("day")]
        public DayOfWeek Day { get; set; }

        [JsonProperty("open")]
        public TimeSpan Open { get; set; }

        [JsonProperty("close")]
        public TimeSpan Close { get; set; }

        public static Opening Convert(Data.BarOpening input)
        {
            if (input == null) { return null; }
            return new Opening()
            {
                Open = input.Open,
                Close = input.Close,
                Day = (DayOfWeek)input.Day
            };
        }

        public static Opening[] GetByBar(Guid id, bool activeOnly = true)
        {
            using (var context = new Data.Entities())
            {
                return context.BarOpenings.Where(r => r.BarId == id).ToList().Select(r => Convert(r)).ToArray();
            }
        }
    }
}
