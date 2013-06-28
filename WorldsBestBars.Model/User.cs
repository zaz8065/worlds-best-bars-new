using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using Newtonsoft.Json;
using System.Collections.Generic;

namespace WorldsBestBars.Model
{
    [Serializable()]
    [JsonObject()]
    public class User : DatabaseObject
    {
        [JsonProperty("active")]
        public bool IsActive { get; set; }

        [JsonProperty("expert")]
        public bool IsExpert { get; set; }

        [JsonProperty("admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("biography")]
        public string Biography { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("reviews")]
        public Review[] Reviews { get; set; }

        [JsonProperty("attributes")]
        public Dictionary<string, object> Attributes { get; set; }

        private string Password { get; set; }

        public bool ValidatePassword(string input)
        {
            return Password == MD5(input);
        }

        static string MD5(string input)
        {
            return string.Join(string.Empty, new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(input)).Select(b => b.ToString("x2")));
        }

        static User Convert(Data.User input)
        {
            if (input == null) { return null; }
            return new User()
            {
                Id = input.Id,
                Name = input.Name,
                Title = input.Title,
                Biography = input.Biography,
                Website = input.Website,
                UrlKey = input.IsExpert ? input.UrlKey : null,
                Url = input.IsExpert ? "experts/" + input.UrlKey : null,
                Email = input.Email,
                IsActive = input.IsActive,
                IsExpert = input.IsExpert,
                IsAdmin = input.IsAdmin,
                Password = input.Password == null ? null : input.Password.ToLower(),
                Attributes = input.Attributes.ToDictionary(e => e.Key, attr => (object)attr.ValueBool ?? (object)attr.ValueDate ?? (object)attr.ValueBinary ?? (object)attr.ValueDecimal ?? (object)attr.ValueInt ?? (object)attr.ValueString)
            };
        }

        public static User[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                return context.Users.ToList().Select(u => Convert(u)).ToArray();
            }
        }

        public static User[] GetExperts()
        {
            using (var context = new Data.Entities())
            {
                return context.Users.Where(u => u.IsExpert && u.IsActive).ToList().Select(u => Convert(u)).ToArray();
            }
        }

        public static User GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                return Convert(context.Users.SingleOrDefault(u => u.Id == id));
            }
        }

        public static User GetByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("email");
            }

            using (var context = new Data.Entities())
            {
                return Convert(context.Users.SingleOrDefault(u => u.Email == email));
            }
        }

        public static string[] GetSubscribedEmails()
        {
            using (var context = new Data.Entities())
            {
                return context.Users.
                    Where(u => 
                        u.IsActive && 
                        !u.IsAdmin && 
                        !u.IsExpert && 
                        !string.IsNullOrEmpty(u.Email) && 
                        !u.Attributes.Any(a => a.Key == "email.unsubscribed" && a.ValueBool == true)
                    ).Select(u => u.Email).ToArray();
            }
        }

        public static Guid Register(string name, string city, string email, DateTime dob, string password)
        {
            using (var context = new Data.Entities())
            {
                if (context.Users.Any(u => u.Email == email))
                {
                    throw new Exception("the email address already exists in the system");
                }

                var user = new Data.User()
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    City = city,
                    Email = email,
                    DateOfBirth = dob,
                    Password = MD5(password),
                    IsActive = true,
                    IsExpert = false,
                    Created = DateTime.Now,
                    Modified = DateTime.Now
                };

                context.AddToUsers(user);

                context.SaveChanges();

                return user.Id;
            }
        }

        public static void SetAttribute(Guid id, string key, object value)
        {
            using (var context = new Data.Entities())
            {
                var user = context.Users.SingleOrDefault(u => u.Id == id);

                if (user != default(Data.User))
                {
                    var attribute = user.Attributes.SingleOrDefault(a => a.Key == key);
                    if (attribute == null)
                    {
                        attribute = new Data.UserAttribute();
                        user.Attributes.Add(attribute);
                    }

                    if (value is int)
                    {
                        attribute.ValueInt = (int)value;
                        attribute.ValueString = null;
                        attribute.ValueBinary = null;
                        attribute.ValueDate = null;
                        attribute.ValueDecimal = null;
                        attribute.ValueBool = null;
                    }
                    else if (value is string)
                    {
                        attribute.ValueString = (string)value;
                        attribute.ValueInt = null;
                        attribute.ValueBinary = null;
                        attribute.ValueDate = null;
                        attribute.ValueDecimal = null;
                        attribute.ValueBool = null;
                    }
                    else if (value is byte[])
                    {
                        attribute.ValueBinary = (byte[])value;
                        attribute.ValueString = null;
                        attribute.ValueInt = null;
                        attribute.ValueDate = null;
                        attribute.ValueDecimal = null;
                        attribute.ValueBool = null;
                    }
                    else if (value is double)
                    {
                        attribute.ValueDecimal = (decimal)(double)value;
                        attribute.ValueString = null;
                        attribute.ValueBinary = null;
                        attribute.ValueDate = null;
                        attribute.ValueInt = null;
                        attribute.ValueBool = null;
                    }
                    else if (value is DateTime)
                    {
                        attribute.ValueDate = (DateTime)value;
                        attribute.ValueString = null;
                        attribute.ValueBinary = null;
                        attribute.ValueInt = null;
                        attribute.ValueDecimal = null;
                        attribute.ValueBool = null;
                    }
                    else if (value is bool)
                    {
                        attribute.ValueBool = (bool)value;
                        attribute.ValueDate = null;
                        attribute.ValueString = null;
                        attribute.ValueBinary = null;
                        attribute.ValueInt = null;
                        attribute.ValueDecimal = null;
                    }

                    context.SaveChanges();
                }
            }
        }
    }
}