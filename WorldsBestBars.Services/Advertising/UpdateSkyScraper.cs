using Dapper;
using System;

namespace WorldsBestBars.Services.Advertising
{
    public class UpdateSkyScraper : BaseService
    {
        #region Constants

        const string Sql = @"update Advert set SkyscraperUrl = @url where Id = @id";

        #endregion

        #region Public Methods

        public void Execute(Guid id, string url)
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql, new
                {
                    id = id,
                    url = url
                });
            }
        }

        #endregion
    }
}
