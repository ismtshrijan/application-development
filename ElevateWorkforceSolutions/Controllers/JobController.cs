using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ElevateWorkforceSolutions.Models.Entities;
using ElevateWorkforceSolutions.Models.Repositories;
using ElevateWorkforceSolutions.Models.ViewModels;

namespace ElevateWorkforceSolutions.Controllers;

/// <summary>Job listing, details, creation, editing, and soft-deletion.</summary>
public class JobController : Controller
{
    private readonly IJobRepository _jobRepo;
    private readonly IRepository<Company> _companyRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>Initializes a new instance of <see cref="JobController"/>.</summary>
    public JobController(
        IJobRepository jobRepo,
        IRepository<Company> companyRepo,
        UserManager<ApplicationUser> userManager)
    {
        _jobRepo = jobRepo;
        _companyRepo = companyRepo;
        _userManager = userManager;
    }

    /// <summary>Public paginated job listing with search and location filter.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, string? search = null, string? location = null)
    {
        const int pageSize = 6;
        var (jobs, totalCount) = await _jobRepo.GetPagedAsync(page, pageSize, search, location);

        var paginatedJobs = new PaginatedList<Job>(jobs.ToList(), totalCount, page, pageSize);
        var viewModel = new JobListViewModel
        {
            Jobs = paginatedJobs,
            SearchTerm = search,
            Location = location
        };

        return View(viewModel);
    }

    /// <summary>Public job details page.</summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job == null)
            return NotFound();

        var isOwner = false;
        var hasApplied = false;

        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Company"))
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser != null)
            {
                var companies = await _companyRepo.GetAllAsync();
                var company = companies.FirstOrDefault(c => c.ApplicationUserId == appUser.Id);
                isOwner = company != null && job.CompanyId == company.Id;
            }
        }

        if (job.IsActive == false && !isOwner)
            return NotFound();

        var viewModel = new JobDetailsViewModel
        {
            Job = job,
            IsOwner = isOwner,
            HasApplied = hasApplied
        };

        return View(viewModel);
    }

    /// <summary>Shows the job creation form (Company only).</summary>
    [HttpGet]
    [Authorize(Roles = "Company")]
    public IActionResult Create()
    {
        return View(new JobCreateEditViewModel());
    }

    /// <summary>Processes job creation (Company only).</summary>
    [HttpPost]
    [Authorize(Roles = "Company")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var appUser = await _userManager.GetUserAsync(User);
        if (appUser == null)
            return Forbid();

        var companies = await _companyRepo.GetAllAsync();
        var company = companies.FirstOrDefault(c => c.ApplicationUserId == appUser.Id);
        if (company == null)
            return Forbid();

        if (model.ClosingDate <= DateTime.UtcNow.Date)
        {
            ModelState.AddModelError(nameof(model.ClosingDate), "Closing date must be a future date.");
            return View(model);
        }

        if (model.SalaryMin.HasValue && model.SalaryMax.HasValue && model.SalaryMax < model.SalaryMin)
        {
            ModelState.AddModelError(nameof(model.SalaryMax), "Maximum salary must be greater than or equal to minimum salary.");
            return View(model);
        }

        var job = new Job
        {
            CompanyId = company.Id,
            Title = model.Title,
            Description = model.Description,
            Location = model.Location,
            EmploymentType = model.EmploymentType,
            SalaryMin = model.SalaryMin,
            SalaryMax = model.SalaryMax,
            ClosingDate = model.ClosingDate,
            PostedDate = DateTime.UtcNow,
            IsActive = true
        };

        await _jobRepo.AddAsync(job);
        await _jobRepo.SaveChangesAsync();

        TempData["SuccessMessage"] = "Job posted successfully.";
        return RedirectToAction("Company", "Dashboard");
    }

    /// <summary>Shows the job edit form (Company owner only).</summary>
    [HttpGet]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> Edit(int id)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job == null)
            return NotFound();

        var appUser = await _userManager.GetUserAsync(User);
        if (appUser == null)
            return Forbid();

        var companies = await _companyRepo.GetAllAsync();
        var company = companies.FirstOrDefault(c => c.ApplicationUserId == appUser.Id);
        if (company == null || job.CompanyId != company.Id)
            return Forbid();

        var viewModel = new JobCreateEditViewModel
        {
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            EmploymentType = job.EmploymentType,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            ClosingDate = job.ClosingDate
        };

        return View(viewModel);
    }

    /// <summary>Processes job edit (Company owner only).</summary>
    [HttpPost]
    [Authorize(Roles = "Company")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, JobCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var job = await _jobRepo.GetByIdAsync(id);
        if (job == null)
            return NotFound();

        var appUser = await _userManager.GetUserAsync(User);
        if (appUser == null)
            return Forbid();

        var companies = await _companyRepo.GetAllAsync();
        var company = companies.FirstOrDefault(c => c.ApplicationUserId == appUser.Id);
        if (company == null || job.CompanyId != company.Id)
            return Forbid();

        if (model.ClosingDate <= DateTime.UtcNow.Date)
        {
            ModelState.AddModelError(nameof(model.ClosingDate), "Closing date must be a future date.");
            return View(model);
        }

        if (model.SalaryMin.HasValue && model.SalaryMax.HasValue && model.SalaryMax < model.SalaryMin)
        {
            ModelState.AddModelError(nameof(model.SalaryMax), "Maximum salary must be greater than or equal to minimum salary.");
            return View(model);
        }

        job.Title = model.Title;
        job.Description = model.Description;
        job.Location = model.Location;
        job.EmploymentType = model.EmploymentType;
        job.SalaryMin = model.SalaryMin;
        job.SalaryMax = model.SalaryMax;
        job.ClosingDate = model.ClosingDate;

        _jobRepo.Update(job);
        await _jobRepo.SaveChangesAsync();

        TempData["SuccessMessage"] = "Job updated successfully.";
        return RedirectToAction("Company", "Dashboard");
    }

    /// <summary>Shows the job deletion confirmation page (Company owner only).</summary>
    [HttpGet]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> Delete(int id)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job == null)
            return NotFound();

        var appUser = await _userManager.GetUserAsync(User);
        if (appUser == null)
            return Forbid();

        var companies = await _companyRepo.GetAllAsync();
        var company = companies.FirstOrDefault(c => c.ApplicationUserId == appUser.Id);
        if (company == null || job.CompanyId != company.Id)
            return Forbid();

        var viewModel = new JobCreateEditViewModel
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            EmploymentType = job.EmploymentType,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            ClosingDate = job.ClosingDate
        };

        return View(viewModel);
    }

    /// <summary>Processes job soft-deletion (Company owner only). Sets IsActive = false.</summary>
    [HttpPost]
    [Authorize(Roles = "Company")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job == null)
            return NotFound();

        var appUser = await _userManager.GetUserAsync(User);
        if (appUser == null)
            return Forbid();

        var companies = await _companyRepo.GetAllAsync();
        var company = companies.FirstOrDefault(c => c.ApplicationUserId == appUser.Id);
        if (company == null || job.CompanyId != company.Id)
            return Forbid();

        job.IsActive = false;
        _jobRepo.Update(job);
        await _jobRepo.SaveChangesAsync();

        TempData["SuccessMessage"] = "Job closed successfully.";
        return RedirectToAction("Company", "Dashboard");
    }
}
