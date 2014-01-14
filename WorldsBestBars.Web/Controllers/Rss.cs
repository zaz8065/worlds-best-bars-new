using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer;

namespace WorldsBestBars.Web.Controllers
{
    public class Rss : Controller
    {
        //
        // GET: /Rss/

        public ActionResult Index()
        {

            context.HttpContext.Response.ContentType = "application/rss+xml";

           

            List<Bar> bars = null;

            string sOffset = Request.QueryString["offset"];
            string sLimit = Request.QueryString["limit"];
            if (!String.IsNullOrEmpty(sOffset) && !String.IsNullOrEmpty(sLimit))
            {
                //  Filtering parameters are present so select subset
                int iOffset = 0;
                int iLimit = 0;

                if (Int32.TryParse(sOffset, out iOffset) && Int32.TryParse(sLimit, out iLimit))
                {
                    bars = Bar.GetAllBar().OrderByDescending(bar => bar.Modified).Skip(iOffset).Take(iLimit).ToList<Bar>();
                }
            }
            else
            {
                // No limits specified so just get all bars
                bars = Bar.GetAllBar();
            }

            // Create XML
            XDocument x = BusinessLayer.CreateRSS(bars);

            // Return XML as response
            string sHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine;
            Response.Clear();
            Response.Write(sHeader + x.ToString());


         //   return View();
        }

    }
}
