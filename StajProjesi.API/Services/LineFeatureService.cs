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

        public async Task<List<LineFeature>> GetLinesAsync()
        {
            return await _context.Lines.ToListAsync();
        }

        public async Task<LineFeature> CreateLineAsync(
            List<CoordinateDto> coordinates)
        {
            var geometryCoordinates = coordinates
                .Select(c => new Coordinate(c.Longitude, c.Latitude))
                .ToArray();

            var line = new LineFeature
            {
                Geometry = new LineString(geometryCoordinates)
                {
                    SRID = 4326
                }
            };

            _context.Lines.Add(line);

            await _context.SaveChangesAsync();

            return line;
        }

        public async Task<bool> DeleteLineAsync(int id)
        {
            var line = await _context.Lines.FindAsync(id);

            if (line == null)
                return false;

            _context.Lines.Remove(line);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}