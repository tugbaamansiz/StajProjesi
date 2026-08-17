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
                // Kullanıcıyı veritabanından bul
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
                    .Where(ur => ur.UserId == user.Id)
                    .Where(ur => ur.Role.IsActive && !ur.Role.IsDeleted)
                    .Select(ur => ur.Role.Name)
                    .ToListAsync();


                // =================================================
                // JWT CLAIMS
                // =================================================

                var claims = new List<Claim>
                {
                    // Kullanıcının gerçek ID'si
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

                var key =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    );

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
                    DateTime.UtcNow.AddMinutes(60),
                    credentials
                );

                var tokenString =
                    new JwtSecurityTokenHandler()
                        .WriteToken(token);


                // =================================================
                // RESPONSE
                // =================================================

                return Ok(new
                {
                    token = tokenString,
                    expiresIn = 3600,

                    userId = user.Id,
                    username = user.Username,

                    roles = roles
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