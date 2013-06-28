using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorldsBestBars.Web.Areas.Admin.Controllers
{
    [Mvc.Filters.RequireAdmin()]
    public class DialogsController : Controller
    {
        public ActionResult Menu()
        {
            return View();
        }

        public ActionResult EditBar()
        {
            return View();
        }

        public ActionResult ManageLocations()
        {
            return View();
        }

        public ActionResult ManageExperts()
        {
            return View();
        }

        public ActionResult ManageLists()
        {
            return View();
        }

        public ActionResult ManageDocuments()
        {
            return View();
        }

        public ActionResult ManageAdverts()
        {
            return View();
        }

		public ActionResult ModerateReviews()
		{
			return View();
		}

        public ActionResult ManageBarResourceSubmissions()
        {
            return View();
        }
    }
}
