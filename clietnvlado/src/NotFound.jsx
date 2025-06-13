import { Link } from "react-router-dom";

export default function NotFound() {
  return (
    <div className="d-flex flex-column justify-content-center align-items-center vh-100 text-center p-3">
      <img
        src="/404notfound.png"
        alt="404 Not Found"
        style={{ maxWidth: 400, width: "100%" }}
        className="mb-4"
      />
      <h1 className="display-5 fw-bold">Страница не найдена</h1>
      <p className="lead mb-4">
        Упс… кажется, такой страницы не существует.
      </p>
      <Link to="/" className="btn btn-primary">
        Вернуться на главную
      </Link>
    </div>
  );
}