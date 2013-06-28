using System.Web.Mvc;
using System.Web.Routing;

namespace WorldsBestBars.Web.Admin
{
    public class RouteConfig
    {
        #region Public Methods

        /// <summary>
        /// Registers the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Home", string.Empty, new { controller = "Home", action = "Index" });

            #region Bars

            routes.MapRoute("Bars - Search - Json", "bars/search/json", new { controller = "Bars", action = "JsonSearch" });
            
            routes.MapRoute("Bars - Active", "bars", new { controller = "Bars", action = "Active" });
            routes.MapRoute("Bars - Closed", "bars/closed", new { controller = "Bars", action = "Closed" });

            routes.MapRoute("Bars - Submissions - Create", "bars/submissions/create", new { controller = "Bars", action = "CreateSubmissions" });
            routes.MapRoute("Bars - Submission - Purge", "bars/submissions/{id}/purge", new { controller = "Bars", action = "PurgeSubmission" });
            routes.MapRoute("Bars - Submissions - Updates - Detail", "bars/submissions/update/{id}", new { controller = "Bars", action = "UpdateSubmission" });
            routes.MapRoute("Bars - Submissions - Updates", "bars/submissions/update", new { controller = "Bars", action = "UpdateSubmissions" });

            routes.MapRoute("Bars - Map", "bars/map", new { controller = "Bars", action = "Map" });
            routes.MapRoute("Bars - Create", "bar/create", new { controller = "Bars", action = "Create" });
            routes.MapRoute("Bars - Update", "bar/{id}/update", new { controller = "Bars", action = "Update" });
            routes.MapRoute("Bars - Single", "bar/{id}", new { controller = "Bars", action = "Single" });
            routes.MapRoute("Bars - Activate", "bars/{id}/activate/{isActive}", new { controller = "Bars", action = "ToggleActive" });

            routes.MapRoute("Bars - Add to Category", "bar/{id}/add-to-category", new { controller = "Bars", action = "AddToCategory" });

            #endregion

            #region Locations

            routes.MapRoute("Locations - Search - Json", "locations/search/json", new { controller = "Locations", action = "JsonSearch" });

            routes.MapRoute("Locations - Create", "locations/create", new { controller = "Locations", action = "Create" });
            routes.MapRoute("Locations", "locations/{id}", new { controller = "Locations", action = "Index", id = UrlParameter.Optional });
            routes.MapRoute("Locations - Update", "locations/{id}/update", new { controller = "Locations", action = "Update" });
            routes.MapRoute("Locations - Delete", "locations/{id}/delete", new { controller = "Locations", action = "Delete" });

            #endregion

            #region Advertising

            routes.MapRoute("Advertising - Active", "advertising/active", new { controller = "Advertising", action = "Active" });
            routes.MapRoute("Advertising - Inactive", "advertising/inactive", new { controller = "Advertising", action = "Inactive" });
            routes.MapRoute("Advertising - Create", "advertising/create", new { controller = "Advertising", action = "Create" });
            routes.MapRoute("Advertising - Single", "advertising/{id}", new { controller = "Advertising", action = "Single" });
            routes.MapRoute("Advertising - Update", "advertising/{id}/update", new { controller = "Advertising", action = "Update" });
            routes.MapRoute("Advertising - Activate", "advertising/{id}/activate", new { controller = "Advertising", action = "ToggleActive", isActive = true });
            routes.MapRoute("Advertising - Deactivate", "advertising/{id}/deactivate", new { controller = "Advertising", action = "ToggleActive", isActive = false });

            #endregion

            #region Users

            routes.MapRoute("Users - Home", "users", new { controller = "Users", action = "Index" });
            routes.MapRoute("Users - Export", "users/export", new { controller = "Users", action = "Export" });

            #endregion

            #region Reviews

            routes.MapRoute("Reviews - Pending Moderation", "reviews/pending-moderation", new { controller = "Reviews", action = "PendingModeration" });
            routes.MapRoute("Reviews - Latest", "reviews", new { controller = "Reviews", action = "Latest" });
            routes.MapRoute("Reviews - Moderate", "reviews/{id}/moderate/{isActive}", new { controller = "Reviews", action = "Moderate" });

            #endregion

            #region Documents

            routes.MapRoute("Documents - Drafts", "documents/drafts", new { controller = "Documents", action = "Drafts" });
            routes.MapRoute("Documents - Active", "documents", new { controller = "Documents", action = "Active" });
            routes.MapRoute("Documents - Create", "document/create", new { controller = "Documents", action = "Create" });
            routes.MapRoute("Documents - Update", "document/{id}", new { controller = "Documents", action = "Update" });
            routes.MapRoute("Documents - Delete", "document/{id}/delete", new { controller = "Documents", action = "Delete" });
            routes.MapRoute("Documents - Activate", "document/{id}/activate/{isActive}", new { controller = "Documents", action = "Activate" });

            routes.MapRoute("Documents - Add to Category", "document/{id}/add-to-category", new { controller = "Documents", action = "AddToCategory" });

            #endregion

            #region Experts

            routes.MapRoute("Experts - Home", "experts", new { controller = "Experts", action = "Index" });
            routes.MapRoute("Experts - Create", "experts/create", new { controller = "Experts", action = "Create" });
            routes.MapRoute("Experts - Update", "experts/{id}/update", new { controller = "Experts", action = "Update" });
            routes.MapRoute("Experts - Delete", "experts/{id}/delete", new { controller = "Experts", action = "Delete" });
            routes.MapRoute("Experts - Single", "experts/{id}", new { controller = "Experts", action = "Single" });

            routes.MapRoute("Experts - Utility - Create Review", "experts/{id}/_/create-review", new { controller = "Experts", action = "CreateReview" });
            routes.MapRoute("Experts - Utility - Delete Review", "experts/{id}/delete-review/{review}", new { controller = "Experts", action = "DeleteReview" });
            routes.MapRoute("Experts - Utility - Activate", "experts/{id}/activate", new { controller = "Experts", action = "ToggleActive", isActive = true });
            routes.MapRoute("Experts - Utility - Deactivate", "experts/{id}/deactivate", new { controller = "Experts", action = "ToggleActive", isActive = false });

            #endregion

            #region Lists

            routes.MapRoute("Lists - Search - Json", "lists/search/json", new { controller = "Lists", action = "JsonSearch" });

            routes.MapRoute("Lists - Home", "lists", new { controller = "Lists", action = "Index" });
            routes.MapRoute("Lists - Create", "lists/create", new { controller = "Lists", action = "Create" });
            routes.MapRoute("Lists - Edit", "lists/{id}/update", new { controller = "Lists", action = "Update" });
            routes.MapRoute("Lists - Single", "lists/{id}", new { controller = "Lists", action = "Single" });
            
            routes.MapRoute("Lists - Utility - Remove", "lists/{id}/remove/{entity}/{type}", new { controller = "Lists", action = "Remove" });

            #endregion

            #region Authentication

            routes.MapRoute("Authentication - Login", "login", new { controller = "Authentication", action = "Login" });
            routes.MapRoute("Authentication - Logout", "logout", new { controller = "Authentication", action = "Logout" });

            #endregion
        }

        #endregion
    }
}