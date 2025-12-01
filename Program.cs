using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
<<<<<<< HEAD
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
=======
    
    options.SignIn.RequireConfirmedAccount = false;
>>>>>>> 8e9c4430fd1b35a356932943293ae4595894d249
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

<<<<<<< HEAD
=======

>>>>>>> 8e9c4430fd1b35a356932943293ae4595894d249
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
<<<<<<< HEAD
    await SeedDataAsync(scope.ServiceProvider);
=======
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Employer", "JobSeeker" };

    foreach (var role in roles)
    {
        var exists = roleManager.RoleExistsAsync(role)
                                .GetAwaiter()
                                .GetResult();

        if (!exists)
        {
            roleManager.CreateAsync(new IdentityRole(role))
                       .GetAwaiter()
                       .GetResult();
        }
    }

   
    string adminEmail = "admin@fptjobmatch.com";
    string adminPassword = "Admin@123";

    var adminUser = userManager.FindByEmailAsync(adminEmail)
                               .GetAwaiter()
                               .GetResult();

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = userManager.CreateAsync(adminUser, adminPassword)
                                      .GetAwaiter()
                                      .GetResult();

        if (createResult.Succeeded)
        {
            userManager.AddToRoleAsync(adminUser, "Admin")
                       .GetAwaiter()
                       .GetResult();
        }
    }
>>>>>>> 8e9c4430fd1b35a356932943293ae4595894d249
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

<<<<<<< HEAD
=======

>>>>>>> 8e9c4430fd1b35a356932943293ae4595894d249
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
<<<<<<< HEAD

static async Task SeedDataAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    var roles = new[] { "Admin", "Employer", "JobSeeker" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    const string adminEmail = "admin@fptjobmatch.com";
    const string adminPassword = "Admin@123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
=======
>>>>>>> 8e9c4430fd1b35a356932943293ae4595894d249
