// src/contexts/AuthContext.jsx
// (Simplified - No jwt-decode needed here anymore for subscription status)
import React, { createContext, useState, useContext, useEffect, useMemo, useCallback } from 'react';
// Removed: import { jwtDecode } from 'jwt-decode';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [token, setToken] = useState(() => localStorage.getItem('token'));
  const [isLoading, setIsLoading] = useState(true);
  // Remove isSubscribed state from here - Footer will manage its own view state
  // const [isSubscribed, setIsSubscribed] = useState(false);

  // Parse ONLY token existence on mount
  useEffect(() => {
    const storedToken = localStorage.getItem('token');
    setToken(storedToken);
    setIsLoading(false);
    console.log("Auth Initial Check Done. Token:", storedToken ? "Found" : "None");
  }, []);

  const login = useCallback((newToken) => {
    if (newToken) {
      localStorage.setItem('token', newToken);
      setToken(newToken); // Just set the token
      console.log("Logged in, token set in storage/state.");
    } else {
      console.error("Login function called with no token.");
    }
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    setToken(null);
    // No need to manage isSubscribed here anymore
    console.log("Logged out, token cleared.");
  }, []);

  // Value provided - NO isSubscribed or setIsSubscribed needed here
  const value = useMemo(() => ({
    token,
    isAuthenticated: !!token,
    login,
    logout,
    isLoading,
  }), [token, login, logout, isLoading]);

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

// Custom Hook remains the same
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === null) throw new Error('useAuth must be used within an AuthProvider');
  return context;
};