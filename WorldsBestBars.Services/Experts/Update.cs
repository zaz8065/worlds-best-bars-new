using Dapper;
using System;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Experts
{
    public class Update : BaseService
    {
        #region Constants

        const string Sql = @"update [User] set
    UrlKey = @urlKey,
    Name = @name,
    Title = @title,
    Biography = @biography,
    Website = @website
where
    [Id] = @id
    and [IsExpert] = 1";

        #endregion

        #region Public Methods

        public void Execute(Guid id, ExpertDetail model)
        {
            using (var connection = GetConnection())
            {
                if (!string.IsNullOrEmpty(model.Website) && !model.Website.StartsWith("http"))
                {
                    model.Website = "http://" + model.Website;
                }

                connection.Execute(Sql, new { id = id, urlKey = model.UrlKey, name = model.Name, title = model.Title, biography = model.Biography, website = model.Website });

                ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "expert");
            }
        }

        #endregion
    }
}
