using System;
using System.Linq;

using Newtonsoft.Json;

namespace WorldsBestBars.Model.JSON.Autocomplete
{
    [JsonObject()]
    public class Bar
    {
        [JsonProperty("value")]
        public Guid Id { get; set; }
        [JsonProperty("label")]
        public string Name { get; set; }

        public static implicit operator Bar(Data.Bar bar)
        {
            return new Bar()
            {
                Id = bar.Id,
                Name = bar.Name
            };
        }

        public static Bar[] Search(string term)
        {
            using (var context = new Data.Entities())
            {
                return context.Bars
                    .Where(b => b.Name.ToLower().Contains(term.ToLower()))
                    .OrderBy(b => b.Name)
                    .Take(15)
                    .ToList()
                    .Select(b => (Bar)b)
                    .ToArray();
            }
        }
    }
}
