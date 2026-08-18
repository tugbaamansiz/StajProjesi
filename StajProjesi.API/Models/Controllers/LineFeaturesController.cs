using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
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
        private readonly IPermissionService _permissionService;
        private readonly IGeographicPermissionService _geographicPermissionService;

        public LineFeaturesController(
            ILineFeatureService lineService,
            IPermissionService permissionService,
            IGeographicPermissionService geographicPermissionService)
        {
            _lineService = lineService;
            _permissionService = permissionService;
            _geographicPermissionService = geographicPermissionService;
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

                // =================================================
                // LINE EKLEME YETKİSİ
                // =================================================

                var hasPermission =
                    await _permissionService.HasPermissionAsync(
                        userId.Value,
                        "Line Ekleme");

                if (!hasPermission)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Line ekleme yetkiniz bulunmamaktadır."
                    });
                }

                if (dto.Coordinates == null ||
                    dto.Coordinates.Count < 2)
                {
                    return BadRequest(
                        "Çizgi için en az 2 nokta gerekli.");
                }

                // =================================================
                // LINE GEOMETRY OLUŞTUR
                // =================================================

                var coordinates =
                    dto.Coordinates
                        .Select(c =>
                            new Coordinate(
                                c.Longitude,
                                c.Latitude))
                        .ToArray();

                var lineGeometry =
                    new LineString(coordinates)
                    {
                        SRID = 4326
                    };

                // =================================================
                // COĞRAFİ YETKİ KONTROLÜ
                // =================================================

                var geoAllowed =
                    await _geographicPermissionService
                        .IsGeometryAllowedAsync(
                            userId.Value,
                            lineGeometry);

                if (!geoAllowed)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Bu çizgiyi oluşturmak için coğrafi yetkiniz bulunmamaktadır."
                    });
                }

                // =================================================
                // LINE OLUŞTUR
                // =================================================

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

                // =================================================
                // LINE GÜNCELLEME YETKİSİ
                // =================================================

                var hasPermission =
                    await _permissionService.HasPermissionAsync(
                        userId.Value,
                        "Line Güncelleme");

                if (!hasPermission)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Line güncelleme yetkiniz bulunmamaktadır."
                    });
                }

                if (dto.Coordinates == null ||
                    dto.Coordinates.Count < 2)
                {
                    return BadRequest(
                        "Çizgi için en az 2 nokta gerekli.");
                }

                // =================================================
                // YENİ LINE GEOMETRY
                // =================================================

                var coordinates =
                    dto.Coordinates
                        .Select(c =>
                            new Coordinate(
                                c.Longitude,
                                c.Latitude))
                        .ToArray();

                var lineGeometry =
                    new LineString(coordinates)
                    {
                        SRID = 4326
                    };

                // =================================================
                // COĞRAFİ YETKİ KONTROLÜ
                // =================================================

                var geoAllowed =
                    await _geographicPermissionService
                        .IsGeometryAllowedAsync(
                            userId.Value,
                            lineGeometry);

                if (!geoAllowed)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Çizgiyi bu konuma taşımak için coğrafi yetkiniz bulunmamaktadır."
                    });
                }

                // =================================================
                // LINE GÜNCELLE
                // =================================================

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

                // =================================================
                // LINE SİLME YETKİSİ
                // =================================================

                var hasPermission =
                    await _permissionService.HasPermissionAsync(
                        userId.Value,
                        "Line Silme");

                if (!hasPermission)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Line silme yetkiniz bulunmamaktadır."
                    });
                }

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

    // =========================================================
    // LINE DTO
    // =========================================================

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