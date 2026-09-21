using BLL.DTOs.Account;
using BLL.Helpers;
using BLL.Interfaces;
using BLL.Interfaces.Account;
using BLL.Interfaces.Admin;
using BLL.Interfaces.CourseLearning;
using BLL.Interfaces.Enrollment;
using BLL.Interfaces.Instructor;
using BLL.Interfaces.Student;
using BLL.Services;
using BLL.Services.Account;
using BLL.Services.Admin;
using BLL.Services.CourseLearning;
using BLL.Services.Enrollment;
using BLL.Services.Instructor;
using BLL.Services.Student;
using BLL.Services.Track;
using Core.Entities;
using Core.RepositoryInterfaces;
using DAL.Data;
using DAL.Data.RepositoryServices;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Web.Hubs;
using Web.Interfaces;
using Web.Services;

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
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<IInstructorProfileRepository, InstructorProfileRepository>();
builder.Services.AddScoped<IInstructorManageCourseService, InstructorManageCourseService>();

// Add generic repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Current User Service
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
builder.Services.AddScoped<IInstructorDashboardService, InstructorDashboardService>();
builder.Services.AddScoped<IStudentProfileService, StudentProfileService>(); // مكتوبة مرة واحدة فقط
builder.Services.AddScoped<ICourseCreationService, CourseCreationService>();
builder.Services.AddScoped<ITrackService, TrackService>();

// ========================================
// WEB/STUDENT SERVICES (Refactored Layer)
// ========================================
builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();
builder.Services.AddScoped<IStudentCoursesService, StudentCoursesService>();
builder.Services.AddScoped<IStudentTrackService, StudentTracksService>();
builder.Services.AddScoped<IStudentTrackDetailsService, StudentTrackDetailsService>();
builder.Services.AddScoped<IStudentBrowseTrackService, StudentBrowseTrackService>();
builder.Services.AddScoped<IStudentCourseDetailsService, StudentCourseDetailsService>();
builder.Services.AddScoped<IStudentBrowseCoursesService, StudentBrowseCoursesService>();
builder.Services.AddScoped<IStudentCertificatesService, StudentCertificatesService>();
builder.Services.AddScoped<ICertificateGenerationService, CertificateGenerationService>();

// Authentication & Core Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<RazorViewToStringRenderer>();

// Configure Antiforgery to accept tokens from headers
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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
builder.Services.AddScoped<IPublicInstructorService, PublicInstructorService>();
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<Web.Mappings.StudentMappingProfile>();
    config.AddProfile<Web.Mappings.InstructorMappingProfile>();
    config.AddProfile<Web.Mappings.HomeMappingProfile>();
    config.AddProfile<Web.Mappings.CourseLearningMappingProfile>();
    config.AddProfile<Web.Mappings.PublicMappingProfile>();
});

var app = builder.Build();

app.UseAuthentication();

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

app.MapRazorPages();
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
        Console.WriteLine("✅ Database seeded successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.MapHub<NotificationHub>("/notificationHub");

app.Run();