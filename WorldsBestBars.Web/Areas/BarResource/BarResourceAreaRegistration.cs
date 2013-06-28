using System.Web.Mvc;

namespace WorldsBestBars.Web.Areas.BarResource
{
    public class BarResourceAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "BarResource";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("Bar Resource - Home", "bar-resource", new { controller = "Home", action = "Index" });

            context.MapRoute("Bar Resource - Login", "bar-resource/login", new { controller = "Home", action = "Login" });
            context.MapRoute("Bar Resource - Logout", "bar-resource/logout", new { controller = "Home", action = "Logout" });
            
            context.MapRoute("Bar Resource - Create", "bar-resource/create", new { controller = "Home", action = "Create" });
            context.MapRoute("Bar Resource - Create Thanks", "bar-resource/create/thanks", new { controller = "Home", action = "CreateThanks" });

            context.MapRoute("Bar Resource - Edit", "bar-resource/{id}", new { controller = "Home", action = "Edit" });
            context.MapRoute("Bar Resource - Edit Pictures", "bar-resource/{id}/pictures", new { controller = "Home", action = "EditPictures" });
            context.MapRoute("Bar Resource - Edit Open Hours", "bar-resource/{id}/open-hours", new { controller = "Home", action = "EditOpenHours" });
            context.MapRoute("Bar Resource - Edit Menus", "bar-resource/{id}/menus", new { controller = "Home", action = "EditMenus" });
            context.MapRoute("Bar Resource - Edit Contact Details", "bar-resource/{id}/contact-details", new { controller = "Home", action = "EditContactDetails" });
            context.MapRoute("Bar Resource - Edit Type", "bar-resource/{id}/type", new { controller = "Home", action = "EditType" });
            context.MapRoute("Bar Resource - Edit Features", "bar-resource/{id}/features", new { controller = "Home", action = "EditFeatures" });

            context.MapRoute("Bar Resource - Edit Signature Cocktail", "bar-resource/{id}/signature-cocktail", new { controller = "Home", action = "EditSignatureCocktail" });
            context.MapRoute("Bar Resource - Accreditations", "bar-resource/{id}/accreditations", new { controller = "Home", action = "Accreditations" });
        }
    }
}
