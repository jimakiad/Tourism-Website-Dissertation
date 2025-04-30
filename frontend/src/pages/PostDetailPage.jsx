import React, { useState, useEffect, useCallback } from "react";
import { useParams, Link } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import {
	getPostById,
	getCommentsForPost,
	getApiErrorMessage,
} from "../services/api";
import VoteButtons from "../components/VoteButtons";
import Comment from "../components/Comment"; 
import CreateCommentForm from "../components/CreateCommentForm"; 
import { RMap, ROSM, RLayerVector, RFeature } from "rlayers"; 
import { Point } from "ol/geom";
import { fromLonLat } from "ol/proj";
import "ol/ol.css";

const formatDate = (dateString) => {

};
const IMAGE_BASE_URL = "http://localhost:5019";
const previewContainerStyle = { height: "250px", width: "100%" };

const PostDetailPage = () => {
	const { postId } = useParams();
	const [post, setPost] = useState(null);
	const [comments, setComments] = useState([]);
	const [isLoadingPost, setIsLoadingPost] = useState(true);
	const [isLoadingComments, setIsLoadingComments] = useState(true);
	const [postError, setPostError] = useState(null);
	const [commentsError, setCommentsError] = useState(null);
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

	const fetchComments = useCallback(async () => {
		if (!postId) return;
		setIsLoadingComments(true);
		setCommentsError(null);
		try {
			const response = await getCommentsForPost(postId);
			setComments(response.data); 
		} catch (err) {
			setCommentsError(getApiErrorMessage(err));
		} finally {
			setIsLoadingComments(false);
		}
	}, [postId]);
	useEffect(() => {
		fetchPost();
		fetchComments();
	}, [fetchPost, fetchComments]);
	const handleCommentCreated = useCallback(
		(newComment) => {
			console.log("New comment created, refetching comments...", newComment);
			fetchComments(); 
		},
		[fetchComments],
	);
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
	const hasLocation = post.latitude != null && post.longitude != null;
	const mapPositionCoords = hasLocation
		? fromLonLat([post.longitude, post.latitude])
		: null;
	const fullImageUrl = post.imageUrl
		? `${IMAGE_BASE_URL}${post.imageUrl}`
		: null;

	return (
		<div className="bg-white rounded border border-gray-300 shadow-sm my-4 overflow-hidden">
			{fullImageUrl && (
				<img
					src={fullImageUrl}
					alt={post.title}
					className="w-full max-h-[70vh] object-contain bg-gray-100"
				/>
			)}

			<div className="flex p-0">
				{" "}
				<div className="pt-3 pl-1">
					{" "}
					<VoteButtons postId={post.id} initialScore={post.score} />
				</div>
				<div className="p-4 flex-grow overflow-hidden">
					<div className="text-xs text-gray-500 mb-2 flex flex-wrap items-center space-x-2">
						<span className="font-medium text-blue-700 hover:underline cursor-pointer">
							{" "}
							t/{post.countryName || "unknown"}{" "}
						</span>
						<span className="text-gray-300">•</span>
						<span>Posted by u/{post.authorUsername || "anonymous"}</span>
						<span>{formatDate(post.createdAt)}</span>
					</div>
					<h1 className="text-xl sm:text-2xl font-semibold text-gray-800 mb-3 break-words">
						{post.title}
					</h1>
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
					<div className="prose prose-sm max-w-none text-gray-800">
						<ReactMarkdown>{post.body}</ReactMarkdown>
					</div>
				</div>
			</div>
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
			<div className="border-t border-gray-300 mt-4 p-4">
				<h3 className="text-base font-semibold mb-3 text-gray-700">
					Comments ({isLoadingComments ? "..." : comments.length})
				</h3>
				<div className="mb-6">
					<CreateCommentForm
						postId={Number(postId)} 
						onCommentCreated={handleCommentCreated}
					/>
				</div>
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
								onCommentCreated={handleCommentCreated}
							/>
						))}
					</div>
				)}
			</div>
		</div>
	);
};

export default PostDetailPage;
