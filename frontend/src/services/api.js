import axios from "axios";

const API_BASE_URL = "http://localhost:5019/api";

const apiClient = axios.create({
	baseURL: API_BASE_URL,
	headers: {
		"Content-Type": "application/json",
	},
});

// Axios Request Interceptor: This function runs BEFORE every request is sent.
apiClient.interceptors.request.use(
	(config) => {
		// 1. Get the JWT token from browser's local storage
		const token = localStorage.getItem("token");
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
	},
);

// --- Define functions for specific API endpoints ---

export const login = (credentials) =>
	apiClient.post("/auth/login", credentials);

export const register = (userData) =>
	apiClient.post("/auth/register", userData);

export const getPosts = (sortBy = "new", limit = 25, countryCode = null) => {
	let url = `/posts?sortBy=${sortBy}&limit=${limit}`; // Check casing /posts or /Posts
	if (countryCode) {
		url += `&countryCode=${encodeURIComponent(countryCode)}`; // Add countryCode if provided
	}
	return apiClient.get(url);
};

export const getCountryByCode = (code) => apiClient.get(`/countries/code/${code}`);

export const getPostById = (postId) => apiClient.get(`/posts/${postId}`);

export const createPost = (postData) => apiClient.post("/Posts", postData);

export const votePost = (postId, direction) =>
	apiClient.post(`/Posts/${postId}/vote`, { direction });

export const getCountries = () => apiClient.get("/Countries");

export const getCategories = () => apiClient.get("/categories");

export const getTags = () => apiClient.get("/tags");

export const getCommentsForPost = (postId) =>
	apiClient.get(`/posts/${postId}/comments`);

export const createComment = (postId, commentData) =>
	apiClient.post(`/posts/${postId}/comments`, commentData);

export const voteComment = (commentId, direction) =>
	apiClient.post(`/comments/${commentId}/vote`, { direction });

export const subscribeNewsletter = () =>
	apiClient.post("/newsletter/subscribe");
export const unsubscribeNewsletter = () =>
	apiClient.post("/newsletter/unsubscribe");
export const getNewsletterStatus = () => apiClient.get("/newsletter/status");

export const uploadPostImage = (postId, formData) => {
	return apiClient.post(`/posts/${postId}/image`, formData, {
		headers: {
			"Content-Type": "multipart/form-data",
		},
	});
};

// Simple Error Message Extractor

export const getApiErrorMessage = (error) => {
	// Check if the error object has response data from the backend
	if (error?.response?.data) {
		if (error.response.data.errors) {
			const messages = Object.values(error.response.data.errors).flat();
			return messages.join(" ");
		}

		if (error.response.data.message) return error.response.data.message;

		if (typeof error.response.data === "string") return error.response.data;
	}

	if (error?.response?.statusText) {
		return `Server error: ${error.response.status} ${error.response.statusText}`;
	}

	if (error?.request) return "Network error or backend not running.";

	return error?.message || "An unexpected error occurred.";
};

export default apiClient;
