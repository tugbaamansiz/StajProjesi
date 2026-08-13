using StajProjesi.API.Models;

namespace StajProjesi.API.Services
{
    public interface IPointFeatureService
    {
        Task<List<PointFeature>> GetPointsAsync();

        Task<PointFeature?> GetPointAsync(int id);

        Task<PointFeature> CreatePointAsync(
            double longitude,
            double latitude
        );

        Task<bool> DeletePointAsync(int id);
    }
}