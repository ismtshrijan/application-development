using System.ComponentModel.DataAnnotations;

namespace ElevateWorkforceSolutions.Models.ViewModels;

/// <summary>ViewModel for submitting a job application.</summary>
public class ApplyViewModel
{
    /// <summary>Job ID (set server-side, not from client).</summary>
    public int JobId { get; set; }

    /// <summary>Job title for display purposes.</summary>
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>Optional cover letter.</summary>
    [StringLength(2000)]
    public string? CoverLetter { get; set; }
}
