using StajProjesi.API.Models;

namespace StajProjesi.API.Services
{
    public interface IPolygonFeatureService
    {
        Task<List<PolygonFeature>> GetPolygonsAsync(int userId);

        Task<PolygonFeature> CreatePolygonAsync(
            List<CoordinateDto> coordinates,
            string name,
            string color,
            int userId);

        Task<bool> UpdatePolygonAsync(
            int id,
            List<CoordinateDto> coordinates,
            string name,
            string color,
            int userId);

        Task<bool> DeletePolygonAsync(
            int id,
            int userId);
    }
}