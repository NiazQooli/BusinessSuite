# BusinessSuite

Full-stack business application with a **.NET 10** Clean Architecture API and an **Angular 21** client.

## Solution structure

```text
BusinessSuite/
├── src/
│   ├── BusinessSuite.API/             # Presentation (controllers, middleware, DI composition)
│   ├── BusinessSuite.Application/     # Use cases (CQRS via MediatR, validators, DTOs)
│   ├── BusinessSuite.Domain/          # Entities, domain exceptions (no infrastructure deps)
│   └── BusinessSuite.Infrastructure/  # EF Core, persistence, external services
├── tests/
│   └── BusinessSuite.UnitTests/
└── client/                            # Angular SPA
    └── src/app/
        ├── core/                      # Services, interceptors, models
        ├── shared/                    # Shared UI building blocks
        ├── features/                  # Feature areas (dashboard, customers)
        └── layout/                    # App shell / navigation
```

### Clean Architecture dependency flow

```text
API → Application → Domain
API → Infrastructure → Application
```

Infrastructure implements Application interfaces. Domain has zero project references.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- SQL Server LocalDB (included with Visual Studio) or SQL Server

## Getting started

### 1. Backend API

```bash
cd BusinessSuite
dotnet restore
dotnet run --project src/BusinessSuite.API --launch-profile https
```

An initial EF Core migration is already included. On first Development run the API applies migrations and seeds sample customers.

- Swagger UI: `https://localhost:7180`
- Health: `https://localhost:7180/health`
- Customers API: `https://localhost:7180/api/v1/customers`

Connection string is in `src/BusinessSuite.API/appsettings.json`.

### 2. Angular client

```bash
cd client
npm install
npm start
```

App: `http://localhost:4200` (proxies `/api` to the .NET API).

## Included practices

| Area | Approach |
|------|----------|
| Architecture | Clean Architecture + feature folders |
| CQRS | MediatR commands/queries |
| Validation | FluentValidation pipeline behavior |
| Persistence | EF Core + SQL Server, soft deletes |
| API | Versioned controllers (`/api/v1`), Swagger, CORS |
| Errors | Global exception middleware → ProblemDetails |
| Logging | Serilog |
| Frontend | Standalone Angular, lazy routes, core/shared/features |
| Tests | xUnit + FluentAssertions sample domain tests |

## Useful commands

```bash
# Build everything (.NET)
dotnet build

# Run unit tests
dotnet test

# Angular production build
cd client && npm run build
```

## Sample feature

`Customers` is a complete vertical slice: Domain entity → Application CQRS handlers → EF configuration → API controller → Angular list/create/delete UI.
