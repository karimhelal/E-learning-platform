using BLL.DTOs.Account;
using BLL.Helpers;
using BLL.Interfaces;
using BLL.Interfaces.Account;
using BLL.Interfaces.CourseLearning;
using BLL.Interfaces.Enrollment;
using BLL.Interfaces.Admin;
using BLL.Interfaces.Student;
using BLL.Interfaces.Instructor;
using BLL.Services;
using BLL.Services.Account;
using BLL.Services.CourseLearning;
using BLL.Services.Enrollment;
using BLL.Services.Admin;
using BLL.Services.Student;
using BLL.Services.Instructor;
using Core.Entities;
using Core.RepositoryInterfaces;
using DAL.Data;
using DAL.Data.RepositoryServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Hubs;
using Web.Interfaces;
using Web.Services;
using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// REPOSITORIES (Team's layer)
// ========================================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ILessonProgressRepository, LessonProgressRepository>();
builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>(); // ADD THIS
builder.Services.AddScoped<ILessonRepository, LessonRepository>(); // ADD THIS


// Add generic repositories for LessonProgress and CourseEnrollment
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Add generic repositories for LessonProgress and CourseEnrollment
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


// Current User Service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<INotifier, SignalRNotifier>();

// ========================================
// BLL SERVICES (Team's layer)
// ========================================
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseLearningService, CourseLearningService>();
builder.Services.AddScoped<IInstructorCoursesService, InstructorCoursesService>();
builder.Services.AddScoped<IInstructorProfileService, InstructorProfileService>();
builder.Services.AddScoped<BLL.Interfaces.Student.IStudentProfileService, BLL.Services.Student.StudentProfileService>();
builder.Services.AddScoped<IStudentProfileService, StudentProfileService>();
builder.Services.AddScoped<IInstructorDashboardService, InstructorDashboardService>();

builder.Services.AddScoped<BLL.Interfaces.Student.IStudentProfileService, BLL.Services.Student.StudentProfileService>();
builder.Services.AddScoped<ICourseCreationService, CourseCreationService>(); // ADDED THIS LINE


// ========================================
// WEB SERVICES (Your simplified layer)
// ========================================
builder.Services.AddScoped<BLL.Interfaces.Student.IStudentDashboardService, BLL.Services.Student.StudentDashboardService>();
builder.Services.AddScoped<Web.Interfaces.IStudentDashboardService, Web.Services.StudentDashboardService>();
builder.Services.AddScoped<Web.Interfaces.IStudentCoursesService, Web.Services.StudentCoursesService>();
builder.Services.AddScoped<IStudentTrackService, StudentTracksService>();
builder.Services.AddScoped<IStudentTrackDetailsService, StudentTrackDetailsService>();
builder.Services.AddScoped<IStudentBrowseTrackService, StudentBrowseTrackService>();
builder.Services.AddScoped<IStudentCourseDetailsService, StudentCourseDetailsService>(); // ADDED THIS LINE
builder.Services.AddScoped<Web.Interfaces.IStudentBrowseCoursesService, Web.Services.StudentBrowseCoursesService>(); // ADDED THIS LINE
builder.Services.AddScoped<Web.Interfaces.IStudentCertificatesService, Web.Services.StudentCertificatesService>();


// Authentication Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddScoped<RazorViewToStringRenderer>();

// Configure Antiforgery to accept tokens from headers
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Configure DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
    )
));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddIdentity<User, IdentityRole<int>>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    //options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";

    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();
app.UseAuthentication();
// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Add this BEFORE MapControllerRoute
app.MapRazorPages(); // ADD THIS LINE

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Instructor}/{action=Dashboard}"
);


// --- Seed roles and admin user ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    string[] roles = { "Student", "Instructor", "Admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<int>(role));
    }
}

// --- Seed database with users ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedDatabaseAsync(services);
        Console.WriteLine("? Database seeded successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.MapHub<NotificationHub>("/notificationHub");

app.Run();

