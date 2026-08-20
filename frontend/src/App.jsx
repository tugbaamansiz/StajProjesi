import { useEffect, useRef, useState } from "react";

import { Button } from "primereact/button";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { Card } from "primereact/card";

import "primereact/resources/themes/lara-light-blue/theme.css";
import "primereact/resources/primereact.min.css";
import "primeicons/primeicons.css";

import Map from "ol/Map";
import View from "ol/View";

import TileLayer from "ol/layer/Tile";
import VectorLayer from "ol/layer/Vector";

import TileWMS from "ol/source/TileWMS";

import OSM from "ol/source/OSM";
import VectorSource from "ol/source/Vector";

import Feature from "ol/Feature";

import Point from "ol/geom/Point";
import LineString from "ol/geom/LineString";
import Polygon from "ol/geom/Polygon";

import Draw from "ol/interaction/Draw";
import Modify from "ol/interaction/Modify";
import Collection from "ol/Collection";

import Style from "ol/style/Style";
import Stroke from "ol/style/Stroke";
import Fill from "ol/style/Fill";
import CircleStyle from "ol/style/Circle";

import { fromLonLat, toLonLat } from "ol/proj";

import "ol/ol.css";
import "./App.css";

import basarsoftLogo from "./assets/basarsoft-logo.png";
import AdminPanel from "./AdminPanel";

import {
  GEOSERVER_WMS_URL,
  GEOSERVER_LAYERS,
  getUserIdFromToken
} from "./services/geoServerMap";


