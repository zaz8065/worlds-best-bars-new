using Dapper;
using System;

namespace WorldsBestBars.Services.Experts
{
    public class Delete : BaseService
    {
        #region Constants

        const string Sql = @"delete from [Review] where [UserId] = @id; delete from [User] where [Id] = @id";

        #endregion

        #region Public Methods

        public void Execute(Guid id)
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql, new { id = id });

                ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "expert");
            }
        }

        #endregion
    }
}
