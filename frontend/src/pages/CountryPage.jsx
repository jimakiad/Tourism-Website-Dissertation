// src/pages/CountryPage.jsx
import React, { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getCountryByCode, getPosts, getApiErrorMessage } from '../services/api';
import PostCard from '../components/PostCard';
import { useAuth } from '../contexts/AuthContext';

const CountryDescriptions = [
    {countryCode: 'US', description: 'Welcome to the United States!'},
    {countryCode: 'CA', description: 'Welcome to Canada!'},
    {countryCode: 'MX', description: 'Welcome to Mexico!'},
    {countryCode: 'GB', description: 'Welcome to the United Kingdom!'},
    {countryCode: 'FR', description: 'Welcome to France!'},
    {countryCode: 'JP', description: 'Welcome to Japan!'},
    {countryCode: 'IT', description: 'Welcome to Italy!'},
    {countryCode: 'GR', description: 'Welcome to Greece!'}
]

const CountryPage = () => {
    const { countryCode } = useParams(); // Get country code from URL
    const [country, setCountry] = useState(null);
    const [posts, setPosts] = useState([]);
    const [isLoadingCountry, setIsLoadingCountry] = useState(true);
    const [isLoadingPosts, setIsLoadingPosts] = useState(true);
    const [countryError, setCountryError] = useState(null);
    const [postsError, setPostsError] = useState(null);
    const [sortBy, setSortBy] = useState('top');
    const { isAuthenticated } = useAuth();

    // Fetch Country Details
    const fetchCountryData = useCallback(async () => {
        if (!countryCode) return;
        setIsLoadingCountry(true); setCountryError(null);
        try {
            const response = await getCountryByCode(countryCode);
            setCountry(response.data);
        } catch (err) {
            setCountryError(getApiErrorMessage(err));
             setCountry(null); // Ensure country is null on error
        } finally {
            setIsLoadingCountry(false);
        }
    }, [countryCode]);

    // Fetch Posts for this Country
    const fetchCountryPosts = useCallback(async () => {
        if (!countryCode) return; // Don't fetch if no code
         setIsLoadingPosts(true); setPostsError(null);
         try {
             // Pass countryCode to getPosts
             const response = await getPosts(sortBy, 25, countryCode);
             setPosts(response.data);
         } catch (err) {
             setPostsError(getApiErrorMessage(err));
             setPosts([]);
         } finally {
             setIsLoadingPosts(false);
         }
    }, [countryCode, sortBy]); // Refetch if code or sort changes

    // Initial fetches
    useEffect(() => {
        fetchCountryData();
        fetchCountryPosts();
    }, [fetchCountryData, fetchCountryPosts]); // Depend on callbacks

    // --- Render Logic ---
    if (isLoadingCountry) {
        return <p className="text-center text-gray-500 py-10">Loading country info...</p>;
    }

    if (countryError || !country) {
         return <p className="text-center text-red-500 py-10">Error: Country not found or could not be loaded ({countryError || 'Not Found'}).</p>;
    }

    // Main page content
    return (
        <div className="container mx-auto max-w-3xl p-4 pt-6">
            {/* Country Header */}
            <div className='mb-6 p-4 bg-white rounded border border-gray-300 shadow-sm'>
                 <h1 className="text-2xl font-bold mb-2">{country.name}</h1>
                 {/* Placeholder for Description/Rules */}
                 <p className="text-sm text-gray-600 italic">
                    {CountryDescriptions.find(desc => desc.countryCode === country.code)?.description || 'No description available.'}
                 </p>
            </div>

             {/* Optional Create Post Link */}
             {isAuthenticated && (
                <div className="mb-4">
                    <Link to="/create-post" state={{ prefillCountry: country.id }}
                        className="block p-3 bg-white border border-gray-300 rounded text-sm text-gray-700 hover:border-gray-400">
                        Create Post in t/{country.name}...
                    </Link>
                </div>
             )}

            {/* Sorting Tabs */}
            <div className="flex border-b border-gray-300 mb-4 bg-white px-2 rounded-t">
                 <button
                     type="button"
                     onClick={() => setSortBy('top')}
                     className={`px-4 py-2 text-sm font-semibold ${sortBy === 'top' ? 'border-b-2 border-blue-500 text-blue-600' : 'text-gray-500 hover:text-gray-700'}`}>
                     🔥 Hot / Top
                 </button>
                 <button
                     type="button"
                     onClick={() => setSortBy('new')}
                     className={`px-4 py-2 text-sm font-semibold ${sortBy === 'new' ? 'border-b-2 border-blue-500 text-blue-600' : 'text-gray-500 hover:text-gray-700'}`}>
                     ✨ New
                 </button>
            </div>

            {/* Post List */}
            {isLoadingPosts && <p className="text-center text-gray-500 py-10">Loading posts...</p>}
            {postsError && <p className="text-center text-red-500 py-10">Error loading posts: {postsError}</p>}
            {!isLoadingPosts && !postsError && posts.length === 0 && (
                <p className="text-center text-gray-500 py-10">No posts found for this country yet.</p>
            )}
            {!isLoadingPosts && !postsError && posts.length > 0 && (
                <div>
                    {posts.map((post) => (
                        <PostCard key={post.id} post={post} />
                    ))}
                </div>
            )}
        </div>
    );
};

export default CountryPage;