using System;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using WorldsBestBars.Services;
using WorldsBestBars.Services.Models;
using WorldsBestBars.Web.Admin.Models;
using System.Xml.Linq;

namespace WorldsBestBars.Web.Admin.Controllers
{
    public class BarsController : Controller
    {
        #region Constructors

        public BarsController(SubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        #endregion

        #region Constants

        const int DefaultPageSize = 25;

        #endregion

        #region Public Methods

        public ActionResult JsonSearch(string query)
        {
            var service = DependencyResolver.Current.GetService<Services.Bars.GetAll>();
            int total;
            var results = service.Execute(out total, BarStatus.Active, 0, 5, query);

            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(results.Select(e => new { id = e.Id, name = e.Name })), "application/json");
        }

        public ActionResult Active(int? page, int? pageSize, string filter)
        {
            ViewBag.Page = "bars:active";

            ViewBag.CurrentPage = (page ?? 1);
            ViewBag.PageSize = pageSize ?? DefaultPageSize;

            var service = DependencyResolver.Current.GetService<Services.Bars.GetAll>();

            int total;
            var model = service.Execute(out total, BarStatus.Active, (int)ViewBag.PageSize * ((int)ViewBag.CurrentPage - 1), (int)ViewBag.PageSize, filter);

            ViewBag.PageCount = (int)Math.Ceiling((double)total / (double)ViewBag.PageSize);

            return View(model);
        }

        public ActionResult Closed(int? page, int? pageSize, string filter)
        {
            ViewBag.Page = "bars:closed";

            ViewBag.CurrentPage = (page ?? 1);
            ViewBag.PageSize = pageSize ?? DefaultPageSize;

            var service = DependencyResolver.Current.GetService<Services.Bars.GetAll>();

            int total;
            var model = service.Execute(out total, BarStatus.Closed, (int)ViewBag.PageSize * ((int)ViewBag.CurrentPage - 1), (int)ViewBag.PageSize, filter);

            ViewBag.PageCount = (int)Math.Ceiling((double)total / (double)ViewBag.PageSize);

            return View(model);
        }

        public ActionResult CreateSubmissions()
        {
            ViewBag.Page = "bars:submissions:create";

            var model = _submissionService.GetCreateSubmissions();

            return View(model);
        }

        public ActionResult PurgeSubmission(Guid id)
        {
            var isCreate = _submissionService.GetUpdateSubmission(id) == null;

            _submissionService.PurgeSubmission(id);

            return RedirectToAction(isCreate ? "CreateSubmissions" : "UpdateSubmissions");
        }

        public ActionResult UpdateSubmissions()
        {
            ViewBag.Page = "bars:submissions:update";

            var model = _submissionService.GetUpdateSubmissions();

            return View(model);
        }

        public ActionResult UpdateSubmission(Guid id)
        {
            ViewBag.Page = "bars:submissions:update:single";

            var model = _submissionService.GetUpdateSubmission(id);
            if (model == null) { return HttpNotFound(); }

            return View(model);
        }

        public ActionResult Map()
        {
            ViewBag.Page = "bars:map";

            return View();
        }

        public ActionResult Single(Guid id)
        {
            ViewBag.Page = "bars:single";

            var service = DependencyResolver.Current.GetService<Services.Bars.Get>();

            var model = service.Execute(id);
            if (model == null) { return HttpNotFound(); }

            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.Page = "bars:create";

            ViewBag.IsNew = true;

            return View("Update");
        }

        [HttpPost]
        public ActionResult Create(UpdateBarModel model)
        {
            ViewBag.Page = "bars:create";

            if (ModelState.IsValid)
            {
                var service = DependencyResolver.Current.GetService<Services.Bars.Create>();

                var dto = new UpdateBar
                {
                    Description = model.Description,
                    Email = model.Email,
                    Fax = model.Fax,
                    Intro = model.Intro,
                    MenuUrl = model.MenuUrl,
                    Name = model.Name,
                    Phone = model.Phone,
                    OpenHours = model.OpenHours,
                    Features = model.Features == null ? null : model.Features.Select(f => new BarDetail.Feature
                    {
                        Details = f.Details,
                        Extra = f.Extra,
                        Id = f.Id,
                        IsActive = f.IsActive,
                        Name = f.Name,
                        Sponsor = f.Sponsor,
                        Type = f.Type
                    }),
                    IsActive = model.IsActive,
                    Geo = model.Geo,
                    UrlKey = model.UrlKey,
                    Location = model.Location,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    AddressCity = model.AddressCity,
                    AddressPostcode = model.AddressPostcode,
                    AddressCountry = model.AddressCountry
                };

                var id = service.Execute(dto);

                var bar = DependencyResolver.Current.GetService<Services.Bars.Get>().Execute(id);

                if (model.MediaUpload != null)
                {
                    foreach (var image in model.MediaUpload.Where(i => i != null && i.ContentLength > 0))
                    {
                        Services.Media.Helper.UploadMedia(image.InputStream, bar.Url, true);
                    }
                }

                if (model.Features != null)
                {
                    for (var i = 0; i < model.Features.Count(); i++)
                    {
                        var feature = model.Features.ElementAt(i);
                        var featureId = feature.Id == Guid.Empty ? bar.Features.ElementAt(i).Id : feature.Id;

                        if (feature.File != null && feature.File.ContentLength > 0)
                        {
                            Services.Media.Helper.UploadMedia(feature.File.InputStream, bar.Url + "/features/" + featureId.ToString(), false);
                        }
                    }
                }

                return RedirectToAction("Single", new { id = id });
            }

            ViewBag.IsNew = true;

            return View("Update", model);
        }

