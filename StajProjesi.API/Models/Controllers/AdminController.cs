using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajProjesi.API.Data;
using StajProjesi.API.Models;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // USERS
        // =====================================================

        // GET: api/Admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => !u.IsDeleted)
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .Include(u => u.UserPermissions)
                        .ThenInclude(up => up.Permission)
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.IsActive,

                        Roles = u.UserRoles
                            .Where(ur =>
                                ur.Role.IsActive &&
                                !ur.Role.IsDeleted)
                            .Select(ur => new
                            {
                                ur.RoleId,
                                ur.Role.Name
                            })
                            .ToList(),

                        DirectPermissions = u.UserPermissions
                            .Where(up =>
                                up.Permission.IsActive &&
                                !up.Permission.IsDeleted)
                            .Select(up => new
                            {
                                up.PermissionId,
                                up.Permission.Name
                            })
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Kullanıcılar alınırken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // CREATE USER
        // =====================================================

        // POST: api/Admin/users
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser(
            CreateUserRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new
                    {
                        message = "Kullanıcı adı ve şifre zorunludur."
                    });
                }

                var exists = await _context.Users
                    .AnyAsync(u =>
                        u.Username == request.Username &&
                        !u.IsDeleted);

                if (exists)
                {
                    return BadRequest(new
                    {
                        message = "Bu kullanıcı adı zaten kullanılıyor."
                    });
                }

                var user = new User
                {
                    Username = request.Username,
                    Password = request.Password,
                    IsActive = true,
                    IsDeleted = false,
                    ModifiedDate = DateTime.UtcNow
                };

                _context.Users.Add(user);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Kullanıcı başarıyla oluşturuldu.",
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Kullanıcı oluşturulurken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // UPDATE USER
        // =====================================================

        // PUT: api/Admin/users/5
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            UpdateUserRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Id == id &&
                        !u.IsDeleted);

                if (user == null)
                {
                    return NotFound(new
                    {
                        message = "Kullanıcı bulunamadı."
                    });
                }

                if (!string.IsNullOrWhiteSpace(request.Username))
                {
                    user.Username = request.Username;
                }

                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    user.Password = request.Password;
                }

                user.IsActive = request.IsActive;
                user.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Kullanıcı güncellendi."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Kullanıcı güncellenirken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // DELETE USER
        // =====================================================

        // DELETE: api/Admin/users/5
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new
                    {
                        message = "Kullanıcı bulunamadı."
                    });
                }

                // Soft delete
                user.IsDeleted = true;
                user.IsActive = false;
                user.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Kullanıcı sistemden çıkarıldı."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Kullanıcı silinirken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // ROLES
        // =====================================================

        // GET: api/Admin/roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .Where(r => !r.IsDeleted)
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .Select(r => new
                    {
                        r.Id,
                        r.Name,
                        r.Description,
                        r.IsActive,

                        Permissions = r.RolePermissions
                            .Where(rp =>
                                rp.Permission.IsActive &&
                                !rp.Permission.IsDeleted)
                            .Select(rp => new
                            {
                                rp.PermissionId,
                                rp.Permission.Name,
                                rp.Permission.Description
                            })
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Roller alınırken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // CREATE ROLE
        // =====================================================

        // POST: api/Admin/roles
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole(
            CreateRoleRequest request)
        {
            try
            {
                var exists = await _context.Roles
                    .AnyAsync(r =>
                        r.Name == request.Name &&
                        !r.IsDeleted);

                if (exists)
                {
                    return BadRequest(new
                    {
                        message = "Bu rol zaten mevcut."
                    });
                }

                var role = new Role
                {
                    Name = request.Name,
                    Description = request.Description,
                    IsActive = true,
                    IsDeleted = false
                };

                _context.Roles.Add(role);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Rol başarıyla oluşturuldu.",
                    roleId = role.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Rol oluşturulurken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // DELETE ROLE
        // =====================================================

        // DELETE: api/Admin/roles/5
        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (role == null)
                {
                    return NotFound(new
                    {
                        message = "Rol bulunamadı."
                    });
                }

                role.IsDeleted = true;
                role.IsActive = false;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Rol silindi."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Rol silinirken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // PERMISSIONS
        // =====================================================

        // GET: api/Admin/permissions
        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissions()
        {
            try
            {
                var permissions = await _context.Permissions
                    .Where(p => !p.IsDeleted)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.IsActive
                    })
                    .ToListAsync();

                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Yetkiler alınırken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // CREATE PERMISSION
        // =====================================================

        // POST: api/Admin/permissions
        [HttpPost("permissions")]
        public async Task<IActionResult> CreatePermission(
            CreatePermissionRequest request)
        {
            try
            {
                var exists = await _context.Permissions
                    .AnyAsync(p =>
                        p.Name == request.Name &&
                        !p.IsDeleted);

                if (exists)
                {
                    return BadRequest(new
                    {
                        message = "Bu yetki zaten mevcut."
                    });
                }

                var permission = new Permission
                {
                    Name = request.Name,
                    Description = request.Description,
                    IsActive = true,
                    IsDeleted = false
                };

                _context.Permissions.Add(permission);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Yetki başarıyla oluşturuldu.",
                    permissionId = permission.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Yetki oluşturulurken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // ASSIGN ROLE TO USER
        // =====================================================

        // POST: api/Admin/users/5/roles/1
        [HttpPost("users/{userId}/roles/{roleId}")]
        public async Task<IActionResult> AssignRole(
            int userId,
            int roleId)
        {
            try
            {
                var userExists = await _context.Users
                    .AnyAsync(u =>
                        u.Id == userId &&
                        !u.IsDeleted);

                var roleExists = await _context.Roles
                    .AnyAsync(r =>
                        r.Id == roleId &&
                        !r.IsDeleted &&
                        r.IsActive);

                if (!userExists)
                {
                    return NotFound(new
                    {
                        message = "Kullanıcı bulunamadı."
                    });
                }

                if (!roleExists)
                {
                    return NotFound(new
                    {
                        message = "Rol bulunamadı."
                    });
                }

                var alreadyExists =
                    await _context.UserRoles
                        .AnyAsync(ur =>
                            ur.UserId == userId &&
                            ur.RoleId == roleId);

                if (alreadyExists)
                {
                    return BadRequest(new
                    {
                        message = "Bu rol kullanıcıya zaten atanmış."
                    });
                }

                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Rol kullanıcıya atandı."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Rol atanırken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // REMOVE ROLE FROM USER
        // =====================================================

        // DELETE: api/Admin/users/5/roles/1
        [HttpDelete("users/{userId}/roles/{roleId}")]
        public async Task<IActionResult> RemoveRole(
            int userId,
            int roleId)
        {
            try
            {
                var userRole =
                    await _context.UserRoles
                        .FirstOrDefaultAsync(ur =>
                            ur.UserId == userId &&
                            ur.RoleId == roleId);

                if (userRole == null)
                {
                    return NotFound(new
                    {
                        message = "Kullanıcıda bu rol bulunamadı."
                    });
                }

                _context.UserRoles.Remove(userRole);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Rol kullanıcıdan kaldırıldı."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Rol kaldırılırken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // ASSIGN PERMISSION TO ROLE
        // =====================================================

        // POST: api/Admin/roles/1/permissions/2
        [HttpPost("roles/{roleId}/permissions/{permissionId}")]
        public async Task<IActionResult> AssignPermissionToRole(
            int roleId,
            int permissionId)
        {
            try
            {
                var exists =
                    await _context.RolePermissions
                        .AnyAsync(rp =>
                            rp.RoleId == roleId &&
                            rp.PermissionId == permissionId);

                if (exists)
                {
                    return BadRequest(new
                    {
                        message = "Bu yetki role zaten atanmış."
                    });
                }

                _context.RolePermissions.Add(
                    new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId
                    });

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Yetki role atandı."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Yetki role atanırken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // REMOVE PERMISSION FROM ROLE
        // =====================================================

        // DELETE: api/Admin/roles/1/permissions/2
        [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
        public async Task<IActionResult> RemovePermissionFromRole(
            int roleId,
            int permissionId)
        {
            try
            {
                var rolePermission =
                    await _context.RolePermissions
                        .FirstOrDefaultAsync(rp =>
                            rp.RoleId == roleId &&
                            rp.PermissionId == permissionId);

                if (rolePermission == null)
                {
                    return NotFound(new
                    {
                        message = "Bu yetki role atanmış değil."
                    });
                }

                _context.RolePermissions.Remove(rolePermission);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Yetki rolden kaldırıldı."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Yetki rolden kaldırılırken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // ASSIGN DIRECT PERMISSION TO USER
        // =====================================================

        // POST: api/Admin/users/5/permissions/2
        [HttpPost("users/{userId}/permissions/{permissionId}")]
        public async Task<IActionResult> AssignPermissionToUser(
            int userId,
            int permissionId)
        {
            try
            {
                // Kullanıcının rollerinden gelen yetkileri bul
                var permissionFromRole =
                    await _context.UserRoles
                        .Where(ur => ur.UserId == userId)
                        .SelectMany(ur =>
                            ur.Role.RolePermissions)
                        .AnyAsync(rp =>
                            rp.PermissionId == permissionId);

                // Yetki zaten rolden geliyorsa
                // tekrar kullanıcıya atanamaz.
                if (permissionFromRole)
                {
                    return BadRequest(new
                    {
                        message =
                            "Bu yetki kullanıcının rolünden zaten geliyor."
                    });
                }

                var alreadyExists =
                    await _context.UserPermissions
                        .AnyAsync(up =>
                            up.UserId == userId &&
                            up.PermissionId == permissionId);

                if (alreadyExists)
                {
                    return BadRequest(new
                    {
                        message =
                            "Bu yetki kullanıcıya zaten atanmış."
                    });
                }

                _context.UserPermissions.Add(
                    new UserPermission
                    {
                        UserId = userId,
                        PermissionId = permissionId
                    });

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Yetki kullanıcıya atandı."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Yetki kullanıcıya atanırken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // REMOVE DIRECT USER PERMISSION
        // =====================================================

        // DELETE: api/Admin/users/5/permissions/2
        [HttpDelete("users/{userId}/permissions/{permissionId}")]
        public async Task<IActionResult> RemovePermissionFromUser(
            int userId,
            int permissionId)
        {
            try
            {
                var userPermission =
                    await _context.UserPermissions
                        .FirstOrDefaultAsync(up =>
                            up.UserId == userId &&
                            up.PermissionId == permissionId);

                if (userPermission == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Bu yetki kullanıcıya doğrudan atanmış değil."
                    });
                }

                _context.UserPermissions.Remove(userPermission);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Kullanıcı yetkisi kaldırıldı."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Kullanıcı yetkisi kaldırılırken hata oluştu.",
                    error = ex.Message
                });
            }
        }


        // =====================================================
        // USER PERMISSION VIEW
        // =====================================================
        //
        // Kullanıcının:
        // 1. Rolünden gelen yetkileri
        // 2. Doğrudan atanmış yetkileri
        //
        // ayrı ayrı gösterir.
        //
        // =====================================================

        // GET: api/Admin/users/5/permissions
        [HttpGet("users/{userId}/permissions")]
        public async Task<IActionResult> GetUserPermissions(
            int userId)
        {
            try
            {
                var userExists =
                    await _context.Users
                        .AnyAsync(u =>
                            u.Id == userId &&
                            !u.IsDeleted);

                if (!userExists)
                {
                    return NotFound(new
                    {
                        message = "Kullanıcı bulunamadı."
                    });
                }


                // Rol üzerinden gelen yetkiler
                var rolePermissions =
                    await _context.UserRoles
                        .Where(ur => ur.UserId == userId)
                        .SelectMany(ur =>
                            ur.Role.RolePermissions)
                        .Where(rp =>
                            rp.Permission.IsActive &&
                            !rp.Permission.IsDeleted)
                        .Select(rp => new
                        {
                            rp.PermissionId,
                            rp.Permission.Name,
                            rp.Permission.Description,
                            Source = "Role"
                        })
                        .Distinct()
                        .ToListAsync();


                // Kullanıcıya doğrudan verilen yetkiler
                var directPermissions =
                    await _context.UserPermissions
                        .Where(up =>
                            up.UserId == userId &&
                            up.Permission.IsActive &&
                            !up.Permission.IsDeleted)
                        .Select(up => new
                        {
                            up.PermissionId,
                            up.Permission.Name,
                            up.Permission.Description,
                            Source = "User"
                        })
                        .ToListAsync();


                return Ok(new
                {
                    rolePermissions,
                    directPermissions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Kullanıcı yetkileri alınırken hata oluştu.",
                    error = ex.Message
                });
            }
        }
    }


    // =========================================================
    // REQUEST MODELS
    // =========================================================

    public class CreateUserRequest
    {
        public string Username { get; set; } = "";

        public string Password { get; set; } = "";
    }


    public class UpdateUserRequest
    {
        public string? Username { get; set; }

        public string? Password { get; set; }

        public bool IsActive { get; set; }
    }


    public class CreateRoleRequest
    {
        public string Name { get; set; } = "";

        public string? Description { get; set; }
    }


    public class CreatePermissionRequest
    {
        public string Name { get; set; } = "";

        public string? Description { get; set; }
    }
}