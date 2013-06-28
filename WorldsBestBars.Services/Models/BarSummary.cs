using System;

namespace WorldsBestBars.Services.Models
{
    public class BarSummary : NamedEntity
    {
        public string UrlKey { get; set; }
        public DateTime Created { get; set; }
        public int Reviews { get; set; }
    }
}
