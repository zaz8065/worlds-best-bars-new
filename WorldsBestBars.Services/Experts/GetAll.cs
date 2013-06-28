using Dapper;
using System.Collections.Generic;
using System.Data;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Experts
{
    public class GetAll : BaseService
    {
        #region Constants

        const string SqlCount = @"select @count = count(*) from [User] where ((@filter is null) or (@filter is not null and ([Name] like '%' + @filter + '%' or [Email] like '%' + @filter + '%'))) and IsExpert = 1";
        const string Sql = @"
with cte as ( 
    select 
        Id, 
        Created, 
        Name,
        row_number() over (order by [{0}] {1}) as __RN 
    from 
        [User] 
    where 
        ((@filter is null) or (@filter is not null and ([Name] like '%' + @filter + '%' or [Email] like '%' + @filter + '%'))) 
        and IsExpert = 1
) 
select 
    Id,
    Created,
    Name
from
    cte
where
    __RN between @skip and (@skip + @take)";

        #endregion

        #region Public Methods

        public IEnumerable<ExpertSummary> Execute(out int total, int skip, int take, string filter, string sort, string sortd)
        {
            total = 0;

            switch ((sort ?? string.Empty).ToLower())
            {
                case "email":
                case "created":
                case "isadmin":
                case "city":
                    break;
                default:
                    sort = "name";
                    break;
            }

            switch ((sortd ?? string.Empty).ToLower())
            {
                case "asc":
                case "desc":
                    break;
                default:
                    sortd = "asc";
                    break;
            }

            using (var connection = GetConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@count", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@filter", filter);
                parameters.Add("@skip", skip);
                parameters.Add("@take", take);

                connection.Execute(SqlCount, parameters);

                total = parameters.Get<int>("@count");

                return connection.Query<ExpertSummary>(string.Format(Sql, sort, sortd), parameters);
            }
        }

        #endregion
    }
}
