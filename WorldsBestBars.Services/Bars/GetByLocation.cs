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
    LocationId = @location";

        #endregion

        #region Public Methods

        public IEnumerable<BarSummary> Execute(Guid location)
        {
            using (var connection = GetConnection())
            {
                return connection.Query<BarSummary>(Sql, new
                {
                    location = location
                });
            }
        }

        #endregion
    }
}
