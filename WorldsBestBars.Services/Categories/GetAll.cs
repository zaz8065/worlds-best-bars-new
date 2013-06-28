using Dapper;
using System.Collections.Generic;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Categories
{
    public class GetAll : BaseService
    {
        #region Constants

        const string Sql = @"select
    C.Id,
    C.Name,
    dbo.GetCategoryEntityCount(C.Id) as EntityCount
from 
    Category C
where
    (@filter is null or C.Name like '%' + @filter + '%')
order by
    C.Name";

        #endregion

        #region Public Methods

        public IEnumerable<CategorySummary> Execute(string filter = null)
        {
            using (var connection = GetConnection())
            {
                return connection.Query<CategorySummary>(Sql, new { filter = filter });
            }
        }

        #endregion
    }
}
