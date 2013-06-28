using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorldsBestBars.Services;

namespace WorldsBestBars.Web.Admin.Controllers
{
    public class ReviewsController : Controller
    {
        #region Constructors

        public ReviewsController(ReviewService service)
        {
            _service = service;
        }

        #endregion

        #region Constants

        const int DefaultPageSize = 25;

        #endregion

        #region Public Methods

        public ActionResult PendingModeration(int? page, int? pageSize)
        {
            ViewBag.Page = "reviews:pending";

            ViewBag.CurrentPage = (page ?? 1);
            ViewBag.PageSize = pageSize ?? DefaultPageSize;

            int total;
            var model = _service.GetPendingModeration(out total, (int)ViewBag.PageSize * ((int)ViewBag.CurrentPage - 1), (int)ViewBag.PageSize);

            ViewBag.PageCount = (int)Math.Ceiling((double)total / (double)ViewBag.PageSize);

            return View(model);
        }

        public ActionResult Latest(int? page, int? pageSize)
        {
            ViewBag.Page = "reviews:latest";

            ViewBag.CurrentPage = (page ?? 1);
            ViewBag.PageSize = pageSize ?? DefaultPageSize;

            int total;
            var model = _service.GetAll(out total, (int)ViewBag.PageSize * ((int)ViewBag.CurrentPage - 1), (int)ViewBag.PageSize);

            ViewBag.PageCount = (int)Math.Ceiling((double)total / (double)ViewBag.PageSize);

            return View(model);
        }

        public ActionResult Moderate(Guid id, bool isActive)
        {
            ViewBag.Page = "reviews:moderate";

            _service.Moderate(id, isActive);

            return Redirect(Request.UrlReferrer.PathAndQuery);
        }

        #endregion

        #region Private Fields

        ReviewService _service;

        #endregion
    }
}
