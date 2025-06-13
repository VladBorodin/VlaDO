// src/ForgotPasswordPage.jsx

import { useState } from "react";
import { FaSun, FaMoon, FaArrowLeft } from "react-icons/fa";
import { useNavigate } from "react-router-dom";
import api from "./api";

export default function ForgotPasswordPage() {
  const navigate = useNavigate();

  // Тема
  const [theme, setTheme] = useState(
    () => localStorage.getItem("theme") || "light"
  );
  // Состояние формы
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [info, setInfo] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setInfo("");
    if (!email.trim()) {
      setError("Введите email");
      return;
    }
    setLoading(true);
    try {
      await api.post("/password/forgot", { email });
      setInfo("Если email зарегистрирован, на него будет отправлено письмо со ссылкой для сброса пароля.");
    } catch (err) {
      console.error(err);
      setError(
        err.response?.data?.error ||
          err.response?.data?.message ||
          "Ошибка при отправке письма"
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
              Восстановление пароля
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
      <div className="container my-4 flex-fill d-flex justify-content-center">
        <div
          className={`card w-100 ${
            theme === "dark" ? "bg-secondary text-light" : ""
          }`}
          style={{ maxWidth: 500 }}
        >
          <div
            className={`card-header ${
              theme === "dark" ? "bg-dark text-light" : ""
            }`}
          >
            <h5 className="mb-0">Запросить сброс пароля</h5>
          </div>
          <div className="card-body">
            {error && <div className="alert alert-danger">{error}</div>}
            {info && <div className="alert alert-info">{info}</div>}
            <form onSubmit={handleSubmit} autoComplete="off">
              <div className="form-floating mb-3">
                <input
                  type="email"
                  id="forgotEmail"
                  className="form-control"
                  placeholder="Email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
                <label htmlFor="forgotEmail">Email</label>
              </div>

              <button
                className="btn btn-primary w-100"
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
                {loading ? "Отправляем..." : "Отправить письмо"}
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
