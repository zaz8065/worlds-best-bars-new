using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WorldsBestBars.Services;
using WorldsBestBars.Web.Admin.Models;
using WorldsBestBars.Web.Admin.Security;

namespace WorldsBestBars.Web.Admin.Controllers
{
    public class AuthenticationController : Controller
    {
        #region Constructors

        public AuthenticationController(UserService service)
        {
            _service = service;
        }

        #endregion

        #region Public Methods

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            Session["login:redirect"] = returnUrl;

            return View();
        }

        [AllowAnonymous, HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                Guid id;
                if (_service.ValidateUser(model.Email, model.Password, out id))
                {
                    var expires = DateTime.Now.AddDays(2);

                    var ticket = FormsAuthentication.Encrypt(new FormsAuthenticationTicket(1, id.ToString(), DateTime.Now, expires, true, string.Empty));
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, ticket)
                    {
                        Expires = expires
                    };

                    Response.Cookies.Add(cookie);
                    HttpContext.User = MemberPrincipal.Get(id);

                    var redirect = (Session["login:redirect"] as string) ?? "/";
                    Session.Remove("login:redirect");

                    return Redirect(redirect);
                }

                ModelState.AddModelError("Password", "Invalid username or password.");
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Login");
        }

        #endregion

        #region Private Fields

        UserService _service;

        #endregion
    }
}
