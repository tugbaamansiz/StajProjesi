using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajProjesi.API.Data;
using StajProjesi.API.Models;
using NetTopologySuite.Geometries;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PointFeaturesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PointFeaturesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PointFeatures
        [HttpGet]
        public async Task<IActionResult> GetPoints()
        {
            var points = await _context.Points.ToListAsync();

            var result = points.Select(point => new
            {
                id = point.Id,
                type = "Point",
                wkt = point.Geometry.AsText(),
                coordinates = new
                {
                    longitude = point.Geometry.Coordinate.X,
                    latitude = point.Geometry.Coordinate.Y
                }
            });

            return Ok(result);
        }

        // GET: api/PointFeatures/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPoint(int id)
        {
            var point = await _context.Points.FindAsync(id);

            if (point == null)
                return NotFound();

            return Ok(new
            {
                id = point.Id,
                type = "Point",
                wkt = point.Geometry.AsText(),
                coordinates = new
                {
                    longitude = point.Geometry.Coordinate.X,
                    latitude = point.Geometry.Coordinate.Y
                }
            });
        }

        // POST: api/PointFeatures
        [HttpPost]
        public async Task<IActionResult> CreatePoint(PointDto dto)
        {
            var point = new PointFeature
            {
                Geometry = new Point(dto.Longitude, dto.Latitude)
                {
                    SRID = 4326
                }
            };

            _context.Points.Add(point);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = point.Id,
                type = "Point",
                wkt = point.Geometry.AsText(),
                longitude = dto.Longitude,
                latitude = dto.Latitude
            });
        }

        // DELETE: api/PointFeatures/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePoint(int id)
        {
            var point = await _context.Points.FindAsync(id);

            if (point == null)
                return NotFound();

            _context.Points.Remove(point);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        public class PointDto
        {
            public double Longitude { get; set; }

            public double Latitude { get; set; }
        }
    }
}