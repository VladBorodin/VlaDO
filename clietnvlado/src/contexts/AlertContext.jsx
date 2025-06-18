import { createContext, useContext, useState, useCallback } from "react";

/** Внутренний контекст */
const AlertCtx = createContext();

/** Провайдер, который держит очередь тостов */
export const AlertProvider = ({ children }) => {
  const [alerts, setAlerts] = useState([]);

  /** Добавить новый тост */
  const push = useCallback((text, variant = "info", ms = 4000) => {
    const id = crypto.randomUUID();
    setAlerts(prev => [...prev, { id, text, variant }]);

    // авто-скрытие
    setTimeout(() => {
      setAlerts(prev => prev.filter(a => a.id !== id));
    }, ms);
  }, []);

  return (
    <AlertCtx.Provider value={{ push }}>
      {children}

      {/* Bootstrap toast-container */}
      <div
        className="toast-container position-fixed top-0 end-0 p-3"
        style={{ zIndex: 1060 }}
      >
        {alerts.map(a => (
          <div
            key={a.id}
            className={`toast show text-bg-${a.variant}`}
            role="alert"
          >
            <div className="d-flex">
              <div className="toast-body">{a.text}</div>
              <button
                type="button"
                className="btn-close btn-close-white me-2 m-auto"
                onClick={() =>
                  setAlerts(prev => prev.filter(x => x.id !== a.id))
                }
              />
            </div>
          </div>
        ))}
      </div>
    </AlertCtx.Provider>
  );
};

/** Хук для использования: const { push } = useAlert(); */
export const useAlert = () => useContext(AlertCtx);
