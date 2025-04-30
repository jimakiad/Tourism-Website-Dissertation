// src/components/PostCard.jsx
import React from "react";
import VoteButtons from "./VoteButtons";
import { Link } from "react-router-dom";
import { RMap, ROSM, RLayerVector, RFeature } from "rlayers";
import { Point } from "ol/geom";
import { fromLonLat } from "ol/proj";
import "ol/ol.css";

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
	const mapPositionCoords = hasLocation
		? fromLonLat([post.longitude, post.latitude])
		: null;

	const fullImageUrl = post.imageUrl
		? `${IMAGE_BASE_URL}${post.imageUrl}`
		: null;

	return (
		<div className="flex flex-col bg-white shadow-sm rounded border border-gray-200 hover:border-gray-400 mb-3 overflow-hidden">
			{fullImageUrl && (
				<img
					src={fullImageUrl}
					alt={post.title || "Post image"}
					className="w-full h-48 object-cover"
				/>
			)}
			<div className="flex">
				<VoteButtons postId={post.id} initialScore={post.score} />
				<div className="p-3 flex-grow overflow-hidden">
					<div className="text-xs ... space-x-2">
                        {post.countryCode ? (
                            <Link to={`/country/${post.countryCode}`} className="font-medium text-blue-700 hover:underline cursor-pointer">
                                t/{post.countryName || post.countryCode}
                            </Link>
                        ) : (
                            <span className="font-medium text-gray-500">t/unknown</span>
                        )}
                        <span className="text-gray-300">•</span>
                        <span>Posted by u/{post.authorUsername || 'anonymous'}</span>
                        <span>{formatDate(post.createdAt)}</span>
                    </div>
					<Link to={`/posts/${post.id}`} className="block hover:text-blue-600">
						<h2 className="text-base sm:text-lg font-medium text-gray-800 mb-1 break-words">
							{post.title}
						</h2>
					</Link>
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

			{hasLocation &&
				mapPositionCoords && (
					<div
						className="border-t border-gray-200"
						style={previewContainerStyle}
					>
						<RMap
							className="h-full w-full"
							initial={{ center: mapPositionCoords, zoom: 10 }}
							noDefaultControls={true}
							interactions={[]}
						>
							<ROSM />
							<RLayerVector>
								<RFeature geometry={new Point(mapPositionCoords)}>
								</RFeature>
							</RLayerVector>
						</RMap>
					</div>
				)}
		</div>
	);
};
export default PostCard;
