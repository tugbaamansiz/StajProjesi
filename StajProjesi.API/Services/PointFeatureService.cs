using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using StajProjesi.API.Data;
using StajProjesi.API.Models;

namespace StajProjesi.API.Services
{
    public class PointFeatureService : IPointFeatureService
    {
        private readonly AppDbContext _context;

        public PointFeatureService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PointFeature>> GetPointsAsync()
        {
            return await _context.Points.ToListAsync();
        }

        public async Task<PointFeature?> GetPointAsync(int id)
        {
            return await _context.Points.FindAsync(id);
        }

        public async Task<PointFeature> CreatePointAsync(
            double longitude,
            double latitude)
        {
            var point = new PointFeature
            {
                Geometry = new Point(longitude, latitude)
                {
                    SRID = 4326
                }
            };

            _context.Points.Add(point);

            await _context.SaveChangesAsync();

            return point;
        }

        public async Task<bool> DeletePointAsync(int id)
        {
            var point = await _context.Points.FindAsync(id);

            if (point == null)
                return false;

            _context.Points.Remove(point);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}