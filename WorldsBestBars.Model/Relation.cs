using System;
using System.Linq;

using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [JsonObject]
    public class Relation
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("relations")]
        public Guid[] Relations { get; set; }

        public static Relation[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                return context.Relateds
                    .GroupBy(g => g.LeftId)
                    .ToList()
                    .Select(g => new Relation()
                    {
                        Id = g.Key,
                        Relations = g.Select(r => r.RightId).ToArray()
                    })
                    .ToArray();
            }
        }

        public static Relation GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                return new Relation()
                {
                    Id = id,
                    Relations = context.Relateds.Where(r => r.LeftId == id).Select(r => r.RightId).ToArray()
                };
            }
        }
    }
}