import { describe, it, expect } from 'vitest';
import { analyzeRepository, compareRepositories } from '../services/api';

describe('Frontend API Services', () => {
  it('analyzeRepository should return complete analysis data structure', async () => {
    const result = await analyzeRepository('dotnet', 'aspnetcore');

    expect(result).toBeDefined();
    expect(result.repository.owner).toBe('dotnet');
    expect(result.repository.name).toBe('aspnetcore');
    expect(result.metrics.healthScore).toBeGreaterThanOrEqual(70);
    expect(result.metrics.activityStatus).toBe('Very Active');
    expect(result.activityTrend.length).toBe(6);
    expect(result.languages.length).toBeGreaterThan(0);
  });

  it('compareRepositories should generate comparison matrix with winner highlights', async () => {
    const result = await compareRepositories(['dotnet/aspnetcore', 'facebook/react']);

    expect(result).toBeDefined();
    expect(result.repositories.length).toBe(2);
    expect(result.comparisonMatrix.length).toBeGreaterThan(0);
    expect(result.overallVerdict).toContain('leads');
  });
});
