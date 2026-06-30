using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ElevateWorkforceSolutions.Models.Entities;

namespace ElevateWorkforceSolutions.Data;

/// <summary>Seeds the database with demo roles, users, companies, applicants, jobs, and applications.</summary>
public static class DbSeeder
{
    /// <summary>Applies pending migrations and seeds demo data if the database is empty.</summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        if (await roleManager.RoleExistsAsync("Company"))
            return;

        await roleManager.CreateAsync(new IdentityRole("Company"));
        await roleManager.CreateAsync(new IdentityRole("Applicant"));

        // --- Demo Companies ---
        var (company1User, company1) = await CreateCompanyAsync(userManager, context,
            "hr@techcorp.test", "Password123!", "TechCorp Solutions", "Software", "Leading software development company.", "https://techcorp.test");

        var (company2User, company2) = await CreateCompanyAsync(userManager, context,
            "careers@greenfields.test", "Password123!", "Greenfields Agro", "Agriculture", "Sustainable farming solutions provider.", "https://greenfields.test");

        // --- Demo Applicants ---
        var (app1User, app1) = await CreateApplicantAsync(userManager, context,
            "applicant1@test.com", "Password123!", "Alice Johnson", "555-0101", "Senior .NET Developer");
        var (app2User, app2) = await CreateApplicantAsync(userManager, context,
            "applicant2@test.com", "Password123!", "Bob Smith", "555-0102", "Frontend Engineer");
        var (app3User, app3) = await CreateApplicantAsync(userManager, context,
            "applicant3@test.com", "Password123!", "Carol Davis", "555-0103", "DevOps Engineer");

        // --- Demo Jobs ---
        var jobs = new List<Job>
        {
            new()
            {
                CompanyId = company1.Id, Title = "Senior Software Engineer",
                Description = "We are looking for an experienced software engineer to join our platform team. You will design, build, and maintain scalable microservices that power our core product.",
                Location = "San Francisco, CA", EmploymentType = "Full-time",
                SalaryMin = 120000, SalaryMax = 180000,
                PostedDate = DateTime.UtcNow.AddDays(-10), ClosingDate = DateTime.UtcNow.AddDays(50),
                IsActive = true
            },
            new()
            {
                CompanyId = company1.Id, Title = "Junior .NET Developer",
                Description = "Great opportunity for early-career developers. You will work on internal tools and customer-facing features under the mentorship of senior engineers.",
                Location = "Remote", EmploymentType = "Full-time",
                SalaryMin = 60000, SalaryMax = 85000,
                PostedDate = DateTime.UtcNow.AddDays(-5), ClosingDate = DateTime.UtcNow.AddDays(55),
                IsActive = true
            },
            new()
            {
                CompanyId = company1.Id, Title = "QA Intern",
                Description = "Paid internship for students interested in software quality assurance. Learn test automation, manual testing, and CI/CD pipelines.",
                Location = "San Francisco, CA", EmploymentType = "Internship",
                PostedDate = DateTime.UtcNow.AddDays(-3), ClosingDate = DateTime.UtcNow.AddDays(40),
                IsActive = true
            },
            new()
            {
                CompanyId = company1.Id, Title = "Product Designer (closed)",
                Description = "This position has been filled. We were looking for a product designer with experience in B2B SaaS platforms and design systems.",
                Location = "San Francisco, CA", EmploymentType = "Full-time",
                PostedDate = DateTime.UtcNow.AddDays(-60), ClosingDate = DateTime.UtcNow.AddDays(-10),
                IsActive = false
            },
            new()
            {
                CompanyId = company2.Id, Title = "Agricultural Data Scientist",
                Description = "Analyze crop yield data, build predictive models, and help farmers make data-driven decisions about planting and irrigation.",
                Location = "Kansas City, KS", EmploymentType = "Full-time",
                SalaryMin = 90000, SalaryMax = 130000,
                PostedDate = DateTime.UtcNow.AddDays(-7), ClosingDate = DateTime.UtcNow.AddDays(53),
                IsActive = true
            },
            new()
            {
                CompanyId = company2.Id, Title = "Farm Operations Manager",
                Description = "Oversee daily operations across multiple partner farms. Coordinate logistics, manage budgets, and ensure compliance with sustainability standards.",
                Location = "Kansas City, KS", EmploymentType = "Full-time",
                SalaryMin = 75000, SalaryMax = 95000,
                PostedDate = DateTime.UtcNow.AddDays(-4), ClosingDate = DateTime.UtcNow.AddDays(56),
                IsActive = true
            },
            new()
            {
                CompanyId = company2.Id, Title = "Part-time Agronomist Consultant",
                Description = "Provide expert advice on soil health, pest management, and crop rotation to our network of smallholder farms. Flexible hours.",
                Location = "Remote", EmploymentType = "Part-time",
                SalaryMin = 50000, SalaryMax = 70000,
                PostedDate = DateTime.UtcNow.AddDays(-2), ClosingDate = DateTime.UtcNow.AddDays(58),
                IsActive = true
            },
            new()
            {
                CompanyId = company2.Id, Title = "Sustainability Report Writer",
                Description = "Research and write comprehensive sustainability reports for stakeholders. Requires strong writing skills and knowledge of ESG frameworks.",
                Location = "Remote", EmploymentType = "Contract",
                PostedDate = DateTime.UtcNow.AddDays(-1), ClosingDate = DateTime.UtcNow.AddDays(59),
                IsActive = true
            }
        };

