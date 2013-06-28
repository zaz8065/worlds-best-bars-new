using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using WorldsBestBars.Data;
using WorldsBestBars.Logic;
using WorldsBestBars.Web.Areas.BarResource.Models;

namespace WorldsBestBars.Web.Areas.BarResource.Controllers
{
    public class HomeController : Controller
    {
        Entities db = new Entities();

        public ActionResult Index()
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(string query)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Query = query;

            return View(Search.Lucene.Search(query, "Bar").Take(10).Select(r => Model.Bar.Convert(db.Bars.Single(b => b.Id == r.Id))).ToList());
        }

        public ActionResult Login(string redirect)
        {
            ViewBag.Redirect = redirect;

            return View();
        }

        [HttpPost]
        public ActionResult Login(Login model, string redirect)
        {
            if (ModelState.IsValid)
            {
                var user = Cache.Users.Instance.GetByEmail(model.Username);
                if (user == null)
                {
                    ModelState.AddModelError("Username", "Invalid username");
                }
                else if (user.ValidatePassword(model.Password))
                {
                    if (user.IsActive)
                    {
                        Session.SetCurrentUser(user);

                        if (string.IsNullOrEmpty(redirect))
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return Redirect(redirect);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Username", "Your account has been disabled.  Contact us to find out why");
                    }
                }
                else
                {
                    ModelState.AddModelError("Password", "Invalid password");
                }
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            Session.SetCurrentUser(null);

            return RedirectToAction("Login");
        }

        public ActionResult Accreditations()
        {
            return View();
        }

        public ActionResult Edit(Guid id)
        {
            return RedirectToAction("EditPictures", new { id = id });
        }

        public ActionResult EditPictures(Guid id)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() });
            }

            ViewBag.EditingContainer = GetContainer(id);

            return View(GetContainer(id).Pictures);
        }

        [HttpPost]
        public ActionResult EditPictures(Guid id, string next, string back)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() });
            }

            if (!string.IsNullOrEmpty(back))
            {
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(next))
            {
                return RedirectToAction("EditOpenHours");
            }

            var editingContainer = GetContainer(id);

            if (Request.Files != null && Request.Files.Count > 0)
            {
                var pathPhysical = Server.MapPath("~/content/media/_bar-resource/" + editingContainer.Id.ToString() + "/pictures");
                var pathVirtual = "~/content/media/_bar-resource/" + editingContainer.Id.ToString() + "/pictures";
                if (!System.IO.Directory.Exists(pathPhysical))
                {
                    System.IO.Directory.CreateDirectory(pathPhysical);
                }

                for (var i = 0; i < Request.Files.Count; i++)
                {
                    var index = 1;
                    var filename = string.Empty;
                    while (System.IO.File.Exists(filename = System.IO.Path.Combine(pathPhysical, index.ToString("000") + ".jpg"))) { index++; }

                    var file = Request.Files[i];
                    if (file.ContentLength > 0)
                    {
                        if (file.ContentType.StartsWith("image"))
                        {
                            editingContainer.Pictures.Pending.Add(System.IO.Path.GetFileName(filename));
                            file.SaveAs(filename);
                        }
                    }

                }
            }

            ViewBag.EditingContainer = editingContainer;

            return RedirectToAction("EditPictures", new { id = id });
        }

        public ActionResult EditOpenHours(Guid id)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() + "/open-hours" });
            }

            ViewBag.EditingContainer = GetContainer(id);

            return View(GetContainer(id).OpeningHours);
        }

        [HttpPost]
        public ActionResult EditOpenHours(Guid id, EditBarOpenHours model, string back)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() + "/open-hours" });
            }

            if (!string.IsNullOrEmpty(back))
            {
                return RedirectToAction("EditPictures");
            }

            var editingContainer = GetContainer(id);

            if (ModelState.IsValid)
            {
                editingContainer.OpeningHours = model;

                return RedirectToAction("EditMenus");
            }

            ViewBag.EditingContainer = editingContainer;

            return View(model);
        }

        public ActionResult EditMenus(Guid id)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() + "/menus" });
            }

            ViewBag.EditingContainer = GetContainer(id);

            return View(GetContainer(id).Menus);
        }

        [HttpPost]
        public ActionResult EditMenus(Guid id, EditBarMenus model, string back)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() + "/menus" });
            }

            if (!string.IsNullOrEmpty(back))
            {
                return RedirectToAction("EditOpenHours");
            }

            var editingContainer = GetContainer(id);

            if (ModelState.IsValid)
            {
                editingContainer.Menus.MenuUrl = model.MenuUrl;

                if (Request.Files != null && Request.Files.Count > 0)
                {
                    var pathPhysical = Server.MapPath("~/content/media/_bar-resource/" + editingContainer.Id.ToString() + "/menus");
                    var pathVirtual = "~/content/media/_bar-resource/" + editingContainer.Id.ToString() + "/menus";
                    if (!System.IO.Directory.Exists(pathPhysical))
                    {
                        System.IO.Directory.CreateDirectory(pathPhysical);
                    }

                    for (var i = 0; i < Request.Files.Count; i++)
                    {
                        var index = 1;
                        var filename = string.Empty;
                        var file = Request.Files[i];

                        while (System.IO.File.Exists(filename = System.IO.Path.Combine(pathPhysical, index.ToString("000") + System.IO.Path.GetExtension(file.FileName)))) { index++; }

                        if (file.ContentLength > 0)
                        {
                            (editingContainer.Menus.Files = editingContainer.Menus.Files ?? new List<string>()).Add(System.IO.Path.GetFileName(filename));
                            file.SaveAs(filename);
                        }

                    }
                }

                return RedirectToAction("EditContactDetails");
            }

            ViewBag.EditingContainer = editingContainer;

            return View(model);
        }

        public ActionResult EditContactDetails(Guid id)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() + "/contact-details" });
            }

            ViewBag.EditingContainer = GetContainer(id);

            return View(GetContainer(id).Contact);
        }

        [HttpPost]
        public ActionResult EditContactDetails(Guid id, EditBarContact model, string back)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() + "/contact-details" });
            }

            if (!string.IsNullOrEmpty(back))
            {
                return RedirectToAction("EditMenus");
            }

            var editingContainer = GetContainer(id);

            if (ModelState.IsValid)
            {
                editingContainer.Contact = model;

                return RedirectToAction("EditType");
            }

            ViewBag.EditingContainer = editingContainer;

            return View(model);
        }

        public ActionResult EditType(Guid id)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() + "/type" });
            }

            ViewBag.EditingContainer = GetContainer(id);

            return View(GetContainer(id).TypeOfBar);
        }

        [HttpPost]
        public ActionResult EditType(Guid id, string[] style, string[] speciality, string[] atmosphere, string back)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() + "/type" });
            }

            if (!string.IsNullOrEmpty(back))
            {
                return RedirectToAction("EditContactDetails");
            }

            var editingContainer = GetContainer(id);

            editingContainer.TypeOfBar.Styles = style ?? new string[0];
            editingContainer.TypeOfBar.Specialities = speciality ?? new string[0];
            editingContainer.TypeOfBar.Atmosphere = atmosphere ?? new string[0];

            return RedirectToAction("EditFeatures");
        }

        public ActionResult EditFeatures(Guid id)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() + "/features" });
            }

            ViewBag.EditingContainer = GetContainer(id);

            return View(GetContainer(id).Features);
        }

        [HttpPost]
        public ActionResult EditFeatures(Guid id, string back, string action, string title, string description, string removeId)
        {
            if (Session.CurrentUser() == null)
            {
                return RedirectToAction("Login", new { redirect = "/bar-resource/" + id.ToString() + "/features" });
            }

            if (!string.IsNullOrEmpty(back))
            {
                return RedirectToAction("EditType");
            }

            var editingContainer = GetContainer(id);
            ViewBag.EditingContainer = editingContainer;

            if (action == "remove")
            {
                var item = editingContainer.Features.List.FirstOrDefault(f => f.Id.ToString() == removeId);
                System.IO.File.Delete(item.ImagePhysical);
                editingContainer.Features.List.Remove(item);

                return RedirectToAction("EditFeatures", new { id = id });
            }
            else if (action == "add")
            {
                ModelState.Clear();

                ViewBag.ModelTitle = title;
                ViewBag.ModelDescription = description;

                if (string.IsNullOrWhiteSpace(title))
                {
                    ModelState.AddModelError("Title", "We need to know what to call it..");
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    ModelState.AddModelError("Description", "There must be more to this feature than its name");
                }

                if (Request.Files == null || Request.Files.Count == 0 || Request.Files[0].ContentLength == 0)
                {
                    ModelState.AddModelError("File", "Surely you must have a poster or something for this feature");
                }
                else
                {
                    var contentType = Request.Files[0].ContentType;
                    if (!contentType.StartsWith("image"))
                    {
                        ModelState.AddModelError("File", "We can only accept images to show with your feature");
                    }
                }

                if (ModelState.IsValid)
                {
                    var _id = Guid.NewGuid();
                    var file = Request.Files[0];
                    var pathPhysical = Server.MapPath("~/content/media/_bar-resource/" + editingContainer.Id.ToString() + "/features/" + _id.ToString() + System.IO.Path.GetExtension(file.FileName));
                    var pathVirtual = "~/content/media/_bar-resource/" + editingContainer.Id.ToString() + "/features/" + _id.ToString() + System.IO.Path.GetExtension(file.FileName);
                    var directory = Path.GetDirectoryName(pathPhysical);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    editingContainer.Features.List.Add(new EditBarFeatures.Feature()
                    {
                        Id = _id,
                        Title = title,
                        Description = description,
                        Image = pathVirtual,
                        ImagePhysical = pathPhysical
                    });

                    file.SaveAs(pathPhysical);

                    return RedirectToAction("EditFeatures", new { id = id });
                }
                else
                {
                    return View(editingContainer.Features);
                }
            }
            else
            {
                return RedirectToAction("EditSignatureCocktail");
            }
        }

        public ActionResult EditSignatureCocktail(Guid id)
        {
            var editingContainer = GetContainer(id);

            ViewBag.EditingContainer = editingContainer;

            return View(GetContainer(id).SignatureCocktail);
        }

        [HttpPost]
        public ActionResult EditSignatureCocktail(Guid id, EditSignatureCocktail model, string next, string back)
        {
            var editingContainer = GetContainer(id);

            if (!string.IsNullOrEmpty(next))
            {
                // save
                var directory = Server.MapPath("~/content/media/_bar-resource/" + editingContainer.Id.ToString());
                Directory.CreateDirectory(directory);
                var data = Path.Combine(directory, "content.xml");
                using (var fs = new FileStream(data, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    var serializer = new XmlSerializer(typeof(EditBarContainer));
                    serializer.Serialize(fs, editingContainer);
                }

                return RedirectToAction("Accreditations");
            }

            if (!string.IsNullOrEmpty(back))
            {
                return RedirectToAction("EditFeatures", new { id = id });
            }

            if (ModelState.IsValid)
            {
                editingContainer.SignatureCocktail = model;
            }

            ViewBag.EditingContainer = editingContainer;

            return View(GetContainer(id).SignatureCocktail);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateBar model)
        {
            if (Request.Files.Count == 0 || Request.Files[0].ContentLength == 0)
            {
                ModelState.AddModelError("Upload", "We'll need some pictures of your establishment to help make our decision");
            }

            if (ModelState.IsValid)
            {
                // save
                var id = Guid.NewGuid();

                var directory = Server.MapPath("~/content/media/_bar-resource/" + id.ToString());
                Directory.CreateDirectory(directory);

                if (Request.Files != null && Request.Files.Count > 0)
                {
                    for (var i = 0; i < Request.Files.Count; i++)
                    {
                        var index = 1;
                        var filename = string.Empty;
                        var file = Request.Files[i];

                        while (System.IO.File.Exists(filename = System.IO.Path.Combine(directory, index.ToString("000") + System.IO.Path.GetExtension(file.FileName)))) { index++; }

                        if (file.ContentLength > 0)
                        {
                            file.SaveAs(filename);
                        }

                    }
                }

                var data = Path.Combine(directory, "content.xml");
                using (var fs = new FileStream(data, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    var serializer = new XmlSerializer(typeof(CreateBar));
                    serializer.Serialize(fs, model);
                }

                return RedirectToAction("CreateThanks");
            }

            return View(model);
        }

        public ActionResult CreateThanks()
        {
            return View();
        }

        private EditBarContainer GetContainer(Guid id)
        {
            var sessionKey = "wbb:bar_resource:edit_bar:" + id.ToString();
            if (Session[sessionKey] as EditBarContainer == null)
            {
                var container = new EditBarContainer();
                var bar = Model.Bar.Convert(db.Bars.SingleOrDefault(b => b.Id == id));

                container.Name = bar.Name;
                container.Id = Guid.NewGuid();
                container.BarId = bar.Id;
                container.IsNew = false;
                container.IsModerated = false;

                container.Features.List = new List<EditBarFeatures.Feature>();

                container.Contact.Address = bar.Address.ToString();
                container.Contact.Website = bar.Website;
                container.Contact.Phone = bar.Phone;
                container.Contact.Email = bar.Email;
                container.Contact.Geo = string.Format("{0}, {1}", bar.Geo.Lat, bar.Geo.Long);

                container.Pictures.Existing = new List<string>();
                var existing = Logic.Helper.GetMediaGroups(new UrlHelper(ControllerContext.RequestContext), id);
                if (existing != null)
                {
                    container.Pictures.Existing.AddRange(existing);
                }
                container.Pictures.Pending = new List<string>();
                var existingPath = Server.MapPath("~/content/media/_bar-resource/" + id.ToString() + "/pictures");
                if (System.IO.Directory.Exists(existingPath))
                {
                    container.Pictures.Pending.AddRange(System.IO.Directory.GetFiles(existingPath).Select(f => new System.IO.FileInfo(f).Name));
                }

                container.OpeningHours.Days = new List<EditBarOpenHours.Day>();
                string[] Days = new string[] { "Mon", "Tues", "Wed", "Thurs", "Fri", "Sat", "Sun" };
                for (var i = 0; i < 7; i++)
                {
                    var day = new EditBarOpenHours.Day();
                    day.Name = Days[i];
                    day.TimeFrames = new List<EditBarOpenHours.Day.TimeFrame>();
                    for (var j = 0; j < 2; j++)
                    {
                        day.TimeFrames.Add(new EditBarOpenHours.Day.TimeFrame());
                    }
                    container.OpeningHours.Days.Add(day);
                }

                container.TypeOfBar.Styles = new string[0];
                container.TypeOfBar.Specialities = new string[0];
                container.TypeOfBar.Atmosphere = new string[0];

                Session[sessionKey] = container;
            }

            return Session[sessionKey] as EditBarContainer;
        }

        public EditBarContainer CreateContainer(string name)
        {
            var container = new EditBarContainer();

            container.Name = name;
            container.Id = Guid.NewGuid();
            container.IsModerated = false;

            container.Contact.Geo = string.Format("{0}, {1}", 0, 0);

            container.Pictures.Existing = new List<string>();
            container.Pictures.Pending = new List<string>();

            container.Features.List = new List<EditBarFeatures.Feature>();

            container.OpeningHours.Days = new List<EditBarOpenHours.Day>();
            string[] Days = new string[] { "Mon", "Tues", "Wed", "Thurs", "Fri", "Sat", "Sun" };
            for (var i = 0; i < 7; i++)
            {
                var day = new EditBarOpenHours.Day();
                day.Name = Days[i];
                day.TimeFrames = new List<EditBarOpenHours.Day.TimeFrame>();
                for (var j = 0; j < 2; j++)
                {
                    day.TimeFrames.Add(new EditBarOpenHours.Day.TimeFrame());
                }
                container.OpeningHours.Days.Add(day);
            }

            container.TypeOfBar.Styles = new string[0];
            container.TypeOfBar.Specialities = new string[0];
            container.TypeOfBar.Atmosphere = new string[0];

            Session["wbb:bar_resource:edit_bar:" + container.Id.ToString()] = container;

            return container;
        }
    }
}
