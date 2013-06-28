using System;
using System.Data.SqlClient;

namespace WorldsBestBars.Services.Models
{
    public class Location : NamedEntity
    {
        public string UrlKey { get; set; }
        public Guid? ParentId { get; set; }
        public int ChildLocationCount { get; set; }
        public int ChildBarCount { get; set; }

        internal static Location FromReader(SqlDataReader reader)
        {
            return new Location
            {
                Id = (Guid)reader["Id"],
                UrlKey = reader["UrlKey"] == DBNull.Value ? null : (string)reader["UrlKey"],
                Name = reader["Name"] == DBNull.Value ? null : (string)reader["Name"],
                ParentId = reader["ParentId"] == DBNull.Value ? null : (Guid?)reader["ParentId"],
                ChildLocationCount = (int)reader["ChildLocationCount"],
                ChildBarCount = (int)reader["ChildBarCount"]
            };
        }
    }
}
