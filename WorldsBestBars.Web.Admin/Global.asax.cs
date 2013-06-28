using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using WorldsBestBars.Web.Admin.Security;

namespace WorldsBestBars.Web.Admin
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            AutofacConfig.Configure();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders[typeof(Services.Models.Geo)] = new GeoModelBinder();
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie == null)
            {
                return;
            }

            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);

            Context.User = MemberPrincipal.Get(Guid.Parse(authTicket.Name));
        }
    }

    class GeoModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue;
            
            return Services.Models.Geo.FromString(value);
        }
    }
}