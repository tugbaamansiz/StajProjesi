using NetTopologySuite.Geometries;

namespace StajProjesi.API.Models
{
    public class PolygonFeature
    {
        public int Id { get; set; }

        public Geometry Geometry { get; set; } = null!;

        public string? Name { get; set; }

        public string? Color { get; set; }

        public int InsertedUserId { get; set; }

        public DateTime InsertedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}