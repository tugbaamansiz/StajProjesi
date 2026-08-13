using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using StajProjesi.API.Data;
using StajProjesi.API.Models;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolygonFeaturesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PolygonFeaturesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PolygonFeatures
        [HttpGet]
        public async Task<IActionResult> GetPolygons()
        {
            var polygons = await _context.Polygons.ToListAsync();

            var result = polygons.Select(polygon => new
            {
                id = polygon.Id,
                type = "Polygon",
                wkt = polygon.Geometry.AsText(),
                coordinates = polygon.Geometry.Coordinates.Select(c => new
                {
                    longitude = c.X,
                    latitude = c.Y
                }).ToList()
            });

            return Ok(result);
        }

        // GET: api/PolygonFeatures/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPolygon(int id)
        {
            var polygon = await _context.Polygons.FindAsync(id);

            if (polygon == null)
                return NotFound();

            return Ok(new
            {
                id = polygon.Id,
                type = "Polygon",
                wkt = polygon.Geometry.AsText(),
                coordinates = polygon.Geometry.Coordinates.Select(c => new
                {
                    longitude = c.X,
                    latitude = c.Y
                }).ToList()
            });
        }

        // POST: api/PolygonFeatures
        [HttpPost]
        public async Task<IActionResult> CreatePolygon(PolygonDto dto)
        {
            if (dto.Coordinates == null || dto.Coordinates.Count < 3)
                return BadRequest("Polygon için en az 3 nokta gerekli.");

            var coordinates = dto.Coordinates
                .Select(c => new Coordinate(c.Longitude, c.Latitude))
                .ToList();

            // Polygon kapanması için ilk noktayı sona ekle
            if (coordinates.First().X != coordinates.Last().X ||
                coordinates.First().Y != coordinates.Last().Y)
            {
                coordinates.Add(coordinates.First());
            }

            var ring = new LinearRing(coordinates.ToArray());

            var polygon = new Polygon(ring)
            {
                SRID = 4326
            };

            var entity = new PolygonFeature
            {
                Geometry = polygon
            };

            _context.Polygons.Add(entity);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = entity.Id,
                type = "Polygon",
                wkt = entity.Geometry.AsText(),
                coordinates = dto.Coordinates
            });
        }

        // DELETE: api/PolygonFeatures/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolygon(int id)
        {
            var polygon = await _context.Polygons.FindAsync(id);

            if (polygon == null)
                return NotFound();

            _context.Polygons.Remove(polygon);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class PolygonDto
    {
        public List<CoordinateDto> Coordinates { get; set; } = new();
    }
}