using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WorldsBestBars.Web.Cache
{
    public abstract class Base<TEntity> where TEntity : Model.DatabaseObject
    {
        SortedList<Guid, TEntity> Cache = new SortedList<Guid,TEntity>();

        public Base()
        {
            Populate();
        }

        protected abstract void Populate();
        public abstract void RefreshEntity(Guid id);

        public bool Contains(Guid id)
        {
            return Cache.ContainsKey(id);
        }

        public virtual TEntity GetById(Guid id)
        {
            if (!Contains(id)) { return default(TEntity); }
            return Cache[id];
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return Cache.Values;
        }

        public void Add(TEntity value)
        {
            Cache.Add(((Model.DatabaseObject)value).Id, value);
        }

        public void Add(IEnumerable<TEntity> values)
        {
            var cache = Cache;

            foreach (var value in values)
            {
                cache.Add(((Model.DatabaseObject)value).Id, value);
            }
        }

        public void Remove(Guid id)
        {
            Cache.Remove(id);
        }

        public void Update(Guid id, TEntity value)
        {
            Cache[id] = value;
        }
    }
}