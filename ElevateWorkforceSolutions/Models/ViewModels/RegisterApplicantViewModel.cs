using System.ComponentModel.DataAnnotations;

namespace ElevateWorkforceSolutions.Models.ViewModels;

/// <summary>ViewModel for applicant registration.</summary>
public class RegisterApplicantViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(150)]
    public string? Headline { get; set; }
}
