using System;

using System.Linq;

namespace WorldsBestBars.Web.Cache
{
    public class Locations : Base<Model.Location>
    {
        public Locations() : base() { }

        static Locations instance = new Locations();
        public static Locations Instance { get { return instance; } }

        protected override void Populate()
        {
            Add(Model.Location.GetAll());            
        }

        public override void RefreshEntity(Guid id)
        {
            var location = Model.Location.GetById(id);

            if (Contains(id))
            {
                if (location == null && Contains(id))
                {
                    // remove
                    Remove(id);
                }
                else
                {
                    // update
                    Update(id, location);
                }
            }
            else
            {
                if (location != null)
                {
                    // add
                    Add(location);
                }
            }

            if (location == null)
            {
                Search.Lucene.DeleteFromIndex(id);
            }
            else
            {
                Search.Lucene.UpdateIndex(location);
            }
        }

        public Model.Location[] GetByParent(Guid? parent)
        {
            return GetAll().Where(l => l.Parent == parent).ToArray();
        }

        public Model.SearchResult[] GetClosest(Model.Geo location)
        {
            return GetAll().Where(l => l.Geo != null).Select(l => new Model.SearchResult()
            {
                Id = l.Id,
                DisplayText = l.Intro,
                Distance = l.Geo.DistanceTo(location),
                Name = l.Name,
                Score = 1,
                Type = "Location",
                Url = l.Url,
                UrlKey = l.UrlKey
            })
            .OrderBy(s => s.Distance)
            .ToArray();
        }

        public void UpdateSearchIndex()
        {
            Search.Lucene.AddToIndex(GetAll());
        }
    }
}
