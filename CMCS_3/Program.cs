using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CMCS_3.Data;
using CMCS_3.Models;
using CMCS_3.Services;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure Database with EnableRetryOnFailure
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Application Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register Services
builder.Services.AddScoped<IClaimVerificationService, ClaimVerificationService>();
builder.Services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();
builder.Services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database with roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Ensure database is created
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        await SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "Lecturer", "ProgrammeCoordinator", "AcademicManager", "HR" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Create HR admin user
    var adminEmail = "admin@cmcs.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "HR");
        }
    }

    // Create Programme Coordinator
    var coordinatorEmail = "coordinator@cmcs.com";
    var coordinator = await userManager.FindByEmailAsync(coordinatorEmail);

    if (coordinator == null)
    {
        coordinator = new ApplicationUser
        {
            UserName = coordinatorEmail,
            Email = coordinatorEmail,
            FirstName = "Programme",
            LastName = "Coordinator",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(coordinator, "Coord@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(coordinator, "ProgrammeCoordinator");
        }
    }

    // Create Academic Manager
    var managerEmail = "manager@cmcs.com";
    var manager = await userManager.FindByEmailAsync(managerEmail);

    if (manager == null)
    {
        manager = new ApplicationUser
        {
            UserName = managerEmail,
            Email = managerEmail,
            FirstName = "Academic",
            LastName = "Manager",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(manager, "Manager@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(manager, "AcademicManager");
        }
    }
}