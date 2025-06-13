import { useState } from "react";
import api from "./api";
import { FaSpinner } from "react-icons/fa";
import { Link, useNavigate } from "react-router-dom";

export default function LoginForm({ theme, onLogin }) {
	const navigate = useNavigate();
	const [email, setEmail] = useState("");
	const [password, setPassword] = useState("");
	const [error, setError] = useState("");
	const [loading, setLoading] = useState(false);

  const handleLogin = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const { data } = await api.post("/auth/login", { email, password });
      const token = data.token;
      sessionStorage.setItem("token", token);
      onLogin(token);
	  navigate("/", { replace: true });
    } catch (err) {
      setError("Неверные данные или ошибка сети.");
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleLogin} autoComplete="off">
      <div className="form-floating mb-3">
        <input
          type="email"
          id="loginEmail"
          className={`form-control ${
            theme === "dark" ? "bg-secondary text-light border-0" : ""
          }`}
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <label htmlFor="loginEmail">Email</label>
      </div>

      <div className="form-floating mb-3">
        <input
          type="password"
          id="loginPassword"
          className={`form-control ${
            theme === "dark" ? "bg-secondary text-light border-0" : ""
          }`}
          placeholder="Пароль"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <label htmlFor="loginPassword">Пароль</label>
      </div>

      {error && <div className="alert alert-danger py-2">{error}</div>}

      <button className="btn btn-primary w-100" disabled={loading}>
        {loading && <FaSpinner className="me-2 spin" />}
        Войти
      </button>

      <div className="mt-3 text-center">
        <Link to="/forgot-password">Забыли пароль?</Link>
      </div>
    </form>
  );
}
