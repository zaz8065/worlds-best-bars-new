using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace WorldsBestBars.Web.Mvc.Filters
{
    public class AgeGateFilter : ActionFilterAttribute
    {
        log4net.ILog Log = log4net.LogManager.GetLogger(typeof(AgeGateFilter));

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var routeData = filterContext.RouteData;

            var area = ((string)filterContext.RouteData.DataTokens["area"] ?? string.Empty).ToLower();
            var controllerName = ((string)routeData.Values["controller"] ?? string.Empty).ToLower();
            var actionName = ((string)routeData.Values["action"] ?? string.Empty).ToLower();

            // don't check for admin section
            if (area == "admin") { return; }
            // don't check for bar resource section
            if (area == "barresource") { return; }
            // don't need to check if we're already going to the age gate, privacy policy or terms & conditions
            if (controllerName == "home" && (actionName == "agegate" || actionName == "privacypolicy" || actionName == "cookiepolicy" || actionName == "termsandconditions")) { return; }
            // don't check when in error controller
            if (controllerName == "error") { return; }
            // don't check when in process controller
            if (controllerName == "process") { return; }
            // don't perform check if it's a bot (google, facebook, etc...)
            if (Config.Bots.Any(b => filterContext.HttpContext.Request.UserAgent.ToLower().Contains(b))) { return; }

            var session = filterContext.HttpContext.Session;

            if (session != null)
            {
                if (session["age.check"] == null)
                {
                    var cookie = filterContext.HttpContext.Request.Cookies["wbb:age_check"];
                    if (cookie != null && cookie.Value == "true")
                    {
                        session["age.check"] = true;
                    }
                    else
                    {
                        filterContext.Result = new RedirectToRouteResult("AgeGate", new RouteValueDictionary(new
                        {
                            redirect = filterContext.HttpContext.Request.RawUrl
                        }));
                    }
                }
            }
        }

        public static class Config
        {
            static Config()
            {
                var agecheck = ConfigurationManager.GetSection("worldsbestbars/agecheck") as NameValueCollection;
                Bots = agecheck["Bots"].ToLower().Split(',');
            }

            public static string[] Bots { get; private set; }
        }
    }
}
