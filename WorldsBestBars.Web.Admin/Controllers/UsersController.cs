using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WorldsBestBars.Services;

namespace WorldsBestBars.Web.Admin.Controllers
{
    public class UsersController : Controller
    {
        #region Constructors

        public UsersController(UserService service)
        {
            _service = service;
        }

        #endregion

        #region Constants

        const int DefaultPageSize = 25;

        #endregion

        #region Public Methods

        public ActionResult Index(int? page, int? pageSize, string filter, string sort)
        {
            ViewBag.Page = "users:index";

            ViewBag.CurrentPage = (page ?? 1);
            ViewBag.PageSize = pageSize ?? DefaultPageSize;

            int total;
            var model = _service.GetAll(out total, (int)ViewBag.PageSize * ((int)ViewBag.CurrentPage - 1), (int)ViewBag.PageSize, filter, sort: sort);

            ViewBag.Total = total;
            ViewBag.PageCount = (int)Math.Ceiling((double)total / (double)ViewBag.PageSize);

            return View(model);
        }

        public ActionResult Export()
        {
            ViewBag.Page = "users:export";

            return View();
        }

        [HttpPost]
        public ActionResult Export(string filter, string locationFilter)
        {
            ViewBag.Page = "users:export";

            int total;
            var model = _service.GetAll(out total, null, null, filter, locationFilter);

            var builder = new StringBuilder();
            foreach (var entry in model)
            {
                builder.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3:yyyy-MM-dd}\"", entry.Name, entry.Email, entry.City, entry.Created));
            }

            Response.AddHeader("Content-Disposition", "attachment; filename=user-export." + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv");
            Response.ContentType = "application/octet-stream";

            return Content(builder.ToString(), "text/plain");
        }

        #endregion

        #region Private Fields

        UserService _service;

        #endregion
    }
}