        context.Jobs.AddRange(jobs);
        await context.SaveChangesAsync();

        // --- Demo Job Applications ---
        var applications = new List<JobApplication>
        {
            new()
            {
                JobId = jobs[0].Id, ApplicantId = app1.Id,
                CoverLetter = "I have 8 years of experience building scalable microservices and am very excited about TechCorp's product direction.",
                Status = ApplicationStatus.Submitted, AppliedDate = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                JobId = jobs[1].Id, ApplicantId = app2.Id,
                CoverLetter = "I completed a .NET bootcamp and have built several personal projects. I am eager to grow as a developer.",
                Status = ApplicationStatus.UnderReview, AppliedDate = DateTime.UtcNow.AddDays(-4)
            },
            new()
            {
                JobId = jobs[0].Id, ApplicantId = app3.Id,
                CoverLetter = "My background in site reliability engineering and .NET would allow me to contribute immediately.",
                Status = ApplicationStatus.Shortlisted, AppliedDate = DateTime.UtcNow.AddDays(-6)
            },
            new()
            {
                JobId = jobs[4].Id, ApplicantId = app1.Id,
                CoverLetter = "I have a PhD in computational biology and experience with agricultural data analysis.",
                Status = ApplicationStatus.Rejected, AppliedDate = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                JobId = jobs[5].Id, ApplicantId = app2.Id,
                CoverLetter = "I have managed operations for a 500-acre farm for three years and understand the challenges firsthand.",
                Status = ApplicationStatus.Submitted, AppliedDate = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                JobId = jobs[0].Id, ApplicantId = app2.Id,
                CoverLetter = "I am a full-stack developer with experience in C# and would love to join TechCorp.",
                Status = ApplicationStatus.Hired, AppliedDate = DateTime.UtcNow.AddDays(-50)
            }
        };

        context.JobApplications.AddRange(applications);
        await context.SaveChangesAsync();
    }

    private static async Task<(ApplicationUser User, Company Company)> CreateCompanyAsync(
        UserManager<ApplicationUser> userManager, ApplicationDbContext context,
        string email, string password, string companyName, string industry, string description, string? website)
    {
        var appUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = companyName,
            UserType = "Company"
        };
        var result = await userManager.CreateAsync(appUser, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to seed company user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(appUser, "Company");

        var company = new Company
        {
            ApplicationUserId = appUser.Id,
            FullName = companyName,
            CompanyName = companyName,
            Industry = industry,
            Description = description,
            Website = website,
            CreatedAt = DateTime.UtcNow
        };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        return (appUser, company);
    }

    private static async Task<(ApplicationUser User, Applicant Applicant)> CreateApplicantAsync(
        UserManager<ApplicationUser> userManager, ApplicationDbContext context,
        string email, string password, string fullName, string? phoneNumber, string? headline)
    {
        var appUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            UserType = "Applicant"
        };
        var result = await userManager.CreateAsync(appUser, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to seed applicant user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(appUser, "Applicant");

        var applicant = new Applicant
        {
            ApplicationUserId = appUser.Id,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Headline = headline,
            CreatedAt = DateTime.UtcNow
        };
        context.Applicants.Add(applicant);
        await context.SaveChangesAsync();

        return (appUser, applicant);
    }
}
