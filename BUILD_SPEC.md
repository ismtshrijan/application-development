# Build specification: Elevate Workforce Solutions — Job Portal
**Target: ASP.NET Core MVC, .NET 10 (LTS), C# 14, EF Core 10, SQLite**
**Audience: autonomous coding agent. Build the entire application in one pass, end to end, with no clarifying questions. Every decision needed is made below.**

---

## 0. Non-negotiable constraints

1. Strict **MVC** separation. Controllers must never contain EF Core/DbContext code directly — all data access goes through repository classes injected via constructor DI. Views must never contain business logic beyond display formatting and simple loops/conditionals for rendering.
2. Strict **OOP**: use an abstract base class, inheritance, interfaces, and the repository pattern as specified in section 3. Do not flatten the model into a single concrete class with a "Role" enum — this is graded explicitly on the presence of inheritance and polymorphism.
3. Use **EF Core Code-First with Migrations**. Do not hand-write SQL DDL. Generate the database from the model via `dotnet ef migrations add InitialCreate` and `dotnet ef database update`.
4. Use **SQLite** as the database provider (file `jobportal.db`, created in the project root) — zero external setup, runs anywhere `dotnet run` runs. Do not use SQL Server / LocalDB.
5. Use **ASP.NET Core Identity** for authentication, extended with a custom `ApplicationUser` and role-based authorization (`Company`, `Applicant` roles).
6. Passwords must never be stored in plain text — Identity handles hashing automatically; do not bypass this.
7. The project must build and run with just `dotnet restore && dotnet ef database update && dotnet run` from a clean clone. No manual database GUI steps, no missing NuGet packages, no commented-out broken code.
8. Add inline XML doc comments (`/// <summary>`) on all public classes and public methods. This is required documentation evidence for the assignment, not optional style.
9. Include a `README.md` in the project root explaining: how to run the project, the default seeded login credentials (section 8), and a one-paragraph mapping of folders to MVC layers.
10. Do not use scaffolded Identity UI Razor Pages (`dotnet aspnet-codegenerator identity`). Build custom `AccountController` and matching Razor views under `Areas/Identity` is NOT required — keep Identity-related views in `Views/Account/` to keep everything visibly MVC, not Razor Pages.

---

## 1. Solution & project structure

Create a single ASP.NET Core MVC project named `ElevateWorkforceSolutions`.

```
ElevateWorkforceSolutions/
├── ElevateWorkforceSolutions.csproj
├── Program.cs
├── appsettings.json
├── README.md
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs
├── Models/
│   ├── Entities/
│   │   ├── ApplicationUser.cs
│   │   ├── User.cs                  (abstract domain base — see note below)
│   │   ├── Company.cs
│   │   ├── Applicant.cs
│   │   ├── Job.cs
│   │   ├── JobApplication.cs
│   │   └── ApplicationStatus.cs     (enum)
│   ├── Repositories/
│   │   ├── IRepository.cs
│   │   ├── Repository.cs
│   │   ├── IJobRepository.cs
│   │   ├── JobRepository.cs
│   │   ├── IJobApplicationRepository.cs
│   │   └── JobApplicationRepository.cs
│   └── ViewModels/
│       ├── RegisterCompanyViewModel.cs
│       ├── RegisterApplicantViewModel.cs
│       ├── LoginViewModel.cs
│       ├── JobListViewModel.cs
│       ├── JobCreateEditViewModel.cs
│       ├── JobDetailsViewModel.cs
│       ├── ApplyViewModel.cs
│       ├── CompanyDashboardViewModel.cs
│       └── PaginatedList.cs
├── Controllers/
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── JobController.cs
│   ├── ApplicationController.cs
│   └── DashboardController.cs
├── Views/
│   ├── _ViewStart.cshtml
│   ├── _ViewImports.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _LoginPartial.cshtml
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   ├── Home/
│   │   └── Index.cshtml
│   ├── Account/
│   │   ├── RegisterCompany.cshtml
│   │   ├── RegisterApplicant.cshtml
│   │   └── Login.cshtml
│   ├── Job/
│   │   ├── Index.cshtml             (public paginated listing)
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   ├── Application/
│   │   └── Apply.cshtml
│   └── Dashboard/
│       ├── Company.cshtml
│       └── Applicant.cshtml
└── wwwroot/
    └── css/site.css
```

