// src/pages/CreatePostPage.jsx
import React, { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import ReactMarkdown from "react-markdown";
// Import necessary API functions
import {
	createPost,
	uploadPostImage, // Import the new function
	getCountries,
	getCategories,
	getTags,
	getApiErrorMessage,
} from "../services/api";
import MapInput from "../components/MapInput";

const CreatePostPage = () => {
	// State for form fields
	const [title, setTitle] = useState("");
	const [body, setBody] = useState("");
	const [countryId, setCountryId] = useState("");
	const [selectedCategoryIds, setSelectedCategoryIds] = useState([]);
	const [selectedTagIds, setSelectedTagIds] = useState([]);
	const [countries, setCountries] = useState([]);
	const [categories, setCategories] = useState([]);
	const [tags, setTags] = useState([]);
	const [coordinates, setCoordinates] = useState(null);
	const [imageFile, setImageFile] = useState(null);
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState(null);
	const navigate = useNavigate();

	// Fetch countries, categories, tags when component mounts
	useEffect(() => {
		const fetchData = async () => {
			setIsLoading(true); // Indicate loading starts
			try {
				// Use Promise.all to fetch concurrently
				const [countriesRes, categoriesRes, tagsRes] = await Promise.all([
					getCountries(),
					getCategories(),
					getTags(),
				]);
				setCountries(countriesRes.data);
				setCategories(categoriesRes.data);
				setTags(tagsRes.data);
			} catch (err) {
				setError("Could not load data needed for the form.");
				console.error("Fetch error:", err);
			} finally {
				setIsLoading(false); // Indicate loading finished
			}
		};
		fetchData();
	}, []); // Empty array means run only once on mount

	// Handler for category checkbox changes
	const handleCategoryChange = (event) => {
		const categoryId = Number.parseInt(event.target.value, 10);
		if (event.target.checked) {
			setSelectedCategoryIds((prev) => [...prev, categoryId]);
		} else {
			setSelectedCategoryIds((prev) => prev.filter((id) => id !== categoryId));
		}
	};

	// Handler for tag checkbox changes
	const handleTagChange = (event) => {
		const tagId = Number.parseInt(event.target.value, 10);
		if (event.target.checked) {
			setSelectedTagIds((prev) => [...prev, tagId]);
		} else {
			setSelectedTagIds((prev) => prev.filter((id) => id !== tagId));
		}
	};

	// Handler for map coordinate changes (passed to MapInput)
	const handlePositionChange = useCallback((newCoords) => {
		console.log("CreatePostPage: handlePositionChange called with:", newCoords);
		setCoordinates(newCoords); // Update state in parent
	}, []);

	// Handler for image file selection
	const handleImageChange = (event) => {
		if (event.target?.files?.[0]) {
			setImageFile(event.target.files[0]);
			setError(null); // Clear error if a file is selected
		} else {
			setImageFile(null);
		}
	};

	// Handler for form submission
	const handleSubmit = async (e) => {
		e.preventDefault();
		if (countryId === "") {
			setError("Please select a country.");
			return;
		}
		setIsLoading(true);
		setError(null);

		let newPostId = null;

		try {
			// 1. Prepare core post data
			const postData = {
				title,
				body,
				countryId: Number(countryId),
				categoryIds: selectedCategoryIds,
				tagIds: selectedTagIds,
				latitude: coordinates?.latitude,
				longitude: coordinates?.longitude,
			};

			// 2. Create the post
			const response = await createPost(postData);
			newPostId = response.data.id;
			console.log(`Post created successfully with ID: ${newPostId}`);

			// 3. If post created AND image selected, UPLOAD image
			if (imageFile && newPostId) {
				console.log(`Attempting to upload image for post ${newPostId}...`);
				const formData = new FormData();
				formData.append("imageFile", imageFile); // Key matches backend param

				try {
					// ----- Call the dedicated upload function -----
					const uploadResponse = await uploadPostImage(newPostId, formData);
					console.log("Image upload successful:", uploadResponse.data);
				} catch (uploadError) {
					console.error("Image upload failed:", uploadError);
					// Set error but proceed with navigation
					setError(
						`Post created, but image upload failed: ${getApiErrorMessage(uploadError)}`,
					);
				}
			}

			// 4. Navigate home
			navigate("/");
		} catch (err) {
			console.error("Create post error:", err);
			setError(getApiErrorMessage(err));
			setIsLoading(false); // Stop loading ONLY if post creation failed
		}
	};
	// --- END UPDATED handleSubmit ---

	// Render the form JSX
	return (
		<div className="container mx-auto max-w-2xl p-4 pt-6">
			<div className="bg-white p-6 rounded border border-gray-300 shadow-sm">
				<h2 className="text-lg font-semibold mb-5 text-gray-700 border-b pb-3">
					Create a new post
				</h2>
				<form onSubmit={handleSubmit}>
					{/* Display Error Messages */}
					{error && (
						<p className="bg-red-100 text-red-600 p-2 rounded mb-4 text-xs">
							{error}
						</p>
					)}

					{/* Title Input */}
					<div className="mb-4">
						<input
							type="text"
							placeholder="Post Title"
							value={title}
							onChange={(e) => setTitle(e.target.value)}
							required
							maxLength={300}
							className="border rounded w-full py-2 px-3 text-gray-700 text-base focus:outline-none focus:ring-1 focus:ring-blue-400"
						/>
					</div>

					{/* Country Select */}
					<div className="mb-4">
						<label
							className="block text-gray-700 text-xs font-bold mb-1"
							htmlFor="country"
						>
							Country
						</label>
						<select
							id="country"
							value={countryId}
							onChange={(e) => setCountryId(e.target.value)}
							required
							className="border rounded w-full py-2 px-3 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400 bg-white appearance-none"
						>
							<option value="" disabled>
								-- Select Target Country --
							</option>
							{countries.map((c) => (
								<option key={c.id} value={c.id}>
									{c.name}
								</option>
							))}
						</select>
					</div>

					{/* Categories Checkboxes */}
					<div className="mb-4">
						<label
							htmlFor="categories-group"
							className="block text-gray-700 text-xs font-bold mb-2"
						>
							Categories
						</label>
						<div
							id="categories-group"
							className="grid grid-cols-2 sm:grid-cols-3 gap-2 max-h-32 overflow-y-auto border p-2 rounded"
						>
							{categories.length > 0 ? (
								categories.map((cat) => (
									<label
										key={cat.id}
										className="flex items-center space-x-2 text-sm cursor-pointer"
									>
										<input
											type="checkbox"
											value={cat.id}
											checked={selectedCategoryIds.includes(cat.id)}
											onChange={handleCategoryChange}
											className="rounded text-blue-500 focus:ring-blue-400"
										/>
										<span>{cat.name}</span>
									</label>
								))
							) : (
								<span className="text-gray-400 text-sm italic">Loading...</span>
							)}
						</div>
					</div>

					{/* Tags Checkboxes */}
					<div className="mb-4">
						<label
							htmlFor="tags-group"
							className="block text-gray-700 text-xs font-bold mb-2"
						>
							Tags (Optional)
						</label>
						<div
							id="tags-group"
							className="grid grid-cols-2 sm:grid-cols-3 gap-2 max-h-32 overflow-y-auto border p-2 rounded"
						>
							{tags.length > 0 ? (
								tags.map((tag) => (
									<label
										key={tag.id}
										className="flex items-center space-x-2 text-sm cursor-pointer"
									>
										<input
											type="checkbox"
											value={tag.id}
											checked={selectedTagIds.includes(tag.id)}
											onChange={handleTagChange}
											className="rounded text-blue-500 focus:ring-blue-400"
										/>
										<span>{tag.name}</span>
									</label>
								))
							) : (
								<span className="text-gray-400 text-sm italic">Loading...</span>
							)}
						</div>
					</div>

					{/* Map Input */}
					<div className="mb-4">
						<label
							htmlFor="map-input"
							className="block text-gray-700 text-xs font-bold mb-2"
						>
							Location (Optional)
						</label>
						{/* Render MapInput and pass the handler */}
						<MapInput id="map-input" onPositionChange={handlePositionChange} />
					</div>

					{/* Image Input */}
					<div className="mb-4">
						<label
							className="block text-gray-700 text-xs font-bold mb-2"
							htmlFor="imageFile"
						>
							Upload Image (Optional - Not Uploaded Yet)
						</label>
						<input
							type="file"
							id="imageFile"
							onChange={handleImageChange}
							accept=".jpg,.jpeg,.png,.webp"
							className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
						/>
						{/* Show selected filename */}
						{imageFile && (
							<p className="text-xs text-gray-600 mt-1">
								Selected: {imageFile.name}
							</p>
						)}
					</div>

					{/* Body Textarea */}
					<div className="mb-4">
						<label
							className="block text-gray-700 text-xs font-bold mb-1"
							htmlFor="body"
						>
							Body (Markdown supported)
						</label>
						<textarea
							id="body"
							placeholder="Share your experience, ask a question, or give a tip..."
							value={body}
							onChange={(e) => setBody(e.target.value)}
							required
							rows={10}
							className="border rounded w-full py-2 px-3 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400 font-mono"
						/>
					</div>

					{/* Markdown Preview */}
					<div className="mb-5 p-3 border rounded bg-gray-50 max-h-60 overflow-y-auto">
						<h4 className="text-xs font-semibold text-gray-500 mb-2">
							Preview:
						</h4>
						<div className="prose prose-sm max-w-none">
							{/* Render markdown preview */}
							<ReactMarkdown>
								{body || "*Your post content will preview here.*"}
							</ReactMarkdown>
						</div>
					</div>

					{/* Submit Button */}
					<div className="flex justify-end">
						<button
							type="submit"
							disabled={isLoading}
							className="bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-5 rounded-full text-sm focus:outline-none focus:shadow-outline disabled:opacity-50"
						>
							{isLoading ? "Submitting..." : "Post"}
						</button>
					</div>
				</form>
			</div>
		</div>
	);
};
export default CreatePostPage; // Export the component
