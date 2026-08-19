using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using StajProjesi.API.Models;
using StajProjesi.API.Services;
using System.Security.Claims;
using System.Text.Json;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolygonFeaturesController : ControllerBase
    {
        private readonly IPolygonFeatureService _polygonService;
        private readonly IPermissionService _permissionService;
        private readonly IGeographicPermissionService _geographicPermissionService;
        private readonly IGeoServerService _geoServerService;

        public PolygonFeaturesController(
            IPolygonFeatureService polygonService,
            IPermissionService permissionService,
            IGeographicPermissionService geographicPermissionService,
            IGeoServerService geoServerService)
        {
            _polygonService = polygonService;
            _permissionService = permissionService;
            _geographicPermissionService = geographicPermissionService;
            _geoServerService = geoServerService;
        }

        // =====================================================
        // GET - TÜM POLYGONLAR
        // ARTIK DOĞRUDAN DATABASE DEĞİL,
        // GEOSERVER ÜZERİNDEN GELİYOR
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetPolygons()
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var geoServerResponse =
                    await _geoServerService.GetFeaturesAsync(
                        "tbl_polygon",
                        userId.Value);

                var result =
                    ParseGeoServerFeatures(
                        geoServerResponse);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Polygonlar GeoServer üzerinden getirilirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // =====================================================
        // GET - TEK POLYGON
        // ARTIK DOĞRUDAN DATABASE DEĞİL,
        // GEOSERVER ÜZERİNDEN GELİYOR
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPolygon(int id)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var geoServerResponse =
                    await _geoServerService.GetFeatureAsync(
                        "tbl_polygon",
                        id,
                        userId.Value);

                if (string.IsNullOrWhiteSpace(
                    geoServerResponse))
                {
                    return NotFound();
                }

                var result =
                    ParseGeoServerFeatures(
                        geoServerResponse);

                if (result.Count == 0)
                    return NotFound();

                return Ok(result[0]);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Polygon GeoServer üzerinden getirilirken bir hata oluştu.",
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
        // GEOSERVER GEOJSON PARSER
        // =====================================================

        private List<object> ParseGeoServerFeatures(
            string json)
        {
            var result =
                new List<object>();

            using var document =
                JsonDocument.Parse(json);

            var root =
                document.RootElement;

            if (!root.TryGetProperty(
                "features",
                out var features))
            {
                return result;
            }

            foreach (var feature
                in features.EnumerateArray())
            {
                if (!feature.TryGetProperty(
                    "properties",
                    out var properties))
                {
                    continue;
                }

                if (!feature.TryGetProperty(
                    "geometry",
                    out var geometry))
                {
                    continue;
                }

                if (!geometry.TryGetProperty(
                    "coordinates",
                    out var coordinates))
                {
                    continue;
                }

                // =====================================================
                // ID
                // =====================================================

                int id = 0;

                if (properties.TryGetProperty(
                    "id",
                    out var idProperty))
                {
                    idProperty.TryGetInt32(
                        out id);
                }
                else if (feature.TryGetProperty(
                    "id",
                    out var featureId))
                {
                    var idText =
                        featureId.GetString();

                    if (!string.IsNullOrEmpty(idText))
                    {
                        var parts =
                            idText.Split('.');

                        int.TryParse(
                            parts.Last(),
                            out id);
                    }
                }

                // =====================================================
                // NAME
                // =====================================================

                string name = "";

                if (properties.TryGetProperty(
                    "name",
                    out var nameProperty))
                {
                    name =
                        nameProperty.GetString()
                        ?? "";
                }

                // =====================================================
                // COLOR
                // =====================================================

                string color = "#3388ff";

                if (properties.TryGetProperty(
                    "color",
                    out var colorProperty))
                {
                    color =
                        colorProperty.GetString()
                        ?? "#3388ff";
                }

                // =====================================================
                // POLYGON KOORDİNATLARI
                // GeoJSON Polygon:
                // coordinates -> ring -> coordinate
                // =====================================================

                if (coordinates.GetArrayLength() == 0)
                    continue;

                var outerRing =
                    coordinates[0];

                var coordinateList =
                    new List<object>();

                foreach (var coordinate
                    in outerRing.EnumerateArray())
                {
                    if (coordinate.GetArrayLength() < 2)
                        continue;

                    coordinateList.Add(new
                    {
                        longitude =
                            coordinate[0].GetDouble(),

                        latitude =
                            coordinate[1].GetDouble()
                    });
                }

                if (coordinateList.Count < 3)
                    continue;

                // =====================================================
                // WKT OLUŞTUR
                // =====================================================

                var wktCoordinates =
                    string.Join(
                        ", ",
                        outerRing
                            .EnumerateArray()
                            .Select(c =>
                                $"{c[0].GetDouble()} {c[1].GetDouble()}"));

                var wkt =
                    $"POLYGON (({wktCoordinates}))";

                // =====================================================
                // RESULT
                // =====================================================

                result.Add(new
                {
                    id = id,
                    type = "Polygon",
                    name = name,
                    color = color,
                    wkt = wkt,
                    coordinates = coordinateList
                });
            }

            return result;
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