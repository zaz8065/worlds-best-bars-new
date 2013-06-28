using Dapper;
using System.Collections.Generic;
using System.Data;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Bars
{
    public class GetAll : BaseService
    {
        #region Constants

        const string SqlCount = @"select @count = count(*) from Bar where IsActive = @isActive and (@filter is null or Name like '%' + @filter + '%')";
        const string Sql = @"
with cte as 
( 
    select 
        Id, 
        UrlKey, 
        Name,
        Created,
        (select count(*) from Review where BarId = Bar.Id) as [Reviews],
        row_number() over (order by [Name]) as __RN
    from 
        Bar
    where
        IsActive = @isActive
        and (@filter is null or Name like '%' + @filter + '%')
)

select
    Id,
    UrlKey,
    Name,
    Created,
    Reviews
from
    cte
where
    __RN between @skip and (@skip + @take)";

        #endregion

        #region Public Methods

        public IEnumerable<BarSummary> Execute(out int total, BarStatus status, int skip, int take, string filter)
        {
            total = 0;

            using (var connection = GetConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@count", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@isActive", status == BarStatus.Active);
                parameters.Add("@filter", filter);
                parameters.Add("@skip", skip);
                parameters.Add("@take", take);

                connection.Execute(SqlCount, parameters);

                total = parameters.Get<int>("@count");

                return connection.Query<BarSummary>(Sql, parameters);
            }
        }

        #endregion
    }
}
