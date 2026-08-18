using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using StajProjesi.API.Data;
using StajProjesi.API.Models;

namespace StajProjesi.API.Services
{
    public class GeographicPermissionService
        : IGeographicPermissionService
    {
        private readonly AppDbContext _context;

        public GeographicPermissionService(
            AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // YETKİ ALANI OLUŞTUR
        // =====================================================

        public async Task<GeographicPermission> CreateAsync(
            int? userId,
            int? roleId,
            Geometry geometry,
            int insertedUserId)
        {
            if (userId == null && roleId == null)
            {
                throw new ArgumentException(
                    "Kullanıcı veya rol belirtilmelidir.");
            }

            if (userId != null && roleId != null)
            {
                throw new ArgumentException(
                    "Aynı anda hem kullanıcı hem rol belirtilmemelidir.");
            }

            if (geometry == null)
            {
                throw new ArgumentException(
                    "Yetki alanı geometrisi boş olamaz.");
            }

            geometry.SRID = 4326;

            var geographicPermission =
                new GeographicPermission
                {
                    UserId = userId,
                    RoleId = roleId,
                    Geometry = geometry,

                    InsertedUserId = insertedUserId,
                    InsertedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,

                    IsDeleted = false,
                    IsActive = true
                };

            _context.GeographicPermissions.Add(
                geographicPermission);

            await _context.SaveChangesAsync();

            return geographicPermission;
        }


        // =====================================================
        // KULLANICIYA AİT ALANLAR
        // =====================================================

        public async Task<List<GeographicPermission>>
            GetByUserIdAsync(int userId)
        {
            return await _context.GeographicPermissions
                .Where(x =>
                    x.UserId == userId &&
                    x.IsActive &&
                    !x.IsDeleted)
                .ToListAsync();
        }


        // =====================================================
        // ROLE AİT ALANLAR
        // =====================================================

        public async Task<List<GeographicPermission>>
            GetByRoleIdAsync(int roleId)
        {
            return await _context.GeographicPermissions
                .Where(x =>
                    x.RoleId == roleId &&
                    x.IsActive &&
                    !x.IsDeleted)
                .ToListAsync();
        }


        // =====================================================
        // KULLANICININ TÜM COĞRAFİ YETKİ ALANLARI
        // =====================================================
        //
        // Kullanıcının:
        // 1. Direkt kendi alanları
        // 2. Sahip olduğu rollerden gelen alanları
        //
        // birlikte alınır.
        // =====================================================

        public async Task<List<GeographicPermission>>
            GetUserAreasAsync(int userId)
        {
            var roleIds = await _context.UserRoles
                .Where(x =>
                    x.UserId == userId)
                .Where(x =>
                    x.Role.IsActive &&
                    !x.Role.IsDeleted)
                .Select(x =>
                    x.RoleId)
                .ToListAsync();

            return await _context.GeographicPermissions
                .Where(x =>
                    x.IsActive &&
                    !x.IsDeleted &&
                    (
                        x.UserId == userId ||
                        (x.RoleId != null &&
                         roleIds.Contains(x.RoleId.Value))
                    ))
                .ToListAsync();
        }


        // =====================================================
        // GEOMETRİ YETKİ KONTROLÜ
        // =====================================================
        //
        // Kullanıcının çizdiği geometry tamamen yetki
        // alanlarından birinin içinde olmalıdır.
        //
        // Birden fazla yetki alanı varsa herhangi bir
        // yetki alanının içinde olması yeterlidir.
        //
        // Covers kullanıldığı için yetki alanının sınırına
        // tam olarak denk gelen geometry de kabul edilir.
        // =====================================================

        public async Task<bool> IsGeometryAllowedAsync(
            int userId,
            Geometry geometry)
        {
            if (geometry == null)
            {
                return false;
            }

            geometry.SRID = 4326;

            var areas =
                await GetUserAreasAsync(userId);

            // Kullanıcının hiçbir coğrafi yetkisi yoksa
            // çizim yapmasına izin verilmez.
            if (areas.Count == 0)
            {
                return false;
            }

            foreach (var area in areas)
            {
                if (area.Geometry == null)
                {
                    continue;
                }

                area.Geometry.SRID = 4326;

                // Geometry tamamen yetki alanının
                // içinde veya sınırında olmalıdır.
                if (area.Geometry.Covers(geometry))
                {
                    return true;
                }
            }

            return false;
        }


        // =====================================================
        // ID İLE GETİR
        // =====================================================

        public async Task<GeographicPermission?>
            GetByIdAsync(int id)
        {
            return await _context.GeographicPermissions
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive &&
                    !x.IsDeleted);
        }


        // =====================================================
        // SOFT DELETE
        // =====================================================

        public async Task<bool> DeleteAsync(int id)
        {
            var permission =
                await _context.GeographicPermissions
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.IsDeleted);

            if (permission == null)
            {
                return false;
            }

            permission.IsDeleted = true;
            permission.IsActive = false;
            permission.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}