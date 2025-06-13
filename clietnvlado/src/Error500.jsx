// src/Error500.jsx
import { Link } from "react-router-dom";

export default function Error500() {
  return (
    <div className="d-flex flex-column justify-content-center align-items-center vh-100 text-center p-3">
      <img
        src="/500errorboundary.png"
        alt="500 Internal Server Error"
        style={{ maxWidth: 400, width: "100%" }}
        className="mb-4"
      />
      <h1 className="display-6 fw-bold">Ошибка сервера (500)</h1>
      <p className="lead mb-4">
        Что-то пошло не так. Наши разработчики уже работают над этим.
      </p>
      <Link to="/" className="btn btn-primary">
        На главную
      </Link>
    </div>
  );
}
