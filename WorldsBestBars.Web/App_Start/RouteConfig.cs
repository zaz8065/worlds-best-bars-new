using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace WorldsBestBars.Web
{
    public class RouteConfig
    {
        private static void RegisterStaticRoutes(RouteCollection routes)
        {
            routes.MapRoute("MobileSearch", "search/mobile", new { controller = "Search", action = "ExecuteMobile", query = UrlParameter.Optional }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("Search", "search", new { controller = "Search", action = "Execute", query = UrlParameter.Optional }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("AgeGate", "age-gate", new { controller = "Home", action = "AgeGate" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("Most Talked About", "most-talked-about", new { controller = "Home", action = "MostTalkedAbout" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("Readers Top 100", "top-100", new { controller = "Home", action = "Top100" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("Experts Choice", "experts-choice", new { controller = "Home", action = "ExpertsChoice" }, new string[] { "WorldsBestBars.Web.Controllers" });

            routes.MapRoute("Unsubscribe", "unsubscribe/{email}", new { controller = "User", action = "Unsubscribe", email = UrlParameter.Optional }, new string[] { "WorldsBestBars.Web.Controllers" });

            routes.MapRoute("All Cities", "all-cities", new { controller = "Home", action = "AllCities" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("All Bars", "all-bars", new { controller = "Home", action = "AllBars" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("All Articles", "all-articles", new { controller = "Home", action = "AllArticles" }, new string[] { "WorldsBestBars.Web.Controllers" });

            routes.MapRoute("Privacy Policy", "privacy-policy", new { controller = "Home", action = "PrivacyPolicy" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("Cookie Policy", "cookie-policy", new { controller = "Home", action = "CookiePolicy" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("Terms and Conditions", "terms-and-conditions", new { controller = "Home", action = "TermsAndConditions" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("About Us", "about-us", new { controller = "Home", action = "AboutUs" }, new string[] { "WorldsBestBars.Web.Controllers" });

            routes.MapRoute("Sitemap XML", "sitemap.xml", new { controller = "Home", action = "Sitemap" }, new string[] { "WorldsBestBars.Web.Controllers" });
            
            routes.MapRoute("User - Lost Password", "lost-password", new { controller = "User", action = "LostPassword" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("User - Lost Password - Success", "lost-password/done", new { controller = "User", action = "LostPasswordSuccess" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("User - Profile - Update", "profile", new { controller = "User", action = "UpdateProfile" }, new string[] { "WorldsBestBars.Web.Controllers" });

            routes.MapRoute("Process", "process/{action}/{id}", new { controller = "Process", id = UrlParameter.Optional }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("JSON", "json/{action}/{id}", new { controller = "JSON", id = UrlParameter.Optional }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("User", "user/{action}/{id}", new { controller = "User", id = UrlParameter.Optional }, new string[] { "WorldsBestBars.Web.Controllers" });

            
            routes.MapRoute("Advert - Out", "ad/out/{id}", new { controller = "Ad", action = "Out" }, new string[] { "WorldsBestBars.Web.Controllers" });

            routes.MapRoute("Cache - Invalidate", "cache/invalidate/{type}/{id}", new { controller = "Process", action = "RefreshEntity" }, new string[] { "WorldsBestBars.Web.Controllers" });

            routes.MapRoute("Home", string.Empty, new { controller = "Home", action = "Index" }, new string[] { "WorldsBestBars.Web.Controllers" });

            routes.MapRoute("Error - 404", "error/not-found", new { controller = "Error", action = "NotFound" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("Error - 500", "error/problem", new { controller = "Error", action = "Problem" }, new string[] { "WorldsBestBars.Web.Controllers" });
            routes.MapRoute("Error - IE6", "error/ie6", new { controller = "Error", action = "IE6" }, new string[] { "WorldsBestBars.Web.Controllers" });
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("mobile.ashx");

            Mvc.Routing.Routes.RegisterFromDatabase(RouteTable.Routes);

            RegisterStaticRoutes(routes);
        }
    }
}