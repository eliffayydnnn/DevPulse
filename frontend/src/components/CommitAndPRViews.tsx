import React, { useState } from 'react';
import { GitCommit, GitPullRequest, AlertCircle, ExternalLink } from 'lucide-react';
import type { CommitSummaryDto, PullRequestSummaryDto, IssueSummaryDto } from '../types/api';

interface CommitAndPRViewsProps {
  commits: CommitSummaryDto[];
  pullRequests: PullRequestSummaryDto[];
  issues: IssueSummaryDto[];
}

export const CommitAndPRViews: React.FC<CommitAndPRViewsProps> = ({
  commits,
  pullRequests,
  issues,
}) => {
  const [activeTab, setActiveTab] = useState<'prs' | 'issues'>('prs');

  const formatRelativeTime = (dateStr: string) => {
    const diffMs = Date.now() - new Date(dateStr).getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    if (diffHours < 1) return 'just now';
    if (diffHours < 24) return `${diffHours}h ago`;
    const diffDays = Math.floor(diffHours / 24);
    if (diffDays === 1) return 'yesterday';
    if (diffDays < 30) return `${diffDays}d ago`;
    return new Date(dateStr).toLocaleDateString();
  };

  return (
    <div className="content-grid">
      {/* Recent Commits */}
      <div className="glass-panel list-card">
        <div className="chart-header">
          <div className="chart-title">
            <GitCommit size={20} color="#38bdf8" />
            <span>Recent Commits</span>
          </div>
          <span style={{ fontSize: '0.8rem', color: '#64748b' }}>Latest {commits.length} pushes</span>
        </div>

        {commits.length === 0 ? (
          <div style={{ padding: '2rem 0', textAlign: 'center', color: '#64748b' }}>No recent commits found.</div>
        ) : (
          <div>
            {commits.slice(0, 7).map((commit) => (
              <div key={commit.sha} className="item-row">
                {commit.authorAvatarUrl ? (
                  <img src={commit.authorAvatarUrl} alt={commit.authorName} className="item-avatar" />
                ) : (
                  <div className="item-avatar" style={{ background: '#1e293b', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <GitCommit size={14} color="#94a3b8" />
                  </div>
                )}
                <div className="item-details">
                  <a href={commit.htmlUrl} target="_blank" rel="noreferrer" className="item-title-link">
                    {commit.message}
                  </a>
                  <div className="item-meta">
                    <span style={{ fontFamily: 'JetBrains Mono', color: '#38bdf8', fontSize: '0.75rem' }}>
                      {commit.shortSha}
                    </span>
                    <span>&bull;</span>
                    <span>{commit.authorName}</span>
                    <span>&bull;</span>
                    <span>{formatRelativeTime(commit.date)}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Pull Requests & Issues Tabs */}
      <div className="glass-panel list-card">
        <div className="chart-header">
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button
              className={`nav-tab-btn ${activeTab === 'prs' ? 'active' : ''}`}
              style={{ padding: '0.35rem 0.75rem', fontSize: '0.825rem' }}
              onClick={() => setActiveTab('prs')}
            >
              <GitPullRequest size={15} />
              <span>Pull Requests ({pullRequests.length})</span>
            </button>
            <button
              className={`nav-tab-btn ${activeTab === 'issues' ? 'active' : ''}`}
              style={{ padding: '0.35rem 0.75rem', fontSize: '0.825rem' }}
              onClick={() => setActiveTab('issues')}
            >
              <AlertCircle size={15} />
              <span>Issues ({issues.length})</span>
            </button>
          </div>
        </div>

        {activeTab === 'prs' ? (
          pullRequests.length === 0 ? (
            <div style={{ padding: '2rem 0', textAlign: 'center', color: '#64748b' }}>No pull requests evaluated.</div>
          ) : (
            <div>
              {pullRequests.slice(0, 7).map((pr) => (
                <div key={pr.number} className="item-row">
                  <div className="item-details">
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                      <span className={`status-pill ${pr.isMerged ? 'status-merged' : pr.state === 'open' ? 'status-open' : 'status-closed'}`}>
                        {pr.isMerged ? 'Merged' : pr.state}
                      </span>
                      <a href={pr.htmlUrl} target="_blank" rel="noreferrer" className="item-title-link">
                        #{pr.number} {pr.title}
                      </a>
                    </div>
                    <div className="item-meta">
                      <span>by @{pr.authorLogin}</span>
                      <span>&bull;</span>
                      <span>opened {formatRelativeTime(pr.createdAt)}</span>
                    </div>
                  </div>
                  <a href={pr.htmlUrl} target="_blank" rel="noreferrer" style={{ color: '#64748b' }}>
                    <ExternalLink size={14} />
                  </a>
                </div>
              ))}
            </div>
          )
        ) : (
          issues.length === 0 ? (
            <div style={{ padding: '2rem 0', textAlign: 'center', color: '#64748b' }}>No issues evaluated.</div>
          ) : (
            <div>
              {issues.slice(0, 7).map((issue) => (
                <div key={issue.number} className="item-row">
                  <div className="item-details">
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                      <span className={`status-pill ${issue.state === 'open' ? 'status-open' : 'status-closed'}`}>
                        {issue.state}
                      </span>
                      <a href={issue.htmlUrl} target="_blank" rel="noreferrer" className="item-title-link">
                        #{issue.number} {issue.title}
                      </a>
                    </div>
                    <div className="item-meta">
                      <span>by @{issue.authorLogin}</span>
                      <span>&bull;</span>
                      <span>{issue.commentsCount} comments</span>
                      <span>&bull;</span>
                      <span>{formatRelativeTime(issue.createdAt)}</span>
                    </div>
                  </div>
                  <a href={issue.htmlUrl} target="_blank" rel="noreferrer" style={{ color: '#64748b' }}>
                    <ExternalLink size={14} />
                  </a>
                </div>
              ))}
            </div>
          )
        )}
      </div>
    </div>
  );
};
