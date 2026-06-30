using Microsoft.AspNetCore.Identity;

namespace ElevateWorkforceSolutions.Models.Entities;

/// <summary>
/// Concrete Identity user. Used by ASP.NET Core Identity for authentication only.
/// Domain concerns are handled by the abstract <see cref="User"/> / <see cref="Company"/> / <see cref="Applicant"/> hierarchy.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Display name (mirrored in the domain <see cref="User.FullName"/> for simplicity).</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>"Company" or "Applicant". Used for role assignment.</summary>
    public string UserType { get; set; } = string.Empty;
}
