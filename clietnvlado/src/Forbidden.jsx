// src/Forbidden.jsx
import { Link } from "react-router-dom";

export default function Forbidden() {
  return (
    <div className="d-flex flex-column justify-content-center align-items-center vh-100 text-center p-3">
      <img
        src="/403forbidden.png"
        alt="403 Forbidden"
        style={{ maxWidth: 400, width: "100%" }}
        className="mb-4"
      />
      <h1 className="display-6 fw-bold">Доступ запрещён</h1>
      <p className="lead mb-4">
        У вас нет прав для просмотра этой страницы.
      </p>
      <Link to="/" className="btn btn-primary">
        На главную
      </Link>
    </div>
  );
}