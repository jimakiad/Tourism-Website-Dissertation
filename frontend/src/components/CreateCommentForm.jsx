// src/components/CreateCommentForm.jsx
import React, { useState } from "react";
import { createComment, getApiErrorMessage } from "../services/api";
import { useAuth } from "../contexts/AuthContext";

const CreateCommentForm = ({
	postId,
	parentCommentId = null,
	onCommentCreated,
	onCancel = null,
}) => {
	const [body, setBody] = useState("");
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState(null);
	const { isAuthenticated } = useAuth();

	const handleSubmit = async (e) => {
		e.preventDefault();
		if (!body.trim()) return;
		setIsLoading(true);
		setError(null);
		try {
			const commentData = { body, parentCommentId };
			const response = await createComment(postId, commentData);
			setBody("");
			if (onCommentCreated) {
				onCommentCreated(response.data);
			}
		} catch (err) {
			setError(getApiErrorMessage(err));
		} finally {
			setIsLoading(false);
		}
	};

	if (!isAuthenticated) {
		return (
			<p className="text-xs text-gray-500 p-2 border rounded bg-gray-50">
				Log in to comment.
			</p>
		);
	}

	return (
		<form onSubmit={handleSubmit} className="mt-2 text-sm">
			{error && <p className="text-red-500 text-xs mb-2">{error}</p>}
			<textarea
				value={body}
				onChange={(e) => setBody(e.target.value)}
				placeholder={parentCommentId ? "Write a reply..." : "Add a comment..."}
				required
				rows={parentCommentId ? 2 : 3}
				className="border rounded w-full py-1 px-2 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400 mb-2"
			/>
			<div className="flex justify-end space-x-2">
				{onCancel && (
					<button
						type="button"
						onClick={onCancel}
						className="bg-gray-200 hover:bg-gray-300 text-gray-700 text-xs font-medium py-1 px-3 rounded-full"
					>
						Cancel
					</button>
				)}
				<button
					type="submit"
					disabled={isLoading || !body.trim()}
					className="bg-blue-500 hover:bg-blue-600 text-white text-xs font-medium py-1 px-3 rounded-full disabled:opacity-50"
				>
					{isLoading ? "Submitting..." : parentCommentId ? "Reply" : "Comment"}
				</button>
			</div>
		</form>
	);
};
export default CreateCommentForm;
