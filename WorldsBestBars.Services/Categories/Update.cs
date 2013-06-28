using Dapper;
using System;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Categories
{
    public class Update : BaseService
    {
        #region Constants

        const string Sql = @"
update [Category] set
    [Name] = @name,
    [Key] = @key,
    [Group] = @group
where
    [Id] = @id";

        #endregion

        #region Public Methods

        public void Execute(Guid id, UpdateCategory model)
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql, new
                {
                    id = id,
                    name = model.Name,
                    key = model.Key,
                    group = model.Group
                });
            }
        }

        #endregion
    }
}
