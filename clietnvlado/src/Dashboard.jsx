import { useState, useEffect } from "react";
import {
  FaMoon,
  FaSun,
  FaSignOutAlt,
  FaBell,
  FaPlus,
  FaFolder,
  FaUserCircle,
  FaAddressBook
} from "react-icons/fa";
import { Link } from "react-router-dom";
import ProfileModal from "./ProfileModal";
import ContactsModal from "./ContactsModal";
import { useNavigate } from 'react-router-dom';
import api from "./api";

const MOCK_ROOMS = [
  { id: "r1", title: "Проект 2025" },
  { id: "r2", title: "Личное" },
  { id: "r3", title: "Общий архив" }
];
const MOCK_DOCS = [
  { id: "d1", name: "Договор.pdf", room: MOCK_ROOMS[0], updatedAt: "2025-05-30" },
  { id: "d2", name: "Заявка.xlsx", room: MOCK_ROOMS[1], updatedAt: "2025-05-28" }
];
const MOCK_ACTIVITY = [
  { id: 1, date: "2025-05-30", description: "Добавлен документ 'Договор.pdf'" },
  { id: 2, date: "2025-05-29", description: "Создана новая комната 'Проект 2025'" },
  { id: 3, date: "2025-05-29", description: "Вас пригласили в комнату 'Общий архив'" }
];

export default function Dashboard({ onLogout }) {
    const [showProfile, setShowProfile] = useState(false);
    const [showContacts, setShowContacts] = useState(false);
    const [user, setUser] = useState(null);
    const [theme, setTheme] = useState(() =>
        localStorage.getItem("theme") || "light"
    );
    const [selectedRoom, setSelectedRoom] = useState(MOCK_ROOMS[0]);
    const [rooms] = useState(MOCK_ROOMS);
    const [documents] = useState(MOCK_DOCS);
    const [activities] = useState(MOCK_ACTIVITY);

    const navigate = useNavigate();

    useEffect(() => {
      const loadMe = async () => {
        try {
          const { data } = await api.get("/users/me");
          setUser(data);
        } catch (e) {
          console.error("Не удалось получить профиль", e);
        }
      };
      loadMe();
    }, []);

  useEffect(() => {
    document.body.className =
      theme === "dark" ? "bg-dark text-light" : "bg-light text-dark";
    localStorage.setItem("theme", theme);
  }, [theme]);

  // Helper for card background
  const cardBgClass =
    theme === "dark"
      ? "bg-dark bg-gradient text-light"
      : "bg-white text-dark";

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
            {user && (
              <span className="fw-medium me-2">
                {`Здравствуйте, ${user.name}`}
              </span>
            )}

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

            <button
              className="btn btn-link"
              title="Уведомления"
              style={{ color: theme === "dark" ? "#ccc" : "#333" }}
            >
              <FaBell size={20} />
            </button>
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
                <span className="fw-bold">Мои комнаты</span>
                  <Link to="/rooms/create" className="btn btn-success btn-sm">
                      <FaPlus className="me-1" /> Комната
                  </Link>
              </div>
              <ul className="list-group list-group-flush">
                {rooms.length === 0 && (
                  <li className="list-group-item text-muted text-center">
                    Нет комнат
                  </li>
                )}
                {rooms.map((room) => (
                  <li
                    key={room.id}
                    className={`list-group-item ${
                      selectedRoom?.id === room.id
                        ? "active"
                        : theme === "dark"
                        ? "dark-list-item"
                        : ""
                    }`}
                    style={{ cursor: "pointer" }}
                    onClick={() => setSelectedRoom(room)}
                  >
                    <FaFolder className="me-2" />
                    {room.title}
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
                  <Link to="/documents/create" className="btn btn-success btn-sm">
                      <FaPlus className="me-1" /> Документ
                  </Link>
              </div>
              <div className="card-body">
                {activities.length === 0 && (
                  <div className="text-muted text-center py-3">
                    Нет активности
                  </div>
                )}
                <ul className="list-group list-group-flush">
                  {activities.map((act) => (
                    <li key={act.id} className="list-group-item">
                      <div className="d-flex justify-content-between">
                        <span>{act.description}</span>
                        <span className="text-muted small">{act.date}</span>
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
                  <FaFolder className="me-1" /> Менеджер
                </Link>
              </div>
              <div className="card-body">
                {documents.length === 0 && (
                  <div className="text-muted text-center py-3">
                    Нет документов
                  </div>
                )}
                <ul className="list-group list-group-flush">
                  {documents.map((doc) => (
                    <li key={doc.id} className="list-group-item">
                      <div className="fw-medium">{doc.name}</div>
                      <div className="small text-muted">
                        {doc.room?.title} | Изменен: {doc.updatedAt}
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
