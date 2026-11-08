
using DhakaRent.DataAccess.Context;
using DhakaRent.DataAccess.Identity;
using DhakaRent.Web.UI.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. ASP.NET Core Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. MVC Services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. Seed Roles and Admin Account
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    // Create default roles first
    await RoleSeeder.SeedRolesAsync(roleManager);
}

// Create Admin account and assign Admin role
await AdminSeeder.SeedAsync(app.Services, app.Configuration);

// 5. HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Default MVC Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
