# Elevate Workforce Solutions — Job Portal

An ASP.NET Core MVC job portal built with .NET 10, EF Core 10, SQLite, and ASP.NET Core Identity.

## How to run

```bash
cd ElevateWorkforceSolutions
dotnet restore
dotnet ef database update
dotnet run
```

The app listens on `http://localhost:5000` (or the port shown in the console).

## Demo accounts

| Role      | Email                        | Password       |
|-----------|------------------------------|----------------|
| Company   | hr@techcorp.test             | Password123!   |
| Company   | careers@greenfields.test     | Password123!   |
| Applicant | applicant1@test.com          | Password123!   |
| Applicant | applicant2@test.com          | Password123!   |
| Applicant | applicant3@test.com          | Password123!   |

## Folder-to-MVC-layer mapping

| Folder         | MVC Layer   | Purpose                                                    |
|----------------|-------------|------------------------------------------------------------|
| `Controllers/` | Controller  | Handles HTTP requests, auth, validation, and repository calls |
| `Models/`      | Model       | Domain entities, ViewModels, repository interfaces/implementations |
| `Views/`       | View        | Razor `.cshtml` templates for UI rendering                 |
| `Data/`        | Data        | `ApplicationDbContext` (EF Core) and `DbSeeder`            |
| `wwwroot/`     | Static      | CSS, JS, client-side libraries                             |

## Design decisions

- **Abstract `User` / `Company` / `Applicant` hierarchy**: The domain model uses a three-class inheritance hierarchy with an abstract `GetDashboardSummary()` method, providing clear polymorphism evidence. This is separate from `ApplicationUser` (which extends `IdentityUser`) to keep Identity's auth concern decoupled from domain logic.
- **Soft delete for jobs**: `Job.IsActive` is set to `false` instead of hard-deleting rows, so historical `JobApplication` records remain valid.
- **Repository pattern**: All data access goes through `IJobRepository` / `IJobApplicationRepository` / `IRepository<T>`, injected via constructor DI. Controllers never instantiate `ApplicationDbContext` directly.
- **No raw SQL**: All queries use EF Core LINQ, eliminating SQL injection by construction.
- **Resume upload**: Not implemented as a file upload in this version. `Applicant.ResumeUrl` accepts an external link (plain text field) as a documented simplification.

## Known limitations

- Views are scaffolded separately on a companion branch and may not be present in this branch.
- No SSL/HTTPS certificate is configured for local development; the app runs on HTTP only.
- Resume file upload is replaced by a plain-text URL field (see design decisions above).
