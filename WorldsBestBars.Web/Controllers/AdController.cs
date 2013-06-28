using System;
using System.Web.Mvc;

using WorldsBestBars.Logic;

namespace WorldsBestBars.Web.Controllers
{
    public class AdController : Controller
    {
        static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(AdController));

        public void Out(Guid id)
        {
            try
            {
                var ad = Cache.Adverts.Instance.GetById(id);

                Logic.Adverts.RecordClick(id);
                if (ad != null)
                {
                    Response.Redirect(ad.Destination);
                }
                else
                {
                    Response.Redirect("~/");
                }
            }
            catch(Exception ex)
            {
                Log.Error("Could not redirect user to advert destination for ad '" + id.ToString() + "'", ex);

                Response.Redirect("~/");
            }
        }
    }
}
