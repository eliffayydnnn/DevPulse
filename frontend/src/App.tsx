import { useState, useEffect } from 'react';
import { Navbar } from './components/Navbar';
import { SearchBar } from './components/SearchBar';
import { MetricCards } from './components/MetricCards';
import { ActivityCharts } from './components/ActivityCharts';
import { CommitAndPRViews } from './components/CommitAndPRViews';
import { ComparisonView } from './components/ComparisonView';
import { UserExplorerView } from './components/UserExplorerView';
import { ArchitectureView } from './components/ArchitectureView';
import { analyzeRepository, getRateLimit } from './services/api';
import type { RepositoryAnalysisDto, RateLimitInfoDto } from './types/api';
import { AlertCircle, RefreshCw } from 'lucide-react';

export function App() {
  const [activeTab, setActiveTab] = useState<'analytics' | 'comparison' | 'user' | 'architecture'>('analytics');
  const [query, setQuery] = useState('dotnet/aspnetcore');
  const [analysis, setAnalysis] = useState<RepositoryAnalysisDto | null>(null);
  const [rateLimit, setRateLimit] = useState<RateLimitInfoDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchAnalysis = async (targetQuery: string) => {
    setIsLoading(true);
    setError(null);
    try {
      // Clean query if full URL or owner/repo
      const cleaned = targetQuery.replace(/^https?:\/\/github\.com\//i, '').replace(/\.git$/i, '').trim().replace(/\/$/, '');
      const parts = cleaned.split('/');
      if (parts.length < 2 || !parts[0] || !parts[1]) {
        throw new Error(`Invalid format '${targetQuery}'. Please provide in 'owner/repo' format.`);
      }

      const [owner, repo] = parts;
      const data = await analyzeRepository(owner, repo);
      setAnalysis(data);
      setQuery(`${owner}/${repo}`);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to retrieve repository data.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    // Initial fetch
    fetchAnalysis('dotnet/aspnetcore');
    getRateLimit().then(setRateLimit);
  }, []);

  const handleSelectRepoFromUser = (repoQuery: string) => {
    setActiveTab('analytics');
    fetchAnalysis(repoQuery);
  };

  return (
    <div className="app-container">
      <Navbar activeTab={activeTab} onTabChange={setActiveTab} rateLimit={rateLimit} />

      {activeTab === 'analytics' && (
        <>
          <SearchBar onSearch={fetchAnalysis} isLoading={isLoading} initialQuery={query} />

          {error && (
            <div className="error-banner">
              <AlertCircle size={20} color="#f43f5e" />
              <div style={{ flex: 1 }}>
                <strong>Analysis Failed:</strong> {error}
              </div>
              <button
                type="button"
                className="nav-tab-btn"
                style={{ padding: '0.25rem 0.75rem', background: 'rgba(255,255,255,0.1)' }}
                onClick={() => fetchAnalysis(query)}
              >
                <RefreshCw size={14} />
                <span>Retry</span>
              </button>
            </div>
          )}

          {isLoading && !analysis && (
            <div className="state-container">
              <div className="spinner" />
              <p style={{ color: '#94a3b8', fontSize: '1rem' }}>Extracting commit graphs and repository telemetry...</p>
            </div>
          )}

          {analysis && (
            <div style={{ opacity: isLoading ? 0.6 : 1, transition: 'opacity 0.2s ease' }}>
              <MetricCards analysis={analysis} />
              <ActivityCharts activityTrend={analysis.activityTrend} languages={analysis.languages} />
              <CommitAndPRViews
                commits={analysis.recentCommits}
                pullRequests={analysis.pullRequests}
                issues={analysis.issues}
              />
            </div>
          )}
        </>
      )}

      {activeTab === 'comparison' && <ComparisonView />}

      {activeTab === 'user' && <UserExplorerView onSelectRepoForAnalysis={handleSelectRepoFromUser} />}

      {activeTab === 'architecture' && <ArchitectureView />}

      {/* Footer */}
      <footer
        style={{
          marginTop: '4rem',
          paddingTop: '2rem',
          borderTop: '1px solid var(--border-subtle)',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '1rem',
          color: '#64748b',
          fontSize: '0.85rem',
        }}
      >
        <div>
          <span>DevPulse &bull; Full-Stack GitHub Analytics Platform</span>
        </div>
        <div style={{ display: 'flex', gap: '1.5rem' }}>
          <span>ASP.NET Core 9.0 Web API</span>
          <span>&bull;</span>
          <span>React 19 &bull; Vite &bull; TypeScript</span>
          <span>&bull;</span>
          <span>Clean Architecture</span>
        </div>
      </footer>
    </div>
  );
}

export default App;
