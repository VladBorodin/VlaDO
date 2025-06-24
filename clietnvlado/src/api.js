import axios from "axios";

const api = axios.create({
  baseURL: "/api",
});

api.interceptors.request.use(config => {
  const token =
    localStorage.getItem("token") || sessionStorage.getItem("token");

  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      sessionStorage.clear();
      localStorage.removeItem("token");
      window.location.replace("/login?expired=1");
    }
    return Promise.reject(error);
  }
);

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