using Microsoft.EntityFrameworkCore;
using ElevateWorkforceSolutions.Data;
using ElevateWorkforceSolutions.Models.Entities;

namespace ElevateWorkforceSolutions.Models.Repositories;

/// <summary>Job repository with pagination, search, and company-scoped queries.</summary>
public class JobRepository : Repository<Job>, IJobRepository
{
    /// <summary>Initializes a new instance of <see cref="JobRepository"/>.</summary>
    public JobRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<(IEnumerable<Job> Jobs, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, string? location)
    {
        var query = Context.Jobs
            .Include(j => j.Company)
            .Where(j => j.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(j => j.Title.ToLower().Contains(term)
                                  || j.Description.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var loc = location.ToLower();
            query = query.Where(j => j.Location.ToLower().Contains(loc));
        }

        query = query.OrderByDescending(j => j.PostedDate);

        var totalCount = await query.CountAsync();
        var jobs = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (jobs, totalCount);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Job>> GetByCompanyIdAsync(int companyId)
    {
        return await Context.Jobs
            .Include(j => j.Company)
            .Where(j => j.CompanyId == companyId)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();
    }
}
