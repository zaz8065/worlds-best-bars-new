using System.Web;
using System.Web.Mvc;

namespace WorldsBestBars.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new Mvc.Filters.AgeGateFilter());
            filters.Add(new Mvc.Filters.IE6Filter());
        }
    }
}