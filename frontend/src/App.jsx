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

import OSM from "ol/source/OSM";
import VectorSource from "ol/source/Vector";

import Feature from "ol/Feature";

import Point from "ol/geom/Point";
import LineString from "ol/geom/LineString";
import Polygon from "ol/geom/Polygon";

import Draw from "ol/interaction/Draw";

import { fromLonLat, toLonLat } from "ol/proj";

import "ol/ol.css";
import "./App.css";


function App() {

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
  // REFS
  // =========================

  const mapRef = useRef(null);
  const mapElementRef = useRef(null);

  const vectorSourceRef = useRef(null);
  const drawRef = useRef(null);


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

        ],

        view: new View({

          center:
            fromLonLat([35, 39]),

          zoom: 5.5,

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
              "http://localhost:5166/api/PointFeatures"
            );


          if (!response.ok) {
            return;
          }


          const points =
            await response.json();


          points.forEach(point => {

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
              "http://localhost:5166/api/LineFeatures"
            );


          if (!response.ok) {
            return;
          }


          const lines =
            await response.json();


          lines.forEach(line => {

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
              "http://localhost:5166/api/PolygonFeatures"
            );


          if (!response.ok) {
            return;
          }


          const polygons =
            await response.json();


          polygons.forEach(polygon => {

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


    // =========================
    // TEMİZLEME
    // =========================

    return () => {

      map.setTarget(null);

      mapRef.current = null;

      vectorSourceRef.current = null;

    };

  }, [token]);


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


          // =========================
          // POINT
          // =========================

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

              await fetch(
                "http://localhost:5166/api/PointFeatures",
                {

                  method:
                    "POST",

                  headers: {

                    "Content-Type":
                      "application/json"

                  },

                  body:
                    JSON.stringify({

                      longitude:
                        longitude,

                      latitude:
                        latitude

                    })

                }
              );


              console.log(
                "Point kaydedildi."
              );

            }
            catch (error) {

              console.error(
                "Point kaydetme hatası:",
                error
              );

            }

          }


          // =========================
          // LINE
          // =========================

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
                  "http://localhost:5166/api/LineFeatures",
                  {

                    method:
                      "POST",

                    headers: {

                      "Content-Type":
                        "application/json"

                    },

                    body:
                      JSON.stringify({

                        coordinates:
                          convertedCoordinates

                      })

                  }
                );


              if (!response.ok) {

                console.error(
                  "Line kaydedilemedi."
                );

                return;

              }


              console.log(
                "Line kaydedildi."
              );

            }
            catch (error) {

              console.error(
                "Line kaydetme hatası:",
                error
              );

            }

          }


          // =========================
          // POLYGON
          // =========================

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
                  "http://localhost:5166/api/PolygonFeatures",
                  {

                    method:
                      "POST",

                    headers: {

                      "Content-Type":
                        "application/json"

                    },

                    body:
                      JSON.stringify({

                        coordinates:
                          convertedCoordinates

                      })

                  }
                );


              if (!response.ok) {

                console.error(
                  "Polygon kaydedilemedi."
                );

                return;

              }


              console.log(
                "Polygon kaydedildi."
              );

            }
            catch (error) {

              console.error(
                "Polygon kaydetme hatası:",
                error
              );

            }

          }


          // Çizim bittikten sonra
          // interaction'ı kaldır

          if (mapRef.current) {

            mapRef.current.removeInteraction(
              draw
            );

          }


          drawRef.current =
            null;

        }
      );

    };


  // =====================================================
  // LOGIN SAYFASI
  // =====================================================

  if (!token) {

    return (

      <div className="login-page">

        <Card className="login-card">

          <div className="login-logo">

            <i className="pi pi-map"></i>

          </div>


          <h1>
            StajProjesi
          </h1>


          <p className="subtitle">
            Harita Sistemine Hoş Geldiniz
          </p>


          <form
            onSubmit={handleLogin}
          >

            {/* KULLANICI ADI */}

            <div className="input-group">

              <label htmlFor="username">
                Kullanıcı Adı
              </label>


              <span className="p-input-icon-left login-input-wrapper">

                <i className="pi pi-user"></i>


                <InputText
                  id="username"
                  type="text"
                  placeholder="Kullanıcı adınızı girin"
                  value={username}
                  onChange={(e) =>
                    setUsername(
                      e.target.value
                    )
                  }
                  required
                />

              </span>

            </div>


            {/* ŞİFRE */}

            <div className="input-group">

              <label htmlFor="password">
                Şifre
              </label>


              <span className="p-input-icon-left login-input-wrapper">

                <i className="pi pi-lock"></i>


                <Password
                  id="password"
                  placeholder="Şifrenizi girin"
                  value={password}
                  onChange={(e) =>
                    setPassword(
                      e.target.value
                    )
                  }
                  toggleMask
                  feedback={false}
                  required
                />

              </span>

            </div>


            {/* HATA */}

            {error && (

              <div className="error-message">

                <i className="pi pi-exclamation-circle"></i>

                <span>
                  {error}
                </span>

              </div>

            )}


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

          </form>


          <div className="login-info">

            <i className="pi pi-shield"></i>

            JWT ile güvenli giriş

          </div>

        </Card>

      </div>

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


          <button
            onClick={() =>
              startDrawing("Point")
            }
          >
            📍 Nokta
          </button>


          <button
            onClick={() =>
              startDrawing("LineString")
            }
          >
            📏 Çizgi
          </button>


          <button
            onClick={() =>
              startDrawing("Polygon")
            }
          >
            🔷 Alan
          </button>


          <button
            onClick={
              clearAllFeatures
            }
            className="clear-button"
          >
            🗑️ Tümünü Temizle
          </button>


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