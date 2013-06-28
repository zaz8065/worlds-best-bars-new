using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WorldsBestBars.Services.Models
{
    public class UpdateBar
    {
        public string Name { get; set; }
        public string UrlKey { get; set; }
        public string Intro { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string MenuUrl { get; set; }
        public bool IsActive { get; set; }
        public Geo Geo { get; set; }
        public Guid? Location { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressCity { get; set; }
        public string AddressPostcode { get; set; }
        public string AddressCountry { get; set; }
     
        public IEnumerable<BarDetail.Opening> OpenHours { get; set; }
        public IEnumerable<BarDetail.Feature> Features { get; set; }
        public IEnumerable<string> Images { get; set; }
    }
}