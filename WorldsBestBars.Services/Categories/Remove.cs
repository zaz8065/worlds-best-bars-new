using Dapper;
using System;
using System.Linq;
using System.Collections.Generic;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Categories
{
    public class Remove : BaseService
    {
        #region Constants

        const string Sql = @"delete from {0}Category where {0}Id = @entity and CategoryId = @category";

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
