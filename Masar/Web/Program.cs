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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IInstructorDashboardService, InstructorDashboardService>();
builder.Services.AddScoped<IInstructorCoursesService, InstructorCoursesService>();


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
    //Add some Restirictions on Password
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Make the Account Block if I enter the password five times wrong
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true; // for new users
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor for accessing current user
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

