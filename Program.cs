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
    
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();


builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
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


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
