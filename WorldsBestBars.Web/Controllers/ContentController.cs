using System;
using System.Web.Mvc;
using System.Linq;

namespace WorldsBestBars.Web.Controllers
{
    public class ContentController : Controller
    {
        //[OutputCache(VaryByParam = "id", Duration = 5, Location = System.Web.UI.OutputCacheLocation.Server)]
        public ActionResult Display(Guid id, string type)
        {
            object model = null;
            var view = type;

            switch (type)
            {
                case "Bar":
                    ViewBag.AltFeaturedArticles = Cache.Lists.Instance.GetByKey("featured-articles-2").Documents.OrderBy(_ => Guid.NewGuid()).Take(3);
                    ViewBag.Reviews = WorldsBestBars.Web.Cache.Reviews.Instance.GetByBar(id).Where(r => r.IsActive).OrderByDescending(r => r.Created);

                    model = Web.Cache.Bars.Instance.GetById(id);
                    break;
                case "Location":
                    var children = Cache.Locations.Instance.GetByParent(id);
                    var bars = Cache.Bars.Instance.GetByParent(id).Where(b => b.IsActive).OrderBy(b => b.Name);

                    ViewBag.Locations = children.OrderBy(l => l.Name);
                    ViewBag.Bars = bars;

                    if (children.Count() > 0)
                    {
                        view = "Location-Area";
                        if (bars.Count() == 0)
                        {
                            ViewBag.Bars = null;
                        }
                    }

                    model = Cache.Locations.Instance.GetById(id);
                    break;
                case "Document":
                    var document = Cache.Documents.Instance.GetById(id);

                    if (document.Redirect != null)
                    {
                        return Redirect("/" + document.Redirect.Url);
                    }

                    if (document.Parent == null)
                    {
                        var all = Cache.Documents.Instance.GetAll().Where(d => d.Id != id && d.Redirect == null && d.IsActive).OrderByDescending(d => d.Created);
                        ViewBag.Children = all.Take(6);
                        ViewBag.TotalCount = all.Count();
                    }

                    model = document;
                    break;
                case "Expert":
                    ViewBag.Reviews = WorldsBestBars.Web.Cache.Reviews.Instance.GetByExpert(id).OrderByDescending(r => r.Created);

                    model = Cache.Users.Instance.GetById(id);
                    break;
            }

            return View(view, model);
        }

        public ActionResult ClosedBar(Guid id)
        {
            return View(Web.Cache.Bars.Instance.GetById(id));
        }
    }
}
