using System.ComponentModel.DataAnnotations;

namespace ElevateWorkforceSolutions.Models.ViewModels;

/// <summary>ViewModel for company registration.</summary>
public class RegisterCompanyViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Industry { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Url]
    public string? Website { get; set; }
}
