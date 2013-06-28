using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorldsBestBars.Web.Areas.Admin.Controllers
{
    [Mvc.Filters.RequireAdmin()]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}