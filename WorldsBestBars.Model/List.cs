using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
    [JsonObject]
    public class List
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("searchable")]
        public bool Searchable { get; set; }

        [JsonProperty("system")]
        public bool System { get; set; }

        [JsonProperty("bars")]
        public DatabaseObject[] Bars { get; set; }

        [JsonProperty("documents")]
        public DatabaseObject[] Documents { get; set; }

        [JsonProperty("locations")]
        public DatabaseObject[] Locations { get; set; }

        public static List[] GetAll()
        {
            using (var context = new Data.Entities())
            {
                return context.Categories.ToList().Select(c => new List()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Key = c.Key,
                    Group = c.Group,
                    Searchable = c.IsSearchable,
                    System = c.IsSystem,
                    Bars = c.Bars.Select(b => new DatabaseObject() { Id = b.Id, Name = b.Name, Url = b.Url, UrlKey = b.UrlKey }).OrderBy(b => b, new OrderableListComparer(c.Order == null ? null : XElement.Parse(c.Order))).ToArray(),
                    Documents = c.Documents.Select(d => new DatabaseObject() { Id = d.Id, Name = d.Name, Url = d.Url, UrlKey = d.UrlKey }).OrderBy(d => d, new OrderableListComparer(c.Order == null ? null : XElement.Parse(c.Order))).ToArray(),
                    Locations = c.Locations.Select(l => new DatabaseObject() { Id = l.Id, Name = l.Name, Url = l.Url, UrlKey = l.UrlKey }).OrderBy(l => l, new OrderableListComparer(c.Order == null ? null : XElement.Parse(c.Order))).ToArray()
                }).ToArray();
            }
        }

        public static List GetById(Guid id)
        {
            using (var context = new Data.Entities())
            {
                var category = context.Categories.Single(c => c.Id == id);
                return new List()
                {
                    Id = category.Id,
                    Name = category.Name,
                    Key = category.Key,
                    Group = category.Group,
                    Searchable = category.IsSearchable,
                    System = category.IsSystem,
                    Bars = category.Bars.Select(b => new DatabaseObject() { Id = b.Id, Name = b.Name, Url = b.Url, UrlKey = b.UrlKey }).OrderBy(b => b, new OrderableListComparer(category.Order == null ? null : XElement.Parse(category.Order))).ToArray(),
                    Documents = category.Documents.Select(d => new DatabaseObject() { Id = d.Id, Name = d.Name, Url = d.Url, UrlKey = d.UrlKey }).OrderBy(d => d, new OrderableListComparer(category.Order == null ? null : XElement.Parse(category.Order))).ToArray(),
                    Locations = category.Locations.Select(l => new DatabaseObject() { Id = l.Id, Name = l.Name, Url = l.Url, UrlKey = l.UrlKey }).OrderBy(l => l, new OrderableListComparer(category.Order == null ? null : XElement.Parse(category.Order))).ToArray()
                };
            }
        }

        public static void AddToList(Guid list, string type, Guid entity)
        {
            using (var context = new Data.Entities())
            {
                var category = context.Categories.Single(c => c.Id == list);
                switch (type)
                {
                    case "Bar":
                        category.Bars.Add(context.Bars.Single(b => b.Id == entity));
                        break;
                    case "Location":
                        category.Locations.Add(context.Locations.Single(l => l.Id == entity));
                        break;
                    case "Document":
                        category.Documents.Add(context.Documents.Single(d => d.Id == entity));
                        break;
                }

                context.SaveChanges();
            }
        }

        public static void ClearList(Guid list)
        {
            using (var context = new Data.Entities())
            {
                var category = context.Categories.Single(c => c.Id == list);

                foreach (var bar in category.Bars.ToArray())
                {
                    category.Bars.Attach(bar);
                    category.Bars.Remove(bar);
                }

                foreach (var location in category.Locations.ToArray())
                {
                    category.Locations.Attach(location);
                    category.Locations.Remove(location);
                }

                foreach (var doc in category.Documents.ToArray())
                {
                    category.Documents.Attach(doc);
                    category.Documents.Remove(doc);
                }

                context.SaveChanges();
            }
        }

        public static void UpdateOrder(Guid list, IEnumerable<Guid> order)
        {
            using (var context = new Data.Entities())
            {
                var category = context.Categories.Single(c => c.Id == list);
                if (order == null)
                {
                    category.Order = null;
                }
                else
                {
                    category.Order = new XElement("order", order.Select(e => new XElement("entity", new XAttribute("id", e)))).ToString();
                }

                context.SaveChanges();
            }
        }
    }

    public class OrderableListComparer : IComparer<DatabaseObject>
    {
        List<Guid> order;

        public OrderableListComparer(XElement order)
        {
            this.order = order == null ? null : new List<Guid>(order.Elements("entity").Select(e => Guid.Parse(e.Attribute("id").Value)));
        }

        public int Compare(DatabaseObject x, DatabaseObject y)
        {
            if (this.order == null)
            {
                return 0;
            }

            return order.IndexOf(x.Id) - order.IndexOf(y.Id);
        }
    }
}