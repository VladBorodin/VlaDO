import { useState } from "react";
import LoginForm from "./LoginForm";
import RegisterForm from "./RegisterForm";
import logo from "/logo.png";

export default function AuthPage({ onLogin }) {   // ← принимаем проп!
  const [activeTab, setActiveTab] = useState("login");
  const [theme, setTheme] = useState(
    () => localStorage.getItem("theme") || "light"
  );

  // Смена темы
  const toggleTheme = () => {
    const next = theme === "light" ? "dark" : "light";
    setTheme(next);
    localStorage.setItem("theme", next);
    document.body.className = next === "dark" ? "bg-dark text-light" : "bg-light text-dark";
  };

  // Сбросить классы при монтировании (нужно один раз)
  useState(() => {
    document.body.className = theme === "dark"
        ? "bg-dark text-light"
        : "bg-light text-dark";
  }, []);

  return (
    <div className="container min-vh-100 d-flex flex-column justify-content-center align-items-center">
      <div className="mb-4 text-center">
        <img src="/logo.png" alt="VlaDO" height={80} className="mb-2" />
        <h2 className="fw-bold">Добро пожаловать в VlaDO</h2>
        <button className="btn btn-outline-secondary btn-sm mt-2" onClick={toggleTheme}>
          {theme === "dark" ? "Светлая тема" : "Тёмная тема"}
        </button>
      </div>
      <div className="card shadow rounded" style={{ minWidth: 340, maxWidth: 370 }}>
        <div className="card-header d-flex justify-content-center bg-transparent">
          <ul className="nav nav-tabs card-header-tabs">
            <li className="nav-item">
              <button
                className={`nav-link ${activeTab === "login" ? "active" : ""}`}
                onClick={() => setActiveTab("login")}
                type="button"
              >
                Войти
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
        </div>
        <div className="card-body">
          {activeTab === "login"
            ? <LoginForm theme={theme} onLogin={onLogin} />
            : <RegisterForm theme={theme} />}
        </div>
      </div>
    </div>
  );
}
