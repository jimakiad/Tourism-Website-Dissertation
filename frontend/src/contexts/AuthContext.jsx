// src/contexts/AuthContext.jsx
import React, { createContext, useState, useContext, useEffect, useMemo, useCallback } from 'react';

// 1. Create the Context Object
// This object will hold the shared authentication state and functions.
// Initial value is null, but the Provider will always supply a value.
const AuthContext = createContext(null);

// 2. Create the Provider Component
// This component will wrap parts (or all) of your app.
// It manages the actual state and provides it to the context.
export const AuthProvider = ({ children }) => {
  // State to hold the JWT token. Initialize from localStorage.
  const [token, setToken] = useState(localStorage.getItem('token'));
  // State to indicate if we're still checking localStorage initially.
  const [isLoading, setIsLoading] = useState(true);

  // useEffect runs once when the AuthProvider mounts.
  useEffect(() => {
    const storedToken = localStorage.getItem('token'); // Check storage
    setToken(storedToken); // Set the token state
    setIsLoading(false); // Mark initial check as complete
    console.log("Auth Check Complete. Token:", storedToken ? "Found" : "None");
    // We don't decode the token here in this simplified version.
  }, []); // Empty dependency array means run only on mount

  // login function: Updates state and localStorage
  // useCallback ensures the function identity is stable unless dependencies change
  const login = useCallback((newToken) => {
    if (newToken) {
      localStorage.setItem('token', newToken); // Save to storage
      setToken(newToken); // Update state
      console.log("Logged in, token set.");
    } else {
      console.error("Login function called with no token.");
    }
  }, []); // No dependencies, function is stable

  // logout function: Clears state and localStorage
  const logout = useCallback(() => {
    localStorage.removeItem('token'); // Remove from storage
    setToken(null); // Clear state
    console.log("Logged out, token cleared.");
  }, []); // No dependencies, function is stable

  // Create the value object to be passed down via context.
  // useMemo prevents this object from being recreated on every render
  // unless token, login, logout, or isLoading changes.
  const value = useMemo(() => ({
    token, // The current token string (or null)
    isAuthenticated: !!token, // Simple boolean: true if token exists, false otherwise
    login, // The login function
    logout, // The logout function
    isLoading // Flag indicating initial token check status
  }), [token, login, logout, isLoading]); // Dependencies for memoization

  // Render the Context Provider, passing the calculated 'value'.
  // Any component inside {children} can now access this value using useAuth().
  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

// 3. Create a Custom Hook for easy context consumption
// This simplifies accessing the context value in components.
export const useAuth = () => {
  const context = useContext(AuthContext); // Get the value from the nearest AuthProvider
  // If context is null, it means useAuth was called outside of an AuthProvider
  if (context === null) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  // Return the context value (containing token, isAuthenticated, login, logout, etc.)
  return context;
};