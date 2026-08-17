using Microsoft.EntityFrameworkCore;
using StajProjesi.API.Models;

namespace StajProjesi.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // =====================================================
            // ADMIN ROLE
            // =====================================================

            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Name = "Admin",
                    Description = "Sistemin tüm yönetim yetkilerine sahip rol.",
                    IsActive = true,
                    IsDeleted = false
                };

                context.Roles.Add(adminRole);

                await context.SaveChangesAsync();
            }


            // =====================================================
            // PERMISSIONS
            // =====================================================

            var permissionDefinitions = new[]
            {
                new
                {
                    Name = "Point Ekleme",
                    Description = "Haritaya point ekleme yetkisi."
                },

                new
                {
                    Name = "Point Silme",
                    Description = "Haritadaki pointleri silme yetkisi."
                },

                new
                {
                    Name = "Point Güncelleme",
                    Description = "Haritadaki pointleri güncelleme yetkisi."
                },

                new
                {
                    Name = "Line Ekleme",
                    Description = "Haritaya line ekleme yetkisi."
                },

                new
                {
                    Name = "Line Silme",
                    Description = "Haritadaki lineleri silme yetkisi."
                },

                new
                {
                    Name = "Line Güncelleme",
                    Description = "Haritadaki lineleri güncelleme yetkisi."
                },

                new
                {
                    Name = "Polygon Ekleme",
                    Description = "Haritaya polygon ekleme yetkisi."
                },

                new
                {
                    Name = "Polygon Silme",
                    Description = "Haritadaki polygonları silme yetkisi."
                },

                new
                {
                    Name = "Polygon Güncelleme",
                    Description = "Haritadaki polygonları güncelleme yetkisi."
                },

                new
                {
                    Name = "Kullanıcı Yönetme",
                    Description = "Kullanıcı ekleme, güncelleme ve çıkarma yetkisi."
                },

                new
                {
                    Name = "Rol Yönetme",
                    Description = "Rol ekleme ve silme yetkisi."
                }
            };


            // =====================================================
            // CREATE PERMISSIONS
            // =====================================================

            foreach (var definition in permissionDefinitions)
            {
                var permission = await context.Permissions
                    .FirstOrDefaultAsync(p =>
                        p.Name == definition.Name);

                if (permission == null)
                {
                    permission = new Permission
                    {
                        Name = definition.Name,
                        Description = definition.Description,
                        IsActive = true,
                        IsDeleted = false
                    };

                    context.Permissions.Add(permission);

                    await context.SaveChangesAsync();
                }
            }


            // =====================================================
            // ADMIN ROLE → ALL PERMISSIONS
            // =====================================================

            var permissions = await context.Permissions
                .Where(p =>
                    !p.IsDeleted &&
                    p.IsActive)
                .ToListAsync();


            foreach (var permission in permissions)
            {
                var rolePermissionExists =
                    await context.RolePermissions
                        .AnyAsync(rp =>
                            rp.RoleId == adminRole.Id &&
                            rp.PermissionId == permission.Id);

                if (!rolePermissionExists)
                {
                    context.RolePermissions.Add(
                        new RolePermission
                        {
                            RoleId = adminRole.Id,
                            PermissionId = permission.Id
                        });
                }
            }

            await context.SaveChangesAsync();


            // =====================================================
            // FIRST ACTIVE USER → ADMIN
            // =====================================================

            var firstUser = await context.Users
                .Where(u =>
                    !u.IsDeleted &&
                    u.IsActive)
                .OrderBy(u => u.Id)
                .FirstOrDefaultAsync();

            if (firstUser != null)
            {
                var alreadyAdmin =
                    await context.UserRoles
                        .AnyAsync(ur =>
                            ur.UserId == firstUser.Id &&
                            ur.RoleId == adminRole.Id);

                if (!alreadyAdmin)
                {
                    context.UserRoles.Add(
                        new UserRole
                        {
                            UserId = firstUser.Id,
                            RoleId = adminRole.Id
                        });

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}