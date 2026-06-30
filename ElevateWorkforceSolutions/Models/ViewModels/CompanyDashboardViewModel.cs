using ElevateWorkforceSolutions.Models.Entities;

namespace ElevateWorkforceSolutions.Models.ViewModels;

/// <summary>ViewModel for the company dashboard.</summary>
public class CompanyDashboardViewModel
{
    /// <summary>Company name.</summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>Jobs posted by this company (including inactive).</summary>
    public List<Job> Jobs { get; set; } = new();
}
