using System.Web.Mvc;

namespace WorldsBestBars.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            //Response.StatusCode = 404;

            return View("NotFound");
        }

        public ActionResult Problem()
        {
            //Response.StatusCode = 500;

            // something more serious happened...
            Response.ContentType = "text/html";
            return View("Problem");
        }

        public ActionResult IE6()
        {
            Response.ContentType = "text/html";
            return View("IE6");
        }
    }
}