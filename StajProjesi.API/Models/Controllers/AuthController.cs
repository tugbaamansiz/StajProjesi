using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StajProjesi.API.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(
            IConfiguration configuration,
            AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // =====================================================
        // LOGIN
        // =====================================================

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request)
        {
            try
            {
                // =================================================
                // KULLANICIYI BUL
                // =================================================

                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Username == request.Username &&
                        !u.IsDeleted &&
                        u.IsActive);

                // Kullanıcı yoksa veya şifre yanlışsa
                if (user == null ||
                    user.Password != request.Password)
                {
                    return Unauthorized(new
                    {
                        message =
                            "Kullanıcı adı veya şifre hatalı."
                    });
                }


                // =================================================
                // KULLANICININ ROLLERİNİ GETİR
                // =================================================

                var roles = await _context.UserRoles
                    .Where(ur =>
                        ur.UserId == user.Id)
                    .Where(ur =>
                        ur.Role.IsActive &&
                        !ur.Role.IsDeleted)
                    .Select(ur =>
                        ur.Role.Name)
                    .ToListAsync();


                // =================================================
                // KULLANICIYA DOĞRUDAN VERİLEN PERMISSION'LAR
                // =================================================

                var directPermissions =
                    await _context.UserPermissions
                        .Where(up =>
                            up.UserId == user.Id)
                        .Where(up =>
                            up.Permission.IsActive &&
                            !up.Permission.IsDeleted)
                        .Select(up =>
                            up.Permission.Name)
                        .ToListAsync();


                // =================================================
                // ROLLER ÜZERİNDEN GELEN PERMISSION'LAR
                // =================================================

                var rolePermissions =
                    await _context.UserRoles
                        .Where(ur =>
                            ur.UserId == user.Id)
                        .Where(ur =>
                            ur.Role.IsActive &&
                            !ur.Role.IsDeleted)
                        .SelectMany(ur =>
                            ur.Role.RolePermissions)
                        .Where(rp =>
                            rp.Permission.IsActive &&
                            !rp.Permission.IsDeleted)
                        .Select(rp =>
                            rp.Permission.Name)
                        .ToListAsync();


                // =================================================
                // TÜM PERMISSION'LARI BİRLEŞTİR
                // =================================================

                var permissions =
                    directPermissions
                        .Concat(rolePermissions)
                        .Distinct()
                        .ToList();


                // =================================================
                // JWT CLAIMS
                // =================================================

                var claims = new List<Claim>
                {
                    // Kullanıcı ID
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        user.Id.ToString()),

                    // Kullanıcı adı
                    new Claim(
                        ClaimTypes.Name,
                        user.Username)
                };


                // =================================================
                // ROLE CLAIMS
                // =================================================

                foreach (var role in roles)
                {
                    claims.Add(
                        new Claim(
                            ClaimTypes.Role,
                            role));
                }


                // =================================================
                // PERMISSION CLAIMS
                // =================================================
                //
                // Türkçe karakterleri JWT içinde kullanmıyoruz.
                // DB'deki gerçek permission isimleri korunuyor,
                // ancak JWT'ye ASCII kodları yazılıyor.
                //
                // Örneğin:
                // "Point Güncelleme" -> "POINT_UPDATE"
                //
                // =================================================

                var permissionCodes = new List<string>();

                foreach (var permission in permissions)
                {
                    string permissionCode = permission switch
                    {
                        "Point Ekleme" =>
                            "POINT_CREATE",

                        "Point Güncelleme" =>
                            "POINT_UPDATE",

                        "Point Silme" =>
                            "POINT_DELETE",

                        "Line Ekleme" =>
                            "LINE_CREATE",

                        "Line Güncelleme" =>
                            "LINE_UPDATE",

                        "Line Silme" =>
                            "LINE_DELETE",

                        "Polygon Ekleme" =>
                            "POLYGON_CREATE",

                        "Polygon Güncelleme" =>
                            "POLYGON_UPDATE",

                        "Polygon Silme" =>
                            "POLYGON_DELETE",

                        "Kullanıcı Yönetme" =>
                            "USER_MANAGE",

                        "Rol Yönetme" =>
                            "ROLE_MANAGE",

                        _ => permission
                    };

                    permissionCodes.Add(permissionCode);

                    claims.Add(
                        new Claim(
                            "permission",
                            permissionCode));
                }


                // =================================================
                // JWT KEY
                // =================================================

                var jwtKey =
                    _configuration["Jwt:Key"];

                if (string.IsNullOrEmpty(jwtKey))
                {
                    return StatusCode(500, new
                    {
                        message =
                            "JWT anahtarı bulunamadı."
                    });
                }


                // =================================================
                // SECURITY KEY
                // =================================================

                var key =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    );


                // =================================================
                // SIGNING CREDENTIALS
                // =================================================

                var credentials =
                    new SigningCredentials(
                        key,
                        SecurityAlgorithms.HmacSha256
                    );


                // =================================================
                // TOKEN
                // =================================================

                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    null,
                    DateTime.UtcNow.AddMinutes(10),
                    credentials
                );


                // =================================================
                // TOKEN STRING
                // =================================================

                var tokenString =
                    new JwtSecurityTokenHandler()
                        .WriteToken(token);


                // =================================================
                // RESPONSE
                // =================================================

                return Ok(new
                {
                    token = tokenString,

                    expiresIn = 600,

                    userId = user.Id,

                    username = user.Username,

                    roles = roles,

                    permissions = permissionCodes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Giriş yapılırken bir hata oluştu.",

                    error = ex.Message
                });
            }
        }
    }


    // =========================================================
    // LOGIN DTO
    // =========================================================

    public class LoginRequest
    {
        public string Username { get; set; } = "";

        public string Password { get; set; } = "";
    }
}