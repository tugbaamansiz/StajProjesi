using Microsoft.AspNetCore.Mvc;
using StajProjesi.API.Models;
using StajProjesi.API.Services;
using System.Security.Claims;

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

        // =====================================================
        // GET - TÜM NOKTALAR
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetPoints()
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var points =
                    await _pointService.GetPointsAsync(
                        userId.Value);

                var result = points.Select(point => new
                {
                    id = point.Id,
                    type = "Point",
                    name = point.Name,
                    color = point.Color,
                    wkt = point.Geometry.AsText(),

                    coordinates = new
                    {
                        longitude =
                            point.Geometry.Coordinate.X,

                        latitude =
                            point.Geometry.Coordinate.Y
                    }
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Noktalar getirilirken bir hata oluştu.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // GET - TEK NOKTA
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPoint(int id)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var point =
                    await _pointService.GetPointAsync(
                        id,
                        userId.Value);

                if (point == null)
                    return NotFound();

                return Ok(new
                {
                    id = point.Id,
                    type = "Point",
                    name = point.Name,
                    color = point.Color,
                    wkt = point.Geometry.AsText(),

                    coordinates = new
                    {
                        longitude =
                            point.Geometry.Coordinate.X,

                        latitude =
                            point.Geometry.Coordinate.Y
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Nokta getirilirken bir hata oluştu.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // POST - NOKTA OLUŞTUR
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> CreatePoint(
            PointDto dto)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var point =
                    await _pointService.CreatePointAsync(
                        dto.Longitude,
                        dto.Latitude,
                        dto.Name,
                        dto.Color,
                        userId.Value);

                return Ok(new
                {
                    id = point.Id,
                    type = "Point",
                    name = point.Name,
                    color = point.Color,
                    wkt = point.Geometry.AsText(),

                    coordinates = new
                    {
                        longitude = dto.Longitude,
                        latitude = dto.Latitude
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Nokta oluşturulurken bir hata oluştu.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // PUT - NOKTA GÜNCELLE
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePoint(
            int id,
            PointDto dto)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var updated =
                    await _pointService.UpdatePointAsync(
                        id,
                        dto.Longitude,
                        dto.Latitude,
                        dto.Name,
                        dto.Color,
                        userId.Value);

                if (!updated)
                    return NotFound(new
                    {
                        message =
                            "Nokta bulunamadı veya bu noktayı güncelleme yetkiniz yok."
                    });

                return Ok(new
                {
                    message =
                        "Nokta başarıyla güncellendi."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Nokta güncellenirken bir hata oluştu.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // DELETE - SOFT DELETE
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePoint(
            int id)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var deleted =
                    await _pointService.DeletePointAsync(
                        id,
                        userId.Value);

                if (!deleted)
                    return NotFound();

                return Ok(new
                {
                    message =
                        "Nokta başarıyla silindi."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Nokta silinirken bir hata oluştu.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // JWT'DEN USER ID AL
        // =====================================================

        private int? GetUserId()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return userId;
            }

            return null;
        }


        // =====================================================
        // DTO
        // =====================================================

        public class PointDto
        {
            public double Longitude { get; set; }

            public double Latitude { get; set; }

            public string Name { get; set; }
                = "";

            public string Color { get; set; }
                = "#3388ff";
        }
    }
}