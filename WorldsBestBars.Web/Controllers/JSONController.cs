using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;

namespace WorldsBestBars.Web.Controllers
{
    public class JSONController : Controller
    {
        [WorldsBestBars.Web.Mvc.Filters.CustomContentType(ContentType = "application/json")]
        [OutputCache(Duration = 30)]
        public string MapPoints(Guid id)
        {
            var bars = Cache.Bars.Instance.GetByParent(id);

            return JsonConvert.SerializeObject(bars.Where(b => b.Geo != null).Select((b, i) => new { id = b.Id, index = i, name = b.Name, geo = b.Geo }));
        }

        [WorldsBestBars.Web.Mvc.Filters.CustomContentType(ContentType = "application/json")]
        public string Articles(int start, int count)
        {
            var articles = Cache.Documents.Instance.GetAll().Where(d => d.Redirect == null && d.IsActive).OrderByDescending(d => d.Created).Skip(start).Take(count);

            return JsonConvert.SerializeObject(articles.Select(a => new
            {
                id = a.Id,
                url = a.Url,
                synopsis = a.Synopsis,
                name = a.Name,
                image = Logic.Helper.GetFirstMedia(new UrlHelper(Request.RequestContext), a.Id, "245x165.orig")
            }));
        }
    }
}
