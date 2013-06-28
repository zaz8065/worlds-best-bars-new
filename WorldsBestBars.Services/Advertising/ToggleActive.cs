using Dapper;
using System;

namespace WorldsBestBars.Services.Advertising
{
    public class ToggleActive : BaseService
    {
        #region Constants

        const string Sql = @"update Advert set IsActive = @isActive where Id = @id";

        #endregion

        #region Public Methods

        public void Execute(Guid id, bool isActive)
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql, new
                {
                    id = id,
                    isActive = isActive
                });
            }
        }

        #endregion
    }
}
