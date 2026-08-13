using StajProjesi.API.Models;

namespace StajProjesi.API.Services
{
    public interface IPolygonFeatureService
    {
        Task<List<PolygonFeature>> GetPolygonsAsync();

        Task<PolygonFeature> CreatePolygonAsync(
            List<CoordinateDto> coordinates);

        Task<bool> DeletePolygonAsync(int id);
    }
}