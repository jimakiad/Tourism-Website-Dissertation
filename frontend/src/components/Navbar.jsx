import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

const Navbar = () => {
	const { isAuthenticated, logout, isLoading } = useAuth();
	const navigate = useNavigate();

	const handleLogout = () => {
		logout();
		navigate("/login");
	};

	return (
		<nav className="bg-gradient-to-r from-blue-500 to-blue-600 text-white p-3 shadow-md sticky top-0 z-10">
			<div className="container mx-auto flex justify-between items-center">
				<Link
					to="/"
					className="text-lg font-semibold hover:opacity-80 tracking-tight flex items-center space-x-2"
				>
					<svg
						xmlns="http://www.w3.org/2000/svg"
						className="h-6 w-6"
						fill="none"
						viewBox="0 0 24 24"
						stroke="currentColor"
						strokeWidth={2}
						aria-label="Navigation marker icon"
					>
						<title>Navigation marker icon</title>
						<path
							strokeLinecap="round"
							strokeLinejoin="round"
							d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8"
						/>
					</svg>
					<span>Tourit</span>
				</Link>
				<div className="space-x-3 text-sm">
					{isLoading ? (
						<span className="text-xs italic">Loading...</span>
					) : isAuthenticated ? (
						<>
							<Link
								to="/profile"
								className="p-1 rounded-full hover:bg-blue-700"
								title="My Profile"
							>
								<svg
									xmlns="http://www.w3.org/2000/svg"
									className="h-5 w-5 inline-block text-white"
									viewBox="0 0 20 20"
									fill="currentColor"
								>
									<title>User profile icon</title>
									<path
										fillRule="evenodd"
										d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z"
										clipRule="evenodd"
									/>
								</svg>
							</Link>
							<Link
								to="/create-post"
								className="bg-white text-blue-600 hover:bg-blue-50 px-3 py-1.5 rounded-full font-medium text-xs shadow-sm"
							>
								Create Post
							</Link>
							<button
								onClick={handleLogout}
								type="button"
								className="border border-white hover:bg-white hover:text-blue-600 px-3 py-1.5 rounded-full font-medium text-xs"
							>
								Logout
							</button>
						</>
					) : (
						<>
							<Link
								to="/login"
								className="border border-white hover:bg-white hover:text-blue-600 px-3 py-1.5 rounded-full font-medium text-xs"
							>
								Log In
							</Link>
							<Link
								to="/register"
								className="bg-white text-blue-600 hover:bg-blue-50 px-3 py-1.5 rounded-full font-medium text-xs shadow-sm"
							>
								Sign Up
							</Link>
						</>
					)}
				</div>
			</div>
		</nav>
	);
};
export default Navbar;
