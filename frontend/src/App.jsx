// src/App.jsx
import React from "react";
import {
	BrowserRouter as Router,
	Routes,
	Route,
	Navigate,
} from "react-router-dom";
import { AuthProvider } from "./contexts/AuthContext";
import Navbar from "./components/Navbar";
import ProtectedRoute from "./components/ProtectedRoute";
import CountryPage from "./pages/CountryPage";

// Import Page Components
import HomePage from "./pages/HomePage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import CreatePostPage from "./pages/CreatePostPage";
import PostDetailPage from "./pages/PostDetailPage";
import Footer from "./components/Footer";

function App() {
	return (
		<AuthProvider>
			<Router>
				<div className="flex flex-col min-h-screen bg-gray-100">
					<Navbar />
					<main className="flex-grow w-full max-w-5xl mx-auto pt-4 px-2 sm:px-4">
						<Routes>
							{/* Public Routes */}
							<Route path="/" element={<HomePage />} />
							<Route path="/login" element={<LoginPage />} />
							<Route path="/register" element={<RegisterPage />} />
							<Route path="/posts/:postId" element={<PostDetailPage />} />
							<Route path="/country/:countryCode" element={<CountryPage />} />

							{/* Protected Routes */}
							<Route
								path="/create-post"
								element={
									<ProtectedRoute>
										<CreatePostPage />
									</ProtectedRoute>
								}
							/>

							{/* Catch-all */}
							<Route path="*" element={<Navigate to="/" replace />} />
						</Routes>
					</main>
					<Footer />
				</div>
			</Router>
		</AuthProvider>
	);
}
export default App;
