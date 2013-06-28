using Dapper;
using System;

namespace WorldsBestBars.Services.Bars
{
    public class SetUrlKey : BaseService
    {
        #region Constants

        const string Sql = "update Bar set UrlKey = @urlKey, Modified = getdate() where Id = @id";

        #endregion

        #region Public Methods

        public void Execute(Guid id, string urlKey)
        {
            // TODO: add validation.

            using (var connection = GetConnection())
            {
                connection.Execute(Sql, new { id = id, urlKey = urlKey });

                ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "bar");
            }
        }

        #endregion
    }
}
