using System.ComponentModel.DataAnnotations;

namespace ElevateWorkforceSolutions.Models.Entities;

/// <summary>Represents a job posting created by a company.</summary>
public class Job
{
    public int Id { get; set; }

    /// <summary>FK to <see cref="Entities.Company.Id"/>.</summary>
    public int CompanyId { get; set; }

    /// <summary>Navigation property.</summary>
    public Company Company { get; set; } = null!;

    /// <summary>Job title (max 150 characters).</summary>
    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Full job description (50–4000 characters).</summary>
    [Required, StringLength(4000, MinimumLength = 50)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Job location (max 100 characters).</summary>
    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Employment type. Valid values: "Full-time", "Part-time", "Contract", "Internship", "Remote".
    /// </summary>
    [Required, RegularExpression("^(Full-time|Part-time|Contract|Internship|Remote)$")]
    public string EmploymentType { get; set; } = string.Empty;

    /// <summary>Optional minimum salary.</summary>
    [Range(0, double.MaxValue)]
    public decimal? SalaryMin { get; set; }

    /// <summary>Optional maximum salary. Must be ≥ <see cref="SalaryMin"/> if both are set.</summary>
    [Range(0, double.MaxValue)]
    public decimal? SalaryMax { get; set; }

    /// <summary>Date the job was posted.</summary>
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Closing date for applications. Must be a future date at creation time.</summary>
    [Required]
    public DateTime ClosingDate { get; set; }

    /// <summary>
    /// Soft-delete flag. When <c>false</c> the job is considered deleted/closed
    /// but historical application records remain valid.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Navigation property.</summary>
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
