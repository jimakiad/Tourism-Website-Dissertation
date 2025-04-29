import React, { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { login, getApiErrorMessage } from '../services/api';

const LoginPage = () => {
  const [usernameOrEmail, setUsernameOrEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const { login: authLogin } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from?.pathname || "/";

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    try {
      const response = await login({ usernameOrEmail, password });
      authLogin(response.data.token);
      navigate(from, { replace: true });
    } catch (err) {
      setError(getApiErrorMessage(err));
      setIsLoading(false);
    }
  };

  return (
     <div className="flex justify-center items-start pt-16 min-h-screen">
        <div className="w-full max-w-xs p-6 bg-white rounded shadow-md border border-gray-200">
            <h2 className="text-xl font-semibold mb-6 text-center text-gray-700">Log In</h2>
            <form onSubmit={handleSubmit}>
            {error && <p className="bg-red-100 text-red-600 p-2 rounded mb-4 text-xs text-center">{error}</p>}
            <div className="mb-4">
                <input
                type="text"
                placeholder="Username or Email"
                value={usernameOrEmail}
                onChange={(e) => setUsernameOrEmail(e.target.value)}
                required
                className="border rounded w-full py-2 px-3 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400"
                />
            </div>
            <div className="mb-6">
                <input
                type="password"
                placeholder="Password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="border rounded w-full py-2 px-3 text-gray-700 text-sm mb-3 focus:outline-none focus:ring-1 focus:ring-blue-400"
                />
            </div>
            <div className="flex flex-col items-center space-y-3">
                <button
                type="submit"
                disabled={isLoading}
                className="w-full bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded-full text-sm focus:outline-none focus:shadow-outline disabled:opacity-50"
                >
                {isLoading ? 'Logging in...' : 'Log In'}
                </button>
                <Link to="/register" className="text-xs text-blue-500 hover:underline">
                Don't have an account? Sign Up
                </Link>
            </div>
            </form>
        </div>
     </div>
  );
};
export default LoginPage;