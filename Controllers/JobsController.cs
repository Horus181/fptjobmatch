using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "JobSeeker,Employer,Admin")]
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var query = _context.Jobs.AsQueryable();

            query = query.Where(j => j.IsApproved);

            if (User.IsInRole("JobSeeker"))
            {
                query = query.Where(j => !j.IsClosed);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                query = query.Where(j =>
                    (j.Title != null && j.Title.Contains(searchString)) ||
                    (j.Location != null && j.Location.Contains(searchString)) ||
                    (j.EmployerName != null && j.EmployerName.Contains(searchString)) ||
                    (j.Category != null && j.Category.Contains(searchString)));
            }

            var jobs = await query
                .OrderByDescending(j => j.Deadline)
                .ToListAsync();

            return View(jobs);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id && j.IsApproved);

            if (job == null) return NotFound();

            return View(job);
        }

        [Authorize(Roles = "Employer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Create(Job job)
        {
            if (!ModelState.IsValid)
                return View(job);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            job.EmployerId = currentUser.Id;
            job.EmployerName = currentUser.Email ?? currentUser.UserName ?? string.Empty;
            job.IsApproved = false;
            job.IsClosed = false;
            job.ClosedAt = null;

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            TempData["JobPendingMessage"] =
                "Your job has been submitted and is waiting for admin approval.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply(int id)
        {
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id && j.IsApproved && !j.IsClosed);

            if (job == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            var model = new JobApplication
            {
                JobId = job.Id,
                ApplicantName = currentUser?.UserName ?? string.Empty,
                ApplicantEmail = currentUser?.Email ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply(JobApplication application)
        {
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == application.JobId && j.IsApproved && !j.IsClosed);

            if (job == null) return NotFound();

            if (!ModelState.IsValid)
                return View(application);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var newApplication = new JobApplication
            {
                JobId = job.Id,
                JobSeekerId = currentUser.Id,
                ApplicantName = application.ApplicantName,
                ApplicantEmail = application.ApplicantEmail,
                Introduction = application.Introduction,
                AppliedAt = DateTime.UtcNow,
                Status = ApplicationStatus.Pending,
                IsViewedByEmployer = false
            };

            _context.JobApplications.Add(newApplication);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your application has been submitted successfully.";
            return RedirectToAction(nameof(Details), new { id = job.Id });
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> MyApplications()
        {
            var jobSeekerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(jobSeekerId)) return Challenge();

            var apps = await _context.JobApplications
                .Include(a => a.Job)
                .Where(a => a.JobSeekerId == jobSeekerId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(apps);
        }

        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> ApplicationsForMyJobs()
        {
            var employerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(employerId)) return Challenge();

            var apps = await _context.JobApplications
                .Include(a => a.Job)
                .Where(a => a.Job != null && a.Job.EmployerId == employerId)
                .OrderBy(a => a.Status)
                .ThenBy(a => a.IsViewedByEmployer)
                .ThenByDescending(a => a.AppliedAt)
                .ToListAsync();

            var unseen = apps.Where(a => !a.IsViewedByEmployer).ToList();
            if (unseen.Any())
            {
                foreach (var a in unseen)
                    a.IsViewedByEmployer = true;

                await _context.SaveChangesAsync();
            }

            return View(apps);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> ApproveApplication(int id)
        {
            var employerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(employerId)) return Challenge();

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();
            if (application.Job == null || application.Job.EmployerId != employerId)
                return Forbid();

            application.Status = ApplicationStatus.Approved;
            await _context.SaveChangesAsync();

            TempData["EmployerMessage"] = "Application has been approved.";
            return RedirectToAction(nameof(ApplicationsForMyJobs));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> RejectApplication(int id)
        {
            var employerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(employerId)) return Challenge();

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();
            if (application.Job == null || application.Job.EmployerId != employerId)
                return Forbid();

            application.Status = ApplicationStatus.Rejected;
            await _context.SaveChangesAsync();

            TempData["EmployerMessage"] = "Application has been rejected.";
            return RedirectToAction(nameof(ApplicationsForMyJobs));
        }

        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> ApplicationDetails(int id)
        {
            var employerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(employerId)) return Challenge();

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();
            if (application.Job == null || application.Job.EmployerId != employerId)
                return Forbid();

            return View(application);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> CloseJob(int id)
        {
            var employerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(employerId)) return Challenge();

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
            if (job == null) return NotFound();
            if (job.EmployerId != employerId) return Forbid();

            job.IsClosed = true;
            job.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["EmployerMessage"] = "Job has been closed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> ReopenJob(int id)
        {
            var employerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(employerId)) return Challenge();

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
            if (job == null) return NotFound();
            if (job.EmployerId != employerId) return Forbid();

            job.IsClosed = false;
            job.ClosedAt = null;

            await _context.SaveChangesAsync();

            TempData["EmployerMessage"] = "Job has been reopened.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
