using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WorldsBestBars.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(MvcApplication));

        public MvcApplication()
            : base()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        protected void Application_Start()
        {
            Log.Info("Application started");

            MvcHandler.DisableMvcResponseHeader = true;

            if (Web.Search.Lucene.IsIndexEmpty)
            {
                Cache.Bars.Instance.UpdateSearchIndex();
                Cache.Locations.Instance.UpdateSearchIndex();
                Cache.Documents.Instance.UpdateSearchIndex();
                Cache.Users.Instance.UpdateSearchIndex();
            }

            // HACK - initialise caches. should probably fix this.
            var _1 = Cache.Relations.Instance;
            var _2 = Cache.Lists.Instance;

            AreaRegistration.RegisterAllAreas();

            AutofacConfig.Configure();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_EndRequest()
        {
            if (Context.Response.StatusCode == 404)
            {
                var ektronRedirectUrl = string.Empty;
                if (Logic.Helper.CheckRedirect(Request.Url, out ektronRedirectUrl))
                {
                    Response.Clear();
                    Response.ClearHeaders();
                    Response.ClearContent();

                    Response.Status = "301 Moved Permanently";
                    Response.StatusCode = 301;
                    Response.AddHeader("Location", ektronRedirectUrl);

                    return;
                }
            }
        }
    }
}