function App() {
  const [showPassword, setShowPassword] = useState(false);

  // =========================
  // STATE
  // =========================

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [remainingTime, setRemainingTime] = useState(0);

  // =========================
  // DETAY POPUP
  // =========================

  const [selectedFeature, setSelectedFeature] = useState(null);
  const [selectedName, setSelectedName] = useState("");
  const [showFeatureInfo, setShowFeatureInfo] = useState(false);
  const [selectedColor, setSelectedColor] = useState("#3388ff");
  const [popupPosition, setPopupPosition] = useState(null);
  const [isGeometryEditing, setIsGeometryEditing] = useState(false);
  const [savingFeature, setSavingFeature] = useState(false);

  // =========================
// ÖLÇÜM ARACI
// =========================
const [measurementMode, setMeasurementMode] = useState(null);
const [measurementResult, setMeasurementResult] = useState(null);
const measureDrawRef = useRef(null);

  // =========================
  // ENVANTERLERİ GÖSTER
  // =========================

  const [showInventoryList, setShowInventoryList] = useState(false);
  const [showHeatmap, setShowHeatmap] = useState(false);
  const [inventoryLoading, setInventoryLoading] = useState(false);
  const [inventoryData, setInventoryData] = useState({
    points: [],
    lines: [],
    polygons: []
  });

  const authHeaders = {
    Authorization: `Bearer ${token}`
  };

  // Backend bazı kayıtlarda soft-delete alanlarını döndürüyor.
  // Silinmiş veya pasif kayıtları frontend'de de göstermiyoruz.
  const isVisibleFeature = (item) => {
    if (!item) {
      return false;
    }

    const isDeleted =
      item.isDeleted === true || item.is_deleted === true;

    const isActive =
      item.isActive === false || item.is_active === false;

    return !isDeleted && !isActive;
  };

  // =========================
  // ADMIN KONTROLÜ
  // =========================

  const isAdmin = (() => {
    if (!token) {
      return false;
    }

    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      const role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

      if (Array.isArray(role)) {
        return role.includes("Admin");
      }

      return role === "Admin";
    }
    catch {
      return false;
    }
  })();

  // =========================
  // PERMISSION KONTROLÜ
  // =========================

  const permissions = (() => {
    if (!token) {
      return [];
    }

    try {
      const payload = JSON.parse(
        atob(token.split(".")[1])
      );

      const permissionClaim =
        payload["permission"];

      if (!permissionClaim) {
        return [];
      }

      if (Array.isArray(permissionClaim)) {
        return permissionClaim;
      }

      return [permissionClaim];
    }
    catch {
      return [];
    }
  })();

  const hasPermission = (permissionName) => {
    return permissions.includes(permissionName);
  };

  const [showAdminPanel, setShowAdminPanel] = useState(false);


  // =========================
  // REFS
  // =========================

  const mapRef = useRef(null);
  const mapElementRef = useRef(null);

  const vectorSourceRef = useRef(null);
  const drawRef = useRef(null);
  const modifyRef = useRef(null);
  const heatmapLayerRef = useRef(null);


  // =========================
  // LOGIN
  // =========================

  const handleLogin = async (e) => {

    e.preventDefault();

    setError("");
    setLoading(true);

    try {

      const response = await fetch(
        "http://localhost:5166/api/Auth/login",
        {
          method: "POST",

          headers: {
            "Content-Type": "application/json",
          },

          body: JSON.stringify({
            username: username,
            password: password,
          }),
        }
      );


      if (!response.ok) {

        throw new Error(
          "Kullanıcı adı veya şifre hatalı."
        );

      }


      const data = await response.json();


      localStorage.setItem(
        "token",
        data.token
      );

      setToken(data.token);

    }
    catch (err) {

      setError(err.message);

    }
    finally {

      setLoading(false);

    }

  };


  // =========================
  // LOGOUT
  // =========================

  const logout = () => {

    localStorage.removeItem("token");

    setToken(null);

  };


  // =========================
  // TÜM ÇİZİMLERİ TEMİZLE
  // =========================

  const clearAllFeatures = async () => {

    const confirmed = window.confirm(
      "Tüm nokta, çizgi ve alanları silmek istediğinize emin misiniz?"
    );


    if (!confirmed) {
      return;
    }


    try {

      const response = await fetch(
        "http://localhost:5166/api/ClearFeatures",
        {
          method: "DELETE",
          headers: authHeaders
        }
      );


      if (!response.ok) {

        throw new Error(
          "Çizimler silinemedi."
        );

      }


      if (vectorSourceRef.current) {

        vectorSourceRef.current.clear();

      }


      alert(
        "Tüm çizimler başarıyla silindi."
      );

    }
    catch (error) {

      console.error(
        "Çizimleri silme hatası:",
        error
      );

      alert(
        "Çizimler silinemedi."
      );

    }

  };


  // =========================
  // TOKEN + OTURUM SAYACI
  // =========================

  useEffect(() => {

    if (!token) {

      setRemainingTime(0);

      return;

    }


    try {

      const payload = JSON.parse(
        atob(token.split(".")[1])
      );


      const expirationTime =
        payload.exp * 1000;


      const updateTimer = () => {

        const remaining =
          expirationTime - Date.now();


        if (remaining <= 0) {

          setRemainingTime(0);

          logout();

          alert(
            "Oturum süreniz doldu. Lütfen tekrar giriş yapın."
          );

          return;

        }


        setRemainingTime(remaining);

      };


      updateTimer();


      const timer = setInterval(
        updateTimer,
        1000
      );


      return () => {

        clearInterval(timer);

      };

    }
    catch {

      logout();

    }

  }, [token]);


  // =====================================================
  // DETAY POPUP YARDIMCI FONKSİYONLARI
  // =====================================================

  const closeFeaturePopup = () => {
  if (modifyRef.current && mapRef.current) {
    mapRef.current.removeInteraction(modifyRef.current);
  }

  modifyRef.current = null;
  setIsGeometryEditing(false);
  setSelectedFeature(null);
  setPopupPosition(null);
  setShowFeatureInfo(false);
};

  const getFeatureType = (feature) => {
    return feature?.get("featureType") || "";
  };

  const getFeatureCoordinatesForApi = (feature) => {
    const geometry = feature.getGeometry();

    if (!geometry) {
      return null;
    }

    if (geometry instanceof Point) {
      const [longitude, latitude] = toLonLat(
        geometry.getCoordinates()
      );

      return {
        longitude,
        latitude
      };
    }

    if (geometry instanceof LineString) {
      return geometry.getCoordinates().map((coordinate) => {
        const [longitude, latitude] = toLonLat(coordinate);

        return {
          longitude,
          latitude
        };
      });
    }

    if (geometry instanceof Polygon) {
      return geometry.getCoordinates()[0].map((coordinate) => {
        const [longitude, latitude] = toLonLat(coordinate);

        return {
          longitude,
          latitude
        };
      });
    }

    return null;
  };

  const applyFeatureStyle = (feature, color, type) => {
    if (type === "Point") {
      feature.setStyle(
        new Style({
          image: new CircleStyle({
            radius: 7,
            fill: new Fill({
              color: color || "#3388ff"
            }),
            stroke: new Stroke({
              color: "#ffffff",
              width: 2
            })
          })
        })
      );
    }

    if (type === "LineString") {
      feature.setStyle(
        new Style({
          stroke: new Stroke({
            color: color || "#3388ff",
            width: 4
          })
        })
      );
    }

    if (type === "Polygon") {
      feature.setStyle(
        new Style({
          stroke: new Stroke({
            color: color || "#3388ff",
            width: 4
          }),
          fill: new Fill({
            color: color
              ? color + "40"
              : "rgba(51, 136, 255, 0.25)"
          })
        })
      );
    }
  };

  const startGeometryEditing = () => {
    if (!selectedFeature || !mapRef.current) {
      return;
    }

    if (modifyRef.current) {
      mapRef.current.removeInteraction(modifyRef.current);
    }

    const modify = new Modify({
      features: new Collection([selectedFeature])
    });

    modifyRef.current = modify;
    mapRef.current.addInteraction(modify);
    setIsGeometryEditing(true);
  };

  const finishGeometryEditing = () => {
    if (modifyRef.current && mapRef.current) {
      mapRef.current.removeInteraction(modifyRef.current);
    }

    modifyRef.current = null;
    setIsGeometryEditing(false);
  };

  const updateSelectedFeature = async () => {
    if (!selectedFeature || !token) {
      return;
    }

    const id = selectedFeature.get("featureId");
    const type = getFeatureType(selectedFeature);
    const coordinates = getFeatureCoordinatesForApi(selectedFeature);

    if (!id || !type || !coordinates) {
      alert("Çizim bilgileri bulunamadı.");
      return;
    }

    if (selectedName.trim() === "") {
      alert("Lütfen bir isim girin.");
      return;
    }

    if (!/^#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{3})$/.test(selectedColor.trim())) {
      alert("Lütfen geçerli bir HEX renk girin. Örnek: #ff0000");
      return;
    }

    const endpointMap = {
      Point: "PointFeatures",
      LineString: "LineFeatures",
      Polygon: "PolygonFeatures"
    };

    const endpoint = endpointMap[type];

    if (!endpoint) {
      return;
    }

    setSavingFeature(true);

    try {
      const response = await fetch(
        `http://localhost:5166/api/${endpoint}/${id}`,
        {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            ...authHeaders
          },
          body: JSON.stringify(
            type === "Point"
              ? {
                  longitude: coordinates.longitude,
                  latitude: coordinates.latitude,
                  name: selectedName.trim(),
                  color: selectedColor.trim()
                }
              : {
                  coordinates,
                  name: selectedName.trim(),
                  color: selectedColor.trim()
                }
          )
        }
      );

      if (!response.ok) {
        throw new Error("Çizim güncellenemedi.");
      }

      selectedFeature.set("name", selectedName.trim());
      selectedFeature.set("color", selectedColor.trim());
      applyFeatureStyle(
        selectedFeature,
        selectedColor.trim(),
        type
      );

      finishGeometryEditing();
      alert("Çizim başarıyla güncellendi.");
    }
    catch (error) {
      console.error("Çizim güncelleme hatası:", error);
      alert("Çizim güncellenemedi.");
    }
    finally {
      setSavingFeature(false);
    }
  };

  const deleteSelectedFeature = async () => {
    if (!selectedFeature || !token) {
      return;
    }

    const id = selectedFeature.get("featureId");
    const type = getFeatureType(selectedFeature);

    const confirmed = window.confirm(
      `"${selectedFeature.get("name") || "Bu çizim"}" adlı objeyi silmek istediğinize emin misiniz?`
    );

    if (!confirmed) {
      return;
    }

    const endpointMap = {
      Point: "PointFeatures",
      LineString: "LineFeatures",
      Polygon: "PolygonFeatures"
    };

    const endpoint = endpointMap[type];

    if (!id || !endpoint) {
      alert("Çizim bilgileri bulunamadı.");
      return;
    }

    try {
      const response = await fetch(
        `http://localhost:5166/api/${endpoint}/${id}`,
        {
          method: "DELETE",
          headers: authHeaders
        }
      );

      if (!response.ok) {
        throw new Error("Çizim silinemedi.");
      }

      vectorSourceRef.current?.removeFeature(
        selectedFeature
      );

      closeFeaturePopup();
      alert("Çizim başarıyla silindi.");
    }
    catch (error) {
      console.error("Çizim silme hatası:", error);
      alert("Çizim silinemedi.");
    }
  };

  // =========================
  // ENVANTERLERİ GETİR
  // =========================

  const openInventoryList = async () => {
    if (!token) {
      return;
    }

    setShowInventoryList(true);
    setInventoryLoading(true);

    try {
      const [pointsResponse, linesResponse, polygonsResponse] =
        await Promise.all([
          fetch(
            "http://localhost:5166/api/PointFeatures",
            {
              headers: authHeaders
            }
          ),
          fetch(
            "http://localhost:5166/api/LineFeatures",
            {
              headers: authHeaders
            }
          ),
          fetch(
            "http://localhost:5166/api/PolygonFeatures",
            {
              headers: authHeaders
            }
          )
        ]);

      if (
        !pointsResponse.ok ||
        !linesResponse.ok ||
        !polygonsResponse.ok
      ) {
        throw new Error("Envanterler alınamadı.");
      }

      const [points, lines, polygons] = await Promise.all([
        pointsResponse.json(),
        linesResponse.json(),
        polygonsResponse.json()
      ]);

      setInventoryData({
        points: points
          .filter(isVisibleFeature)
          .map(point => ({
            id: point.id,
            name: point.name || "İsimsiz",
            color: point.color || "#3388ff"
          })),
        lines: lines
          .filter(isVisibleFeature)
          .map(line => ({
            id: line.id,
            name: line.name || "İsimsiz",
            color: line.color || "#3388ff"
          })),
        polygons: polygons
          .filter(isVisibleFeature)
          .map(polygon => ({
            id: polygon.id,
            name: polygon.name || "İsimsiz",
            color: polygon.color || "#3388ff"
          }))
      });
    }
    catch (error) {
      console.error("Envanterleri getirme hatası:", error);

      setInventoryData({
        points: [],
        lines: [],
        polygons: []
      });

      alert("Envanterler alınamadı.");
    }
    finally {
      setInventoryLoading(false);
    }
  };


  // =========================
  // HARİTA
  // =========================

  useEffect(() => {

    if (
      !token ||
      !mapElementRef.current
    ) {

      return;

    }


    // =========================
    // VECTOR SOURCE
    // =========================

    const vectorSource =
      new VectorSource();


    vectorSourceRef.current =
      vectorSource;


    // =========================
    // VECTOR LAYER
    // =========================

    const vectorLayer =
      new VectorLayer({

        source:
          vectorSource,

      });


    // =========================
    // GEOSERVER HEATMAP WMS
    // =========================

    const userId =
      getUserIdFromToken(token);

    let heatmapLayer = null;

    if (userId !== null) {
      heatmapLayer =
        new TileLayer({

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
                GEOSERVER_LAYERS.point,

              STYLES:
                "heatmap_style",

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
            showHeatmap

        });

      heatmapLayerRef.current =
        heatmapLayer;
    }


    // =========================
    // MAP
    // =========================

    const map =
      new Map({

        target:
          mapElementRef.current,

        layers: [

          new TileLayer({

            source:
              new OSM(),

          }),

          vectorLayer,

          ...(heatmapLayer
            ? [heatmapLayer]
            : []),

        ],

        view: new View({

          center:
            fromLonLat([35, 39]),

          zoom: 6.1,

        }),

      });


    mapRef.current =
      map;


    // Haritanın ilk açılışta doğru
    // boyutlanmasını sağlar

    setTimeout(() => {

      map.updateSize();

    }, 100);


    // =========================
    // KAYITLI NOKTALARI GETİR
    // =========================

    const loadPoints =
      async () => {

        try {

          const response =
            await fetch(
              "http://localhost:5166/api/PointFeatures",
              {
                headers: authHeaders
              }
            );


          if (!response.ok) {
            return;
          }


          const points =
            await response.json();


          points
            .filter(isVisibleFeature)
            .forEach(point => {

            const feature =
              new Feature({

                geometry:
                  new Point(
                    fromLonLat([
                      point.coordinates.longitude,
                      point.coordinates.latitude
                    ])
                  )

              });

            feature.setId(point.id);
            feature.setProperties({
              featureId: point.id,
              featureType: "Point",
              name: point.name || "",
              color: point.color || "#3388ff"
            });

            feature.setStyle(
              new Style({
                image:
                  new CircleStyle({
                    radius: 7,
                    fill:
                      new Fill({
                        color:
                          point.color || "#3388ff"
                      }),
                    stroke:
                      new Stroke({
                        color: "#ffffff",
                        width: 2
                      })
                  })
              })
            );

            vectorSource.addFeature(
              feature
            );

          });

        }
        catch (error) {

          console.error(
            "Noktalar yüklenemedi:",
            error
          );

        }

      };


    // =========================
    // KAYITLI ÇİZGİLERİ GETİR
    // =========================

    const loadLines =
      async () => {

        try {

          const response =
            await fetch(
              "http://localhost:5166/api/LineFeatures",
              {
                headers: authHeaders
              }
            );


          if (!response.ok) {
            return;
          }


          const lines =
            await response.json();


          lines
            .filter(isVisibleFeature)
            .forEach(line => {

            const coordinates =
              line.coordinates.map(
                coordinate =>
                  fromLonLat([
                    coordinate.longitude,
                    coordinate.latitude
                  ])
              );


            const feature =
              new Feature({

                geometry:
                  new LineString(
                    coordinates
                  )

              });

            feature.setId(line.id);
            feature.setProperties({
              featureId: line.id,
              featureType: "LineString",
              name: line.name || "",
              color: line.color || "#3388ff"
            });

            feature.setStyle(
              new Style({
                stroke:
                  new Stroke({
                    color:
                      line.color || "#3388ff",
                    width: 4
                  })
              })
            );

            vectorSource.addFeature(
              feature
            );

          });

        }
        catch (error) {

          console.error(
            "Çizgiler yüklenemedi:",
            error
          );

        }

      };


    // =========================
    // KAYITLI POLYGONLARI GETİR
    // =========================

    const loadPolygons =
      async () => {

        try {

          const response =
            await fetch(
              "http://localhost:5166/api/PolygonFeatures",
              {
                headers: authHeaders
              }
            );


          if (!response.ok) {
            return;
          }


          const polygons =
            await response.json();


          polygons
            .filter(isVisibleFeature)
            .forEach(polygon => {

            const coordinates =
              polygon.coordinates.map(
                coordinate =>
                  fromLonLat([
                    coordinate.longitude,
                    coordinate.latitude
                  ])
              );


            const feature =
              new Feature({

                geometry:
                  new Polygon([
                    coordinates
                  ])

              });

            feature.setId(polygon.id);
            feature.setProperties({
              featureId: polygon.id,
              featureType: "Polygon",
              name: polygon.name || "",
              color: polygon.color || "#3388ff"
            });

            feature.setStyle(
              new Style({
                stroke:
                  new Stroke({
                    color:
                      polygon.color || "#3388ff",
                    width: 4
                  }),
                fill:
                  new Fill({
                    color:
                      polygon.color
                        ? polygon.color + "40"
                        : "rgba(51, 136, 255, 0.25)"
                  })
              })
            );

            vectorSource.addFeature(
              feature
            );

          });

        }
        catch (error) {

          console.error(
            "Polygonlar yüklenemedi:",
            error
          );

        }

      };


    // =========================
    // HEPSİNİ YÜKLE
    // =========================

    loadPoints();
    loadLines();
    loadPolygons();


    // =====================================================
    // HARİTADA OBJENİN ÜZERİNE TIKLAYINCA DETAY POPUP
    // =====================================================

    const handleMapClick = (event) => {
      if (drawRef.current) {
        return;
      }

      let clickedFeature = null;

      map.forEachFeatureAtPixel(
        event.pixel,
        (feature) => {
          if (feature.get("featureId")) {
            clickedFeature = feature;
            return true;
          }
          return false;
        },
        { hitTolerance: 6 }
      );

      if (!clickedFeature) {
        closeFeaturePopup();
        return;
      }

      const color = clickedFeature.get("color") || "#3388ff";
      const name = clickedFeature.get("name") || "";

      setSelectedFeature(clickedFeature);
      setSelectedName(name);
      setSelectedColor(color);
      setPopupPosition(event.pixel);
      setIsGeometryEditing(false);
      setShowFeatureInfo(true);
    };

    map.on("singleclick", handleMapClick);


    // =========================
    // TEMİZLEME
    // =========================

    return () => {

      map.un("singleclick", handleMapClick);

      if (modifyRef.current) {
        map.removeInteraction(modifyRef.current);
        modifyRef.current = null;
      }

      map.setTarget(null);

      mapRef.current = null;

      vectorSourceRef.current = null;
      heatmapLayerRef.current = null;

    };

  }, [token, showAdminPanel]);


  // =========================
  // HEATMAP GÖRÜNÜRLÜĞÜ
  // =========================

  useEffect(() => {

    if (!heatmapLayerRef.current) {
      return;
    }

    heatmapLayerRef.current.setVisible(
      showHeatmap
    );

  }, [showHeatmap]);


  // =====================================================
