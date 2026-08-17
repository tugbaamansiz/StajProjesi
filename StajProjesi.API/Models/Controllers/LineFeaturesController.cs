using Microsoft.AspNetCore.Mvc;
using StajProjesi.API.Models;
using StajProjesi.API.Services;
using System.Security.Claims;

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

        // =====================================================
        // GET - TÜM ÇİZGİLER
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetLines()
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var lines =
                    await _lineService.GetLinesAsync(
                        userId.Value);

                var result = lines.Select(line => new
                {
                    id = line.Id,
                    type = "LineString",
                    name = line.Name,
                    color = line.Color,
                    wkt = line.Geometry.AsText(),

                    coordinates =
                        line.Geometry.Coordinates.Select(c => new
                        {
                            longitude = c.X,
                            latitude = c.Y
                        }).ToList()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Çizgiler getirilirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // =====================================================
        // POST - ÇİZGİ OLUŞTUR
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> CreateLine(
            LineDto dto)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                if (dto.Coordinates == null ||
                    dto.Coordinates.Count < 2)
                {
                    return BadRequest(
                        "Çizgi için en az 2 nokta gerekli.");
                }

                var line =
                    await _lineService.CreateLineAsync(
                        dto.Coordinates,
                        dto.Name,
                        dto.Color,
                        userId.Value);

                return Ok(new
                {
                    id = line.Id,
                    type = "LineString",
                    name = line.Name,
                    color = line.Color,
                    coordinates = dto.Coordinates
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Çizgi oluşturulurken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // =====================================================
        // PUT - ÇİZGİ GÜNCELLE
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLine(
            int id,
            LineDto dto)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                if (dto.Coordinates == null ||
                    dto.Coordinates.Count < 2)
                {
                    return BadRequest(
                        "Çizgi için en az 2 nokta gerekli.");
                }

                var updated =
                    await _lineService.UpdateLineAsync(
                        id,
                        dto.Coordinates,
                        dto.Name,
                        dto.Color,
                        userId.Value);

                if (!updated)
                {
                    return NotFound(new
                    {
                        message =
                            "Çizgi bulunamadı veya bu çizgiyi güncelleme yetkiniz yok."
                    });
                }

                return Ok(new
                {
                    message =
                        "Çizgi başarıyla güncellendi."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Çizgi güncellenirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // =====================================================
        // DELETE - SOFT DELETE
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLine(
            int id)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var deleted =
                    await _lineService.DeleteLineAsync(
                        id,
                        userId.Value);

                if (!deleted)
                    return NotFound();

                return Ok(new
                {
                    message =
                        "Çizgi başarıyla silindi."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Çizgi silinirken bir hata oluştu.",
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
    }

    public class LineDto
    {
        public List<CoordinateDto> Coordinates { get; set; }
            = new();

        public string Name { get; set; }
            = "";

        public string Color { get; set; }
            = "#3388ff";
    }
}