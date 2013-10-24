using System;
using Dapper;
using System.Data;

namespace WorldsBestBars.Services.Users
{
    public class CreateUser : BaseService
    {
        #region Constants

        const string Sql = @"select @id = newid();
insert into [User] ( [Id], [Name], [Email], [DateOfBirth], [City], [IsExpert], [IsActive], [IsAdmin], [Created], [Modified], [Password] ) values ( @id, @name, @email, @dateOfBirth, @city, 0, 1, 0, getdate(), getdate(), @password );

if @favouriteCities is not null
    insert into UserAttribute ( Id, UserId, [Key], ValueString ) values ( newid(), @id, 'favourite:cities', @favouriteCities );

if @favouriteBars is not null
    insert into UserAttribute ( Id, UserId, [Key], ValueString ) values ( newid(), @id, 'favourite:bars', @favouriteBars );

if @favouriteBrands is not null
    insert into UserAttribute ( Id, UserId, [Key], ValueString ) values ( newid(), @id, 'favourite:brands', @favouriteBrands );

if @favouriteCocktails is not null
    insert into UserAttribute ( Id, UserId, [Key], ValueString ) values ( newid(), @id, 'favourite:cocktails', @favouriteCocktails );
";

        #endregion

        #region Public Methods

        public Guid Execute(Models.UpdateUser updates)
        {
            Guid id;

            using (var connection = GetConnection())
            {
                var parameters = new DynamicParameters(new
                {
                    name = updates.Name,
                    email = updates.Email,
                    dateOfBirth = updates.DateOfBirth,
                    city = updates.City,
                    favouriteCities = updates.FavouriteCities,
                    favouriteBars = updates.FavouriteBars,
                    favouriteCocktails = updates.FavouriteCocktails,
                    favouriteBrands = updates.FavouriteBrands,
                    password = string.IsNullOrEmpty(updates.Password) ? null : Helper.MD5(updates.Password)
                });
                parameters.Add("id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                connection.Execute(Sql, parameters);

                id = parameters.Get<Guid>("id");
            }

            try
            {
                GetService<Email.MailChimp.Subscribe>().Execute(id);
            }
            catch
            {
                // probably should do something here.
            }

            return id;
        }

        #endregion
    }
}
