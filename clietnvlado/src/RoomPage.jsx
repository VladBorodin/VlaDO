// src/RoomPage.jsx

import { useState, useEffect } from "react";
import { FaSun, FaMoon, FaArrowLeft } from "react-icons/fa";
import { Link, useNavigate, useParams } from "react-router-dom";
import api from "./api"; // axios-обёртка

export default function RoomPage() {
  const navigate = useNavigate();
  const { id: roomId } = useParams(); // получаем ID текущей комнаты из URL

  // Тема (light / dark)
  const [theme, setTheme] = useState(
    () => localStorage.getItem("theme") || "light"
  );
  useEffect(() => {
    document.body.className =
      theme === "dark" ? "bg-dark text-light" : "bg-light text-dark";
    localStorage.setItem("theme", theme);
  }, [theme]);

  // Список всех комнат (моки)
  const [allRooms, setAllRooms] = useState([]);
  // Текущее свойство фильтра для списка комнат: "my", "others", "all"
  const [roomFilter, setRoomFilter] = useState("my");

  // Список пользователей текущей комнаты
  const [roomUsers, setRoomUsers] = useState([]);

  // Список документов текущей комнаты
  const [roomDocuments, setRoomDocuments] = useState([]);

  // Загрузка данных (моки пока что)
  useEffect(() => {
    // TODO: заменить на реальный API-вызов:
    // api.get("/rooms").then(res => setAllRooms(res.data));
    // api.get(`/rooms/${roomId}/users`).then(res => setRoomUsers(res.data));
    // api.get(`/rooms/${roomId}/documents`).then(res => setRoomDocuments(res.data));

    // Мок: все комнаты
    const mockRooms = [
      { id: "room1", title: "Финансы", creatorId: "user1" },
      { id: "room2", title: "Разработка", creatorId: "user3" },
      { id: "room3", title: "Маркетинг", creatorId: "user2" },
      { id: "room4", title: "Дизайн", creatorId: "user1" },
    ];
    setAllRooms(mockRooms);

    // Мок: пользователи комнаты
    const mockUsers = [
      { id: "user1", name: "Иван Иванов", accessLevel: "Admin" },
      { id: "user2", name: "Петр Петров", accessLevel: "Write" },
      { id: "user4", name: "Мария Серова", accessLevel: "Read" },
    ];
    setRoomUsers(mockUsers);

    // Мок: документы в комнате
    const mockDocs = [
      {
        id: "doc1",
        name: "Отчет Q1.pdf",
        version: 3,
        createdAt: "2025-05-30T10:15:00Z",
        createdBy: { id: "user1", name: "Иван Иванов" },
        previousVersionId: "doc1_v2",
      },
      {
        id: "doc2",
        name: "ТЗ проекта.docx",
        version: 1,
        createdAt: "2025-05-28T14:42:00Z",
        createdBy: { id: "user2", name: "Петр Петров" },
        previousVersionId: null,
      },
      {
        id: "doc5",
        name: "Бюджет.xlsx",
        version: 2,
        createdAt: "2025-06-01T12:00:00Z",
        createdBy: { id: "user4", name: "Мария Серова" },
        previousVersionId: "doc5_v1",
      },
    ];
    setRoomDocuments(mockDocs);
  }, [roomId]);

  // Текущий пользователь (ID)
  const currentUserId = sessionStorage.getItem("userId") || null;

  // Фильтрация списка комнат по roomFilter
  const filteredRooms = allRooms.filter((room) => {
    if (roomFilter === "my") {
      return room.creatorId === currentUserId;
    }
    if (roomFilter === "others") {
      return room.creatorId !== currentUserId;
    }
    return true; // all
  });

  // CSS для табличек и виджетов в зависимости от темы
  const cardBgClass = theme === "dark" ? "bg-secondary text-light" : "";
  const tableHeadClass = theme === "dark" ? "table-dark text-light" : "table-light text-dark";
  const widgetBgClass = theme === "dark" ? "bg-secondary text-light" : "bg-white";

  return (
    <div className="min-vh-100 d-flex flex-column">
      {/* Шапка */}
      <nav
        className="navbar navbar-expand-lg shadow-sm"
        style={{
          background:
            theme === "dark"
              ? "linear-gradient(90deg, #2c2c2c, #1a1a1a)"
              : "#fff",
        }}
      >
        <div className="container">
          <button
            className="btn btn-link d-flex align-items-center p-0"
            onClick={() => navigate("/")}
          >
            <FaArrowLeft
              size={24}
              className={theme === "dark" ? "text-light" : "text-dark"}
            />
            <span
              className={`ms-2 fw-bold ${
                theme === "dark" ? "text-light" : "text-dark"
              }`}
            >
              Управление комнатой
            </span>
          </button>

          <div className="ms-auto">
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
          </div>
        </div>
      </nav>

      {/* Основной контент: три колонки */}
      <div className="container my-4 flex-fill">
        <div className="row g-4">
          {/* 1) Виджет списка комнат (колонка слева) */}
          <div className="col-md-3">
            <div className={`card ${widgetBgClass} shadow-sm`}>
              <div className={`card-header ${theme === "dark" ? "bg-dark text-light" : ""}`}>
                <h6 className="mb-0">Комнаты</h6>
              </div>
              <div className="card-body">
                <div className="mb-3">
                  <select
                    className="form-select"
                    value={roomFilter}
                    onChange={(e) => setRoomFilter(e.target.value)}
                  >
                    <option value="my">Мои комнаты</option>
                    <option value="others">Другие комнаты</option>
                    <option value="all">Все</option>
                  </select>
                </div>
                <ul className="list-group list-group-flush">
                  {filteredRooms.map((room) => (
                    <li
                      key={room.id}
                      className={`list-group-item ${
                        theme === "dark" ? "bg-secondary text-light" : ""
                      } ${
                        room.id === roomId
                          ? "fw-bold"
                          : "text-muted"
                      }`}
                    >
                      <Link
                        to={`/rooms/${room.id}`}
                        className={theme === "dark" ? "text-light" : "text-dark"}
                      >
                        {room.title}
                      </Link>
                    </li>
                  ))}
                  {filteredRooms.length === 0 && (
                    <li className="list-group-item text-muted">
                      Нет комнат
                    </li>
                  )}
                </ul>
              </div>
            </div>
          </div>

          {/* 2) Виджет пользователей текущей комнаты (колонка справа) */}
          <div className="col-md-3">
            <div className={`card ${widgetBgClass} shadow-sm`}>
              <div className={`card-header ${theme === "dark" ? "bg-dark text-light" : ""}`}>
                <h6 className="mb-0">Пользователи</h6>
              </div>
              <ul className="list-group list-group-flush">
                {roomUsers.map((user) => (
                  <li
                    key={user.id}
                    className={`list-group-item ${
                      theme === "dark" ? "bg-secondary text-light" : ""
                    }`}
                  >
                    <div className="d-flex justify-content-between">
                      <span>{user.name}</span>
                      <span className="badge bg-primary">
                        {user.accessLevel}
                      </span>
                    </div>
                  </li>
                ))}
                {roomUsers.length === 0 && (
                  <li className="list-group-item text-muted">
                    Нет пользователей
                  </li>
                )}
              </ul>
            </div>
          </div>

          {/* 3) Основное окно: документы текущей комнаты (центральная колонка) */}
          <div className="col-md-6">
            <div className={`card ${cardBgClass} shadow-sm`}>
              <div className={`card-header ${theme === "dark" ? "bg-dark text-light" : ""}`}>
                <h6 className="mb-0">Документы комнаты</h6>
              </div>
              <div className="card-body p-0">
                <div className="table-responsive">
                  <table className="table table-hover align-middle mb-0">
                    <thead className={tableHeadClass}>
                      <tr>
                        <th>Название</th>
                        <th>Версия</th>
                        <th>Дата создания</th>
                        <th>Создал</th>
                        <th>Пред. версия</th>
                      </tr>
                    </thead>
                    <tbody>
                      {roomDocuments.length === 0 ? (
                        <tr>
                          <td colSpan="5" className="text-center text-muted py-4">
                            Нет документов
                          </td>
                        </tr>
                      ) : (
                        roomDocuments.map((doc) => (
                          <tr key={doc.id}>
                            <td>{doc.name}</td>
                            <td>{doc.version}</td>
                            <td>
                              {new Date(doc.createdAt).toLocaleString("ru-RU", {
                                day: "2-digit",
                                month: "long",
                                year: "numeric",
                                hour: "2-digit",
                                minute: "2-digit",
                              })}
                            </td>
                            <td>{doc.createdBy?.name || "-"}</td>
                            <td>
                              {doc.previousVersionId ? (
                                <Link
                                  to={`/documents/${doc.previousVersionId}`}
                                  className="link-primary"
                                >
                                  Просмотр
                                </Link>
                              ) : (
                                "—"
                              )}
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Футер */}
      <footer
        className={`py-3 text-center ${
          theme === "dark" ? "bg-dark text-secondary" : "bg-light text-muted"
        }`}
      >
        &copy; {new Date().getFullYear()} VlaDO
      </footer>
    </div>
  );
}