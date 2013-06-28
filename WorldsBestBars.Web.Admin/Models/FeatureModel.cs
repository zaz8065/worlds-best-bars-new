using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorldsBestBars.Web.Admin.Models
{
    public class FeatureModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string Extra { get; set; }
        public bool IsActive { get; set; }
        public string Type { get; set; }
        public string Sponsor { get; set; }
        public HttpPostedFileBase File { get; set; }
    }
}