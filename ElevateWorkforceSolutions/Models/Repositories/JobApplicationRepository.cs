using Microsoft.EntityFrameworkCore;
using ElevateWorkforceSolutions.Data;
using ElevateWorkforceSolutions.Models.Entities;

namespace ElevateWorkforceSolutions.Models.Repositories;

/// <summary>Job-application repository with job-, applicant-, and duplicate-check queries.</summary>
public class JobApplicationRepository : Repository<JobApplication>, IJobApplicationRepository
{
    /// <summary>Initializes a new instance of <see cref="JobApplicationRepository"/>.</summary>
    public JobApplicationRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IEnumerable<JobApplication>> GetByJobIdAsync(int jobId)
    {
        return await Context.JobApplications
            .Include(a => a.Applicant)
            .Include(a => a.Job)
            .Where(a => a.JobId == jobId)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<JobApplication>> GetByApplicantIdAsync(int applicantId)
    {
        return await Context.JobApplications
            .Include(a => a.Job)
                .ThenInclude(j => j.Company)
            .Include(a => a.Applicant)
            .Where(a => a.ApplicantId == applicantId)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> HasAppliedAsync(int jobId, int applicantId)
    {
        return await Context.JobApplications
            .AnyAsync(a => a.JobId == jobId && a.ApplicantId == applicantId);
    }
}
