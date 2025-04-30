// src/components/Comment.jsx
import React, { useState } from "react";
import ReactMarkdown from "react-markdown";
import CommentVoteButtons from "./CommentVoteButtons";
import CreateCommentForm from "./CreateCommentForm"; // Create this next

const formatDate = (dateString) => {
	if (!dateString) return "";
	const options = {
		year: "numeric",
		month: "short",
		day: "numeric",
		hour: "2-digit",
		minute: "2-digit",
	};
	try {
		return new Date(dateString).toLocaleDateString(undefined, options);
	} catch (e) {
		return dateString;
	}
};

const Comment = ({ comment, postId, onCommentCreated }) => {
	const [showReplyForm, setShowReplyForm] = useState(false);

	// When a reply is successfully created *for this comment*
	const handleReplyCreated = (newReply) => {
		setShowReplyForm(false); // Hide form after reply
		onCommentCreated(newReply); // Notify parent page to potentially update list
	};

	return (
		<div className="ml-0 py-2 border-b border-gray-200 last:border-b-0">
			{" "}
			{/* Top level comment */}
			{/* Comment Metadata */}
			<div className="text-xs text-gray-500 mb-1 flex items-center">
				<CommentVoteButtons
					commentId={comment.id}
					initialScore={comment.score}
				/>
				<span>u/{comment.authorUsername || "anonymous"}</span>
				<span className="mx-1">•</span>
				<time dateTime={comment.createdAt}>
					{formatDate(comment.createdAt)}
				</time>
			</div>
			{/* Comment Body */}
			<div className="text-sm text-gray-800 mb-2 prose prose-sm max-w-none">
				<ReactMarkdown>{comment.body}</ReactMarkdown>
			</div>
			{/* Action Buttons (Reply) */}
			<div className="text-xs">
				<button
					type="button"
					onClick={() => setShowReplyForm(!showReplyForm)}
					className="text-gray-500 hover:text-gray-800 font-medium"
				>
					Reply
				</button>
				{/* Add Edit/Delete later */}
			</div>
			{/* Reply Form (Conditional) */}
			{showReplyForm && (
				<div className="ml-4 mt-2">
					<CreateCommentForm
						postId={postId}
						parentCommentId={comment.id} // Pass current comment ID as parent
						onCommentCreated={handleReplyCreated}
						onCancel={() => setShowReplyForm(false)}
					/>
				</div>
			)}
			{/* Render Replies Recursively */}
			{comment.replies && comment.replies.length > 0 && (
				<div className="ml-4 pl-4 border-l-2 border-gray-200 mt-2">
					{comment.replies.map((reply) => (
						<Comment
							key={reply.id}
							comment={reply}
							postId={postId}
							onCommentCreated={onCommentCreated} // Pass down handler
						/>
					))}
				</div>
			)}
		</div>
	);
};

export default Comment;
