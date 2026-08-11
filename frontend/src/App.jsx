import { useEffect, useRef, useState } from "react";
import Map from "ol/Map";
import View from "ol/View";
import TileLayer from "ol/layer/Tile";
import OSM from "ol/source/OSM";
import { fromLonLat } from "ol/proj";
import "ol/ol.css";
import "./App.css";

function App() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const mapRef = useRef(null);
  const mapElementRef = useRef(null);

  const logout = () => {
    localStorage.removeItem("token");
    setToken(null);
  };

  useEffect(() => {
  if (!token) return;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    const expirationTime = payload.exp * 1000;
    const remainingTime = expirationTime - Date.now();

    if (remainingTime <= 0) {
      logout();
      return;
    }

    const timer = setTimeout(() => {
      logout();
    }, remainingTime);

    return () => clearTimeout(timer);
  } catch {
    logout();
  }
}, [token]);

  const handleLogin = async (e) => {
    e.preventDefault();

    setError("");
    setLoading(true);

    try {
      const response = await fetch("http://localhost:5166/api/Auth/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          username: username,
          password: password,
        }),
      });

      if (!response.ok) {
        throw new Error("Kullanıcı adı veya şifre hatalı.");
      }

      const data = await response.json();

      localStorage.setItem("token", data.token);
      setToken(data.token);

      // Token'ın süresi dolunca otomatik çıkış
      const expiresIn = data.expiresIn || 600;

      setTimeout(() => {
        logout();
        alert("Oturum süreniz doldu. Lütfen tekrar giriş yapın.");
      }, expiresIn * 1000);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!token || !mapElementRef.current) {
      return;
    }

    const map = new Map({
      target: mapElementRef.current,

      layers: [
        new TileLayer({
          source: new OSM(),
        }),
      ],

      view: new View({
        center: fromLonLat([35.0, 39.0]),
        zoom: 5.5,
      }),
    });

    mapRef.current = map;

    return () => {
      map.setTarget(null);
      mapRef.current = null;
    };
  }, [token]);

  if (!token) {
    return (
      <div className="login-page">
        <div className="login-card">
          <div className="logo">🗺️</div>

          <h1>StajProjesi</h1>
          <p className="subtitle">Harita Sistemine Hoş Geldiniz</p>

          <form onSubmit={handleLogin}>
            <div className="input-group">
              <label>Kullanıcı Adı</label>
              <input
                type="text"
                placeholder="Kullanıcı adınızı girin"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
              />
            </div>

            <div className="input-group">
              <label>Şifre</label>
              <input
                type="password"
                placeholder="Şifrenizi girin"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>

            {error && <div className="error-message">{error}</div>}

            <button type="submit" className="login-button" disabled={loading}>
              {loading ? "Giriş yapılıyor..." : "Giriş Yap"}
            </button>
          </form>

          <div className="login-info">
            JWT ile güvenli giriş
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="map-page">
      <header className="map-header">
        <div>
          <h1>🗺️ StajProjesi</h1>
          <span>Türkiye Haritası</span>
        </div>

        <button onClick={logout} className="logout-button">
          Çıkış Yap
        </button>
      </header>

      <main className="map-container">
        <div ref={mapElementRef} className="map"></div>

        <div className="map-info">
          <strong>Türkiye</strong>
          <span>OpenLayers Haritası</span>
        </div>
      </main>
    </div>
  );
}

export default App;