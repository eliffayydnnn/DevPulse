# DevPulse — Full-Stack GitHub Analytics SaaS Platform

[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19.x-blue.svg)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-blue.svg)](https://www.typescriptlang.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

DevPulse is a full-stack developer analytics platform that empowers engineering teams, tech leads, and developers to analyze public GitHub repositories, inspect commit trends, track issue/PR velocity, and compare multi-repo performance through an interactive, modern SaaS dashboard.

---

## 🏛 Architecture Overview

DevPulse is structured following Clean Architecture principles with strict separation of concerns:

```
DevPulse/
├── src/
│   ├── DevPulse.Domain/             # Entities, Value Objects, Domain Exceptions & Enums
│   ├── DevPulse.Application/        # Use Cases, DTOs, Service Interfaces, Validation
│   ├── DevPulse.Infrastructure/     # GitHub API Client, EF Core DbContext, Caching
│   └── DevPulse.Api/                # ASP.NET Core 9 Web API, Middleware, Controllers, Swagger
├── frontend/                        # React + TypeScript + Vite + Recharts + Lucide UI
├── tests/
│   ├── DevPulse.UnitTests/          # Unit tests with xUnit, FluentAssertions & Moq
│   └── DevPulse.IntegrationTests/   # API Integration tests with WebApplicationFactory
├── .github/workflows/               # GitHub Actions CI/CD pipelines
├── docker-compose.yml               # Multi-container orchestration (API, Client, SQL Server)
└── README.md
```

### Layer Responsibilities
- **DevPulse.Domain**: Pure business rules, core domain models (`RepositoryAnalysis`, `User`, `SavedRepository`), zero external infrastructure dependencies.
- **DevPulse.Application**: Application business logic, contract interfaces (`IGitHubService`, `IAnalyticsService`), DTO mappings, and workflow orchestration.
- **DevPulse.Infrastructure**: Technical implementations including typed `HttpClientFactory` GitHub integration with rate-limit handling, EF Core with SQL Server (`SQLEXPRESS`), and memory/distributed caching.
- **DevPulse.Api**: RESTful API presentation layer with centralized problem details exception handling, input validation, Swagger/OpenAPI documentation, and CORS configuration.
- **Frontend**: High-performance React SPA built with Vite and TypeScript, featuring dark-mode SaaS aesthetics, interactive analytics graphs, and resilient loading/error states.

---

## 🛠 Technology Stack

### Backend
- **Framework**: .NET 9.0 (C# 13)
- **Web API**: ASP.NET Core with Controllers & OpenAPI / Swagger
- **Persistence**: Entity Framework Core 9.0 (SQL Server / SQLEXPRESS)
- **HTTP Client**: Typed `HttpClientFactory` with Polly resilience (timeout, backoff retry)
- **Caching**: In-Memory Cache with Redis abstraction ready
- **Testing**: xUnit, Moq, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`

### Frontend
- **Framework**: React 19 + TypeScript
- **Build Tool**: Vite
- **Data Visualization**: Recharts
- **Iconography**: Lucide React
- **Styling**: Vanilla CSS Design System with dark glassmorphic SaaS theme

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js (v20+)](https://nodejs.org/) & npm
- [SQL Server](https://www.microsoft.com/sql-server) or SQL Server Express (detected running locally)

### 1. Clone & Setup
```bash
git clone https://github.com/<your-username>/DevPulse.git
cd DevPulse
```

### 2. Backend API
```bash
# Restore and run API
cd src/DevPulse.Api
dotnet run
```
API Swagger UI will be accessible at: `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`).

### 3. Frontend Dashboard
```bash
cd frontend
npm install
npm run dev
```
Frontend will be accessible at: `http://localhost:5173`.

### 4. Running Tests
```bash
# Run all backend unit & integration tests
dotnet test DevPulse.sln
```

---

## 📋 Development Roadmap

- [x] **Stage 1**: Environment analysis, solution scaffolding, Clean Architecture setup, Git initialization.
- [ ] **Stage 2**: Core backend foundational services, centralized error handling (`ProblemDetails`), Swagger configuration, and domain models.
- [ ] **Stage 3**: Resilient GitHub API integration with typed client, rate-limit header parsing, and pagination support.
- [ ] **Stage 4**: Repository analytics engine (commit velocity, PR/issue metrics, multi-repo comparison).
- [ ] **Stage 5**: SaaS React dashboard with live search, interactive charts, and comparison views.
- [ ] **Stage 6**: User persistence (EF Core SQL Server, saved analyses, favorite repositories).
- [ ] **Stage 7**: Unit & integration test suites, caching, performance benchmarking.
- [ ] **Stage 8**: Dockerization, GitHub Actions CI/CD workflows, and portfolio documentation.

---

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
