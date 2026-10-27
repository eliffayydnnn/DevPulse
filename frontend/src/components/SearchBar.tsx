import React, { useState } from 'react';
import { Search, Loader2 } from 'lucide-react';

interface SearchBarProps {
  onSearch: (query: string) => void;
  isLoading: boolean;
  initialQuery?: string;
}

const PRESETS = [
  'dotnet/aspnetcore',
  'facebook/react',
  'vercel/next.js',
  'microsoft/vscode',
  'torvalds/linux',
];

export const SearchBar: React.FC<SearchBarProps> = ({ onSearch, isLoading, initialQuery = '' }) => {
  const [query, setQuery] = useState(initialQuery);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (query.trim() && !isLoading) {
      onSearch(query.trim());
    }
  };

  const handlePresetClick = (preset: string) => {
    setQuery(preset);
    onSearch(preset);
  };

  return (
    <div className="search-hero">
      <h1 className="search-title">Developer Repository Intelligence</h1>
      <p className="search-subtitle">
        Deep dive into commit frequencies, pull request velocity, community metrics, and engineering health of any public GitHub project.
      </p>

      <div className="search-box-wrapper">
        <form className="search-form" onSubmit={handleSubmit}>
          <Search size={20} color="#94a3b8" />
          <input
            type="text"
            className="search-input"
            placeholder="Enter owner/repo (e.g. dotnet/aspnetcore) or full GitHub URL..."
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            disabled={isLoading}
          />
          <button type="submit" className="search-submit-btn" disabled={isLoading || !query.trim()}>
            {isLoading ? (
              <>
                <Loader2 size={18} className="spinner" style={{ width: 18, height: 18, margin: 0, borderWidth: 2 }} />
                <span>Analyzing...</span>
              </>
            ) : (
              <span>Analyze</span>
            )}
          </button>
        </form>

        <div className="quick-chips">
          <span className="chip-label">Quick samples:</span>
          {PRESETS.map((preset) => (
            <button
              key={preset}
              type="button"
              className="quick-chip"
              onClick={() => handlePresetClick(preset)}
              disabled={isLoading}
            >
              {preset}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
};
