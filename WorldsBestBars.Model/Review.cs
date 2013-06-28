using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [Serializable()]
    [JsonObject()]
    public class Review : DatabaseObject
    {
        [JsonProperty("user")]
        public DatabaseObject User { get; set; }

        [JsonProperty("bar")]
        public DatabaseObject Bar { get; set; }

        [JsonProperty("active")]
        public bool IsActive { get; set; }

        [JsonProperty("moderated")]
        public bool IsModerated { get; set; }

        [JsonProperty("rating")]
        public ReviewRating Rating { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("parent")]
        public Guid? Parent { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("ip")]
        public string UserIP { get; set; }

        [Serializable()]
        [JsonObject()]
        public class ReviewRating
        {
            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("drinks")]
            public double Drinks { get; set; }

            [JsonProperty("design")]
            public double Design { get; set; }

            [JsonProperty("service")]
            public double Service { get; set; }

            [JsonProperty("overall")]
            public double Overall { get; set; }

            [JsonProperty("overall2")]
            public double? Overall2
            {
                get
                {
                    var values = new List<double>();

                    if (Drinks != 0)
                    {
                        values.Add(Drinks);
                    }

                    if (Design != 0)
                    {
                        values.Add(Design);
                    }

                    if (Service != 0)
                    {
                        values.Add(Service);
                    }

                    if (!values.Any())
                    {
                        return null;
                    }

                    return values.Average();
                }
            }
        }

        public static Review Convert(Data.Review input)
        {
            if (input == null) { return null; }
            return new Review()
            {
                Id = input.Id,
                IsActive = input.IsActive,
                IsModerated = input.IsModerated,
                Name = input.Title,
                Comment = input.Comment,
                Parent = input.ParentId,
                Created = input.Created,
                UserIP = input.UserIP,
                Bar = new DatabaseObject()
                {
                    Id = input.Bar.Id,
                    Name = input.Bar.Name,
                    Url = input.Bar.Url,
                    UrlKey = input.Bar.UrlKey
                },
                User = new DatabaseObject()
                {
                    Id = input.User == null ? Guid.Empty : input.User.Id,
                    Name = input.User == null ? "Anonymous" : input.User.Name
                },
                Rating = input.Rating == null ? null : new ReviewRating()
                {
                    Id = input.Rating.Id,
                    Drinks = (double)input.Rating.Drinks / 255.0 * 5.0,
                    Design = (double)input.Rating.Design / 255.0 * 5.0,
                    Service = (double)input.Rating.Service / 255.0 * 5.0,
                    Overall = (double)(int)input.Rating.Overall / 255.0 * 5.0
                }
            };
        }

        public static Review[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                return context.Reviews.ToList().Select(r => Convert(r)).ToArray();
            }
        }

        public static Review GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                return Convert(context.Reviews.SingleOrDefault(r => r.Id == id));
            }
        }

        public static Guid? Create(Guid bar, Guid? user, Guid? parent, string ip, string title, string comment, double design = -1, double drinks = -1, double service = -1)
        {
            using (var context = new Data.Entities())
            {
                var review = new Data.Review()
                {
                    Id = Guid.NewGuid(),
                    BarId = bar,
                    UserId = user,
                    Title = title,
                    Comment = comment,
                    ParentId = parent,
                    Rating = (design == -1 && drinks == -1 && service == -1) ? null : new Data.Rating()
                    {
                        Id = Guid.NewGuid(),
                        Design = (byte)((design / 5.0) * byte.MaxValue),
                        Drinks = (byte)((drinks / 5.0) * byte.MaxValue),
                        Service = (byte)((service / 5.0) * byte.MaxValue),
                    },
                    UserIP = ip,
                    Created = DateTime.Now,
                    IsActive = true,
                    IsModerated = false
                };

                context.Reviews.AddObject(review);

                context.SaveChanges();

                return review.Id;
            }
        }

        public static Model.Review[] GetByBar(Guid id, bool activeOnly = true)
        {
            using (var context = new Data.Entities())
            {
                return context.Reviews.Where(r => r.BarId == id && (!activeOnly || r.IsActive)).ToList().Select(r => Convert(r)).ToArray();
            }
        }

        public static Model.Review[] GetByExpert(Guid id, bool activeOnly = true)
        {
            using (var context = new Data.Entities())
            {
                return context.Reviews.Where(r => r.UserId == id && (!activeOnly || r.IsActive)).ToList().Select(r => Convert(r)).ToArray();
            }
        }

        public static Model.Review[] GetExpertByBar(Guid id, bool activeOnly = true)
        {
            using (var context = new Data.Entities())
            {
                return context.Reviews.Where(r => r.User != null && r.User.IsExpert && r.BarId == id && (!activeOnly || r.IsActive)).ToList().Select(r => Convert(r)).ToArray();
            }
        }

        public static Model.Review[] GetByParent(Guid id, bool activeOnly = true)
        {
            using (var context = new Data.Entities())
            {
                return context.Reviews.Where(r => r.ParentId == id && (!activeOnly || r.IsActive)).ToList().Select(r => Convert(r)).ToArray();
            }
        }

        public static Model.Review[] GetRecent(int limit, bool activeOnly = true)
        {
            using (var context = new Data.Entities())
            {
                return context.Reviews
                    .Where(r => (!activeOnly || r.IsActive))
                    .OrderByDescending(r => r.Created)
                    .Take(limit)
                    .ToList()
                    .Select(r => Convert(r))
                    .ToArray();
            }
        }
    }
}
