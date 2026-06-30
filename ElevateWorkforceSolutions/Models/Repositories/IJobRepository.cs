using ElevateWorkforceSolutions.Models.Entities;

namespace ElevateWorkforceSolutions.Models.Repositories;

/// <summary>Specialised repository interface for <see cref="Job"/>-specific queries.</summary>
public interface IJobRepository : IRepository<Job>
{
    /// <summary>
    /// Returns a paged, filtered set of active jobs and the total matching count.
    /// Filters by search term (title/description) and location when provided.
    /// Results ordered by <see cref="Job.PostedDate"/> descending.
    /// </summary>
    Task<(IEnumerable<Job> Jobs, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? location);

    /// <summary>Returns all jobs (including inactive) for a given company.</summary>
    Task<IEnumerable<Job>> GetByCompanyIdAsync(int companyId);
}
