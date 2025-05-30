import { useState } from "react";
import api from "./api"; 

export default function LoginForm({ theme, onLogin }) {
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
		setLoading(false);
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
			className="form-control"
			id="loginEmail"
			placeholder="Email"
			value={email}
			onChange={e => setEmail(e.target.value)}
			required
		/>
		<label htmlFor="loginEmail">Email</label>
		</div>
		<div className="form-floating mb-3">
		<input
			type="password"
			className="form-control"
			id="loginPassword"
			placeholder="Пароль"
			value={password}
			onChange={e => setPassword(e.target.value)}
			required
		/>
		<label htmlFor="loginPassword">Пароль</label>
		</div>
		{error && <div className="alert alert-danger py-2">{error}</div>}
		<button className="btn btn-primary w-100" disabled={loading}>
		{loading ? (
			<span className="spinner-border spinner-border-sm me-2" />
		) : null}
		Войти
		</button>
	</form>
	);
}