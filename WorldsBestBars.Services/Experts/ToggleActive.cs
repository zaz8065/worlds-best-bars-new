using Dapper;
using System;

namespace WorldsBestBars.Services.Experts
{
    public class ToggleActive : BaseService
    {
        #region Constants

        const string Sql = "update [User] set IsActive = @isActive, Modified = getdate() where Id = @id";

        #endregion

        #region Public Methods

        public void Execute(Guid id, bool isActive)
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql, new { id = id, isActive = isActive });

                ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "expert");
            }
        }

        #endregion
    }
}
