using ElevateWorkforceSolutions.Models.Entities;

namespace ElevateWorkforceSolutions.Models.ViewModels;

/// <summary>ViewModel for the job details page.</summary>
public class JobDetailsViewModel
{
    /// <summary>The job being viewed.</summary>
    public Job Job { get; set; } = null!;

    /// <summary>Whether the current user is the owning company (controls edit/delete visibility).</summary>
    public bool IsOwner { get; set; }

    /// <summary>Whether the current applicant has already applied (controls "Apply" button visibility).</summary>
    public bool HasApplied { get; set; }
}
