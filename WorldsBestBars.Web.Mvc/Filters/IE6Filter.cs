using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WorldsBestBars.Web.Mvc.Filters
{
    public class IE6Filter : ActionFilterAttribute
    {
        log4net.ILog Log = log4net.LogManager.GetLogger(typeof(IE6Filter));

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var userAgent = filterContext.HttpContext.Request.UserAgent;

            if (!string.IsNullOrEmpty(userAgent) && userAgent.Contains("MSIE 6"))
            {
                var cookie = filterContext.HttpContext.Request.Cookies["wbb:ie6_check"];
                if (cookie == null)
                {
                    filterContext.HttpContext.Response.AppendCookie(new HttpCookie("wbb:ie6_check", "true")
                    {
                        Expires = DateTime.Now.AddHours(12)
                    });

                    filterContext.Result = new RedirectResult("/error/ie6");
                }
            }
        }
    }
}