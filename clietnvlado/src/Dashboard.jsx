import { useState, useEffect } from "react";
import {FaMoon,FaSun,FaSignOutAlt,FaBell,FaPlus,FaFolder,FaUserCircle,FaAddressBook,FaHistory} from "react-icons/fa";
import { Link } from "react-router-dom";
import ProfileModal from "./ProfileModal";
import ContactsModal from "./ContactsModal";
import { useNavigate } from 'react-router-dom';
import api from "./api";
import { FaFolderOpen } from "react-icons/fa";
import Notifications from "./Notifications";
import LoadingSpinner from "./LoadingSpinner";

export default function Dashboard({ onLogout }) {
  const [showProfile, setShowProfile] = useState(false);
  const [showContacts, setShowContacts] = useState(false);
  const [user, setUser] = useState(null);
  const [theme, setTheme] = useState(() =>(
    document.body.classList.contains("dark") ? "dark" : "light"
  ));
  const [selectedRoom, setSelectedRoom] = useState(null);
  const [rooms, setRooms] = useState([]);
  const [documents, setDocuments] = useState([]);
  const [activities,setActivities] = useState([]);

  const [showNotifications, setShowNotifications] = useState(false);

  const navigate = useNavigate();

  const [isLoading, setIsLoading] = useState(true);
  const [fadeOut, setFadeOut] = useState(false);

  useEffect(() => {
    const loadMe = async () => {
      try {
        const { data } = await api.get("/users/getMe");
        setUser(data);
      } catch (e) {
        console.error("Не удалось получить профиль", e);
      } finally {
        setFadeOut(true);
        setTimeout(() => setIsLoading(false), 500);
      }
    };
    loadMe();
  }, []);

  useEffect(() => {
    document.body.classList.toggle("dark", theme === "dark");
    document.body.classList.toggle("light", theme !== "dark");
  }, [theme]);

  useEffect(() => {
  if (!user) return;
  const load = async () => {
    try {
      const [{ data: docs },
            { data: acts },
            { data: rms }] = await Promise.all([
        api.get("/documents", { params:{ type:"lastupdate" } }),
        api.get("/activity",  { params:{ top:10 } }),
        api.get("/rooms/last-active", { params:{ top:10 } })
      ]);

      docs.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));

      setDocuments(docs.slice(0, 10));
      setRooms(rms);
      setSelectedRoom(rms[0] ?? null);
    } catch(e){ 
      console.error("Не удалось загрузить дашборд", e); 
    } finally {
      setIsLoading(false); // ⬅️ вот здесь
    }
  };
  load();
}, [user]);

  useEffect(() => {
    api.get("/activity/dashboard?top=10").then(r => {
      if (Array.isArray(r.data)) setActivities(r.data);
    });
  }, []);

  const cardBgClass = theme === "dark" ? "bg-dark bg-gradient text-light" : "bg-white text-dark";

  function formatActivity(activity) {
    const { type, meta, createdAt } = activity;
    const dt = new Date(createdAt).toLocaleString();

    switch (type) {
      // ─── Документы ──────────────────────────────────────────────
      case "CreatedDocument":
          return `📄 Добавлен документ «${meta?.Name ?? "(без названия)"}».`;
      case "UpdatedDocument":
          return `✏️ Обновлён документ «${meta?.Name ?? "(без названия)"}» v${meta?.Version}-${meta?.ForkPath || '0'}.`;
      case "DeletedDocument":
          return `🗑️ Удалён документ «${meta?.Name ?? "(без названия)"}».`;
      case "ArchivedDocument":
          return `📦 Архивирован документ ${meta?.Name} v${meta?.Version}-${meta?.ForkPath || "0"}.`;
      case "RenamedDocument":
          return `🔖 Переименован документ: «${meta?.OldName}» → «${meta?.NewName}».`;

      // ─── Токены ────────────────────────────────────────────────
      case "IssuedToken":
          return `🔑 Выдан доступ к документу «${meta?.Name}».`;
      case "UpdatedToken":
          return `🔐 Обновлён доступ к документу «${meta?.Name}».`;
      case "RevokedToken":
          return `🚫 Отозван доступ к документу «${meta?.Name}».`;

      // ─── Комнаты ───────────────────────────────────────────────
      case "CreatedRoom":
          return `🆕 Создана комната «${meta?.RoomTitle ?? "(без названия)"}».`;
      case "InvitedToRoom":
          return `✉️ Приглашение в комнату «${meta?.RoomTitle ?? "(без названия)"}» от ${meta?.UserName}.`;
      case "AcceptedRoom":
          return `✅ Приглашение для ${meta?.UserName} в комнату принято.`;
      case "DeclinedRoom":
          return `🚫 Приглашение для ${meta?.UserName} в комнату отклонено.`;
      case "RevokedRoom":
          return `🚫 Отозван доступ к комнате «${meta?.RoomTitle}».`;
      case "UpdatedRoomAccess":
          return `🔧 Изменён уровень доступа в комнате «${meta?.RoomTitle ?? "(без названия)"}».`;
      case "DeletedRoom":
          return `❌ Удалена комната «${meta?.RoomTitle ?? "(без названия)"}» с ${meta?.Count ?? "0"} документами.`;

      // ─── Контакты ───────────────────────────────────────────────
      case "InvitedToContacts":
          return `👤 Запрос на добавление в контакты от ${meta?.UserName}.`;
      case "AcceptedContact":
          return `✅ Ваш запрос принял ${meta?.UserName}.`;
      case "DeclinedContact":
          return `🚫 Ваш запрос отклонил ${meta?.UserName}.`;

      default:
          return "ℹ️ Неизвестная активность";
    }
  }

  if (isLoading) {
    return (
      <div className={`fade-screen ${fadeOut ? "fade-out" : ""} ${theme === "dark" ? "bg-dark" : "bg-light"}`}>
        <LoadingSpinner size={400} />
      </div>
    );
  }

  return (
    <>
      {/* Header */}
      <nav
        className="navbar navbar-expand-lg shadow-sm"
        style={{
          background:
            theme === "dark"
              ? "linear-gradient(90deg, #2c2c2c, #1a1a1a)"
              : "#fff"
        }}
      >
        <div className="container">
          <img
            src="/logo-small.png"
            alt="Logo"
            height={38}
            className="me-3"
          />

          <div className="ms-auto d-flex align-items-center gap-3">
            {user
              ? <span className="fw-medium me-2">{`Здравствуйте, ${user.name}`}</span>
              : <span className="text-muted me-2">Загрузка профиля…</span>}

            {/* Аватар + открытие модала */}
            <button
                className="btn btn-link"
                title="Профиль"
                onClick={() => setShowProfile(true)}
                style={{ color: theme === "dark" ? "#ccc" : "#333" }}
            >
                <FaUserCircle size={24} />
            </button>

            {/* иконка-контактов */}
            <button
              className="btn btn-link"
              title="Контакты"
              style={{ color: theme === "dark" ? "#ccc" : "#333" }}
              /* TODO: открыть модал */
              onClick={()=>setShowContacts(true)}
            >
              {/* выберите иконку */}
              <FaAddressBook size={22}/>
            </button>

            <Notifications
              theme={theme}
              isOpen={showNotifications}
              onToggle={() => setShowNotifications(prev => !prev)}
              onActivitiesRead={(ids) =>
                setActivities(prev => prev.map(a =>
                  ids.includes(a.id) ? { ...a, isRead: true } : a
                ))
              }
            />
            <button
              className="btn btn-link"
              title="Переключить тему"
              onClick={() =>
                setTheme(theme === "dark" ? "light" : "dark")
              }
              style={{ color: theme === "dark" ? "#ccc" : "#333" }}
            >
              {theme === "dark" ? <FaSun size={20} /> : <FaMoon size={20} />}
            </button>
            <button
              className={`btn btn-${
                theme === "dark" ? "secondary" : "outline-secondary"
              } btn-sm ms-2`}
              title="Выйти"
              onClick={onLogout}
            >
              <FaSignOutAlt size={18} className="me-1" />
              Выйти
            </button>
          </div>
        </div>
      </nav>

      {/* Main Grid */}
      <div className="container py-4">
        <div className="row g-4">
          {/* Мои комнаты */}
          <div className="col-md-3">
            <div className={`card shadow h-100 ${cardBgClass}`}>
              <div className="card-header bg-transparent d-flex align-items-center justify-content-between">
                <div className="d-flex gap-2">
                  <Link to="/room-manager" className="btn btn-primary btn-sm">
                    <FaFolderOpen className="me-1" /> Мои комнаты
                  </Link>
                  <Link to="/rooms/create" className="btn btn-success btn-sm">
                    <FaPlus className="me-1" /> Комната
                  </Link>
                </div>
              </div>
              <ul className="list-group list-group-flush">
                {rooms.length === 0 && (
                  <li className="list-group-item text-muted text-center">Нет активных комнат</li>
                )}
                {rooms.map(r => (
                  <li key={r.id}
                      className={`list-group-item ${selectedRoom?.id === r.id ? "active" : ""}`}
                      style={{cursor:"pointer"}}
                      onClick={() => setSelectedRoom(r)}>
                      <FaFolder className="me-2" />
                      {r.title}
                  </li>
                ))}
              </ul>
            </div>
          </div>

          {/* Последние активности */}
          <div className="col-md-5">
            <div className={`card shadow h-100 ${cardBgClass}`}>
              <div className="card-header bg-transparent d-flex align-items-center justify-content-between">
                <div className="card-header bg-transparent fw-bold">
                  Последние активности
                </div>
                  {/* Мои активности */}
                  <Link
                    to="/activities"
                    className="btn btn-primary btn-sm"
                    title="История моих действий"
                  >
                    <FaHistory className="me-1" /> Мои&nbsp;активности
                  </Link>
                </div>
              <div className="card-body">
                {activities.length > 0 ? (
                  activities.map(a => (
                    <div key={a.id}>{/* ... */}</div>
                  ))
                ) : (
                  <div className="text-center text-muted">Нет активности</div>
                )}
                <ul className="list-group list-group-flush">
                  {activities.map(a => (
                    <li key={a.id} className="list-group-item">
                      <div className="d-flex justify-content-between">
                        <span>{formatActivity(a)}</span>
                        <span className="text-muted small">
                          {new Date(a.createdAt).toLocaleDateString()}
                        </span>
                      </div>
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          </div>

          {/* Последние документы */}
          <div className="col-md-4">
            <div className={`card shadow h-100 ${cardBgClass}`}>
              <div className="card-header bg-transparent d-flex justify-content-between align-items-center">
                <span className="fw-bold">Последние документы</span>
                <Link to="/documents" className="btn btn-primary btn-sm" title="Менеджер файлов">
                  <FaFolder className="me-1" /> Мои документы
                </Link>
              </div>
              <div className="card-body">
                {documents.length === 0 && (
                  <li className="list-group-item text-muted text-center">
                    Нет документов
                  </li>
                )}
                <ul className="list-group list-group-flush">
                  {documents.map(d => (
                    <li key={d.id} className="list-group-item">
                      <div className="fw-medium">{d.name}</div>
                      <div className="small text-muted">
                        {(d.room?.title || "-")} | Изменён:{" "}
                        {new Date(d.createdAt).toLocaleDateString()}
                      </div>
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Footer */}
      <footer
        className={`text-center py-3 mt-4 ${
          theme === "dark" ? "bg-dark text-secondary" : "bg-light text-muted"
        }`}
      >
        &copy; {new Date().getFullYear()} VlaDO. Все права защищены.
      </footer>

        {/* Профильный модал */}
        {user && showProfile && (
          <ProfileModal
            show={showProfile}
            onClose={() => setShowProfile(false)}
            user={user}
            theme={theme}
            onUpdateUser={u => { setUser(prev => ({ ...prev, ...u })); setShowProfile(false); }}
          />
        )}
        <ContactsModal
          show={showContacts}
          onClose={()=>setShowContacts(false)}
          theme={theme}
        />
    </>
  );
}
