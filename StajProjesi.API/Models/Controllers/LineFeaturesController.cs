using Microsoft.AspNetCore.Mvc;
using StajProjesi.API.Models;
using StajProjesi.API.Services;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LineFeaturesController : ControllerBase
    {
        private readonly ILineFeatureService _lineService;

        public LineFeaturesController(
            ILineFeatureService lineService)
        {
            _lineService = lineService;
        }

        // GET: api/LineFeatures
        [HttpGet]
        public async Task<IActionResult> GetLines()
        {
            var lines = await _lineService.GetLinesAsync();

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

            var line = await _lineService.CreateLineAsync(
                dto.Coordinates);

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
            var deleted = await _lineService.DeleteLineAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }

    public class LineDto
    {
        public List<CoordinateDto> Coordinates { get; set; } = new();
    }
}