using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WorldsBestBars.Data;

namespace WorldsBestBars.Web.Areas.Admin.Controllers
{
    [Mvc.Filters.RequireAdmin()]
    public class JSONController : Controller
    {
        log4net.ILog Log = log4net.LogManager.GetLogger(typeof(JSONController));

        public string GetBar(Guid id)
        {
            try
            {
                Response.ContentType = "application/json";

                return JsonConvert.SerializeObject(Web.Cache.Bars.Instance.GetById(id));
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get bar", ex);

                return null;
            }
        }

        public string Bars(string term)
        {
            try
            {
                Response.ContentType = "application/json";

                return JsonConvert.SerializeObject(Model.JSON.Autocomplete.Bar.Search(term));
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get bars", ex);

                return null;
            }
        }

        public string GetExperts()
        {
            try
            {
                Response.ContentType = "application/json";

                return JsonConvert.SerializeObject(Web.Cache.Users.Instance.GetExperts(false).OrderBy(e => e.Name));
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get experts list", ex);

                return null;
            }
        }

        public string GetAdverts()
        {
            try
            {
                Response.ContentType = "application/json";

                return JsonConvert.SerializeObject(Web.Cache.Adverts.Instance.GetAll().OrderBy(a => a.IsActive ? 0 : 1).ThenBy(a => a.Name), new IsoDateTimeConverter());
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get experts list", ex);

                return null;
            }
        }

        public string GetLocations()
        {
            Response.ContentType = "application/json";

            return Newtonsoft.Json.JsonConvert.SerializeObject(GetLocations(null));
        }

        public string GetLocationsFlat()
        {
            Response.ContentType = "application/json";

            return Newtonsoft.Json.JsonConvert.SerializeObject(Web.Cache.Locations.Instance.GetAll()
                .OrderBy(l => l.Name)
                .Select(l => new
                {
                    id = l.Id,
                    name = l.Name,
                    parent = l.Parent
                }));
        }

        public string GetBars()
        {
            Response.ContentType = "application/json";

            return Newtonsoft.Json.JsonConvert.SerializeObject(Web.Cache.Bars.Instance.GetAll()
                .Take(50)
                .Select(b => new
                {
                    id = b.Id,
                    name = b.Name,
                    url = Web.Cache.UrlMap.Instance.GetById(b.Id).Url
                }));
        }

        public string GetLists()
        {
            Response.ContentType = "application/json";

            return JsonConvert.SerializeObject(Cache.Lists.Instance.GetAll().OrderBy(l => l.Name));
        }

        public string GetDocuments()
        {
            Response.ContentType = "application/json";

            return JsonConvert.SerializeObject(GetDocuments(null), new IsoDateTimeConverter());
        }

        public string GetMedia(Guid id)
        {
            Response.ContentType = "application/json";

            return JsonConvert.SerializeObject(Logic.Helper.GetMediaGroups(new UrlHelper(Request.RequestContext), id));
        }

        public string GetModerationData()
        {
            Response.ContentType = "application/json";

            using (var context = new Data.Entities())
            {
                Func<Data.Review, object> JsonObj = (review) =>
                {
                    return new
                    {
                        id = review.Id,
                        user = review.UserId == null ? null : new
                        {
                            id = review.User.Id,
                            name = review.User.Name
                        },
                        bar = new
                        {
                            id = review.Bar.Id,
                            name = review.Bar.Name
                        },
                        ip = review.UserIP,
                        title = review.Title,
                        comment = review.Comment,
                        created = review.Created,
                        moderated = review.IsModerated,
                        active = review.IsActive,
                        url = review.Bar.Url,
                        rating = review.Rating == null ? null : new
                        {
                            design = review.Rating.Design,
                            drinks = review.Rating.Drinks,
                            service = review.Rating.Service,
                        }
                    };
                };

                var ret = new
                {
                    awaiting = context.Reviews.Where(r => !r.IsModerated && (r.User == null || !r.User.IsExpert)).ToList().Where(r => r.Created > DateTime.Today.AddMonths(-3)).Select(r => JsonObj(r)),
                    recent = context.Reviews.Where(r => r.IsModerated && (r.User == null || !r.User.IsExpert)).ToList().Where(r => r.Created > DateTime.Today.AddDays(-7)).Select(r => JsonObj(r))
                };

                return Newtonsoft.Json.JsonConvert.SerializeObject(ret, new IsoDateTimeConverter());
            }
        }

