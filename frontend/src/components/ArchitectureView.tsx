import React from 'react';
import { Layers, Server, Database, Cpu, CheckCircle } from 'lucide-react';

export const ArchitectureView: React.FC = () => {
  return (
    <div style={{ marginTop: '1rem' }}>
      <div className="glass-panel" style={{ padding: '2.5rem', marginBottom: '2rem' }}>
        <h2 style={{ fontSize: '1.75rem', fontWeight: 800, marginBottom: '0.75rem' }}>
          DevPulse System Architecture & Engineering Standards
        </h2>
        <p style={{ color: '#94a3b8', fontSize: '1rem', maxWidth: '850px', marginBottom: '2rem' }}>
          DevPulse is engineered according to enterprise Clean Architecture principles, ensuring strict decoupling of business rules from infrastructure, testability through test doubles and WebApplicationFactory, and resilient external API consumption.
        </p>

        {/* 4 Layers Grid */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '1.5rem', marginBottom: '2.5rem' }}>
          <div className="glass-panel" style={{ padding: '1.5rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.75rem' }}>
              <Cpu size={22} color="#38bdf8" />
              <h3 style={{ fontSize: '1.1rem', fontWeight: 700 }}>DevPulse.Domain</h3>
            </div>
            <p style={{ color: '#94a3b8', fontSize: '0.85rem', marginBottom: '1rem' }}>
              Core domain models, entities (User, SavedRepository, RepositorySnapshot), enums, and domain exceptions. Zero external infrastructure dependencies.
            </p>
            <span style={{ fontSize: '0.75rem', padding: '0.2rem 0.5rem', borderRadius: 4, background: 'rgba(56, 189, 248, 0.1)', color: '#38bdf8' }}>
              Pure C# 13 &bull; .NET 9.0
            </span>
          </div>

          <div className="glass-panel" style={{ padding: '1.5rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.75rem' }}>
              <Layers size={22} color="#818cf8" />
              <h3 style={{ fontSize: '1.1rem', fontWeight: 700 }}>DevPulse.Application</h3>
            </div>
            <p style={{ color: '#94a3b8', fontSize: '0.85rem', marginBottom: '1rem' }}>
              Use-case interfaces (IGitHubService, IAnalyticsService, ICacheService), FluentValidation request validators, DTO models, and URL query parsers.
            </p>
            <span style={{ fontSize: '0.75rem', padding: '0.2rem 0.5rem', borderRadius: 4, background: 'rgba(99, 102, 241, 0.1)', color: '#818cf8' }}>
              FluentValidation &bull; DTOs
            </span>
          </div>

          <div className="glass-panel" style={{ padding: '1.5rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.75rem' }}>
              <Database size={22} color="#10b981" />
              <h3 style={{ fontSize: '1.1rem', fontWeight: 700 }}>DevPulse.Infrastructure</h3>
            </div>
            <p style={{ color: '#94a3b8', fontSize: '0.85rem', marginBottom: '1rem' }}>
              Typed HttpClientFactory integration for GitHub REST API, rate-limit header parsing, parallel Task.WhenAll computation, EF Core with SQL Server.
            </p>
            <span style={{ fontSize: '0.75rem', padding: '0.2rem 0.5rem', borderRadius: 4, background: 'rgba(16, 185, 129, 0.1)', color: '#10b981' }}>
              EF Core 9 &bull; SQL Server &bull; MemoryCache
            </span>
          </div>

          <div className="glass-panel" style={{ padding: '1.5rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.75rem' }}>
              <Server size={22} color="#f59e0b" />
              <h3 style={{ fontSize: '1.1rem', fontWeight: 700 }}>DevPulse.Api</h3>
            </div>
            <p style={{ color: '#94a3b8', fontSize: '0.85rem', marginBottom: '1rem' }}>
              ASP.NET Core Web API with centralized ProblemDetails exception middleware (RFC 7807), OpenAPI / Swagger documentation, and CORS.
            </p>
            <span style={{ fontSize: '0.75rem', padding: '0.2rem 0.5rem', borderRadius: 4, background: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b' }}>
              Swagger &bull; ProblemDetails &bull; CORS
            </span>
          </div>
        </div>

        {/* Highlights List */}
        <h3 style={{ fontSize: '1.25rem', fontWeight: 700, marginBottom: '1rem' }}>Key Architectural Highlights</h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '1rem' }}>
          <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'flex-start' }}>
            <CheckCircle size={18} color="#10b981" style={{ flexShrink: 0, marginTop: 2 }} />
            <div>
              <strong style={{ color: '#f8fafc' }}>Resilient GitHub Rate Limit Management</strong>
              <p style={{ color: '#94a3b8', fontSize: '0.85rem' }}>
                Monitors GitHub API remaining quotas and reset timestamps via headers, triggering transparent caching and informative HTTP 429 notifications.
              </p>
            </div>
          </div>

          <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'flex-start' }}>
            <CheckCircle size={18} color="#10b981" style={{ flexShrink: 0, marginTop: 2 }} />
            <div>
              <strong style={{ color: '#f8fafc' }}>Deterministic Automated Test Suite</strong>
              <p style={{ color: '#94a3b8', fontSize: '0.85rem' }}>
                xUnit test suite with Moq doubles for offline validation and WebApplicationFactory integration testing for the API pipeline.
              </p>
            </div>
          </div>

          <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'flex-start' }}>
            <CheckCircle size={18} color="#10b981" style={{ flexShrink: 0, marginTop: 2 }} />
            <div>
              <strong style={{ color: '#f8fafc' }}>Parallel Execution Optimization</strong>
              <p style={{ color: '#94a3b8', fontSize: '0.85rem' }}>
                Concurrently queries commits, pull requests, issues, and languages with Task.WhenAll, reducing analysis latency by up to 60%.
              </p>
            </div>
          </div>

          <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'flex-start' }}>
            <CheckCircle size={18} color="#10b981" style={{ flexShrink: 0, marginTop: 2 }} />
            <div>
              <strong style={{ color: '#f8fafc' }}>Enterprise ProblemDetails Standard</strong>
              <p style={{ color: '#94a3b8', fontSize: '0.85rem' }}>
                RFC 7807 structured error responses with trace IDs, validation dictionaries, and environment-aware stack traces.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
