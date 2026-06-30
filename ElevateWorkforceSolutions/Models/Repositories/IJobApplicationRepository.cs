using ElevateWorkforceSolutions.Models.Entities;

namespace ElevateWorkforceSolutions.Models.Repositories;

/// <summary>Specialised repository interface for <see cref="JobApplication"/>-specific queries.</summary>
public interface IJobApplicationRepository : IRepository<JobApplication>
{
    /// <summary>Returns all applications for a given job.</summary>
    Task<IEnumerable<JobApplication>> GetByJobIdAsync(int jobId);

    /// <summary>Returns all applications submitted by a given applicant.</summary>
    Task<IEnumerable<JobApplication>> GetByApplicantIdAsync(int applicantId);

    /// <summary>Checks whether a given applicant has already applied to a given job.</summary>
    Task<bool> HasAppliedAsync(int jobId, int applicantId);
}
