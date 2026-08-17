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

        public async Task<List<PointFeature>> GetPointsAsync(
            int userId)
        {
            return await _context.Points
                .Where(p =>
                    p.InsertedUserId == userId &&
                    !p.IsDeleted &&
                    p.IsActive)
                .ToListAsync();
        }

        public async Task<PointFeature?> GetPointAsync(
            int id,
            int userId)
        {
            return await _context.Points
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.InsertedUserId == userId &&
                    !p.IsDeleted &&
                    p.IsActive);
        }

        public async Task<PointFeature> CreatePointAsync(
            double longitude,
            double latitude,
            string name,
            string color,
            int userId)
        {
            var point = new PointFeature
            {
                Geometry = new Point(
                    longitude,
                    latitude)
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

            _context.Points.Add(point);

            await _context.SaveChangesAsync();

            return point;
        }

        public async Task<bool> UpdatePointAsync(
            int id,
            double longitude,
            double latitude,
            string name,
            string color,
            int userId)
        {
            var point = await _context.Points
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.InsertedUserId == userId &&
                    !p.IsDeleted &&
                    p.IsActive);

            if (point == null)
                return false;

            point.Geometry = new Point(
                longitude,
                latitude)
            {
                SRID = 4326
            };

            point.Name = name;
            point.Color = color;
            point.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeletePointAsync(
            int id,
            int userId)
        {
            var point = await _context.Points
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.InsertedUserId == userId &&
                    !p.IsDeleted);

            if (point == null)
                return false;

            // Soft Delete
            point.IsDeleted = true;
            point.IsActive = false;
            point.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}