// GEÇİCİ ENVANTER ANALİZİ
// =====================================================

const startInventoryAnalysis = () => {

  if (!mapRef.current || !vectorSourceRef.current) {
    return;
  }

  // Önceki çizim varsa kaldır
  if (drawRef.current) {
    mapRef.current.removeInteraction(
      drawRef.current
    );
  }

  const draw =
    new Draw({
      source:
        vectorSourceRef.current,
      type:
        "Polygon"
    });

  drawRef.current =
    draw;

  mapRef.current.addInteraction(
    draw
  );

  draw.on(
    "drawend",
    async (event) => {

      const geometry =
        event.feature.getGeometry();

      const coordinates =
        geometry.getCoordinates()[0];

      const convertedCoordinates =
        coordinates.map(
          coordinate => {

            const [
              longitude,
              latitude
            ] =
              toLonLat(
                coordinate
              );

            return {
              longitude:
                longitude,
              latitude:
                latitude
            };
          }
        );

      try {

        const response =
          await fetch(
            "http://localhost:5166/api/Analysis/inventory",
            {
              method:
                "POST",

              headers: {
                "Content-Type":
                  "application/json",
                ...authHeaders
              },

              body:
                JSON.stringify({
                  coordinates:
                    convertedCoordinates
                })
            }
          );

        if (!response.ok) {
          throw new Error(
            "Envanter analizi yapılamadı."
          );
        }

        const result =
          await response.json();

        const pointNames =
          (result.pointNames || [])
            .map(name => "• " + (name || "İsimsiz"))
            .join("\n") || "• Yok";

        const lineNames =
          (result.lineNames || [])
            .map(name => "• " + (name || "İsimsiz"))
            .join("\n") || "• Yok";

        const polygonNames =
          (result.polygonNames || [])
            .map(name => "• " + (name || "İsimsiz"))
            .join("\n") || "• Yok";

        alert(
          "Envanter Analizi Sonucu:\n\n" +
          "📍 Kesişen noktalar: " + result.pointCount + "\n" +
          pointNames + "\n\n" +
          "📏 Kesişen çizgiler: " + result.lineCount + "\n" +
          lineNames + "\n\n" +
          "🔷 Kesişen alanlar: " + result.polygonCount + "\n" +
          polygonNames + "\n\n" +
          "Toplam envanter: " + result.totalCount
        );

      }
      catch (error) {

        console.error(
          "Envanter analizi hatası:",
          error
        );

        alert(
          "Envanter analizi yapılamadı."
        );
      }

      // Geçici polygonu haritadan kaldır
      vectorSourceRef.current?.removeFeature(
        event.feature
      );

      // Çizim aracını kapat
      mapRef.current?.removeInteraction(
        draw
      );

      drawRef.current =
        null;
    }
  );
};

