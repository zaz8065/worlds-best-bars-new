using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services
{
    public class ReviewService : BaseService
    {
        #region Public Methods

        /// <summary>
        /// Gets the pending moderation.
        /// </summary>
        /// <param name="total">The total.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        public IEnumerable<Review> GetPendingModeration(out int total, int skip, int take)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select @count = count(*) from Review where IsModerated = 0";
                    command.Parameters.Add(new SqlParameter("@count", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    command.ExecuteNonQuery();
                    total = (int)command.Parameters["@count"].Value;
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"with cte as ( 
select 
    RE.Id, 
    RE.BarId,
    B.Name as [BarName],
    RE.UserId, 
    U.Name as [UserName],
    RE.ParentId, 
    RE.Title, 
    RE.Comment, 
    RE.IsActive, 
    RE.Created, 
    RE.IsModerated, 
    RE.UserIP, 
    RE.RatingId, 
    RA.Design, 
    RA.Drinks, 
    RA.Service, 
    RA.Overall,
    row_number() over (order by RE.Created desc) as __RN
from 
    Review RE 
    left join Rating RA on (RE.RatingId = RA.Id) 
    inner join Bar B on (RE.BarId = B.Id)
    left join [User] U on (RE.UserId = U.Id)
where 
    RE.IsModerated = 0 )

select     
    Id, 
    BarId, 
    BarName,
    UserId, 
    UserName,
    ParentId, 
    Title, 
    Comment, 
    IsActive, 
    Created, 
    IsModerated, 
    UserIP, 
    RatingId, 
    Design, 
    Drinks, 
    Service, 
    Overall
from 
    cte 
where
    __RN between @skip and (@skip + @take)";

                    command.Parameters.AddWithValue("@skip", skip);
                    command.Parameters.AddWithValue("@take", take);

                    using (var reader = command.ExecuteReader())
                    {
                        var result = new List<Review>();
                        while (reader.Read())
                        {
                            result.Add(Review.FromReader(reader));
                        }

                        return result.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="total">The total.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        public IEnumerable<Review> GetAll(out int total, int skip, int take)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select @count = count(*) from Review";
                    command.Parameters.Add(new SqlParameter("@count", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    command.ExecuteNonQuery();
                    total = (int)command.Parameters["@count"].Value;
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"with cte as ( 
select 
    RE.Id, 
    RE.BarId, 
    B.Name as [BarName],
    RE.UserId, 
    U.Name as [UserName],
    RE.ParentId, 
    RE.Title, 
    RE.Comment, 
    RE.IsActive, 
    RE.Created, 
    RE.IsModerated, 
    RE.UserIP, 
    RE.RatingId, 
    RA.Design, 
    RA.Drinks, 
    RA.Service, 
    RA.Overall,
    row_number() over (order by RE.Created desc) as __RN
from 
    Review RE 
    left join Rating RA on (RE.RatingId = RA.Id)
    inner join Bar B on (RE.BarId = B.Id)
    left join [User] U on (RE.UserId = U.Id) )

select     
    Id, 
    BarId, 
    BarName,
    UserId, 
    UserName,
    ParentId, 
    Title, 
    Comment, 
    IsActive, 
    Created, 
    IsModerated, 
    UserIP, 
    RatingId, 
    Design, 
    Drinks, 
    Service, 
    Overall
from 
    cte 
where
    __RN between @skip and (@skip + @take)";

                    command.Parameters.AddWithValue("@skip", skip);
                    command.Parameters.AddWithValue("@take", take);

                    using (var reader = command.ExecuteReader())
                    {
                        var result = new List<Review>();
                        while (reader.Read())
                        {
                            result.Add(Review.FromReader(reader));
                        }

                        return result.ToArray();
                    }
                }
            }
        }

        public void Moderate(Guid id, bool isActive)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "update Review set IsActive = @isActive, IsModerated = 1 where Id = @id";

                    command.Parameters.AddWithValue("@isActive", isActive);
                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}
