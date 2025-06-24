import { useEffect, useState, useRef } from "react";
import { FaPlus, FaMoon, FaSun, FaArrowLeft } from "react-icons/fa";
import { useNavigate, Link } from "react-router-dom";
import api from "./api";
import LoadingSpinner from "./LoadingSpinner";

export default function CreateDocumentPage() {
  const [rooms, setRooms] = useState([]);
  const [docs, setDocs] = useState([]);
  const [roomId, setRoomId] = useState("");
  const [parentId, setParentId] = useState("");
  const [note, setNote] = useState("");
  const [name, setName] = useState("");
  const [file, setFile] = useState(null);
  const [loading, setLoading] = useState(false);
  const fileInputRef = useRef();
  const navigate = useNavigate();
  const [prevHash, setPrevHash] = useState("");
  const [darkMode, setDarkMode] = useState(() => 
    localStorage.getItem("theme") === "dark" ||
    (window.matchMedia("(prefers-color-scheme: dark)").matches &&
    !localStorage.getItem("theme"))
  );
  const [dragActive, setDragActive] = useState(false);

  const [isLoading, setIsLoading] = useState(true);
  const [fadeOut, setFadeOut] = useState(false);

  useEffect(() => {
    api.get("/rooms/my").then(r => {
    console.log("Rooms response:", r);
    if (Array.isArray(r.data)) setRooms(r.data);
    setFadeOut(true);
    setTimeout(() => setIsLoading(false), 400);
  }).catch(err => {
    console.error("Ошибка при загрузке комнат:", err.response?.status, err.response?.data);
  });
    api.get("/documents?type=own").then(r => {
      if (Array.isArray(r.data)) setDocs(r.data);
    });
    
    if (darkMode) {
      document.body.classList.add("dark");
      document.body.classList.remove("light");
      localStorage.setItem("theme", "dark");
    } else {
      document.body.classList.add("light");
      document.body.classList.remove("dark");
      localStorage.setItem("theme", "light");
    }
  }, [darkMode]);

  const handleFileDrop = e => {
    e.preventDefault();
    const f = e.dataTransfer.files[0];
    if (f) {
      setFile(f);
      setName(f.name);
    }
  };

  const handleUpload = async () => {
    if (!file) return;
    setLoading(true);

    try {
      const arrayBuffer = await file.arrayBuffer();
      const uint8Array = new Uint8Array(arrayBuffer);

      const hashBuffer = await crypto.subtle.digest("SHA-256", uint8Array);
      const hashArray = Array.from(new Uint8Array(hashBuffer));
      const hexHash = hashArray.map(b => b.toString(16).padStart(2, "0")).join("");
      setPrevHash(hexHash);
      const form = new FormData();
      form.append("file", file);
      form.append("name", name); // обязательно
      if (note) form.append("note", note);
      if (parentId) form.append("parentDocId", parentId);
      if (prevHash) form.append("prevHash", prevHash);

      let endpoint = "/documents"; // ✅ без /api
      if (roomId) {
        form.append("roomId", roomId);
        endpoint = `/rooms/${roomId}/docs/create`;
      }

      await api.post(endpoint, form);
      alert("Документ успешно загружен");
      navigate("/documents");
    } catch (err) {
      console.error(err);
      alert("Ошибка при загрузке документа");
    } finally {
      setLoading(false);
    }
  };

  if (isLoading) {
    return (
      <div className={`fade-screen ${fadeOut ? "fade-out" : ""} ${darkMode ? "bg-dark" : "bg-light"}`}>
        <LoadingSpinner size={200} />
      </div>
    );
  }
  return (
    <div className="container mt-4">
      <div className="card shadow-sm">
        <div className="d-flex justify-content-between align-items-center mb-3">
            <button className="btn btn-outline-secondary" onClick={() => navigate("/")}>
              <FaArrowLeft className="me-1" /> Назад
            </button>
            <button
              className="btn theme-toggle-btn"
              onClick={() => setDarkMode(!darkMode)}
            >
              {darkMode ? <FaSun /> : <FaMoon />}
            </button>
          </div>
        <div className="card-header fw-bold">Создание документа</div>
          
        <div className="card-body">
          {/* Комната */}
          <div className="mb-3">
            <label className="form-label">Комната</label>
            <div className="d-flex align-items-center gap-2">
              <select
                className="form-select"
                value={roomId}
                onChange={e => setRoomId(e.target.value)}
              >
                <option value="">— Без комнаты —</option>
                {rooms.map(room => (
                  <option key={room.id} value={room.id}>
                    {room.title}
                  </option>
                ))}
              </select>
              <Link to="/rooms/create" className="btn btn-success btn-sm">
                <FaPlus className="me-1" />
              </Link>
            </div>
          </div>

          {/* Название */}
          <div className="mb-3">
            <label className="form-label">Название документа</label>
            <input
              type="text"
              className="form-control"
              value={name}
              onChange={e => setName(e.target.value)}
              placeholder="Введите название"
            />
          </div>

          {/* Примечание */}
          <div className="mb-3">
            <label className="form-label">Примечание</label>
            <textarea
              className="form-control"
              rows="3"
              value={note}
              onChange={e => setNote(e.target.value)}
              placeholder="Краткое описание или комментарий"
            />
          </div>

          {/* Загрузка файла */}
          <div
            className={`mb-3 p-4 border rounded text-center drop-zone ${dragActive ? "active" : ""}`}
            onClick={() => fileInputRef.current?.click()}
            onDragOver={e => {
              e.preventDefault();
              setDragActive(true);
            }}
            onDragLeave={e => {
              e.preventDefault();
              setDragActive(false);
            }}
            onDrop={e => {
              e.preventDefault();
              setDragActive(false);
              const f = e.dataTransfer.files[0];
              if (f) {
                setFile(f);
                setName(f.name);
              }
            }}
            style={{ cursor: "pointer" }}
          >
            {file ? (
              <div className="text-success fw-bold">{file.name}</div>
            ) : (
              <div className="drop-zone">Кликните или перетащите файл сюда</div>
            )}
            <input
              ref={fileInputRef}
              type="file"
              onChange={e => {
                const f = e.target.files[0];
                if (f) {
                  setFile(f);
                  setName(f.name);
                }
              }}
              hidden
            />
          </div>

          {/* Кнопка */}
          <button
            className="btn btn-success w-100"
            disabled={!file || loading}
            onClick={handleUpload}
          >
            {loading ? "Загрузка..." : "Загрузить"}
          </button>
        </div>
      </div>
    </div>
  );
}