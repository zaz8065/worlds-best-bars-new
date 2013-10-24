using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Users
{
    public static class Helper
    {
        public static UserSummary Get(Guid id)
        {
            var service = new Get();

            return service.Execute(id);
        }

        public static bool IsEmailInUse(string email)
        {
            var service = new IsEmailInUse();

            return service.Execute(email);
        }

        public static string GetNormalisedCity(string city)
        {
            if (string.IsNullOrEmpty(city))
            {
                return null;
            }

            var lookup = new Dictionary<string, string>()
                {
                    { "london", "London, United Kingdom" },
                    { "sydney", "Sydney, Australia" },
                    { "new york", "New York City, United States" },
                    { "nyc", "New York City, United States" },
                    { "paris", "Paris, France" }
                };

            city = city.ToLower();

            foreach (var check in lookup)
            {
                if (city.Contains(check.Key))
                {
                    return check.Value;
                }
            }

            return null;
        }

        public static string MD5(string input)
        {
            return string.Join(string.Empty, new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(input)).Select(b => b.ToString("x2")));
        }
    }
}
