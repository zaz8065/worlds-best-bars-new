using Dapper;
using System;

namespace WorldsBestBars.Services.Reviews
{
    public class DeleteExpert : BaseService
    {
        #region Constants

        const string Sql = @"delete from Review where Id = @id";

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
