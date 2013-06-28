using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldsBestBars.Services.Models
{
    public enum AdvertStatus
    {
        Draft,
        Active
    }

    public class Advert
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string SkyscraperUrl { get; set; }
        public string VideoUrl { get; set; }
        public string VideoImageUrl { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? Finish { get; set; }
        public bool IsActive { get; set; }
        public byte Weight { get; set; }
        public string Type { get; set; }
        public string DestinationUrl { get; set; }

        public IEnumerable<Statistic> Stats { get; set; }

        public class Statistic
        {
            public DateTime Period { get; set; }
            public int Clicks { get; set; }
            public int Impressions { get; set; }
        }
    }
}
