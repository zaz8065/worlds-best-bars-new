using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
 


namespace WorldsBestBars.Web
{
    public partial class Rss : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            List<Bar> bars = null;

            string sOffset = Request.QueryString["offset"];
            string sLimit = Request.QueryString["limit"];
            if (!String.IsNullOrEmpty(sOffset) && !String.IsNullOrEmpty(sLimit))
            {
                //  FIltering parameters are present so select subset
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


        }
    }

    public class BusinessLayer
    {
        /// <summary>
        /// Create the image url
        /// </summary>
        /// <param name="RelativeUrl"></param>
        /// <returns></returns>
        private static string GetImageUrl(string RelativeUrl)
        {

            try
            {
                string sServerPathBase = "http://www.worldsbestbars.com/content/media/" + RelativeUrl;// +"/245x165.orig.01.jpg";
                string sPath = @"C:\websites\com.worldsbestbars\#shared\media\" + RelativeUrl.Replace("/", "\\");

                DirectoryInfo dir = new DirectoryInfo(sPath);

                List<string> fileList = Directory.GetFiles(sPath, "original.*", System.IO.SearchOption.TopDirectoryOnly).ToList<string>();

                if (fileList.Count > 0)
                    return "http://www.worldsbestbars.com/content/media/" + RelativeUrl + "/" + Path.GetFileName(fileList[0]);


            }
            catch (Exception ex)
            {

             //   File.AppendAllText("Error.log", DateTime.Now.ToString() + " : " + ex.Message);

            }
            return "";
        }



        /// <summary>
        /// Generate the RSS XML
        /// </summary>
        public static XDocument CreateRSS(List<Bar> list)
        {
            // Create XML
            XNamespace nsGeoRSS = "http://www.georss.org/georss";
            XNamespace nsFieldTrip = "http://www.fieldtripper.com/fieldtrip_rss";

           // List<Bar> list = Bar.GetAllBar();
            XDocument doc =
         new XDocument(
         new XElement("rss", new XAttribute("version", "2.0"),
         new XAttribute(XNamespace.Xmlns + "georss", "http://www.georss.org/georss"),
           new XAttribute(XNamespace.Xmlns + "fieldtrip", "http://www.fieldtripper.com/fieldtrip_rss"),
         new XElement("feed",
         new XElement("title", "Worlds Best Bars"),
         new XElement("link", "http://www.worldsbestbars.com"),
         new XElement("Description", "WorldsBestBars.com is much more than a bar directory, it's a site with an ongoing mission to locate the finest bars the planet has to offer and boasts reviews of over 2,000 of the world's most prestigious places to drink and be seen in."),
         new XElement("image",
         new XElement("url", "http://www.worldsbestbars.com/content/img/logos/wbb_logo_rss.jpg")),
         new XElement("pubDate", DateTime.Today.ToString("R")),
         new XElement("lastBuildDate", "Worlds Best Bars"),
         new XElement("managingEditor", "noreply@worldsbestbars.com"),
         new XElement("language", "en"),

         list.Select(x => new XElement("item",
             new XElement("title", x.Name),
             new XElement(nsGeoRSS + "point", ReformatCoordString(x.GeocoordinateString)),
             new XElement(nsFieldTrip + "address", CreateAddressString(x.Address)),
             new XElement("guid", x.Id),
             new XElement("description", x.Description),
             new XElement("link", "http://www.worldsbestbars.com/" + x.Url),
             new XElement(nsFieldTrip + "image",
             new XElement("url", GetImageUrl(x.Url)),
             new XElement("title", "Image of " + x.Name),
             new XElement("attribution", "Source www.worldsbestbars.com")),
             new XElement("pubDate", DateTime.Today.ToString("R")),
             new XElement("startDate", DateTime.Today.ToString("R"))
          )
         )
                //  new XElement("endDate",DateTime.Today.ToString("R")),
             ))

        );

            return doc;
        }

        private static string CreateAddressString(string p)
        {
            // Parse xml and return string
            XDocument doc = XDocument.Parse(p);
            string sAddress = "";

            foreach (XElement x in doc.Root.Descendants())
            {
                if (!String.IsNullOrEmpty(x.Value))
                    sAddress += x.Value + ", ";
            }
            return sAddress.TrimEnd(',', ' ');
        }

        private static string ReformatCoordString(string p)
        {
            // remove unneeded formatting
            string sNew = p.Substring(7);
            sNew = sNew.Substring(0, sNew.IndexOf(")"));

            // switch lat/long order
            string[] saCoords = sNew.Split(' ');

            string s = saCoords[1] + " " + saCoords[0];
            return s;
        }
    }

    public partial class Bar
    {
        #region Fields

        private Guid id; //
        private Guid locationId; //
        private string name; //
        private string intro; //
        private string description; //
        private string website; //
        private string email; //
        private string phone; //
        private string fax; //
        private string address; //
        private string openHours; //
        private int legacyId; //
        private string urlKey; //
        private bool isActive; //
        //private SqlGeography geocoordinate; //
        private string geocoordinateString; //
        private DateTime created; //
        private DateTime modified; //
        private string url; //
        private string menuUrl; //



        #endregion

        #region Properties


        /// <summary>
        ///
        /// </summary>
        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public Guid LocationId
        {
            get { return locationId; }
            set { locationId = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string Intro
        {
            get { return intro; }
            set { intro = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string Website
        {
            get { return website; }
            set { website = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string Email
        {
            get { return email; }
            set { email = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string Phone
        {
            get { return phone; }
            set { phone = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string Fax
        {
            get { return fax; }
            set { fax = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string Address
        {
            get { return address; }
            set { address = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string OpenHours
        {
            get { return openHours; }
            set { openHours = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public int LegacyId
        {
            get { return legacyId; }
            set { legacyId = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string UrlKey
        {
            get { return urlKey; }
            set { urlKey = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }
 
        /// <summary>
        ///
        /// </summary>
        public string GeocoordinateString
        {
            get { return geocoordinateString; }
            set { geocoordinateString = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public DateTime Created
        {
            get { return created; }
            set { created = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public DateTime Modified
        {
            get { return modified; }
            set { modified = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        /// <summary>
        ///
        /// </summary>
        public string MenuUrl
        {
            get { return menuUrl; }
            set { menuUrl = value; }
        }





        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>

        public Bar()
        {
            // Empty Constructor
        }

        /// <summary>
        /// Instanciates a Bar object from the database using the Id
        /// </summary>

    
        public static List<Bar> GetAllBar()
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["worldsbestbars_db"].ConnectionString))
            using (var cmd = new SqlCommand("SELECT * FROM Bar", conn)
            {
                // CommandType = CommandType.Text;
            })
            {
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                // Create the object array from the datareader
                return PopulateObject(dr);
            }
        }


        /// <summary>
        /// Takes an prepopulated SqlDataReader and creates an array of Bars
        /// </summary>

        public static List<Bar> PopulateObject(SqlDataReader dr)
        {
            List<Bar> arr = new List<Bar>();
            Bar obj;

            while (dr.Read())
            {
                try
                {
                    obj = new Bar();
                    obj.id = (Guid)dr["Id"];
                    obj.locationId = (Guid)dr["LocationId"];
                    obj.name = (string)dr["Name"] as string;
                    obj.intro = (string)dr["Intro"] as string;
                    obj.description = (string)dr["Description"] as string;
                    obj.website = (string)dr["Website"] as string;
                    obj.email = Convert.ToString(dr["Email"] as string) ?? "";
                    obj.phone = Convert.ToString(dr["Phone"] as string) ?? "";
                    obj.fax = Convert.ToString(dr["Fax"] as string) ?? "";
                    obj.address = (string)dr["Address"] as string;
                    obj.openHours = (string)dr["OpenHours"] as string;
                    obj.legacyId = (int)dr["LegacyId"];
                    obj.urlKey = (string)dr["UrlKey"] as string;
                    obj.isActive = (bool)dr["IsActive"];
                  
                    obj.geocoordinateString = (string)dr["GeocoordinateString"];
                    obj.created = (DateTime)dr["Created"];
                    obj.modified = (DateTime)dr["Modified"];
                    obj.url = (string)dr["Url"];
                    obj.menuUrl = Convert.ToString(dr["MenuUrl"] as string) ?? "";

                    arr.Add(obj);
                }
                catch (Exception ex)
                {
                    // Log
                    Console.WriteLine(ex.Message);
                }
            }

            dr.Close();


            return arr;
        }




    }
}