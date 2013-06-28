using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorldsBestBars.Web.Areas.Admin.Controllers
{
    [Mvc.Filters.RequireAdmin()]
    public class HtmlController : Controller
    {
        public ActionResult Bars(Guid? location, string alt)
        {
            if (!string.IsNullOrEmpty(alt) && alt.Length != 36)
            {
                switch (alt)
                {
                    case "orphan":
                        return View("TableBarPartial", Web.Cache.Bars.Instance.GetByParent(null).OrderBy(b => b.Name));
                    case "no-geo":
                        return View("TableBarPartial", Web.Cache.Bars.Instance.GetAll().Where(b => b.Geo == null).OrderBy(b => b.Name));
                }

                return null;
            }
            else
            {
                return View("TableBarPartial", Web.Cache.Bars.Instance.GetByParent(location).OrderBy(b => b.Name));
            }
        }
    }
}
