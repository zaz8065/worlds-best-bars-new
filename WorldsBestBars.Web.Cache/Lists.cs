using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsBestBars.Web.Cache
{
    public class Lists
    {
        List<Model.List> Cache = new List<Model.List>();

        static Lists instance = new Lists();
        public static Lists Instance { get { return instance; } }

        public Lists()
        {
            Populate();
        }

        public void Add(Model.List list)
        {
            Cache.Add(list);
        }

        public void Update(Guid id)
        {
            if (Contains(id))
            {
                Remove(id);
            }

            var list = Model.List.GetById(id);
            if (list != null)
            {
                Add(list);
            }
        }

        public void Remove(Guid id)
        {
            if (Contains(id))
            {
                Cache.Remove(Cache.Single(l => l.Id == id));
            }
        }

        public bool Contains(Guid id)
        {
            return Cache.Any(l => l.Id == id);
        }

        public bool Contains(string key)
        {
            return Cache.Any(l => l.Key == key);
        }

        public Model.List[] GetAll()
        {
            return Cache.ToArray();
        }

        public Model.List[] GetSearchable()
        {
            return Cache.Where(l => l.Searchable).ToArray();
        }

        public Model.List GetById(Guid id)
        {
            if (Contains(id))
            {
                return Cache.Single(l => l.Id == id);
            }

            return null;
        }

        public Model.List GetByKey(string key)
        {
            if (Contains(key))
            {
                return Cache.Single(l => l.Key == key);
            }

            return null;
        }

        private void Populate()
        {
            foreach (var list in Model.List.GetAll())
            {
                Add(list);
            }
        }

        public Model.List[] GetByBar(Guid id)
        {
            var result = new List<Model.List>();

            foreach (var list in Cache)
            {
                if (list.Bars.Any(b => b.Id == id))
                {
                    result.Add(list);
                }
            }

            return result.ToArray();
        }
    }
}
