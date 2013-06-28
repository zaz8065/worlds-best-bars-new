using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Web.Admin.Controllers
{
    public class AdvertisingController : Controller
    {
        #region Public Methods

        public ActionResult Active()
        {
            ViewBag.Page = "advertising:active";

            var service = DependencyResolver.Current.GetService<Services.Advertising.GetAll>();

            var model = service.Execute(AdvertStatus.Active);

            return View(model);
        }

        public ActionResult Inactive()
        {
            ViewBag.Page = "advertising:inactive";

            var service = DependencyResolver.Current.GetService<Services.Advertising.GetAll>();

            var model = service.Execute(AdvertStatus.Draft);

            return View(model);
        }

        public ActionResult Single(Guid id)
        {
            ViewBag.Page = "advertising:single";

            var service = DependencyResolver.Current.GetService<Services.Advertising.Get>();

            var model = service.Execute(id);

            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.Page = "advertising:create";

            return View("Update");
        }

        [HttpPost]
        public ActionResult Create(UpdateAdvert model, HttpPostedFileBase file)
        {
            ViewBag.Page = "advertising:create";

            if (ModelState.IsValid)
            {
                var service = DependencyResolver.Current.GetService<Services.Advertising.Create>();
                var id = service.Execute(model);

                UploadSkyScraper(id, file);

                return RedirectToAction("Single", new { id = id });
            }

            return View("Update", model);
        }

        public ActionResult Update(Guid id)
        {
            ViewBag.Page = "advertising:update";

            var service = DependencyResolver.Current.GetService<Services.Advertising.Get>();

            var entity = service.Execute(id);

            ViewBag.SkyscraperUrl = entity.SkyscraperUrl;
            var model = new UpdateAdvert
            {
                DestinationUrl = entity.DestinationUrl,
                Finish = entity.Finish,
                Start = entity.Start,
                Title = entity.Title,
                Type = entity.Type,
                Weight = entity.Weight
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Update(Guid id, UpdateAdvert model, HttpPostedFileBase file)
        {
            ViewBag.Page = "advertising:update";

            if (ModelState.IsValid)
            {
                var service = DependencyResolver.Current.GetService<Services.Advertising.Update>();
                service.Execute(id, model);

                UploadSkyScraper(id, file);

                return RedirectToAction("Single");
            }

            return View(model);
        }

        public ActionResult ToggleActive(Guid id, bool isActive)
        {
            var service = DependencyResolver.Current.GetService<Services.Advertising.ToggleActive>();
            service.Execute(id, isActive);

            return RedirectToAction("Single");
        }

        #endregion

        #region Private Methods

        void UploadSkyScraper(Guid id, HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var path = @"ads/" + id.ToString("N") + System.IO.Path.GetExtension(file.FileName).ToLower();
                var filename = System.IO.Path.Combine(ConfigurationManager.AppSettings["path:media:physical"], path);

                file.SaveAs(filename);
                
                var service = DependencyResolver.Current.GetService<Services.Advertising.UpdateSkyScraper>();

                service.Execute(id, "/content/media/" + path);
            }
        }

        #endregion
    }
}
