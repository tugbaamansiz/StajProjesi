using NetTopologySuite.Geometries;

namespace StajProjesi.API.Models
{
    public class GeographicPermission
    {
        public int Id { get; set; }

        // =====================================================
        // YETKİNİN AİT OLDUĞU KULLANICI
        // =====================================================

        public int? UserId { get; set; }

        // =====================================================
        // YETKİNİN AİT OLDUĞU ROL
        // =====================================================

        public int? RoleId { get; set; }

        // =====================================================
        // YETKİ ALANI
        // =====================================================

        public Geometry Geometry { get; set; } = null!;

        // =====================================================
        // TAKİP ALANLARI
        // =====================================================

        public int InsertedUserId { get; set; }

        public DateTime InsertedDate { get; set; }
            = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; }
            = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
            = false;

        public bool IsActive { get; set; }
            = true;
    }
}