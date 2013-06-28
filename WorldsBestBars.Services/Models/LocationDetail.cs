using System.Collections.Generic;

namespace WorldsBestBars.Services.Models
{
    public class LocationDetail : NamedEntity
    {
        public string UrlKey { get; set; }
        public string Synopsis { get; set; }
        public string Intro { get; set; }
        public string Url { get; set; }

        public IEnumerable<string> Images { get; set; }
    }
}
