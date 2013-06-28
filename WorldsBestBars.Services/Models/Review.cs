using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldsBestBars.Services.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid BarId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ParentId { get; set; }

        public string BarName { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public bool IsModerated { get; set; }
        public string UserIP { get; set; }

        public Rating? Rating { get; set; }

        internal static Review FromReader(SqlDataReader reader)
        {
            return new Review
            {
                Id = (Guid)reader["Id"],
                BarId = (Guid)reader["BarId"],
                BarName = (string)reader["BarName"],
                UserId = reader["UserId"] == DBNull.Value ? null : (Guid?)reader["UserId"],
                UserName = reader["UserName"] == DBNull.Value ? null : (string)reader["UserName"],
                ParentId = reader["ParentId"] == DBNull.Value ? null : (Guid?)reader["ParentId"],
                Title = reader["Title"] == DBNull.Value ? null : (string)reader["Title"],
                Comment = (string)reader["Comment"],
                IsActive = (bool)reader["IsActive"],
                Created = (DateTime)reader["Created"],
                IsModerated = (bool)reader["IsModerated"],
                UserIP = reader["UserIP"] == DBNull.Value ? null : (string)reader["UserIP"],
                Rating = reader["RatingId"] == DBNull.Value ? null : (Rating?)Models.Rating.FromReader(reader)
            };
        }
    }

    public struct Rating
    {
        public Guid Id { get; set; }
        public byte Design { get; set; }
        public byte Drinks { get; set; }
        public byte Service { get; set; }
        public byte Overall { get; set; }


        internal static Rating FromReader(SqlDataReader reader)
        {
            return new Rating
            {
                Id = (Guid)reader["RatingId"],
                Design = (byte)reader["Design"],
                Drinks = (byte)reader["Drinks"],
                Service = (byte)reader["Service"],
                Overall = (byte)(int)reader["Overall"]
            };
        }
    }
}
