using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace WorldsBestBars.Services.Models
{
    public enum BarStatus
    {
        Closed = 0,
        Active
    }

    public class BarDetail : NamedEntity
    {
        public string UrlKey { get; set; }
        public string Url { get; set; }
        public Guid? Location { get; set; }
        public string LocationName { get; set; }
        public string Intro { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public bool IsActive { get; set; }
        public string MenuUrl { get; set; }
        public Geo Geo { get; set; }
        public string Address { get; set; }

        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public IEnumerable<Opening> OpenHours { get; set; }
        public IEnumerable<string> Images { get; set; }
        public IEnumerable<Feature> Features { get; set; }
        public IEnumerable<NamedEntity> Categories { get; set; }

        public class Opening
        {
            public DayOfWeek Day { get; set; }
            public TimeSpan Open { get; set; }
            public TimeSpan Close { get; set; }
        }

        public class Feature
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Details { get; set; }
            public string Extra { get; set; }
            public bool IsActive { get; set; }
            public string Type { get; set; }
            public string Sponsor { get; set; }
            public IEnumerable<string> Images { get; set; }
        }

        internal string GeoString { set { Geo = Geo.FromString(value); } }
    }
}
