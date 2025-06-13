import axios from "axios";

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

export async function uploadDocument({ roomId, file, parentDocId, note }) {
  const formData = new FormData();
  formData.append("file", file);
  if (parentDocId) formData.append("parentDocId", parentDocId);
  if (note) formData.append("note", note);

  const endpoint = roomId ? `/rooms/${roomId}/docs` : `/documents`;

  return api.post(endpoint, formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
}

export default api;
