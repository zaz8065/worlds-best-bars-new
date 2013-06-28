using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace WorldsBestBars.Services.Models
{
    public class Document : NamedEntity
    {
        public string UrlKey { get; set; }
        public string Url { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? RedirectId { get; set; }

        public string Synopsis { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public IEnumerable<string> Images { get; set; }
        public IEnumerable<NamedEntity> Categories { get; set; }

        internal static Document FromReader(SqlDataReader reader)
        {
            return new Document
            {
                Id = (Guid)reader["Id"],
                UrlKey = reader["UrlKey"] == DBNull.Value ? null : (string)reader["UrlKey"],
                Url = reader["Url"] == DBNull.Value ? null : (string)reader["Url"],
                Name = reader["Name"] == DBNull.Value ? null : (string)reader["Name"],
                ParentId = reader["ParentId"] == DBNull.Value ? null : (Guid?)reader["ParentId"],
                Synopsis = (string)reader["Synopsis"],
                Content = (string)reader["Content"],
                IsActive = (bool)reader["IsActive"],
                Created = (DateTime)reader["Created"],
                Modified = (DateTime)reader["Modified"],
            };
        }
    }
}
