import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MetricCards } from '../components/MetricCards';
import type { RepositoryAnalysisDto } from '../types/api';

const mockAnalysis: RepositoryAnalysisDto = {
  repository: {
    id: 1,
    name: 'aspnetcore',
    fullName: 'dotnet/aspnetcore',
    owner: 'dotnet',
    htmlUrl: 'https://github.com/dotnet/aspnetcore',
    stars: 36000,
    forks: 10000,
    openIssues: 400,
    watchers: 1500,
    primaryLanguage: 'C#',
    license: 'MIT',
    isArchived: false,
    isFork: false,
    createdAt: '2015-01-01',
    updatedAt: '2026-10-27',
  },
  metrics: {
    stars: 36000,
    forks: 10000,
    openIssues: 400,
    totalRecentCommitsEvaluated: 50,
    openPullRequests: 20,
    closedPullRequests: 80,
    openIssuesCount: 400,
    closedIssuesCount: 1600,
    healthScore: 92,
    healthGrade: 'A+',
    daysSinceLastCommit: 1,
    pullRequestMergeRatio: 0.8,
    activityStatus: 'Very Active',
  },
  recentCommits: [],
  pullRequests: [],
  issues: [],
  languages: [],
  activityTrend: [],
  analyzedAtUtc: new Date().toISOString(),
  isFromCache: false,
};

describe('MetricCards Component', () => {
  it('renders repository name, health score and activity status correctly', () => {
    render(<MetricCards analysis={mockAnalysis} />);

    expect(screen.getByText('dotnet/aspnetcore')).toBeDefined();
    expect(screen.getByText('92')).toBeDefined();
    expect(screen.getByText('A+')).toBeDefined();
    expect(screen.getByText('Very Active')).toBeDefined();
    expect(screen.getByText(/36[.,]000/)).toBeDefined();
  });
});
