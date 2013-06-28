using Dapper;
using System.Collections.Generic;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Advertising
{
    public class GetAll : BaseService
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
    IsActive = @status
    and ((@status = 1 and IsActive = 1 and (([Start] is null) or ([Start] is not null and [Start] < getdate())) and (([Finish] is null) or ([Finish] is not null and [Finish] > getdate()))) or (@status = 0 and IsActive = 0))";

        #endregion

        #region Public Methods

        public IEnumerable<Advert> Execute(AdvertStatus status = AdvertStatus.Active)
        {
            using (var connection = GetConnection())
            {
                return connection.Query<Advert>(Sql, new { status = status });
            }
        }

        #endregion
    }
}
