using System.Collections.Generic;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Web.Admin.Models
{
    public class LocationModel
    {
        public Location Current { get; set; }
        public IEnumerable<Location> Locations { get; set; }
        public IEnumerable<BarSummary> Bars { get; set; }
    }
}