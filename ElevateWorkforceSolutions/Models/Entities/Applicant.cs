namespace ElevateWorkforceSolutions.Models.Entities;

/// <summary>Represents a job applicant. Inherits from <see cref="User"/>.</summary>
public class Applicant : User
{
    /// <summary>Optional contact number (max 20 characters).</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Optional professional headline (max 150 characters), e.g. "Junior .NET Developer".</summary>
    public string? Headline { get; set; }

    /// <summary>Optional URL or file path to the applicant's resume.</summary>
    public string? ResumeUrl { get; set; }

    /// <summary>Navigation property: applications submitted by this applicant.</summary>
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();

    /// <inheritdoc />
    public override string GetDashboardSummary() => $"{FullName} — {Applications.Count} application(s) submitted";
}
