import { useState, useEffect } from "react";
import { FaMoon, FaSun, FaSignOutAlt, FaBell, FaPlus, FaFolder } from "react-icons/fa";

const MOCK_USER = { username: "Владислав" };
const MOCK_ROOMS = [
  { id: "r1", title: "Проект 2025" },
  { id: "r2", title: "Личное" },
  { id: "r3", title: "Общий архив" },
];
const MOCK_DOCS = [
  { id: "d1", name: "Договор.pdf", room: MOCK_ROOMS[0], updatedAt: "2025-05-30" },
  { id: "d2", name: "Заявка.xlsx", room: MOCK_ROOMS[1], updatedAt: "2025-05-28" },
];
const MOCK_ACTIVITY = [
  { id: 1, date: "2025-05-30", description: "Добавлен документ 'Договор.pdf'" },
  { id: 2, date: "2025-05-29", description: "Создана новая комната 'Проект 2025'" },
  { id: 3, date: "2025-05-29", description: "Вас пригласили в комнату 'Общий архив'" },
];

export default function Dashboard() {
  const [theme, setTheme] = useState(() =>
    localStorage.getItem("theme") || "light"
  );
  const [selectedRoom, setSelectedRoom] = useState(MOCK_ROOMS[0]);
  const [rooms, setRooms] = useState(MOCK_ROOMS);
  const [documents, setDocuments] = useState(MOCK_DOCS);
  const [activities, setActivities] = useState(MOCK_ACTIVITY);

  // Тема
  useEffect(() => {
    document.body.className = theme === "dark" ? "bg-dark text-light" : "bg-light text-dark";
    localStorage.setItem("theme", theme);
  }, [theme]);

  return (
    <div className="min-vh-100">
      {/* Header */}
      <nav className="navbar navbar-expand-lg navbar-light bg-white shadow-sm mb-4">
        <div className="container">
          <img src="/logo-small.png" alt="Logo" height={38} className="me-3" />
          <span className="navbar-brand fw-bold">VlaDO</span>
          <div className="ms-auto d-flex align-items-center gap-3">
            <span className="fw-medium me-2">Здравствуйте, {MOCK_USER.username}</span>
            <button className="btn btn-link" title="Уведомления">
              <FaBell size={20} />
            </button>
            <button className="btn btn-link" title="Переключить тему" onClick={() => setTheme(theme === "dark" ? "light" : "dark")}>
              {theme === "dark" ? <FaSun size={20} /> : <FaMoon size={20} />}
            </button>
            <button className="btn btn-outline-secondary btn-sm ms-2" title="Выйти">
              <FaSignOutAlt size={18} className="me-1" />
              Выйти
            </button>
          </div>
        </div>
      </nav>

      {/* Main Grid */}
      <div className="container">
        <div className="row g-4">
          {/* Мои комнаты */}
          <div className="col-md-3">
            <div className="card shadow h-100">
              <div className="card-header bg-transparent d-flex align-items-center justify-content-between">
                <span className="fw-bold">Мои комнаты</span>
                <button className="btn btn-outline-primary btn-sm" title="Создать комнату">
                  <FaPlus className="me-1" /> Комната
                </button>
              </div>
              <ul className="list-group list-group-flush">
                {rooms.length === 0 && (
                  <li className="list-group-item text-muted text-center">Нет комнат</li>
                )}
                {rooms.map(room => (
                  <li
                    key={room.id}
                    className={
                      "list-group-item cursor-pointer" +
                      (selectedRoom && selectedRoom.id === room.id
                        ? " bg-primary text-white"
                        : " hover:bg-light")
                    }
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
            <div className="card shadow h-100">
              <div className="card-header bg-transparent fw-bold">Последние активности</div>
              <div className="card-body">
                {activities.length === 0 && (
                  <div className="text-muted text-center py-3">Нет активности</div>
                )}
                <ul className="list-group list-group-flush">
                  {activities.map(act => (
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
            <div className="card shadow h-100">
              <div className="card-header bg-transparent d-flex justify-content-between align-items-center">
                <span className="fw-bold">Последние документы</span>
                <button className="btn btn-outline-primary btn-sm" title="Менеджер файлов">
                  <FaFolder className="me-1" /> Менеджер
                </button>
              </div>
              <div className="card-body">
                {documents.length === 0 && (
                  <div className="text-muted text-center py-3">Нет документов</div>
                )}
                <ul className="list-group list-group-flush">
                  {documents.map(doc => (
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
    </div>
  );
}
