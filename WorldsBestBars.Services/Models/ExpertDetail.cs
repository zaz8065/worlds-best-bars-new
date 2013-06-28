using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldsBestBars.Services.Models
{
    public class ExpertDetail : NamedEntity
    {
        public string Title { get; set; }
        public string Biography { get; set; }
        public string Website { get; set; }
        public string UrlKey { get; set; }
        public bool IsActive { get; set; }
        
        public string Url { get { return UrlKey == null ? null : "experts/" + UrlKey; } }

        public IEnumerable<Review> Reviews { get; set; }
        public IEnumerable<string> Images { get; set; }
    }
}
