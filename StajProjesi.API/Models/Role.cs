namespace StajProjesi.API.Models;

public class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public bool IsDeleted { get; set; } = false;

    public bool IsActive { get; set; } = true;


    // =====================================================
    // ROLE - USER
    // =====================================================

    public ICollection<UserRole> UserRoles { get; set; }
        = new List<UserRole>();


    // =====================================================
    // ROLE - PERMISSION
    // =====================================================

    public ICollection<RolePermission> RolePermissions { get; set; }
        = new List<RolePermission>();
}