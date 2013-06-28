using System;

namespace WorldsBestBars.Services.Models
{
    public enum EntityType
    {
        Bar,
        Location,
        Document
    }

    public class Entity
    {
        public Guid Id { get; set; }
    }
}
