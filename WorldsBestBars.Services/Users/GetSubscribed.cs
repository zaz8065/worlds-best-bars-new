using Dapper;
using System.Collections.Generic;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Users
{
    public class GetSubscribed : BaseService
    {
        #region Constants

        const string Sql = @"select Id, Name, Email, IsAdmin, Created, City from [User] U where not exists (select 1 from UserAttribute UA where UA.UserId = U.Id and UA.[Key] = 'email.unsubscribed')";

        #endregion

        #region Public Methods

        public IEnumerable<UserSummary> Execute()
        {
            using (var connection = GetConnection())
            {
                return connection.Query<UserSummary>(Sql);
            }
        }

        #endregion
    }
}
