using System.Collections.Generic;

namespace WorldsBestBars.Services.Models
{
    public class CategoryDetail : NamedEntity
    {
        #region Public Properties

        public string Group { get; set; }
        public string Key { get; set; }

        public IEnumerable<NamedEntity> Bars { get; set; }
        public IEnumerable<NamedEntity> Locations { get; set; }
        public IEnumerable<NamedEntity> Documents { get; set; }

        #endregion
    }
}
