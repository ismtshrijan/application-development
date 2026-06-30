using System.ComponentModel.DataAnnotations;

namespace ElevateWorkforceSolutions.Models.ViewModels;

/// <summary>ViewModel for login.</summary>
public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }

    /// <summary>Optional return URL to redirect to after successful login.</summary>
    public string? ReturnUrl { get; set; }
}
