using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorldsBestBars.Web.Controllers
{
    public class SearchController : Controller
    {
        //[OutputCache(Duration=30, VaryByParam="query")]
        public ActionResult Execute(string query, string filter, string location)
        {
            if (string.IsNullOrEmpty(query))
            {
                return View("Results");
            }
            else
            {
                ViewBag.SearchFilter = filter;
                ViewBag.SearchQuery = query;
                if (!string.IsNullOrEmpty(location))
                {
                    var parts = location.Split(',');
                    ViewBag.SearchLocation = new Model.Geo() { Lat = double.Parse(parts[0]), Long = double.Parse(parts[1]) };
                }

                try
                {
                    var model = Web.Search.Lucene.Search(query, filter: filter == "-" ? null : filter);

                    return View("Results", model);
                }
                catch
                {
                    return View("Results");
                }
            }
        }

        public ActionResult ExecuteMobile(string query, string filter, string location)
        {
            if (string.IsNullOrEmpty(query) && string.IsNullOrEmpty(location))
            {
                return View("MobileResults");
            }
            else
            {
                ViewBag.SearchFilter = filter;
                ViewBag.SearchQuery = query;
                if (!string.IsNullOrEmpty(location))
                {
                    var parts = location.Split(',');
                    ViewBag.SearchLocation = new Model.Geo() { Lat = double.Parse(parts[0]), Long = double.Parse(parts[1]) };
                }

                try
                {
                    if (ViewBag.SearchLocation != null)
                    {
                        var orderedBars = Web.Cache.Bars.Instance.GetClosest((Model.Geo)ViewBag.SearchLocation);
                        var orderedLocations = Web.Cache.Locations.Instance.GetClosest((Model.Geo)ViewBag.SearchLocation);

                        var ordered = orderedBars.Concat(orderedLocations).OrderBy(sr => sr.Distance);

                        if (string.IsNullOrEmpty(query))
                        {
                            return View("MobileResults", ordered.Take(30));
                        }
                        else
                        {
                            var search = Web.Search.Lucene.Search(query);

                            var filtered = ordered.Where(r => search.Any(sr => sr.Id == r.Id));

                            return View("MobileResults", filtered.Take(30));
                        }
                    }
                    else
                    {
                        return View("MobileResults", Web.Search.Lucene.Search(query, filter: filter == "-" ? null : filter).Take(30));
                    }
                }
                catch
                {
                    return View("MobileResults");
                }
            }
        }
    }
}
