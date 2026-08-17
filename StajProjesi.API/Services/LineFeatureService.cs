using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using StajProjesi.API.Data;
using StajProjesi.API.Models;

namespace StajProjesi.API.Services
{
    public class LineFeatureService : ILineFeatureService
    {
        private readonly AppDbContext _context;

        public LineFeatureService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LineFeature>> GetLinesAsync(
            int userId)
        {
            return await _context.Lines
                .Where(l =>
                    l.InsertedUserId == userId &&
                    !l.IsDeleted &&
                    l.IsActive)
                .ToListAsync();
        }

        public async Task<LineFeature> CreateLineAsync(
            List<CoordinateDto> coordinates,
            string name,
            string color,
            int userId)
        {
            var geometryCoordinates = coordinates
                .Select(c => new Coordinate(
                    c.Longitude,
                    c.Latitude))
                .ToArray();

            var line = new LineFeature
            {
                Geometry = new LineString(
                    geometryCoordinates)
                {
                    SRID = 4326
                },

                Name = name,
                Color = color,

                InsertedUserId = userId,
                InsertedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false,
                IsActive = true
            };

            _context.Lines.Add(line);

            await _context.SaveChangesAsync();

            return line;
        }

        public async Task<bool> UpdateLineAsync(
            int id,
            List<CoordinateDto> coordinates,
            string name,
            string color,
            int userId)
        {
            var line = await _context.Lines
                .FirstOrDefaultAsync(l =>
                    l.Id == id &&
                    l.InsertedUserId == userId &&
                    !l.IsDeleted &&
                    l.IsActive);

            if (line == null)
                return false;

            if (coordinates == null ||
                coordinates.Count < 2)
            {
                return false;
            }

            var geometryCoordinates = coordinates
                .Select(c => new Coordinate(
                    c.Longitude,
                    c.Latitude))
                .ToArray();

            line.Geometry = new LineString(
                geometryCoordinates)
            {
                SRID = 4326
            };

            line.Name = name;
            line.Color = color;
            line.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteLineAsync(
            int id,
            int userId)
        {
            var line = await _context.Lines
                .FirstOrDefaultAsync(l =>
                    l.Id == id &&
                    l.InsertedUserId == userId &&
                    !l.IsDeleted);

            if (line == null)
                return false;

            // Soft Delete
            line.IsDeleted = true;
            line.IsActive = false;
            line.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}