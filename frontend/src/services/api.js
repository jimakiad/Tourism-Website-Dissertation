// src/services/api.js
import axios from 'axios'; // Import the Axios library for making HTTP requests

// Define the base URL for your backend API.
// CRITICAL: Use the HTTPS URL and port shown when you run `dotnet run` for your backend.
const API_BASE_URL = 'http://localhost:5019/api'; // <<< --- ADJUST PORT IF NEEDED (e.g., 7001, 7250, etc.)

// Create a pre-configured instance of Axios.
// Components will use this instance.
const apiClient = axios.create({
  baseURL: API_BASE_URL, // All requests will be prefixed with this URL
  headers: {
    'Content-Type': 'application/json', // Default header for sending JSON data
  }
});

// Axios Request Interceptor: This function runs BEFORE every request is sent.
apiClient.interceptors.request.use(
  (config) => {
    // 1. Get the JWT token from browser's local storage
    const token = localStorage.getItem('token');
    // 2. If a token exists...
    if (token) {
      // 3. ...add it to the 'Authorization' header for the request.
      // The backend will use this header to verify the user.
      config.headers.Authorization = `Bearer ${token}`;
    }
    // 4. Return the modified config object for Axios to use.
    return config;
  },
  (error) => {
    // If there's an error setting up the request, reject the promise.
    return Promise.reject(error);
  }
);

// --- Define functions for specific API endpoints ---

// Auth Endpoints
// Function to call the backend's login endpoint
export const login = (credentials) => apiClient.post('/auth/login', credentials);
// Function to call the backend's register endpoint
export const register = (userData) => apiClient.post('/auth/register', userData);

// Post Endpoints
// Function to get posts (allows sorting and limiting)
export const getPosts = (sortBy = 'new', limit = 25) =>
  apiClient.get(`/Posts?sortBy=${sortBy}&limit=${limit}`);
// Function to create a new post (requires authentication via interceptor)
export const createPost = (postData) => apiClient.post('/Posts', postData);
// Function to vote on a post (requires authentication)
export const votePost = (postId, direction) =>
  apiClient.post(`/Posts/${postId}/vote`, { direction }); // Send direction in request body

// Other Endpoints
// Function to get the list of countries
export const getCountries = () => apiClient.get('/Countries');

// Simple Error Message Extractor
// Tries to find a user-friendly error message from the backend response.
export const getApiErrorMessage = (error) => {
    // Check if the error object has response data from the backend
    if (error?.response?.data) {
        // Check specifically for ASP.NET Core validation errors format
        if (error.response.data.errors) {
           const messages = Object.values(error.response.data.errors).flat(); // Combine all error messages
           return messages.join(' '); // Join them into a single string
        }
        // Check for a simple 'message' property in the response data
        if (error.response.data.message) return error.response.data.message;
        // If the response data itself is just a string, return that
        if (typeof error.response.data === 'string') return error.response.data;
    }
    // If no specific data, use the HTTP status text (e.g., "Not Found", "Unauthorized")
    if (error?.response?.statusText) {
         return `Server error: ${error.response.status} ${error.response.statusText}`;
    }
    // If the request was made but no response received (network error, backend down)
    if (error?.request) return 'Network error or backend not running.';
    // Otherwise, return the general error message or a default fallback
    return error?.message || 'An unexpected error occurred.';
};

// Export the configured apiClient instance (optional, if needed directly elsewhere)
export default apiClient;