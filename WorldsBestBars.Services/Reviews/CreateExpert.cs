using Dapper;
using System;
using System.Data;

namespace WorldsBestBars.Services.Reviews
{
    public class CreateExpert : BaseService
    {
        #region Constants

        const string Sql = @"select @id = newid(); insert into Review ( Id, BarId, UserId, Comment, IsActive, Created, IsModerated ) values ( @id, @bar, @expert, @comment, 1, getdate(), 1 )";

        #endregion

        #region Public Methods

        public Guid Execute(Guid bar, Guid expert, string comment)
        {
            using (var connection = GetConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", dbType: DbType.Guid, direction: ParameterDirection.Output);
                parameters.Add("@bar", bar);
                parameters.Add("@expert", expert);
                parameters.Add("@comment", comment);

                connection.Execute(Sql, parameters);

                return parameters.Get<Guid>("@id");
            }
        }

        #endregion
    }
}
