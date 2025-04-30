import React, { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import {
	getCurrentUserProfile,
	getCurrentUserPosts,
	getCurrentUserComments,
	deletePost, 
	deleteComment, 
	deleteCurrentUserAccount, 
	getApiErrorMessage,
} from "../services/api";
import PostCard from "../components/PostCard";

const UserComment = ({ comment, onDelete }) => {
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

	const isRedacted = comment.body === "[REMOVED]";

	return (
		<div
			className={`text-xs p-3 border-b last:border-b-0 ${isRedacted ? "bg-gray-100" : "bg-white hover:bg-gray-50"}`}
		>
			<p className="text-gray-600 mb-1">
				{isRedacted ? "Comment Redacted" : `Comment on post ${comment.postId}`}
				<span className="mx-1">•</span> {formatDate(comment.createdAt)}
				{!isRedacted && <span className="mx-1">•</span>}
				{!isRedacted && `Score: ${comment.score}`}
			</p>
			<p
				className={`text-gray-800 mb-1 line-clamp-2 ${isRedacted ? "italic text-gray-500" : ""}`}
			>
				{comment.body}
			</p>
			{!isRedacted && (
				<button
					type="button"
					onClick={() => onDelete(comment.id)}
					className="text-red-500 hover:text-red-700 text-xs font-medium"
				>
					Delete Comment
				</button>
			)}
		</div>
	);
};

const ProfilePage = () => {
	const [profile, setProfile] = useState(null);
	const [posts, setPosts] = useState([]);
	const [comments, setComments] = useState([]);
	const [activeTab, setActiveTab] = useState("posts"); 
	const [isLoading, setIsLoading] = useState(true);
	const [error, setError] = useState(null);
	const [isDeleting, setIsDeleting] = useState(false); 

	const { logout } = useAuth(); 
	const navigate = useNavigate(); 

	const fetchData = useCallback(async () => {
		setIsLoading(true);
		setError(null);
		console.log("Fetching profile data...");
		try {
			const [profileRes, postsRes, commentsRes] = await Promise.all([
				getCurrentUserProfile(),
				getCurrentUserPosts("new", 100), 
				getCurrentUserComments("new", 100),
			]);
			console.log("Profile data received:", profileRes.data);
			console.log("Posts data received:", postsRes.data);
			console.log("Comments data received:", commentsRes.data);
			setProfile(profileRes.data);
			setPosts(postsRes.data);
			setComments(commentsRes.data);
		} catch (err) {
			const errorMsg = getApiErrorMessage(err);
			setError(errorMsg);
			console.error("Profile fetch error:", errorMsg, err);
		} finally {
			setIsLoading(false);
		}
	}, []); 

	useEffect(() => {
		fetchData();
	}, [fetchData]); 

	const handleDeletePost = async (postId) => {
		if (!window.confirm("Are you sure you want to delete (redact) this post?"))
			return;

		setIsDeleting(true);
		setError(null);
		try {
			await deletePost(postId); 
			setPosts((prevPosts) => prevPosts.filter((p) => p.id !== postId)); 
			alert("Post redacted successfully.");
		} catch (err) {
			setError(getApiErrorMessage(err));
			alert(`Failed to redact post: ${getApiErrorMessage(err)}`);
		} finally {
			setIsDeleting(false);
		}
	};

	const handleDeleteComment = async (commentId) => {
		if (
			!window.confirm("Are you sure you want to delete (redact) this comment?")
		)
			return;

		setIsDeleting(true);
		setError(null);
		try {
			await deleteComment(commentId);
			setComments((prevComments) =>
				prevComments.filter((c) => c.id !== commentId),
			);
			alert("Comment redacted successfully.");
		} catch (err) {
			setError(getApiErrorMessage(err));
			alert(`Failed to redact comment: ${getApiErrorMessage(err)}`);
		} finally {
			setIsDeleting(false);
		}
	};

	const handleDeleteAccount = async () => {
		const enteredUsername = window.prompt(
			`This action is irreversible and will deactivate your account and redact your content.\n\nTo confirm, please type your username: ${profile?.username || "(cannot get username)"}`,
		);
		if (enteredUsername !== profile?.username) {
			alert("Username did not match. Account deletion cancelled.");
			return;
		}
		if (
			!window.confirm(
				"FINAL CONFIRMATION: Really deactivate your account? Your username will be changed and content redacted.",
			)
		)
			return;

		setIsDeleting(true);
		setError(null);
		try {
			await deleteCurrentUserAccount(); 
			alert("Account deactivated successfully. You will be logged out.");
			logout();
			navigate("/"); 
		} catch (err) {
			setError(`Account deactivation failed: ${getApiErrorMessage(err)}`);
			setIsDeleting(false); 
			alert(`Account deactivation failed: ${getApiErrorMessage(err)}`);
		}
	};
	if (isLoading)
		return (
			<p className="text-center text-gray-500 py-10">Loading profile...</p>
		);
	if (error && !profile)
		return (
			<p className="text-center text-red-500 py-10">
				Error loading profile: {error}
			</p>
		);
	if (!profile)
		return (
			<p className="text-center text-gray-500 py-10">
				Could not load profile data. Try logging out and back in.
			</p>
		);

	const tabButtonStyle = (tabName) =>
		`px-4 py-2 text-sm font-semibold ${activeTab === tabName ? "border-b-2 border-blue-500 text-blue-600" : "text-gray-500 hover:text-gray-700"}`;

	return (
		<div className="container mx-auto max-w-4xl p-4 pt-6">
			{error && !isLoading && (
				<p className="bg-red-100 text-red-600 p-2 rounded mb-4 text-xs text-center">
					{error}
				</p>
			)}
			<div className="bg-white p-4 rounded border border-gray-300 shadow-sm mb-6 flex justify-between items-start">
				<div>
					<h1 className="text-2xl font-bold text-gray-800">
						u/{profile.username}
					</h1>
					<p className="text-sm text-gray-600">{profile.email}</p>
					<p
						className={`text-xs mt-1 ${profile.isSubscribed ? "text-green-600" : "text-gray-500"}`}
					>
						Newsletter: {profile.isSubscribed ? "Subscribed" : "Not Subscribed"}
					</p>
				</div>
				<div>
					<button
						type="button"
						onClick={() => setActiveTab("settings")}
						disabled={isDeleting}
						className="text-xs text-gray-500 hover:text-red-600 hover:underline focus:outline-none disabled:opacity-50"
					>
						Account Settings
					</button>
				</div>
			</div>
			<div className="flex border-b border-gray-300 mb-4 bg-white px-2 rounded-t">
				<button
					type="button"
					onClick={() => setActiveTab("posts")}
					className={tabButtonStyle("posts")}
				>
					Posts ({posts.length})
				</button>
				<button
					type="button"
					onClick={() => setActiveTab("comments")}
					className={tabButtonStyle("comments")}
				>
					Comments ({comments.length})
				</button>
				<button
					type="button"
					onClick={() => setActiveTab("settings")}
					className={tabButtonStyle("settings")}
				>
					Settings
				</button>
			</div>
			{isDeleting && activeTab !== "settings" && (
				<p className="text-center text-yellow-600 my-4">
					Processing deletion...
				</p>
			)}

			{activeTab === "posts" && (
				<div className="space-y-3">
					{posts.length === 0 && (
						<p className="text-center text-gray-500 py-5 bg-white border rounded shadow-sm">
							No posts found.
						</p>
					)}
					{posts.map((post) => (
						<div key={post.id} className="relative group">
							<PostCard post={post} />
							<button
								type="button"
								onClick={() => handleDeletePost(post.id)}
								disabled={isDeleting}
								className="absolute top-2 right-2 z-10 bg-red-100 hover:bg-red-500 text-red-600 hover:text-white p-1 rounded-full opacity-0 group-hover:opacity-100 focus:opacity-100 transition-opacity disabled:opacity-50"
								title="Delete Post"
							>
								<svg
									xmlns="http://www.w3.org/2000/svg"
									className="h-4 w-4"
									viewBox="0 0 20 20"
									fill="currentColor"
								>
									<title>Delete Post</title>
									<path
										fillRule="evenodd"
										d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z"
										clipRule="evenodd"
									/>
								</svg>
							</button>
						</div>
					))}
				</div>
			)}

			{activeTab === "comments" && (
				<div className="bg-white rounded border border-gray-300 shadow-sm divide-y divide-gray-200">
					{comments.length === 0 && (
						<p className="text-center text-gray-500 p-5">No comments found.</p>
					)}
					{comments.map((comment) => (
						<UserComment
							key={comment.id}
							comment={comment}
							onDelete={handleDeleteComment}
						/>
					))}
				</div>
			)}

			{activeTab === "settings" && (
				<div className="bg-white p-4 rounded border border-gray-300 shadow-sm">
					<h2 className="font-semibold text-lg mb-4">Account Settings</h2>
					<p className="text-sm text-gray-700 mb-4">
						Deactivating your account will change your username, clear your
						email, and redact the content of your posts and comments. This
						action cannot be undone.
					</p>
					<button
						type="button"
						onClick={handleDeleteAccount}
						disabled={isDeleting}
						className="text-sm bg-red-500 hover:bg-red-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline disabled:opacity-50"
					>
						{isDeleting ? "Deactivating..." : "Deactivate My Account"}
					</button>
					{isDeleting && error && (
						<p className="text-red-500 text-xs mt-2">{error}</p>
					)}
				</div>
			)}
		</div>
	);
};

export default ProfilePage;
