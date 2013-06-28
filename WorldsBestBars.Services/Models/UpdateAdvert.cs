using System;

namespace WorldsBestBars.Services.Models
{
    public class UpdateAdvert
    {
        public string Title { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? Finish { get; set; }
        public byte Weight { get; set; }
        public string Type { get; set; }
        public string DestinationUrl { get; set; }
    }
}
