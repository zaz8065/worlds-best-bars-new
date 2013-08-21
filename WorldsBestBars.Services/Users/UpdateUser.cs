using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace WorldsBestBars.Services.Users
{
    public class UpdateUser : BaseService
    {
        #region Constants

        const string Sql = @"
update 
    [User]
set
    [Name] = @name,
    [Email] = @email,
    [DateOfBirth] = @dateOfBirth,
    [City] = @city
where
    [Id] = @id;

delete from UserAttribute where UserId = @id and [Key] = 'favourite:cities';
if @favouriteCities is not null
    insert into UserAttribute ( Id, UserId, [Key], ValueString ) values ( newid(), @id, 'favourite:cities', @favouriteCities );

delete from UserAttribute where UserId = @id and [Key] = 'favourite:bars';
if @favouriteBars is not null
    insert into UserAttribute ( Id, UserId, [Key], ValueString ) values ( newid(), @id, 'favourite:bars', @favouriteBars );

delete from UserAttribute where UserId = @id and [Key] = 'favourite:brands';
if @favouriteBrands is not null
    insert into UserAttribute ( Id, UserId, [Key], ValueString ) values ( newid(), @id, 'favourite:brands', @favouriteBrands );

delete from UserAttribute where UserId = @id and [Key] = 'favourite:cocktails';
if @favouriteCocktails is not null
    insert into UserAttribute ( Id, UserId, [Key], ValueString ) values ( newid(), @id, 'favourite:cocktails', @favouriteCocktails );

if @password is not null
    update [User] set [Password] = @password where [Id] = @id;

";

        #endregion

        #region Public Methods

        public void Execute(Guid id, Models.UpdateUser updates)
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql, new
                {
                    id = id,
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
            }
        }

        #endregion
    }
}
