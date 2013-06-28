#undef CACHE_IN_MEMORY
using System;
using System.Linq;
using System.Collections.Generic;

namespace WorldsBestBars.Web.Cache
{
    public class Reviews : Base<Model.Review>
    {
        public Reviews() : base() { }

        static Reviews instance = new Reviews();
        public static Reviews Instance { get { return instance; } }

        protected override void Populate()
        {
#if CACHE_IN_MEMORY
            Add(Model.Review.GetAll());
#endif
        }

        public override void RefreshEntity(Guid id)
        {
            #if CACHE_IN_MEMORY
            var review = Model.Review.GetById(id);

            if (Contains(id))
            {
                if (review == null && Contains(id))
                {
                    // remove
                    Remove(id);
                }
                else
                {
                    // update
                    Update(id, review);
                }
            }
            else
            {
                if (review != null)
                {
                    // add
                    Add(review);
                }
            }
#endif
        }

        public override IEnumerable<Model.Review> GetAll()
        {
#if CACHE_IN_MEMORY
            return GetAll().ToArray();
#else
            return Model.Review.GetAll();
#endif
        }

        public Model.Review[] GetByBar(Guid id, bool activeOnly = true)
        {
#if CACHE_IN_MEMORY
            return GetAll().Where(r => (!activeOnly || r.IsActive) && r.Bar.Id == id).ToArray();
#else
            return Model.Review.GetByBar(id, activeOnly);
#endif
        }

        public Model.Review[] GetExpertByBar(Guid id, bool activeOnly = true)
        {
#if CACHE_IN_MEMORY

#else
            return Model.Review.GetExpertByBar(id, activeOnly);
#endif
        }

        public Model.Review[] GetByExpert(Guid id, bool activeOnly = true)
        {
#if CACHE_IN_MEMORY
            return GetAll().Where(r => (!activeOnly || r.IsActive) && r.User.Id == id).ToArray();
#else
            return Model.Review.GetByExpert(id, activeOnly);
#endif
        }

        public Model.Review[] GetByParent(Guid id, bool activeOnly = true)
        {
#if CACHE_IN_MEMORY
            return GetAll().Where(r => (!activeOnly || r.IsActive) && r.Parent == id).ToArray();
#else
            return Model.Review.GetByParent(id, activeOnly);
#endif
        }

        public Model.Review[] GetRecent(int limit = 10, bool activeOnly = true)
        {
#if CACHE_IN_MEMORY

#else
            return Model.Review.GetRecent(limit, activeOnly);
#endif
        }
    }
}
