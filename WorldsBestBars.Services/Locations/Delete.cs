using Dapper;
using System;

namespace WorldsBestBars.Services.Locations
{
    public class Delete : BaseService
    {
        #region Constants

        const string Sql = @"delete from [Location] where Id = @id";

        #endregion

        #region Public Methods

        public void Execute(Guid id)
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql, new { id = id });
            }
        }

        #endregion
    }
}
