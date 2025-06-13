import { useState } from "react";
import { useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { FaArrowLeft, FaSun, FaMoon, FaEye, FaEyeSlash } from "react-icons/fa";
import api from "./api";


export default function ResetPassword() {
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const token = params.get("token");
  const userId = params.get("userId");

  const [theme, setTheme] = useState(localStorage.getItem("theme") || "light");
  const dark = theme === "dark";

  useEffect(() => {
    document.body.classList.add(theme === "dark" ? "dark" : "light");
    document.body.classList.remove(theme === "dark" ? "light" : "dark");

    return () => {
      document.body.classList.remove("dark", "light");
    };
  }, [theme]);

  const [newPwd, setNewPwd] = useState("");
  const [confirmPwd, setConfirmPwd] = useState("");
  const [showNew, setShowNew] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleReset = async (e) => {
    e.preventDefault();
    setError("");
    setMessage("");

    if (!newPwd || !confirmPwd) return setError("Заполните все поля");
    if (newPwd !== confirmPwd) return setError("Пароли не совпадают");

    try {
      setLoading(true);
      await api.post("/password/reset", {
        userId,
        token,
        newPassword: newPwd,
        confirmPassword: confirmPwd
      });
      setMessage("Пароль успешно сброшен! Теперь вы можете войти с новым паролем.");
    } catch (err) {
      setError(err.response?.data?.error || "Ошибка сброса пароля");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-vh-100 d-flex flex-column justify-content-center" style={{ backgroundColor: dark ? "#121212" : "#f8f9fa" }}>
      {/* Шапка */}
      <nav className={`navbar navbar-expand-lg shadow-sm ${dark ? "bg-dark text-light" : "bg-light text-dark"}`}>
        <div className="container">
          <button className="btn btn-link d-flex align-items-center p-0" onClick={() => navigate("/")}>
            <FaArrowLeft size={24} className={dark ? "text-light" : "text-dark"} />
            <span className={`ms-2 fw-bold ${dark ? "text-light" : "text-dark"}`}>
              Сброс пароля
            </span>
          </button>
          <div className="ms-auto">
            <button
              className="btn btn-link"
              title="Переключить тему"
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

      {/* Контент */}
      <div className="container my-4 flex-fill d-flex justify-content-center">
        <div className={`card w-100 ${dark ? "bg-dark text-light border-secondary" : ""}`} style={{ maxWidth: 500 }}>
          <div className={`card-header ${dark ? "bg-secondary text-light" : ""}`}>
            <h5 className="mb-0">Введите новый пароль</h5>
          </div>
          <div className="card-body">
            {error && <div className="alert alert-danger">{error}</div>}
            {message && <div className="alert alert-success">{message}</div>}

            <form onSubmit={handleReset}>
              {/* Новый пароль */}
              <div className="form-floating mb-3 position-relative">
                <input
                  type={showNew ? "text" : "password"}
                  id="newPwd"
                  className="form-control"
                    style={dark ? { backgroundColor: "#3a3a3a", color: "#eee", border: "1px solid #555" } : {}}
                  placeholder="Новый пароль"
                  value={newPwd}
                  onChange={e => setNewPwd(e.target.value)}
                  required
                />
                <label htmlFor="newPwd">Новый пароль</label>
                <button
                  type="button"
                  onClick={() => setShowNew(!showNew)}
                  className="btn btn-sm position-absolute top-50 end-0 translate-middle-y me-2"
                  style={{ zIndex: 2 }}
                >
                  {showNew ? <FaEyeSlash /> : <FaEye />}
                </button>
              </div>

              {/* Подтверждение пароля */}
              <div className="form-floating mb-3 position-relative">
                <input
                  type={showConfirm ? "text" : "password"}
                  id="confirmPwd"
                  className="form-control"
                    style={dark ? { backgroundColor: "#3a3a3a", color: "#eee", border: "1px solid #555" } : {}}
                  placeholder="Подтвердите пароль"
                  value={confirmPwd}
                  onChange={e => setConfirmPwd(e.target.value)}
                  required
                />
                <label htmlFor="confirmPwd">Подтвердите пароль</label>
                <button
                  type="button"
                  onClick={() => setShowConfirm(!showConfirm)}
                  className="btn btn-sm position-absolute top-50 end-0 translate-middle-y me-2"
                  style={{ zIndex: 2 }}
                >
                  {showConfirm ? <FaEyeSlash /> : <FaEye />}
                </button>
              </div>

              <button
                className="btn btn-primary w-100"
                type="submit"
                disabled={loading}
              >
                {loading && (
                  <span className="spinner-border spinner-border-sm me-2" role="status" />
                )}
                {loading ? "Сохраняем..." : "Сбросить пароль"}
              </button>
            </form>
          </div>
        </div>
      </div>

      {/* Футер */}
      <footer className={`py-3 text-center ${dark ? "bg-dark text-secondary" : "bg-light text-muted"}`}>
        &copy; {new Date().getFullYear()} VlaDO
      </footer>
    </div>
  );
}
