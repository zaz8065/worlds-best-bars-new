using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WorldsBestBars.Services;
using WorldsBestBars.Services.Models;
using WorldsBestBars.Web.Admin.Models;

namespace WorldsBestBars.Web.Admin.Controllers
{
    public class LocationsController : AsyncController
    {
        #region Constructors

        public LocationsController(LocationService locationService)
        {
            _locationService = locationService;
        }

        #endregion

        #region Public Methods

        public async Task<ActionResult> Index(Guid? id)
        {
            ViewBag.Page = "locations:index";

            var model = new LocationModel
            {
                Current = id.HasValue ? await _locationService.Get(id.Value) : null,
                Locations = await _locationService.GetChildren(id),
                Bars = id.HasValue ? DependencyResolver.Current.GetService<Services.Bars.GetByLocation>().Execute(id.Value) : null
            };

            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.Page = "locations:create";

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateLocation model)
        {
            ViewBag.Page = "locations:create";

            if (ModelState.IsValid)
            {
                var id = await _locationService.Create(model);

                return RedirectToAction("Index", new { id = id });
            }

            return View(model);
        }

        public async Task<ActionResult> JsonSearch(string query)
        {
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject((await _locationService.Search(query)).Select(e => new { id = e.Id, name = e.Name })), "application/json");
        }

        public ActionResult Update(Guid id)
        {
            var service = DependencyResolver.Current.GetService<Services.Locations.Get>();

            var model = service.Execute(id);

            if (model == null) { return HttpNotFound(); }

            return View(model);
        }

        [HttpPost]
        public ActionResult Update(Guid id, LocationDetail model, IEnumerable<HttpPostedFileBase> images_)
        {
            if (ModelState.IsValid)
            {
                var service = DependencyResolver.Current.GetService<Services.Locations.Update>();

                service.Execute(id, model);

                if (images_ != null && (images_ = images_.Where(i => i != null)).Any())
                {
                    foreach (var image in images_.Where(i => i != null))
                    {
                        Services.Media.Helper.UploadMedia(image.InputStream, model.Url, true);
                    }
                }

                return RedirectToAction("Index", new { id = id });
            }

            return View(model);
        }

        public ActionResult Delete(Guid id)
        {
            var service = DependencyResolver.Current.GetService<Services.Locations.Delete>();

            service.Execute(id);

            return RedirectToAction("Index", new { id = (Guid?)null });            
        }

        #endregion

        #region Private Fields

        LocationService _locationService;

        #endregion
    }
}
