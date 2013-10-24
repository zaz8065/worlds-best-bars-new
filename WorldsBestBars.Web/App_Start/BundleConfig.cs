using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace WorldsBestBars.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/css").Include("~/Content/css/rateit.css", "~/Content/css/select2.css", "~/Content/css/global.css"));
            bundles.Add(new StyleBundle("~/css/ie6").Include("~/Content/css/ie6.css"));
            bundles.Add(new StyleBundle("~/css/ie7").Include("~/Content/css/ie7.css"));
            bundles.Add(new StyleBundle("~/css/ie8").Include("~/Content/css/ie8.css"));
            bundles.Add(new StyleBundle("~/css/ie9").Include("~/Content/css/ie9.css"));

            bundles.Add(new StyleBundle("~/css/bar-resource").Include("~/Areas/BarResource/Content/css/base.css", "~/Areas/BarResource/Content/css/site.css"));

            bundles.Add(new ScriptBundle("~/js/moderniser").Include("~/Content/js/modernizr-2.5.3.min.js"));
            bundles.Add(new ScriptBundle("~/js/bar-resource").Include("~/Areas/BarResource/Content/js/site.js"));
        }
    }
}