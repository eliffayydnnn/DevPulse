import React from 'react';
import {
  ResponsiveContainer,
  AreaChart,
  Area,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
} from 'recharts';
import { TrendingUp, PieChart as PieIcon } from 'lucide-react';
import type { ActivityDataPointDto, LanguageDistributionDto } from '../types/api';

interface ActivityChartsProps {
  activityTrend: ActivityDataPointDto[];
  languages: LanguageDistributionDto[];
}

export const ActivityCharts: React.FC<ActivityChartsProps> = ({ activityTrend, languages }) => {
  return (
    <div className="charts-grid">
      {/* Activity Timeline Area Chart */}
      <div className="glass-panel chart-card">
        <div className="chart-header">
          <div className="chart-title">
            <TrendingUp size={20} color="#38bdf8" />
            <span>Activity Velocity (Weekly History)</span>
          </div>
          <div style={{ display: 'flex', gap: '1rem', fontSize: '0.8rem', color: '#94a3b8' }}>
            <span style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
              <span style={{ width: 10, height: 10, borderRadius: '50%', background: '#38bdf8' }} /> Commits
            </span>
            <span style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
              <span style={{ width: 10, height: 10, borderRadius: '50%', background: '#818cf8' }} /> Pull Requests
            </span>
            <span style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
              <span style={{ width: 10, height: 10, borderRadius: '50%', background: '#c084fc' }} /> Issues
            </span>
          </div>
        </div>

        <div style={{ width: '100%', height: 280 }}>
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={activityTrend} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
              <defs>
                <linearGradient id="commitGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#38bdf8" stopOpacity={0.4} />
                  <stop offset="95%" stopColor="#38bdf8" stopOpacity={0.0} />
                </linearGradient>
                <linearGradient id="prGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#818cf8" stopOpacity={0.35} />
                  <stop offset="95%" stopColor="#818cf8" stopOpacity={0.0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.05)" />
              <XAxis dataKey="periodLabel" stroke="#64748b" fontSize={11} tickLine={false} />
              <YAxis stroke="#64748b" fontSize={11} tickLine={false} />
              <Tooltip
                contentStyle={{
                  backgroundColor: '#0f172a',
                  border: '1px solid rgba(255,255,255,0.1)',
                  borderRadius: '8px',
                  boxShadow: '0 8px 24px rgba(0,0,0,0.5)',
                  fontSize: '12px',
                  color: '#f8fafc',
                }}
              />
              <Area
                type="monotone"
                dataKey="commits"
                name="Commits"
                stroke="#38bdf8"
                strokeWidth={2.5}
                fillOpacity={1}
                fill="url(#commitGrad)"
              />
              <Area
                type="monotone"
                dataKey="pullRequests"
                name="Pull Requests"
                stroke="#818cf8"
                strokeWidth={2}
                fillOpacity={1}
                fill="url(#prGrad)"
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Language Breakdown */}
      <div className="glass-panel chart-card">
        <div className="chart-header">
          <div className="chart-title">
            <PieIcon size={20} color="#8b5cf6" />
            <span>Languages</span>
          </div>
          <span style={{ fontSize: '0.8rem', color: '#64748b' }}>By code volume</span>
        </div>

        {languages.length === 0 ? (
          <div style={{ padding: '2rem 0', textAlign: 'center', color: '#64748b', fontSize: '0.875rem' }}>
            No programming languages detected.
          </div>
        ) : (
          <div className="language-list">
            {languages.slice(0, 5).map((lang) => (
              <div key={lang.language} className="language-item">
                <div className="language-info">
                  <span style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <span
                      style={{
                        width: 10,
                        height: 10,
                        borderRadius: '50%',
                        backgroundColor: lang.colorHex || '#8b949e',
                      }}
                    />
                    {lang.language}
                  </span>
                  <span style={{ color: '#94a3b8' }}>{lang.percentage}%</span>
                </div>
                <div className="progress-track">
                  <div
                    className="progress-fill"
                    style={{
                      width: `${lang.percentage}%`,
                      backgroundColor: lang.colorHex || '#38bdf8',
                    }}
                  />
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