        public string GetUserReviewsForBar(Guid id)
        {
            Response.ContentType = "application/json";

            return JsonConvert.SerializeObject(Cache.Reviews.Instance.GetByBar(id, false), new IsoDateTimeConverter());
        }

        public string GetUserReviewsForExpert(Guid id)
        {
            Response.ContentType = "application/json";

            return JsonConvert.SerializeObject(Cache.Reviews.Instance.GetByExpert(id, false), new IsoDateTimeConverter());
        }

        public string Search(string search, string type, int limit = 25)
        {
            Response.ContentType = "application/json";

            return JsonConvert.SerializeObject(Web.Search.Lucene.Search(search, type).Take(limit));
        }

        public string GetRelated(Guid id)
        {
            Response.ContentType = "application/json";

            var relations = Cache.Relations.Instance.GetById(id);
            var ret = new List<object>();

            foreach (var relation in relations)
            {
                var map = Cache.UrlMap.Instance.GetById(relation);
                if (map != null)
                {
                    ret.Add(map);
                }
            }

            return JsonConvert.SerializeObject(ret);
        }

        public string GetAdvertStats(Guid id)
        {
            Response.ContentType = "application/json";

            return JsonConvert.SerializeObject(Model.AdvertStat.GetByAd(id, DateTime.Now.AddHours(-14)), new IsoDateTimeConverter());
        }

