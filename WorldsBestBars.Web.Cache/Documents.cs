using System;
using System.Linq;

namespace WorldsBestBars.Web.Cache
{
    public class Documents : Base<Model.Document>
    {
        public Documents() : base() { }

        static Documents instance = new Documents();
        public static Documents Instance { get { return instance; } }

        protected override void Populate()
        {
            Add(Model.Document.GetAll());
        }

        public override void RefreshEntity(Guid id)
        {
            var doc = Model.Document.GetById(id);

            if (Contains(id))
            {
                if (doc == null && Contains(id))
                {
                    // remove
                    Remove(id);
                }
                else
                {
                    // update
                    Update(id, doc);
                }
            }
            else
            {
                if (doc != null)
                {
                    // add
                    Add(doc);
                }
            }

            if (doc == null || !doc.IsActive)
            {
                Search.Lucene.DeleteFromIndex(id);
            }
            else
            {
                Search.Lucene.UpdateIndex(doc);
            }
        }

        public Model.Document[] GetByParent(Guid? parent)
        {
            return GetAll().Where(d => d.Parent == parent).ToArray();
        }

        public void UpdateSearchIndex()
        {
            Search.Lucene.AddToIndex(GetAll().Where(d => d.IsActive));
        }
    }
}
