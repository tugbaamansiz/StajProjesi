using NetTopologySuite.Geometries;
using StajProjesi.API.Models;

namespace StajProjesi.API.Services
{
    public interface IGeographicPermissionService
    {
        // =====================================================
        // YETKİ ALANI OLUŞTUR
        // =====================================================

        Task<GeographicPermission> CreateAsync(
            int? userId,
            int? roleId,
            Geometry geometry,
            int insertedUserId);


        // =====================================================
        // KULLANICIYA AİT YETKİ ALANLARI
        // =====================================================

        Task<List<GeographicPermission>> GetByUserIdAsync(
            int userId);


        // =====================================================
        // ROLE AİT YETKİ ALANLARI
        // =====================================================

        Task<List<GeographicPermission>> GetByRoleIdAsync(
            int roleId);


        // =====================================================
        // KULLANICININ GEÇERLİ COĞRAFİ YETKİ ALANLARI
        // =====================================================
        // Kullanıcının kendi alanları +
        // rollerinden gelen alanlar
        // =====================================================

        Task<List<GeographicPermission>> GetUserAreasAsync(
            int userId);


        // =====================================================
        // GEOMETRİ KONTROLÜ
        // =====================================================
        // Verilen geometry kullanıcının yetki alanında mı?
        // =====================================================

        Task<bool> IsGeometryAllowedAsync(
            int userId,
            Geometry geometry);


        // =====================================================
        // ID İLE GETİR
        // =====================================================

        Task<GeographicPermission?> GetByIdAsync(
            int id);


        // =====================================================
        // SOFT DELETE
        // =====================================================

        Task<bool> DeleteAsync(
            int id);
    }
}