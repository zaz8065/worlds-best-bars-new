using Dapper;
using System.Data;

namespace WorldsBestBars.Services.Users
{
    public class IsEmailInUse : BaseService
    {
        #region Constants
        
        const string Sql = "select @count = count(*) from [User] where Email = @email";

        #endregion

        #region Public Methods

        public bool Execute(string email)
        {
            using (var connection = GetConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@email", email);
                parameters.Add("@count", dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute(Sql, parameters);

                return parameters.Get<int>("@count") > 0;
            }
        }

        #endregion
    }
}
