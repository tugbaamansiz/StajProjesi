import TileLayer from "ol/layer/Tile";
import TileWMS from "ol/source/TileWMS";
import GeoJSON from "ol/format/GeoJSON";


// ============================================================
// GEOSERVER AYARLARI
// ============================================================

export const GEOSERVER_WMS_URL =
  "http://localhost:8080/geoserver/staj_projesi/wms";


// ============================================================
// GEOSERVER SQL VIEW KATMANLARI
// ============================================================

export const GEOSERVER_LAYERS = {
  point: "staj_projesi:point_view",
  line: "staj_projesi:line_view",
  polygon: "staj_projesi:polygon_view"
};


// ============================================================
// WMS KATMANI OLUŞTUR
//
// Genel harita gösteriminde WMS kullanılacak.
//
// Kullanıcı filtresi:
// inserted_user_id = userId
// ============================================================

export const createUserWmsLayer = (
  layerName,
  userId
) => {

  return new TileLayer({

    source: new TileWMS({

      url:
        GEOSERVER_WMS_URL,

      params: {

        SERVICE:
          "WMS",

        VERSION:
          "1.1.1",

        REQUEST:
          "GetMap",

        LAYERS:
          layerName,

        FORMAT:
          "image/png",

        TRANSPARENT:
          true,

        TILED:
          true,

        CQL_FILTER:
          `inserted_user_id = ${userId}`

      },

      serverType:
        "geoserver"

    }),

    visible:
      true

  });

};


// ============================================================
// WFS FEATURE'LARINI BACKEND ÜZERİNDEN GETİR
//
// Backend:
// /api/GeoServer/point
// /api/GeoServer/line
// /api/GeoServer/polygon
//
// Backend tarafında GeoServer WFS kullanılıyor.
//
// Böylece:
// WMS  -> genel gösterim
// WFS  -> etkileşim / seçim / düzenleme
// ============================================================

export const fetchWfsFeatures = async (
  featureType,
  token
) => {

  const endpointMap = {

    point:
      "point",

    line:
      "line",

    polygon:
      "polygon"

  };


  const endpoint =
    endpointMap[featureType];


  if (!endpoint) {

    throw new Error(
      "Geçersiz WFS feature tipi."
    );

  }


  const response =
    await fetch(
      `http://localhost:5166/api/GeoServer/${endpoint}`,
      {
        method: "GET",

        headers: {

          Authorization:
            `Bearer ${token}`

        }

      }
    );


  if (!response.ok) {

    const errorText =
      await response.text();

    throw new Error(
      `WFS verileri alınamadı. HTTP ${response.status}: ${errorText}`
    );

  }


  const geoJson =
    await response.json();


  // ==========================================================
  // GeoJSON -> OpenLayers Feature
  //
  // GeoServer:
  // EPSG:4326
  //
  // OpenLayers harita:
  // EPSG:3857
  // ==========================================================

  const format =
    new GeoJSON();


  const features =
    format.readFeatures(
      geoJson,
      {

        dataProjection:
          "EPSG:4326",

        featureProjection:
          "EPSG:3857"

      }
    );


  return features;

};


// ============================================================
// TOKEN'DAN USER ID AL
// ============================================================

export const getUserIdFromToken = (
  token
) => {

  if (!token) {

    return null;

  }


  try {

    const payload =
      JSON.parse(
        atob(
          token.split(".")[1]
        )
      );


    const possibleClaims = [

      "nameid",

      "sub",

      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",

      "http://schemas.microsoft.com/ws/2008/06/identity/claims/nameidentifier"

    ];


    for (
      const claim of possibleClaims
    ) {

      if (
        payload[claim] !== undefined &&
        payload[claim] !== null
      ) {

        const userId =
          Number(
            payload[claim]
          );


        if (
          Number.isInteger(
            userId
          )
        ) {

          return userId;

        }

      }

    }


    return null;

  }
  catch {

    return null;

  }

};