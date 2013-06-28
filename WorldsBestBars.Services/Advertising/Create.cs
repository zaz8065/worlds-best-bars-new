using Dapper;
using System;
using System.Data;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Advertising
{
    public class Create : BaseService
    {
        #region Constants

        const string Sql = @"select @id = newid();
insert into Advert ( [Id], [Title], [Start], [Finish], [Weight], [Type], [DestinationUrl], SkyscraperUrl, IsActive )
values ( @id, @title, @start, @finish, @weight, @type, @destination, '', 0 );";

        #endregion

        #region Public Methods

        public Guid Execute(UpdateAdvert model)
        {
            using (var connection = GetConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", dbType: DbType.Guid, direction: ParameterDirection.Output);
                parameters.Add("@title", model.Title);
                parameters.Add("@start", model.Start);
                parameters.Add("@finish", model.Finish);
                parameters.Add("@weight", model.Weight);
                parameters.Add("@type", model.Type);
                parameters.Add("@destination", model.DestinationUrl);

                connection.Execute(Sql, parameters);

                return parameters.Get<Guid>("@id");
            }
        }

        #endregion
    }
}
