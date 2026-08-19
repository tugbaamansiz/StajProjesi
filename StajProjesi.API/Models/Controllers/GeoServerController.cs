using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajProjesi.API.Services;
using System.Security.Claims;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GeoServerController : ControllerBase
    {
        private readonly IGeoServerService _geoServerService;

        public GeoServerController(
            IGeoServerService geoServerService)
        {
            _geoServerService = geoServerService;
        }

        // =====================================================
        // KULLANICI ID'SİNİ JWT'DEN AL
        // =====================================================
        private int GetUserId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException(
                    "Kullanıcı kimliği bulunamadı.");
            }

            return int.Parse(userIdClaim);
        }

        // =====================================================
        // GEOSERVER'DAN NOKTALARI GETİR
        // =====================================================
        [HttpGet("point")]
        public async Task<IActionResult> GetPoints()
        {
            try
            {
                var userId = GetUserId();

                var result =
                    await _geoServerService.GetFeaturesAsync(
                        "tbl_point",
                        userId);

                return Content(
                    result,
                    "application/json");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "GeoServer üzerinden noktalar alınırken hata oluştu.",
                        error = ex.Message
                    });
            }
        }

        // =====================================================
        // GEOSERVER'DAN ÇİZGİLERİ GETİR
        // =====================================================
        [HttpGet("line")]
        public async Task<IActionResult> GetLines()
        {
            try
            {
                var userId = GetUserId();

                var result =
                    await _geoServerService.GetFeaturesAsync(
                        "tbl_line",
                        userId);

                return Content(
                    result,
                    "application/json");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "GeoServer üzerinden çizgiler alınırken hata oluştu.",
                        error = ex.Message
                    });
            }
        }

        // =====================================================
        // GEOSERVER'DAN POLYGONLARI GETİR
        // =====================================================
        [HttpGet("polygon")]
        public async Task<IActionResult> GetPolygons()
        {
            try
            {
                var userId = GetUserId();

                var result =
                    await _geoServerService.GetFeaturesAsync(
                        "tbl_polygon",
                        userId);

                return Content(
                    result,
                    "application/json");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "GeoServer üzerinden polygonlar alınırken hata oluştu.",
                        error = ex.Message
                    });
            }
        }

        // =====================================================
        // GEOSERVER'DAN TEK FEATURE GETİR
        // =====================================================
        [HttpGet("{layerName}/{featureId:int}")]
        public async Task<IActionResult> GetFeature(
            string layerName,
            int featureId)
        {
            try
            {
                var userId = GetUserId();

                var result =
                    await _geoServerService.GetFeatureAsync(
                        layerName,
                        featureId,
                        userId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Feature bulunamadı."
                    });
                }

                return Content(
                    result,
                    "application/json");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "GeoServer üzerinden feature alınırken hata oluştu.",
                        error = ex.Message
                    });
            }
        }
    }
}