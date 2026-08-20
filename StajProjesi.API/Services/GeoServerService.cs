using System.Text.Json.Nodes;
using System.Web;

namespace StajProjesi.API.Services
{
    public class GeoServerService : IGeoServerService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // =========================================================
        // GEOSERVER
        // =========================================================

        private const string GeoServerBaseUrl =
            "http://localhost:8080/geoserver";

        private const string Workspace =
            "staj_projesi";


        // =========================================================
        // SADECE SQL VIEW KATMANLARINA İZİN VER
        // =========================================================

        private readonly string[] _allowedLayers =
        {
            "point_view",
            "line_view",
            "polygon_view"
        };


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public GeoServerService(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
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


            // =====================================================
            // WFS PARAMETRELERİ
            // =====================================================

            var query =
                HttpUtility.ParseQueryString(
                    string.Empty);


            query["service"] =
                "WFS";

            query["version"] =
                "1.0.0";

            query["request"] =
                "GetFeature";

            query["typeName"] =
                $"{Workspace}:{layerName}";

            query["outputFormat"] =
                "application/json";


            // =====================================================
            // CQL FILTER
            //
            // ÖNEMLİ:
            //
            // is_deleted ve is_active artık SQL View içerisinde.
            //
            // Burada sadece kullanıcı filtresi kullanıyoruz.
            // =====================================================

            query["cql_filter"] =
    $"inserted_user_id = {userId}";


            var url =
                $"{GeoServerBaseUrl}/ows?{query}";


            // =====================================================
            // GEOSERVER'A İSTEK
            // =====================================================

            var response =
                await client.GetAsync(url);


            var content =
                await response.Content.ReadAsStringAsync();


            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    "GeoServer isteği başarısız oldu. " +
                    $"HTTP {(int)response.StatusCode}: {content}");
            }


            // =====================================================
            // EK GÜVENLİK KONTROLÜ
            // =====================================================

            return FilterGeoServerResponse(
                content,
                userId);
        }


        // =========================================================
        // TEK FEATURE GETİR
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
                HttpUtility.ParseQueryString(
                    string.Empty);


            query["service"] =
                "WFS";

            query["version"] =
                "1.0.0";

            query["request"] =
                "GetFeature";

            query["typeName"] =
                $"{Workspace}:{layerName}";

            query["outputFormat"] =
                "application/json";


            // =====================================================
            // KULLANICI BAZLI CQL FILTER
            // =====================================================

            query["cql_filter"] =
    $"inserted_user_id = {userId}";


            var url =
                $"{GeoServerBaseUrl}/ows?{query}";


            var response =
                await client.GetAsync(url);


            var content =
                await response.Content.ReadAsStringAsync();


            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    "GeoServer isteği başarısız oldu. " +
                    $"HTTP {(int)response.StatusCode}: {content}");
            }


            // =====================================================
            // EK GÜVENLİK FİLTRESİ
            // =====================================================

            var filteredContent =
                FilterGeoServerResponse(
                    content,
                    userId);


            // =====================================================
            // FEATURE ID'SİNİ BUL
            // =====================================================

            try
            {
                var json =
                    JsonNode.Parse(
                        filteredContent);


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


                    // GeoServer FID örnekleri:
                    //
                    // point_view.1
                    // line_view.5
                    // polygon_view.12

                    if (idValue != null &&
                        idValue.EndsWith(
                            $".{featureId}"))
                    {
                        var result =
                            new JsonObject
                            {
                                ["type"] =
                                    "FeatureCollection",

                                ["features"] =
                                    new JsonArray(
                                        feature.DeepClone()),

                                ["totalFeatures"] =
                                    1,

                                ["numberMatched"] =
                                    1,

                                ["numberReturned"] =
                                    1
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
        // GEOSERVER CEVABINI EK OLARAK KONTROL ET
        // =========================================================

        private string FilterGeoServerResponse(
            string content,
            int userId)
        {
            try
            {
                var json =
                    JsonNode.Parse(
                        content);


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


                    // =================================================
                    // KULLANICI ID
                    // =================================================

                    var insertedUserId =
                        properties[
                            "inserted_user_id"]?
                        .GetValue<int?>();


                    // =================================================
                    // SOFT DELETE
                    // =================================================

                    var isDeleted =
                        properties[
                            "is_deleted"]?
                        .GetValue<bool?>();


                    // =================================================
                    // AKTİF Mİ?
                    // =================================================

                    var isActive =
                        properties[
                            "is_active"]?
                        .GetValue<bool?>();


                    // =================================================
                    // SADECE GEÇERLİ FEATURE'LAR
                    // =================================================

                    if (insertedUserId == userId &&
                        isDeleted == false &&
                        isActive == true)
                    {
                        filteredFeatures.Add(
                            feature.DeepClone());
                    }
                }


                // =================================================
                // FİLTRELENMİŞ SONUÇ
                // =================================================

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
                return content;
            }
        }


        // =========================================================
        // GEOSERVER KATMAN KONTROLÜ
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