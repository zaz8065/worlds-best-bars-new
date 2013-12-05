 using System; 
using System.Collections.Generic; 
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;

namespace WorldsBestBars.Process.RssXmlGen
{
                
		      
	/// <summary>
	/// Represents a Bar in the system
	/// </summary>

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
        private SqlGeography geocoordinate; //
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
        public SqlGeography Geocoordinate
        {
            get { return geocoordinate; }
            set { geocoordinate = value; }
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


        public static List<Bar> GetAllBar()
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
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
                    obj.name = (string)dr["Name"];
                    obj.intro = (string)dr["Intro"];
                    obj.description = (string)dr["Description"];
                    obj.website = (string)dr["Website"];
                    obj.email = (string)dr["Email"];
                    obj.phone = (string)dr["Phone"];
                    obj.fax = (string)dr["Fax"];
                    obj.address = (string)dr["Address"];
                    obj.openHours = (string)dr["OpenHours"];
                    obj.legacyId = (int)dr["LegacyId"];
                    obj.urlKey = (string)dr["UrlKey"];
                    obj.isActive = (bool)dr["IsActive"];
                    obj.geocoordinate = (SqlGeography)dr["Geocoordinate"];
                    obj.geocoordinateString = (string)dr["GeocoordinateString"];
                    obj.created = (DateTime)dr["Created"];
                    obj.modified = (DateTime)dr["Modified"];
                    obj.url = (string)dr["Url"];
                    obj.menuUrl = (string)dr["MenuUrl"];

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
