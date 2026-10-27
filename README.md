# DevPulse — Full-Stack GitHub Analytics SaaS Platform

[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19.x-blue.svg)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-blue.svg)](https://www.typescriptlang.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![CI/CD](https://github.com/eliffayydnnn/DevPulse/actions/workflows/ci.yml/badge.svg)](https://github.com/eliffayydnnn/DevPulse/actions)

**DevPulse** is an enterprise-grade full-stack developer analytics platform that empowers engineering leaders, open-source maintainers, and developers to audit GitHub repositories, evaluate commit trends, track issue/PR velocity, compare multi-repository performance, and bookmark favorites with JWT authentication and resilient hybrid caching.

---

## 🏛 Architecture & Solution Structure

DevPulse is architected in accordance with **Clean Architecture** and Domain-Driven Design (DDD) principles:

```
DevPulse/
├── src/
│   ├── DevPulse.Domain/             # Pure Entities (User, SavedRepository, RepositorySnapshot), Enums, Domain Exceptions
│   ├── DevPulse.Application/        # Use Cases, DTOs, Service Contracts, FluentValidation, Caching Abstraction
│   ├── DevPulse.Infrastructure/     # EF Core DbContext, Migrations, SQL Server, RedisCacheService, AuthService, GitHub API Client
│   └── DevPulse.Api/                # ASP.NET Core 9 Web API, JWT Bearer Auth, Middleware, Controllers, Swagger
├── frontend/                        # React 19 + TypeScript + Vite + Recharts + Lucide UI SPA
├── tests/
│   ├── DevPulse.UnitTests/          # Unit tests for Domain, Auth, Analytics, Caching, Hasher (xUnit, Moq)
│   └── DevPulse.IntegrationTests/   # End-to-end WebApplicationFactory integration tests with SQL Server & JWT
├── .github/workflows/ci.yml         # Automated GitHub Actions CI workflow for .NET and Frontend
├── Dockerfile.backend               # Multi-stage production container for .NET 9 API
├── Dockerfile.frontend              # Multi-stage production container with Nginx for React SPA
├── docker-compose.yml               # Multi-container orchestration (API, SPA, SQL Server 2022, Redis)
└── README.md
```

### Layer Responsibilities
- **DevPulse.Domain**: Enterprise entities (`User`, `SavedRepository`, `RepositorySnapshot`), pure business invariants, zero infrastructure dependencies.
- **DevPulse.Application**: Application interfaces (`IAuthService`, `IGitHubService`, `IAnalyticsService`, `ICacheService`), data transfer objects, validation logic.
- **DevPulse.Infrastructure**: Technical adapters:
  - **Entity Framework Core 9.0**: SQL Server persistence with code-first migrations.
  - **Authentication**: PBKDF2 cryptographic password hashing (100,000 iterations, 128-bit salt) and HMAC-SHA256 JWT generation.
  - **Hybrid Caching**: `RedisCacheService` with connection resilience and graceful fallback to `IMemoryCache` if Redis is offline.
  - **GitHub API Integration**: Typed `HttpClient` with Polly exponential backoff retry and rate limit monitoring.
- **DevPulse.Api**: RESTful controllers (`AuthController`, `AnalyticsController`, `RepositoriesController`, `SavedRepositoriesController`), centralized ProblemDetails exception handling, Swagger UI with Bearer Auth.
- **Frontend**: React 19 SPA built with Vite, TypeScript, and Vanilla CSS glassmorphic dark design system.

---

## 🛠 Technology Stack

### Backend
- **Framework**: .NET 9.0 (C# 13)
- **Web API**: ASP.NET Core with Controllers & OpenAPI / Swagger
- **Authentication**: JWT Bearer Tokens (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Password Hashing**: PBKDF2 (`Rfc2898DeriveBytes`) with cryptographically secure random salts
- **Persistence**: Entity Framework Core 9.0 (SQL Server / LocalDB / Azure SQL)
- **Caching**: StackExchange.Redis + In-Memory Fallback (`ICacheService`)
- **HTTP Client**: Typed `HttpClientFactory` with Polly resilience
- **Testing**: xUnit, Moq, `Microsoft.AspNetCore.Mvc.Testing`

### Frontend
- **Framework**: React 19 + TypeScript
- **Build Tool**: Vite 8.3
- **Data Visualization**: Recharts (trend charts, velocity graphs, language breakdowns)
- **Icons**: Lucide React
- **Design System**: Vanilla CSS dark-mode glassmorphic interface (zero Tailwind dependency)
- **Testing**: Vitest, React Testing Library, JSDOM

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js (v20+)](https://nodejs.org/) & npm
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, SQL Express, or Docker container)
- *(Optional)* [Redis](https://redis.io/) (if distributed caching is desired; defaults to in-memory if offline)

### 1. Clone Repository
```bash
git clone https://github.com/eliffayydnnn/DevPulse.git
cd DevPulse
```

### 2. Environment Variables & Configuration
You can configure the backend via `appsettings.json` or environment variables:

| Environment Variable | Description | Default / Example |
|---|---|---|
| `ConnectionStrings__DevPulseDb` | SQL Server Connection String | `Server=(localdb)\mssqllocaldb;Database=DevPulseDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;` |
| `ConnectionStrings__Redis` | Redis Connection String | `localhost:6379,abortConnect=false` |
| `Jwt__SecretKey` | JWT Signing Key (>= 32 chars) | `DevPulseSuperSecureKeyForJwtTokens_MustBeAtLeast32BytesLong!` |
| `Jwt__Issuer` | JWT Issuer Claim | `DevPulse` |
| `Jwt__Audience` | JWT Audience Claim | `DevPulseApp` |
| `GitHub__Token` | Optional GitHub Personal Access Token | *(Increases rate limit from 60 to 5,000 req/hr)* |

### 3. Database Migration
Apply the EF Core migrations to create tables (`Users`, `SavedRepositories`, `RepositorySnapshots`, `__EFMigrationsHistory`):

```bash
dotnet ef database update --project src/DevPulse.Infrastructure --startup-project src/DevPulse.Api
```

### 4. Running the Backend API Locally
```bash
cd src/DevPulse.Api
dotnet run
```
- API Base URL: `http://localhost:5000` (or `https://localhost:5001`)
- Swagger Documentation: `http://localhost:5000/swagger`

### 5. Running the Frontend SPA Locally
```bash
cd frontend
npm install
npm run dev
```
- Frontend Dashboard: `http://localhost:5173`

---

## 🐳 Docker Deployment

A multi-container setup is ready via `docker-compose.yml`:

```bash
docker compose up -d --build
```

Services initialized:
- `backend`: ASP.NET Core API on port `5000`
- `frontend`: React SPA on port `3000`
- `sqlserver`: Microsoft SQL Server 2022 on port `1433`
- `redis`: Redis Alpine cache on port `6379`

> **Note on Host Environment:** Local Windows development environments without Docker Desktop running can run the application directly using the LocalDB / In-Memory cache fallback workflow above.

---

## 📡 API Endpoints

### Authentication (`/api/auth`)
- `POST /api/auth/register` — Register a new user (`username`, `email`, `password`)
- `POST /api/auth/login` — Authenticate and receive a JWT Bearer token
- `GET /api/auth/me` — Retrieve current authenticated user profile *(Requires Bearer Token)*

### Analytics (`/api/analytics`)
- `GET /api/analytics/repository/{owner}/{repo}` — Full analysis (commits, PRs, issues, languages, health grade)
- `POST /api/analytics/compare` — Compare multiple repositories side-by-side with matrix scoring

### Repositories (`/api/repositories`)
- `GET /api/repositories/{owner}/{repo}` — Retrieve core metadata for a repository
- `GET /api/repositories/user/{username}` — Retrieve repositories owned by a user
- `POST /api/repositories/parse` — Validate and parse owner/repo query string
- `GET /api/repositories/rate-limit` — Check current GitHub API rate limit quota

### Saved Repositories / Bookmarks (`/api/saved-repositories`) *(Requires Bearer Token)*
- `GET /api/saved-repositories` — List bookmarked repositories for the authenticated user
- `POST /api/saved-repositories` — Save a repository to user favorites (`owner`, `repositoryName`, `notes`)
- `DELETE /api/saved-repositories/{id}` — Remove repository from user favorites

---

## 🧪 Testing

### Backend Test Suite (24 Tests)
```bash
dotnet test DevPulse.sln
```
- **Unit Tests (18)**:
  - Domain entity validation & calculations
  - `PasswordHasherService` PBKDF2 salting and verification
  - `JwtTokenGenerator` claim mapping and expiration
  - Analytics health scoring formulas and rate-limit parsing
- **Integration Tests (6)**:
  - `WebApplicationFactory` API integration
  - User registration & login flows
  - JWT token validation and route authorization guards
  - Saved repositories creation, persistence in test database, and deletion

### Frontend Test Suite (6 Tests)
```bash
npm --prefix frontend run test
```
- **Authentication Service**: Token and user session management in LocalStorage
- **API Services**: Repository analysis and comparison data transformations
- **UI Components**: `MetricCards` rendering and health grade indicators

---

## ⚠️ Known Limitations & Considerations
1. **GitHub API Rate Limits**: Unauthenticated calls are limited by GitHub to 60 requests/hour per IP. Setting the `GITHUB_TOKEN` environment variable raises this limit to 5,000 requests/hour.
2. **Redis Fallback**: If a Redis instance is not reachable, the system logs an informational warning and transparently switches to `IMemoryCache` without crashing or interrupting user requests.
3. **Local Docker CLI**: If Docker Desktop is not active on the development workstation, all services run natively using .NET SDK, Node.js, and SQL Server LocalDB.

---

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
