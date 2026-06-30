using System.ComponentModel.DataAnnotations;

namespace ElevateWorkforceSolutions.Models.Entities;

/// <summary>Represents an applicant's application to a job.</summary>
public class JobApplication
{
    public int Id { get; set; }

    /// <summary>FK to <see cref="Entities.Job.Id"/>.</summary>
    public int JobId { get; set; }

    /// <summary>Navigation property.</summary>
    public Job Job { get; set; } = null!;

    /// <summary>FK to <see cref="Entities.Applicant.Id"/>.</summary>
    public int ApplicantId { get; set; }

    /// <summary>Navigation property.</summary>
    public Applicant Applicant { get; set; } = null!;

    /// <summary>Optional cover letter (max 2000 characters).</summary>
    [StringLength(2000)]
    public string? CoverLetter { get; set; }

    /// <summary>Date the application was submitted.</summary>
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Current status of the application.</summary>
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
}