        public ActionResult Update(Guid id)
        {
            ViewBag.Page = "bars:update";

            var service = DependencyResolver.Current.GetService<Services.Bars.Get>();

            var entity = service.Execute(id);
            if (entity == null) { return HttpNotFound(); }

            var model = new UpdateBarModel
            {
                Description = entity.Description,
                Email = entity.Email,
                Fax = entity.Fax,
                Intro = entity.Intro,
                MenuUrl = entity.MenuUrl,
                Name = entity.Name,
                Phone = entity.Phone,
                OpenHours = entity.OpenHours,
                Features = entity.Features.Select(f => new FeatureModel
                {
                    Details = f.Details,
                    Extra = f.Extra,
                    Id = f.Id,
                    IsActive = f.IsActive,
                    Name = f.Name,
                    Sponsor = f.Sponsor,
                    Type = f.Type
                }),
                Images = entity.Images,
                IsActive = entity.IsActive,
                Geo = entity.Geo,
                UrlKey = entity.UrlKey,
                Location = entity.Location,
                AddressLine1 = GetXmlPartIfExists(entity.Address, "street1"),
                AddressLine2 = GetXmlPartIfExists(entity.Address, "street2"),
                AddressCity = GetXmlPartIfExists(entity.Address, "city"),
                AddressPostcode = GetXmlPartIfExists(entity.Address, "postcode"),
                AddressCountry = GetXmlPartIfExists(entity.Address, "country")
            };

            ViewBag.LocationName = entity.LocationName;
            ViewBag.IsNew = false;

            return View(model);
        }

        string GetXmlPartIfExists(string xml, string part)
        {
            if (xml == null) { return null; }
            var element = XElement.Parse(xml);
            
            if (element == null) { return null; }
            var _part = element.Element(part);

            if (_part == null) { return null; }

            return _part.Value;
        }

        [HttpPost]
        public ActionResult Update(Guid id, UpdateBarModel model, IEnumerable<string> delete)
        {
            ViewBag.Page = "bars:update";

            if (ModelState.IsValid)
            {
                var dto = new UpdateBar
                {
                    Description = model.Description,
                    Email = model.Email,
                    Fax = model.Fax,
                    Intro = model.Intro,
                    MenuUrl = model.MenuUrl,
                    Name = model.Name,
                    Phone = model.Phone,
                    OpenHours = model.OpenHours,
                    Features = model.Features == null ? null : model.Features.Select(f => new BarDetail.Feature
                    {
                        Details = f.Details,
                        Extra = f.Extra,
                        Id = f.Id,
                        IsActive = f.IsActive,
                        Name = f.Name,
                        Sponsor = f.Sponsor,
                        Type = f.Type
                    }),
                    IsActive = model.IsActive,
                    Geo = model.Geo,
                    UrlKey = model.UrlKey,
                    Location = model.Location,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    AddressCity = model.AddressCity,
                    AddressPostcode = model.AddressPostcode,
                    AddressCountry = model.AddressCountry
                };

                var service = DependencyResolver.Current.GetService<Services.Bars.Update>();
                service.Execute(id, dto);

                var bar = DependencyResolver.Current.GetService<Services.Bars.Get>().Execute(id);

                if (model.MediaUpload != null)
                {
                    foreach (var image in model.MediaUpload.Where(i => i != null && i.ContentLength > 0))
                    {
                        Services.Media.Helper.UploadMedia(image.InputStream, bar.Url, true);
                    }
                }

                if (model.Features != null)
                {
                    for (var i = 0; i < model.Features.Count(); i++)
                    {
                        var feature = model.Features.ElementAt(i);
                        var featureId = feature.Id == Guid.Empty ? bar.Features.ElementAt(i).Id : feature.Id;

                        if (feature.File != null && feature.File.ContentLength > 0)
                        {
                            Services.Media.Helper.UploadMedia(feature.File.InputStream, bar.Url + "/features/" + featureId.ToString(), false);
                        }
                    }
                }

                if (delete != null)
                {
                    foreach (var image in delete)
                    {
                        Services.Media.Helper.Delete(image);
                    }
                }

                return RedirectToAction("Single", new { id = id });
            }

            ViewBag.IsNew = false;

            return View(model);
        }

        public ActionResult AddToCategory(Guid id, Guid category)
        {
            var service = DependencyResolver.Current.GetService<Services.Categories.Add>();

            service.Execute(category, id, EntityType.Bar);

            return RedirectToAction("Single", new { id = id });
        }

        public ActionResult ToggleActive(Guid id, bool isActive)
        {
            var service = DependencyResolver.Current.GetService<Services.Bars.ToggleActive>();

            service.Execute(id, isActive);

            return RedirectToAction("Single", new { id = id });
        }

        #endregion

        #region Private Fields

        SubmissionService _submissionService;

        #endregion
    }
}
