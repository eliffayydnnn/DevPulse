import React, { useState } from 'react';
import { User, MapPin, Building, Users, Star, GitFork, ArrowRight, Loader2, ExternalLink } from 'lucide-react';
import type { GitHubUserDto, GitHubRepositoryDto } from '../types/api';
import { getUserProfile, getUserRepositories } from '../services/api';

interface UserExplorerViewProps {
  onSelectRepoForAnalysis: (repoQuery: string) => void;
}

export const UserExplorerView: React.FC<UserExplorerViewProps> = ({ onSelectRepoForAnalysis }) => {
  const [username, setUsername] = useState('davidfowl');
  const [user, setUser] = useState<GitHubUserDto | null>(null);
  const [repos, setRepos] = useState<GitHubRepositoryDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSearch = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!username.trim()) return;

    setIsLoading(true);
    setError(null);
    try {
      const [userData, reposData] = await Promise.all([
        getUserProfile(username.trim()),
        getUserRepositories(username.trim()),
      ]);
      setUser(userData);
      setRepos(reposData);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'User not found or GitHub API quota exceeded.');
      setUser(null);
      setRepos([]);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div style={{ marginTop: '1rem' }}>
      {/* Search Header */}
      <div className="glass-panel" style={{ padding: '2rem', marginBottom: '2rem' }}>
        <h2 style={{ fontSize: '1.5rem', fontWeight: 800, marginBottom: '0.5rem' }}>GitHub Developer Explorer</h2>
        <p style={{ color: '#94a3b8', fontSize: '0.95rem', marginBottom: '1.5rem' }}>
          Inspect developer portfolios, organizations, and instantly analyze their repositories.
        </p>

        <form onSubmit={handleSearch} style={{ display: 'flex', gap: '0.75rem', maxWidth: '500px' }}>
          <div className="search-form" style={{ flex: 1, padding: '0.35rem 0.75rem' }}>
            <User size={18} color="#94a3b8" />
            <input
              type="text"
              className="search-input"
              placeholder="GitHub username (e.g. davidfowl, torvalds)..."
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />
          </div>
          <button type="submit" className="search-submit-btn" disabled={isLoading || !username.trim()}>
            {isLoading ? <Loader2 size={18} className="spinner" style={{ width: 18, height: 18, margin: 0 }} /> : <span>Search</span>}
          </button>
        </form>

        <div className="quick-chips" style={{ justifyContent: 'flex-start', marginTop: '1rem' }}>
          <span className="chip-label">Suggestions:</span>
          {['davidfowl', 'torvalds', 'gaearon', 'shanselman'].map((u) => (
            <button
              key={u}
              type="button"
              className="quick-chip"
              onClick={() => {
                setUsername(u);
                setIsLoading(true);
                getUserProfile(u).then(usr => {
                  setUser(usr);
                  getUserRepositories(u).then(setRepos).finally(() => setIsLoading(false));
                }).catch(() => setIsLoading(false));
              }}
            >
              @{u}
            </button>
          ))}
        </div>
      </div>

      {error && (
        <div className="error-banner">
          <span>{error}</span>
        </div>
      )}

      {/* User Profile Card */}
      {user && (
        <div className="glass-panel user-profile-header">
          {user.avatarUrl ? (
            <img src={user.avatarUrl} alt={user.login} className="user-avatar-lg" />
          ) : (
            <div className="user-avatar-lg" style={{ background: '#1e293b', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <User size={40} color="#38bdf8" />
            </div>
          )}

          <div style={{ flex: 1 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', flexWrap: 'wrap' }}>
              <h2 style={{ fontSize: '1.75rem', fontWeight: 800 }}>{user.name || user.login}</h2>
              <span style={{ color: '#38bdf8', fontFamily: 'JetBrains Mono', fontSize: '0.95rem' }}>
                @{user.login}
              </span>
              <a href={user.htmlUrl} target="_blank" rel="noreferrer" style={{ color: '#94a3b8' }}>
                <ExternalLink size={16} />
              </a>
            </div>

            {user.bio && (
              <p style={{ color: '#94a3b8', fontSize: '0.95rem', margin: '0.5rem 0' }}>{user.bio}</p>
            )}

            <div style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap', marginTop: '0.75rem', fontSize: '0.875rem', color: '#64748b' }}>
              {user.company && (
                <span style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
                  <Building size={14} /> {user.company}
                </span>
              )}
              {user.location && (
                <span style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
                  <MapPin size={14} /> {user.location}
                </span>
              )}
              <span style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
                <Users size={14} /> {user.followers.toLocaleString()} followers &bull; {user.following.toLocaleString()} following
              </span>
              <span>{user.publicRepos} public repositories</span>
            </div>
          </div>
        </div>
      )}

      {/* Repositories Grid */}
      {repos.length > 0 && (
        <div>
          <h3 style={{ fontSize: '1.25rem', fontWeight: 700, marginBottom: '1.25rem' }}>
            Repositories by @{user?.login} ({repos.length})
          </h3>
          <div className="user-repos-grid">
            {repos.map((repo) => (
              <div key={repo.id} className="glass-panel user-repo-card">
                <div>
                  <h4 style={{ fontSize: '1.1rem', fontWeight: 700, marginBottom: '0.5rem', color: '#f8fafc' }}>
                    {repo.name}
                  </h4>
                  <p style={{ color: '#94a3b8', fontSize: '0.85rem', marginBottom: '1rem', minHeight: '2.5rem', display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                    {repo.description || 'No description provided.'}
                  </p>
                </div>

                <div>
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', fontSize: '0.8rem', color: '#64748b', marginBottom: '1rem' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                      <span style={{ display: 'flex', alignItems: 'center', gap: '0.25rem' }}>
                        <Star size={13} color="#f59e0b" /> {repo.stars.toLocaleString()}
                      </span>
                      <span style={{ display: 'flex', alignItems: 'center', gap: '0.25rem' }}>
                        <GitFork size={13} /> {repo.forks.toLocaleString()}
                      </span>
                    </div>
                    {repo.primaryLanguage && (
                      <span style={{ padding: '0.15rem 0.5rem', borderRadius: 4, background: 'rgba(255,255,255,0.06)' }}>
                        {repo.primaryLanguage}
                      </span>
                    )}
                  </div>

                  <button
                    type="button"
                    className="nav-tab-btn active"
                    style={{ width: '100%', justifyContent: 'center' }}
                    onClick={() => onSelectRepoForAnalysis(repo.fullName)}
                  >
                    <span>Analyze in DevPulse</span>
                    <ArrowRight size={14} />
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};
