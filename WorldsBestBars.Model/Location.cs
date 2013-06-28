using System;
using System.Linq;

using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [JsonObject()]
    public class Location : DatabaseObject
    {
        [JsonProperty("intro")]
        public string Intro { get; set; }

        [JsonProperty("synopsis")]
        public string Synopsis { get; set; }

        [JsonProperty("parent")]
        public Guid? Parent { get; set; }

        [JsonProperty("geo")]
        public Geo Geo { get; set; }

        public static Location Convert(Data.Location input)
        {
            if (input == null) { return null; }
            return new Location()
            {
                Id = input.Id,
                Name = input.Name,
                Intro = input.Intro,
                Synopsis = input.Synopsis,
                Parent = input.ParentId,
                UrlKey = input.UrlKey,
                Url = input.Url,
                Geo = input.Geocoordinate == null ? null : new Geo()
                {
                    Lat = (double)input.Geocoordinate.Lat,
                    Long = (double)input.Geocoordinate.Long
                }
            };
        }

        public static Location[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                return context.Locations.ToList().Select(l => Convert(l)).ToArray();
            }
        }

        public static Location GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                return Convert(context.Locations.SingleOrDefault(l => l.Id == id));
            }
        }
    }
}
