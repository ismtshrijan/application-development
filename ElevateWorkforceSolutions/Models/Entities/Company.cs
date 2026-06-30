namespace ElevateWorkforceSolutions.Models.Entities;

/// <summary>Represents a company that posts jobs. Inherits from <see cref="User"/>.</summary>
public class Company : User
{
    /// <summary>Company display name (max 150 characters).</summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>Industry sector (max 100 characters).</summary>
    public string Industry { get; set; } = string.Empty;

    /// <summary>Optional company description (max 2000 characters).</summary>
    public string? Description { get; set; }

    /// <summary>Optional company website URL.</summary>
    public string? Website { get; set; }

    /// <summary>Navigation property: jobs posted by this company.</summary>
    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    /// <inheritdoc />
    public override string GetDashboardSummary() => $"{CompanyName} — {Jobs.Count} job(s) posted";
}
