using System;
using System.Linq;

using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    public class InvalidCountryException : Exception
    {
    }

    public class CountryNotAvailableException : Exception
    {
    }

    [Serializable()]
    [JsonObject()]
    public class Country
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("shortiso")]
        public string ShortISO { get; set; }

        [JsonProperty("longiso")]
        public string LongISO { get; set; }

        [JsonProperty("minage")]
        public byte? MinAge { get; set; }

        public static Country Convert(Data.Country input)
        {
            if (input == null) { return null; }
            return new Country()
            {
                Id = input.Id,
                Name = input.Name,
                ShortISO = input.ShortISO,
                LongISO = input.LongISO,
                MinAge = input.MinAge
            };
        }

        public static Country[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                return context.Countries.OrderBy(c => c.Name).ToList().Select(c => Convert(c)).ToArray();
            }
        }

        public static Country GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                return Convert(context.Countries.SingleOrDefault(c => c.Id == id));
            }
        }

        public static Country GetByShortISO(string iso)
        {
            using (var context = new Data.Entities())
            {
                return Convert(context.Countries.SingleOrDefault(c => c.ShortISO == iso));
            }
        }

        public static bool VerifyAge(DateTime dob, Guid country)
        {
            using (var context = new Data.Entities())
            {
                if (!context.Countries.Any(c => c.Id == country))
                {
                    throw new InvalidCountryException();
                }

                var minAge = context.Countries.Single(c => c.Id == country).MinAge;

                if (minAge == null)
                {
                    // if no minimum age for the country, country is not available

                    throw new CountryNotAvailableException();
                }
                else
                {
                    return dob <= DateTime.Today.AddYears(-(int)minAge);
                }
            }
        }
    }
}