// =====================================================
// ÖLÇÜM ARACI
// =====================================================

const stopMeasurement = () => {
  if (measureDrawRef.current && mapRef.current) {
    mapRef.current.removeInteraction(
      measureDrawRef.current
    );
  }

  measureDrawRef.current = null;
  setMeasurementMode(null);
};

const startMeasurement = (type) => {

  // Admin ölçüm araçlarını her zaman kullanabilir.
  // Diğer kullanıcılar ilgili permission'a sahip olmalı.
  if (
    !isAdmin &&
    type === "distance" &&
    !hasPermission("DISTANCE_MEASURE")
  ) {
    alert("Mesafe ölçme yetkiniz bulunmuyor.");
    return;
  }

  if (
    !isAdmin &&
    type === "area" &&
    !hasPermission("AREA_MEASURE")
  ) {
    alert("Alan ölçme yetkiniz bulunmuyor.");
    return;
  }

  if (!mapRef.current) {
    return;
  }
  

  // Normal çizim varsa kapat
  if (drawRef.current) {
    mapRef.current.removeInteraction(
      drawRef.current
    );

    drawRef.current = null;
  }

  // Önceki ölçümü kapat
  stopMeasurement();

  setMeasurementMode(type);
  setMeasurementResult(null);

  const draw = new Draw({
    source: new VectorSource(),
    type: type === "distance"
      ? "LineString"
      : "Polygon"
  });

  measureDrawRef.current = draw;

  mapRef.current.addInteraction(draw);

  draw.on("drawend", (event) => {
    const geometry =
      event.feature.getGeometry();

    let result = "";

    if (type === "distance") {
      const coordinates =
        geometry.getCoordinates();

      let totalDistance = 0;

      for (let i = 1; i < coordinates.length; i++) {
        const start = toLonLat(
          coordinates[i - 1]
        );

        const end = toLonLat(
          coordinates[i]
        );

        const R = 6371008.8;

        const lat1 =
          start[1] * Math.PI / 180;

        const lat2 =
          end[1] * Math.PI / 180;

        const deltaLat =
          (end[1] - start[1]) *
          Math.PI / 180;

        const deltaLon =
          (end[0] - start[0]) *
          Math.PI / 180;

        const a =
          Math.sin(deltaLat / 2) ** 2 +
          Math.cos(lat1) *
          Math.cos(lat2) *
          Math.sin(deltaLon / 2) ** 2;

        const c =
          2 *
          Math.atan2(
            Math.sqrt(a),
            Math.sqrt(1 - a)
          );

        totalDistance += R * c;
      }

      if (totalDistance >= 1000) {
        result =
          `Mesafe: ${(totalDistance / 1000).toFixed(2)} km`;
      } else {
        result =
          `Mesafe: ${totalDistance.toFixed(2)} m`;
      }
    }

    if (type === "area") {
      const coordinates =
        geometry.getCoordinates()[0];

      let area = 0;

      for (
        let i = 0;
        i < coordinates.length - 1;
        i++
      ) {
        const p1 =
          toLonLat(coordinates[i]);

        const p2 =
          toLonLat(coordinates[i + 1]);

        area +=
          p1[0] * p2[1] -
          p2[0] * p1[1];
      }

      area =
        Math.abs(area) *
        12364.0;

      result =
        `Alan: ${area.toFixed(2)} km²`;
    }

    setMeasurementResult(result);

    if (mapRef.current) {
      mapRef.current.removeInteraction(draw);
    }

    measureDrawRef.current = null;
    setMeasurementMode(null);
  });
};


  // =========================
  // ÇİZİM BAŞLAT
  // =========================

  const startDrawing =
    (type) => {

      if (!mapRef.current) {
        return;
      }


      // Önceki çizimi kaldır

      if (drawRef.current) {

        mapRef.current.removeInteraction(
          drawRef.current
        );

      }


      const draw =
        new Draw({

          source:
            vectorSourceRef.current,

          type:
            type,

        });


      drawRef.current =
        draw;


      mapRef.current.addInteraction(
        draw
      );


      // =========================
      // ÇİZİM TAMAMLANDI
      // =========================

      draw.on(
        "drawend",
        async (event) => {

          const geometry =
            event.feature.getGeometry();

          // =========================================
          // KULLANICIDAN İSİM AL
          // =========================================

          const name =
            window.prompt(
              "Çizimin ismini girin:"
            );

          // Kullanıcı İptal'e basarsa çizimi kaldır
          if (name === null) {

            vectorSourceRef.current?.removeFeature(
              event.feature
            );

            mapRef.current?.removeInteraction(
              draw
            );

            drawRef.current = null;

            return;
          }

          // Boş isim girilmesini engelle
          if (name.trim() === "") {

            vectorSourceRef.current?.removeFeature(
              event.feature
            );

            alert(
              "Lütfen bir isim girin."
            );

            mapRef.current?.removeInteraction(
              draw
            );

            drawRef.current = null;

            return;
          }

          // =========================================
          // KULLANICIDAN RENK AL
          // =========================================

          const color =
            window.prompt(
              "Çizimin rengini girin.\nÖrnek: #ff0000",
              "#3388ff"
            );

          // Kullanıcı İptal'e basarsa çizimi kaldır
          if (color === null) {

            vectorSourceRef.current?.removeFeature(
              event.feature
            );

            mapRef.current?.removeInteraction(
              draw
            );

            drawRef.current = null;

            return;
          }

          const trimmedColor =
            color.trim();

          // Basit HEX renk kontrolü
          if (!/^#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{3})$/.test(trimmedColor)) {

            vectorSourceRef.current?.removeFeature(
              event.feature
            );

            alert(
              "Lütfen geçerli bir HEX renk girin. Örnek: #ff0000"
            );

            mapRef.current?.removeInteraction(
              draw
            );

            drawRef.current = null;

            return;
          }

          // =========================================
          // ÇİZİME SEÇİLEN RENGİ UYGULA
          // =========================================

          if (type === "Point") {

            event.feature.setStyle(
              new Style({
                image:
                  new CircleStyle({
                    radius: 7,
                    fill:
                      new Fill({
                        color: trimmedColor
                      }),
                    stroke:
                      new Stroke({
                        color: "#ffffff",
                        width: 2
                      })
                  })
              })
            );

          }

          if (type === "LineString") {

            event.feature.setStyle(
              new Style({
                stroke:
                  new Stroke({
                    color: trimmedColor,
                    width: 4
                  })
              })
            );

          }

          if (type === "Polygon") {

            event.feature.setStyle(
              new Style({
                stroke:
                  new Stroke({
                    color: trimmedColor,
                    width: 4
                  }),
                fill:
                  new Fill({
                    color: trimmedColor + "40"
                  })
              })
            );

          }

          // =========================================
          // POINT
          // =========================================

          if (type === "Point") {

            const coordinates =
              geometry.getCoordinates();

            const [
              longitude,
              latitude
            ] =
              toLonLat(
                coordinates
              );

            try {

              const response =
                await fetch(
                  "http://localhost:5166/api/PointFeatures",
                  {
                    method: "POST",

                    headers: {
                      "Content-Type":
                        "application/json",
                      ...authHeaders
                    },

                    body: JSON.stringify({
                      longitude: longitude,
                      latitude: latitude,
                      name: name.trim(),
                      color: trimmedColor
                    })
                  }
                );

              if (!response.ok) {
                throw new Error(
                  "Point kaydedilemedi."
                );
              }

              const savedPoint = await response.json();

              event.feature.setId(savedPoint.id);
              event.feature.setProperties({
                featureId: savedPoint.id,
                featureType: "Point",
                name: name.trim(),
                color: trimmedColor
              });

              console.log(
                "Point kaydedildi."
              );

            }
            catch (error) {

              console.error(
                "Point kaydetme hatası:",
                error
              );

              vectorSourceRef.current?.removeFeature(
                event.feature
              );

              alert(
                "Point kaydedilemedi."
              );
            }
          }

          // =========================================
          // LINE
          // =========================================

          if (type === "LineString") {

            const coordinates =
              geometry.getCoordinates();

            const convertedCoordinates =
              coordinates.map(
                coordinate => {

                  const [
                    longitude,
                    latitude
                  ] =
                    toLonLat(
                      coordinate
                    );

                  return {
                    longitude: longitude,
                    latitude: latitude
                  };
                }
              );

            try {

              const response =
                await fetch(
                  "http://localhost:5166/api/LineFeatures",
                  {
                    method: "POST",

                    headers: {
                      "Content-Type":
                        "application/json",
                      ...authHeaders
                    },

                    body: JSON.stringify({
                      coordinates:
                        convertedCoordinates,
                      name: name.trim(),
                      color: trimmedColor
                    })
                  }
                );

              if (!response.ok) {
                throw new Error(
                  "Line kaydedilemedi."
                );
              }

              const savedLine = await response.json();

              event.feature.setId(savedLine.id);
              event.feature.setProperties({
                featureId: savedLine.id,
                featureType: "LineString",
                name: name.trim(),
                color: trimmedColor
              });

              console.log(
                "Line kaydedildi."
              );

            }
            catch (error) {

              console.error(
                "Line kaydetme hatası:",
                error
              );

              vectorSourceRef.current?.removeFeature(
                event.feature
              );

              alert(
                "Line kaydedilemedi."
              );
            }
          }

          // =========================================
          // POLYGON
          // =========================================

          if (type === "Polygon") {

            const coordinates =
              geometry.getCoordinates()[0];

            const convertedCoordinates =
              coordinates.map(
                coordinate => {

                  const [
                    longitude,
                    latitude
                  ] =
                    toLonLat(
                      coordinate
                    );

                  return {
                    longitude: longitude,
                    latitude: latitude
                  };
                }
              );

            try {

              const response =
                await fetch(
                  "http://localhost:5166/api/PolygonFeatures",
                  {
                    method: "POST",

                    headers: {
                      "Content-Type":
                        "application/json",
                      ...authHeaders
                    },

                    body: JSON.stringify({
                      coordinates:
                        convertedCoordinates,
                      name: name.trim(),
                      color: trimmedColor
                    })
                  }
                );

              if (!response.ok) {
                throw new Error(
                  "Polygon kaydedilemedi."
                );
              }

              const savedPolygon = await response.json();

              event.feature.setId(savedPolygon.id);
              event.feature.setProperties({
                featureId: savedPolygon.id,
                featureType: "Polygon",
                name: name.trim(),
                color: trimmedColor
              });

              console.log(
                "Polygon kaydedildi."
              );

              // =========================================
              // POLYGON İLE KESİŞEN ENVANTERLERİ BUL
              // =========================================

              try {
                const analysisResponse =
                  await fetch(
                    "http://localhost:5166/api/Analysis/intersection",
                    {
                      method: "POST",

                      headers: {
                        "Content-Type":
                          "application/json",
                        ...authHeaders
                      },

                      body: JSON.stringify({
                        coordinates:
                          convertedCoordinates
                      })
                    }
                  );

                if (!analysisResponse.ok) {
                  throw new Error(
                    "Polygon envanter analizi yapılamadı."
                  );
                }

                const analysisResult =
                  await analysisResponse.json();

                const pointNames =
                  (analysisResult.pointNames || [])
                    .map(
                      name =>
                        "• " +
                        (name || "İsimsiz")
                    )
                    .join("\n") || "• Yok";

                const lineNames =
                  (analysisResult.lineNames || [])
                    .map(
                      name =>
                        "• " +
                        (name || "İsimsiz")
                    )
                    .join("\n") || "• Yok";

                const polygonNameList =
                  (analysisResult.polygonNames || [])
                    .filter(
                      name =>
                        name !== savedPolygon.name
                    );

                const polygonNames =
                  polygonNameList
                    .map(
                      name =>
                        "• " +
                        (name || "İsimsiz")
                    )
                    .join("\n") || "• Yok";

                const pointCount =
                  analysisResult.pointCount || 0;

                const lineCount =
                  analysisResult.lineCount || 0;

                const polygonCount =
                  polygonNameList.length;

                const totalCount =
                  pointCount +
                  lineCount +
                  polygonCount;

                alert(
                  "Alan başarıyla kaydedildi.\n\n" +
                  "Bu alanla kesişen envanterler:\n\n" +
                  "📍 Noktalar (" +
                  pointCount +
                  "):\n" +
                  pointNames +
                  "\n\n" +
                  "📏 Çizgiler (" +
                  lineCount +
                  "):\n" +
                  lineNames +
                  "\n\n" +
                  "🔷 Alanlar (" +
                  polygonCount +
                  "):\n" +
                  polygonNames +
                  "\n\n" +
                  "Toplam envanter: " +
                  totalCount
                );
              }
              catch (analysisError) {
                console.error(
                  "Polygon envanter analizi hatası:",
                  analysisError
                );

                alert(
                  "Alan kaydedildi fakat kesişen envanterler analiz edilemedi."
                );
              }

            }
            catch (error) {

              console.error(
                "Polygon kaydetme hatası:",
                error
              );

              vectorSourceRef.current?.removeFeature(
                event.feature
              );

              alert(
                "Polygon kaydedilemedi."
              );
            }
          }

          // =========================================
          // ÇİZİM BİTTİ
          // =========================================

          if (mapRef.current) {
            mapRef.current.removeInteraction(
              draw
            );
          }

          drawRef.current = null;
        }
      );

    };
  {/* =========================================
    LOGIN SAYFASI
    ========================================= */}

