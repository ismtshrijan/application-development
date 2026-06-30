using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ElevateWorkforceSolutions.Models.Entities;
using ElevateWorkforceSolutions.Models.Repositories;
using ElevateWorkforceSolutions.Models.ViewModels;

namespace ElevateWorkforceSolutions.Controllers;

/// <summary>Company and applicant dashboards.</summary>
public class DashboardController : Controller
{
    private readonly IJobRepository _jobRepo;
    private readonly IJobApplicationRepository _appRepo;
    private readonly IRepository<Company> _companyRepo;
    private readonly IRepository<Applicant> _applicantRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>Initializes a new instance of <see cref="DashboardController"/>.</summary>
    public DashboardController(
        IJobRepository jobRepo,
        IJobApplicationRepository appRepo,
        IRepository<Company> companyRepo,
        IRepository<Applicant> applicantRepo,
        UserManager<ApplicationUser> userManager)
    {
        _jobRepo = jobRepo;
        _appRepo = appRepo;
        _companyRepo = companyRepo;
        _applicantRepo = applicantRepo;
        _userManager = userManager;
    }

    /// <summary>Company dashboard: shows all jobs (including inactive) with applicant lists and status controls.</summary>
    [HttpGet]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> Company()
    {
        var appUser = await _userManager.GetUserAsync(User);
        if (appUser == null)
            return Forbid();

        var companies = await _companyRepo.GetAllAsync();
        var company = companies.FirstOrDefault(c => c.ApplicationUserId == appUser.Id);
        if (company == null)
            return Forbid();

        var jobs = await _jobRepo.GetByCompanyIdAsync(company.Id);
        var jobsList = jobs.ToList();

        // Load applications per job
        foreach (var job in jobsList)
        {
            job.Applications = (await _appRepo.GetByJobIdAsync(job.Id)).ToList();
        }

        var viewModel = new CompanyDashboardViewModel
        {
            CompanyName = company.CompanyName,
            Jobs = jobsList
        };

        return View(viewModel);
    }

    /// <summary>Applicant dashboard: shows all submitted applications with current status.</summary>
    [HttpGet]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> Applicant()
    {
        var appUser = await _userManager.GetUserAsync(User);
        if (appUser == null)
            return Forbid();

        var applicants = await _applicantRepo.GetAllAsync();
        var applicant = applicants.FirstOrDefault(a => a.ApplicationUserId == appUser.Id);
        if (applicant == null)
            return Forbid();

        var applications = await _appRepo.GetByApplicantIdAsync(applicant.Id);

        return View(applications.ToList());
    }
}
