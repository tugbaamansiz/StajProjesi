using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using StajProjesi.API.Data;
using StajProjesi.API.Models;

namespace StajProjesi.API.Services
{
    public class PolygonFeatureService : IPolygonFeatureService
    {
        private readonly AppDbContext _context;

        public PolygonFeatureService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PolygonFeature>> GetPolygonsAsync()
        {
            return await _context.Polygons.ToListAsync();
        }

        public async Task<PolygonFeature> CreatePolygonAsync(
            List<CoordinateDto> coordinates)
        {
            var geometryCoordinates = coordinates
                .Select(c => new Coordinate(c.Longitude, c.Latitude))
                .ToList();

            // Polygon kapanması için ilk noktayı sona ekle
            if (geometryCoordinates.Count > 0 &&
                !geometryCoordinates.First().Equals2D(geometryCoordinates.Last()))
            {
                geometryCoordinates.Add(geometryCoordinates.First());
            }

            var polygon = new Polygon(
                new LinearRing(geometryCoordinates.ToArray()))
            {
                SRID = 4326
            };

            var feature = new PolygonFeature
            {
                Geometry = polygon
            };

            _context.Polygons.Add(feature);

            await _context.SaveChangesAsync();

            return feature;
        }

        public async Task<bool> DeletePolygonAsync(int id)
        {
            var polygon = await _context.Polygons.FindAsync(id);

            if (polygon == null)
                return false;

            _context.Polygons.Remove(polygon);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}