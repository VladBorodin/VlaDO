// src/DocumentsPage.jsx

import { useState, useEffect, useRef } from "react";
import { FaSun, FaMoon, FaFolderOpen, FaArrowLeft, FaTimes } from "react-icons/fa";
import { Link, useNavigate } from "react-router-dom";
import api from "./api"; // ваша axios-обёртка

export default function DocumentsPage() {
  const navigate = useNavigate();

  // Попытка взять userId из sessionStorage (если вы туда сохраняли при логине)
  const storedUserId = sessionStorage.getItem("userId");
  const currentUserId = storedUserId ? storedUserId : null;
  const [menuPosition, setMenuPosition] = useState({ x: 0, y: 0 });

  // Тема (light / dark)
  const [theme, setTheme] = useState(
    () => localStorage.getItem("theme") || "light"
  );
  useEffect(() => {
      document.body.className =
        theme === "dark" ? "bg-dark text-light" : "bg-light text-dark";
      localStorage.setItem("theme", theme);
    }, [theme]);

    // Вкладки
    const tabs = [
      { key: "lastupdate", label: "Все актуальные" },
      { key: "userDoc", label: "Свои документы" },
      { key: "otherDoc", label: "Чужие документы" },
      { key: "all", label: "Все" }
    ];
    const [activeTab, setActiveTab] = useState("lastupdate");

    // Список документов (моковые данные)
    const [documents, setDocuments] = useState([]);
    useEffect(() => {
      let endpoint = "/documents";

      if (activeTab === "userDoc") {
        endpoint = "/documents?type=userDoc";
      } else if (activeTab === "otherDoc") {
        endpoint = "/documents?type=otherDoc";
      } else if (activeTab === "lastupdate") {
        endpoint = "/documents?type=lastupdate";
      } else if (activeTab === "all") {
        endpoint = "/documents?type=all";
      }

    api.get(endpoint)
      .then((res) => {
        if (Array.isArray(res.data)) {
          setDocuments(res.data);
        }
      })
      .catch((err) => {
        console.error("Ошибка при загрузке документов:", err);
      });
  }, [activeTab]);

  const tabClass = (key) =>
    `nav-link ${activeTab === key ? "active" : ""}`;

  // ==== Модальное контекстное меню ====
  const [modalVisible, setModalVisible] = useState(false);
  const [selectedDoc, setSelectedDoc] = useState(null);
  const modalRef = useRef(null);

  // Вызывается при правом клике на строку
  const onRowContextMenu = (e, doc) => {
    e.preventDefault();
    setSelectedDoc(doc);
    setMenuPosition({ x: e.pageX, y: e.pageY });
    setModalVisible(true);
  };

  // Закрытие модального при клике вне содержимого
  useEffect(() => {
    const handleClickOutside = (e) => {
      if (modalVisible && modalRef.current && !modalRef.current.contains(e.target)) {
        setModalVisible(false);
      }
    };
    if (modalVisible) {
      document.addEventListener("mousedown", handleClickOutside);
    } else {
      document.removeEventListener("mousedown", handleClickOutside);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [modalVisible]);

  // Обработчики пунктов меню (все пока заглушки)
  const handleEdit = () => {
    console.log("Редактировать", selectedDoc);
    setModalVisible(false);
  };
  const handleDelete = () => {
    console.log("Удалить", selectedDoc);
    setModalVisible(false);
  };
  const handleArchive = () => {
    console.log("Архивировать", selectedDoc);
    setModalVisible(false);
  };
  const handleCopy = () => {
    console.log("Копировать", selectedDoc);
    setModalVisible(false);
  };
  const handleRename = () => {
    console.log("Переименовать", selectedDoc);
    setModalVisible(false);
  };
  const handleChangeAccess = () => {
    console.log("Изменить доступ", selectedDoc);
    setModalVisible(false);
  };
  const handleToggleRoom = () => {
    if (selectedDoc.room) {
      console.log("Удалить из комнаты", selectedDoc);
    } else {
      console.log("Добавить в комнату", selectedDoc);
    }
    setModalVisible(false);
  };

  return (
    <div className="min-vh-100 d-flex flex-column">
      {/* Header */}
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
            <FaFolderOpen
              size={28}
              className={theme === "dark" ? "text-warning" : "text-primary"}
            />
            <span
              className={`ms-2 fw-bold ${
                theme === "dark" ? "text-light" : "text-dark"
              }`}
            >
              Менеджер документов
            </span>
          </button>

          <div className="ms-auto d-flex align-items-center gap-3">
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
              className="btn btn-outline-secondary btn-sm"
              title="Назад"
              onClick={() => navigate("/")}
            >
              <FaArrowLeft className="me-1" />
              Выйти
            </button>
          </div>
        </div>
      </nav>

      {/* Контент */}
      <div className="container my-4 flex-fill">
        {/* Вкладки */}
        <ul className="nav nav-tabs mb-3">
          {tabs.map((t) => (
            <li className="nav-item" key={t.key}>
              <button
                className={tabClass(t.key)}
                onClick={() => setActiveTab(t.key)}
                style={{ cursor: "pointer" }}
                type="button"
              >
                {t.label}
              </button>
            </li>
          ))}
        </ul>

        {/* Таблица документов */}
        <div className="table-responsive">
          <table className="table table-hover align-middle">
            <thead
              className={
                theme === "dark"
                  ? "table-dark text-light"
                  : "table-light text-dark"
              }
            >
              <tr>
                <th>Название</th>
                <th>Версия</th>
                <th>Дата создания</th>
                <th>Создал</th>
                <th>Пред. версия</th>
                <th>Комната</th>
              </tr>
            </thead>
            <tbody>
              {documents.length === 0 ? (
                <tr>
                  <td colSpan="6" className="text-center text-muted py-4">
                    Документы не найдены
                  </td>
                </tr>
              ) : (
                documents.map((doc) => (
                  <tr
                    key={doc.id}
                    onContextMenu={(e) => onRowContextMenu(e, doc)}
                    style={{ cursor: "context-menu" }}
                  >
                    <td>{doc.name}</td>
                    <td>{doc.version}</td>
                    <td>{new Date(doc.createdAt).toLocaleString("ru-RU", {
                      day: "2-digit",
                      month: "long",
                      year: "numeric",
                      hour: "2-digit",
                      minute: "2-digit",
                    })}</td>
                    <td>{doc.createdBy?.name || "-"}</td>
                    <td>
                      {doc.previousVersionId ? (
                        <Link to={`/documents/${doc.previousVersionId}`} className="link-primary">
                          Просмотр
                        </Link>
                      ) : (
                        "-"
                      )}
                    </td>
                    <td>
                      {doc.room ? (
                        <Link to={`/rooms/${doc.room.id}`} className="link-secondary">
                          {doc.room.title}
                        </Link>
                      ) : (
                        <span className="text-muted">—</span>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
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

      {modalVisible && selectedDoc && (
        <div
          ref={modalRef}
          style={{
            position: "absolute",
            top: `${menuPosition.y}px`,
            left: `${menuPosition.x}px`,
            zIndex: 1000,
            backgroundColor: theme === "dark" ? "#333" : "#fff",
            border: "1px solid #ccc",
            borderRadius: "6px",
            padding: "0.5rem",
            boxShadow: "0 0 10px rgba(0,0,0,0.2)",
            minWidth: "200px",
          }}
        >
          <div className="fw-bold mb-2">{selectedDoc.name}</div>
          <div className="list-group list-group-flush">
            <button className="list-group-item list-group-item-action" onClick={handleEdit}>
              Изменить
            </button>
            <button className="list-group-item list-group-item-action" onClick={handleDelete}>
              Удалить
            </button>
            <button className="list-group-item list-group-item-action" onClick={handleArchive}>
              Архивировать
            </button>
            <button className="list-group-item list-group-item-action" onClick={handleCopy}>
              Копировать
            </button>
            <button className="list-group-item list-group-item-action" onClick={handleRename}>
              Переименовать
            </button>
            <button className="list-group-item list-group-item-action" onClick={handleChangeAccess}>
              Изменить доступ
            </button>
            <button className="list-group-item list-group-item-action" onClick={handleToggleRoom}>
              {selectedDoc.room ? "Удалить из комнаты" : "Добавить в комнату"}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}