if (!token) {
    return (
        <div className="login-page">

            {/* Harita dekorasyonları */}
            <div className="map-decoration map-decoration-left"></div>
            <div className="map-decoration map-decoration-right"></div>

            <div className="login-card">

                {/* LOGO */}
                <div className="login-logo">
                    <img
                        src={basarsoftLogo}
                        alt="Başarsoft"
                    />
                </div>

                {/* ALT BAŞLIK */}
                <p className="subtitle">
                    Harita ve Envanter Yönetim Sistemi
                </p>

                <form onSubmit={handleLogin}>

                    {/* KULLANICI ADI */}
                    <div className="input-group">

                        <label htmlFor="username">
                            Kullanıcı Adı
                        </label>

                        <div className="login-input-wrapper">

                            <span className="login-input-icon">
                                <i className="pi pi-user"></i>
                            </span>

                            <input
                                id="username"
                                type="text"
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                                placeholder="Kullanıcı adınızı giriniz"
                            />

                        </div>
                    </div>


                    {/* ŞİFRE */}
                    <div className="input-group">

                        <label htmlFor="password">
                            Şifre
                        </label>

                        <div className="login-input-wrapper">

                            <span className="login-input-icon">
                                <i className="pi pi-lock"></i>
                            </span>

                            <input
                                id="password"
                                type={showPassword ? "text" : "password"}
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                placeholder="Şifrenizi giriniz"
                            />

                            <button
                                type="button"
                                className="password-toggle"
                                onClick={() =>
                                    setShowPassword(!showPassword)
                                }
                            >
                                <i
                                    className={
                                        showPassword
                                            ? "pi pi-eye-slash"
                                            : "pi pi-eye"
                                    }
                                ></i>
                            </button>

                        </div>
                    </div>


                  {/* GİRİŞ BUTONU */}

<Button
  type="submit"
  label={
    loading
      ? "Giriş yapılıyor..."
      : "Giriş Yap"
  }
  icon={
    loading
      ? "pi pi-spin pi-spinner"
      : "pi pi-sign-in"
  }
  loading={loading}
  disabled={loading}
  className="login-button"
/>


{/* KAYDOL BUTONU */}

<button
  type="button"
  className="register-button"
>
  <i className="pi pi-user-plus"></i>
  <span>Kaydol</span>
</button>

</form>


                {/* GÜVENLİ GİRİŞ */}
                <div className="login-info">
                    <i className="pi pi-shield"></i>
                    <span>JWT ile güvenli giriş</span>
                </div>

            </div>
        </div>
    );
}


  // =====================================================
  // ADMIN PANELİ
  // =====================================================

  if (showAdminPanel && isAdmin) {
    return (
      <AdminPanel
        token={token}
        onBack={() => setShowAdminPanel(false)}
      />
    );
  }


  // =====================================================
  // HARİTA SAYFASI
  // =====================================================

  return (

    <div className="map-page">


      {/* HEADER */}

      <header className="map-header">


        <div>

          <h1>
            🗺️ StajProjesi
          </h1>


          <span>
            Türkiye Haritası
          </span>

        </div>


        {/* OTURUM SÜRESİ */}

        <div className="session-timer">

          ⏱️ Oturum{" "}

          {Math.floor(
            remainingTime / 60000
          )
            .toString()
            .padStart(2, "0")}

          :

          {Math.floor(
            (remainingTime % 60000) / 1000
          )
            .toString()
            .padStart(2, "0")}

        </div>





        {/* BUTONLAR */}

        <div className="map-buttons">


          {hasPermission("POINT_CREATE") && (
            <button
              onClick={() =>
                startDrawing("Point")
              }
            >
              📍 Nokta
            </button>
          )}


          {hasPermission("LINE_CREATE") && (
            <button
              onClick={() =>
                startDrawing("LineString")
              }
            >
              📏 Çizgi
            </button>
          )}


          {hasPermission("POLYGON_CREATE") && (
            <button
              onClick={() =>
                startDrawing("Polygon")
              }
            >
              🔷 Alan
            </button>
          )}

          <button
  onClick={
    startInventoryAnalysis
  }
>
  🔍 Envanter Analizi
</button>

{(isAdmin || hasPermission("DISTANCE_MEASURE")) && (
  <button
    onClick={() => startMeasurement("distance")}
  >
    📏 Mesafe Ölç
  </button>
)}

{(isAdmin || hasPermission("AREA_MEASURE")) && (
  <button
    onClick={() => startMeasurement("area")}
  >
    📐 Alan Ölç
  </button>
)}

{measurementMode && (
  <button
    onClick={stopMeasurement}
    className="clear-button"
  >
    ✖️ Ölçümü İptal Et
  </button>
)}


          <button
            onClick={openInventoryList}
          >
            📋 Envanterleri Göster
          </button>


          <button
            onClick={() =>
              setShowHeatmap(
                current => !current
              )
            }
            className={
              showHeatmap
                ? "heatmap-button active"
                : "heatmap-button"
            }
          >
            {showHeatmap
              ? "🔥 Heatmap Kapat"
              : "🔥 Heatmap"}
          </button>


          <button
            onClick={
              clearAllFeatures
            }
            className="clear-button"
          >
            🗑️ Tümünü Temizle
          </button>


          {isAdmin && (
            <button
              onClick={() => setShowAdminPanel(true)}
            >
              🛡️ Admin Paneli
            </button>
          )}


          <button
            onClick={logout}
            className="logout-button"
          >
            Çıkış Yap
          </button>


        </div>

      </header>


      {/* HARİTA */}

      <main className="map-container">


        <div
          ref={mapElementRef}
          className="map"
        >
        </div>


{/* =====================================================
    FEATURE BİLGİ PANELİ
    ===================================================== */}

{showFeatureInfo && selectedFeature && (
  <div className="feature-info-panel">

    <div className="feature-info-header">
      <div>
        <strong>Feature Bilgileri</strong>

        <span>
          {getFeatureType(selectedFeature) === "Point"
            ? "📍 Nokta"
            : getFeatureType(selectedFeature) === "LineString"
              ? "📏 Çizgi"
              : "🔷 Alan"}
        </span>
      </div>

      <button
        type="button"
        className="feature-info-close"
        onClick={() => setShowFeatureInfo(false)}
      >
        ×
      </button>
    </div>

    <div className="feature-info-content">

      <div className="feature-info-row">
        <span>ID</span>
        <strong>
          {selectedFeature.get("featureId") || "-"}
        </strong>
      </div>

      <div className="feature-info-row">
        <span>Tür</span>
        <strong>
          {getFeatureType(selectedFeature) === "Point"
            ? "Nokta"
            : getFeatureType(selectedFeature) === "LineString"
              ? "Çizgi"
              : "Alan"}
        </strong>
      </div>

      <div className="feature-info-row">
        <span>İsim</span>
        <strong>
          {selectedFeature.get("name") || "İsimsiz"}
        </strong>
      </div>

      <div className="feature-info-row">
        <span>Renk</span>

        <div className="feature-info-color">
          <span
            className="feature-info-color-box"
            style={{
              background:
                selectedFeature.get("color") || "#3388ff"
            }}
          />

          <strong>
            {selectedFeature.get("color") || "#3388ff"}
          </strong>
        </div>
      </div>

    </div>

  </div>
)}
        {/* =====================================================
            DETAY POPUP
            ===================================================== */}

        {selectedFeature && popupPosition && (
          <div
            className="feature-popup"
            style={{
              left: `${Math.min(
                popupPosition[0] + 15,
                Math.max(20, window.innerWidth - 340)
              )}px`,
              top: `${Math.max(20, popupPosition[1] - 10)}px`
            }}
          >
            <div className="feature-popup-header">
              <div>
                <strong>Objeyi Düzenle</strong>
                <span>
                  {getFeatureType(selectedFeature) === "Point"
                    ? "Nokta"
                    : getFeatureType(selectedFeature) === "LineString"
                      ? "Çizgi"
                      : "Alan"}
                </span>
              </div>

              <button
                type="button"
                className="feature-popup-close"
                onClick={closeFeaturePopup}
              >
                ×
              </button>
            </div>

            <div className="feature-popup-field">
              <label>İsim</label>
              <input
                type="text"
                value={selectedName}
                onChange={(e) => setSelectedName(e.target.value)}
                placeholder="Çizimin adı"
              />
            </div>

            <div className="feature-popup-field">
              <label>Renk</label>
              <div className="feature-color-row">
                <input
                  type="color"
                  value={selectedColor}
                  onChange={(e) => setSelectedColor(e.target.value)}
                />
                <input
                  type="text"
                  value={selectedColor}
                  onChange={(e) => setSelectedColor(e.target.value)}
                  placeholder="#3388ff"
                />
              </div>
            </div>

            <div className="feature-popup-actions">
              {hasPermission(
                getFeatureType(selectedFeature) === "Point"
                  ? "POINT_UPDATE"
                  : getFeatureType(selectedFeature) === "LineString"
                    ? "LINE_UPDATE"
                    : "POLYGON_UPDATE"
              ) && (
                <>
                  <button
                    type="button"
                    className="geometry-button"
                    onClick={
                      isGeometryEditing
                        ? finishGeometryEditing
                        : startGeometryEditing
                    }
                  >
                    {isGeometryEditing
                      ? "✓ Konum Düzenlemeyi Bitir"
                      : "📍 Konumu Düzenle"}
                  </button>

                  <button
                    type="button"
                    className="update-button"
                    onClick={updateSelectedFeature}
                    disabled={savingFeature}
                  >
                    {savingFeature
                      ? "Kaydediliyor..."
                      : "💾 Güncelle"}
                  </button>
                </>
              )}

              {hasPermission(
                getFeatureType(selectedFeature) === "Point"
                  ? "POINT_DELETE"
                  : getFeatureType(selectedFeature) === "LineString"
                    ? "LINE_DELETE"
                    : "POLYGON_DELETE"
              ) && (
                <button
                  type="button"
                  className="delete-feature-button"
                  onClick={deleteSelectedFeature}
                >
                  🗑️ Sil
                </button>
              )}
            </div>
          </div>
        )}


        {showInventoryList && (
          <div
            className="inventory-modal-overlay"
            onClick={() => setShowInventoryList(false)}
            style={{
              position: "fixed",
              inset: 0,
              zIndex: 2000,
              background: "rgba(15, 23, 42, 0.45)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              padding: "24px"
            }}
          >
            <div
              onClick={(event) => event.stopPropagation()}
              style={{
                width: "min(900px, 94vw)",
                maxHeight: "82vh",
                overflowY: "auto",
                background: "#ffffff",
                borderRadius: "18px",
                padding: "24px",
                boxShadow: "0 20px 60px rgba(0, 0, 0, 0.25)"
              }}
            >
              <div
                style={{
                  display: "flex",
                  justifyContent: "space-between",
                  alignItems: "center",
                  gap: "16px",
                  marginBottom: "20px"
                }}
              >
                <div>
                  <h2
                    style={{
                      margin: 0,
                      fontSize: "22px",
                      color: "#1f2937"
                    }}
                  >
                    📋 Envanterler
                  </h2>

                  <p
                    style={{
                      margin: "6px 0 0",
                      color: "#6b7280",
                      fontSize: "14px"
                    }}
                  >
                    Haritada kayıtlı envanterlerin listesi
                  </p>
                </div>

                <button
                  type="button"
                  onClick={() => setShowInventoryList(false)}
                  style={{
                    border: "none",
                    background: "#f3f4f6",
                    borderRadius: "10px",
                    width: "38px",
                    height: "38px",
                    cursor: "pointer",
                    fontSize: "20px"
                  }}
                  aria-label="Envanter penceresini kapat"
                >
                  ×
                </button>
              </div>

              {inventoryLoading ? (
                <div
                  style={{
                    padding: "40px 20px",
                    textAlign: "center",
                    color: "#6b7280"
                  }}
                >
                  Envanterler yükleniyor...
                </div>
              ) : (
                <>
                  <div
                    style={{
                      display: "grid",
                      gridTemplateColumns:
                        "repeat(auto-fit, minmax(150px, 1fr))",
                      gap: "12px",
                      marginBottom: "22px"
                    }}
                  >
                    {[
                      {
                        label: "Noktalar",
                        count: inventoryData.points.length,
                        icon: "📍"
                      },
                      {
                        label: "Çizgiler",
                        count: inventoryData.lines.length,
                        icon: "📏"
                      },
                      {
                        label: "Alanlar",
                        count: inventoryData.polygons.length,
                        icon: "🔷"
                      }
                    ].map(item => (
                      <div
                        key={item.label}
                        style={{
                          background: "#f8fafc",
                          border: "1px solid #e5e7eb",
                          borderRadius: "12px",
                          padding: "14px"
                        }}
                      >
                        <div
                          style={{
                            fontSize: "13px",
                            color: "#6b7280"
                          }}
                        >
                          {item.icon} {item.label}
                        </div>

                        <strong
                          style={{
                            display: "block",
                            marginTop: "5px",
                            fontSize: "24px",
                            color: "#111827"
                          }}
                        >
                          {item.count}
                        </strong>
                      </div>
                    ))}
                  </div>

                  <div
                    style={{
                      display: "grid",
                      gridTemplateColumns:
                        "repeat(auto-fit, minmax(250px, 1fr))",
                      gap: "16px"
                    }}
                  >
                    {[
                      {
                        title: "📍 Noktalar",
                        items: inventoryData.points
                      },
                      {
                        title: "📏 Çizgiler",
                        items: inventoryData.lines
                      },
                      {
                        title: "🔷 Alanlar",
                        items: inventoryData.polygons
                      }
                    ].map(section => (
                      <div
                        key={section.title}
                        style={{
                          border: "1px solid #e5e7eb",
                          borderRadius: "14px",
                          overflow: "hidden"
                        }}
                      >
                        <div
                          style={{
                            padding: "12px 14px",
                            background: "#f8fafc",
                            fontWeight: 600,
                            color: "#374151"
                          }}
                        >
                          {section.title}
                        </div>

                        {section.items.length === 0 ? (
                          <div
                            style={{
                              padding: "18px 14px",
                              color: "#9ca3af",
                              fontSize: "14px"
                            }}
                          >
                            Kayıt bulunmuyor.
                          </div>
                        ) : (
                          <div>
                            {section.items.map(item => (
                              <div
                                key={item.id}
                                style={{
                                  display: "flex",
                                  alignItems: "center",
                                  gap: "10px",
                                  padding: "12px 14px",
                                  borderTop:
                                    "1px solid #f1f5f9"
                                }}
                              >
                                <span
                                  style={{
                                    width: "12px",
                                    height: "12px",
                                    borderRadius: "50%",
                                    background: item.color,
                                    border:
                                      "1px solid #d1d5db",
                                    flexShrink: 0
                                  }}
                                />

                                <div
                                  style={{
                                    minWidth: 0
                                  }}
                                >
                                  <div
                                    style={{
                                      color: "#1f2937",
                                      fontWeight: 500,
                                      overflow: "hidden",
                                      textOverflow: "ellipsis",
                                      whiteSpace: "nowrap"
                                    }}
                                  >
                                    {item.name}
                                  </div>

                                  <div
                                    style={{
                                      color: "#9ca3af",
                                      fontSize: "12px",
                                      marginTop: "2px"
                                    }}
                                  >
                                    ID: {item.id}
                                  </div>
                                </div>
                              </div>
                            ))}
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                </>
              )}
            </div>
          </div>
        )}

        {measurementResult && (
  <div className="measurement-result">
    <span>📐</span>
    <strong>{measurementResult}</strong>

    <button
      type="button"
      onClick={() => setMeasurementResult(null)}
    >
      ×
    </button>
  </div>
)}


        <div className="map-info">

          <strong>
            Türkiye
          </strong>


          <span>
            OpenLayers Haritası
          </span>

        </div>


      </main>


    </div>

  );
}

export default App;