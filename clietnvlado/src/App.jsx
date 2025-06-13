import { Routes, Route, Navigate } from "react-router-dom";
import { useState } from 'react'
import './App.css'
import AuthPage from './AuthPage';
import Dashboard from "./Dashboard";
import NotFound from "./NotFound";
import Forbidden from "./Forbidden";
import ForgotPasswordPage from "./ForgotPasswordPage";
import ResetPassword from "./ResetPassword";
import DocumentsPage from "./DocumentsPage";
import CreateRoomPage from "./CreateRoomPage";
import RoomPage from "./RoomPage";
import CreateDocumentPage from './CreateDocumentPage';

export default function App() {
  const [token, setToken] = useState(() =>
    sessionStorage.getItem("token")
  );

  const handleLogin = (t) => {
    setToken(t);
    sessionStorage.setItem("token", t);
  };
  const handleLogout = () => {
    setToken(null);
    sessionStorage.removeItem("token");
  };

  return (
      <Routes>
        <Route
          path="/login"
          element={
            token
              ? <Navigate to="/" replace />
              : <AuthPage onLogin={handleLogin}/>
          }
        />
        <Route
          path="/"
          element={
            token ? (
              <Dashboard onLogout={handleLogout} />
            ) : (
              <Navigate to="/login" replace />
            )
          }
        />
        <Route
          path="/documents"
          element={
            token
              ? <DocumentsPage onBackToDashboard={() => window.location.href = "/"} />
              : <Navigate to="/login" replace/>
          }
        />
        <Route
          path="/rooms/create"
          element={
            token ? <CreateRoomPage /> : <Navigate to="/login" replace />
          }
        />
        <Route
          path="/rooms/:id"
          element={token ? <RoomPage /> : <Navigate to="/login" replace />}
        />
        <Route
          path="/forgot-password"
          element={token ? <Navigate to="/" replace/> : <ForgotPasswordPage />}
        />
        <Route
          path="/reset-password"
          element={<ResetPassword />}
        />
        <Route path="/documents/create" element={token ? <CreateDocumentPage /> : <Navigate to="/login" />} />
        <Route path="/403" element={<Forbidden />} />
        {/* Любой другой путь */}
        <Route path="*" element={<NotFound />} />
      </Routes>
  );
}