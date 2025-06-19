import React, { useState, useEffect } from "react";
import { useAuth } from "../contexts/AuthContext";
import {
	subscribeNewsletter,
	unsubscribeNewsletter,
	getNewsletterStatus,
	getApiErrorMessage,
} from "../services/api";

const Footer = () => {
	const { isAuthenticated } = useAuth();
	const [isSubscribedView, setIsSubscribedView] = useState(false);
	const [isLoadingStatus, setIsLoadingStatus] = useState(false);
	const [isLoadingToggle, setIsLoadingToggle] = useState(false);
	const [subError, setSubError] = useState(null);
	const [subMessage, setSubMessage] = useState(null);

	useEffect(() => {
		if (isAuthenticated) {
			console.log(
				"Footer: User is authenticated, fetching newsletter status...",
			);
			setIsLoadingStatus(true);
			setSubError(null);
			getNewsletterStatus()
				.then((response) => {
					console.log("Footer: Received status:", response.data.isSubscribed);
					setIsSubscribedView(response.data.isSubscribed);
				})
				.catch((err) => {
					console.error("Footer: Error fetching subscription status:", err);
					setSubError("Could not fetch subscription status.");
				})
				.finally(() => {
					setIsLoadingStatus(false);
				});
		} else {
			setIsSubscribedView(false);
			setSubError(null);
			setSubMessage(null);
		}
	}, [isAuthenticated]);

	const handleNewsletterToggle = async () => {
		setIsLoadingToggle(true);
		setSubError(null);
		setSubMessage(null);

		try {
			if (isSubscribedView) {
				await unsubscribeNewsletter();
				setSubMessage("Successfully unsubscribed!");
				setIsSubscribedView(false);
			} else {
				await subscribeNewsletter();
				setSubMessage("Successfully subscribed!");
				setIsSubscribedView(true);
			}
		} catch (err) {
			console.error("Newsletter toggle error:", err);
			setSubError(getApiErrorMessage(err));
		} finally {
			setIsLoadingToggle(false);
		}
	};

	const isLoading = isLoadingStatus || isLoadingToggle;

	return (
		<footer className="bg-gray-200 text-gray-600 text-xs border-t border-gray-300 mt-8">
			<div className="container mx-auto px-4 py-6 grid grid-cols-1 md:grid-cols-3 gap-4 text-center md:text-left">
				<div>
					{" "}
					<h4 className="font-semibold mb-2 text-gray-700">About Tourit</h4>{" "}
					<p>
						A forum for tourism discussion. This platform is part of the
						Master's Thesis by Dimitris Vakirtzis for the University of Piraeus,
						Department of Informatics.{" "}
					</p>{" "}
				</div>

				<div>
					<h4 className="font-semibold mb-2 text-gray-700">Connect</h4>
					<a
						href="mailto:jimakiad@gmail.com?subject=Tourit%20Contact"
						className="block hover:text-blue-600 hover:underline mb-2"
					>
						{" "}
						Contact Us{" "}
					</a>

					{isAuthenticated && (
						<div>
							<button
								type="button"
								onClick={handleNewsletterToggle}
								disabled={isLoading}
								className={`px-3 py-1 rounded-full text-xs font-medium border ${
									isLoadingStatus
										? "bg-gray-100 border-gray-300 text-gray-400 cursor-wait"
										: isSubscribedView
											? "bg-red-100 border-red-300 text-red-700 hover:bg-red-200"
											: "bg-green-100 border-green-300 text-green-700 hover:bg-green-200"
								} disabled:opacity-70`}
							>
								{isLoadingStatus
									? "Checking Status..."
									: isLoadingToggle
										? "Updating..."
										: isSubscribedView
											? "Unsubscribe Newsletter"
											: "Subscribe Newsletter"}
							</button>
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
