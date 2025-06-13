// src/CreateRoomPage.jsx

import { useState, useEffect } from "react";
import { FaSun, FaMoon, FaArrowLeft, FaPlus } from "react-icons/fa";
import { useNavigate } from "react-router-dom";
import api from "./api"; // ваш axios-обёртка
import { AccessLevelOptions } from "./constants"; 
// AccessLevelOptions — массив для выпадающего списка ролей (см. ниже)

export default function CreateRoomPage() {
  const navigate = useNavigate();

  // Тема (light / dark)
  const [theme, setTheme] = useState(
    () => localStorage.getItem("theme") || "light"
  );
  useEffect(() => {
    document.body.className =
      theme === "dark" ? "bg-dark text-light" : "bg-light text-dark";
    localStorage.setItem("theme", theme);
  }, [theme]);

  // Состояния формы
  const [title, setTitle] = useState("");
  const [defaultAccess, setDefaultAccess] = useState(""); // выбор из AccessLevelOptions
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  // Обработка сабмита формы
  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    if (!title.trim()) {
      setError("Название комнаты не может быть пустым");
      return;
    }
    if (!defaultAccess) {
      setError("Выберите уровень доступа по умолчанию");
      return;
    }
    setLoading(true);
    try {
      // Предполагаем, что бэк-энд принимает { title, defaultAccessLevel }
      await api.post("/rooms", {
        title,
        defaultAccessLevel: Number(defaultAccess),
      });
      navigate(-1);
    } catch (err) {
      console.error(err);
      setError(
        err.response?.data?.error ||
          err.response?.data?.message ||
          "Ошибка при создании комнаты"
      );
    }
    setLoading(false);
  };

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
            onClick={() => navigate(-1)}
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
              Создать комнату
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

      {/* Контент формы */}
      <div className="container my-4 flex-fill">
        <div
          className={`card mx-auto ${
            theme === "dark" ? "bg-secondary text-light" : ""
          }`}
          style={{ maxWidth: 500 }}
        >
          <div
            className={`card-header ${
              theme === "dark" ? "bg-dark text-light" : ""
            }`}
          >
            <h5 className="mb-0">Новая комната</h5>
          </div>
          <div className="card-body">
            {error && <div className="alert alert-danger">{error}</div>}
            <form onSubmit={handleSubmit} autoComplete="off">
              {/* Название комнаты */}
              <div className="form-floating mb-3">
                <input
                  type="text"
                  id="roomTitle"
                  className="form-control"
                  placeholder="Название комнаты"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  required
                />
                <label htmlFor="roomTitle">Название комнаты</label>
              </div>

              {/* Уровень доступа по умолчанию */}
              <div className="form-floating mb-3">
                <select
                  id="defaultAccess"
                  className="form-select"
                  value={defaultAccess}
                  onChange={(e) => setDefaultAccess(e.target.value)}
                  required
                >
                  <option value="">— Выберите уровень —</option>
                  {AccessLevelOptions.map((opt) => (
                    <option key={opt.value} value={opt.value}>
                      {opt.label}
                    </option>
                  ))}
                </select>
                <label htmlFor="defaultAccess">
                  Уровень доступа по умолчанию
                </label>
              </div>

              <button
                className={`btn btn-primary w-100`}
                type="submit"
                disabled={loading}
              >
                {loading && (
                  <span
                    className="spinner-border spinner-border-sm me-2"
                    role="status"
                    aria-hidden="true"
                  />
                )}
                {loading ? "Создание..." : "Создать"}
              </button>
            </form>
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
