using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ElevateWorkforceSolutions.Models.Entities;

namespace ElevateWorkforceSolutions.Data;

/// <summary>Application database context. Extends <see cref="IdentityDbContext{TUser}"/> for ASP.NET Core Identity support.</summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>Initializes a new instance of <see cref="ApplicationDbContext"/>.</summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    /// <summary>Companies (domain entities, not Identity-managed).</summary>
    public DbSet<Company> Companies => Set<Company>();

    /// <summary>Applicants (domain entities, not Identity-managed).</summary>
    public DbSet<Applicant> Applicants => Set<Applicant>();

    /// <summary>Job postings.</summary>
    public DbSet<Job> Jobs => Set<Job>();

    /// <summary>Job applications (join table with status).</summary>
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    /// <inheritdoc />
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
