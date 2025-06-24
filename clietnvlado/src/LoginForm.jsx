import { useState } from "react";
import api from "./api";
import { FaSpinner, FaEye, FaEyeSlash } from "react-icons/fa";
import { Link, useNavigate } from "react-router-dom";

export default function LoginForm({ theme, onLogin }) {
	const navigate = useNavigate();
	const [email, setEmail] = useState("");
	const [password, setPassword] = useState("");
	const [error, setError] = useState("");
	const [loading, setLoading] = useState(false);
  const [showPwd, setShowPwd] = useState(false);

  const handleLogin = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const { data } = await api.post("/auth/login", { email, password });
      const token = data.token;
      sessionStorage.setItem("token", token);

      const base64Payload = token.split('.')[1];
      const jsonPayload = atob(base64Payload);
      const payload = JSON.parse(jsonPayload);
      const userId = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];

      if (userId) {
        sessionStorage.setItem("userId", userId);
      }

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
          type={showPwd ? "text" : "password"}
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

        <button type="button"
                className="btn btn-link position-absolute top-50 end-0 translate-middle-y
                          text-decoration-none p-0 me-2"
                tabIndex={-1}
                onClick={()=>setShowPwd(p=>!p)}
                aria-label={showPwd?"Скрыть пароль":"Показать пароль"}>
          {showPwd ? <FaEyeSlash/> : <FaEye/>}
        </button>
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
