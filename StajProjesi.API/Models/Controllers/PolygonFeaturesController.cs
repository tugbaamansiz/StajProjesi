using Microsoft.AspNetCore.Mvc;
using StajProjesi.API.Models;
using StajProjesi.API.Services;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolygonFeaturesController : ControllerBase
    {
        private readonly IPolygonFeatureService _polygonService;

        public PolygonFeaturesController(
            IPolygonFeatureService polygonService)
        {
            _polygonService = polygonService;
        }

        // GET: api/PolygonFeatures
        [HttpGet]
        public async Task<IActionResult> GetPolygons()
        {
            var polygons = await _polygonService.GetPolygonsAsync();

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

        // POST: api/PolygonFeatures
        [HttpPost]
        public async Task<IActionResult> CreatePolygon(PolygonDto dto)
        {
            if (dto.Coordinates == null || dto.Coordinates.Count < 3)
                return BadRequest("Polygon için en az 3 nokta gerekli.");

            var polygon = await _polygonService.CreatePolygonAsync(
                dto.Coordinates);

            return Ok(new
            {
                id = polygon.Id,
                type = "Polygon",
                coordinates = dto.Coordinates
            });
        }

        // DELETE: api/PolygonFeatures/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolygon(int id)
        {
            var deleted = await _polygonService.DeletePolygonAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }

    public class PolygonDto
    {
        public List<CoordinateDto> Coordinates { get; set; } = new();
    }
}