import React, { createContext, useContext, useState, useEffect } from 'react';
import api from '../services/api';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem('ebs_token') || '');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const savedUser = localStorage.getItem('ebs_user');
    if (savedUser && token) {
      try {
        setUser(JSON.parse(savedUser));
      } catch (e) {
        logout();
      }
    }
    setLoading(false);
  }, [token]);

  const login = async (email, password) => {
    const response = await api.post('/auth/login', { email, password });
    const data = response.data;
    setToken(data.token);
    setUser(data);
    localStorage.setItem('ebs_token', data.token);
    localStorage.setItem('ebs_user', JSON.stringify(data));
    return data;
  };

  const register = async (formData) => {
    const response = await api.post('/auth/register', formData);
    const data = response.data;
    setToken(data.token);
    setUser(data);
    localStorage.setItem('ebs_token', data.token);
    localStorage.setItem('ebs_user', JSON.stringify(data));
    return data;
  };

  const logout = () => {
    setToken('');
    setUser(null);
    localStorage.removeItem('ebs_token');
    localStorage.removeItem('ebs_user');
  };

  return (
    <AuthContext.Provider value={{ user, token, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
