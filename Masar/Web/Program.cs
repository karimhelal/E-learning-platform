using BLL.Helpers;
using BLL.Interfaces;
using BLL.Interfaces.Instructor;
using BLL.Services;
using BLL.Services.Instructor;
using Core.RepositoryInterfaces;
using DAL.Data;
using DAL.Data.RepositoryServices;
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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Instructor}/{action=Dashboard}"
);


//// scope to get services
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var _context = services.GetRequiredService<AppDbContext>();
//    try {
//        var Courses = GetInstructorCourses(_context, 1);
//    }
//    catch (Exception ex) {

//    }
//}
//// --- END: TEST CODE ---

app.Run();


//// Tag
//// Title
//// Desc
//// # students
//// # modules
//// # hours
//// # lessons
//// # assignments
//// thumbnail

//static ICollection<CourseViewModel> GetInstructorCourses (AppDbContext _context, int instructor_id)
//{
//    var courses = _context.Courses
//        .Where(c => c.InstructorId == instructor_id)
//        .Include(c => c.Categories)
//        .Include(c => c.Enrollments)
//        .Include(c => c.Modules!)
//            .ThenInclude(m => m.Lessons!)
//                .ThenInclude(l => l.LessonContent)
//        .Include(c => c.Modules!)
//            .ThenInclude(m => m.Assignments);

//    ICollection<CourseViewModel> lst = new List<CourseViewModel>();

//    foreach (var c in courses)
//    {
//        int numberOfStudents = c.Enrollments!.Where(ce => ce.CourseId == c.Id).Count();
//        int numberOfModules = c.Modules!.Count();
//        int numberOfLessons = c.Modules!.Sum(m => m.Lessons!.Count());
//        int numberOfMinutes = c.Modules!
//            .SelectMany(c => c.Lessons!)
//            .Select(l => l.LessonContent)
//            .OfType<VideoContent>()
//            .Sum(vc => vc.DurationInSeconds);

//        numberOfMinutes = (int)Math.Ceiling(numberOfMinutes / 60.0);

//        int numberOfAssignments = c.Modules!
//            .SelectMany(m => m.Assignments!)
//            .Count();

//        lst.Add(
//        new CourseViewModel(
//            c.Id,
//            c.Title,
//            c.Description!,
//            c.ThumbnailImageUrl!,
//            c.Language!,
//            c.Level!,
//            c.Categories!,
//            numberOfStudents,
//            numberOfModules,
//            numberOfLessons,
//            numberOfAssignments,
//            numberOfMinutes
//            )
//        );
//    }

//    return lst;
//}