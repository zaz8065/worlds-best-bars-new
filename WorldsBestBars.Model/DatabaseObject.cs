using System;

using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [Serializable()]
    [JsonObject()]
    public class DatabaseObject
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("urlkey")]
        public string UrlKey { get; set; }
    }
}
