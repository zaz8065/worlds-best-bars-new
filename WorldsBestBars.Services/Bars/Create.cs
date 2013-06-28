using Dapper;
using System;
using System.Data;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Bars
{
    public class Create : BaseService
    {
        #region Constants

        const string Sql = @"select @id = newid();
insert into [Bar] 
    ( Id,
      Name,
      UrlKey,
      Intro,
      Description,
      Website,
      Email,
      Phone,
      Fax,
      MenuUrl,
      Address,
      IsActive,
      Created,
      Modified )
values
    ( @id,
      @name,
      @urlKey,
      @intro,
      @description,
      @website,
      @email,
      @phone,
      @fax,
      @menuUrl,
      @address,
      0, 
      getdate(), 
      getdate() );";

        #endregion

        #region Public Methods

        public Guid Execute(UpdateBar model)
        {
            using (var connection = GetConnection())
            {
                var transaction = connection.BeginTransaction();

                var parameters = new DynamicParameters(new
                {
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
                });
                parameters.Add("@id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                connection.Execute(Sql, parameters, transaction);

                var id = parameters.Get<Guid>("@id");

                Shared.UpdateOpenHours(transaction, id, model.OpenHours);
                Shared.UpdateFeatures(transaction, id, model.Features);

                transaction.Commit();

                ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "bar");

                return id;
            }
        }

        #endregion
    }
}
