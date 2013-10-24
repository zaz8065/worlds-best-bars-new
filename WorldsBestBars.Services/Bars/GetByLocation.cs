using Dapper;
using System;
using System.Collections.Generic;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Bars
{
    public class GetByLocation : BaseService
    {
        #region Constants

        const string Sql = @"select 
    Id, 
    UrlKey, 
    Name,
    row_number() over (order by [Name]) as __RN
from 
    Bar
where
    LocationId = @location
    and ((@activeOnly and IsActive = 1) or not @activeOnly)";

        #endregion

        #region Public Methods

        public IEnumerable<BarSummary> Execute(Guid location, bool activeOnly = false)
        {
            using (var connection = GetConnection())
            {
                return connection.Query<BarSummary>(Sql, new
                {
                    location = location,
                    activeOnly = activeOnly
                });
            }
        }

        #endregion
    }
}
