# Clinic Appointment System — API

A solo backend portfolio project (.NET 10 / ASP.NET Core) demonstrating clean
architecture with CQRS, RBAC with JWT, concurrency-safe appointment booking,
event-driven notifications, background jobs, and production-grade security
hardening.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Language | C# / .NET 10 |
| Framework | ASP.NET Core Web API (Minimal + Controllers) |
| Architecture | Clean Architecture — `Domain` / `Application` / `Infrastructure` / `API` |
| CQRS | MediatR 14 (commands/queries/events), FluentValidation 12 |
| Database | SQL Server + EF Core 10 (Code-First migrations, single source of truth) |
| Auth | ASP.NET Core Identity + JWT Bearer with rotating refresh tokens |
| Background jobs | Hangfire 1.8 (recurring jobs, admin-protected dashboard) |
| Logging | Serilog (Console + rolling file) |
| Observability / ops | `/health` health checks, response compression, `ApiVersioning` |
| Abuse protection | Fixed-window rate limiter on auth endpoints |
| API docs | Swashbuckle (Swagger UI, dev only) |
| Testing | xUnit, Moq, FluentAssertions, EF Core InMemory |

## Structure

Package-by-feature over a classic clean-architecture layout. `Clinic.Domain`
holds plain entities with zero dependencies. `Clinic.Application` implements
every use case as a MediatR handler in `Features/<Area>/{Commands|Queries}/`,
owning its own DTOs, validators and events. `Clinic.Infrastructure` contains
EF Core + Identity + Hangfire wiring, the JWT generator, and cross-cutting
services (email, SMS, current-user, cleanup jobs). `Clinic.API` exposes the
controllers, middleware and startup configuration. `Clinic.Tests` covers the
handlers with in-memory integration tests.

Feature areas: `Admin`, `Appointments`, `Auth`, `Clinics`, `Doctors`,
`Notifications`, `Patients`, `Specializations`.

## Features

- **Auth & RBAC** — register / login with JWT; three roles (Admin / Doctor /
  Patient) enforced with `[Authorize(Roles = ...)]`; access tokens + rotating
  refresh tokens.
- **Appointments** — create, reschedule, cancel, confirm, complete, mark
  no-show, and rate after completion; big-picture admin view with filters.
- **Concurrency-safe booking** — a slot is never double-booked: the app-level
  check is backed by a filtered unique DB index on
  `(DoctorId, AppointmentDateTime)`; the race survivor gets the legacy 409.
- **Doctor working model** — per-doctor weekly working hours with
  configurable slot duration, unavailability days, and slot-alignment math
  (slots must start exactly at a working-hour boundary).
- **Ownership checks (anti-IDOR)** — patients and doctors can only read /
  mutate their own records; admin bypass; every cross-entity mutation is
  scoped to the token owner.
- **Notifications** — appointment created/confirmed events fan out to the
  doctor via in-app notifications + email + SMS services.
- **Departments & clinics** — admin-managed departments that group
  specializations, plus a "clinic day" availability query.
- **Admin dashboard** — daily overview with waiting-list and stats.
- **Audit logging** — MediatR behavior records mutating operations to
  `AuditLog`.
- **Background jobs** — hourly cancellation of stale pending appointments and
  daily cleanup of expired refresh tokens (Hangfire).
- **Development seeding** — roles, a default admin, and sample clinics/doctors
  are seeded only in the Development environment.

## Security Hardening

- **JWT secret is never committed as a real value** — `appsettings.json` holds
  a placeholder and startup fails fast unless a real 32+ char secret is
  provided via `JwtSettings__Secret` (environment variable) or user-secrets.
  Repo-committed "dev-only" secrets are automatically rejected outside the
  Development environment.
- **Refresh tokens are stored as SHA-256 hashes** and rotated on every use, so
  a stolen database cannot be used to forge sessions and a used token cannot
  be replayed.
- **Semantically correct status codes** — business errors map to `404`, `403`,
  `409`, or `400` via an `ErrorType` on `Result<T>`, instead of blanket 400s.
- **CORS from configuration** — allowed origins are read from
  `Cors:AllowedOrigins` instead of being hardcoded to localhost.
- **Hangfire dashboard is Admin-only** — `/hangfire` requires a Bearer token
  with the Admin role, from a dashboard auth filter.
- **Precise conflict handling** — only genuine unique-constraint violations are
  surfaced as "slot already booked"; other DB failures propagate as 500s.

## Getting Started

Prerequisites: .NET 10 SDK and a local SQL Server instance.

```bash
# Development runs out of the box (dev-only JWT secret is in appsettings.Development.json)
dotnet build Clinic.API/Clinic.API.csproj
dotnet run --project Clinic.API
```

- App: `http://localhost:5136`
- Swagger UI: `http://localhost:5136/swagger` (Development only)
- Health check: `http://localhost:5136/health`
- EF Core creates/migrates the database from the committed migrations on first
  run against the local `ClinicDb` (Windows auth connection string in
  `appsettings.json`).

### Configuration

| Key | Default | Description |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | `Server=.;Database=ClinicDb;Trusted_Connection=True` | SQL Server connection string (override for remote/Linux hosting) |
| `JwtSettings__Secret` | (required) | 32+ char signing key. **Production must override; startup fails otherwise** |
| `JwtSettings__ExpiryMinutes` | `60` | Access token lifetime |
| `Cors__AllowedOrigins__0` | `http://localhost:5173` | Comma-separated origins allowed to call the API |

## Running the Tests

```bash
dotnet test Clinic.Tests/Clinic.Tests.csproj
```

The handler integration tests boot an isolated in-memory EF Core context per
test — no database or external services needed. Coverage includes happy-path
booking, doctor/patient-not-found, outside-working-hours, invalid slot
alignment, duplicate slot rejection, event publication, and specialization
creation.

## Key Things to Point to in Interviews

- **Social Login-style security review done before shipping** — the API went
  through a security audit that fixed a full-auth bypass (JWT secret committed
  to the repo), IDOR in three appointment endpoints, and a raw refresh-token
  store. Ownership checks, token hashing + rotation, and fail-fast secret
  validation are all in place (`Clinic.Application/Features/Auth/...`,
  `Clinic.Infrastructure/DependencyInjection.cs`).
- **Concurrency-safe booking** — `AppointmentConfiguration` adds a filtered
  unique index so two requests for the same slot cannot both succeed; the
  application check is only an optimization, correctness comes from the
  database. A `DbUpdateException` filter (`DbExceptionHelper`) distinguishes a
  real duplicate-key race from unrelated DB failures.
- **Goal: fail-safe coordinates there and then** — `Result<T>` + `ErrorType`
  flows from the handler up to a controller extension (`ResultExtensions`)
  that produces the correct HTTP status, so the API contract stays honest.
- **Refresh token rotation done right** — tokens are hashed at rest (SHA-256),
  issued one-time, and revoked on use; the SPA persists the rotated token so
  sessions survive the 401-refresh cycle.
- **Explicit trade-offs documented** — dev-only JWT secret is committed for
  "clone and run", but the guard rejects it outside Development; a CORS
  allow-list is config-driven; Hangfire is locked to Admin.

## Status

Feature-complete for the planned scope: auth/RBAC, appointment lifecycle with
concurrency guarantees, doctor working model, departments/clinics,
notifications, admin dashboard, audit logging, background jobs, rate limiting,
CORS, health checks, and security hardening.

## License

Distributed under the [MIT License](LICENSE).