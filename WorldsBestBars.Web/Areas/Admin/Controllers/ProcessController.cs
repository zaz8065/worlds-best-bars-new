using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Text;
using System.IO;

using Newtonsoft.Json;
using WorldsBestBars.Logic;

namespace WorldsBestBars.Web.Areas.Admin.Controllers
{
    [Mvc.Filters.RequireAdmin()]
    public class ProcessController : Controller
    {
        log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ProcessController));

        public Guid? CreateBar(string name, string intro, string description, string urlkey, string phone, string fax, string email, string website, string street1, string street2, string city, string county, string postcode, string country, Dictionary<string, Dictionary<string, string>> openhours, Dictionary<string, double> geo, string menuUrl)
        {
            using (var context = new Data.Entities())
            {
                var entity = new Data.Bar()
                {
                    Id = Guid.NewGuid(),
                    IsActive = false,
                    Created = DateTime.Now,
                    Modified = DateTime.Now
                };

                PopulateBarEntity(entity, name, intro, description, urlkey, phone, fax, email, website, street1, street2, city, county, postcode, country, openhours, menuUrl);

                context.Bars.AddObject(entity);
                context.SaveChanges();

                if (geo != null)
                {
                    context.ExecuteStoreCommand(string.Format("update Bar set Geocoordinate = geography::STGeomFromText('POINT ({1} {2})', 4326) where Id = '{0}'", entity.Id, geo["lng"], geo["lat"]));
                    //context.ExecuteCommand(string.Format("update Bar set Geocoordinate = geography::STGeomFromText('POINT ({1} {2})', 4326) where Id = '{0}'", entity.Id, geo["lng"], geo["lat"]));
                }

                Web.Cache.Bars.Instance.RefreshEntity(entity.Id);
                Web.Cache.UrlMap.Instance.RefreshEntity(entity.Id);

                Log.InfoFormat("Created new bar '{0}' ({1})", name, entity.Id);

                return entity.Id;
            }
        }

        public string CreateExpertReview(Guid expert, Guid bar, string comment)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var entity = new Data.Review()
                    {
                        Id = Guid.NewGuid(),
                        UserId = expert,
                        BarId = bar,
                        Comment = comment,
                        Created = DateTime.Now,
                        IsActive = false,
                        IsModerated = true
                    };

                    context.Reviews.AddObject(entity);

                    context.SaveChanges();

                    return entity.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to create a review on bar '" + bar.ToString() + "' for expert '" + expert.ToString() + "'", ex);

                return "false";
            }
        }

        public string CreateLocation(string name, Guid parent)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var entity = new Data.Location()
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        ParentId = parent
                    };

                    context.Locations.AddObject(entity);

                    context.SaveChanges();

                    Log.InfoFormat("Created location '{0}' ({1})", name, entity.Id);

                    Web.Cache.Locations.Instance.RefreshEntity(entity.Id);

                    return entity.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to create location '" + name + "'", ex);

                return "false";
            }
        }

        public string CreateDocument(string name, Guid? parent)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var entity = new Data.Document()
                    {
                        Id = Guid.NewGuid(),
                        ParentId = parent,
                        Name = name,
                        IsActive = false,
                        Content = string.Empty,
                        Synopsis = string.Empty,
                        Created = DateTime.Now,
                        Modified = DateTime.Now
                    };

                    context.AddToDocuments(entity);

                    context.SaveChanges();

                    Web.Cache.Documents.Instance.RefreshEntity(entity.Id);

                    return entity.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to create document named '" + name + "'", ex);

                return "false";
            }
        }

        public string CreateAdvert(string title)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var entity = new Data.Advert()
                    {
                        Id = Guid.NewGuid(),
                        Title = title,
                        SkyscraperUrl = string.Empty,
                        IsActive = false,
                        Weight = 0,
                        Type = "image"
                    };

                    context.AddToAdverts(entity);

                    context.SaveChanges();

                    Web.Cache.Adverts.Instance.RefreshEntity(entity.Id);

                    return entity.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to create advert named '" + title + "'", ex);

                return "false";
            }
        }

        public string CreateExpert(string name)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var entity = new Data.User()
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        IsExpert = true,
                        IsActive = false,
                        Created = DateTime.Now,
                        Modified = DateTime.Now
                    };

                    context.Users.AddObject(entity);

                    context.SaveChanges();

                    Web.Cache.Users.Instance.RefreshEntity(entity.Id);
                    Web.Cache.UrlMap.Instance.RefreshEntity(entity.Id);

                    return entity.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to create an expert '" + name + "'", ex);

                return "false";
            }
        }

        public void CreateFeature(string name, string sponsor, string details, string extra, string type, bool isActive, Guid bar)
        {
            using (var context = new Data.Entities())
            {
                var entity = context.Features.CreateObject();
                context.Features.AddObject(entity);

                entity.Id = Guid.NewGuid();
                entity.Name = name;
                entity.Sponsor = sponsor;
                entity.Details = details;
                entity.Extra = extra;
                entity.Type = type;
                entity.IsActive = isActive;
                entity.BarId = bar;

                context.SaveChanges();

                Web.Cache.Bars.Instance.RefreshEntity(entity.BarId);
            }
        }

        [ValidateInput(false)]
        public void EditBar(Guid id, string name, string intro, string description, string urlkey, string phone, string fax, string email, string website, string street1, string street2, string city, string county, string postcode, string country, Dictionary<string, Dictionary<string, string>> openhours, Dictionary<string, double> geo, string menuUrl)
        {
            using (var context = new Data.Entities())
            {
                var origMediaPath = Helper.GetMediaPath(id);

                var entity = context.Bars.Single(b => b.Id == id);
                var urlkeyChanged = entity.UrlKey != urlkey;
                var nameChanged = entity.Name != name;

                entity.Modified = DateTime.Now;

                PopulateBarEntity(entity, name, intro, description, urlkey, phone, fax, email, website, street1, street2, city, county, postcode, country, openhours, menuUrl);

                context.SaveChanges();

                if (geo != null)
                {
                    context.ExecuteStoreCommand(string.Format("update Bar set Geocoordinate = geography::STGeomFromText('POINT ({1} {2})', 4326) where Id = '{0}'", entity.Id, geo["lng"], geo["lat"]));
                }

                Log.InfoFormat("Updated bar '{0}' ({1})", name, entity.Id);

                Cache.Bars.Instance.RefreshEntity(entity.Id);
                if (urlkeyChanged || nameChanged)
                {
                    Cache.UrlMap.Instance.RefreshEntity(entity.Id);

                    var newMediaPath = Helper.GetMediaPath(id);

                    Helper.MoveMedia(origMediaPath, newMediaPath);
                }
            }
        }

        [ValidateInput(false)]
        public void EditLocation(Guid id, string synopsis, string intro, string urlkey)
        {
            using (var context = new Data.Entities())
            {
                var entity = context.Locations.Single(l => l.Id == id);
                var urlkeyChanged = entity.UrlKey != urlkey;

                entity.Synopsis = synopsis;
                entity.UrlKey = urlkey;
                entity.Intro = intro;
                entity.Modified = DateTime.Now;

                context.SaveChanges();

                Web.Cache.Locations.Instance.RefreshEntity(entity.Id);
                if (urlkeyChanged)
                {
                    Web.Cache.UrlMap.Instance.RefreshEntity(entity.Id);
                }
            }
        }

        [ValidateInput(false)]
        public string EditDocument(Guid id, string name, string synopsis, string content, string urlkey)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var entity = context.Documents.Single(b => b.Id == id);
                    var urlkeyChanged = entity.UrlKey != urlkey;
                    var nameChanged = entity.Name != name;

                    entity.Name = name;
                    entity.Synopsis = synopsis;
                    entity.Content = content;
                    entity.UrlKey = urlkey;
                    entity.Modified = DateTime.Now;

                    context.SaveChanges();

                    Log.InfoFormat("Updated document '{0}' ({1})", name, entity.Id);

                    Web.Cache.Documents.Instance.RefreshEntity(entity.Id);
                    if (urlkeyChanged || nameChanged)
                    {
                        Web.Cache.UrlMap.Instance.RefreshEntity(entity.Id);
                    }
                }
                return "true";
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to create document named '" + name + "'", ex);

                return "false";
            }
        }

        [ValidateInput(false)]
        public string EditExpert(Guid id, string name, string biography, string urlkey)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var entity = context.Users.Single(u => u.Id == id);

                    var urlkeyChanged = entity.UrlKey != urlkey;
                    var nameChanged = entity.Name != name;

                    entity.Name = name;
                    entity.Biography = biography;
                    entity.UrlKey = urlkey;
                    entity.Modified = DateTime.Now;

                    context.SaveChanges();

                    Web.Cache.Users.Instance.RefreshEntity(entity.Id);
                    if (urlkeyChanged || nameChanged)
                    {
                        Web.Cache.UrlMap.Instance.RefreshEntity(entity.Id);
                    }
                }

                return "true";
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to toggle save expert '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public void EditAdvert(Guid id, string title, string body, string type, string skyscraper, string video, byte weight, DateTime? start, DateTime? finish, string destination)
        {
            using (var context = new Data.Entities())
            {
                var entity = context.Adverts.Single(a => a.Id == id);

                entity.Title = title;
                entity.Body = body;
                entity.SkyscraperUrl = skyscraper;
                entity.VideoUrl = video;
                entity.Start = start;
                entity.Finish = finish;
                entity.Weight = weight;
                entity.Type = type;
                entity.DestinationUrl = destination;

                context.SaveChanges();

                Web.Cache.Adverts.Instance.RefreshEntity(entity.Id);
            }
        }

        public void EditFeature(Guid id, string name, string sponsor, string details, string extra, string type, bool isActive)
        {
            using (var context = new Data.Entities())
            {
                var entity = context.Features.Single(e => e.Id == id);

                entity.Name = name;
                entity.Sponsor = sponsor;
                entity.Details = details;
                entity.Extra = extra;
                entity.Type = type;
                entity.IsActive = isActive;

                context.SaveChanges();

                Web.Cache.Bars.Instance.RefreshEntity(entity.BarId);
            }
        }

        public string DeleteExpert(Guid id)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    foreach (var review in context.Reviews.Where(r => r.UserId == id))
                    {
                        context.Reviews.DeleteObject(review);
                    }
                    context.Users.DeleteObject(context.Users.Single(u => u.Id == id));

                    context.SaveChanges();

                    Web.Cache.Users.Instance.RefreshEntity(id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to delete an expert '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string DeleteExpertReview(Guid id)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var entity = context.Reviews.Single(r => r.Id == id);

                    context.Reviews.DeleteObject(entity);

                    context.SaveChanges();

                    return entity.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to delete a review on bar '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string DeleteAdvert(Guid id)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    foreach (var stat in context.AdvertStats.Where(s => s.AdvertId == id))
                    {
                        context.AdvertStats.DeleteObject(stat);
                    }
                    context.Adverts.DeleteObject(context.Adverts.Single(a => a.Id == id));

                    context.SaveChanges();

                    Web.Cache.Adverts.Instance.RefreshEntity(id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to delete an advert '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string DeleteLocation(Guid id)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    context.Locations.DeleteObject(context.Locations.Single(l => l.Id == id));

                    context.SaveChanges();

                    Web.Cache.Locations.Instance.RefreshEntity(id);
                    Web.Cache.UrlMap.Instance.RefreshEntity(id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to delete a location '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string DeleteFeature(Guid id)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var feature = context.Features.Single(l => l.Id == id);
                    var barId = feature.BarId;
                    context.Features.DeleteObject(feature);

                    context.SaveChanges();

                    Web.Cache.Bars.Instance.RefreshEntity(barId);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to delete a feature '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string MoveLocation(Guid id, Guid parent)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var entity = context.Locations.Single(l => l.Id == id);

                    entity.Parent = context.Locations.Single(l => l.Id == parent);

                    context.SaveChanges();

                    Log.InfoFormat("Moved location '{0}' (to {1})", entity.Id, parent);

                    Web.Cache.Locations.Instance.RefreshEntity(entity.Id);

                    return entity.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to move location '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string MoveBar(Guid id, Guid parent)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var entity = context.Bars.Single(l => l.Id == id);

                    entity.Location = context.Locations.Single(l => l.Id == parent);

                    context.SaveChanges();

                    Log.InfoFormat("Moved bar '{0}' (to {1})", id, parent);

                    Web.Cache.Bars.Instance.RefreshEntity(entity.Id);

                    return entity.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to move location '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string DeleteDocument(Guid id)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    context.Documents.DeleteObject(context.Documents.Single(u => u.Id == id));

                    context.SaveChanges();

                    Web.Cache.UrlMap.Instance.RefreshEntity(id);
                    Web.Cache.Documents.Instance.RefreshEntity(id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to delete a document '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string MoveDocument(Guid id, Guid parent)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var doc = context.Documents.Single(u => u.Id == id);

                    doc.ParentId = parent;

                    context.SaveChanges();

                    Web.Cache.UrlMap.Instance.RefreshEntity(id);
                    Web.Cache.Documents.Instance.RefreshEntity(id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to delete a document '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string ToggleDocumentActivation(Guid id, bool active)
        {
            try
            {
                var changed = false;
                using (var context = new Data.Entities())
                {
                    var entity = context.Documents.Single(u => u.Id == id);

                    changed = entity.IsActive != active;

                    entity.Modified = DateTime.Now;
                    entity.IsActive = active;

                    if (entity.IsActive && string.IsNullOrEmpty(entity.UrlKey))
                    {
                        return "false";
                    }
                    else
                    {
                        context.SaveChanges();
                    }
                }

                if (changed)
                {
                    Web.Cache.UrlMap.Instance.RefreshEntity(id);
                    Web.Cache.Documents.Instance.RefreshEntity(id);

                    Log.InfoFormat("{0} document '{1}'", (active ? "Activated" : "Deactivated"), id);
                }

                return "true";
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to toggle activation on document '" + id.ToString() + "'", ex);
                return "false";
            }
        }

        public string ToggleBarActivation(Guid id, bool active)
        {
            try
            {
                var changed = false;
                using (var context = new Data.Entities())
                {
                    var entity = context.Bars.Single(b => b.Id == id);

                    changed = entity.IsActive != active;

                    entity.Modified = DateTime.Now;
                    entity.IsActive = active;

                    if (entity.IsActive && string.IsNullOrEmpty(entity.UrlKey))
                    {
                        return "false";
                    }
                    else
                    {
                        context.SaveChanges();
                    }
                }

                if (changed)
                {
                    Web.Cache.Bars.Instance.RefreshEntity(id);
                    Web.Cache.UrlMap.Instance.RefreshEntity(id);

                    Log.InfoFormat("{0} bar '{1}'", (active ? "Activated" : "Deactivated"), id);
                }

                return "true";
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to toggle activation on bar '" + id.ToString() + "'", ex);
                return "false";
            }
        }

        public string ToggleExpertActivation(Guid id, bool active)
        {
            try
            {
                var changed = false;
                using (var context = new Data.Entities())
                {
                    var entity = context.Users.Single(u => u.Id == id);

                    changed = entity.IsActive != active;

                    entity.IsActive = active;

                    if (entity.IsActive && string.IsNullOrEmpty(entity.UrlKey))
                    {
                        return "false";
                    }
                    else
                    {
                        context.SaveChanges();
                    }
                }

                if (changed)
                {
                    Web.Cache.Users.Instance.RefreshEntity(id);
                    Web.Cache.UrlMap.Instance.RefreshEntity(id);

                    Log.InfoFormat("{0} expert '{1}'", (active ? "Activated" : "Deactivated"), id);
                }

                return "true";
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to toggle activation on expert '" + id.ToString() + "'", ex);
                return "false";
            }
        }

        public string ToggleAdvertActivation(Guid id, bool active)
        {
            try
            {
                var changed = false;
                using (var context = new Data.Entities())
                {
                    var entity = context.Adverts.Single(u => u.Id == id);

                    changed = entity.IsActive != active;

                    entity.IsActive = active;

                    context.SaveChanges();
                }

                if (changed)
                {
                    Web.Cache.Adverts.Instance.RefreshEntity(id);

                    Log.InfoFormat("{0} advert '{1}'", (active ? "Activated" : "Deactivated"), id);
                }

                return "true";
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to toggle activation on advert '" + id.ToString() + "'", ex);
                return "false";
            }
        }

        public string ModerateUserReview(Guid id, bool approve)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var review = context.Reviews.Single(r => r.Id == id);

                    review.IsModerated = true;
                    review.IsActive = approve;

                    context.SaveChanges();

                    Cache.Reviews.Instance.RefreshEntity(id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to approve user review '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string UpdateUserReview(Guid id, double drinks, double design, double service)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var review = context.Reviews.Single(r => r.Id == id);

                    if (review.Rating == null)
                    {
                        review.Rating = new Data.Rating();
                        review.Rating.Id = Guid.NewGuid();
                    }

                    review.Rating.Drinks = (byte)(drinks / 5.0 * 255.0);
                    review.Rating.Design = (byte)(design / 5.0 * 255.0);
                    review.Rating.Service = (byte)(service / 5.0 * 255.0);

                    context.SaveChanges();

                    Cache.Reviews.Instance.RefreshEntity(id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to approve user review '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string RenameLocation(Guid id, string name)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var location = context.Locations.Single(l => l.Id == id);

                    location.Name = name;

                    context.SaveChanges();

                    Web.Cache.Locations.Instance.RefreshEntity(id);
                    Web.Cache.UrlMap.Instance.RefreshEntity(id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to rename a location '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string RenameDocument(Guid id, string name)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var document = context.Documents.Single(d => d.Id == id);

                    document.Name = name;

                    context.SaveChanges();

                    Web.Cache.Documents.Instance.RefreshEntity(id);
                    Web.Cache.UrlMap.Instance.RefreshEntity(id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to rename a document '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string GenerateTop100()
        {
            var ret = new StringBuilder();

            Log.Info("Generating Top 100");
            ret.AppendLine("Generating Top 100");

            var list = Cache.Lists.Instance.GetByKey("top-100-bars");

            Model.List.ClearList(list.Id);
            var order = new List<Guid>();

            var bars = Cache.Reviews.Instance.GetAll()
              .Where(r => r.IsActive && r.Rating != null && r.Created > DateTime.Today.AddMonths(-12) && r.Rating.Overall > 0)
              .GroupBy(r => r.Bar.Id)
                //.Where(g => g.Count() > 5) // this needs to be added back in when enough ratings have been submitted
              .OrderByDescending(r => r.Average(ra => ra.Rating.Overall))
              .Take(100)
              .Select(r => r.Key);

            var index = 0;
            foreach (var bar in bars)
            {
                var _bar = Cache.Bars.Instance.GetById(bar);
                ret.AppendLine((++index).ToString() + ". " + _bar.Name);

                Model.List.AddToList(list.Id, "Bar", bar);
                order.Add(bar);
            }

            Model.List.UpdateOrder(list.Id, order);
            Cache.Lists.Instance.Update(list.Id);

            Response.ContentType = "text/plain";
            return ret.ToString();
        }

        public string AddRelated(Guid id, Guid right)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    context.ExecuteStoreCommand("insert into Related values ('" + id.ToString() + "', '" + right.ToString() + "')");
                }

                Cache.Relations.Instance.Update(id);

                return "true";
            }
            catch (Exception ex)
            {
                Log.Error("An error occured attach related '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string RemoveRelated(Guid id, Guid right)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    context.ExecuteStoreCommand("delete from Related where LeftId= '" + id.ToString() + "' AND RightId='" + right.ToString() + "'");
                }

                Cache.Relations.Instance.Update(id);

                return "true";
            }
            catch (Exception ex)
            {
                Log.Error("An error occured detach related '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string AddToList(Guid id, Guid item, string type)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var _category = context.Categories.Single(c => c.Id == id);

                    switch (type)
                    {
                        case "Bar":
                            var bar = context.Bars.Single(b => b.Id == item);
                            bar.Categories.Add(_category);
                            break;
                        case "Location":
                            var location = context.Locations.Single(b => b.Id == item);
                            location.Categories.Add(_category);
                            break;
                        case "Document":
                            var document = context.Documents.Single(b => b.Id == item);
                            document.Categories.Add(_category);
                            break;
                    }

                    context.SaveChanges();

                    Cache.Lists.Instance.Update(id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to add an item to a list '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string RemoveFromList(Guid id, Guid item, string type)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var _category = context.Categories.Single(c => c.Id == id);

                    switch (type)
                    {
                        case "Bar":
                            var bar = context.Bars.Single(b => b.Id == item);
                            bar.Categories.Remove(_category);
                            break;
                        case "Location":
                            var location = context.Locations.Single(b => b.Id == item);
                            location.Categories.Remove(_category);
                            break;
                        case "Document":
                            var document = context.Documents.Single(b => b.Id == item);
                            document.Categories.Remove(_category);
                            break;
                    }

                    context.SaveChanges();

                    Cache.Lists.Instance.Update(id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to remove an item from a list '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string CreateList(string name)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var category = new Data.Category()
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Key = name.Slugify(),
                        IsSystem = false,
                        IsSearchable = false
                    };

                    context.Categories.AddObject(category);

                    context.SaveChanges();

                    Cache.Lists.Instance.Update(category.Id);

                    Response.ContentType = "application/json";
                    return JsonConvert.SerializeObject(Cache.Lists.Instance.GetById(category.Id));
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to create list '" + name + "'", ex);

                return "false";
            }
        }

        public string EditList(Guid id, string name, string key, string group, bool searchable)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var category = context.Categories.Single(c => c.Id == id);

                    category.Name = name;
                    category.Key = key;
                    category.Group = group;
                    category.IsSearchable = searchable;

                    context.SaveChanges();

                    Cache.Lists.Instance.Update(category.Id);

                    return "true";
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to create list '" + name + "'", ex);

                return "false";
            }
        }

        public string UploadMedia(Guid id, HttpPostedFileBase file, bool process = true)
        {
            try
            {
                Logic.Helper.UploadImage(id, file.InputStream, process);

                return "true";
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to upload media to '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string DeleteMedia(Guid id, string reference)
        {
            try
            {
                var parts = reference.Split('.');
                Logic.Helper.DeleteMediaGroup(id, int.Parse(parts[parts.Length - 2]));

                return "true";
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to delete media groups '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string FeatureUploadMedia(Guid id, HttpPostedFileBase file)
        {
            try
            {
                Logic.Helper.FeatureUploadImage(id, file.InputStream);

                return "true";
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to upload feature media to '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string SetRedirect(Guid id, Guid parent)
        {
            try
            {
                using (var context = new Data.Entities())
                {
                    var entity = context.Documents.Single(d => d.Id == id);

                    entity.RedirectTo = context.Documents.Single(d => d.Id == parent);

                    context.SaveChanges();

                    Log.InfoFormat("Redirect document '{0}' (to {1})", entity.Id, parent);

                    Web.Cache.Documents.Instance.RefreshEntity(entity.Id);

                    return entity.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured trying to redirect a document '" + id.ToString() + "'", ex);

                return "false";
            }
        }

        public string UpdateSearchIndex()
        {
            Cache.Bars.Instance.UpdateSearchIndex();
            Cache.Locations.Instance.UpdateSearchIndex();
            Cache.Documents.Instance.UpdateSearchIndex();
            Cache.Users.Instance.UpdateSearchIndex();

            return "done.";
        }

        public string UpdateLocationCoordinates()
        {
            using (var context = new Data.Entities())
            {
                foreach (var location in Cache.Locations.Instance.GetAll())
                {
                    var bars = Cache.Bars.Instance.GetByParentRecursive(location.Id);
                    var coords = bars.Where(b => b.Geo != null).Select(b => b.Geo);

                    if (coords.Any())
                    {
                        var latX = 0.00;
                        var latY = 0.00;
                        var longX = 0.00;
                        var longY = 0.00;
                        foreach (var coord in coords)
                        {
                            latX += Math.Cos(coord.Lat * Math.PI / 180.0);
                            latY += Math.Sin(coord.Lat * Math.PI / 180.0);
                            longX += Math.Cos(coord.Long * Math.PI / 180.0);
                            longY += Math.Sin(coord.Long * Math.PI / 180.0);
                        }

                        var lat = Math.Atan2(latY, latX) / Math.PI * 180;
                        var @long = Math.Atan2(longY, longX) / Math.PI * 180;

                        context.ExecuteStoreCommand(string.Format("update Location set Geocoordinate = geography::STGeomFromText('POINT ({1} {2})', 4326) where Id = '{0}'", location.Id, @long, lat));
                    }
                }
            }

            return "done.";
        }

        public void FlushLogs()
        {
            Log.Info("Flushing logs...");
            foreach (var repo in log4net.LogManager.GetAllRepositories())
            {
                foreach (var appender in repo.GetAppenders())
                {
                    var buffered = appender as log4net.Appender.BufferingForwardingAppender;
                    if (buffered != null)
                    {
                        buffered.Flush();
                    }
                }
            }
        }

        [ActionName("email-export")]
        public void EmailExport()
        {
            Response.AddHeader("Content-Disposition", "attachment;filename=emails.txt");
            Response.ContentType = "text/plain";
            foreach (var email in Model.User.GetSubscribedEmails())
            {
                Response.Write(email);
                Response.Write(Environment.NewLine);
            }
        }

        private Data.Bar PopulateBarEntity(Data.Bar entity, string name, string intro, string description, string urlkey, string phone, string fax, string email, string website, string street1, string street2, string city, string county, string postcode, string country, Dictionary<string, Dictionary<string, string>> openhours, string menuUrl)
        {
            entity.Name = name;
            entity.Intro = intro;
            entity.Description = description;
            entity.UrlKey = urlkey;
            entity.Phone = phone;
            entity.Fax = fax;
            entity.Email = email;
            entity.Website = website;
            entity.MenuUrl = menuUrl;
            entity.Address = new XElement("address",
              new XElement("street1", street1),
              new XElement("street2", street2),
              new XElement("city", city),
              new XElement("county", county),
              new XElement("postcode", postcode),
              new XElement("country", country)).ToString();

            entity.OpenHours = new XElement("openhours",
              new XElement("monday",
                new XAttribute("fromA", openhours["monday"]["fromA"]),
                new XAttribute("toA", openhours["monday"]["toA"]),
                new XAttribute("fromB", openhours["monday"]["fromB"]),
                new XAttribute("toB", openhours["monday"]["toB"])),
              new XElement("tuesday",
                new XAttribute("fromA", openhours["tuesday"]["fromA"]),
                new XAttribute("toA", openhours["tuesday"]["toA"]),
                new XAttribute("fromB", openhours["tuesday"]["fromB"]),
                new XAttribute("toB", openhours["tuesday"]["toB"])),
              new XElement("wednesday",
                new XAttribute("fromA", openhours["wednesday"]["fromA"]),
                new XAttribute("toA", openhours["wednesday"]["toA"]),
                new XAttribute("fromB", openhours["wednesday"]["fromB"]),
                new XAttribute("toB", openhours["wednesday"]["toB"])),
              new XElement("thursday",
                new XAttribute("fromA", openhours["thursday"]["fromA"]),
                new XAttribute("toA", openhours["thursday"]["toA"]),
                new XAttribute("fromB", openhours["thursday"]["fromB"]),
                new XAttribute("toB", openhours["thursday"]["toB"])),
              new XElement("friday",
                new XAttribute("fromA", openhours["friday"]["fromA"]),
                new XAttribute("toA", openhours["friday"]["toA"]),
                new XAttribute("fromB", openhours["friday"]["fromB"]),
                new XAttribute("toB", openhours["friday"]["toB"])),
              new XElement("saturday",
                new XAttribute("fromA", openhours["saturday"]["fromA"]),
                new XAttribute("toA", openhours["saturday"]["toA"]),
                new XAttribute("fromB", openhours["saturday"]["fromB"]),
                new XAttribute("toB", openhours["saturday"]["toB"])),
              new XElement("sunday",
                new XAttribute("fromA", openhours["sunday"]["fromA"]),
                new XAttribute("toA", openhours["sunday"]["toA"]),
                new XAttribute("fromB", openhours["sunday"]["fromB"]),
                new XAttribute("toB", openhours["sunday"]["toB"]))
              ).ToString();

            return entity;
        }
    }
}
