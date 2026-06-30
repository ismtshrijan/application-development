using ElevateWorkforceSolutions.Models.Entities;

namespace ElevateWorkforceSolutions.Models.ViewModels;

/// <summary>ViewModel for the public job listing page with search and pagination.</summary>
public class JobListViewModel
{
    /// <summary>Paginated list of jobs for the current page.</summary>
    public PaginatedList<Job> Jobs { get; set; } = null!;

    /// <summary>Current search term.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>Current location filter.</summary>
    public string? Location { get; set; }
}
