using System;
using System.Collections.Generic;

namespace WorldsBestBars.Services.Models
{
    public class BarCreateSubmission
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string Reasoning { get; set; }
        public string Location { get; set; }
        public DateTime Created { get; set; }

        public IEnumerable<string> Media { get; set; }
    }
}
