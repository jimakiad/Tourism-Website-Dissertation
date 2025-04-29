import React, { useState, useEffect } from 'react';
import { getPosts, getApiErrorMessage } from '../services/api';
import PostCard from '../components/PostCard';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const HomePage = () => {
  const [posts, setPosts] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [sortBy, setSortBy] = useState('top');
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    const fetchPosts = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const response = await getPosts(sortBy);
        setPosts(response.data);
      } catch (err) {
        setError(getApiErrorMessage(err));
        setPosts([]);
      } finally {
        setIsLoading(false);
      }
    };
    fetchPosts();
  }, [sortBy]);

  return (
    <div className="container mx-auto max-w-3xl p-4 pt-6">
       {isAuthenticated && (
         <div className="mb-4">
            <Link to="/create-post" className="block p-3 bg-white border border-gray-300 rounded text-sm text-gray-700 hover:border-gray-400">
                Create New Post...
            </Link>
         </div>
       )}

       <div className="flex border-b border-gray-300 mb-4">
         <button
           type="button"
           onClick={() => setSortBy('top')}
           className={`px-4 py-2 text-sm font-semibold ${sortBy === 'top' ? 'border-b-2 border-blue-500 text-blue-600' : 'text-gray-500 hover:text-gray-700'}`}
         >
           🔥 Hot / Top
         </button>
         <button
           type="button"
           onClick={() => setSortBy('new')}
           className={`px-4 py-2 text-sm font-semibold ${sortBy === 'new' ? 'border-b-2 border-blue-500 text-blue-600' : 'text-gray-500 hover:text-gray-700'}`}
         >
           ✨ New
         </button>
       </div>

      {isLoading && <p className="text-center text-gray-500 py-10">Loading posts...</p>}
      {error && <p className="text-center text-red-500 py-10">Error: {error}</p>}
      {!isLoading && !error && posts.length === 0 && (
        <p className="text-center text-gray-500 py-10">No posts yet. Be the first!</p>
      )}
      {!isLoading && !error && posts.length > 0 && (
        <div>
          {posts.map((post) => (
            <PostCard key={post.id} post={post} />
          ))}
        </div>
      )}
    </div>
  );
};
export default HomePage;