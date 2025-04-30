// src/components/CommentVoteButtons.jsx
import React, { useState } from "react";
import { voteComment, getApiErrorMessage } from "../services/api";
import { useAuth } from "../contexts/AuthContext";

const CommentVoteButtons = ({ commentId, initialScore }) => {
	const { isAuthenticated } = useAuth();
	const [score, setScore] = useState(initialScore);
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState(null);

	const handleVote = async (direction) => {
		if (!isAuthenticated || isLoading) return;
		setIsLoading(true);
		setError(null);
		try {
			const response = await voteComment(commentId, direction);
			setScore(response.data.score);
		} catch (err) {
			setError(getApiErrorMessage(err));
		} finally {
			setIsLoading(false);
		}
	};

	const btnStyle =
		"p-0.5 rounded text-gray-400 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed";
	const loadingClass = isLoading ? "!text-gray-300 cursor-wait" : "";

	return (
		<div className="flex items-center space-x-1 mr-2">
			<button
				type="button"
				onClick={() => handleVote(1)}
				disabled={!isAuthenticated || isLoading}
				className={`${btnStyle} ${loadingClass} hover:text-orange-600`}
				aria-label="Like Comment"
			>
				<svg
					xmlns="http://www.w3.org/2000/svg"
					className="h-3.5 w-3.5"
					viewBox="0 0 20 20"
					fill="currentColor"
				>
					<title>Like Comment</title>
					<path
						fillRule="evenodd"
						d="M3.293 9.707a1 1 0 010-1.414l6-6a1 1 0 011.414 0l6 6a1 1 0 01-1.414 1.414L11 5.414V17a1 1 0 11-2 0V5.414L4.707 9.707a1 1 0 01-1.414 0z"
						clipRule="evenodd"
					/>
				</svg>
			</button>
			<span className="font-semibold text-xs text-gray-600 w-4 text-center">
				{score}
			</span>
			<button
				type="button"
				onClick={() => handleVote(-1)}
				disabled={!isAuthenticated || isLoading}
				className={`${btnStyle} ${loadingClass} hover:text-blue-600`}
				aria-label="Dislike Comment"
			>
				<svg
					xmlns="http://www.w3.org/2000/svg"
					className="h-3.5 w-3.5"
					viewBox="0 0 20 20"
					fill="currentColor"
				>
					<title>Dislike Comment</title>
					<path
						fillRule="evenodd"
						d="M16.707 10.293a1 1 0 010 1.414l-6 6a1 1 0 01-1.414 0l-6-6a1 1 0 111.414-1.414L9 14.586V3a1 1 0 012 0v11.586l4.293-4.293a1 1 0 011.414 0z"
						clipRule="evenodd"
					/>
				</svg>
			</button>
			{/* Tiny error display maybe */}
			{/* {error && <span className="text-red-500 text-xs ml-1">!</span>} */}
		</div>
	);
};
export default CommentVoteButtons;
