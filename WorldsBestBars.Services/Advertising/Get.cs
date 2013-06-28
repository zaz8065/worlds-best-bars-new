using Dapper;
using System;
using System.Linq;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Advertising
{
    public class Get : BaseService
    {
        #region Constants

        const string Sql = @"select
    [Id],
    [Title],
    [SkyscraperUrl],
    [Start],
    [Finish],
    [IsActive],
    [Weight],
    [Type],
    [DestinationUrl]
from
    Advert
where
    Id = @id";

        const string SqlStats = @"select
    top 48
    Period,
    Clicks,
    Impressions
from
    AdvertStats
where
    AdvertId = @id
order by 
    Period desc";

        #endregion

        #region Public Methods

        public Advert Execute(Guid id)
        {
            using (var connection = GetConnection())
            {
                var result = connection.Query<Advert>(Sql, new { id = id }).FirstOrDefault();

                if (result != null)
                {
                    result.Stats = connection.Query<Advert.Statistic>(SqlStats, new { id = id });
                }

                return result;
            }
        }

        #endregion
    }
}
