using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;

using WorldsBestBars.Logic;
using WorldsBestBars.Web.Models;

namespace WorldsBestBars.Web.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost()]
        public JsonResult Login(string email, string password)
        {
            if (Session.CurrentUser() != null)
            {
                // user already logged in
                return new JsonResult() { Data = new { success = false, reason = "you are already logged in" } };
            }
            else
            {
                if (string.IsNullOrEmpty(email))
                {
                    return new JsonResult() { Data = new { success = false, reason = "you entered an invalid username or password" } };
                }
                else
                {
                    var user = Cache.Users.Instance.GetByEmail(email);

                    if (user != null && user.ValidatePassword(password))
                    {
                        if (user.IsActive)
                        {
                            Session.SetCurrentUser(user);

                            return new JsonResult() { Data = new { success = true, name = user.Name } };
                        }
                        else
                        {
                            return new JsonResult() { Data = new { success = false, reason = "your account has been deactivated, contact us to find out why" } };
                        }
                    }
                    else
                    {
                        return new JsonResult() { Data = new { success = false, reason = "you entered an invalid username or password" } };
                    }
                }
            }
        }

        [HttpPost()]
        public JsonResult Logout()
        {
            if (Session.CurrentUser() == null)
            {
                return new JsonResult() { Data = new { success = false, reason = "you are not logged in" } };
            }
            else
            {
                Session.SetCurrentUser(null);

                return new JsonResult() { Data = new { success = true } };
            }
        }

        [HttpPost()]
        public JsonResult Register(string name, string city, string email, int? dobDay, int? dobMonth, int? dobYear, string password, string passwordConfirm)
        {
            var errors = new List<string>();
            if (Session.CurrentUser() != null)
            {
                // user already logged in
                errors.Add("you are already logged in");
            }

            if (string.IsNullOrEmpty(name))
            {
                errors.Add("you didn't enter your name");
            }

            if (string.IsNullOrEmpty(city))
            {
                errors.Add("you didn't enter your city");
            }

            if (string.IsNullOrEmpty(email))
            {
                errors.Add("you didn't enter your email address");
            }

            if (!Helper.ValidEmail(email))
            {
                errors.Add("you entered an invalid email address");
            }

            if (Services.Users.Helper.IsEmailInUse(email))
            {
                errors.Add("the email address you entered is already in use");
            }

            DateTime dob = DateTime.MinValue;
            if (dobDay == null || dobMonth == null || dobYear == null)
            {
                errors.Add("you didn't enter a correct date for your date of birth");
            }
            else
            {

                try
                {
                    dob = new DateTime((int)dobYear, (int)dobMonth, (int)dobDay);
                }
                catch
                {
                    errors.Add("the date you entered as your date of birth was invalid");
                }
            }

            if (string.IsNullOrEmpty(password))
            {
                errors.Add("you didn't enter a password");
            }
            else
            {
                if (password != passwordConfirm)
                {
                    errors.Add("your passwords don't match");
                }
            }

            try
            {
                var id = Model.User.Register(name, city, email, dob, password);

                Services.Email.MailChimp.Helper.Subscribe(id);

                Cache.Users.Instance.RefreshEntity(id);

                Session.SetCurrentUser(Cache.Users.Instance.GetById(id));

                return new JsonResult() { Data = new { success = true } };
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }

            return new JsonResult() { Data = new { success = false, reason = MakeSentence(errors) } };
        }

        public ActionResult Unsubscribe(string email)
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(email));

                var user = Model.User.GetByEmail(decoded);

                if (user == null)
                {
                    ViewBag.Error = "I'm sorry, we can't find your email on our records.  Are you sure you didn't end up here by mistake?";
                }
                else
                {
                    if (user.Attributes.ContainsKey("email.unsubscribed") && (bool)user.Attributes["email.unsubscribed"])
                    {
                        ViewBag.Error = "It looks like you've already unsubscribed from our mailing list, if you're still receiving emails from us click the link below to contact us.";
                    }
                    else
                    {
                        Services.Email.MailChimp.Helper.Unsubscribe(user.Id);
                        Model.User.SetAttribute(user.Id, "email.unsubscribed", true);
                    }
                }
            }
            catch (FormatException)
            {
                ViewBag.Error = "I'm sorry, we can't determine what email address you're trying to unsubscribe.  Please click the link in the email.";
            }
            catch (ArgumentNullException)
            {
                ViewBag.Error = "I'm sorry, we can't determine what email address you're trying to unsubscribe.  Please click the link in the email.";
            }


            return View();
        }

        public ActionResult LostPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LostPassword(LostPasswordModel model)
        {
            if (!Services.Users.Helper.IsEmailInUse(model.Email))
            {
                ModelState.AddModelError("Email", "Email address not found.");
            }

            if (ModelState.IsValid)
            {
                var service = DependencyResolver.Current.GetService<Services.Users.LostPasswordRequest>();

                service.Execute(model.Email);

                return RedirectToAction("LostPasswordSuccess");
            }

            return View(model);
        }

        public ActionResult LostPasswordSuccess()
        {
            return View();
        }

        public ActionResult UpdateProfile()
        {
            ViewBag.Cities = Cache.Locations.Instance.GetAll().Where(l => l.Parent != null);

            var user = Session.CurrentUser();
            if (user != null)
            {
                var model = new UpdateProfile()
                {
                    Name = user.Name,
                    Email = user.Email,
                    City = user.City,
                    DateOfBirth = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : null,
                    FavouriteCities = user.Attributes.ContainsKey("favourite:cities") ? user.Attributes["favourite:cities"] == null ? null : ((string)user.Attributes["favourite:cities"]).Split(',').Select(s => s.Trim()).ToArray() : null,
                    FavouriteBars = user.Attributes.ContainsKey("favourite:bars") ? (string)user.Attributes["favourite:bars"] : null,
                    FavouriteBrands = user.Attributes.ContainsKey("favourite:brands") ? (string)user.Attributes["favourite:brands"] : null,
                    FavouriteCocktails = user.Attributes.ContainsKey("favourite:cocktails") ? (string)user.Attributes["favourite:cocktails"] : null
                };

                return View(model);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult UpdateProfile(UpdateProfile model)
        {
            var newUser = Session.CurrentUser() == null;

            if (!string.IsNullOrEmpty(model.Password))
            {
                if (model.PasswordConfirm != model.PasswordConfirm)
                {
                    ModelState.AddModelError("ConfirmPassword", "The passwords do not match.");
                }
            }

            if (ModelState.IsValid)
            {
                var updates = new Services.Models.UpdateUser()
                {
                    Name = model.Name,
                    Email = model.Email,
                    City = model.City,
                    DateOfBirth = string.IsNullOrEmpty(model.DateOfBirth) ? null : (DateTime?)DateTime.Parse(model.DateOfBirth),
                    FavouriteBars = model.FavouriteBars ?? string.Empty,
                    FavouriteBrands = model.FavouriteBrands ?? string.Empty,
                    FavouriteCocktails = model.FavouriteCocktails ?? string.Empty,
                    FavouriteCities = string.Join(", ", model.FavouriteCities ?? new string[0]),
                    Password = string.IsNullOrEmpty(model.Password) ? null : model.Password
                };

                if (newUser)
                {
                    var service = new Services.Users.CreateUser();

                    var id = service.Execute(updates);

                    Cache.Users.Instance.RefreshEntity(id);

                    Session.SetCurrentUser(Cache.Users.Instance.GetById(id));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var service = new Services.Users.UpdateUser();

                    service.Execute(Session.CurrentUser().Id, updates);

                    Cache.Users.Instance.RefreshEntity(Session.CurrentUser().Id);

                    Session.SetCurrentUser(Cache.Users.Instance.GetById(Session.CurrentUser().Id));

                    return RedirectToAction("UpdateProfile");
                }
            }

            ViewBag.Cities = Cache.Locations.Instance.GetAll().Where(l => l.Parent != null);

            return View(model);
        }

        static string MakeSentence(IEnumerable<string> parts)
        {
            if (parts.Count() == 1)
            {
                return parts.ElementAt(0);
            }
            else if (parts.Count() == 2)
            {
                return parts.ElementAt(0) + " and " + parts.ElementAt(1);
            }
            else
            {
                var ret = new StringBuilder();
                for (var i = 0; i < parts.Count() - 1; i++)
                {
                    ret.Append(parts.ElementAt(i));
                    ret.Append(", ");
                }
                ret.Remove(ret.Length - 2, 2);
                ret.Append(" and ");
                ret.Append(parts.ElementAt(parts.Count() - 1));

                return ret.ToString();
            }
        }
    }
}
