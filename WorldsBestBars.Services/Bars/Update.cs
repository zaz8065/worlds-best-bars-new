using Dapper;
using System;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Bars
{
    public class Update : BaseService
    {
        #region Constants

        const string Sql = @"
update [Bar] set
    Name = @name,
    UrlKey = @urlKey,
    LocationId = @location,
    Intro = @intro,
    Description = @description,
    Website = @website,
    Email = @email,
    Phone = @phone,
    Fax = @fax,
    MenuUrl = @menuUrl,
    Address = @address,
    Geocoordinate = geography::STGeomFromText('POINT(' + convert(nvarchar(max), @longitude) + ' ' + convert(nvarchar(max), @latitude) + ')', 4326),
    Modified = getdate()
where
    Id = @id";

        #endregion

        #region Public Methods

        public void Execute(Guid id, UpdateBar model)
        {
            // TODO: move media if key changes
            using (var connection = GetConnection())
            {
                var transaction = connection.BeginTransaction();

                connection.Execute(Sql, new
                {
                    id = id,
                    name = model.Name,
                    urlKey = model.UrlKey,
                    location = model.Location,
                    intro = model.Intro,
                    description = model.Description,
                    website = model.Website,
                    email = model.Email,
                    phone = model.Phone,
                    fax = model.Fax,
                    menuUrl = model.MenuUrl,
                    address = Shared.AddressToXml(model),
                    latitude = model.Geo == null ? null : (double?)model.Geo.Lat,
                    longitude = model.Geo == null ? null : (double?)model.Geo.Long
                }, transaction);

                Shared.UpdateOpenHours(transaction, id, model.OpenHours);
                Shared.UpdateFeatures(transaction, id, model.Features);

                transaction.Commit();

                ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "bar");
            }
        }

        #endregion
    }
}
