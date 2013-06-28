using System;
using System.Collections.Generic;

namespace WorldsBestBars.Services.Models
{
    public class UpdateDocument
    {
        public string UrlKey { get; set; }
        public string Name { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? RedirectId { get; set; }

        public string Synopsis { get; set; }
        public string Content { get; set; }
        public IEnumerable<string> Images { get; set; }

        public IEnumerable<NamedEntity> Categories { get; set; }
    }
}
