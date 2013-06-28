using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Bars
{
    class Shared
    {
        internal static string AddressToXml(UpdateBar bar)
        {
            var element = new XElement("address",
                new XElement("street1", bar.AddressLine1),
                new XElement("street2", bar.AddressLine2),
                new XElement("city", bar.AddressCity),
                new XElement("postcode", bar.AddressPostcode),
                new XElement("country", bar.AddressCountry));

            return element.ToString();
        }

        internal static void UpdateOpenHours(IDbTransaction transaction, Guid id, IEnumerable<BarDetail.Opening> opening)
        {
            var connection = transaction.Connection;

            connection.Execute("delete from BarOpening where [BarId] = @id", new { id = id }, transaction);

            if (opening != null && opening.Any())
            {
                foreach (var period in opening)
                {
                    connection.Execute("insert into BarOpening ( id, [BarId], [Day], [Open], [Close] ) values ( newid(), @bar, @day, @open, @close )", new
                    {
                        bar = id,
                        day = (byte)period.Day,
                        open = period.Open,
                        close = period.Close
                    }, transaction);
                }
            }
        }

        internal static void UpdateFeatures(IDbTransaction transaction, Guid id, IEnumerable<BarDetail.Feature> features)
        {
            var connection = transaction.Connection;

            features = features ?? new BarDetail.Feature[0];

            var existing = connection.Query<Guid>("select Id from Feature where BarId = @id", new { id = id }, transaction);

            // delete removed
            var removed = existing.Except(features.Select(e => e.Id));
            if (removed.Any())
            {
                foreach (var idToRemove in removed)
                {
                    connection.Execute("delete from Feature where Id = @id", new { id = idToRemove }, transaction);
                }
            }

            // update existing
            var updates = features.Where(f => existing.Contains(f.Id));
            if (updates.Any())
            {
                foreach (var update in updates)
                {
                    connection.Execute("update Feature set Name = @name, Details = @details, Extra = @extra, IsActive = @isActive, Type = @type, Sponsor = @sponsor where Id = @id", new
                    {
                        id = update.Id,
                        name = update.Name,
                        details = update.Details,
                        extra = update.Extra,
                        isActive = update.IsActive,
                        type = update.Type,
                        sponsor = update.Sponsor
                    }, transaction);
                }

            }

            //add new
            var additions = features.Where(f => f.Id == Guid.Empty);
            if (additions.Any())
            {
                using (var command = connection.CreateCommand())
                {
                    foreach (var addition in additions)
                    {
                        var parameters = new DynamicParameters(new
                        {
                            bar = id,
                            name = addition.Name,
                            details = addition.Details,
                            extra = addition.Extra,
                            isActive = addition.IsActive,
                            type = addition.Type,
                            sponsor = addition.Sponsor
                        });

                        parameters.Add("@id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                        connection.Execute("select @id = newid(); insert into Feature (Id, BarId, Name, Details, Extra, IsActive, Type, Sponsor) values (@id, @bar, @name, @details, @extra, @isActive, @type, @sponsor)", parameters, transaction);

                        addition.Id = parameters.Get<Guid>("@id");
                    }
                }
            }
        }
    }
}
