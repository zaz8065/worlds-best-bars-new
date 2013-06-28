using Dapper;
using System;

namespace WorldsBestBars.Services.Reviews
{
    public class UpdateExpert : BaseService
    {
        #region Constants

        const string Sql = @"update Review set Comment = @comment where Id = @id";

        #endregion

        #region Public Methods

        public void Execute(Guid id, string comment)
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql, new { id = id, comment = comment });
            }
        }

        #endregion
    }
}
