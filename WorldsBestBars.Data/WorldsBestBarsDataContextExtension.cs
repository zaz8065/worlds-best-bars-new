using System.Configuration;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;
using System.Linq;

namespace WorldsBestBars.Data
{
    partial class Bar
    {
        public SqlGeography Geocoordinate
        {
            get
            {
                if (GeocoordinateString != null)
                {
                    return SqlGeography.STGeomFromText(new SqlChars(GeocoordinateString), 4326);
                }

                return null;
            }
        }

        [System.Runtime.Serialization.DataMember]
        public System.Xml.Linq.XElement AddressXml
        {
            get { return System.Xml.Linq.XElement.Parse(this.Address); }
            set { this.Address = value.ToString(); }
        }

        [System.Runtime.Serialization.DataMember]
        public System.Xml.Linq.XElement OpenHoursXml
        {
            get { return System.Xml.Linq.XElement.Parse(this.OpenHours); }
            set { this.OpenHours = value.ToString(); }
        }
    }
    
    partial class Location
    {
        public SqlGeography Geocoordinate
        {
            get
            {
                if (GeocoordinateString != null)
                {
                    return SqlGeography.STGeomFromText(new SqlChars(GeocoordinateString), 4326);
                }

                return null;
            }
        }
    }
}