// src/components/PostCard.jsx
import React from 'react';
import VoteButtons from './VoteButtons';
import ReactMarkdown from 'react-markdown';

const formatDate = (dateString) => {
  if (!dateString) return '';
  const options = { year: 'numeric', month: 'short', day: 'numeric' };
  try {
     return new Date(dateString).toLocaleDateString(undefined, options);
  } catch (e) { return dateString; }
};

const PostCard = ({ post }) => {
  return (
    <div className="flex bg-white shadow-sm rounded border border-gray-200 hover:border-gray-400 mb-3">
      <VoteButtons postId={post.id} initialScore={post.score} />
      <div className="p-3 flex-grow overflow-hidden">
        <div className="text-xs text-gray-500 mb-1 flex flex-wrap items-center space-x-2">
          <span className="font-medium text-blue-700 hover:underline cursor-pointer">
            t/{post.countryName || 'unknown'}
          </span>
          <span className="text-gray-300">•</span>
          <span>Posted by u/{post.authorUsername || 'anonymous'}</span>
          <span>{formatDate(post.createdAt)}</span>
        </div>

        <h2 className="text-base sm:text-lg font-medium text-gray-800 mb-1 break-words">
          {post.title}
        </h2>

         <div className="mt-2 text-xs flex flex-wrap gap-1">
           {post.categories?.split(',').map(c => c.trim()).filter(Boolean).map(cat => (
             <span key={`cat-${cat}`} className="bg-blue-100 text-blue-800 px-1.5 py-0.5 rounded">{cat}</span>
           ))}
           {post.tags?.split(',').map(t => t.trim()).filter(Boolean).map(tag => (
             <span key={`tag-${tag}`} className="bg-gray-200 text-gray-700 px-1.5 py-0.5 rounded">{tag}</span>
           ))}
        </div>
      </div>
    </div>
  );
};
export default PostCard;