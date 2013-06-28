using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace WorldsBestBars.Web.Mvc.Filters
{
    public class RequireAdminAttribute : ActionFilterAttribute
    {
        log4net.ILog Log = log4net.LogManager.GetLogger(typeof(RequireAdminAttribute));

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var routeData = filterContext.RouteData;

            var controllerName = (string)routeData.Values["controller"];
            var actionName = (string)routeData.Values["action"];

            if (controllerName == "Auth") { return; }

            var session = filterContext.HttpContext.Session;

            if (session != null)
            {
                if ((Model.User)session["user"] == null || !((Model.User)session["user"]).IsAdmin)
                {
                    filterContext.Result = new RedirectToRouteResult("Admin_Login", new RouteValueDictionary());
                }
            }
        }
    }
}
