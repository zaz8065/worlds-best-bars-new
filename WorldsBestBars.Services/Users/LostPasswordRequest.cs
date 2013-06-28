using Dapper;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WorldsBestBars.Services.Users
{
    public class LostPasswordRequest : BaseService
    {
        #region Constants

        const string Sql = "update [User] set [Password] = @password where [Email] = @email";

        #endregion

        #region Public Methods

        public void Execute(string email)
        {
            using (var connection = GetConnection())
            {
                var password = RandomPassword();

                connection.Execute(Sql, new { email = email, password = EncodePassword(password) });

                var service = ServiceResolver.GetService<Services.Email.TrySendEmail>();
                var body = "Your password has been reset on http://www.worldsbestbars.com/ you can now login using this email address (" + email + ") and the password: " + password;

                service.Execute("website@worldsbestbars.com", email, "Password Reset", body);
            }
        }

        #endregion

        #region Private Methods

        string RandomPassword()
        {
            var rand = new Random();
            var length = rand.Next(8, 10);
            char[] chars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

            var result = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                result.Append(chars[rand.Next(chars.Length)]);
            }

            return result.ToString();
        }

        string EncodePassword(string originalPassword)
        {
            var md5 = new MD5CryptoServiceProvider();
            var encodedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(originalPassword));

            return string.Join(string.Empty, encodedBytes.Select(x => x.ToString("x2")));
        }

        #endregion
    }
}
