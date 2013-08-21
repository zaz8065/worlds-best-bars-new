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
            var original = GetService<Get>().Execute(id);

            if (original.Email != updates.Email)
            {
                // if the user has changed their email address, unsubscribe them from mail chimp first
                try
                {
                    GetService<Email.MailChimp.Unsubscribe>().Execute(id);
                }
                catch
                {
                    // probably should do something here.
                }
            }

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

            if (original.Email != updates.Email)
            {
                // if the user has changed their email address, re-subscribe them from mail chimp first
                try
                {
                    GetService<Email.MailChimp.Subscribe>().Execute(id);
                }
                catch
                {
                    // probably should do something here.
                }
            }

            if (original.City != updates.City && original.Email == updates.Email)
            {
                // if the user has updated their city, re-subscribing them updates their details
                try
                {
                    GetService<Email.MailChimp.Subscribe>().Execute(id);
                }
                catch
                {
                    // probably should do something here.
                }
            }
        }

        #endregion
    }
}
