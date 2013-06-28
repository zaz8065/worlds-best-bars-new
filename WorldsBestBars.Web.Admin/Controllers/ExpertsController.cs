using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WorldsBestBars.Services;
using WorldsBestBars.Web.Admin.Models;

namespace WorldsBestBars.Web.Admin.Controllers
{
    public class ExpertsController : Controller
    {
        #region Constants

        const int DefaultPageSize = 25;

        #endregion

        #region Public Methods

        public ActionResult Index(int? page, int? pageSize, string filter, string sort, string sortd)
        {
            ViewBag.Page = "experts:index";

            ViewBag.CurrentPage = (page ?? 1);
            ViewBag.PageSize = pageSize ?? DefaultPageSize;

            int total;
            var service = DependencyResolver.Current.GetService<Services.Experts.GetAll>();
            var model = service.Execute(out total, (int)ViewBag.PageSize * ((int)ViewBag.CurrentPage - 1), (int)ViewBag.PageSize, filter, sort, sortd);

            ViewBag.Total = total;
            ViewBag.PageCount = (int)Math.Ceiling((double)total / (double)ViewBag.PageSize);

            ViewBag.Sort = sort;
            ViewBag.SortDirection = sortd;

            return View(model);
        }

        public ActionResult Single(Guid id)
        {
            ViewBag.Page = "experts:single";

            var service = DependencyResolver.Current.GetService<Services.Experts.Get>();
            var model = service.Execute(id);

            if (model == null) { return HttpNotFound(); }

            return View(model);
        }

        public ActionResult Update(Guid id)
        {
            ViewBag.Page = "experts:update";

            var service = DependencyResolver.Current.GetService<Services.Experts.Get>();
            var model = service.Execute(id);

            if (model == null) { return HttpNotFound(); }

            return View(model);
        }

        [HttpPost]
        public ActionResult Update(Guid id, Services.Models.ExpertDetail model, IEnumerable<HttpPostedFileBase> images_)
        {
            ViewBag.Page = "experts:update";

            if (ModelState.IsValid)
            {
                var service = DependencyResolver.Current.GetService<Services.Experts.Update>();

                service.Execute(id, model);

                if (images_ != null && (images_ = images_.Where(i => i != null)).Any())
                {
                    foreach (var image in images_.Where(i => i != null))
                    {
                        Services.Media.Helper.UploadMedia(image.InputStream, "experts/" + model.UrlKey, true);
                    }
                }

                return RedirectToAction("Single", new { id = id });
            }

            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.Page = "experts:create";

            return View("Update", new Services.Models.ExpertDetail());
        }

        [HttpPost]
        public ActionResult Create(Services.Models.ExpertDetail model, IEnumerable<HttpPostedFileBase> images_)
        {
            ViewBag.Page = "experts:create";

            if (ModelState.IsValid)
            {
                var service = DependencyResolver.Current.GetService<Services.Experts.Create>();

                var id = service.Execute(model);

                if (images_ != null && (images_ = images_.Where(i => i != null)).Any())
                {
                    foreach (var image in images_.Where(i => i != null))
                    {
                        Services.Media.Helper.UploadMedia(image.InputStream, "experts/" + model.UrlKey, true);
                    }
                }

                return RedirectToAction("Single", new { id = id });
            }

            return View("Update", model);
        }

        public ActionResult Delete(Guid id)
        {
            var service = DependencyResolver.Current.GetService<Services.Experts.Delete>();
            service.Execute(id);

            return RedirectToAction("Index");
        }

        public ActionResult ToggleActive(Guid id, bool isActive)
        {
            var service = DependencyResolver.Current.GetService<Services.Experts.ToggleActive>();
            service.Execute(id, isActive);

            return RedirectToAction("Single");
        }

        public ActionResult CreateReview(Guid id)
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult CreateReview(Guid id, CreateExpertReviewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Comment))
            {
                ModelState.AddModelError("Comment", "Required field.");
            }

            if (model.Bar == Guid.Empty)
            {
                ModelState.AddModelError("Bar", "Required field.");
            }

            if (ModelState.IsValid)
            {
                var service = DependencyResolver.Current.GetService<Services.Reviews.CreateExpert>();

                service.Execute(model.Bar, id, model.Comment);

                return Json(new { ok = true });
            }

            return PartialView(model);
        }

        public ActionResult DeleteReview(Guid id, Guid review)
        {
            var service = DependencyResolver.Current.GetService<Services.Reviews.DeleteExpert>();

            service.Execute(review);

            return RedirectToAction("Single", new { id = id });
        }

        #endregion
    }
}
