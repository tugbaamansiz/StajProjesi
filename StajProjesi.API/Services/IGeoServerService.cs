namespace StajProjesi.API.Services
{
    public interface IGeoServerService
    {
        // =====================================================
        // GEOSERVER'DAN KULLANICIYA AİT KATMANLARI GETİR
        // =====================================================

        Task<string> GetFeaturesAsync(
            string layerName,
            int userId);


        // =====================================================
        // GEOSERVER'DAN TEK BİR FEATURE GETİR
        // =====================================================

        Task<string?> GetFeatureAsync(
            string layerName,
            int featureId,
            int userId);
    }
}