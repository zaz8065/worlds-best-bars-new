using System;
using System.Linq;

using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [JsonObject()]
    public class Document : DatabaseObject
    {
        [JsonProperty("parent")]
        public Guid? Parent { get; set; }

        [JsonProperty("redirect")]
        public DatabaseObject Redirect { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("synopsis")]
        public string Synopsis { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("active")]
        public bool IsActive { get; set; }

        public static Document Convert(Data.Document input)
        {
            if (input == null) { return null; }
            return new Document()
            {
                Id = input.Id,
                Name = input.Name,
                Parent = input.ParentId,
                Redirect = input.RedirectTo == null ? null : new DatabaseObject()
                {
                    Id = input.RedirectTo.Id,
                    Name = input.RedirectTo.Name,
                    Url = input.RedirectTo.Url,
                    UrlKey = input.RedirectTo.UrlKey
                },
                Content = input.Content,
                Synopsis = input.Synopsis,
                UrlKey = input.UrlKey,
                Created = input.Created,
                Url = input.Url,
                IsActive = input.IsActive
            };
        }

        public static Document[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                return context.Documents.ToList().Select(d => Convert(d)).ToArray();
            }
        }

        public static Document GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                return Convert(context.Documents.SingleOrDefault(d => d.Id == id));
            }
        }
    }
}
