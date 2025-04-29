// src/components/PostCard.jsx
import React from "react";
import VoteButtons from "./VoteButtons";
// --- Import OpenLayers / rlayers ---
import { RMap, ROSM, RLayerVector, RFeature } from "rlayers"; // Removed RPopup for preview
import { Point } from "ol/geom";
import { fromLonLat } from "ol/proj";
import "ol/ol.css";
// --- End Imports ---

const previewContainerStyle = { height: "150px", width: "100%" };

const formatDate = (dateString) => {
	if (!dateString) return "";
	const options = { year: "numeric", month: "short", day: "numeric" };
	try {
		return new Date(dateString).toLocaleDateString(undefined, options);
	} catch (e) {
		return dateString;
	}
};

const IMAGE_BASE_URL = "http://localhost:5019";

const PostCard = ({ post }) => {
	const hasLocation = post.latitude != null && post.longitude != null;
	// Convert Lat/Lon from post data to OpenLayers map coordinates
	const mapPositionCoords = hasLocation
		? fromLonLat([post.longitude, post.latitude]) // Lon, Lat order for fromLonLat
		: null;

	const fullImageUrl = post.imageUrl
		? `${IMAGE_BASE_URL}${post.imageUrl}`
		: null;

	return (
		<div className="flex flex-col bg-white shadow-sm rounded border border-gray-200 hover:border-gray-400 mb-3 overflow-hidden">
			{/* --- Image Display --- */}
			{fullImageUrl && (
				<img
					src={fullImageUrl}
					alt={post.title || "Post image"}
					className="w-full h-48 object-cover"
				/>
			)}

			{/* --- Vote Buttons + Text Content --- */}
			<div className="flex">
				<VoteButtons postId={post.id} initialScore={post.score} />
				<div className="p-3 flex-grow overflow-hidden">
					{/* --- Metadata --- */}
					<div className="text-xs text-gray-500 mb-1 flex flex-wrap items-center space-x-2">
						<span className="font-medium text-blue-700 hover:underline cursor-pointer">
							t/{post.countryName || "unknown"}
						</span>
						<span className="text-gray-300">•</span>
						<span>Posted by u/{post.authorUsername || "anonymous"}</span>
						<span>{formatDate(post.createdAt)}</span>
					</div>
					{/* --- Title --- */}
					<h2 className="text-base sm:text-lg font-medium text-gray-800 mb-1 break-words">
						{post.title}
					</h2>
					{/* --- Categories & Tags --- */}
					<div className="mt-2 text-xs flex flex-wrap gap-1">
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
				</div>
			</div>

			{/* --- Optional OpenLayers Map Preview --- */}
			{/* Ensure this uses RMap, not MapContainer */}
			{hasLocation &&
				mapPositionCoords && ( // Check mapPositionCoords as well
					<div
						className="border-t border-gray-200"
						style={previewContainerStyle} // Use defined style
					>
						<RMap
							className="h-full w-full"
							initial={{ center: mapPositionCoords, zoom: 10 }} // Use calculated OL coords
							noDefaultControls={true} // Remove zoom controls etc.
							// Disable all map interactions for preview
							interactions={[]}
						>
							<ROSM /> {/* OpenStreetMap Base Layer */}
							<RLayerVector>
								{/* Feature representing the marker */}
								<RFeature geometry={new Point(mapPositionCoords)}>
									{/* No Popup needed for preview */}
								</RFeature>
							</RLayerVector>
						</RMap>
					</div>
				)}
			{/* --- End Preview --- */}
		</div>
	);
};
export default PostCard;
