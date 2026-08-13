using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using StajProjesi.API.Data;
using StajProjesi.API.Models;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LineFeaturesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LineFeaturesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/LineFeatures
        [HttpGet]
        public async Task<IActionResult> GetLines()
        {
            var lines = await _context.Lines.ToListAsync();

            var result = lines.Select(line => new
{
    id = line.Id,
    type = "LineString",
    wkt = line.Geometry.AsText(),
    coordinates = line.Geometry.Coordinates.Select(c => new
    {
        longitude = c.X,
        latitude = c.Y
    }).ToList()
});

            return Ok(result);
        }

        // POST: api/LineFeatures
        [HttpPost]
        public async Task<IActionResult> CreateLine(LineDto dto)
        {
            if (dto.Coordinates == null || dto.Coordinates.Count < 2)
                return BadRequest("Çizgi için en az 2 nokta gerekli.");

            var coordinates = dto.Coordinates
                .Select(c => new Coordinate(c.Longitude, c.Latitude))
                .ToArray();

            var line = new LineFeature
            {
                Geometry = new LineString(coordinates)
                {
                    SRID = 4326
                }
            };

            _context.Lines.Add(line);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = line.Id,
                type = "LineString",
                coordinates = dto.Coordinates
            });
        }

        // DELETE: api/LineFeatures/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLine(int id)
        {
            var line = await _context.Lines.FindAsync(id);

            if (line == null)
                return NotFound();

            _context.Lines.Remove(line);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class LineDto
    {
        public List<CoordinateDto> Coordinates { get; set; } = new();
    }
}