using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WorldsBestBars.Services;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Web.Admin.Controllers
{
    public class DocumentsController : Controller
    {
        #region Constructors

        public DocumentsController(DocumentService service)
        {
            _service = service;
        }

        #endregion

        #region Constants

        const int DefaultPageSize = 25;

        #endregion

        #region Public Methods

        public ActionResult Drafts(int? page, int? pageSize)
        {
            ViewBag.Page = "documents:drafts";

            ViewBag.CurrentPage = (page ?? 1);
            ViewBag.PageSize = pageSize ?? DefaultPageSize;

            int total;
            var model = _service.GetDrafts(out total, (int)ViewBag.PageSize * ((int)ViewBag.CurrentPage - 1), (int)ViewBag.PageSize);

            ViewBag.PageCount = (int)Math.Ceiling((double)total / (double)ViewBag.PageSize);

            return View(model);
        }

        public ActionResult Active(int? page, int? pageSize)
        {
            ViewBag.Page = "documents:active";

            ViewBag.CurrentPage = (page ?? 1);
            ViewBag.PageSize = pageSize ?? DefaultPageSize;

            int total;
            var model = _service.GetActive(out total, (int)ViewBag.PageSize * ((int)ViewBag.CurrentPage - 1), (int)ViewBag.PageSize);

            ViewBag.PageCount = (int)Math.Ceiling((double)total / (double)ViewBag.PageSize);

            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.Page = "documents:create";

            ViewBag.IsNew = true;

            PopulateDocuments();

            return View("Update");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(UpdateDocument model)
        {
            ViewBag.Page = "documents:create";

            if (ModelState.IsValid)
            {
                var id = _service.Create(model);

                return RedirectToAction("Update", new { id = id });
            }

            ViewBag.IsNew = true;

            PopulateDocuments();

            return View("Update", model);
        }

        public ActionResult Update(Guid id)
        {
            ViewBag.Page = "documents:update";

            ViewBag.IsNew = false;

            var entity = _service.Get(id);
            var model = new UpdateDocument
            {
                Name = entity.Name,
                Content = entity.Content,
                ParentId = entity.ParentId,
                RedirectId = entity.RedirectId,
                Synopsis = entity.Synopsis,
                UrlKey = entity.UrlKey,
                Images = entity.Images,
                Categories = entity.Categories
            };

            ViewBag.IsActive = entity.IsActive;

            PopulateDocuments();

            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Update(Guid id, UpdateDocument model, IEnumerable<HttpPostedFileBase> images_, bool? process, Guid? category, IEnumerable<string> delete)
        {
            ViewBag.Page = "documents:update";

            if (ModelState.IsValid)
            {
                _service.Update(id, model);

                if (images_ != null && (images_ = images_.Where(i => i != null)).Any())
                {
                    var document = _service.Get(id);
                    foreach (var image in images_)
                    {
                        Services.Media.Helper.UploadMedia(image.InputStream, document.Url, process.HasValue && process.Value);
                    }
                }

                if (delete != null)
                {
                    foreach (var image in delete)
                    {
                        Services.Media.Helper.Delete(image);
                    }
                }

                if (category.HasValue)
                {
                    var service = DependencyResolver.Current.GetService<Services.Categories.Add>();

                    service.Execute(category.Value, id, EntityType.Document);
                }

                return RedirectToAction("Update");
            }

            ViewBag.IsNew = false;
            var entity = _service.Get(id);
            model.Categories = entity.Categories;
            ViewBag.IsActive = entity.IsActive;

            PopulateDocuments();

            return View(model);
        }

        public ActionResult Delete(Guid id)
        {
            _service.Delete(id);

            return RedirectToAction("Active");
        }

        public ActionResult Activate(Guid id, bool isActive)
        {
            _service.Activate(id, isActive);

            return RedirectToAction("Update");
        }

        public ActionResult AddToCategory(Guid id, Guid category)
        {
            var service = DependencyResolver.Current.GetService<Services.Categories.Add>();

            service.Execute(category, id, EntityType.Document);

            return RedirectToAction("Single", new { id = id });
        }

        #endregion

        #region Private Methods

        void PopulateDocuments()
        {
            int total = 0;
            ViewBag.Documents = _service.GetActive(out total, 0, int.MaxValue).Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = "(" + d.Url + ") " + d.Name
            }).OrderBy(e => e.Text);
        }

        #endregion

        #region Private Fields

        DocumentService _service;

        #endregion
    }
}