**Note on `User` abstract class vs `ApplicationUser`:** ASP.NET Core Identity requires a concrete (non-abstract) `IdentityUser`-derived class for its internal machinery. To satisfy both Identity's requirement AND the assignment's mandatory OOP/inheritance requirement, use this two-layer design:

- `ApplicationUser : IdentityUser` — concrete, used only by Identity internally for auth (Email, PasswordHash, etc. already provided by `IdentityUser`). Add `FullName` and `UserType` (string: "Company" or "Applicant") here.
- `User` (abstract, plain C# class, NOT tied to Identity) — the actual domain abstraction with `Id`, `FullName`, `CreatedAt`, abstract method `GetDashboardSummary()`. `Company` and `Applicant` inherit from this.
- Link them via a foreign key: `Company.ApplicationUserId` and `Applicant.ApplicationUserId` (string, FK to `ApplicationUser.Id`).

This is the correct, defensible real-world pattern (Identity's auth concern stays separate from the domain model) and gives the agent an unambiguous answer instead of fighting Identity's sealed assumptions. Document this exact rationale in the XML doc comment on the `User` class — it doubles as evidence for the SDD discussion of design decisions.

---

## 2. NuGet packages (exact, for .NET 10)

```
Microsoft.AspNetCore.Identity.EntityFrameworkCore  (10.0.0 or latest 10.x)
Microsoft.EntityFrameworkCore.Sqlite               (10.0.0 or latest 10.x)
Microsoft.EntityFrameworkCore.Design               (10.0.0 or latest 10.x)
Microsoft.AspNetCore.Identity.UI                   (10.0.0 or latest 10.x) — only if needed for password validators; otherwise omit
```

Target framework in the `.csproj`: `<TargetFramework>net10.0</TargetFramework>`. Enable nullable reference types and implicit usings:
```xml
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

---

## 3. Domain model — exact specification

### 3.1 `ApplicationStatus` enum
```csharp
public enum ApplicationStatus
{
    Submitted,
    UnderReview,
    Shortlisted,
    Rejected,
    Hired
}
```

### 3.2 `User` (abstract)
| Member | Type | Notes |
|---|---|---|
| `Id` | `int` | PK, identity |
| `ApplicationUserId` | `string` | FK to `ApplicationUser.Id`, required |
| `FullName` | `string` | required, max 100 chars |
| `CreatedAt` | `DateTime` | default `DateTime.UtcNow` |
| `GetDashboardSummary()` | abstract `string` | overridden differently by `Company` and `Applicant` — this is the polymorphism evidence point |

### 3.3 `Company : User`
| Member | Type | Notes |
|---|---|---|
| `CompanyName` | `string` | required, max 150 |
| `Industry` | `string` | required, max 100 |
| `Description` | `string?` | max 2000, optional |
| `Website` | `string?` | optional, validated as URL if present |
| `Jobs` | `ICollection<Job>` | navigation property |
| `GetDashboardSummary()` override | returns `"{CompanyName} — {Jobs.Count} job(s) posted"` |

### 3.4 `Applicant : User`
| Member | Type | Notes |
|---|---|---|
| `PhoneNumber` | `string?` | optional, max 20 |
| `Headline` | `string?` | max 150, e.g. "Junior .NET Developer" |
| `ResumeUrl` | `string?` | optional — store as a path under `wwwroot/uploads/resumes/` if file upload is implemented (section 6.6), else a plain text/link field |
| `Applications` | `ICollection<JobApplication>` | navigation property |
| `GetDashboardSummary()` override | returns `"{FullName} — {Applications.Count} application(s) submitted"` |

### 3.5 `Job`
| Member | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `CompanyId` | `int` | FK → `Company.Id`, required |
| `Company` | `Company` | navigation |
| `Title` | `string` | required, max 150 |
| `Description` | `string` | required, max 4000 |
| `Location` | `string` | required, max 100 |
| `EmploymentType` | `string` | required — constrain via `[RegularExpression]` or a small enum: `"Full-time"`, `"Part-time"`, `"Contract"`, `"Internship"`, `"Remote"` |
| `SalaryMin` | `decimal?` | optional |
| `SalaryMax` | `decimal?` | optional, must be ≥ `SalaryMin` if both present (validate in controller/ViewModel) |
| `PostedDate` | `DateTime` | default `DateTime.UtcNow` |
| `ClosingDate` | `DateTime` | required, must be a future date at creation time (`[FutureDate]` custom attribute or controller-level check) |
| `IsActive` | `bool` | default `true` — toggled false instead of hard-deleting (soft delete pattern; explain this in the SDD as a design decision) |
| `Applications` | `ICollection<JobApplication>` | navigation |

### 3.6 `JobApplication`
| Member | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `JobId` | `int` | FK → `Job.Id` |
| `Job` | `Job` | navigation |
| `ApplicantId` | `int` | FK → `Applicant.Id` |
| `Applicant` | `Applicant` | navigation |
| `CoverLetter` | `string?` | max 2000, optional |
| `AppliedDate` | `DateTime` | default `DateTime.UtcNow` |
| `Status` | `ApplicationStatus` | default `Submitted` |

**Composite uniqueness rule**: a given `Applicant` may apply to a given `Job` only once. Enforce with a unique index in `OnModelCreating`:
```csharp
modelBuilder.Entity<JobApplication>()
    .HasIndex(a => new { a.JobId, a.ApplicantId })
    .IsUnique();
```
and re-check this in the controller before insert, returning a friendly validation error ("You have already applied to this job") rather than letting a `DbUpdateException` bubble up to the user.

---

## 4. Repository pattern (OOP requirement — implement exactly)

### 4.1 `IRepository<T>` (generic interface)
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
}
```

### 4.2 `Repository<T>` (generic implementation)
Wraps `ApplicationDbContext`, implements all of the above using `DbSet<T>`. Constructor takes `ApplicationDbContext` via DI.

### 4.3 `IJobRepository : IRepository<Job>` (specialised interface — demonstrates interface segregation)
Add job-specific query methods:
```csharp
Task<(IEnumerable<Job> Jobs, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? location);
Task<IEnumerable<Job>> GetByCompanyIdAsync(int companyId);
```

### 4.4 `JobRepository : Repository<Job>, IJobRepository`
Implements the above with `.Include(j => j.Company)`, `.Where()` filters for search/location, `.Skip()/.Take()` for pagination, ordered by `PostedDate descending`. Only return jobs `WHERE IsActive == true` for the public listing method; provide a separate method or parameter to include inactive jobs for the owning company's dashboard.

### 4.5 `IJobApplicationRepository : IRepository<JobApplication>`
```csharp
Task<IEnumerable<JobApplication>> GetByJobIdAsync(int jobId);
Task<IEnumerable<JobApplication>> GetByApplicantIdAsync(int applicantId);
Task<bool> HasAppliedAsync(int jobId, int applicantId);
```

### 4.6 `JobApplicationRepository : Repository<JobApplication>, IJobApplicationRepository`
Implements the above with appropriate `.Include()` calls for `Job` and `Applicant`.

### 4.7 Dependency injection registration (in `Program.cs`)
```csharp
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

Controllers receive `IJobRepository`/`IJobApplicationRepository` via constructor injection — never instantiate `ApplicationDbContext` or repositories with `new` inside a controller.

---

## 5. `ApplicationDbContext`

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobApplication>()
            .HasIndex(a => new { a.JobId, a.ApplicantId })
            .IsUnique();

        modelBuilder.Entity<Job>()
            .HasOne(j => j.Company)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JobApplication>()
            .HasOne(a => a.Job)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JobApplication>()
            .HasOne(a => a.Applicant)
            .WithMany(ap => ap.Applications)
            .HasForeignKey(a => a.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Job>()
            .Property(j => j.SalaryMin).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Job>()
            .Property(j => j.SalaryMax).HasColumnType("decimal(18,2)");
    }
}
```

`Company` and `Applicant` are plain EF entities (not Identity-managed) mapped normally — EF Core will create `Companies` and `Applicants` tables with `ApplicationUserId` as a plain string FK column (no formal foreign key constraint to AspNetUsers is required, but add one if straightforward).

---

## 6. Controllers — exact behaviour specification

### 6.1 `AccountController`
- `GET/POST RegisterCompany` — shows/processes `RegisterCompanyViewModel` (Email, Password, ConfirmPassword, CompanyName, Industry, Description, Website). On POST: create `ApplicationUser` with `UserType = "Company"`, call `UserManager.CreateAsync`, add to `"Company"` role via `RoleManager`/`UserManager.AddToRoleAsync`, then create and save a linked `Company` domain entity, then sign in via `SignInManager.SignInAsync`. Redirect to `Job/Index`.
- `GET/POST RegisterApplicant` — same pattern for `Applicant` (FullName, Email, Password, ConfirmPassword, PhoneNumber, Headline).
- `GET/POST Login` — standard `SignInManager.PasswordSignInAsync`, show validation summary on failure, support `returnUrl`.
- `POST Logout` — `SignInManager.SignOutAsync()`, redirect home.
- `GET AccessDenied` — simple static view for `[Authorize(Roles="...")]` failures.

### 6.2 `JobController`
- `GET Index(int page = 1, string? search, string? location)` — public, no auth required. Calls `IJobRepository.GetPagedAsync(page, pageSize: 6, search, location)`, wraps result + pagination metadata in `JobListViewModel` (current page, total pages, total count, search term, jobs list). View must render a search box, location filter, and Bootstrap-style pagination links (Previous/1/2/3/Next), preserving the search/location query string across page links.
- `GET Details(int id)` — public. 404 (`NotFound()`) if job doesn't exist or `IsActive == false` (unless the requester is the owning company, in which case show it anyway — check via `User.IsInRole("Company")` and ownership).
- `GET Create()` / `POST Create(JobCreateEditViewModel)` — `[Authorize(Roles = "Company")]`. On POST, resolve the logged-in company's `Company.Id` from the current `ApplicationUser.Id` claim, set `CompanyId` server-side (never trust a client-submitted CompanyId), validate `ClosingDate > DateTime.UtcNow` and `SalaryMax >= SalaryMin` if both given, save via `IJobRepository.AddAsync`, redirect to `Dashboard/Company`.
- `GET Edit(int id)` / `POST Edit(int id, JobCreateEditViewModel)` — `[Authorize(Roles = "Company")]`. Must verify the job's `CompanyId` matches the current user's company before allowing the edit (return `Forbid()` / `403` otherwise — this ownership check is a specific, gradeable security requirement, not optional).
- `GET Delete(int id)` / `POST DeleteConfirmed(int id)` — `[Authorize(Roles = "Company")]`, same ownership check. Implement as a **soft delete** (`IsActive = false`), not a hard `DELETE`, so historical `JobApplication` records remain valid. State this design decision explicitly in the README and SDD.

### 6.3 `ApplicationController`
- `GET Apply(int jobId)` — `[Authorize(Roles = "Applicant")]`. Loads job, shows `ApplyViewModel` (JobId, JobTitle, CoverLetter textbox). If `IJobApplicationRepository.HasAppliedAsync` is already true, redirect to `Details` with a TempData message "You have already applied to this job" instead of showing the form.
- `POST Apply(ApplyViewModel)` — re-checks `HasAppliedAsync` server-side (do not rely on the GET-time check alone — race condition / resubmission protection), creates `JobApplication` with `Status = ApplicationStatus.Submitted`, saves, redirects to `Dashboard/Applicant` with a success TempData message.
- `POST UpdateStatus(int applicationId, ApplicationStatus newStatus)` — `[Authorize(Roles = "Company")]`. Verifies the application's job belongs to the current company before allowing the status change. Used from the company dashboard to move applicants through Submitted → UnderReview → Shortlisted/Rejected → Hired.

### 6.4 `DashboardController`
- `GET Company()` — `[Authorize(Roles = "Company")]`. Shows all jobs posted by the logged-in company (including inactive ones, clearly marked "Closed"), and for each job, the list of applicants with their current status and a dropdown/buttons to call `UpdateStatus`.
- `GET Applicant()` — `[Authorize(Roles = "Applicant")]`. Shows all applications the logged-in applicant has submitted, with job title, company name, applied date, and current status.

### 6.5 `HomeController`
- `GET Index()` — public landing page, brief intro + call-to-action buttons linking to `Job/Index`, `Account/RegisterCompany`, `Account/RegisterApplicant`.

### 6.6 File upload (resume) — optional but recommended for Merit-level evidence of "appropriate methods/tools"
If implemented: accept `IFormFile Resume` on `RegisterApplicantViewModel`, validate extension (`.pdf`, `.doc`, `.docx`) and max size (5 MB) server-side, save to `wwwroot/uploads/resumes/{Guid}_{filename}`, store the relative path in `Applicant.ResumeUrl`. If the agent judges this adds excessive scope risk for a same-day build, it may instead implement `ResumeUrl` as a plain text input (a link to an externally hosted resume) — this is an acceptable, documented simplification; note it explicitly in the README "Known limitations" section either way.

---

## 7. Validation rules (apply via Data Annotations on ViewModels, mirrored in entities where sensible)

| Field | Rule |
|---|---|
| Email | `[Required, EmailAddress]` |
| Password | `[Required, StringLength(100, MinimumLength = 8)]`, Identity's default policy (uppercase, lowercase, digit) stays enabled — do not weaken `PasswordOptions` |
| ConfirmPassword | `[Compare("Password")]` |
| Job.Title | `[Required, StringLength(150)]` |
| Job.Description | `[Required, StringLength(4000, MinimumLength = 50)]` — enforce a meaningful minimum so empty/junk postings can't be created |
| Job.ClosingDate | `[Required]` + controller-level check that it is strictly after `DateTime.UtcNow` |
| Job.SalaryMax | controller-level check `SalaryMax >= SalaryMin` when both present |
| JobApplication.CoverLetter | `[StringLength(2000)]`, optional |
| Company.Website | `[Url]` when present |

All forms must display validation errors using `asp-validation-for` tag helpers and `<div asp-validation-summary="All">`, with client-side jQuery validation enabled via `_ValidationScriptsPartial`.

---

## 8. Seed data (`Data/DbSeeder.cs`, called from `Program.cs` at startup)

Create a static `SeedAsync(IServiceProvider services)` method that runs migrations (`context.Database.MigrateAsync()`) then, only if the database is empty, seeds:

1. **Roles**: `"Company"`, `"Applicant"` via `RoleManager<IdentityRole>`.
2. **2 demo companies** with linked `ApplicationUser` + `Company` rows:
   - `hr@techcorp.test` / `Password123!` — TechCorp Solutions, Industry: Software
   - `careers@greenfields.test` / `Password123!` — Greenfields Agro, Industry: Agriculture
3. **3 demo applicants** with linked `ApplicationUser` + `Applicant` rows:
   - `applicant1@test.com` / `Password123!`
   - `applicant2@test.com` / `Password123!`
   - `applicant3@test.com` / `Password123!`
4. **6–8 demo jobs** spread across the two companies, varied `EmploymentType`, `Location`, realistic `ClosingDate` values (set 30–60 days from seed time), at least one with `IsActive = false` to prove the soft-delete/closed-job display path works.
5. **A handful of demo `JobApplication` rows** linking the demo applicants to some of the demo jobs, with varied `Status` values, so the dashboards are non-empty on first run.

List all five seeded login credentials in `README.md` under a "Demo accounts" heading exactly as above — this is what you (the assignment author) will use for screenshots in the Implementation and Testing sections of your report.

---

## 9. Authorization & security checklist (explicit gradeable security requirements)

- [ ] `[Authorize]` / `[Authorize(Roles = "...")]` attributes on every controller action listed in section 6 with a role restriction — do not rely on hiding links in the UI alone.
- [ ] Server-side ownership checks on `Job/Edit`, `Job/Delete`, `Application/UpdateStatus` (a company must not be able to edit another company's job by guessing a URL/id — this is the single most common thing a P5/M4 marker tests by hand).
- [ ] Anti-forgery tokens: `[ValidateAntiForgeryToken]` on every `[HttpPost]` action; `@Html.AntiForgeryToken()` / `asp-antiforgery="true"` (default with tag helpers) on every form.
- [ ] No raw SQL string concatenation anywhere (EF Core LINQ only) — eliminates SQL injection by construction; mention this explicitly in the Evaluation section of the report as a deliberate mitigation.
- [ ] Identity's default password complexity rules left enabled in `Program.cs`'s `AddIdentity` options.
- [ ] `app.UseHttpsRedirection()`, `app.UseAuthentication()` before `app.UseAuthorization()`, in that order, in `Program.cs`.
- [ ] Generic error page (`Views/Shared/Error.cshtml`) shown in production (`app.UseExceptionHandler("/Home/Error")` when not Development) — no raw stack traces exposed to end users.

---

## 10. `Program.cs` — required configuration shape

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=jobportal.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
```

`appsettings.json` must include:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=jobportal.db"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
```

---

## 11. `PaginatedList<T>` helper (`Models/ViewModels/PaginatedList.cs`)

Implement the standard pattern so pagination logic is reusable and OOP-demonstrating rather than copy-pasted Skip/Take in every controller:

```csharp
public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; private set; }
    public int TotalPages { get; private set; }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        AddRange(items);
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}
```

Use this directly in `JobRepository.GetPagedAsync` or in the controller — either is acceptable, but be consistent (prefer doing it in the repository, keeping the controller thin).

---

## 12. Views — minimum content requirements (Bootstrap 5, included via CDN link in `_Layout.cshtml`)

- `_Layout.cshtml`: navbar with conditional links — Home, Browse Jobs always visible; Post a Job (Company only); My Applications (Applicant only); Login/Register when anonymous; Logout + welcome name when authenticated. Use `_LoginPartial.cshtml` for the auth-state section, following the standard ASP.NET Identity convention.
- `Job/Index.cshtml`: search form (GET, `asp-action="Index"` with `search`/`location` query params preserved), a card or table per job showing Title, Company name, Location, EmploymentType, salary range (formatted, e.g. "$50,000–$70,000" or "Not specified"), a "View details" link, and pagination controls at the bottom.
- `Job/Details.cshtml`: full job description, company name + industry, closing date, an "Apply now" button (Applicant role only, hidden/disabled if already applied or job inactive/closed), Edit/Delete buttons (Company owner only).
- `Dashboard/Company.cshtml`: table of own jobs (including closed ones, with a "Closed" badge) each expandable/linked to its list of applicants and status-update controls.
- `Dashboard/Applicant.cshtml`: table of own applications — Job title, Company, Applied date, Status (as a colored badge: Submitted=gray, UnderReview=blue, Shortlisted=amber, Hired=green, Rejected=red).

Keep styling simple and clean (Bootstrap defaults + the provided `site.css`) — visual design is not what this assignment grades; functional correctness and architecture are.

---

## 13. Testing expectations (for the agent to leave in a testable state — actual test writing/execution is the student's section 8 of the report, not strictly required from the agent, but appreciated if time allows)

If time allows, add a minimal `ElevateWorkforceSolutions.Tests` xUnit project with:
- One unit test proving `JobApplicationRepository.HasAppliedAsync` returns true after an application is added and false before.
- One unit test proving `PaginatedList<T>.TotalPages` calculates correctly for a known count/pageSize.
- One unit test proving a `Company`'s `GetDashboardSummary()` override returns the expected string (polymorphism evidence).

This is explicitly lower priority than sections 1–12 — do not let it block a working main application.

---

## 14. Definition of done (acceptance checklist — the agent should self-verify against this before finishing)

- [ ] `dotnet build` succeeds with zero errors, zero warnings related to nullability misuse.
- [ ] `dotnet ef database update` creates `jobportal.db` with all five tables (`AspNetUsers` + Identity tables, `Companies`, `Applicants`, `Jobs`, `JobApplications`) and the unique index from section 3.6.
- [ ] App starts with `dotnet run`, home page loads.
- [ ] Can register a new Company account and a new Applicant account; both can log in/out.
- [ ] Logged-in Company can create, edit, and (soft) delete a job; cannot edit/delete another company's job (returns 403/Forbid when attempted directly via URL).
- [ ] Public job listing shows pagination, search by keyword, and filter by location, all working together via query string.
- [ ] Logged-in Applicant can view a job and apply once; a second attempt is blocked with a friendly message, not a server error.
- [ ] Company dashboard shows applicants per job and can change an application's status; Applicant dashboard reflects that status change.
- [ ] Seed data from section 8 is present on first run; credentials match exactly what's documented in `README.md`.
- [ ] No `DbContext` or repository is ever instantiated with `new` inside a controller — constructor injection only.
- [ ] All five demo login credentials work as documented.

---

## 15. What NOT to do

- Do not use Razor Pages for the core job/application features — Controllers + Views only (Identity's own internal scaffolding can stay Razor Pages style internally if using `AddDefaultIdentity`, but this spec uses custom `AccountController` instead specifically to keep everything visibly MVC for the assignment's grading criteria).
- Do not store the Identity `ApplicationUser` as the only model — the abstract `User`/`Company`/`Applicant` domain layer is mandatory, not optional, even though it adds a join.
- Do not hard-delete jobs or applications.
- Do not trust any client-submitted `CompanyId`, `ApplicantId`, or `Status` value without a server-side ownership/role check.
- Do not skip migrations and call `EnsureCreated()` instead — migrations are required as evidence of a proper Code-First workflow for the report.