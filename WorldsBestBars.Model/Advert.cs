using System;
using System.Linq;

using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [Serializable()]
    [JsonObject()]
    public class Advert : DatabaseObject
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("skyscraper")]
        public string Skyscraper { get; set; }

        [JsonProperty("video")]
        public string Video { get; set; }

        [JsonProperty("video_image")]
        public string VideoImage { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("destination")]
        public string Destination { get; set; }

        [JsonProperty("start")]
        public DateTime? Start { get; set; }

        [JsonProperty("finish")]
        public DateTime? Finish { get; set; }

        [JsonProperty("weight")]
        public byte Weight { get; set; }

        [JsonProperty("active")]
        public bool IsActive { get; set; }

        [JsonProperty("target_countries")]
        public string TargetCountries { get; set; }

        public static Advert Convert(Data.Advert input)
        {
            if (input == null) { return null; }
            return new Advert()
            {
                Id = input.Id,
                Name = input.Title,
                Body = input.Body,
                Skyscraper = input.SkyscraperUrl,
                Video = input.VideoUrl,
                VideoImage = input.VideoImageUrl,
                Destination = input.DestinationUrl,
                Start = input.Start,
                Finish = input.Finish,
                Type = input.Type,
                Weight = input.Weight,
                IsActive = input.IsActive,
                TargetCountries = input.TargetCountries
            };
        }

        public static Advert[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                return context.Adverts.ToList().Select(a => Convert(a)).ToArray();
            }
        }

        public static Advert GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                return Convert(context.Adverts.SingleOrDefault(a => a.Id == id));
            }
        }

        public static void IncrementStats(Guid id, DateTime period, int clicks, int impressions)
        {
            if (clicks + impressions > 0)
            {
                using (var context = new Data.Entities())
                {
                    var entity = context.AdvertStats.SingleOrDefault(a => a.AdvertId == id && a.Period.Year == period.Year && a.Period.Month == period.Month && a.Period.Day == period.Day && a.Period.Hour == period.Hour);
                    if (entity == null)
                    {
                        entity = new Data.AdvertStat()
                        {
                            Id = Guid.NewGuid(),
                            AdvertId = id,
                            Clicks = 0,
                            Impressions = 0,
                            Period = period
                        };

                        context.AdvertStats.AddObject(entity);
                    }

                    entity.Impressions += impressions;
                    entity.Clicks += clicks;

                    context.SaveChanges();
                }
            }
        }
    }
}
