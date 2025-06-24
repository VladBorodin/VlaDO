import { useState, useEffect, useCallback } from "react";
import api from "./api";
import { FaSpinner, FaEye, FaEyeSlash } from "react-icons/fa";
import debounce from "./utils/debounce";
import { useAlert } from "./contexts/AlertContext"

export default function RegisterForm({ theme, onLogin }) {
  const [email, setEmail] = useState("");
  const [username, setUsername] = useState("");
  const [nameOk , setNameOk] = useState(true);
  const [checking , setChecking] = useState(false);
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const { push } = useAlert();
  const [showPwd , setShowPwd ] = useState(false);
  const [showConf, setShowConf] = useState(false);

  function validate() {
    if (!email.includes("@")) return "Некорректный email";
    if (username.length < 3) return "Имя не менее 3 символов";
    if (!nameOk) return "Имя уже занято"
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
      if (data.token) {
        sessionStorage.setItem("token", data.token);
        onLogin(data.token);
        return;
      }
      setLoading(false);
      push("Регистрация прошла успешно! Войдите в систему.", "success");
    } catch (err) {
      if (err.response?.status === 409)
        setError(err.response.data);
      else
        setError("Ошибка сети или сервера.");
      setLoading(false);
    }
  };

  const checkName = useCallback(
    debounce(async (name) => {
      if (name.trim().length < 3) return setNameOk(true);
      setChecking(true);
      try {
        const { data } = await api.get("/users/name-exists",
                                      { params:{ name }});
        setNameOk(!data.exists);
      } finally { setChecking(false); }
    }, 350),
    []
  );

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

      <div className="form-floating mb-3 position-relative">
        <input
          type={showPwd ? "text" : "password"}
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

        <button type="button"
          className="btn btn-link position-absolute top-50 end-0 translate-middle-y
                     text-decoration-none p-0 me-2"
          tabIndex={-1}
          onClick={()=>setShowPwd(p=>!p)}
          aria-label={showPwd?"Скрыть пароль":"Показать пароль"}>
          {showPwd ? <FaEyeSlash/> : <FaEye/>}
        </button>
      </div>

      <div className="form-floating mb-3 position-relative">
        <input
          type={showPwd ? "text" : "password"}
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
      {!nameOk && !error && (
        <div className="alert alert-warning py-2">Имя уже занято</div>
      )}

      <button className="btn btn-success w-100" disabled={loading || !nameOk}>
        {loading && <FaSpinner className="me-2 spin" />}
        Зарегистрироваться
      </button>
    </form>
  );
}
