using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using WorldsBestBars.Services.Models;
using System.Linq;

namespace WorldsBestBars.Services.Experts
{
    public class Get : BaseService
    {
        #region Constants

        const string Sql = @"select
    top 1
    U.Id,
    U.UrlKey,
    U.Name,
    U.Title,
    U.Biography,
    U.Website,
    U.IsActive
from
    [User] U
where
    U.Id = @id
    and U.IsExpert = 1";

        const string SqlComments = "select R.*, B.Name as BarName from Review R inner join Bar B on (R.BarId = B.Id) where UserId = @id order by Created desc";

        #endregion

        #region Public Methods

        public ExpertDetail Execute(Guid id)
        {
            using (var connection = GetConnection())
            {
                var result = connection.Query<ExpertDetail>(Sql, new { id = id }).FirstOrDefault();

                if (result != null)
                {
                    result.Reviews = connection.Query<Review>(SqlComments, new { id = id });
                    result.Images = Services.Shared.GetImages(result.Url);
                }

                return result;
            }
        }

        #endregion
    }
}
