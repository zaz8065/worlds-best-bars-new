using System;
using System.Linq;

using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [JsonObject]
    public class UrlMap : DatabaseObject
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("parent")]
        public Guid? Parent { get; set; }
        
        [JsonProperty("is_active")]
        public bool IsActive { get; set; }

        public static UrlMap Convert(Data.UrlMap input)
        {
            if (input == null) { return null; }
            return new UrlMap()
            {
                Id = input.Id,
                Name = input.Name,
                Parent = input.ParentId,
                Type = input.Type,
                UrlKey = input.UrlKey,
                Url = input.Url,
                IsActive = input.IsActive == 1
            };
        }

        public static UrlMap[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                return context.UrlMaps.ToList().Select(u => UrlMap.Convert(u)).ToArray();
            }
        }

        public static UrlMap GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                return UrlMap.Convert(context.UrlMaps.SingleOrDefault(u => u.Id == id));
            }
        }
    }
}