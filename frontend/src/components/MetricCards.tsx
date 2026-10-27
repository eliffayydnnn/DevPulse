import React from 'react';
import { Star, GitFork, AlertCircle, ShieldCheck, Clock, ExternalLink, GitPullRequest } from 'lucide-react';
import type { RepositoryAnalysisDto } from '../types/api';

interface MetricCardsProps {
  analysis: RepositoryAnalysisDto;
}

export const MetricCards: React.FC<MetricCardsProps> = ({ analysis }) => {
  const { repository: repo, metrics } = analysis;

  const gradeClass = `grade-${metrics.healthGrade.replace('+', '-plus')}`;

  return (
    <>
      {/* Repository Hero Overview Header */}
      <div className="glass-panel" style={{ padding: '1.75rem', marginBottom: '2rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '1rem' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '1.25rem' }}>
            {repo.ownerAvatarUrl ? (
              <img
                src={repo.ownerAvatarUrl}
                alt={repo.owner}
                style={{ width: 56, height: 56, borderRadius: '12px', border: '1px solid rgba(255,255,255,0.1)' }}
              />
            ) : (
              <div style={{ width: 56, height: 56, borderRadius: '12px', background: '#1e293b', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <Star size={24} color="#38bdf8" />
              </div>
            )}
            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', flexWrap: 'wrap' }}>
                <h2 style={{ fontSize: '1.75rem', fontWeight: 800, letterSpacing: '-0.03em' }}>{repo.fullName}</h2>
                {repo.primaryLanguage && (
                  <span style={{ fontSize: '0.8rem', padding: '0.2rem 0.6rem', borderRadius: 9999, background: 'rgba(255,255,255,0.06)', border: '1px solid rgba(255,255,255,0.1)' }}>
                    {repo.primaryLanguage}
                  </span>
                )}
                {repo.license && (
                  <span style={{ fontSize: '0.8rem', padding: '0.2rem 0.6rem', borderRadius: 9999, background: 'rgba(255,255,255,0.06)', border: '1px solid rgba(255,255,255,0.1)' }}>
                    {repo.license}
                  </span>
                )}
                {analysis.isFromCache && (
                  <span style={{ fontSize: '0.75rem', padding: '0.15rem 0.5rem', borderRadius: 4, background: 'rgba(56, 189, 248, 0.15)', color: '#38bdf8' }}>
                    ⚡ Cached
                  </span>
                )}
              </div>
              <p style={{ color: '#94a3b8', fontSize: '0.95rem', marginTop: '0.35rem', maxWidth: '850px' }}>
                {repo.description || 'No description provided for this repository.'}
              </p>
            </div>
          </div>

          <a
            href={repo.htmlUrl}
            target="_blank"
            rel="noreferrer"
            className="search-submit-btn"
            style={{ textDecoration: 'none', padding: '0.6rem 1.25rem', fontSize: '0.875rem' }}
          >
            <span>View on GitHub</span>
            <ExternalLink size={16} />
          </a>
        </div>
      </div>

      {/* High-Level Metric KPI Cards */}
      <div className="metrics-grid">
        {/* Health Score */}
        <div className="glass-panel metric-card">
          <div className="metric-card-header">
            <span>Repository Health</span>
            <div className="metric-card-icon">
              <ShieldCheck size={20} color="#10b981" />
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.75rem' }}>
            <span className="metric-card-value">{metrics.healthScore}</span>
            <span style={{ color: '#94a3b8', fontSize: '1rem', fontWeight: 600 }}>/100</span>
            <span className={`health-grade-badge ${gradeClass}`}>{metrics.healthGrade}</span>
          </div>
          <div className="metric-card-footer">
            <span>Commit frequency, issue hygiene & maintenance</span>
          </div>
        </div>

        {/* Stars & Popularity */}
        <div className="glass-panel metric-card">
          <div className="metric-card-header">
            <span>Community Adoption</span>
            <div className="metric-card-icon">
              <Star size={20} color="#f59e0b" />
            </div>
          </div>
          <div className="metric-card-value">{repo.stars.toLocaleString()}</div>
          <div className="metric-card-footer">
            <GitFork size={14} />
            <span>{repo.forks.toLocaleString()} forks &bull; {repo.watchers.toLocaleString()} watchers</span>
          </div>
        </div>

        {/* Activity & Velocity */}
        <div className="glass-panel metric-card">
          <div className="metric-card-header">
            <span>Commit Velocity</span>
            <div className="metric-card-icon">
              <Clock size={20} color="#38bdf8" />
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <span className="pulse-dot" style={{ background: metrics.daysSinceLastCommit <= 7 ? '#10b981' : '#f59e0b' }} />
            <span className="metric-card-value" style={{ margin: 0 }}>{metrics.activityStatus}</span>
          </div>
          <div className="metric-card-footer" style={{ marginTop: '0.5rem' }}>
            <span>
              {metrics.daysSinceLastCommit === 0
                ? 'Latest commit pushed today'
                : metrics.daysSinceLastCommit === 1
                ? '1 day since latest commit'
                : `${metrics.daysSinceLastCommit} days since latest commit`}
            </span>
          </div>
        </div>

        {/* Pull Requests & Resolution */}
        <div className="glass-panel metric-card">
          <div className="metric-card-header">
            <span>PR & Issue Health</span>
            <div className="metric-card-icon">
              <GitPullRequest size={20} color="#8b5cf6" />
            </div>
          </div>
          <div className="metric-card-value">
            {metrics.openPullRequests} <span style={{ fontSize: '1rem', fontWeight: 500, color: '#94a3b8' }}>open PRs</span>
          </div>
          <div className="metric-card-footer">
            <AlertCircle size={14} />
            <span>{metrics.openIssues.toLocaleString()} open issues ({metrics.closedPullRequests} closed PRs)</span>
          </div>
        </div>
      </div>
    </>
  );
};
