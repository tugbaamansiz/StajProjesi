using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace StajProjesi.API.Services
{
    public class GeoServerService : IGeoServerService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private const string GeoServerBaseUrl =
            "http://localhost:8080/geoserver";

        private const string Workspace =
            "staj_projesi";

        private readonly string[] _allowedLayers =
        {
            "tbl_point",
            "tbl_line",
            "tbl_polygon"
        };

        public GeoServerService(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // =========================================================
        // GEOSERVER'DAN KULLANICIYA AİT FEATURE'LARI GETİR
        // =========================================================

        public async Task<string> GetFeaturesAsync(
            string layerName,
            int userId)
        {
            ValidateLayer(layerName);

            var client =
                _httpClientFactory.CreateClient();

            var query =
                HttpUtility.ParseQueryString(string.Empty);

            query["service"] = "WFS";
            query["version"] = "1.0.0";
            query["request"] = "GetFeature";
            query["typeName"] =
                $"{Workspace}:{layerName}";
            query["outputFormat"] =
                "application/json";

            // GeoServer'a filtreyi gönderiyoruz.
            query["CQL_FILTER"] =
                $"inserted_user_id = {userId} AND " +
                $"is_deleted = false AND " +
                $"is_active = true";

            var url =
                $"{GeoServerBaseUrl}/ows?{query}";

            var response =
                await client.GetAsync(url);

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"GeoServer isteği başarısız oldu. " +
                    $"HTTP {(int)response.StatusCode}: {content}");
            }

            // =====================================================
            // ÖNEMLİ:
            // GeoServer filtreyi uygulamasa bile backend tarafında
            // ikinci kez kontrol ediyoruz.
            // =====================================================

            return FilterGeoServerResponse(
                content,
                userId);
        }

        // =========================================================
        // GEOSERVER'DAN TEK FEATURE GETİR
        // =========================================================

        public async Task<string?> GetFeatureAsync(
            string layerName,
            int featureId,
            int userId)
        {
            ValidateLayer(layerName);

            var client =
                _httpClientFactory.CreateClient();

            var query =
                HttpUtility.ParseQueryString(string.Empty);

            query["service"] = "WFS";
            query["version"] = "1.0.0";
            query["request"] = "GetFeature";
            query["typeName"] =
                $"{Workspace}:{layerName}";
            query["outputFormat"] =
                "application/json";

            // Feature ID + kullanıcı + aktif/silinmemiş kontrolü
            query["CQL_FILTER"] =
                $"inserted_user_id = {userId} AND " +
                $"is_deleted = false AND " +
                $"is_active = true";

            var url =
                $"{GeoServerBaseUrl}/ows?{query}";

            var response =
                await client.GetAsync(url);

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"GeoServer isteği başarısız oldu. " +
                    $"HTTP {(int)response.StatusCode}: {content}");
            }

            // Önce kullanıcı/aktif/silinmiş filtrelerini uygula
            var filteredContent =
                FilterGeoServerResponse(
                    content,
                    userId);

            // Sonra istediğimiz feature ID'sini bul
            try
            {
                var json =
                    JsonNode.Parse(filteredContent);

                var features =
                    json?["features"]?.AsArray();

                if (features == null)
                {
                    return null;
                }

                foreach (var feature in features)
                {
                    var idValue =
                        feature?["id"]?.ToString();

                    // GeoServer FID:
                    // tbl_point.27
                    // tbl_line.24
                    // tbl_polygon.XX
                    if (idValue != null &&
                        idValue.EndsWith(
                            $".{featureId}"))
                    {
                        var result =
                            new JsonObject
                            {
                                ["type"] = "FeatureCollection",
                                ["features"] =
                                    new JsonArray(feature.DeepClone()),
                                ["totalFeatures"] = 1,
                                ["numberMatched"] = 1,
                                ["numberReturned"] = 1
                            };

                        return result.ToJsonString();
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // =========================================================
        // GEOSERVER CEVABINI BACKEND TARAFINDA FİLTRELE
        // =========================================================

        private string FilterGeoServerResponse(
            string content,
            int userId)
        {
            try
            {
                var json =
                    JsonNode.Parse(content);

                if (json == null)
                {
                    return content;
                }

                var features =
                    json["features"]?.AsArray();

                if (features == null)
                {
                    return content;
                }

                var filteredFeatures =
                    new JsonArray();

                foreach (var feature in features)
                {
                    var properties =
                        feature?["properties"];

                    if (properties == null)
                    {
                        continue;
                    }

                    // ---------------------------------------------
                    // inserted_user_id
                    // ---------------------------------------------

                    var insertedUserId =
                        properties["inserted_user_id"]?
                            .GetValue<int?>();

                    // ---------------------------------------------
                    // is_deleted
                    // ---------------------------------------------

                    var isDeleted =
                        properties["is_deleted"]?
                            .GetValue<bool?>();

                    // ---------------------------------------------
                    // is_active
                    // ---------------------------------------------

                    var isActive =
                        properties["is_active"]?
                            .GetValue<bool?>();

                    // ---------------------------------------------
                    // SADECE:
                    //
                    // aynı kullanıcı
                    // is_deleted = false
                    // is_active = true
                    // ---------------------------------------------

                    if (insertedUserId == userId &&
                        isDeleted == false &&
                        isActive == true)
                    {
                        filteredFeatures.Add(
                            feature.DeepClone());
                    }
                }

                json["features"] =
                    filteredFeatures;

                json["totalFeatures"] =
                    filteredFeatures.Count;

                json["numberMatched"] =
                    filteredFeatures.Count;

                json["numberReturned"] =
                    filteredFeatures.Count;

                return json.ToJsonString();
            }
            catch
            {
                // JSON parse edilemezse orijinal cevabı döndür.
                return content;
            }
        }

        // =========================================================
        // KATMAN KONTROLÜ
        // =========================================================

        private void ValidateLayer(
            string layerName)
        {
            if (!_allowedLayers.Contains(
                    layerName))
            {
                throw new ArgumentException(
                    "Geçersiz GeoServer katmanı.",
                    nameof(layerName));
            }
        }
    }
}