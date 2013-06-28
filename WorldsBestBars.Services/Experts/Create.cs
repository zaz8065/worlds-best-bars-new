using Dapper;
using System;
using System.Data;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Experts
{
    public class Create : BaseService
    {
        #region Constants

        const string Sql = @"select @id = newid();
insert into [User] ( [Id], [UrlKey], [Name], [Title], [Biography], [Website], [IsExpert], [IsActive], [IsAdmin], [Created], [Modified] )
values ( @id, @urlKey, @name, @title, @biography, @website, 1, 0, 1, getdate(), getdate() )";

        #endregion

        #region Public Methods

        public Guid Execute(ExpertDetail model)
        {
            using (var connection = GetConnection())
            {
                if (!string.IsNullOrEmpty(model.Website) && !model.Website.StartsWith("http"))
                {
                    model.Website = "http://" + model.Website;
                }

                var parameters = new DynamicParameters(new { urlKey = model.UrlKey, name = model.Name, title = model.Title, biography = model.Biography, website = model.Website });
                parameters.Add("@id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                connection.Execute(Sql, parameters);

                var result = parameters.Get<Guid>("@id");                

                ServiceResolver.GetService<TryInvalidateCache>().Execute(result, "expert");

                return result;
            }
        }

        #endregion
    }
}
