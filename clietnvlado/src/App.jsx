import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import AuthPage from './AuthPage';
import Dashboard from "./Dashboard";

function App() {
  const [token, setToken] = useState(() => sessionStorage.getItem("token"));

  const handleLogin = (token) => {
    setToken(token);
    sessionStorage.setItem("token", token);
  };

  const handleLogout = () => {
    setToken(null);
    sessionStorage.removeItem("token");
  };

  return token ? (
    <Dashboard onLogout={handleLogout} />
  ) : (
    <AuthPage onLogin={handleLogin} />
  );
}

export default App;