using StajProjesi.API.Models;

namespace StajProjesi.API.Services
{
    public interface ILineFeatureService
    {
        Task<List<LineFeature>> GetLinesAsync();

        Task<LineFeature> CreateLineAsync(
            List<CoordinateDto> coordinates);

        Task<bool> DeleteLineAsync(int id);
    }
}