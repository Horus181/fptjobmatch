using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "JobSeeker,Employer,Admin")]
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var query = _context.Jobs
                .Where(j => j.IsApproved);

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

        [Authorize(Roles = "Employer,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> Create(Job job)
        {
            if (!ModelState.IsValid)
                return View(job);

            job.IsApproved = false;

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            TempData["JobPendingMessage"] =
                "Your job has been submitted and is waiting for admin approval.";

            return RedirectToAction(nameof(Index));
        }
    }
}
