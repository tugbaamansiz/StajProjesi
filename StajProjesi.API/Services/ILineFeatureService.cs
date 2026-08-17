using StajProjesi.API.Models;

namespace StajProjesi.API.Services
{
    public interface ILineFeatureService
    {
        Task<List<LineFeature>> GetLinesAsync(int userId);

        Task<LineFeature> CreateLineAsync(
            List<CoordinateDto> coordinates,
            string name,
            string color,
            int userId);

        Task<bool> UpdateLineAsync(
            int id,
            List<CoordinateDto> coordinates,
            string name,
            string color,
            int userId);

        Task<bool> DeleteLineAsync(
            int id,
            int userId);
    }
}