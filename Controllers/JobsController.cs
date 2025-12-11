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

        // -------------------------------------------------------
        //  JOB LIST
        // -------------------------------------------------------
        public async Task<IActionResult> Index(string? searchString)
        {
            var query = _context.Jobs.Where(j => j.IsApproved);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(j =>
                    j.Title.Contains(searchString) ||
                    j.Location.Contains(searchString) ||
                    j.Category.Contains(searchString) ||
                    j.EmployerName.Contains(searchString));
            }

            var jobs = await query.OrderByDescending(j => j.Deadline).ToListAsync();
            return View(jobs);
        }

        // -------------------------------------------------------
        //  JOB DETAILS
        // -------------------------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.IsApproved);
            if (job == null) return NotFound();

            return View(job);
        }

        // -------------------------------------------------------
        //  EMPLOYER CREATE JOB
        // -------------------------------------------------------
        [Authorize(Roles = "Employer,Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> Create(Job job)
        {
            if (!ModelState.IsValid)
                return View(job);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            job.EmployerId = currentUser.Id;
            job.EmployerName = currentUser.Email ?? currentUser.UserName ?? "";
            job.IsApproved = false;

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            TempData["JobPendingMessage"] = "Your job has been submitted and is waiting for admin approval.";
            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------------
        //  APPLY FOR A JOB
        // -------------------------------------------------------
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply(int id)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.IsApproved);
            if (job == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            return View(new JobApplication
            {
                JobId = job.Id,
                ApplicantName = user?.UserName ?? "",
                ApplicantEmail = user?.Email ?? ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply(JobApplication application)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == application.JobId && j.IsApproved);
            if (job == null) return NotFound();

            if (!ModelState.IsValid)
                return View(application);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var newApp = new JobApplication
            {
                JobId = job.Id,
                JobSeekerId = user.Id,
                ApplicantName = application.ApplicantName,
                ApplicantEmail = application.ApplicantEmail,
                Introduction = application.Introduction,
                AppliedAt = DateTime.UtcNow,
                Status = ApplicationStatus.Pending,
                IsViewedByEmployer = false  // Employer chưa xem
            };

            _context.JobApplications.Add(newApp);
            await _context.SaveChangesAsync();

            TempData["AppliedMessage"] = "You have successfully submitted your application!";

            // → Quay về danh sách job thay vì Details
            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------------
        //  EMPLOYER VIEW APPLICATIONS FOR THEIR JOB POSTS
        // -------------------------------------------------------
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> MyApplications()
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (employerId == null) return Challenge();

            var apps = await _context.JobApplications
                .Include(a => a.Job)
                .Where(a => a.Job != null && a.Job.EmployerId == employerId)
                .OrderBy(a => a.IsViewedByEmployer)         // hồ sơ chưa xem hiển thị trước
                .ThenByDescending(a => a.AppliedAt)
                .ToListAsync();

            // Đánh dấu tất cả hồ sơ chưa xem thành đã xem
            foreach (var app in apps.Where(a => !a.IsViewedByEmployer))
            {
                app.IsViewedByEmployer = true;
            }

            await _context.SaveChangesAsync();

            return View(apps);
        }
    }
}
