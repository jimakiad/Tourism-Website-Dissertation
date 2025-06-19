import React, { createContext, useState, useContext, useEffect, useMemo, useCallback } from 'react';


const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [token, setToken] = useState(() => localStorage.getItem('token'));
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const storedToken = localStorage.getItem('token');
    setToken(storedToken);
    setIsLoading(false);
    console.log("Auth Initial Check Done. Token:", storedToken ? "Found" : "None");
  }, []);

  const login = useCallback((newToken) => {
    if (newToken) {
      localStorage.setItem('token', newToken);
      setToken(newToken); 
      console.log("Logged in, token set in storage/state.");
    } else {
      console.error("Login function called with no token.");
    }
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    setToken(null);

    console.log("Logged out, token cleared.");
  }, []);


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

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === null) throw new Error('useAuth must be used within an AuthProvider');
  return context;
};