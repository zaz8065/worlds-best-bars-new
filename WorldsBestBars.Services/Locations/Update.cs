using Dapper;
using System;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Locations
{
    public class Update : BaseService
    {
        #region Constants

        const string Sql = @"update [Location] set
    UrlKey = @urlKey,
    Name = @name,
    Synopsis = @synopsis,
    Intro = @intro
where
    [Id] = @id";

        #endregion

        #region Public Methods

        public void Execute(Guid id, LocationDetail model)
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql, new { id = id, urlKey = model.UrlKey, name = model.Name, synopsis = model.Synopsis, intro = model.Intro });

                ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "location");
            }
        }

        #endregion
    }
}
