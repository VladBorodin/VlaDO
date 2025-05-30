import { useState } from "react";

export default function RegisterForm({ theme }) {
	const [email, setEmail] = useState("");
	const [username, setUsername] = useState("");
	const [password, setPassword] = useState("");
	const [confirm, setConfirm] = useState("");
	const [error, setError] = useState("");
	const [success, setSuccess] = useState("");
	const [loading, setLoading] = useState(false);

	// Минимальная валидация для email и пароля
	function validate() {
	if (!email.includes("@")) return "Некорректный email";
	if (username.length < 3) return "Имя должно быть не короче 3 символов";
	if (password.length < 6) return "Пароль должен быть не короче 6 символов";
	if (password !== confirm) return "Пароли не совпадают";
	return null;
	}

	async function handleRegister(e) {
	e.preventDefault();
	setError("");
	setSuccess("");
	const val = validate();
	if (val) {
		setError(val);
		return;
	}
	setLoading(true);
	try {
		// Здесь должен быть реальный POST-запрос к API
		// await register({ email, username, password });
		// Пример имитации:
		await api.post("/auth/register", {
		email, username, password, confirmPassword: confirm
		});
		setSuccess("Вы успешно зарегистрированы! Теперь вы можете войти.");
		setEmail("");
		setUsername("");
		setPassword("");
		setConfirm("");
	} catch (err) {
		setError("Ошибка регистрации. Проверьте данные и попробуйте снова.");
	}
	setLoading(false);
	}

	return (
	<form onSubmit={handleRegister} autoComplete="off">
		<div className="form-floating mb-3">
		<input
			type="email"
			className="form-control"
			id="registerEmail"
			placeholder="Email"
			value={email}
			onChange={e => setEmail(e.target.value)}
			required
		/>
		<label htmlFor="registerEmail">Email</label>
		</div>
		<div className="form-floating mb-3">
		<input
			type="text"
			className="form-control"
			id="registerUsername"
			placeholder="Имя пользователя"
			value={username}
			onChange={e => setUsername(e.target.value)}
			required
		/>
		<label htmlFor="registerUsername">Имя пользователя</label>
		</div>
		<div className="form-floating mb-3">
		<input
			type="password"
			className="form-control"
			id="registerPassword"
			placeholder="Пароль"
			value={password}
			onChange={e => setPassword(e.target.value)}
			required
		/>
		<label htmlFor="registerPassword">Пароль</label>
		</div>
		<div className="form-floating mb-3">
		<input
			type="password"
			className="form-control"
			id="registerConfirm"
			placeholder="Повторите пароль"
			value={confirm}
			onChange={e => setConfirm(e.target.value)}
			required
		/>
		<label htmlFor="registerConfirm">Повторите пароль</label>
		</div>
		{error && <div className="alert alert-danger py-2">{error}</div>}
		{success && <div className="alert alert-success py-2">{success}</div>}
		<button className="btn btn-primary w-100" disabled={loading}>
		{loading ? (
			<span className="spinner-border spinner-border-sm me-2" />
		) : null}
		Зарегистрироваться
		</button>
	</form>
	);
}
