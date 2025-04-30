// src/components/Footer.jsx
import React, { useState, useEffect } from "react"; // Add useEffect
import { useAuth } from "../contexts/AuthContext";
import {
	subscribeNewsletter,
	unsubscribeNewsletter,
	getNewsletterStatus, // Import the status check function
	getApiErrorMessage,
} from "../services/api";

const Footer = () => {
	const { isAuthenticated } = useAuth(); // Only need isAuthenticated from context now
	// Local state for the button's display and API calls
	const [isSubscribedView, setIsSubscribedView] = useState(false); // Local view state
	const [isLoadingStatus, setIsLoadingStatus] = useState(false); // Loading status fetch
	const [isLoadingToggle, setIsLoadingToggle] = useState(false); // Loading toggle action
	const [subError, setSubError] = useState(null);
	const [subMessage, setSubMessage] = useState(null);

	// --- Effect to fetch status on mount/auth change ---
	useEffect(() => {
		// Only fetch if user is authenticated
		if (isAuthenticated) {
			console.log(
				"Footer: User is authenticated, fetching newsletter status...",
			);
			setIsLoadingStatus(true); // Indicate loading status
			setSubError(null); // Clear previous errors
			getNewsletterStatus()
				.then((response) => {
					console.log("Footer: Received status:", response.data.isSubscribed);
					setIsSubscribedView(response.data.isSubscribed); // Update local view state
				})
				.catch((err) => {
					console.error("Footer: Error fetching subscription status:", err);
					setSubError("Could not fetch subscription status.");
					// Keep default view state (false) or handle error differently
				})
				.finally(() => {
					setIsLoadingStatus(false); // Done loading status
				});
		} else {
			// If user logs out, reset the view state
			setIsSubscribedView(false);
			setSubError(null);
			setSubMessage(null);
		}
		// Rerun this effect if the isAuthenticated status changes (user logs in/out)
	}, [isAuthenticated]);

	const handleNewsletterToggle = async () => {
		// No need to check isAuthenticated again here, button is only shown if true
		setIsLoadingToggle(true); // Use separate loading state for the action
		setSubError(null);
		setSubMessage(null);

		try {
			// Use the local view state to decide which API to call
			if (isSubscribedView) {
				await unsubscribeNewsletter();
				setSubMessage("Successfully unsubscribed!");
				setIsSubscribedView(false); // Update local view state immediately
			} else {
				await subscribeNewsletter();
				setSubMessage("Successfully subscribed!");
				setIsSubscribedView(true); // Update local view state immediately
			}
		} catch (err) {
			console.error("Newsletter toggle error:", err);
			setSubError(getApiErrorMessage(err));
			// Don't toggle view state if API call failed
		} finally {
			setIsLoadingToggle(false); // Action finished
		}
	};

	// Determine overall loading state for the button
	const isLoading = isLoadingStatus || isLoadingToggle;

	return (
		<footer className="bg-gray-200 text-gray-600 text-xs border-t border-gray-300 mt-8">
			<div className="container mx-auto px-4 py-6 grid grid-cols-1 md:grid-cols-3 gap-4 text-center md:text-left">
				{/* Column 1: About */}
				<div>
					{" "}
					<h4 className="font-semibold mb-2 text-gray-700">About Tourit</h4>{" "}
					<p>
						A forum for tourism discussion. This platform is part of the
						Master's Thesis by Dimitris Vakirtzis for the University of Piraeus,
						Department of Informatics.{" "}
					</p>{" "}
				</div>

				{/* Column 2: Actions */}
				<div>
					<h4 className="font-semibold mb-2 text-gray-700">Connect</h4>
					<a
						href="mailto:jimakiad@gmail.com?subject=Tourit%20Contact"
						className="block hover:text-blue-600 hover:underline mb-2"
					>
						{" "}
						Contact Us{" "}
					</a>

					{/* Newsletter Toggle - Logic uses isAuthenticated and local view state */}
					{isAuthenticated && (
						<div>
							<button
								type="button"
								onClick={handleNewsletterToggle}
								disabled={isLoading} // Disable if fetching status OR toggling
								className={`px-3 py-1 rounded-full text-xs font-medium border ${
									isLoadingStatus
										? "bg-gray-100 border-gray-300 text-gray-400 cursor-wait" // Special loading style
										: isSubscribedView
											? "bg-red-100 border-red-300 text-red-700 hover:bg-red-200"
											: "bg-green-100 border-green-300 text-green-700 hover:bg-green-200"
								} disabled:opacity-70`} // Slightly less faded when disabled due to loading
							>
								{isLoadingStatus
									? "Checking Status..."
									: isLoadingToggle
										? "Updating..."
										: isSubscribedView
											? "Unsubscribe Newsletter"
											: "Subscribe Newsletter"}
							</button>
							{/* Only show messages when *not* loading status */}
							{!isLoadingStatus && subMessage && (
								<p className="text-green-600 mt-1 text-xs">{subMessage}</p>
							)}
							{!isLoadingStatus && subError && (
								<p className="text-red-600 mt-1 text-xs">{subError}</p>
							)}
						</div>
					)}
					{!isAuthenticated && (
						<p className="text-gray-500 mt-2">Log in to manage newsletter.</p>
					)}
				</div>

				{/* Column 3: Copyright */}
				<div className="md:text-right">
					{" "}
					<h4 className="font-semibold mb-2 text-gray-700">Tourit Forum</h4>{" "}
					<p>
						© {new Date().getFullYear()} Dimitris Vakirtzis. All rights
						reserved.
					</p>{" "}
				</div>
			</div>
		</footer>
	);
};

export default Footer;
