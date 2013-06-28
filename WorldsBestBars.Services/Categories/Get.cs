using Dapper;
using System;
using System.Linq;
using System.Collections.Generic;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Categories
{
    public class Get : BaseService
    {
        #region Constants

        const string Sql = @"select
    C.Id,
    C.Name,
    C.[Key],
    C.[Group]
from 
    Category C
where
    C.Id = @id";

        const string SqlBars = @"select
    B.Id,
    B.Name
from
    BarCategory BC
    inner join Bar B on (BC.BarId = B.Id)
where
    BC.CategoryId = @id
order by 
    B.Name";

        const string SqlLocations = @"select
    L.Id,
    L.Name
from
    LocationCategory LC
    inner join Location L on (LC.LocationId = L.Id)
where
    LC.CategoryId = @id
order by 
    L.Name;";

        const string SqlDocuments = @"select
    D.Id,
    D.Name
from
    DocumentCategory DC
    inner join Document D on (DC.DocumentId = D.Id)
where
    DC.CategoryId = @id
order by 
    D.Name;";

        #endregion

        #region Public Methods

        public CategoryDetail Execute(Guid id)
        {
            using (var connection = GetConnection())
            {
                var result = connection.Query<CategoryDetail>(Sql, new { id = id }).FirstOrDefault();

                if (result != null)
                {
                    result.Bars = connection.Query<NamedEntity>(SqlBars, new { id = id });
                    result.Locations = connection.Query<NamedEntity>(SqlLocations, new { id = id });
                    result.Documents = connection.Query<NamedEntity>(SqlDocuments, new { id = id });
                }

                return result;
            }
        }

        #endregion
    }
}
