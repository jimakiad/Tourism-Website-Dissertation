import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import { createPost, getCountries, getApiErrorMessage } from '../services/api';

const CreatePostPage = () => {
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [countryId, setCountryId] = useState('');
  const [tags, setTags] = useState('');
  const [categories, setCategories] = useState('');
  const [countries, setCountries] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchCountries = async () => {
      try {
        const response = await getCountries();
        setCountries(response.data);
      } catch (err) {
        setError("Could not load countries list.");
      }
    };
    fetchCountries();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (countryId === '') { setError("Please select a country."); return; }
    setIsLoading(true); setError(null);
    try {
      const postData = { title, body, countryId: Number(countryId), tags, categories };
      await createPost(postData);
      navigate('/');
    } catch (err) {
      setError(getApiErrorMessage(err));
      setIsLoading(false);
    }
  };

  return (
    <div className="container mx-auto max-w-2xl p-4 pt-6">
        <div className="bg-white p-6 rounded border border-gray-300 shadow-sm">
            <h2 className="text-lg font-semibold mb-5 text-gray-700 border-b pb-3">Create a new post</h2>
            <form onSubmit={handleSubmit}>
            {error && <p className="bg-red-100 text-red-600 p-2 rounded mb-4 text-xs">{error}</p>}

            <div className="mb-4">
                <input type="text" placeholder="Post Title" value={title} onChange={(e) => setTitle(e.target.value)} required maxLength={300} className="border rounded w-full py-2 px-3 text-gray-700 text-base focus:outline-none focus:ring-1 focus:ring-blue-400"/>
            </div>

            <div className="mb-4">
                <select value={countryId} onChange={(e) => setCountryId(e.target.value)} required className="border rounded w-full py-2 px-3 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400 bg-white appearance-none">
                    <option value="" disabled>-- Select Target Country --</option>
                    {countries.map(c => (
                        <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                </select>
            </div>

             <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                <input type="text" placeholder="Categories (comma-separated)" value={categories} onChange={(e) => setCategories(e.target.value)} className="border rounded w-full py-2 px-3 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400"/>
                <input type="text" placeholder="Tags (comma-separated)" value={tags} onChange={(e) => setTags(e.target.value)} className="border rounded w-full py-2 px-3 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400"/>
            </div>

            <div className="mb-4">
                <textarea placeholder="Text (Markdown supported)" value={body} onChange={(e) => setBody(e.target.value)} required rows={10} className="border rounded w-full py-2 px-3 text-gray-700 text-sm focus:outline-none focus:ring-1 focus:ring-blue-400 font-mono"/>
            </div>

            <div className="mb-5 p-3 border rounded bg-gray-50 max-h-60 overflow-y-auto">
                <h4 className="text-xs font-semibold text-gray-500 mb-2">Preview:</h4>
                <div className="prose prose-sm max-w-none">
                <ReactMarkdown>{body || "*Your post content will preview here.*"}</ReactMarkdown>
                </div>
            </div>

            <div className="flex justify-end">
                <button type="submit" disabled={isLoading} className="bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-5 rounded-full text-sm focus:outline-none focus:shadow-outline disabled:opacity-50">
                {isLoading ? 'Submitting...' : 'Post'}
                </button>
            </div>
            </form>
        </div>
    </div>
  );
};
export default CreatePostPage;