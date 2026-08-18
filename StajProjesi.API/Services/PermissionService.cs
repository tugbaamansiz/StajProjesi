using Microsoft.EntityFrameworkCore;
using StajProjesi.API.Data;

namespace StajProjesi.API.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(
            int userId,
            string permissionName)
        {
            // =====================================================
            // 1. KULLANICIYA DOĞRUDAN VERİLMİŞ YETKİ
            // =====================================================

            bool hasDirectPermission = await _context.UserPermissions
                .AnyAsync(up =>
                    up.UserId == userId &&
                    up.Permission.Name == permissionName &&
                    up.Permission.IsActive &&
                    !up.Permission.IsDeleted);

            if (hasDirectPermission)
            {
                return true;
            }


            // =====================================================
            // 2. KULLANICININ ROLLERİ ÜZERİNDEN GELEN YETKİ
            // =====================================================

            bool hasRolePermission = await _context.UserRoles
                .Where(ur =>
                    ur.UserId == userId &&
                    ur.Role.IsActive &&
                    !ur.Role.IsDeleted)
                .AnyAsync(ur =>
                    ur.Role.RolePermissions.Any(rp =>
                        rp.Permission.Name == permissionName &&
                        rp.Permission.IsActive &&
                        !rp.Permission.IsDeleted));

            if (hasRolePermission)
            {
                return true;
            }


            // =====================================================
            // 3. YETKİ YOK
            // =====================================================

            return false;
        }
    }
}