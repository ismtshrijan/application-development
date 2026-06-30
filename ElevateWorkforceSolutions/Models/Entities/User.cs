namespace ElevateWorkforceSolutions.Models.Entities;

/// <summary>
/// Abstract domain base class for <see cref="Company"/> and <see cref="Applicant"/>.
/// This is deliberately separate from <see cref="ApplicationUser"/> (which extends IdentityUser)
/// so that Identity's auth concern stays decoupled from the domain model.
/// A foreign-key link (<see cref="ApplicationUserId"/> → ApplicationUser.Id) connects them.
/// </summary>
public abstract class User
{
    public int Id { get; set; }

    /// <summary>FK to <see cref="ApplicationUser.Id"/>.</summary>
    public string ApplicationUserId { get; set; } = string.Empty;

    /// <summary>Display name (max 100 characters).</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Account creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Returns a role-specific summary string. Polymorphism evidence point.</summary>
    public abstract string GetDashboardSummary();
}
