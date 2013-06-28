using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services
{
    public class UserService : BaseService
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService" /> class.
        /// </summary>
        public UserService()
        {
        }

        #endregion

        #region Public Methods

        public bool ValidateUser(string email, string password, out Guid id)
        {
            id = Guid.Empty;

            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select Id from [User] where [Email] = @email and [Password] = @password";

                    command.Parameters.AddWithValue("@email", email.Trim());
                    command.Parameters.AddWithValue("@password", MD5(password.Trim()));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            id = (Guid)reader["Id"];

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public UserSummary GetSummary(Guid id)
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

        public IEnumerable<UserSummary> GetAll(out int total, int? skip, int? take, string filter, string locationFilter = null, string sort = "name")
        {
            total = 0;

            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select @count = count(*) from [User] where ((@filter is null) or (@filter is not null and ([Name] like '%' + @filter + '%' or [Email] like '%' + @filter + '%'))) and ((@locationFilter is null) or (@locationFilter is not null and City like '%' + @locationFilter + '%')) and IsExpert = 0";
                    command.Parameters.Add(new SqlParameter("@count", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    command.Parameters.AddWithValue("@filter", string.IsNullOrEmpty(filter) ? DBNull.Value : (object)filter);
                    command.Parameters.AddWithValue("@locationFilter", string.IsNullOrEmpty(locationFilter) ? DBNull.Value : (object)locationFilter);
                    command.ExecuteNonQuery();
                    total = (int)command.Parameters["@count"].Value;
                }

                using (var command = connection.CreateCommand())
                {
                    switch ((sort ?? string.Empty).ToLower())
                    {
                        case "email":
                        case "created":
                        case "isadmin":
                        case "city":
                            break;
                        default:
                            sort = "name";
                            break;
                    }

                    command.CommandText = string.Format("with cte as ( select Id, Created, Name, Email, IsAdmin, City, row_number() over (order by [{0}]) as __RN from [User] where ((@filter is null) or (@filter is not null and ([Name] like '%' + @filter + '%' or [Email] like '%' + @filter + '%'))) and ((@locationFilter is null) or (@locationFilter is not null and City like '%' + @locationFilter + '%')) and IsExpert = 0 ) select Id, Created, Name, Email, IsAdmin, City from cte where __RN between @skip and (@skip + @take)", sort);

                    command.Parameters.AddWithValue("@skip", skip ?? 0);
                    command.Parameters.AddWithValue("@take", take ?? int.MaxValue);
                    command.Parameters.AddWithValue("@filter", string.IsNullOrEmpty(filter) ? DBNull.Value : (object)filter);
                    command.Parameters.AddWithValue("@locationFilter", string.IsNullOrEmpty(locationFilter) ? DBNull.Value : (object)locationFilter);

                    using (var reader = command.ExecuteReader())
                    {
                        var result = new List<UserSummary>();
                        while (reader.Read())
                        {
                            result.Add(UserSummary.FromReader(reader));
                        }

                        return result.ToArray();
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        static string MD5(string input)
        {
            return string.Join(string.Empty, new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(input)).Select(b => b.ToString("x2")));
        }

        #endregion
    }
}
