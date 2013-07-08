using System;
using System.Web.Mvc;
using System.Net.Mail;
using System.Text;

using WorldsBestBars.Logic;

namespace WorldsBestBars.Web.Controllers
{
    public class ProcessController : Controller
    {
        public ActionResult SubmitReview(Guid id, string title, string comment, double design, double drinks, double service)
        {
            if (string.IsNullOrEmpty(title))
            {
                TempData["submit_review:errors"] = "you didn't specify a title for your review";
            }
            if (string.IsNullOrEmpty(comment))
            {
                if (TempData["submit_review:errors"] != null) { TempData["submit_review:errors"] += " and "; }
                TempData["submit_review:errors"] += "you didn't write anything";
            }

            if (TempData["submit_review:errors"] == null)
            {
                var user = Session.CurrentUser();

                var review = Model.Review.Create(id, user == null ? null : (Guid?)user.Id, null, Request.UserHostAddress, title, comment, design, drinks, service);
                if (review != null)
                {
                    Cache.Reviews.Instance.RefreshEntity((Guid)review);
                }

                TempData["submit_review:success"] = true;
            }
            else
            {
                TempData["submit_review:data:title"] = title;
                TempData["submit_review:data:comment"] = comment;
                TempData["submit_review:data:design"] = design;
                TempData["submit_review:data:drinks"] = drinks;
                TempData["submit_review:data:service"] = service;

                TempData["submit_review:success"] = false;
            }

            var map = Web.Cache.UrlMap.Instance.GetById(id);

            return Redirect("~/" + map.Url);
        }

        public ActionResult SubmitReviewReply(Guid id, string comment)
        {
            var review = Model.Review.GetById(id);

            if (string.IsNullOrEmpty(comment))
            {
                TempData["submit_review_reply:errors"] += "you didn't enter any comment";
            }

            if (TempData["submit_review_reply:errors"] == null)
            {
                var user = Session.CurrentUser();

                if (user != null)
                {
                    var reply = Model.Review.Create(review.Bar.Id, user.Id, id, Request.UserHostAddress, null, comment);
                    if (reply != null)
                    {
                        Cache.Reviews.Instance.RefreshEntity((Guid)reply);
                    }
                }
            }

            var map = Web.Cache.UrlMap.Instance.GetById(review.Bar.Id);

            return Redirect("~/" + map.Url);
        }

        public ActionResult Share(Guid id, string recipient, string sender, string message)
        {
            if (string.IsNullOrEmpty(recipient))
            {
                TempData["share_email:errors"] += "you didn't say who this should be emailed to";
            }
            if (string.IsNullOrEmpty(sender))
            {
                if (TempData["share_email:errors"] != null) { TempData["share_email:errors"] += " and "; }

                TempData["share_email:errors"] += "missing_field:comment";
            }

            if (TempData["share_email:errors"] == null)
            {
                var bar = Web.Cache.Bars.Instance.GetById(id);

                using (var client = new SmtpClient())
                {
                    if (client.Host == "smtp.gmail.com")
                    {
                        client.EnableSsl = true;
                    }

                    var body = new StringBuilder();
                    body.AppendLine(string.Format("From: {0}", sender));

                    body.AppendLine(bar.Name);
                    if (bar.Address != null)
                    {
                        body.AppendLine(string.Format("Address: {0}", bar.Address));
                    }
                    if (!string.IsNullOrEmpty(bar.Phone))
                    {
                        body.AppendLine(string.Format("Telephone: {0}", bar.Phone));
                    }
                    if (!string.IsNullOrEmpty(bar.Website))
                    {
                        body.AppendLine(string.Format("Website: {0}", bar.Website));
                    }
                    body.AppendLine();
                    body.AppendLine("Check it out and see what others have to say about it on World's Best Bars - http://www.worldsbestbars.com" + bar.Url);
                    body.AppendLine();
                    body.AppendLine(bar.Description);

                    if (!string.IsNullOrEmpty(message))
                    {
                        body.AppendLine();
                        body.AppendLine("--");
                        body.AppendLine();
                        body.AppendLine(message);
                    }

                    var _message = new MailMessage()
                    {
                        Subject = "Check out this great bar - " + bar.Name,
                        Body = body.ToString(),
                        IsBodyHtml = false
                    };

                    _message.To.Add(recipient);

                    try
                    {
                        client.Send(_message);
                    }
                    catch(Exception ex)
                    {
                        log4net.LogManager.GetLogger(typeof(ProcessController)).Error("Email sending failed", ex);

                        TempData["share_email:errors"] = "there's a problem with our mail server, we're looking into it and should have it fixed in no time";
                    }
                }
            }

            var map = Web.Cache.UrlMap.Instance.GetById(id);

            return Redirect("~/" + map.Url);
        }

        public ActionResult Reset()
        {
            Session.Clear();
            if (Response.Cookies["wbb:age_check"] != null)
            {
                Response.Cookies["wbb:age_check"].Expires = DateTime.Now.AddDays(-1);
            }

            return Redirect("~/");
        }

        public void RefreshEntity(string type, Guid id)
        {
            switch (type)
            {
                case "bar":
                    Cache.Bars.Instance.RefreshEntity(id);
                    Cache.UrlMap.Instance.RefreshEntity(id);
                    break;
                case "location":
                    Cache.Locations.Instance.RefreshEntity(id);
                    Cache.UrlMap.Instance.RefreshEntity(id);
                    break;
                case "advert":
                    Cache.Adverts.Instance.RefreshEntity(id);
                    break;
                case "document":
                    Cache.Documents.Instance.RefreshEntity(id);
                    Cache.UrlMap.Instance.RefreshEntity(id);
                    break;
                case "expert":
                    Cache.Users.Instance.RefreshEntity(id);
                    Cache.UrlMap.Instance.RefreshEntity(id);
                    break;
                case "list":
                    Cache.Lists.Instance.Update(id);
                    break;
            }
        }
    }
}