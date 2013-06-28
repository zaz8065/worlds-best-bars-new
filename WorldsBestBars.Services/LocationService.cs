using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services
{
    public class LocationService : BaseService
    {
        #region Public Methods

        public async Task<Location> Get(Guid id)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"select 
    top 1
    L.Id,
    L.Name,
    L.UrlKey,
    L.ParentId,
    (select count(*) from Location L2 where L2.ParentId = L.Id) as ChildLocationCount,
    (select count(*) from Bar B where B.LocationId = L.Id) as ChildBarCount
from
    Location L
where
    Id = @id;";

                    command.Parameters.AddWithValue("@id", id);

                    var result = new List<Location>();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            return Location.FromReader(reader);
                        }

                        return null;
                    }
                }
            }
        }

        public async Task<IEnumerable<Location>> GetChildren(Guid? parent)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"select 
    L.Id,
    L.Name,
    L.UrlKey,
    L.ParentId,
    (select count(*) from Location L2 where L2.ParentId = L.Id) as ChildLocationCount,
    (select count(*) from Bar B where B.LocationId = L.Id) as ChildBarCount
from
    Location L
where
    (@parent is null and ParentId is null) 
    or (@parent is not null and ParentId = @parent)
order by
    L.Name;";

                    command.Parameters.AddWithValue("@parent", (object)parent ?? DBNull.Value);

                    var result = new List<Location>();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            result.Add(Location.FromReader(reader));
                        }
                    }

                    return result.ToArray();
                }
            }
        }

        public async Task<IEnumerable<Location>> Search(string query)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"select 
    L.Id,
    L.Name,
    L.UrlKey,
    L.ParentId,
    (select count(*) from Location L2 where L2.ParentId = L.Id) as ChildLocationCount,
    (select count(*) from Bar B where B.LocationId = L.Id) as ChildBarCount
from
    Location L
where
    Name like '%' + @query + '%'
order by
    L.Name;";

                    command.Parameters.AddWithValue("@query", query ?? string.Empty);

                    var result = new List<Location>();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            result.Add(Location.FromReader(reader));
                        }
                    }

                    return result.ToArray();
                }
            }
        }

        public async Task<Guid> Create(CreateLocation model)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select @id = newid(); insert into Location ( Id, Created, Modified, Name, ParentId ) values ( @id, getdate(), getdate(), @name, @parent )";

                    command.Parameters.AddWithValue("@name", model.Name);
                    command.Parameters.AddWithValue("@parent", model.ParentId.HasValue ? (object)model.ParentId : DBNull.Value);
                    command.Parameters.Add(new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    await command.ExecuteNonQueryAsync();

                    var id = (Guid)command.Parameters["@id"].Value;

                    ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "location");

                    return id;
                }
            }
        }

        #endregion
    }
}
