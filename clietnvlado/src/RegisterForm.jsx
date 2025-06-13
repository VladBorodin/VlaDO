import { useState } from "react";
import api from "./api";
import { FaSpinner } from "react-icons/fa";

export default function RegisterForm({ theme, onLogin }) {
  const [email, setEmail] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  function validate() {
    if (!email.includes("@")) return "Некорректный email";
    if (username.length < 3) return "Имя не менее 3 символов";
    if (password.length < 6) return "Пароль не менее 6 символов";
    if (password !== confirm) return "Пароли не совпадают";
    return null;
  }

  const handleRegister = async (e) => {
    e.preventDefault();
    setError("");
    const err = validate();
    if (err) {
      setError(err);
      return;
    }
    setLoading(true);
    try {
      const { data } = await api.post("/auth/register", {
        email,
        name: username,
        password,
        confirmPassword: confirm
      });
      // если бэкенд сразу возвращает token
      if (data.token) {
        sessionStorage.setItem("token", data.token);
        onLogin(data.token);
        return;
      }
      // иначе — перенаправляем на вкладку логина
      setLoading(false);
      alert("Регистрация прошла успешно! Войдите в систему.");
    } catch {
      setError("Ошибка при регистрации. Попробуйте ещё раз.");
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleRegister} autoComplete="off">
      <div className="form-floating mb-3">
        <input
          type="email"
          id="regEmail"
          className={`form-control ${
            theme === "dark" ? "bg-secondary text-light border-0" : ""
          }`}
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <label htmlFor="regEmail">Email</label>
      </div>

      <div className="form-floating mb-3">
        <input
          type="text"
          id="regUsername"
          className={`form-control ${
            theme === "dark" ? "bg-secondary text-light border-0" : ""
          }`}
          placeholder="Имя пользователя"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required
        />
        <label htmlFor="regUsername">Имя пользователя</label>
      </div>

      <div className="form-floating mb-3">
        <input
          type="password"
          id="regPassword"
          className={`form-control ${
            theme === "dark" ? "bg-secondary text-light border-0" : ""
          }`}
          placeholder="Пароль"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <label htmlFor="regPassword">Пароль</label>
      </div>

      <div className="form-floating mb-3">
        <input
          type="password"
          id="regConfirm"
          className={`form-control ${
            theme === "dark" ? "bg-secondary text-light border-0" : ""
          }`}
          placeholder="Повторите пароль"
          value={confirm}
          onChange={(e) => setConfirm(e.target.value)}
          required
        />
        <label htmlFor="regConfirm">Повторите пароль</label>
      </div>

      {error && <div className="alert alert-danger py-2">{error}</div>}

      <button className="btn btn-success w-100" disabled={loading}>
        {loading && <FaSpinner className="me-2 spin" />}
        Зарегистрироваться
      </button>
    </form>
  );
}
