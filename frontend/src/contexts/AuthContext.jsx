// src/contexts/AuthContext.jsx
import React, { createContext, useEffect, useState } from "react";
import api from "../api/axios";

export const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [token, setToken] = useState(localStorage.getItem("token") || null);
  const [user, setUser] = useState(() => {
    const raw = localStorage.getItem("user");
    return raw ? JSON.parse(raw) : null;
  });

  useEffect(() => {
    if (token) {
      localStorage.setItem("token", token);
    } else {
      localStorage.removeItem("token");
    }
  }, [token]);

  useEffect(() => {
    if (user) localStorage.setItem("user", JSON.stringify(user));
    else localStorage.removeItem("user");
  }, [user]);

  const login = async (email, password) => {
    const res = await api.post("/api/auth/login", { Email: email, Password: password });
    const { token: jwt, userId, roles } = res.data;
    setToken(jwt);
    setUser({ id: userId, roles });
    return res.data;
  };

  const register = async (email, password) => {
    const res = await api.post("/api/auth/register", { Email: email, Password: password });
    const { token: jwt, userId, roles } = res.data;
    setToken(jwt);
    setUser({ id: userId, roles });
    return res.data;
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    localStorage.removeItem("token");
    localStorage.removeItem("user");
  };

  return (
    <AuthContext.Provider value={{ token, user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
