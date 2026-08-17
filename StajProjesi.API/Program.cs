using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StajProjesi.API.Data;
using StajProjesi.API.Services;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// DATABASE
// =====================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseNetTopologySuite()
    )
);


// =====================================================
// CONTROLLERS
// =====================================================

builder.Services.AddControllers();


// =====================================================
// EXISTING SERVICES
// =====================================================

builder.Services.AddScoped<IPointFeatureService, PointFeatureService>();
builder.Services.AddScoped<ILineFeatureService, LineFeatureService>();
builder.Services.AddScoped<IPolygonFeatureService, PolygonFeatureService>();


// =====================================================
// CORS
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// =====================================================
// JWT AUTHENTICATION
// =====================================================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!
                        )
                    ),

                // JWT içerisindeki Role claim'ini kullan
                RoleClaimType = ClaimTypes.Role,

                // JWT içerisindeki kullanıcı adını kullan
                NameClaimType = ClaimTypes.Name
            };
    });


// =====================================================
// AUTHORIZATION
// =====================================================

builder.Services.AddAuthorization();


// =====================================================
// SWAGGER
// =====================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();


// =====================================================
// DATABASE SEED
// =====================================================
//
// Admin rolü
// Permission'lar
// Role-Permission ilişkileri
// İlk aktif kullanıcıya Admin rolü
//
// migration + database update yapıldıktan sonra
// uygulama başlarken otomatik çalışır.
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context =
            services.GetRequiredService<AppDbContext>();

        await DbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "Database seed sırasında hata oluştu:"
        );

        Console.WriteLine(ex.Message);
    }
}


// =====================================================
// SWAGGER
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// =====================================================
// CORS
// =====================================================

app.UseCors("AllowFrontend");


// =====================================================
// HTTPS
// =====================================================

// app.UseHttpsRedirection();


// =====================================================
// AUTHENTICATION
// =====================================================

app.UseAuthentication();


// =====================================================
// AUTHORIZATION
// =====================================================

app.UseAuthorization();


// =====================================================
// CONTROLLERS
// =====================================================

app.MapControllers();


// =====================================================
// RUN
// =====================================================

app.Run();