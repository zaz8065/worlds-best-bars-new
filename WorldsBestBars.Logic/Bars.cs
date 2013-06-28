using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsBestBars.Logic
{
    public static class Bars
    {
        // todo - optimise/cache this
        public static bool IsExpertsChoice(this Model.Bar bar)
        {
            using (var context = new Data.Entities())
            {
                return context.Reviews.Any(r => r.BarId == bar.Id && r.User.IsExpert && r.User.IsActive && r.IsActive);
            }
        }

        public static bool IsTop100(this Model.Bar bar)
        {
            using (var context = new Data.Entities())
            {
                return context.Categories.Single(c => c.Key == "top-100-bars").Bars.Any(b => b.Id == bar.Id);
            }
        }
        
        public static IList<Model.Review> UserReviews(this Model.Bar bar)
        {
            return null;
        }
    }
}
