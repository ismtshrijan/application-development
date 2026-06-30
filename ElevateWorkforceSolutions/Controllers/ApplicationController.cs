using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ElevateWorkforceSolutions.Models.Entities;
using ElevateWorkforceSolutions.Models.Repositories;
using ElevateWorkforceSolutions.Models.ViewModels;

namespace ElevateWorkforceSolutions.Controllers;

/// <summary>Handles job application submission and status updates by companies.</summary>
public class ApplicationController : Controller
{
    private readonly IJobRepository _jobRepo;
    private readonly IJobApplicationRepository _appRepo;
    private readonly IRepository<Applicant> _applicantRepo;
    private readonly IRepository<Company> _companyRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>Initializes a new instance of <see cref="ApplicationController"/>.</summary>
    public ApplicationController(
        IJobRepository jobRepo,
        IJobApplicationRepository appRepo,
        IRepository<Applicant> applicantRepo,
        IRepository<Company> companyRepo,
        UserManager<ApplicationUser> userManager)
    {
        _jobRepo = jobRepo;
        _appRepo = appRepo;
        _applicantRepo = applicantRepo;
        _companyRepo = companyRepo;
        _userManager = userManager;
    }

    /// <summary>Shows the application form (Applicant only).</summary>
    [HttpGet]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> Apply(int jobId)
    {
        var job = await _jobRepo.GetByIdAsync(jobId);
        if (job == null || !job.IsActive)
            return NotFound();

        var appUser = await _userManager.GetUserAsync(User);
        if (appUser == null)
            return Forbid();

        var applicants = await _applicantRepo.GetAllAsync();
        var applicant = applicants.FirstOrDefault(a => a.ApplicationUserId == appUser.Id);
        if (applicant == null)
            return Forbid();

        if (await _appRepo.HasAppliedAsync(jobId, applicant.Id))
        {
            TempData["ErrorMessage"] = "You have already applied to this job.";
            return RedirectToAction("Details", "Job", new { id = jobId });
        }

        var viewModel = new ApplyViewModel
        {
            JobId = job.Id,
            JobTitle = job.Title
        };

        return View(viewModel);
    }

    /// <summary>Processes the application submission (Applicant only).</summary>
    [HttpPost]
    [Authorize(Roles = "Applicant")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ApplyViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var appUser = await _userManager.GetUserAsync(User);
        if (appUser == null)
            return Forbid();

        var applicants = await _applicantRepo.GetAllAsync();
        var applicant = applicants.FirstOrDefault(a => a.ApplicationUserId == appUser.Id);
        if (applicant == null)
            return Forbid();

        // Re-check for duplicate (race-condition / resubmission protection)
        if (await _appRepo.HasAppliedAsync(model.JobId, applicant.Id))
        {
            TempData["ErrorMessage"] = "You have already applied to this job.";
            return RedirectToAction("Details", "Job", new { id = model.JobId });
        }

        var jobApplication = new JobApplication
        {
            JobId = model.JobId,
            ApplicantId = applicant.Id,
            CoverLetter = model.CoverLetter,
            AppliedDate = DateTime.UtcNow,
            Status = ApplicationStatus.Submitted
        };

        await _appRepo.AddAsync(jobApplication);
        await _appRepo.SaveChangesAsync();

        TempData["SuccessMessage"] = "Application submitted successfully.";
        return RedirectToAction("Applicant", "Dashboard");
    }

    /// <summary>Updates an application's status (Company owner only).</summary>
    [HttpPost]
    [Authorize(Roles = "Company")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int applicationId, ApplicationStatus newStatus)
    {
        var appUser = await _userManager.GetUserAsync(User);
        if (appUser == null)
            return Forbid();

        var companies = await _companyRepo.GetAllAsync();
        var company = companies.FirstOrDefault(c => c.ApplicationUserId == appUser.Id);
        if (company == null)
            return Forbid();

        var application = await _appRepo.GetByIdAsync(applicationId);
        if (application == null)
            return NotFound();

        // Verify the application's job belongs to this company
        if (application.JobId == 0) // Force load if needed; but we already have JobId from the entity
        {
            // Reload with job info
        }

        var job = await _jobRepo.GetByIdAsync(application.JobId);
        if (job == null || job.CompanyId != company.Id)
            return Forbid();

        application.Status = newStatus;
        _appRepo.Update(application);
        await _appRepo.SaveChangesAsync();

        TempData["SuccessMessage"] = "Application status updated.";
        return RedirectToAction("Company", "Dashboard");
    }
}
