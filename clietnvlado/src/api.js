import axios from "axios";

// Можно указать полный адрес API, если ты не используешь прокси в vite.config.js
const api = axios.create({
  baseURL: "/api", // если нужен полный адрес: http://localhost:5223/api
});

// Перехватчик: подставляем токен ко всем запросам
api.interceptors.request.use((config) => {
  // Можно выбирать между localStorage/sessionStorage — смотри, где ты хранишь токен
  const token = localStorage.getItem("token") || sessionStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
