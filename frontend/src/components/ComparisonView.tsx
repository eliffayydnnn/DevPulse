import React, { useState } from 'react';
import { GitCompare, Trophy, Plus, X, Loader2, CheckCircle2 } from 'lucide-react';
import type { RepositoryComparisonDto } from '../types/api';
import { compareRepositories } from '../services/api';

export const ComparisonView: React.FC = () => {
  const [repoInputs, setRepoInputs] = useState<string[]>(['dotnet/aspnetcore', 'facebook/react']);
  const [newRepo, setNewRepo] = useState('');
  const [comparison, setComparison] = useState<RepositoryComparisonDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleAddRepo = (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = newRepo.trim();
    if (!trimmed) return;
    if (repoInputs.length >= 4) {
      setError('You can compare a maximum of 4 repositories simultaneously.');
      return;
    }
    if (repoInputs.includes(trimmed)) {
      setError('Repository already added to comparison list.');
      return;
    }
    setRepoInputs([...repoInputs, trimmed]);
    setNewRepo('');
    setError(null);
  };

  const handleRemoveRepo = (repo: string) => {
    if (repoInputs.length <= 2) {
      setError('Minimum of 2 repositories required for comparison.');
      return;
    }
    setRepoInputs(repoInputs.filter((r) => r !== repo));
    setError(null);
  };

  const handleRunComparison = async () => {
    if (repoInputs.length < 2) {
      setError('Select at least 2 repositories.');
      return;
    }
    setIsLoading(true);
    setError(null);
    try {
      const data = await compareRepositories(repoInputs);
      setComparison(data);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Comparison failed.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div style={{ marginTop: '1rem' }}>
      <div className="glass-panel" style={{ padding: '2rem', marginBottom: '2rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '1rem' }}>
          <GitCompare size={24} color="#38bdf8" />
          <h2 style={{ fontSize: '1.5rem', fontWeight: 800 }}>Multi-Repository Benchmarking</h2>
        </div>
        <p style={{ color: '#94a3b8', fontSize: '0.95rem', marginBottom: '1.5rem' }}>
          Compare velocity, community adoption, issue resolution rates, and code health between 2 and 4 open-source repositories.
        </p>

        {/* Selected Repos Tags */}
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.75rem', alignItems: 'center', marginBottom: '1.5rem' }}>
          {repoInputs.map((repo) => (
            <div
              key={repo}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '0.5rem',
                padding: '0.4rem 0.85rem',
                borderRadius: '8px',
                background: 'rgba(56, 189, 248, 0.1)',
                border: '1px solid rgba(56, 189, 248, 0.3)',
                color: '#f8fafc',
                fontSize: '0.9rem',
                fontFamily: 'JetBrains Mono, monospace',
              }}
            >
              <span>{repo}</span>
              <button
                type="button"
                onClick={() => handleRemoveRepo(repo)}
                style={{ background: 'transparent', border: 'none', color: '#94a3b8', cursor: 'pointer', display: 'flex' }}
              >
                <X size={14} />
              </button>
            </div>
          ))}

          {repoInputs.length < 4 && (
            <form onSubmit={handleAddRepo} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <input
                type="text"
                placeholder="Add owner/repo..."
                value={newRepo}
                onChange={(e) => setNewRepo(e.target.value)}
                style={{
                  background: 'rgba(15, 23, 42, 0.6)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: '8px',
                  padding: '0.4rem 0.75rem',
                  color: '#fff',
                  fontSize: '0.85rem',
                  outline: 'none',
                }}
              />
              <button
                type="submit"
                className="nav-tab-btn"
                style={{ padding: '0.4rem 0.75rem', background: 'rgba(255,255,255,0.06)' }}
              >
                <Plus size={14} />
                <span>Add</span>
              </button>
            </form>
          )}
        </div>

        {error && (
          <div className="error-banner" style={{ margin: '1rem 0' }}>
            <span>{error}</span>
          </div>
        )}

        <button
          type="button"
          onClick={handleRunComparison}
          disabled={isLoading || repoInputs.length < 2}
          className="search-submit-btn"
          style={{ width: 'auto', padding: '0.75rem 2rem' }}
        >
          {isLoading ? (
            <>
              <Loader2 size={18} className="spinner" style={{ width: 18, height: 18, margin: 0, borderWidth: 2 }} />
              <span>Benchmarking Repositories...</span>
            </>
          ) : (
            <>
              <GitCompare size={18} />
              <span>Run Comparison Matrix</span>
            </>
          )}
        </button>
      </div>

      {/* Comparison Matrix Display */}
      {comparison && (
        <>
          <div className="verdict-banner">
            <Trophy size={28} color="#f59e0b" style={{ flexShrink: 0 }} />
            <div>
              <div style={{ fontWeight: 700, fontSize: '1rem', color: '#f8fafc', marginBottom: '0.25rem' }}>
                Automated Analytical Verdict
              </div>
              <div style={{ color: '#94a3b8', fontSize: '0.925rem' }}>{comparison.overallVerdict}</div>
            </div>
          </div>

          <div className="glass-panel" style={{ padding: '1.75rem', marginBottom: '3rem' }}>
            <h3 style={{ fontSize: '1.2rem', fontWeight: 700, marginBottom: '1rem' }}>Head-to-Head Comparison Matrix</h3>
            <div className="comparison-table-wrapper">
              <table className="comparison-table">
                <thead>
                  <tr>
                    <th>Metric</th>
                    <th>Category</th>
                    {comparison.repositories.map((r) => (
                      <th key={r.repository.fullName}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                          <span>{r.repository.fullName}</span>
                        </div>
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {comparison.comparisonMatrix.map((row) => (
                    <tr key={row.metricKey}>
                      <td style={{ fontWeight: 600 }}>{row.metricLabel}</td>
                      <td>
                        <span style={{ fontSize: '0.75rem', padding: '0.2rem 0.5rem', borderRadius: 9999, background: 'rgba(255,255,255,0.06)' }}>
                          {row.category}
                        </span>
                      </td>
                      {comparison.repositories.map((r) => {
                        const val = row.formattedValues[r.repository.fullName] || 'N/A';
                        const isWinner = row.winnerRepositoryFullName === r.repository.fullName;
                        return (
                          <td key={r.repository.fullName} className={isWinner ? 'winner-cell' : ''}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem' }}>
                              {isWinner && <CheckCircle2 size={15} color="#10b981" />}
                              <span>{val}</span>
                            </div>
                          </td>
                        );
                      })}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </div>
  );
};
