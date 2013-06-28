using Dapper;
using System;
using System.Linq;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Locations
{
    public class Get : BaseService
    {
        #region Constants

        const string Sql = @"select
    top 1
    L.Id,
    L.UrlKey,
    L.Name,
    L.Synopsis,
    L.Intro,
    L.Url
from
    [Location] L
where
    L.Id = @id";

        #endregion

        #region Public Methods

        public LocationDetail Execute(Guid id)
        {
            using (var connection = GetConnection())
            {
                var result = connection.Query<LocationDetail>(Sql, new { id = id }).FirstOrDefault();

                if (result != null)
                {
                    result.Images = Services.Shared.GetImages(result.Url);
                }

                return result;
            }
        }

        #endregion
    }
}
