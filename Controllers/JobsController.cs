using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication2.Controllers
{

    [Authorize(Roles = "JobSeeker,Employer,Admin")]

    public class JobsController : Controller
    {
        
        private static readonly List<Job> _jobs = new();

        
        public IActionResult Index(string searchString)
        {
            var jobs = _jobs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                jobs = jobs.Where(j =>
                    (!string.IsNullOrEmpty(j.Title) && j.Title.Contains(searchString)) ||
                    (!string.IsNullOrEmpty(j.Location) && j.Location.Contains(searchString)));
            }

            return View(jobs.OrderByDescending(j => j.Deadline).ToList());
        }

       
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var job = _jobs.FirstOrDefault(j => j.Id == id);
            if (job == null) return NotFound();

            return View(job);
        }

        
        public IActionResult Create()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Job job)
        {
            if (ModelState.IsValid)
            {
              
                job.Id = _jobs.Count == 0 ? 1 : _jobs.Max(j => j.Id) + 1;

                _jobs.Add(job);
                return RedirectToAction(nameof(Index));
            }

            return View(job);
        }

       
    }
}
