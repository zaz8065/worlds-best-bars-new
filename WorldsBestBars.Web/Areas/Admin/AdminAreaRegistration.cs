using System.Web.Mvc;

namespace WorldsBestBars.Web.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("Admin_Login",
                "admin/auth/login",
                new
                {
                    controller = "Auth",
                    action = "Login"
                },
                new string[] { "WorldsBestBars.Web.Areas.Admin.Controllers" });

            context.MapRoute(
                "Admin_Default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new string[] { "WorldsBestBars.Web.Areas.Admin.Controllers" }
            );
        }
    }
}