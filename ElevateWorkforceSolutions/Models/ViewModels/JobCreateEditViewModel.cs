using System.ComponentModel.DataAnnotations;

namespace ElevateWorkforceSolutions.Models.ViewModels;

/// <summary>ViewModel for creating and editing a job.</summary>
public class JobCreateEditViewModel
{
    /// <summary>Job ID (used for edit/delete scenarios).</summary>
    public int Id { get; set; }
    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000, MinimumLength = 50)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Required, RegularExpression("^(Full-time|Part-time|Contract|Internship|Remote)$",
         ErrorMessage = "Employment type must be one of: Full-time, Part-time, Contract, Internship, Remote.")]
    public string EmploymentType { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal? SalaryMin { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? SalaryMax { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Closing Date")]
    public DateTime ClosingDate { get; set; }
}
