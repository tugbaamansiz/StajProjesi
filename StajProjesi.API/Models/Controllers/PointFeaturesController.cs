using Microsoft.AspNetCore.Mvc;
using StajProjesi.API.Models;
using StajProjesi.API.Services;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PointFeaturesController : ControllerBase
    {
        private readonly IPointFeatureService _pointService;

        public PointFeaturesController(
            IPointFeatureService pointService)
        {
            _pointService = pointService;
        }

        // GET: api/PointFeatures
        [HttpGet]
        public async Task<IActionResult> GetPoints()
        {
            var points = await _pointService.GetPointsAsync();

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
            var point = await _pointService.GetPointAsync(id);

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
            var point = await _pointService.CreatePointAsync(
                dto.Longitude,
                dto.Latitude
            );

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
            var deleted = await _pointService.DeletePointAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        public class PointDto
        {
            public double Longitude { get; set; }

            public double Latitude { get; set; }
        }
    }
}