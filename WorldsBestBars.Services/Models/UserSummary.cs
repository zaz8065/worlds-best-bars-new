using System;
using System.Data.SqlClient;

namespace WorldsBestBars.Services.Models
{
    public class UserSummary : NamedEntity
    {
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime Created { get; set; }
        public string City { get; set; }

        internal static UserSummary FromReader(SqlDataReader reader)
        {
            return new UserSummary
            {
                Id = (Guid)reader["Id"],
                Name = reader["Name"] as string,
                Email = reader["Email"] as string,
                IsAdmin = (bool)reader["IsAdmin"],
                Created = (DateTime)reader["Created"],
                City = reader["City"] == DBNull.Value ? null : (string)reader["City"]
            };
        }
    }
}
