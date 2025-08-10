// src/api/axios.js
import axios from "axios";

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "https://localhost:7283";

const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: false,
});

api.interceptors.request.use(config => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers = config.headers || {};
    config.headers.Authorization = token;
  }
  return config;
});

export default api;