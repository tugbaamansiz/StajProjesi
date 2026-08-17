namespace StajProjesi.API.Models;

public class Permission
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public bool IsDeleted { get; set; } = false;

    public bool IsActive { get; set; } = true;


    // =====================================================
    // PERMISSION - ROLE
    // =====================================================

    public ICollection<RolePermission> RolePermissions { get; set; }
        = new List<RolePermission>();


    // =====================================================
    // PERMISSION - USER
    // =====================================================

    public ICollection<UserPermission> UserPermissions { get; set; }
        = new List<UserPermission>();
}