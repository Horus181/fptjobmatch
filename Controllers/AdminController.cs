using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }



        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<AdminUserViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                model.Add(new AdminUserViewModel
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
<<<<<<< HEAD
                    Roles = string.Join(", ", roles),
                    EmailConfirmed = u.EmailConfirmed
=======
                    Roles = string.Join(", ", roles)
>>>>>>> 8e9c4430fd1b35a356932943293ae4595894d249
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> EditUserRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allRoles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                Roles = allRoles.Select(r => new RoleSelection
                {
                    RoleName = r.Name!,
                    Selected = userRoles.Contains(r.Name!)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRoles(EditUserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

        
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

          
            var selectedRoles = model.Roles.Where(r => r.Selected).Select(r => r.RoleName);
            await _userManager.AddToRolesAsync(user, selectedRoles);

            return RedirectToAction(nameof(Users));
        }

       

        public async Task<IActionResult> CreateUser()
        {
            var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            var model = new CreateUserViewModel
            {
                AvailableRoles = allRoles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }

                model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(model);
            }

           
            if (!string.IsNullOrEmpty(model.SelectedRole))
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }

            return RedirectToAction(nameof(Users));
        }

<<<<<<< HEAD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Users");
        }


=======
     
>>>>>>> 8e9c4430fd1b35a356932943293ae4595894d249

        public async Task<IActionResult> Jobs()
        {
           
            var jobs = await _context.Jobs.ToListAsync();
            return View(jobs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Jobs));
        }
    }

    public class AdminUserViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string Roles { get; set; } = "";
<<<<<<< HEAD

        public bool EmailConfirmed { get; set; }
=======
>>>>>>> 8e9c4430fd1b35a356932943293ae4595894d249
    }

    public class RoleSelection
    {
        public string RoleName { get; set; } = "";
        public bool Selected { get; set; }
    }

    public class EditUserRolesViewModel
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public List<RoleSelection> Roles { get; set; } = new();
    }

    public class CreateUserViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        public string SelectedRole { get; set; } = "";

        public List<string> AvailableRoles { get; set; } = new();
    }
}
