using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;

using WorldsBestBars.Web.Mvc.Routing;

namespace WorldsBestBars.Web.Cache
{
    public class UrlMap : Base<Model.UrlMap>
    {
        public UrlMap() :base() { }

        static UrlMap instance = new UrlMap();
        public static UrlMap Instance { get { return instance; } }

        protected override void Populate()
        {
            Add(Model.UrlMap.GetAll());
        }

        public override void RefreshEntity(Guid id)
        {
            var routes = RouteTable.Routes;

            var map = Model.UrlMap.GetById(id);

            if (Contains(id))
            {
                var route = routes.GetContentRoute(id);

                if (map == null || !map.IsActive)
                {
                    // remove
                    if (Contains(id))
                    {
                        Remove(id);
                    }

                    using (routes.GetWriteLock())
                    {
                        routes.Remove(route);
                        routes.AddContentRoute(id);
                    }
                }
                else
                {
                    // update
                    Update(id, map);

                    using (routes.GetWriteLock())
                    {
                        if (route == null)
                        {
                            routes.AddContentRoute(map, true);
                        }
                        else
                        {
                            route.Url = map.Url;
                        }
                    }
                }
            }
            else
            {
                if (map != null)
                {
                    var route = routes.GetContentRoute(id);
                    if (route != null)
                    {
                        routes.Remove(route);
                    }

                    // add
                    Add(map);

                    routes.AddContentRoute(map, true);
                }
            }

            foreach (var entity in GetAll().Where(m => m.Parent == id).ToArray())
            {
                RefreshEntity(entity.Id);
            }
        }
    }
}
