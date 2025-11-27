using BLL.Helpers;
using BLL.Interfaces;
using BLL.Interfaces.Instructor;
using BLL.Services;
using BLL.Services.Instructor;
using Core.Entities;
using Core.RepositoryInterfaces;
using DAL.Data;
using DAL.Data.RepositoryServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Interfaces;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// REPOSITORIES (Team's layer)
// ========================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();

// ========================================
// BLL SERVICES (Team's layer)
// ========================================
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IInstructorDashboardService, InstructorDashboardService>();
builder.Services.AddScoped<IInstructorCoursesService, InstructorCoursesService>();
builder.Services.AddScoped<IInstructorProfileService, InstructorProfileService>();

// ========================================
// WEB SERVICES (Your simplified layer)
// ========================================
builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();

builder.Services.AddScoped<RazorViewToStringRenderer>();

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

builder.Services.AddIdentity<User, IdentityRole<int>>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Instructor}/{action=Dashboard}"
);

app.Run();

