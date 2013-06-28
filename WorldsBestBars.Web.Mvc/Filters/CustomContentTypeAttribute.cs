using System.Web.Mvc;

namespace WorldsBestBars.Web.Mvc.Filters
{
    public class CustomContentTypeAttribute : ActionFilterAttribute
    {
        public string ContentType { get; set; }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.ContentType = ContentType;
        }
    }
}