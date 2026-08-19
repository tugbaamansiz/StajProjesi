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
    public class PointFeaturesController : ControllerBase
    {
        private readonly IPointFeatureService _pointService;
        private readonly IPermissionService _permissionService;
        private readonly IGeographicPermissionService _geographicPermissionService;
        private readonly IGeoServerService _geoServerService;

        public PointFeaturesController(
            IPointFeatureService pointService,
            IPermissionService permissionService,
            IGeographicPermissionService geographicPermissionService,
            IGeoServerService geoServerService)
        {
            _pointService = pointService;
            _permissionService = permissionService;
            _geographicPermissionService = geographicPermissionService;
            _geoServerService = geoServerService;
        }


        // =====================================================
        // GET - TÜM NOKTALAR
        // ARTIK DOĞRUDAN DATABASE DEĞİL,
        // GEOSERVER ÜZERİNDEN GELİYOR
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetPoints()
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var geoServerResponse =
                    await _geoServerService.GetFeaturesAsync(
                        "tbl_point",
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
                        "Noktalar GeoServer üzerinden getirilirken bir hata oluştu.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // GET - TEK NOKTA
        // ARTIK DOĞRUDAN DATABASE DEĞİL,
        // GEOSERVER ÜZERİNDEN GELİYOR
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPoint(int id)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var geoServerResponse =
                    await _geoServerService.GetFeatureAsync(
                        "tbl_point",
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
                        "Nokta GeoServer üzerinden getirilirken bir hata oluştu.",

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


                // =================================================
                // POINT EKLEME YETKİSİ
                // =================================================

                var hasPermission =
                    await _permissionService.HasPermissionAsync(
                        userId.Value,
                        "Point Ekleme");

                if (!hasPermission)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Point ekleme yetkiniz bulunmamaktadır."
                    });
                }


                // =================================================
                // COĞRAFİ YETKİ KONTROLÜ
                // =================================================

                var pointGeometry =
                    new Point(
                        dto.Longitude,
                        dto.Latitude)
                    {
                        SRID = 4326
                    };

                var geoAllowed =
                    await _geographicPermissionService
                        .IsGeometryAllowedAsync(
                            userId.Value,
                            pointGeometry);

                if (!geoAllowed)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Bu noktayı çizmek için coğrafi yetkiniz bulunmamaktadır."
                    });
                }


                // =================================================
                // POINT OLUŞTUR
                // =================================================

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


                // =================================================
                // POINT GÜNCELLEME YETKİSİ
                // =================================================

                var hasPermission =
                    await _permissionService.HasPermissionAsync(
                        userId.Value,
                        "Point Güncelleme");

                if (!hasPermission)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Point güncelleme yetkiniz bulunmamaktadır."
                    });
                }


                // =================================================
                // COĞRAFİ YETKİ KONTROLÜ
                // =================================================

                var pointGeometry =
                    new Point(
                        dto.Longitude,
                        dto.Latitude)
                    {
                        SRID = 4326
                    };

                var geoAllowed =
                    await _geographicPermissionService
                        .IsGeometryAllowedAsync(
                            userId.Value,
                            pointGeometry);

                if (!geoAllowed)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Point'i bu konuma taşımak için coğrafi yetkiniz bulunmamaktadır."
                    });
                }


                // =================================================
                // POINT GÜNCELLE
                // =================================================

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


                // =================================================
                // POINT SİLME YETKİSİ
                // =================================================

                var hasPermission =
                    await _permissionService.HasPermissionAsync(
                        userId.Value,
                        "Point Silme");

                if (!hasPermission)
                {
                    return StatusCode(403, new
                    {
                        message =
                            "Point silme yetkiniz bulunmamaktadır."
                    });
                }


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
        // GEOSERVER GEOJSON → FRONTEND FORMATINA ÇEVİR
        // =====================================================

        private List<object> ParseGeoServerFeatures(
            string json)
        {
            var result = new List<object>();

            using var document =
                JsonDocument.Parse(json);

            var root = document.RootElement;

            if (!root.TryGetProperty(
                "features",
                out var features))
            {
                return result;
            }

            foreach (var feature in features.EnumerateArray())
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

                // =================================================
                // ID
                // =================================================

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


                // =================================================
                // NAME
                // =================================================

                string name = "";

                if (properties.TryGetProperty(
                    "name",
                    out var nameProperty))
                {
                    name =
                        nameProperty.GetString()
                        ?? "";
                }


                // =================================================
                // COLOR
                // =================================================

                string color = "#3388ff";

                if (properties.TryGetProperty(
                    "color",
                    out var colorProperty))
                {
                    color =
                        colorProperty.GetString()
                        ?? "#3388ff";
                }


                // =================================================
                // POINT COORDINATES
                // =================================================

                if (coordinates.GetArrayLength() < 2)
                    continue;

                var longitude =
                    coordinates[0].GetDouble();

                var latitude =
                    coordinates[1].GetDouble();


                // =================================================
                // WKT
                // =================================================

                var wkt =
                    $"POINT ({longitude} {latitude})";


                result.Add(new
                {
                    id = id,
                    type = "Point",
                    name = name,
                    color = color,
                    wkt = wkt,

                    coordinates = new
                    {
                        longitude = longitude,
                        latitude = latitude
                    }
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