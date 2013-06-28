using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorldsBestBars.Web.Areas.Admin.Controllers
{
    public class AuthController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost()]
        public ActionResult Login(string user, string pass)
        {
            var _user = Cache.Users.Instance.GetByEmail(user);
            if (_user == null || !_user.ValidatePassword(pass))
            {
                ViewBag.Error = true;
                return View();
            }

            Session["user"] = _user;
            return Redirect("/admin");
        }

        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("Login");
        }
    }
}