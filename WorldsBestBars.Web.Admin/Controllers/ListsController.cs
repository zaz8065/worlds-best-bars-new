using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Web.Admin.Controllers
{
    public class ListsController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Page = "lists:index";

            var service = DependencyResolver.Current.GetService<Services.Categories.GetAll>();
            var model = service.Execute();

            return View(model);
        }

        public ActionResult Single(Guid id)
        {
            var service = DependencyResolver.Current.GetService<Services.Categories.Get>();
            var model = service.Execute(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Remove(Guid id, Guid entity, EntityType type)
        {
            var service = DependencyResolver.Current.GetService<Services.Categories.Remove>();
            service.Execute(id, entity, type);

            return RedirectToAction("Single", new { id = id });
        }

        public ActionResult Create()
        {
            ViewBag.Page = "lists:create";

            return View("Update");
        }

        [HttpPost]
        public ActionResult Create(CreateCategory model)
        {
            ViewBag.Page = "lists:create";

            Validate(model);

            if (ModelState.IsValid)
            {
                var service = DependencyResolver.Current.GetService<Services.Categories.Create>();

                var id = service.Execute(model);

                return RedirectToAction("Single", new { id = id });
            }

            return View("Update", model);
        }

        public ActionResult Update(Guid id)
        {
            var service = DependencyResolver.Current.GetService<Services.Categories.Get>();
            var entity = service.Execute(id);

            if (entity == null)
            {
                return HttpNotFound();
            }

            var model = new UpdateCategory
            {
                Group = entity.Group,
                Name = entity.Name,
                Key = entity.Key
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Update(Guid id, UpdateCategory model)
        {
            Validate(model);

            if (ModelState.IsValid)
            {
                var service = DependencyResolver.Current.GetService<Services.Categories.Update>();

                service.Execute(id, model);

                return RedirectToAction("Single", new { id = id });
            }

            return View(model);
        }

        public ActionResult JsonSearch(string query)
        {
            var service = DependencyResolver.Current.GetService<Services.Categories.GetAll>();
            var results = service.Execute(query);

            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(results.Select(e => new { id = e.Id, name = e.Name })), "application/json");
        }

        void Validate(CreateCategory model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("Name", "Required");
            }

            if (string.IsNullOrEmpty(model.Key))
            {
                ModelState.AddModelError("Key", "Required");
            }
        }
    }
}
