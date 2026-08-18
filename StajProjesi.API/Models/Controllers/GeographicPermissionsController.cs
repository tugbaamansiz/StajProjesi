using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using StajProjesi.API.Data;
using StajProjesi.API.Models;
using StajProjesi.API.Services;
using System.Security.Claims;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class GeographicPermissionsController : ControllerBase
    {
        private readonly IGeographicPermissionService
            _geographicPermissionService;

        private readonly AppDbContext _context;

        public GeographicPermissionsController(
            IGeographicPermissionService geographicPermissionService,
            AppDbContext context)
        {
            _geographicPermissionService =
                geographicPermissionService;

            _context = context;
        }


        // =====================================================
        // GET - TÜM COĞRAFİ YETKİLER
        // =====================================================

        // GET: api/GeographicPermissions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var permissions =
                    await _context.GeographicPermissions
                        .Where(x =>
                            x.IsActive &&
                            !x.IsDeleted)
                        .ToListAsync();

                var result = new List<object>();

                foreach (var permission in permissions)
                {
                    string? username = null;
                    string? roleName = null;

                    if (permission.UserId != null)
                    {
                        username =
                            await _context.Users
                                .Where(x =>
                                    x.Id ==
                                    permission.UserId.Value)
                                .Select(x =>
                                    x.Username)
                                .FirstOrDefaultAsync();
                    }

                    if (permission.RoleId != null)
                    {
                        roleName =
                            await _context.Roles
                                .Where(x =>
                                    x.Id ==
                                    permission.RoleId.Value)
                                .Select(x =>
                                    x.Name)
                                .FirstOrDefaultAsync();
                    }

                    result.Add(new
                    {
                        id = permission.Id,

                        userId = permission.UserId,

                        username = username,

                        roleId = permission.RoleId,

                        roleName = roleName,

                        coordinates =
                            permission.Geometry
                                .Coordinates
                                .Select(c => new
                                {
                                    longitude = c.X,
                                    latitude = c.Y
                                })
                                .ToList()
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Coğrafi yetkiler alınırken bir hata oluştu.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // GET - KULLANICIYA AİT COĞRAFİ YETKİLER
        // =====================================================

        // GET: api/GeographicPermissions/user/2
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(
            int userId)
        {
            try
            {
                var permissions =
                    await _geographicPermissionService
                        .GetByUserIdAsync(userId);

                var result =
                    permissions.Select(permission => new
                    {
                        id = permission.Id,

                        userId = permission.UserId,

                        roleId = permission.RoleId,

                        coordinates =
                            permission.Geometry
                                .Coordinates
                                .Select(c => new
                                {
                                    longitude = c.X,
                                    latitude = c.Y
                                })
                                .ToList()
                    });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Kullanıcının coğrafi yetkileri alınırken hata oluştu.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // GET - ROLE AİT COĞRAFİ YETKİLER
        // =====================================================

        // GET: api/GeographicPermissions/role/2
        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRole(
            int roleId)
        {
            try
            {
                var permissions =
                    await _geographicPermissionService
                        .GetByRoleIdAsync(roleId);

                var result =
                    permissions.Select(permission => new
                    {
                        id = permission.Id,

                        userId = permission.UserId,

                        roleId = permission.RoleId,

                        coordinates =
                            permission.Geometry
                                .Coordinates
                                .Select(c => new
                                {
                                    longitude = c.X,
                                    latitude = c.Y
                                })
                                .ToList()
                    });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Rolün coğrafi yetkileri alınırken hata oluştu.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // POST - COĞRAFİ YETKİ OLUŞTUR
        // =====================================================

        // POST: api/GeographicPermissions
        [HttpPost]
        public async Task<IActionResult> Create(
            GeographicPermissionDto dto)
        {
            try
            {
                // =================================================
                // KULLANICI VEYA ROL KONTROLÜ
                // =================================================

                if (dto.UserId == null &&
                    dto.RoleId == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "Kullanıcı veya rol seçilmelidir."
                    });
                }

                if (dto.UserId != null &&
                    dto.RoleId != null)
                {
                    return BadRequest(new
                    {
                        message =
                            "Aynı anda hem kullanıcı hem rol seçilemez."
                    });
                }


                // =================================================
                // KOORDİNAT KONTROLÜ
                // =================================================

                if (dto.Coordinates == null ||
                    dto.Coordinates.Count < 3)
                {
                    return BadRequest(new
                    {
                        message =
                            "En az 3 koordinat gereklidir."
                    });
                }


                // =================================================
                // USER KONTROLÜ
                // =================================================

                if (dto.UserId != null)
                {
                    var userExists =
                        await _context.Users
                            .AnyAsync(x =>
                                x.Id ==
                                dto.UserId.Value &&
                                x.IsActive &&
                                !x.IsDeleted);

                    if (!userExists)
                    {
                        return BadRequest(new
                        {
                            message =
                                "Seçilen kullanıcı bulunamadı."
                        });
                    }
                }


                // =================================================
                // ROLE KONTROLÜ
                // =================================================

                if (dto.RoleId != null)
                {
                    var roleExists =
                        await _context.Roles
                            .AnyAsync(x =>
                                x.Id ==
                                dto.RoleId.Value &&
                                x.IsActive &&
                                !x.IsDeleted);

                    if (!roleExists)
                    {
                        return BadRequest(new
                        {
                            message =
                                "Seçilen rol bulunamadı."
                        });
                    }
                }


                // =================================================
                // POLYGON OLUŞTUR
                // =================================================

                var geometryFactory =
                    NtsGeometryServices.Instance
                        .CreateGeometryFactory(
                            srid: 4326);

                var coordinates =
                    dto.Coordinates
                        .Select(c =>
                            new Coordinate(
                                c.Longitude,
                                c.Latitude))
                        .ToList();


                // Polygon'un kapanması gerekiyor.
                // İlk ve son koordinat aynı değilse
                // ilk koordinatı sona ekliyoruz.

                var first =
                    coordinates.First();

                var last =
                    coordinates.Last();

                if (!first.Equals2D(last))
                {
                    coordinates.Add(
                        new Coordinate(
                            first.X,
                            first.Y));
                }


                // =================================================
                // POLYGON
                // =================================================

                var linearRing =
                    geometryFactory
                        .CreateLinearRing(
                            coordinates.ToArray());

                var polygon =
                    geometryFactory
                        .CreatePolygon(
                            linearRing);


                // =================================================
                // GEOMETRY VALIDATION
                // =================================================

                if (!polygon.IsValid)
                {
                    return BadRequest(new
                    {
                        message =
                            "Çizilen coğrafi alan geçerli bir polygon değil."
                    });
                }


                // =================================================
                // ADMIN USER ID
                // =================================================

                var adminIdValue =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (!int.TryParse(
                        adminIdValue,
                        out var adminId))
                {
                    return Unauthorized();
                }


                // =================================================
                // SERVICE
                // =================================================

                var created =
                    await _geographicPermissionService
                        .CreateAsync(
                            dto.UserId,
                            dto.RoleId,
                            polygon,
                            adminId);


                // =================================================
                // RESPONSE
                // =================================================

                return Ok(new
                {
                    id = created.Id,

                    userId = created.UserId,

                    roleId = created.RoleId,

                    coordinates =
                        created.Geometry
                            .Coordinates
                            .Select(c => new
                            {
                                longitude = c.X,
                                latitude = c.Y
                            })
                            .ToList(),

                    message =
                        "Coğrafi yetki alanı başarıyla oluşturuldu."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Coğrafi yetki oluşturulurken bir hata oluştu.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // DELETE - SOFT DELETE
        // =====================================================

        // DELETE: api/GeographicPermissions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                var deleted =
                    await _geographicPermissionService
                        .DeleteAsync(id);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        message =
                            "Coğrafi yetki alanı bulunamadı."
                    });
                }

                return Ok(new
                {
                    message =
                        "Coğrafi yetki alanı silindi."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Coğrafi yetki silinirken bir hata oluştu.",

                    error = ex.Message
                });
            }
        }
    }


    // =========================================================
    // DTO
    // =========================================================

    public class GeographicPermissionDto
    {
        public int? UserId { get; set; }

        public int? RoleId { get; set; }

        public List<CoordinateDto> Coordinates { get; set; }
            = new();
    }
}