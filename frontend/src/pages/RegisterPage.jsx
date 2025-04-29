import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { register, getApiErrorMessage } from '../services/api';

const RegisterPage = () => {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null); setSuccessMessage(null);
    if (password !== confirmPassword) {
      setError("Passwords do not match."); return;
    }
    setIsLoading(true);
    try {
      const response = await register({ username, email, password });
      setSuccessMessage(response.data.message || "Registration successful! Redirecting to login...");
      setTimeout(() => navigate('/login'), 2000);
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  };

  return (
     <div className="flex justify-center items-start pt-16 min-h-screen">
        <div className="w-full max-w-xs p-6 bg-white rounded shadow-md border border-gray-200">
             <h2 className="text-xl font-semibold mb-6 text-center text-gray-700">Sign Up</h2>
            <form onSubmit={handleSubmit}>
            {error && <p className="bg-red-100 text-red-600 p-2 rounded mb-4 text-xs text-center">{error}</p>}
            {successMessage && <p className="bg-green-100 text-green-600 p-2 rounded mb-4 text-xs text-center">{successMessage}</p>}

            <div className="mb-4">
                <input type="text" placeholder="Username" value={username} onChange={(e) => setUsername(e.target.value)} required className="border rounded w-full py-2 px-3 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400"/>
            </div>
            <div className="mb-4">
                <input type="email" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} required className="border rounded w-full py-2 px-3 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400"/>
            </div>
            <div className="mb-4">
                <input type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} className="border rounded w-full py-2 px-3 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400"/>
            </div>
            <div className="mb-6">
                <input type="password" placeholder="Confirm Password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} required className="border rounded w-full py-2 px-3 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400"/>
            </div>

            <div className="flex flex-col items-center space-y-3">
                <button type="submit" disabled={isLoading || successMessage} className="w-full bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded-full text-sm focus:outline-none focus:shadow-outline disabled:opacity-50">
                {isLoading ? 'Registering...' : 'Sign Up'}
                </button>
                <Link to="/login" className="text-xs text-blue-500 hover:underline">
                Already have an account? Log In
                </Link>
            </div>
            </form>
        </div>
    </div>
  );
};
export default RegisterPage;