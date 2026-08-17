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

        public async Task<List<PolygonFeature>> GetPolygonsAsync(
            int userId)
        {
            return await _context.Polygons
                .Where(p =>
                    p.InsertedUserId == userId &&
                    !p.IsDeleted &&
                    p.IsActive)
                .ToListAsync();
        }

        public async Task<PolygonFeature> CreatePolygonAsync(
            List<CoordinateDto> coordinates,
            string name,
            string color,
            int userId)
        {
            var geometryCoordinates = coordinates
                .Select(c => new Coordinate(
                    c.Longitude,
                    c.Latitude))
                .ToList();

            if (
                geometryCoordinates.First().X !=
                    geometryCoordinates.Last().X ||
                geometryCoordinates.First().Y !=
                    geometryCoordinates.Last().Y
            )
            {
                geometryCoordinates.Add(
                    geometryCoordinates.First());
            }

            var ring = new LinearRing(
                geometryCoordinates.ToArray());

            var polygon = new Polygon(ring)
            {
                SRID = 4326
            };

            var polygonFeature = new PolygonFeature
            {
                Geometry = polygon,

                Name = name,
                Color = color,

                InsertedUserId = userId,
                InsertedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false,
                IsActive = true
            };

            _context.Polygons.Add(polygonFeature);

            await _context.SaveChangesAsync();

            return polygonFeature;
        }

        public async Task<bool> UpdatePolygonAsync(
            int id,
            List<CoordinateDto> coordinates,
            string name,
            string color,
            int userId)
        {
            var polygonFeature =
                await _context.Polygons
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.InsertedUserId == userId &&
                        !p.IsDeleted &&
                        p.IsActive);

            if (polygonFeature == null)
                return false;

            if (coordinates == null ||
                coordinates.Count < 3)
            {
                return false;
            }

            var geometryCoordinates = coordinates
                .Select(c => new Coordinate(
                    c.Longitude,
                    c.Latitude))
                .ToList();

            // Polygon kapanmamışsa kapat
            if (
                geometryCoordinates.First().X !=
                    geometryCoordinates.Last().X ||
                geometryCoordinates.First().Y !=
                    geometryCoordinates.Last().Y
            )
            {
                geometryCoordinates.Add(
                    geometryCoordinates.First());
            }

            var ring = new LinearRing(
                geometryCoordinates.ToArray());

            polygonFeature.Geometry = new Polygon(ring)
            {
                SRID = 4326
            };

            polygonFeature.Name = name;
            polygonFeature.Color = color;
            polygonFeature.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeletePolygonAsync(
            int id,
            int userId)
        {
            var polygon = await _context.Polygons
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.InsertedUserId == userId &&
                    !p.IsDeleted);

            if (polygon == null)
                return false;

            // Soft Delete
            polygon.IsDeleted = true;
            polygon.IsActive = false;
            polygon.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}