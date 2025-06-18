import { useEffect, useState } from "react";
import clsx from "clsx";

export default function AppAlert({ text, type = "info", onClose, ttl = 4000 }) {
  const [show, setShow] = useState(true);

  useEffect(() => {
    const id = setTimeout(() => setShow(false), ttl);
    return () => clearTimeout(id);
  }, [ttl]);

  useEffect(() => {
    if (!show && onClose) {
      const timer = setTimeout(onClose, 350);
      return () => clearTimeout(timer);
    }
  }, [show, onClose]);

  return (
    <div
      className={clsx(
        "app-alert alert alert-dismissible fade",
        show ? "show" : "",
        `alert-${type}`
      )}
      role="alert"
    >
      {text}
      <button
        type="button"
        className="btn-close"
        onClick={() => setShow(false)}
      />
    </div>
  );
}