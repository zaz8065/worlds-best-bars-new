using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;

namespace WorldsBestBars.Web.Mvc.Routing
{
    public static class Routes
    {
        static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Routes));

        public static void RegisterFromDatabase(RouteCollection routes)
        {
            var maps = Model.UrlMap.GetAll();
            foreach (var entity in maps)
            {
                routes.AddContentRoute(entity, false);
            }

            Log.InfoFormat("Created {0} custom route(s)", maps.Count());
        }

        public static void AddContentRoute(this RouteCollection routes, Model.UrlMap map, bool needsLock = false)
        {
            var route = new Route(map.Url, new MvcRouteHandler());
            if (map.Type == "Bar" && !map.IsActive)
            {
                route.Defaults = new RouteValueDictionary(new
                {
                    controller = "Content",
                    action = "ClosedBar",
                    id = map.Id
                });
            }
            else
            {
                route.Defaults = new RouteValueDictionary(new
                {
                    controller = "Content",
                    action = "Display",
                    id = map.Id,
                    type = map.Type
                });
            }

            if (needsLock)
            {
                using (routes.GetWriteLock())
                {
                    routes.Insert(0, route);
                }
            }
            else
            {
                routes.Insert(0, route);
            }
        }

        public static void AddContentRoute(this RouteCollection routes, Guid id)
        {
            routes.AddContentRoute(Model.UrlMap.GetById(id));
        }

        public static Route GetContentRoute(this RouteCollection routes, Guid id)
        {
            return routes.Cast<Route>().SingleOrDefault(r =>
            {
                if (r != null)
                {
                    if (r.Defaults != null)
                    {
                        if (r.Defaults.ContainsKey("id"))
                        {
                            var field = r.Defaults["id"];
                            if (field != UrlParameter.Optional)
                            {
                                return (Guid)field == id;
                            }
                        }
                    }
                }
                return false;
            });
        }
    }
}
