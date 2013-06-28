using Dapper;
using System;
using System.Linq;
using System.Collections.Generic;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Categories
{
    public class Add : BaseService
    {
        #region Constants

        const string Sql = @"insert into {0}Category ( {0}Id, CategoryId ) values ( @entity, @category )";

        #endregion

        #region Public Methods

        public void Execute(Guid category, Guid entity, EntityType type)
        {
            using (var connection = GetConnection())
            {
                connection.Execute(string.Format(Sql, type), new { entity = entity, category = category });
            }
        }

        #endregion
    }
}
