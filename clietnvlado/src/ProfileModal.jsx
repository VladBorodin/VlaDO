import { useState, useEffect, useCallback } from "react";
import api from "./api";
import { FaTimes } from "react-icons/fa";
import debounce from "./utils/debounce";

export default function ProfileModal({ show, onClose, user, onUpdateUser, theme }) {
  const [originalName] = useState(user.name);

  const [name, setName] = useState(user.name);
  const [email, setEmail] = useState(user?.email ?? "");

  const [currentPwd, setCurrentPwd] = useState("");
  const [newPwd, setNewPwd] = useState("");
  const [confirmPwd, setConfirmPwd] = useState("");

  const [nameOk, setNameOk] = useState(true);
  const [error,  setError ] = useState("");
  const [success,setSuccess] = useState("");
  const [loading,setLoading] = useState(false);

  const [openSection, setOpenSection] = useState(null);

  const modalContentStyle = theme === "dark"
  ? { background: "#1e1e1e", color: "#f8f9fa" }
  : {};

  const modalSectionClass = theme === "dark"
    ? "bg-dark text-light"
    : "bg-light text-dark";

  useEffect(() => {
    if (show) {
      setName (user?.name  ?? "");
      setEmail(user?.email ?? "");
      setCurrentPwd("");
      setNewPwd("");
      setConfirmPwd("");
      setSuccess("");
      setError("");
    }
  }, [show, user]);

  useEffect(() => {
    document.body.className = theme === "dark" ? "dark" : "light";
  }, [theme]);

  const checkName = useCallback(
    debounce(async n => {
      if (!n || typeof n !== "string" || !n.trim() || n === originalName) {
        setNameOk(true);
        return;
      }
      const { data } = await api.get("/users/name-exists",{ params:{ name:n } });
      setNameOk(!data.exists);
    }, 350),
    [originalName]
  );

  useEffect(() => { checkName(name); }, [name, checkName]);
  
  if (!show) return null;

  const handleUpdateProfile = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    setLoading(true);
    if (!nameOk) { setError("Имя занято"); return; }
    try {
      const { data } = await api.put("/users/me", { name, email });
      setSuccess("Профиль обновлён");
      onUpdateUser(data);
    } catch (err) {
      if (err.response?.status === 409)
        setError(err.response.data);
      else
        setError("Не удалось обновить профиль");
    }
    setLoading(false);
  };

  const handleChangePassword = async (e) => {
    e.preventDefault();
    if (newPwd !== confirmPwd) {
      setError("Пароли не совпадают");
      return;
    }
    setError("");
    setSuccess("");
    setLoading(true);
    try {
      await api.post("/users/change-password", {
        currentPassword: currentPwd,
        newPassword: newPwd,
      });
      setSuccess("Пароль изменён успешно");
      setCurrentPwd("");
      setNewPwd("");
      setConfirmPwd("");
    } catch {
      setError("Не удалось сменить пароль");
    }
    setLoading(false);
  };

  return (
    <>
      <div className="modal fade show" tabIndex="-1" style={{ display: "block" }}>
        <div className="modal-dialog modal-dialog-centered">
          <div className="modal-content" style={modalContentStyle}>
            {/* Заголовок */}
            <div className={`modal-header ${modalSectionClass}`}>
              <h5 className="modal-title">Управление профилем</h5>
              <button type="button" className="btn-close" onClick={onClose} />
            </div>

            {/* Тело модала */}
            <div className={`modal-body ${modalSectionClass}`}>
              {/* Ошибки / Успех */}
              {error && <div className="alert alert-danger">{error}</div>}
              {success && <div className="alert alert-success">{success}</div>}

              {/* Аккордеон */}
              <div className="accordion" id="profileAccordion">
                {/* 1. Управление логином */}
                <div className="accordion-item">
                  <h2 className="accordion-header" id="headingProfile">
                    <button
                      className={`accordion-button ${openSection !== "profile" ? "collapsed" : ""}`}
                      type="button"
                      onClick={() =>
                        setOpenSection(openSection === "profile" ? null : "profile")
                      }
                    >
                      Управление логином
                    </button>
                  </h2>
                  <div
                    id="collapseProfile"
                    className={`accordion-collapse collapse ${openSection === "profile" ? "show" : ""}`}
                    aria-labelledby="headingProfile"
                    data-bs-parent="#profileAccordion"
                  >
                    <div className={`accordion-body ${modalSectionClass}`}>
                      <form onSubmit={handleUpdateProfile} className="mb-4">
                        <div className="form-floating mb-3">
                          <input
                            type="text"
                            className="form-control"
                            id="profileName"
                            placeholder="Имя"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            required
                          />
                          <label htmlFor="profileName">Имя</label>
                        </div>

                        {!nameOk && (
                          <div className="alert alert-warning py-2">
                            Имя уже занято
                          </div>
                        )}

                        <div className="form-floating mb-3">
                          <input
                            type="email"
                            className="form-control"
                            id="profileEmail"
                            placeholder="Email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                          />
                          <label htmlFor="profileEmail">Email</label>
                        </div>
                        <button className="btn btn-primary w-100" disabled={loading}>
                          {loading ? (
                            <span className="spinner-border spinner-border-sm me-2" />
                          ) : null}
                          Сохранить
                        </button>
                      </form>
                    </div>
                  </div>
                </div>

                {/* 2. Управление паролем */}
                <div className="accordion-item">
                  <h2 className="accordion-header" id="headingPassword">
                    <button
                      className={`accordion-button ${openSection !== "password" ? "collapsed" : ""}`}
                      type="button"
                      onClick={() =>
                        setOpenSection(openSection === "password" ? null : "password")
                      }
                    >
                      Управление паролем
                    </button>
                  </h2>
                  <div
                    id="collapsePassword"
                    className={`accordion-collapse collapse ${openSection === "password" ? "show" : ""}`}
                    aria-labelledby="headingPassword"
                    data-bs-parent="#profileAccordion"
                  >
                    <div className={`accordion-body ${modalSectionClass}`}>
                      <form onSubmit={handleChangePassword}>
                        <div className="form-floating mb-3">
                          <input
                            type="password"
                            className="form-control"
                            id="currentPwd"
                            placeholder="Текущий пароль"
                            value={currentPwd}
                            onChange={(e) => setCurrentPwd(e.target.value)}
                            required
                          />
                          <label htmlFor="currentPwd">Текущий пароль</label>
                        </div>
                        <div className="form-floating mb-3">
                          <input
                            type="password"
                            className="form-control"
                            id="newPwd"
                            placeholder="Новый пароль"
                            value={newPwd}
                            onChange={(e) => setNewPwd(e.target.value)}
                            required
                          />
                          <label htmlFor="newPwd">Новый пароль</label>
                        </div>
                        <div className="form-floating mb-3">
                          <input
                            type="password"
                            className="form-control"
                            id="confirmPwd"
                            placeholder="Подтвердите пароль"
                            value={confirmPwd}
                            onChange={(e) => setConfirmPwd(e.target.value)}
                            required
                          />
                          <label htmlFor="confirmPwd">Подтвердите пароль</label>
                        </div>
                        <button className="btn btn-warning w-100" disabled={loading}>
                          {loading ? (
                            <span className="spinner-border spinner-border-sm me-2" />
                          ) : null}
                          Сменить пароль
                        </button>
                      </form>
                    </div>
                  </div>
                </div>
              </div>
              {/* Конец аккордеона */}
            </div>
          </div>
        </div>
      </div>
      <div className="modal-backdrop fade show" />
    </>
  );
}