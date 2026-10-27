import React, { useState, useEffect } from 'react';
import { Bookmark, Trash2, ArrowUpRight, FolderGit2, AlertCircle, RefreshCw } from 'lucide-react';
import { getSavedRepositories, removeSavedRepository, getStoredToken } from '../services/auth';
import type { SavedRepositoryDto } from '../types/api';

interface BookmarksViewProps {
  onSelectRepoForAnalysis: (repoQuery: string) => void;
  onOpenAuthModal: () => void;
}

export const BookmarksView: React.FC<BookmarksViewProps> = ({
  onSelectRepoForAnalysis,
  onOpenAuthModal,
}) => {
  const [bookmarks, setBookmarks] = useState<SavedRepositoryDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const isLoggedIn = !!getStoredToken();

  const loadBookmarks = async () => {
    if (!isLoggedIn) return;
    setIsLoading(true);
    setError(null);
    try {
      const data = await getSavedRepositories();
      setBookmarks(data);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Could not load bookmarks.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (isLoggedIn) {
      loadBookmarks();
    }
  }, [isLoggedIn]);

  const handleDelete = async (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    setDeletingId(id);
    try {
      await removeSavedRepository(id);
      setBookmarks((prev) => prev.filter((b) => b.id !== id));
    } catch (err: unknown) {
      alert(err instanceof Error ? err.message : 'Failed to delete bookmark.');
    } finally {
      setDeletingId(null);
    }
  };

  if (!isLoggedIn) {
    return (
      <div className="state-container glass-card" style={{ padding: '3rem 2rem', textAlign: 'center', maxWidth: '600px', margin: '2rem auto' }}>
        <Bookmark size={48} color="#38bdf8" style={{ marginBottom: '1rem', opacity: 0.8 }} />
        <h2 style={{ fontSize: '1.5rem', fontWeight: 600, color: 'var(--text-primary)', marginBottom: '0.5rem' }}>
          Bookmarks & Saved Repositories
        </h2>
        <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem', marginBottom: '1.5rem', lineHeight: 1.5 }}>
          Sign in or create an account to save repositories, maintain custom tracking lists, and monitor codebases across sessions.
        </p>
        <button
          type="button"
          onClick={onOpenAuthModal}
          className="search-btn"
          style={{ padding: '0.75rem 1.5rem', fontSize: '0.95rem' }}
        >
          Sign In to View Bookmarks
        </button>
      </div>
    );
  }

  return (
    <div style={{ maxWidth: '1000px', margin: '2rem auto', display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h2 style={{ fontSize: '1.5rem', fontWeight: 700, color: 'var(--text-primary)', display: 'flex', alignItems: 'center', gap: '0.5rem', margin: 0 }}>
            <Bookmark size={22} color="#38bdf8" />
            <span>Saved Repositories</span>
            <span className="nav-brand-badge" style={{ fontSize: '0.75rem', padding: '0.15rem 0.5rem' }}>
              {bookmarks.length} Bookmarks
            </span>
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem', marginTop: '0.25rem' }}>
            Quickly jump into analytics or manage your tracked GitHub repositories.
          </p>
        </div>
        <button
          type="button"
          onClick={loadBookmarks}
          disabled={isLoading}
          className="nav-tab-btn"
          style={{ padding: '0.5rem 1rem', background: 'rgba(255,255,255,0.06)' }}
        >
          <RefreshCw size={14} className={isLoading ? 'spinner' : ''} />
          <span>Refresh</span>
        </button>
      </div>

      {error && (
        <div className="error-banner">
          <AlertCircle size={20} color="#f43f5e" />
          <div style={{ flex: 1 }}>{error}</div>
        </div>
      )}

      {isLoading && bookmarks.length === 0 ? (
        <div className="state-container">
          <div className="spinner" />
          <p style={{ color: 'var(--text-muted)' }}>Retrieving your bookmarked repositories...</p>
        </div>
      ) : bookmarks.length === 0 ? (
        <div className="glass-card" style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>
          <FolderGit2 size={40} style={{ opacity: 0.5, marginBottom: '0.75rem' }} />
          <p style={{ fontSize: '1rem', color: 'var(--text-primary)' }}>No repositories saved yet</p>
          <p style={{ fontSize: '0.85rem' }}>
            Search for any repository in the Analytics tab and click &quot;Bookmark&quot; to keep it here.
          </p>
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1.25rem' }}>
          {bookmarks.map((b) => (
            <div
              key={b.id}
              className="glass-card"
              style={{
                padding: '1.25rem',
                cursor: 'pointer',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'space-between',
                transition: 'transform 0.2s, border-color 0.2s',
              }}
              onClick={() => onSelectRepoForAnalysis(b.fullName)}
            >
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                  <div style={{ fontWeight: 600, fontSize: '1.05rem', color: 'var(--text-primary)', wordBreak: 'break-word' }}>
                    {b.fullName}
                  </div>
                  <button
                    type="button"
                    title="Remove from favorites"
                    disabled={deletingId === b.id}
                    onClick={(e) => handleDelete(b.id, e)}
                    style={{
                      background: 'rgba(239, 68, 68, 0.1)',
                      border: '1px solid rgba(239, 68, 68, 0.2)',
                      borderRadius: '6px',
                      color: '#f87171',
                      padding: '0.35rem',
                      cursor: 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    <Trash2 size={14} />
                  </button>
                </div>
                {b.notes && (
                  <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', marginBottom: '0.75rem', fontStyle: 'italic' }}>
                    &ldquo;{b.notes}&rdquo;
                  </p>
                )}
              </div>

              <div
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  paddingTop: '0.75rem',
                  borderTop: '1px solid var(--border-subtle)',
                  marginTop: '0.75rem',
                  fontSize: '0.75rem',
                  color: 'var(--text-muted)',
                }}
              >
                <span>Saved: {new Date(b.savedAt).toLocaleDateString()}</span>
                <span style={{ color: 'var(--primary-color)', display: 'flex', alignItems: 'center', gap: '0.25rem', fontWeight: 500 }}>
                  Analyze <ArrowUpRight size={13} />
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
