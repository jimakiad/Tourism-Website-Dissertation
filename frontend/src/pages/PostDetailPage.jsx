// src/pages/PostDetailPage.jsx
import React, { useState, useEffect, useCallback } from "react";
import { useParams, Link } from "react-router-dom"; // useParams to get postId from URL
import ReactMarkdown from "react-markdown";
import {
	getPostById,
	getCommentsForPost,
	getApiErrorMessage,
} from "../services/api";
import VoteButtons from "../components/VoteButtons";
import Comment from "../components/Comment"; // Import Comment component
import CreateCommentForm from "../components/CreateCommentForm"; // Import Form
import { RMap, ROSM, RLayerVector, RFeature } from "rlayers"; // OpenLayers for preview
import { Point } from "ol/geom";
import { fromLonLat } from "ol/proj";
import "ol/ol.css";

const formatDate = (dateString) => {
	/* ... same as in PostCard ... */
};
const IMAGE_BASE_URL = "http://localhost:5019"; // Ensure this matches backend
const previewContainerStyle = { height: "250px", width: "100%" }; // Slightly larger preview

const PostDetailPage = () => {
	const { postId } = useParams(); // Get postId from the URL parameter
	const [post, setPost] = useState(null);
	const [comments, setComments] = useState([]);
	const [isLoadingPost, setIsLoadingPost] = useState(true);
	const [isLoadingComments, setIsLoadingComments] = useState(true);
	const [postError, setPostError] = useState(null);
	const [commentsError, setCommentsError] = useState(null);

	// Fetch Post Data
	const fetchPost = useCallback(async () => {
		if (!postId) return;
		setIsLoadingPost(true);
		setPostError(null);
		try {
			const response = await getPostById(postId);
			setPost(response.data);
		} catch (err) {
			setPostError(getApiErrorMessage(err));
		} finally {
			setIsLoadingPost(false);
		}
	}, [postId]);

	// Fetch Comments Data
	const fetchComments = useCallback(async () => {
		if (!postId) return;
		setIsLoadingComments(true);
		setCommentsError(null);
		try {
			// Use the function that reconstructs hierarchy
			const response = await getCommentsForPost(postId);
			setComments(response.data); // Expects root comments with nested replies
		} catch (err) {
			setCommentsError(getApiErrorMessage(err));
		} finally {
			setIsLoadingComments(false);
		}
	}, [postId]);

	// Initial data fetch
	useEffect(() => {
		fetchPost();
		fetchComments();
	}, [fetchPost, fetchComments]); // Depend on the memoized fetch functions

	// Callback for when a new comment/reply is created
	// Refetches all comments to get the updated structure simply
	const handleCommentCreated = useCallback(
		(newComment) => {
			console.log("New comment created, refetching comments...", newComment);
			fetchComments(); // Refetch the whole comment tree
			// More advanced: insert the new comment into the local state directly
		},
		[fetchComments],
	);

	// --- Rendering Logic ---
	if (isLoadingPost) {
		return <p className="text-center text-gray-500 py-10">Loading post...</p>;
	}
	if (postError) {
		return (
			<p className="text-center text-red-500 py-10">
				Error loading post: {postError}
			</p>
		);
	}
	if (!post) {
		return <p className="text-center text-gray-500 py-10">Post not found.</p>;
	}

	// Prep map data if location exists
	const hasLocation = post.latitude != null && post.longitude != null;
	const mapPositionCoords = hasLocation
		? fromLonLat([post.longitude, post.latitude])
		: null;
	const fullImageUrl = post.imageUrl
		? `${IMAGE_BASE_URL}${post.imageUrl}`
		: null;

	return (
		<div className="bg-white rounded border border-gray-300 shadow-sm my-4 overflow-hidden">
			{/* Optional Post Image */}
			{fullImageUrl && (
				<img
					src={fullImageUrl}
					alt={post.title}
					className="w-full max-h-[70vh] object-contain bg-gray-100"
				/>
			)}

			<div className="flex p-0">
				{" "}
				{/* No padding on flex container */}
				{/* Post Vote Buttons */}
				<div className="pt-3 pl-1">
					{" "}
					{/* Add padding here */}
					<VoteButtons postId={post.id} initialScore={post.score} />
				</div>
				{/* Post Content Area */}
				<div className="p-4 flex-grow overflow-hidden">
					{/* Metadata */}
					<div className="text-xs text-gray-500 mb-2 flex flex-wrap items-center space-x-2">
						<span className="font-medium text-blue-700 hover:underline cursor-pointer">
							{" "}
							t/{post.countryName || "unknown"}{" "}
						</span>
						<span className="text-gray-300">•</span>
						<span>Posted by u/{post.authorUsername || "anonymous"}</span>
						<span>{formatDate(post.createdAt)}</span>
					</div>
					{/* Title */}
					<h1 className="text-xl sm:text-2xl font-semibold text-gray-800 mb-3 break-words">
						{post.title}
					</h1>
					{/* Categories & Tags */}
					<div className="mb-4 text-xs flex flex-wrap gap-1">
						{post.categoryNames?.map((cat) => (
							<span
								key={`cat-${cat}`}
								className="bg-blue-100 text-blue-800 px-1.5 py-0.5 rounded"
							>
								{cat}
							</span>
						))}
						{post.tagNames?.map((tag) => (
							<span
								key={`tag-${tag}`}
								className="bg-gray-200 text-gray-700 px-1.5 py-0.5 rounded"
							>
								{tag}
							</span>
						))}
					</div>
					{/* Post Body (Markdown) */}
					<div className="prose prose-sm max-w-none text-gray-800">
						<ReactMarkdown>{post.body}</ReactMarkdown>
					</div>
				</div>
			</div>

			{/* Optional Map Display for Post */}
			{hasLocation && mapPositionCoords && (
				<div
					className="border-t border-gray-200 mt-4"
					style={previewContainerStyle}
				>
					<RMap
						className="h-full w-full"
						initial={{ center: mapPositionCoords, zoom: 10 }}
					>
						<ROSM />
						<RLayerVector>
							{" "}
							<RFeature geometry={new Point(mapPositionCoords)} />{" "}
						</RLayerVector>
					</RMap>
				</div>
			)}

			{/* Comment Section */}
			<div className="border-t border-gray-300 mt-4 p-4">
				<h3 className="text-base font-semibold mb-3 text-gray-700">
					Comments ({isLoadingComments ? "..." : comments.length})
				</h3>

				{/* Form to create top-level comment */}
				<div className="mb-6">
					<CreateCommentForm
						postId={Number(postId)} // Ensure postId is number
						onCommentCreated={handleCommentCreated}
					/>
				</div>

				{/* Display Comments */}
				{isLoadingComments && (
					<p className="text-sm text-gray-500">Loading comments...</p>
				)}
				{commentsError && (
					<p className="text-sm text-red-500">
						Error loading comments: {commentsError}
					</p>
				)}
				{!isLoadingComments && !commentsError && comments.length === 0 && (
					<p className="text-sm text-gray-500">No comments yet.</p>
				)}
				{!isLoadingComments && !commentsError && comments.length > 0 && (
					<div className="space-y-3">
						{comments.map((comment) => (
							<Comment
								key={comment.id}
								comment={comment}
								postId={Number(postId)}
								onCommentCreated={handleCommentCreated} // Pass down for replies
							/>
						))}
					</div>
				)}
			</div>
		</div>
	);
};

export default PostDetailPage;
