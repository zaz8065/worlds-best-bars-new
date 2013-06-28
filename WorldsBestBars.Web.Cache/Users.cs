#undef CACHE_IN_MEMORY
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldsBestBars.Web.Cache
{
    public class Users : Base<Model.User>
    {
        public Users() : base() { }

        static Users instance = new Users();
        public static Users Instance { get { return instance; } }

        protected override void Populate()
        {
#if CACHE_IN_MEMORY
            Add(Model.User.GetAll());
#endif
        }

        public override void RefreshEntity(Guid id)
        {
            var user = Model.User.GetById(id);

#if CACHE_IN_MEMORY
            if (Contains(id))
            {
                if (user == null && Contains(id))
                {
                    // remove
                    Remove(id);
                }
                else
                {
                    // update
                    Update(id, user);
                }
            }
            else
            {
                if (user != null)
                {
                    // add
                    Add(user);
                }
            }
#endif
            if (user == null || !user.IsActive || !user.IsExpert)
            {
                Search.Lucene.DeleteFromIndex(id);
            }
            else
            {
                if (user.IsExpert && user.IsActive)
                {
                    Search.Lucene.UpdateIndex(user);
                }
            }
        }

#if !CACHE_IN_MEMORY
        public override IEnumerable<Model.User> GetAll()
        {
            return Model.User.GetAll();
        }

        public override Model.User GetById(Guid id)
        {
            return Model.User.GetById(id);
        }
        public Model.User[] GetExperts(bool activeOnly = true)
        {
            return Model.User.GetExperts();
        }
        public Model.User GetByEmail(string email)
        {
            return Model.User.GetByEmail(email);
        }
#else
        public Model.User[] GetExperts(bool activeOnly = true)
        {
            return GetAll().Where(u => (!activeOnly || u.IsActive) && u.IsExpert).ToArray();
        }
        public Model.User GetByEmail(string email)
        {
            return GetAll().SingleOrDefault(u => u.Email == email);
        }

#endif
        public void UpdateSearchIndex()
        {
#if CACHE_IN_MEMORY
            Search.Lucene.AddToIndex(GetAll().Where(u => u.IsExpert && u.IsActive));
#else
            Search.Lucene.AddToIndex(Model.User.GetExperts());
#endif
        }
    }
}