        public string GetBarResourceData()
        {
            try
            {
                Response.ContentType = "application/json";

                var ret = new List<object>();

                var root = Server.MapPath("~/content/media/_bar-resource/");
                using (var context = new Entities())
                {
                    foreach (var dir in System.IO.Directory.GetDirectories(root))
                    {
                        var dataFile = System.IO.Path.Combine(dir, "content.xml");
                        if (System.IO.File.Exists(dataFile))
                        {
                            try
                            {
                                var data = XDocument.Load(dataFile);
                                var rootElement = data.Root;
                                Guid id = Guid.Empty;
                                try
                                {
                                    Guid.TryParse(rootElement.Element("BarId").Value, out id);
                                }
                                catch { }
                                var isNew = bool.Parse(rootElement.Element("IsNew").Value);
                                var db = isNew ? null : Model.Bar.Convert(context.Bars.Single(b => b.Id == id));

                                ret.Add(new
                                {
                                    id = rootElement.Element("Id").Value,
                                    bar_id = id,
                                    name = rootElement.Element("Name").Value,
                                    is_new = isNew,
                                    pictures = rootElement.Element("Pictures").Element("Pending").Elements().Select(e => e.Value),
                                    contact = new
                                    {
                                        website = rootElement.Element("Contact").Element("Website") == null ? string.Empty : rootElement.Element("Contact").Element("Website").Value,
                                        address = rootElement.Element("Contact").Element("Address") == null ? string.Empty : rootElement.Element("Contact").Element("Address").Value,
                                        phone = rootElement.Element("Contact").Element("Phone") == null ? string.Empty : rootElement.Element("Contact").Element("Phone").Value,
                                        email = rootElement.Element("Contact").Element("Email") == null ? string.Empty : rootElement.Element("Contact").Element("Email").Value,
                                        geo = rootElement.Element("Contact").Element("Geo") == null ? null : rootElement.Element("Contact").Element("Geo").Value.Split(',').Select(p => double.Parse(p))
                                    },
                                    type = new
                                    {
                                        styles = rootElement.Element("TypeOfBar").Element("Styles").Elements().Select(e => e.Value),
                                        specialities = rootElement.Element("TypeOfBar").Element("Specialities").Elements().Select(e => e.Value),
                                        atmosphere = rootElement.Element("TypeOfBar").Element("Atmosphere").Elements().Select(e => e.Value)
                                    },
                                    features = rootElement.Element("Features") == null ? null : rootElement.Element("Features").Element("List").Elements("Feature").Select(f => new
                                    {
                                        title = f.Element("Title").Value,
                                        description = f.Element("Description").Value,
                                        image = Url.Content(f.Element("Image").Value),
                                        id = f.Element("Id").Value
                                    }),
                                    cocktail = rootElement.Element("SignatureCocktail") == null ? null : new
                                    {
                                        name = rootElement.Element("SignatureCocktail").Element("Name").Value,
                                        description = rootElement.Element("SignatureCocktail").Element("Description").Value,
                                        ingredients = rootElement.Element("SignatureCocktail").Element("Ingredients").Value
                                    },
                                    menu = new
                                    {
                                        url = rootElement.Element("Menus") == null ? null : rootElement.Element("Menus").Element("MenuUrl") == null ? string.Empty : rootElement.Element("Menus").Element("MenuUrl").Value,
                                        files = rootElement.Element("Menus") == null ? null : rootElement.Element("Menus").Element("Files") == null ? null : rootElement.Element("Menus").Element("Files").Elements().Select(e => e.Value)
                                    },
                                    open_hours = rootElement.Element("OpeningHours").Element("Days").Elements("Day").Select(e => e.Element("TimeFrames").Elements("TimeFrame").Select(ee => ee.Elements().Any() ? new { open = ee.Element("Open").Value, close = ee.Element("Close").Value } : null)),
                                    current = isNew ? null : new
                                    {
                                        name = db.Name,
                                        contact = new
                                        {
                                            website = db.Website,
                                            address = db.Address.ToString(),
                                            phone = db.Phone,
                                            email = db.Email
                                        }
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                ret.Add(new
                                {
                                    error = true,
                                    msg = ex.Message,
                                    file = dataFile
                                });
                                // invalid data file
                            }
                        }
                    }
                }

                return JsonConvert.SerializeObject(ret, Formatting.Indented, new IsoDateTimeConverter());
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get bar resource submission data", ex);

                return null;
            }
        }

        private IEnumerable<dynamic> GetLocations(Guid? parent)
        {
            return Web.Cache.Locations.Instance.GetAll()
                .Where(l => l.Parent == parent)
                .OrderBy(l => l.Name)
                .Select(l => new
                {
                    metadata = new { id = l.Id, name = l.Name },
                    data = l.Name,
                    children = GetLocations(l.Id)
                });
        }

        private IEnumerable<dynamic> GetDocuments(Guid? parent)
        {
            if (parent == null)
            {
                return new dynamic[] {
                new {
                    data = "Documents",
                    children = 
                        Web.Cache.Documents.Instance.GetAll()
                        .Where(d => d.Parent == parent)
                        .OrderBy(d => d.Name)
                        .Select(d => new
                        {
                            metadata = new
                            {
                                id = d.Id,
                                name = d.Name,
                                content = d.Content,
                                synopsis = d.Synopsis,
                                urlkey = d.UrlKey,
                                active = d.IsActive,
                                redirect = d.Redirect,
                                created = d.Created
                            },
                            data = d.Name,
                            children = GetDocuments(d.Id)
                        })
                    }
                };
            }
            else
            {
                return Web.Cache.Documents.Instance.GetAll()
                        .Where(d => d.Parent == parent)
                        .OrderBy(d => d.Name)
                        .Select(d => new
                        {
                            metadata = new
                            {
                                id = d.Id,
                                name = d.Name,
                                content = d.Content,
                                synopsis = d.Synopsis,
                                urlkey = d.UrlKey,
                                active = d.IsActive,
                                redirect = d.Redirect,
                                created = d.Created
                            },
                            data = d.Name,
                            children = GetDocuments(d.Id)
                        });
            }
        }
    }
}
