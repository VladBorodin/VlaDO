import { useState, useEffect } from "react";
import api from "./api";
import { Link } from "react-router-dom";
import { FaSpinner, FaMoon, FaSun } from "react-icons/fa";

export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const [theme, setTheme] = useState(localStorage.getItem("theme") || "light");
  const dark = theme === "dark";

  useEffect(() => {
    document.body.classList.add(theme === "dark" ? "dark" : "light");
    document.body.classList.remove(theme === "dark" ? "light" : "dark");

    return () => {
      document.body.classList.remove("dark", "light");
    };
  }, [theme]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setMessage("");
    if (!email.includes("@")) {
      setError("Введите корректный email");
      return;
    }
    setLoading(true);
    try {
      const { data } = await api.post("/auth/forgot-password", { email });
      setMessage(data.message || "Ссылка для сброса пароля отправлена.");
    } catch (err) {
      setError(
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Ошибка при отправке письма"
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-vh-100 d-flex flex-column justify-content-center" style={{ backgroundColor: dark ? "#121212" : "#f8f9fa" }}>
      {/* Шапка */}
      <nav className={`navbar navbar-expand-lg shadow-sm ${dark ? "bg-dark text-light" : "bg-light text-dark"}`}>
        <div className="container">
          <span className={`fw-bold ${dark ? "text-light" : "text-dark"}`}>Восстановление пароля</span>
          <div className="ms-auto">
            <button
              className="btn btn-link"
              title="Сменить тему"
              onClick={() => {
                const next = dark ? "light" : "dark";
                setTheme(next);
                localStorage.setItem("theme", next);
              }}
              style={{ color: dark ? "#ccc" : "#333" }}
            >
              {dark ? <FaSun size={20} /> : <FaMoon size={20} />}
            </button>
          </div>
        </div>
      </nav>

      <div className="container my-4 flex-fill d-flex justify-content-center">
        <div className={`card w-100 ${dark ? "bg-dark text-light border-secondary" : ""}`} style={{ maxWidth: 500 }}>
          <div className={`card-header ${dark ? "bg-secondary text-light" : ""}`}>
            <h5 className="mb-0">Введите ваш email</h5>
          </div>
          <div className="card-body">
            {error && <div className="alert alert-danger">{error}</div>}
            {message && <div className="alert alert-success">{message}</div>}

            <form onSubmit={handleSubmit} autoComplete="off">
              <div className={`form-floating mb-3 ${dark ? "dark-label" : ""}`}>
                <input
                  type="email"
                  id="forgotEmail"
                  className={`form-control ${dark ? "dark-placeholder" : ""}`}
                  placeholder="email@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  style={dark ? { backgroundColor: "#3a3a3a", color: "#eee", border: "1px solid #555" } : {}}
                />
                <label htmlFor="forgotEmail">Email</label>
              </div>

              <button className="btn btn-primary w-100" disabled={loading}>
                {loading && <FaSpinner className="me-2 spin" />}
                Отправить ссылку
              </button>
            </form>

            <div className="mt-3 text-center">
              <Link to="/login" className={dark ? "text-light" : ""}>
                Вернуться на страницу входа
              </Link>
            </div>
          </div>
        </div>
      </div>

      <footer className={`py-3 text-center ${dark ? "bg-dark text-secondary" : "bg-light text-muted"}`}>
        &copy; {new Date().getFullYear()} VlaDO
      </footer>
    </div>
  );
}
