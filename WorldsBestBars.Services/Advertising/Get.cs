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
    [DestinationUrl],
    [TargetCountries],
    (select sum([Clicks]) from AdvertStats where AdvertId = Advert.Id) as [TotalClicks],
    (select sum([Impressions]) from AdvertStats where AdvertId = Advert.Id) as [TotalImpressions]
from
    Advert
where
    Id = @id";

        const string SqlStats = @"select
    top 24
    cast(Period as date) as [Period],
    sum(Clicks) as [Clicks],
    sum(Impressions) as [Impressions]
from
    AdvertStats
where
    AdvertId = @id
group by
    cast(Period as date)
order by 
    cast(Period as date) desc";

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
