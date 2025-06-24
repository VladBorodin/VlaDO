import { useEffect, useState, useRef } from "react";
import { FaPlus, FaMoon, FaSun, FaArrowLeft } from "react-icons/fa";
import { useNavigate, Link, useParams, useLocation } from "react-router-dom";
import { useAlert } from "./contexts/AlertContext";
import api from "./api";
import LoadingSpinner from "./LoadingSpinner";

export default function UpdateDocumentPage() {
  const { id: routeId }  = useParams();
  const { state } = useLocation();
  const parentDoc = state?.parentDoc;
  const parentRoom    = parentDoc?.room
  const parentRoomId  = parentRoom?.id ?? "";
  const parentFixedId = parentDoc?.id || routeId || "";
  const [rooms, setRooms] = useState([]);
  const [docs, setDocs] = useState(parentDoc ? [parentDoc] : []);
  const [roomId, setRoomId] = useState("");
  const [parentId, setParentId] = useState(parentFixedId);
  const [parentInfo, setParentInfo] = useState(null);
  const [alerts, setAlerts] = useState([]);
  const [note, setNote] = useState("");
  const [name, setName] = useState("");
  const [file, setFile] = useState(null);
  const [loading, setLoading] = useState(false);
  const fileInputRef = useRef();
  const navigate = useNavigate();
  const [darkMode, setDarkMode] = useState(() => 
    localStorage.getItem("theme") === "dark" ||
    (window.matchMedia("(prefers-color-scheme: dark)").matches &&
    !localStorage.getItem("theme"))
  );
  const [dragActive, setDragActive] = useState(false);
  const { push } = useAlert();

  const [isLoading, setIsLoading] = useState(true);
  const [fadeOut, setFadeOut] = useState(false);

  useEffect(() => {
    alerts.forEach(m => push(m, "warning"));
  }, [alerts]);

  useEffect(() => {
    api.get("/rooms/my").then(r => {
      if (Array.isArray(r.data)) setRooms(r.data);
      setFadeOut(true);
      setTimeout(() => setIsLoading(false), 400);
    }).catch(err => {
      console.error("Ошибка при загрузке комнат:", err.response?.status, err.response?.data);
    });
    if (parentDoc) {
      setParentInfo(prev => ({
        name   : parentDoc.name,
        ext    : parentDoc.name.split(".").pop().toLowerCase(),
        version: parentDoc.version,
        size   : prev?.size,
        forkPath: parentDoc.forkPath
      }));

      api.get(`/documents/${parentFixedId}/meta`)
        .then(r => setParentInfo(info => ({
          ...info,
          size: r.data.size,
        })))
        .catch(console.error);
    } else if (parentFixedId) {
      api.get(`/documents/${parentFixedId}/meta`)
        .then(r => setParentInfo({
          name   : r.data.name,
          size   : r.data.size,
          ext    : r.data.extension,
          version: r.data.version,
          forkPath: r.data.forkPath
        }))
        .catch(console.error);
    }

    
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

  useEffect(() => {
    if (!file || !parentInfo) { setAlerts([]); return; }
    
    const newExt  = file.name.split(".").pop().toLowerCase();
    const sizePct = Math.abs(file.size - parentInfo.size) / parentInfo.size * 100;
    
    const a = [];
    if (file.name !== parentInfo.name)
      a.push(`Название отличается: «${parentInfo.name}» → «${file.name}»`);
    if (newExt !== parentInfo.ext)
      a.push(`Формат изменился: .${parentInfo.ext} → .${newExt}`);
    if (sizePct > 20)
      a.push(`Размер отличается более чем на 20 %`);
   
    setAlerts(a);
  }, [file, parentInfo]);

  const handleUpload = async () => {
    if (!file) return;
    setLoading(true);

    try {
      const form = new FormData();
      form.append("file", file);
      if (note) form.append("note", note);

      let endpoint;

      if (parentId) {
        endpoint = parentDoc?.room
          ? `/rooms/${parentDoc.room.id}/docs/${parentId}/version`
          : `/documents/${parentId}/version`;
      } else if (roomId) {
        endpoint = `/rooms/${roomId}/docs`;
        form.append("name", name);
      } else {
        endpoint = `/documents`;
        form.append("name", name);
      }
      await api.post(endpoint, form);
      push("Документ обновлён", "success");
      navigate("/documents");
    } catch (err) {
      console.error(err);
      push("Ошибка при обновлении документа", "danger");
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
        <div className="card-header fw-bold">Обновление документа</div>
          
        <div className="card-body">
          {alerts.length > 0 && (
            <div className="alert alert-warning">
              <ul className="mb-0 ps-3">
                {alerts.map((m, i) => <li key={i}>{m}</li>)}
              </ul>
            </div>
          )}
          {/* Комната */}
          <div className="mb-3">
            <label className="form-label">Комната</label>
            <input
              className="form-control"
              value={parentRoom?.title ?? "— Без комнаты —"}
              disabled
            />
          </div>

          {/* Родительский документ */}
          <div className="mb-3">
            <label className="form-label">Родительский документ</label>
            <select
              className="form-select"
              value={parentId}
              disabled
            >
              {parentInfo && (
                <option value={parentId}>
                  {parentInfo.name} (v{parentInfo.version}{parentInfo.forkPath && parentInfo.forkPath !== "0" ? `-${parentInfo.forkPath}` : ""})
                </option>
              )}
            </select>
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