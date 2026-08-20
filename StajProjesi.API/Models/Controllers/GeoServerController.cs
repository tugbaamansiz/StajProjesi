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
        // JWT'DEN KULLANICI ID'SİNİ AL
        // =====================================================

        private int GetUserId()
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException(
                    "Kullanıcı kimliği bulunamadı.");
            }

            if (!int.TryParse(
                    userIdClaim,
                    out var userId))
            {
                throw new UnauthorizedAccessException(
                    "Geçersiz kullanıcı kimliği.");
            }

            return userId;
        }


        // =====================================================
        // POINT
        //
        // GeoServer SQL View:
        // point_view
        //
        // SQL View içerisinde:
        // is_deleted = false
        // is_active = true
        //
        // GeoServer CQL_FILTER:
        // inserted_user_id = userId
        // =====================================================

        [HttpGet("point")]
        public async Task<IActionResult> GetPoints()
        {
            try
            {
                var userId =
                    GetUserId();

                var result =
                    await _geoServerService.GetFeaturesAsync(
                        "point_view",
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
        // LINE
        //
        // GeoServer SQL View:
        // line_view
        //
        // SQL View içerisinde:
        // is_deleted = false
        // is_active = true
        //
        // GeoServer CQL_FILTER:
        // inserted_user_id = userId
        // =====================================================

        [HttpGet("line")]
        public async Task<IActionResult> GetLines()
        {
            try
            {
                var userId =
                    GetUserId();

                var result =
                    await _geoServerService.GetFeaturesAsync(
                        "line_view",
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
        // POLYGON
        //
        // GeoServer SQL View:
        // polygon_view
        //
        // SQL View içerisinde:
        // is_deleted = false
        // is_active = true
        //
        // GeoServer CQL_FILTER:
        // inserted_user_id = userId
        // =====================================================

        [HttpGet("polygon")]
        public async Task<IActionResult> GetPolygons()
        {
            try
            {
                var userId =
                    GetUserId();

                var result =
                    await _geoServerService.GetFeaturesAsync(
                        "polygon_view",
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
        // TEK FEATURE GETİR
        //
        // Kullanım:
        //
        // GET /api/GeoServer/point_view/5
        // GET /api/GeoServer/line_view/5
        // GET /api/GeoServer/polygon_view/5
        //
        // =====================================================

        [HttpGet("{layerName}/{featureId:int}")]
        public async Task<IActionResult> GetFeature(
            string layerName,
            int featureId)
        {
            try
            {
                var userId =
                    GetUserId();

                var result =
                    await _geoServerService.GetFeatureAsync(
                        layerName,
                        featureId,
                        userId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Feature bulunamadı."
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