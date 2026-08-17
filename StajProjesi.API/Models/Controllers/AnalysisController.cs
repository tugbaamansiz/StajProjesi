using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using StajProjesi.API.Data;
using StajProjesi.API.Models;
using System.Security.Claims;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalysisController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 1. ÇİZİLEN POLİGONUN KESİŞİM ANALİZİ
        // =====================================================

        [HttpPost("intersection")]
        public async Task<IActionResult> Intersection(
            AnalysisDto dto)
        {
            try
            {
                // ---------------------------------------------
                // Giriş yapan kullanıcının ID'sini al
                // ---------------------------------------------

                var userId = GetUserId();

                if (userId == null)
                {
                    return Unauthorized(new
                    {
                        message = "Kullanıcı girişi gerekli."
                    });
                }

                // ---------------------------------------------
                // Koordinat kontrolü
                // ---------------------------------------------

                if (dto.Coordinates == null ||
                    dto.Coordinates.Count < 3)
                {
                    return BadRequest(
                        "Analiz için en az 3 koordinat gerekli.");
                }

                // ---------------------------------------------
                // Koordinatları NetTopologySuite formatına çevir
                // ---------------------------------------------

                var coordinates = dto.Coordinates
                    .Select(c => new Coordinate(
                        c.Longitude,
                        c.Latitude))
                    .ToList();

                // Polygon kapanmamışsa kapat
                if (
                    coordinates.First().X !=
                    coordinates.Last().X ||
                    coordinates.First().Y !=
                    coordinates.Last().Y
                )
                {
                    coordinates.Add(
                        coordinates.First());
                }

                // ---------------------------------------------
                // Analiz polygonunu oluştur
                // ---------------------------------------------

                var ring = new LinearRing(
                    coordinates.ToArray());

                var analysisPolygon = new Polygon(ring)
                {
                    SRID = 4326
                };

                // =====================================================
                // NOKTALAR
                // =====================================================

                var points = await _context.Points
                    .Where(point =>
                        point.InsertedUserId == userId.Value &&
                        !point.IsDeleted &&
                        point.IsActive &&
                        point.Geometry.Intersects(
                            analysisPolygon))
                    .Select(point => point.Name)
                    .ToListAsync();

                // =====================================================
                // ÇİZGİLER
                // =====================================================

                var lines = await _context.Lines
                    .Where(line =>
                        line.InsertedUserId == userId.Value &&
                        !line.IsDeleted &&
                        line.IsActive &&
                        line.Geometry.Intersects(
                            analysisPolygon))
                    .Select(line => line.Name)
                    .ToListAsync();

                // =====================================================
                // POLYGONLAR
                // =====================================================

                var polygons = await _context.Polygons
                    .Where(polygon =>
                        polygon.InsertedUserId == userId.Value &&
                        !polygon.IsDeleted &&
                        polygon.IsActive &&
                        polygon.Geometry.Intersects(
                            analysisPolygon))
                    .Select(polygon => polygon.Name)
                    .ToListAsync();

                // =====================================================
                // SONUÇ
                // =====================================================

                return Ok(new
                {
                    pointCount = points.Count,

                    lineCount = lines.Count,

                    polygonCount = polygons.Count,

                    totalCount =
                        points.Count +
                        lines.Count +
                        polygons.Count,

                    pointNames = points,

                    lineNames = lines,

                    polygonNames = polygons
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Kesişim analizi sırasında bir hata oluştu.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // 2. ENVANTER ANALİZİ
        // =====================================================

        [HttpPost("inventory")]
        public async Task<IActionResult> InventoryAnalysis(
            AnalysisDto dto)
        {
            try
            {
                // ---------------------------------------------
                // Giriş yapan kullanıcının ID'sini al
                // ---------------------------------------------

                var userId = GetUserId();

                if (userId == null)
                {
                    return Unauthorized(new
                    {
                        message = "Kullanıcı girişi gerekli."
                    });
                }

                // ---------------------------------------------
                // Koordinat kontrolü
                // ---------------------------------------------

                if (dto.Coordinates == null ||
                    dto.Coordinates.Count < 3)
                {
                    return BadRequest(
                        "Analiz için en az 3 koordinat gerekli.");
                }

                // ---------------------------------------------
                // Koordinatları oluştur
                // ---------------------------------------------

                var coordinates = dto.Coordinates
                    .Select(c => new Coordinate(
                        c.Longitude,
                        c.Latitude))
                    .ToList();

                // Polygon kapanmamışsa kapat
                if (
                    coordinates.First().X !=
                    coordinates.Last().X ||
                    coordinates.First().Y !=
                    coordinates.Last().Y
                )
                {
                    coordinates.Add(
                        coordinates.First());
                }

                // ---------------------------------------------
                // Analiz polygonunu oluştur
                // ---------------------------------------------

                var ring = new LinearRing(
                    coordinates.ToArray());

                var analysisPolygon = new Polygon(ring)
                {
                    SRID = 4326
                };

                // =====================================================
                // NOKTALAR
                // =====================================================

                var points = await _context.Points
                    .Where(point =>
                        point.InsertedUserId == userId.Value &&
                        !point.IsDeleted &&
                        point.IsActive &&
                        point.Geometry.Intersects(
                            analysisPolygon))
                    .Select(point => point.Name)
                    .ToListAsync();

                // =====================================================
                // ÇİZGİLER
                // =====================================================

                var lines = await _context.Lines
                    .Where(line =>
                        line.InsertedUserId == userId.Value &&
                        !line.IsDeleted &&
                        line.IsActive &&
                        line.Geometry.Intersects(
                            analysisPolygon))
                    .Select(line => line.Name)
                    .ToListAsync();

                // =====================================================
                // POLYGONLAR
                // =====================================================

                var polygons = await _context.Polygons
                    .Where(polygon =>
                        polygon.InsertedUserId == userId.Value &&
                        !polygon.IsDeleted &&
                        polygon.IsActive &&
                        polygon.Geometry.Intersects(
                            analysisPolygon))
                    .Select(polygon => polygon.Name)
                    .ToListAsync();

                // =====================================================
                // TOPLAM
                // =====================================================

                var totalCount =
                    points.Count +
                    lines.Count +
                    polygons.Count;

                // =====================================================
                // SONUÇ
                // =====================================================

                return Ok(new
                {
                    pointCount = points.Count,

                    lineCount = lines.Count,

                    polygonCount = polygons.Count,

                    totalCount = totalCount,

                    pointNames = points,

                    lineNames = lines,

                    polygonNames = polygons
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Envanter analizi sırasında bir hata oluştu.",

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

        public class AnalysisDto
        {
            public List<CoordinateDto> Coordinates { get; set; }
                = new();
        }
    }
}