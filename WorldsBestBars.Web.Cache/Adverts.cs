using System;

namespace WorldsBestBars.Web.Cache
{
    public class Adverts : Base<Model.Advert>
    {
        public Adverts() : base() { }

        static Adverts instance = new Adverts();
        public static Adverts Instance { get { return instance; } }

        protected override void Populate()
        {
            Add(Model.Advert.GetAll());
        }

        public override void RefreshEntity(Guid id)
        {
            var advert = Model.Advert.GetById(id);

            if (Contains(id))
            {
                if (advert == null && Contains(id))
                {
                    // remove
                    Remove(id);
                }
                else
                {
                    // update
                    Update(id, advert);
                }
            }
            else
            {
                if (advert != null)
                {
                    // add
                    Add(advert);
                }
            }
        }
    }
}
