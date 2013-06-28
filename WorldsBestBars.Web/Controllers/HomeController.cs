using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace WorldsBestBars.Web.Controllers
{
    public class HomeController : Controller
    {
        log4net.ILog Log = log4net.LogManager.GetLogger(typeof(HomeController));

        [OutputCache(Duration = 30)]
        public ActionResult Index()
        {
            var advert = WorldsBestBars.Logic.Adverts.GetRandomAdvert(Session);
            var experts = Cache.Users.Instance.GetExperts().OrderBy(e => Guid.NewGuid());
            var count = (int)Math.Floor((double)experts.Count() / 3.0) * 3;
            ViewBag.Experts = experts.Take(count);
            ViewBag.ExpertCount = count;
            ViewBag.TopCities = Cache.Lists.Instance.GetByKey("top-cities").Locations.OrderBy(_ => Guid.NewGuid());
            ViewBag.FeaturedArticles = Cache.Lists.Instance.GetByKey("featured-articles").Documents.OrderBy(_ => Guid.NewGuid()).Take(4);
            ViewBag.AltFeaturedArticles = Cache.Lists.Instance.GetByKey("featured-articles-2").Documents.OrderBy(_ => Guid.NewGuid()).Take(advert == null || string.IsNullOrEmpty(advert.Video) ? 3 : 2);
            ViewBag.MostTalkedAbout = Cache.Lists.Instance.GetByKey("most-talked-about").Bars.OrderBy(_ => Guid.NewGuid()).Take(5);
            ViewBag.RecentReviews = Cache.Reviews.Instance.GetRecent(5);

            return View();
        }

        [OutputCache(Duration = 30)]
        public ActionResult ExpertsChoice()
        {
            return View(Cache.Users.Instance.GetExperts().OrderBy(e => e.Name));
        }

        [OutputCache(Duration = 30)]
        public ActionResult Top100()
        {
            var entities = Cache.Lists.Instance.GetByKey("top-100-bars").Bars;
            var bars = entities.Select(e => Cache.Bars.Instance.GetById(e.Id));
            ViewBag.Cities = bars.Where(b => b.Address != null).Select(b => b.Address.City).Distinct().OrderBy(c => c);

            return View(bars);
        }

        [OutputCache(Duration = 30)]
        public ActionResult MostTalkedAbout()
        {
            var entities = Cache.Lists.Instance.GetByKey("most-talked-about").Bars.OrderBy(_ => Guid.NewGuid());
            var bars = entities.Select(e => Cache.Bars.Instance.GetById(e.Id));
            ViewBag.Cities = bars.Where(b => b.Address != null).Select(b => b.Address.City).Distinct().OrderBy(c => c);

            return View(bars);
        }

        [OutputCache(Duration = 120)]
        public ActionResult AllBars()
        {
            return View(Cache.Bars.Instance.GetAll().OrderBy(b => b.Name));
        }

        [OutputCache(Duration = 120)]
        public ActionResult AllCities()
        {
            var barLocation = Cache.Bars.Instance.GetAll().Where(b => b.Parent != null).Select(b => b.Parent.Id);
            return View(Cache.Locations.Instance.GetAll().Where(l => barLocation.Contains(l.Id)).OrderBy(l => l.Name));
        }

        [OutputCache(Duration = 120)]
        public ActionResult AllArticles()
        {
            var documents = Cache.Documents.Instance.GetAll().Where(d => d.IsActive).OrderByDescending(d => d.Created);
            return View(documents);
        }

        [OutputCache(Duration = 120)]
        public ActionResult AboutUs()
        {
            return View();
        }

        [OutputCache(Duration = 120)]
        public ActionResult TermsAndConditions()
        {
            return View();
        }

        [OutputCache(Duration = 120)]
        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        [OutputCache(Duration = 120)]
        public ActionResult CookiePolicy()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AgeGate(string redirect)
        {
            if (Request.Cookies["wbb:age_check"] != null)
            {
                Session["age.check"] = true;
            }

            if (Session["age.check"] != null)
            {
                if (!string.IsNullOrEmpty(redirect))
                {
                    if (!redirect.StartsWith("/")) { redirect = "/" + redirect; }
                    return Redirect(redirect);
                }
                else
                {
                    return Redirect("/");
                }
            }

            var countries = Model.Country.GetAll();
            ViewBag.Countries = countries;
            ViewBag.Months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sept", "Oct", "Nov", "Dec" };

            if (Request.Url.Host == "worldsbestbars.local" || Request.UserHostAddress == "127.0.0.1" || Request.UserHostAddress == "::1")
            {
                ViewBag.DetectedCountry = "US";
            }
            else
            {
                var lookup = new Logic.GeoIP.LookupService(Server.MapPath("~/App_Data/GeoLiteCity.dat"), Logic.GeoIP.LookupService.GEOIP_MEMORY_CACHE);
                var country = lookup.getCountry(Request.UserHostAddress);
                if (country != null)
                {
                    ViewBag.DetectedCountry = country.getCode();
                }
            }

            var oldestDate = DateTime.Today.AddYears(-21);
            if (ViewBag.DetectedCountry != null)
            {
                var country = countries.SingleOrDefault(c => c.ShortISO == ViewBag.DetectedCountry);

                if (country != null && country.MinAge.HasValue)
                {
                    oldestDate = DateTime.Now.AddYears(-country.MinAge.Value).AddDays(-1);
                }
            }

            ViewBag.SelectedDay = oldestDate.Day;
            ViewBag.SelectedMonth = oldestDate.Month;
            ViewBag.SelectedYear = oldestDate.Year;

            return View("AgeGate", (object)redirect);
        }

        [HttpPost]
        public ActionResult AgeGate(string redirect, string country, string remember, int? day, int? month, int? year)
        {
            Guid _country = Guid.Empty;

            ViewBag.SelectedCountry = country;
            ViewBag.SelectedDay = day;
            ViewBag.SelectedMonth = month;
            ViewBag.SelectedYear = year;

            if (!Guid.TryParse(country, out _country))
            {
                ViewBag.Errors = "you didn't select your country";
            }

            if (day == null || month == null || year == null)
            {
                if (ViewBag.Errors != null)
                {
                    ViewBag.Errors += " and you didn't specify your date of birth";
                }
                else
                {
                    ViewBag.Errors = "you didn't specify your date of birth";
                }
            }
            else
            {
                try
                {
                    var dob = new DateTime((int)year, (int)month, (int)day);
                    if (Model.Country.VerifyAge(dob, _country))
                    {
                        Session["age.check"] = true;

                        if (remember == "yes")
                        {
                            Response.SetCookie(new HttpCookie("wbb:age_check", "true") { Expires = DateTime.Now.AddDays(7) });
                        }

                        if (!redirect.StartsWith("/")) { redirect = "/" + redirect; }
                        return Redirect(redirect);
                    }
                    else
                    {
                        ViewBag.Errors = "you are too young";
                    }
                }
                catch (Model.InvalidCountryException)
                {
                }
                catch (Model.CountryNotAvailableException)
                {
                    ViewBag.Errors = "this website is not available in the country you have selected";
                }
                catch (ArgumentOutOfRangeException)
                {
                    if (ViewBag.Errors != null)
                    {
                        ViewBag.Errors += " and you didn't specify your date of birth";
                    }
                    else
                    {
                        ViewBag.Errors = "you didn't specify your date of birth";
                    }
                }
            }

            if (ViewBag.Errors != null) { ViewBag.Errors += "."; }

            return AgeGate(redirect);
        }

        [OutputCache(Duration = 120)]
        [Mvc.Filters.CustomContentType(ContentType = "text/xml")]
        public XDocument Sitemap()
        {
            var root = new XElement("urlset"/*, new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9")*/);

            foreach (var map in Cache.UrlMap.Instance.GetAll())
            {
                root.Add(new XElement("url",
                    new XElement("loc", "http://www.worldsbestbars.com/" + map.Url),
                    new XElement("lastmod", DateTime.Today.ToString("yyyy-MM-dd")),
                    new XElement("changefreq", "daily"),
                    new XElement("priority", "0.8")
                    ));
            }

            var doc = new XDocument(root);
            doc.Declaration = new XDeclaration("1.0", "utf-8", "yes");

            return doc;
        }
    }
}