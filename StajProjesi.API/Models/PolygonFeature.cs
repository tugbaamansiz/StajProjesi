using NetTopologySuite.Geometries;

namespace StajProjesi.API.Models
{
    public class PolygonFeature
    {
        public int Id { get; set; }

        public Geometry Geometry { get; set; } = null!;
    }
}