using System;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Users
{
    public class Get : BaseService
    {
        #region Public Methods

        public UserSummary Execute(Guid id)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select Id, Created, Name, Email, IsAdmin, City from [User] where Id = @id";

                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return UserSummary.FromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
