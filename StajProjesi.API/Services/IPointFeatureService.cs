using StajProjesi.API.Models;

namespace StajProjesi.API.Services
{
    public interface IPointFeatureService
    {
        Task<List<PointFeature>> GetPointsAsync(int userId);

        Task<PointFeature?> GetPointAsync(
            int id,
            int userId);

        Task<PointFeature> CreatePointAsync(
            double longitude,
            double latitude,
            string name,
            string color,
            int userId);

        Task<bool> UpdatePointAsync(
            int id,
            double longitude,
            double latitude,
            string name,
            string color,
            int userId);

        Task<bool> DeletePointAsync(
            int id,
            int userId);
    }
}