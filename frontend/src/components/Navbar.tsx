import React from 'react';
import { Activity, BarChart2, GitCompare, User as UserIcon, BookOpen, ExternalLink, Bookmark, LogIn, LogOut } from 'lucide-react';
import type { RateLimitInfoDto, UserDto } from '../types/api';

interface NavbarProps {
  activeTab: 'analytics' | 'comparison' | 'user' | 'architecture' | 'bookmarks';
  onTabChange: (tab: 'analytics' | 'comparison' | 'user' | 'architecture' | 'bookmarks') => void;
  rateLimit: RateLimitInfoDto | null;
  user: UserDto | null;
  onOpenAuthModal: () => void;
  onSignOut: () => void;
}

export const Navbar: React.FC<NavbarProps> = ({
  activeTab,
  onTabChange,
  rateLimit,
  user,
  onOpenAuthModal,
  onSignOut,
}) => {
  return (
    <nav className="navbar">
      <div className="nav-brand" onClick={() => onTabChange('analytics')}>
        <Activity size={26} color="#38bdf8" />
        <span>DevPulse</span>
        <span className="nav-brand-badge">SaaS v1.0</span>
      </div>

      <div className="nav-tabs">
        <button
          type="button"
          className={`nav-tab-btn ${activeTab === 'analytics' ? 'active' : ''}`}
          onClick={() => onTabChange('analytics')}
        >
          <BarChart2 size={16} />
          <span>Analytics</span>
        </button>
        <button
          type="button"
          className={`nav-tab-btn ${activeTab === 'comparison' ? 'active' : ''}`}
          onClick={() => onTabChange('comparison')}
        >
          <GitCompare size={16} />
          <span>Compare Repos</span>
        </button>
        <button
          type="button"
          className={`nav-tab-btn ${activeTab === 'user' ? 'active' : ''}`}
          onClick={() => onTabChange('user')}
        >
          <UserIcon size={16} />
          <span>User Explorer</span>
        </button>
        <button
          type="button"
          className={`nav-tab-btn ${activeTab === 'bookmarks' ? 'active' : ''}`}
          onClick={() => onTabChange('bookmarks')}
        >
          <Bookmark size={16} />
          <span>Bookmarks</span>
        </button>
        <button
          type="button"
          className={`nav-tab-btn ${activeTab === 'architecture' ? 'active' : ''}`}
          onClick={() => onTabChange('architecture')}
        >
          <BookOpen size={16} />
          <span>Architecture</span>
        </button>
      </div>

      <div className="nav-status">
        {rateLimit && (
          <div className="rate-limit-pill" title={`Quota reset window: ${rateLimit.resetTimeUtc || 'N/A'}`}>
            <span className="pulse-dot" />
            <span>API Quota: <strong>{rateLimit.remaining}</strong>/{rateLimit.limit}</span>
          </div>
        )}
        <a
          href="https://github.com/eliffayydnnn/DevPulse"
          target="_blank"
          rel="noreferrer"
          className="nav-tab-btn"
          style={{ textDecoration: 'none' }}
        >
          <span>GitHub</span>
          <ExternalLink size={14} />
        </a>

        {user ? (
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '0.4rem',
                padding: '0.35rem 0.75rem',
                background: 'rgba(56, 189, 248, 0.1)',
                border: '1px solid rgba(56, 189, 248, 0.25)',
                borderRadius: '8px',
                fontSize: '0.85rem',
                color: '#38bdf8',
                fontWeight: 600,
              }}
            >
              <UserIcon size={14} />
              <span>{user.username}</span>
            </div>
            <button
              type="button"
              onClick={onSignOut}
              className="nav-tab-btn"
              title="Sign Out"
              style={{ padding: '0.4rem 0.6rem' }}
            >
              <LogOut size={15} />
            </button>
          </div>
        ) : (
          <button
            type="button"
            onClick={onOpenAuthModal}
            className="search-btn"
            style={{ padding: '0.4rem 0.9rem', fontSize: '0.85rem', display: 'flex', alignItems: 'center', gap: '0.4rem' }}
          >
            <LogIn size={14} />
            <span>Sign In</span>
          </button>
        )}
      </div>
    </nav>
  );
};

