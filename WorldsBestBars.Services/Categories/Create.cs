using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using WorldsBestBars.Services.Models;
using System.Linq;

namespace WorldsBestBars.Services.Categories
{
    public class Create : BaseService
    {
        #region Constants

        const string Sql = @"select @id = newid();
insert into [Category] 
    ( [Id],
      [Name],
      [Key],
      [IsSystem],
      [IsSearchable],
      [Group])
values
    ( @id,
      @name,
      @key,
      0,
      0,
      @group );";

        #endregion

        #region Public Methods

        public Guid Execute(CreateCategory model)
        {
            using (var connection = GetConnection())
            {
                var parameters = new DynamicParameters(new { name = model.Name, key = model.Key, group = model.Group });
                parameters.Add("@id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                connection.Execute(Sql, parameters);

                var id = parameters.Get<Guid>("@id");

                return id;
            }
        }

        #endregion
    }
}
