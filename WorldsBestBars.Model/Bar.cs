using System;
using System.Text;
using System.Linq;

using Newtonsoft.Json;
using System.Collections.Generic;

namespace WorldsBestBars.Model
{
    [JsonObject()]
    public class Bar : DatabaseObject
    {
        [JsonProperty("intro")]
        public string Intro { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("fax")]
        public string Fax { get; set; }

        [JsonProperty("active")]
        public bool IsActive { get; set; }

        [JsonProperty("geo")]
        public Geo Geo { get; set; }

        [JsonProperty("address")]
        public BarAddress Address { get; set; }

        [JsonProperty("parent")]
        public DatabaseObject Parent { get; set; }

        [JsonProperty("experts_choice")]
        public bool IsExpertsChoice { get; set; }

        [JsonProperty("most_talked_about")]
        public bool IsMostTalkedAbout { get; set; }

        [JsonProperty("top100")]
        public bool IsTop100 { get; set; }

        public List[] Lists { get; set; }

        [JsonObject()]
        public class BarAddress
        {
            [JsonProperty("street1")]
            public string Street1 { get; set; }

            [JsonProperty("street2")]
            public string Street2 { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("county")]
            public string County { get; set; }

            [JsonProperty("postcode")]
            public string Postcode { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            public override string ToString()
            {
                var ret = new StringBuilder();

                var parts = new string[] {
                    Street1,
                    Street2,
                    City,
                    County,
                    Postcode
                };

                foreach (var part in parts)
                {
                    if (!string.IsNullOrEmpty(part))
                    {
                        ret.Append(part.Trim());
                        ret.Append(", ");
                    }
                }

                if (ret.Length > 0)
                {
                    ret.Remove(ret.Length - 2, 2);
                }

                return ret.ToString();
            }
        }

        /** TODO: FIX THIS */
        public IEnumerable<Opening> OpenHours { get; set; }

        [JsonProperty("signature_cocktail")]
        public Feature SignatureCocktail { get; set; }

        [JsonProperty("features")]
        public IEnumerable<Feature> Features { get; set; }

        [JsonProperty("menu_url")]
        public string MenuUrl { get; set; }

        public static Bar Convert(Data.Bar input)
        {
            if (input == null) { return null; }
            return new Bar()
            {
                Id = input.Id,
                Name = input.Name,
                Intro = input.Intro,
                Description = input.Description,
                UrlKey = input.UrlKey,
                Url = input.Url,
                Website = input.Website,
                Email = input.Email,
                Phone = input.Phone,
                Fax = input.Fax,
                IsActive = input.IsActive,
                Address = string.IsNullOrEmpty(input.Address) ? new BarAddress() : new BarAddress()
                {
                    Street1 = input.AddressXml.Element("street1").Value,
                    Street2 = input.AddressXml.Element("street2").Value,
                    City = input.AddressXml.Element("city").Value,
                    //County = input.AddressXml.Element("county").Value,
                    Postcode = input.AddressXml.Element("postcode").Value,
                    Country = input.AddressXml.Element("country").Value
                },
                Geo = input.Geocoordinate == null ? null : new Geo()
                {
                    Lat = (double)input.Geocoordinate.Lat,
                    Long = (double)input.Geocoordinate.Long
                },
                Parent = input.Location == null ? null : new DatabaseObject()
                {
                    Id = input.Location.Id,
                    Name = input.Location.Name,
                    Url = input.Location.Url,
                    UrlKey = input.Location.UrlKey
                },
                SignatureCocktail = Feature.Convert(input.Features.FirstOrDefault(f => f.Type == "signature-cocktail" && f.IsActive)),
                Features = input.Features.ToList().Select(f => Feature.Convert(f)),
                MenuUrl = input.MenuUrl,
                OpenHours = input.BarOpenings.ToList().Select(o => Opening.Convert(o))
            };
        }

        public static Bar GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                return Convert(context.Bars.SingleOrDefault(b => b.Id == id));
            }
        }

        public static Bar[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                var result = context.Bars.ToList().Select(b => Convert(b)).ToArray();

                var expertReviews = context.Reviews.Where(r => r.User.IsExpert && r.IsActive).Select(r => r.BarId).ToArray();
                foreach (var bar in result.Where(b => expertReviews.Contains(b.Id)))
                {
                    bar.IsExpertsChoice = true;
                }

                var top100 = context.Categories.Single(c => c.Key == "top-100-bars").Bars.Select(b => b.Id).ToArray();
                foreach (var bar in result.Where(b => top100.Contains(b.Id)))
                {
                    bar.IsTop100 = true;
                }

                var mostTalkedAbout = context.Categories.Single(c => c.Key == "most-talked-about").Bars.Select(b => b.Id).ToArray();
                foreach (var bar in result.Where(b => mostTalkedAbout.Contains(b.Id)))
                {
                    bar.IsMostTalkedAbout = true;
                }

                return result;
            }
        }

        public static void Delete(Guid id)
        {
            using (var context = new Data.Entities())
            {
                context.ExecuteStoreCommand("delete from Related where LeftId = '{0}'", id);
                context.ExecuteStoreCommand("delete from Related where RightId = '{0}'", id);

                foreach (var review in context.Reviews.Where(r => r.BarId == id).ToArray())
                {
                    if (review.Rating != null) { context.Ratings.DeleteObject(review.Rating); }

                    context.Reviews.DeleteObject(review);
                }

                context.Bars.DeleteObject(context.Bars.Single(b => b.Id == id));

                context.SaveChanges();
            }
        }
    }
}