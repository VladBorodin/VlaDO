import { useState, useEffect } from "react";
import LoginForm from "./LoginForm";
import RegisterForm from "./RegisterForm";
import { FaSun, FaMoon } from "react-icons/fa";

export default function AuthPage({ onLogin }) {
  const [activeTab, setActiveTab] = useState("login");
  const [theme, setTheme] = useState(() => localStorage.getItem("theme") || "light");
  useEffect(() => {
    document.body.className = theme === "dark" 
      ? "bg-dark text-light" 
      : "bg-light text-dark";
    localStorage.setItem("theme", theme);
  }, [theme]);

  const cardBgClass = theme === "dark"
    ? "bg-dark bg-gradient text-light"
    : "bg-white text-dark";

  return (
    <div className="d-flex flex-column align-items-center justify-content-center min-vh-100 p-3">
      <img src="/logo-small.png" alt="Logo" height={80} className="mb-4" />

      <div className={`card shadow ${cardBgClass}`} style={{ minWidth: 340, maxWidth: 380 }}>
        <div className="card-header bg-transparent d-flex justify-content-between">
          <ul className="nav nav-tabs card-header-tabs">
            <li className="nav-item">
              <button
                className={`nav-link ${activeTab === "login" ? "active" : ""}`}
                onClick={() => setActiveTab("login")}
                type="button"
              >
                Вход
              </button>
            </li>
            <li className="nav-item">
              <button
                className={`nav-link ${activeTab === "register" ? "active" : ""}`}
                onClick={() => setActiveTab("register")}
                type="button"
              >
                Регистрация
              </button>
            </li>
          </ul>
          <button
            className="btn btn-link"
            onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
            title="Переключить тему"
          >
            {theme === "dark" ? <FaSun size={20}/> : <FaMoon size={20}/>}
          </button>
        </div>

        <div className="card-body">
          {activeTab === "login"
            ? <LoginForm theme={theme} onLogin={onLogin}/>
            : <RegisterForm theme={theme} onLogin={onLogin}/>}
        </div>
      </div>

      <footer className={`mt-4 text-center w-100 ${theme === "dark" ? "text-secondary" : "text-muted"}`}>
        &copy; {new Date().getFullYear()} VlaDO. Все права защищены.
      </footer>
    </div>
  );
}
