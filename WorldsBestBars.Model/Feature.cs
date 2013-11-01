using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [Serializable()]
    [JsonObject()]
    public class Feature : DatabaseObject
    {
        [JsonProperty("bar")]
        public DatabaseObject Bar { get; set; }

        [JsonProperty("active")]
        public bool IsActive { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("extra")]
        public string Extra { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sponsor")]
        public string Sponsor { get; set; }


        public static Feature Convert(Data.Feature input)
        {
            if (input == null) { return null; }
            return new Feature()
            {
                Id = input.Id,
                IsActive = input.IsActive,
                Name = input.Name,
                Details = input.Details,
                Extra = input.Extra,
                Type = input.Type,
                Sponsor = input.Sponsor
            };
        }

        public static Feature[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                return context.Features.ToList().Select(r => Convert(r)).ToArray();
            }
        }

        public static Feature GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                return Convert(context.Features.SingleOrDefault(r => r.Id == id));
            }
        }

        public static Guid? Create(Guid bar, string name, string details, string extra, string type, string sponsor, bool isActive)
        {
            using (var context = new Data.Entities())
            {
                var feature = new Data.Feature()
                {
                    Id = Guid.NewGuid(),
                    BarId = bar,
                    Name = name,
                    Details = details,
                    Extra = extra,
                    Type = type,
                    Sponsor = sponsor,
                    IsActive = isActive
                };

                context.Features.AddObject(feature);

                context.SaveChanges();

                return feature.Id;
            }
        }

        public static Model.Feature[] GetByBar(Guid id, bool activeOnly = true)
        {
            using (var context = new Data.Entities())
            {
                return context.Features.Where(r => r.BarId == id && (!activeOnly || r.IsActive)).ToList().Select(r => Convert(r)).ToArray();
            }
        }
    }
}
