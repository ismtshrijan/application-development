using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ElevateWorkforceSolutions.Data;
using ElevateWorkforceSolutions.Models.Entities;
using ElevateWorkforceSolutions.Models.Repositories;
using ElevateWorkforceSolutions.Models.ViewModels;

namespace ElevateWorkforceSolutions.Controllers;

/// <summary>Handles registration, login, logout, and access denied.</summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IRepository<Company> _companyRepo;
    private readonly IRepository<Applicant> _applicantRepo;

    /// <summary>Initializes a new instance of <see cref="AccountController"/>.</summary>
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IRepository<Company> companyRepo,
        IRepository<Applicant> applicantRepo)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _companyRepo = companyRepo;
        _applicantRepo = applicantRepo;
    }

    /// <summary>Shows the company registration form.</summary>
    [HttpGet]
    public IActionResult RegisterCompany()
    {
        return View();
    }

    /// <summary>Processes company registration.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterCompany(RegisterCompanyViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var appUser = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.CompanyName,
            UserType = "Company"
        };

        var createResult = await _userManager.CreateAsync(appUser, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        if (!await _roleManager.RoleExistsAsync("Company"))
            await _roleManager.CreateAsync(new IdentityRole("Company"));

        await _userManager.AddToRoleAsync(appUser, "Company");

        var company = new Company
        {
            ApplicationUserId = appUser.Id,
            FullName = model.CompanyName,
            CompanyName = model.CompanyName,
            Industry = model.Industry,
            Description = model.Description,
            Website = model.Website,
            CreatedAt = DateTime.UtcNow
        };
        await _companyRepo.AddAsync(company);
        await _companyRepo.SaveChangesAsync();

        await _signInManager.SignInAsync(appUser, isPersistent: false);
        return RedirectToAction("Index", "Job");
    }

    /// <summary>Shows the applicant registration form.</summary>
    [HttpGet]
    public IActionResult RegisterApplicant()
    {
        return View();
    }

    /// <summary>Processes applicant registration.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterApplicant(RegisterApplicantViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var appUser = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            UserType = "Applicant"
        };

        var createResult = await _userManager.CreateAsync(appUser, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        if (!await _roleManager.RoleExistsAsync("Applicant"))
            await _roleManager.CreateAsync(new IdentityRole("Applicant"));

        await _userManager.AddToRoleAsync(appUser, "Applicant");

        var applicant = new Applicant
        {
            ApplicationUserId = appUser.Id,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Headline = model.Headline,
            CreatedAt = DateTime.UtcNow
        };
        await _applicantRepo.AddAsync(applicant);
        await _applicantRepo.SaveChangesAsync();

        await _signInManager.SignInAsync(appUser, isPersistent: false);
        return RedirectToAction("Index", "Job");
    }

    /// <summary>Shows the login form.</summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var model = new LoginViewModel { ReturnUrl = returnUrl };
        return View(model);
    }

    /// <summary>Processes login.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index", "Job");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    /// <summary>Logs the user out.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Shows access denied page.</summary>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
