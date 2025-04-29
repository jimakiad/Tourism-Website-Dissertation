// src/main.jsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App.jsx';
import './index.css'; // Imports global CSS including Tailwind directives

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode> {/* Helps catch potential problems */}
    <App />
  </React.StrictMode>,
);