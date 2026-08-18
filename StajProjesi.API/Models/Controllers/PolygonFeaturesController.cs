using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using StajProjesi.API.Models;
using StajProjesi.API.Services;
using System.Security.Claims;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolygonFeaturesController : ControllerBase
    {
        private readonly IPolygonFeatureService _polygonService;
        private readonly IPermissionService _permissionService;
        private readonly IGeographicPermissionService _geographicPermissionService;

        public PolygonFeaturesController(
            IPolygonFeatureService polygonService,
            IPermissionService permissionService,
            IGeographicPermissionService geographicPermissionService)
        {
            _polygonService = polygonService;
            _permissionService = permissionService;
            _geographicPermissionService = geographicPermissionService;
        }

        // =====================================================
        // GET - TÜM POLYGONLAR
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetPolygons()
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var polygons =
                    await _polygonService.GetPolygonsAsync(
                        userId.Value);

                var result = polygons.Select(polygon => new
                {
                    id = polygon.Id,
                    type = "Polygon",
                    name = polygon.Name,
                    color = polygon.Color,
                    wkt = polygon.Geometry.AsText(),

                    coordinates =
                        polygon.Geometry.Coordinates.Select(c => new
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
                        "Polygonlar getirilirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // =====================================================
        // POST - POLYGON OLUŞTUR
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> CreatePolygon(
            PolygonDto dto)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                // =====================================================
                // POLYGON EKLEME YETKİSİ
                // =====================================================

                var hasPermission =
                    await _permissionService.HasPermissionAsync(
                        userId.Value,
                        "Polygon Ekleme");

                if (!hasPermission)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Polygon ekleme yetkiniz bulunmamaktadır."
                    });
                }

                if (dto.Coordinates == null ||
                    dto.Coordinates.Count < 3)
                {
                    return BadRequest(
                        "Polygon için en az 3 nokta gerekli.");
                }

                // =====================================================
                // POLYGON GEOMETRY OLUŞTUR
                // =====================================================

                var coordinates =
                    dto.Coordinates
                        .Select(c =>
                            new Coordinate(
                                c.Longitude,
                                c.Latitude))
                        .ToList();

                // Polygon'un kapanması gerekir.
                var first = coordinates.First();
                var last = coordinates.Last();

                if (!first.Equals2D(last))
                {
                    coordinates.Add(
                        new Coordinate(
                            first.X,
                            first.Y));
                }

                var geometryFactory =
                    NetTopologySuite.NtsGeometryServices.Instance
                        .CreateGeometryFactory(
                            srid: 4326);

                var linearRing =
                    geometryFactory.CreateLinearRing(
                        coordinates.ToArray());

                var polygonGeometry =
                    geometryFactory.CreatePolygon(
                        linearRing);

                // =====================================================
                // GEOMETRY VALIDATION
                // =====================================================

                if (!polygonGeometry.IsValid)
                {
                    return BadRequest(new
                    {
                        message =
                            "Çizilen polygon geçerli değil."
                    });
                }

                // =====================================================
                // COĞRAFİ YETKİ KONTROLÜ
                // =====================================================
                //
                // Kullanıcının çizdiği polygonun tamamı,
                // tanımlı coğrafi yetki alanlarından birinin
                // içinde olmalıdır.
                // =====================================================

                var geoAllowed =
                    await _geographicPermissionService
                        .IsGeometryAllowedAsync(
                            userId.Value,
                            polygonGeometry);

                if (!geoAllowed)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Bu polygonu çizmek için coğrafi yetkiniz bulunmamaktadır."
                    });
                }

                // =====================================================
                // POLYGON OLUŞTUR
                // =====================================================

                var polygon =
                    await _polygonService.CreatePolygonAsync(
                        dto.Coordinates,
                        dto.Name,
                        dto.Color,
                        userId.Value);

                return Ok(new
                {
                    id = polygon.Id,
                    type = "Polygon",
                    name = polygon.Name,
                    color = polygon.Color,
                    coordinates = dto.Coordinates
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Polygon oluşturulurken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // =====================================================
        // PUT - POLYGON GÜNCELLE
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePolygon(
            int id,
            PolygonDto dto)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                // =====================================================
                // POLYGON GÜNCELLEME YETKİSİ
                // =====================================================

                var hasPermission =
                    await _permissionService.HasPermissionAsync(
                        userId.Value,
                        "Polygon Güncelleme");

                if (!hasPermission)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Polygon güncelleme yetkiniz bulunmamaktadır."
                    });
                }

                if (dto.Coordinates == null ||
                    dto.Coordinates.Count < 3)
                {
                    return BadRequest(
                        "Polygon için en az 3 nokta gerekli.");
                }

                // =====================================================
                // YENİ POLYGON GEOMETRY
                // =====================================================

                var coordinates =
                    dto.Coordinates
                        .Select(c =>
                            new Coordinate(
                                c.Longitude,
                                c.Latitude))
                        .ToList();

                var first = coordinates.First();
                var last = coordinates.Last();

                if (!first.Equals2D(last))
                {
                    coordinates.Add(
                        new Coordinate(
                            first.X,
                            first.Y));
                }

                var geometryFactory =
                    NetTopologySuite.NtsGeometryServices.Instance
                        .CreateGeometryFactory(
                            srid: 4326);

                var linearRing =
                    geometryFactory.CreateLinearRing(
                        coordinates.ToArray());

                var polygonGeometry =
                    geometryFactory.CreatePolygon(
                        linearRing);

                // =====================================================
                // GEOMETRY VALIDATION
                // =====================================================

                if (!polygonGeometry.IsValid)
                {
                    return BadRequest(new
                    {
                        message =
                            "Güncellenen polygon geçerli değil."
                    });
                }

                // =====================================================
                // COĞRAFİ YETKİ KONTROLÜ
                // =====================================================

                var geoAllowed =
                    await _geographicPermissionService
                        .IsGeometryAllowedAsync(
                            userId.Value,
                            polygonGeometry);

                if (!geoAllowed)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Polygonu bu alana taşımak için coğrafi yetkiniz bulunmamaktadır."
                    });
                }

                // =====================================================
                // POLYGON GÜNCELLE
                // =====================================================

                var updated =
                    await _polygonService.UpdatePolygonAsync(
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
                            "Polygon bulunamadı veya bu polygonu güncelleme yetkiniz yok."
                    });
                }

                return Ok(new
                {
                    message =
                        "Polygon başarıyla güncellendi."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Polygon güncellenirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // =====================================================
        // DELETE - SOFT DELETE
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolygon(
            int id)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                // =====================================================
                // POLYGON SİLME YETKİSİ
                // =====================================================

                var hasPermission =
                    await _permissionService.HasPermissionAsync(
                        userId.Value,
                        "Polygon Silme");

                if (!hasPermission)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Polygon silme yetkiniz bulunmamaktadır."
                    });
                }

                var deleted =
                    await _polygonService.DeletePolygonAsync(
                        id,
                        userId.Value);

                if (!deleted)
                    return NotFound();

                return Ok(new
                {
                    message =
                        "Polygon başarıyla silindi."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Polygon silinirken bir hata oluştu.",
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
    // POLYGON DTO
    // =========================================================

    public class PolygonDto
    {
        public List<CoordinateDto> Coordinates { get; set; }
            = new();

        public string Name { get; set; }
            = "";

        public string Color { get; set; }
            = "#3388ff";
    }
}