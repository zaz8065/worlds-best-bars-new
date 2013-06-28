using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using WorldsBestBars.Services.Models;
using System.Linq;

namespace WorldsBestBars.Services.Bars
{
    public class Get : BaseService
    {
        #region Constants

        const string Sql = @"select
    top 1
    B.Id,
    B.UrlKey,
    B.Url,
    B.Name,
    B.LocationId as [Location],
    L.Name as [LocationName],
    B.Address,
    B.Intro,
    B.Description,
    B.Website,
    B.Email,
    B.Phone,
    B.Fax,
    B.IsActive,
    B.MenuUrl,
    B.Created,
    B.Modified,
    cast(B.Geocoordinate.Lat as nvarchar(10)) + ',' + cast(B.Geocoordinate.Long as nvarchar(10)) as GeoString
from
    Bar B
    left join Location L on B.LocationId = L.Id
where
    B.Id = @id";

        #endregion

        #region Public Methods

        public BarDetail Execute(Guid id)
        {
            using (var connection = GetConnection())
            {
                var result = connection.Query<BarDetail>(Sql, new { id = id }).FirstOrDefault();

                if (result != null)
                {
                    result.OpenHours = GetOpening(connection, id);
                    result.Images = Services.Shared.GetImages(result.Url);
                    result.Features = GetFeatures(connection, id).Select(f => { f.Images = Services.Shared.GetImages(result.Url + "/features/" + f.Id); return f; });
                    result.Categories = GetCategories(connection, id);

                    // TODO: populate reviews, related, etc...
                }

                return result;
            }
        }

        #endregion

        #region Private Methods

        IEnumerable<BarDetail.Opening> GetOpening(IDbConnection connection, Guid id)
        {
            const string Sql = "select [Day], [Open], [Close] from [BarOpening] where barId = @id order by [Day], [Open]";

            return connection.Query<BarDetail.Opening>(Sql, new { id = id });
        }

        IEnumerable<BarDetail.Feature> GetFeatures(IDbConnection connection, Guid id)
        {
            const string Sql = "select [Id], [Name], [Details], [Extra], [IsActive], [Type], [Sponsor] from [Feature] where BarId = @id order by [Name]";

            return connection.Query<BarDetail.Feature>(Sql, new { id = id });
        }

        IEnumerable<NamedEntity> GetCategories(IDbConnection connection, Guid id)
        {
            const string Sql = "select C.[Id], C.[Name] from [BarCategory] BC inner join [Category] C on (BC.CategoryId = C.[Id]) where BC.[BarId] = @id";

            return connection.Query<NamedEntity>(Sql, new { id = id });
        }

        #endregion
    }
}
