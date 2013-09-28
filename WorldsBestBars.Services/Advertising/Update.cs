using Dapper;
using System;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Advertising
{
    public class Update : BaseService
    {
        #region Constants

        const string Sql = @"update Advert
    set [Title] = @title,
    [Start] = @start,
    [Finish] = @finish,
    [Weight] = @weight,
    [Type] = @type,
    [DestinationUrl] = @destination,
    [TargetCountries] = @targetCountries
where
    Id = @id";

        #endregion

        #region Public Methods

        public void Execute(Guid id, UpdateAdvert model)
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql, new
                {
                    id = id,
                    title = model.Title,
                    start = model.Start,
                    finish = model.Finish,
                    weight = model.Weight,
                    type = model.Type,
                    destination = model.DestinationUrl,
                    targetCountries = model.TargetCountries
                });
            }
        }

        #endregion
    }
}
