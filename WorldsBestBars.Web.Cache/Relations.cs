using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsBestBars.Web.Cache
{
    public class Relations
    {
        Dictionary<Guid, IEnumerable<Guid>> Cache = new Dictionary<Guid, IEnumerable<Guid>>();

        static Relations instance = new Relations();
        public static Relations Instance { get { return instance; } }
        
        public Relations()
        {
            Populate();
        }

        public void Add(Guid id, IEnumerable<Guid> relations)
        {
            Cache.Add(id, relations.ToList());
        }

        public void Update(Guid id)
        {
            if (Contains(id))
            {
                Remove(id);
            }

            var relation = Model.Relation.GetById(id);
            Add(id, relation.Relations);
        }

        public void Remove(Guid id)
        {
            if (Contains(id))
            {
                Cache.Remove(id);
            }
        }

        public bool Contains(Guid id)
        {
            return Cache.ContainsKey(id);
        }

        public IEnumerable<Guid> GetById(Guid id)
        {
            if (Contains(id))
            {
                return Cache[id];
            }

            return new Guid[] { };
        }

        public IEnumerable<Guid> GetRandom(int count)
        {
            return WorldsBestBars.Web.Cache.UrlMap.Instance.GetAll().OrderBy(_ => Guid.NewGuid()).Take(count).Select(m => m.Id).ToArray();
        }

        private void Populate()
        {
            foreach (var relation in Model.Relation.GetAll())
            {
                Add(relation.Id, relation.Relations);
            }
        }
    }
}
