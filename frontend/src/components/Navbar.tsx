import React from 'react';
import { Activity, BarChart2, GitCompare, User, BookOpen, ExternalLink } from 'lucide-react';
import type { RateLimitInfoDto } from '../types/api';

interface NavbarProps {
  activeTab: 'analytics' | 'comparison' | 'user' | 'architecture';
  onTabChange: (tab: 'analytics' | 'comparison' | 'user' | 'architecture') => void;
  rateLimit: RateLimitInfoDto | null;
}

export const Navbar: React.FC<NavbarProps> = ({ activeTab, onTabChange, rateLimit }) => {
  return (
    <nav className="navbar">
      <div className="nav-brand" onClick={() => onTabChange('analytics')}>
        <Activity size={26} color="#38bdf8" />
        <span>DevPulse</span>
        <span className="nav-brand-badge">SaaS v1.0</span>
      </div>

      <div className="nav-tabs">
        <button
          className={`nav-tab-btn ${activeTab === 'analytics' ? 'active' : ''}`}
          onClick={() => onTabChange('analytics')}
        >
          <BarChart2 size={16} />
          <span>Analytics</span>
        </button>
        <button
          className={`nav-tab-btn ${activeTab === 'comparison' ? 'active' : ''}`}
          onClick={() => onTabChange('comparison')}
        >
          <GitCompare size={16} />
          <span>Compare Repos</span>
        </button>
        <button
          className={`nav-tab-btn ${activeTab === 'user' ? 'active' : ''}`}
          onClick={() => onTabChange('user')}
        >
          <User size={16} />
          <span>User Explorer</span>
        </button>
        <button
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
      </div>
    </nav>
  );
};
