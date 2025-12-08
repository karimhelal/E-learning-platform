using Core.Entities;
using Core.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data;

public static class DbSeeder
{
    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Check if already seeded
            if (context.Users.Any() && context.Courses.Any())
            {
                Console.WriteLine("Database already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Starting comprehensive database seeding...");

            // ============================================
            // STEP 1: Seed Roles
            // ============================================
            await SeedRoles(roleManager);

            // ============================================
            // STEP 2: Seed Categories
            // ============================================
            await SeedCategories(context);

            // ============================================
            // STEP 3: Seed Languages
            // ============================================
            await SeedLanguages(context);

            // ============================================
            // STEP 4: Seed Users (ALL with both Student & Instructor roles)
            // ============================================
            await SeedUsers(userManager, context);

            // ============================================
            // STEP 5: Seed Skills
            // ============================================
            await SeedSkills(context);

            // ============================================
            // STEP 6: Seed User Social Links
            // ============================================
            await SeedUserSocialLinks(context);

            // ============================================
            // STEP 7: Seed Courses and Tracks
            // ============================================
            await SeedCoursesAndTracks(context);

            // ============================================
            // STEP 8: Seed Modules
            // ============================================
            await SeedModules(context);

            // ============================================
            // STEP 9: Seed Lessons
            // ============================================
            await SeedLessons(context);

            // ============================================
            // STEP 10: Seed Lesson Contents
            // ============================================
            await SeedLessonContents(context);

            // ============================================
            // STEP 11: Seed Course Learning Outcomes
            // ============================================
            await SeedLearningOutcomes(context);

            // ============================================
            // STEP 12: Seed Enrollments (BOTH Course and Track)
            // ============================================
            await SeedEnrollments(context);

            // ============================================
            // STEP 13: Seed Lesson Progress
            // ============================================
            await SeedLessonProgress(context);

            // ============================================
            // STEP 14: Seed Certificates (MORE CERTIFICATES)
            // ============================================
            await SeedCertificates(context);

            // ============================================
            // STEP 15: Seed Assignments
            // ============================================
            await SeedAssignments(context);

            // ============================================
            // STEP 16: Seed Lesson Resources
            // ============================================
            await SeedLessonResources(context);

            // ============================================
            // STEP 17: Seed Notifications
            // ============================================
            await SeedNotifications(context);

            Console.WriteLine("? Database seeded successfully!");
            Console.WriteLine("========================================");
            Console.WriteLine("Seeded Data Summary:");
            Console.WriteLine($"- Users: {await context.Users.CountAsync()}");
            Console.WriteLine($"- Instructors: {await context.InstructorProfiles.CountAsync()}");
            Console.WriteLine($"- Students: {await context.StudentProfiles.CountAsync()}");
            Console.WriteLine($"- Skills: {await context.Set<Skill>().CountAsync()}");
            Console.WriteLine($"- Social Links: {await context.UserSocialLinks.CountAsync()}");
            Console.WriteLine($"- Categories: {await context.Categories.CountAsync()}");
            Console.WriteLine($"- Languages: {await context.Languages.CountAsync()}");
            Console.WriteLine($"- Courses: {await context.Courses.CountAsync()}");
            Console.WriteLine($"  • Published: {await context.Courses.CountAsync(c => c.Status == LearningEntityStatus.Published)}");
            Console.WriteLine($"  • Pending: {await context.Courses.CountAsync(c => c.Status == LearningEntityStatus.Pending)}");
            Console.WriteLine($"- Tracks: {await context.Tracks.CountAsync()}");
            Console.WriteLine($"- Modules: {await context.Modules.CountAsync()}");
            Console.WriteLine($"- Lessons: {await context.Lessons.CountAsync()}");
            Console.WriteLine($"- Lesson Contents: {await context.LessonContents.CountAsync()}");
            Console.WriteLine($"- Learning Outcomes: {await context.CourseLearningOutcomes.CountAsync()}");
            Console.WriteLine($"- Course Enrollments: {await context.CourseEnrollments.CountAsync()}");
            Console.WriteLine($"- Track Enrollments: {await context.TrackEnrollments.CountAsync()}");
            Console.WriteLine($"- Lesson Progress: {await context.LessonProgress.CountAsync()}");
            Console.WriteLine($"- Course Certificates: {await context.CourseCertificates.CountAsync()}");
            Console.WriteLine($"- Track Certificates: {await context.TrackCertificates.CountAsync()}");
            Console.WriteLine($"- Assignments: {await context.Assignments.CountAsync()}");
            Console.WriteLine($"- Lesson Resources: {await context.LessonResources.CountAsync()}");
            Console.WriteLine($"- Notifications: {await context.Notifications.CountAsync()}");
            Console.WriteLine("========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? ERROR during seeding: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            throw;
        }
    }

    private static async Task SeedRoles(RoleManager<IdentityRole<int>> roleManager)
    {
        string[] roles = ["Student", "Instructor", "Admin"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
        Console.WriteLine("? Roles seeded");
    }

    private static async Task SeedCategories(AppDbContext context)
    {
        if (context.Categories.Any()) return;

        var categories = new List<Category>
        {
            new() { Name = "Development", Slug = "development" },
            new() { Name = "Data Science", Slug = "data-science" },
            new() { Name = "Design", Slug = "design" },
            new() { Name = "Business", Slug = "business" },
            new() { Name = "Marketing", Slug = "marketing" },
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var development = await context.Categories.FirstAsync(c => c.Slug == "development");
        var dataScience = await context.Categories.FirstAsync(c => c.Slug == "data-science");
        var design = await context.Categories.FirstAsync(c => c.Slug == "design");

        var subCategories = new List<Category>
        {
            new() { Name = "Web Development", Slug = "web-development", ParentCategoryId = development.CategoryId },
            new() { Name = "Mobile Development", Slug = "mobile-development", ParentCategoryId = development.CategoryId },
            new() { Name = "Programming Languages", Slug = "programming-languages", ParentCategoryId = development.CategoryId },
            new() { Name = "Machine Learning", Slug = "machine-learning", ParentCategoryId = dataScience.CategoryId },
            new() { Name = "Data Analysis", Slug = "data-analysis", ParentCategoryId = dataScience.CategoryId },
            new() { Name = "UI/UX Design", Slug = "ui-ux-design", ParentCategoryId = design.CategoryId },
        };

        context.Categories.AddRange(subCategories);
        await context.SaveChangesAsync();
        Console.WriteLine("? Categories seeded");
    }

    private static async Task SeedLanguages(AppDbContext context)
    {
        if (context.Languages.Any()) return;

        var languages = new List<Language>
        {
            new() { Name = "English", Slug = "english" },
            new() { Name = "Arabic", Slug = "arabic" },
            new() { Name = "Spanish", Slug = "spanish" },
            new() { Name = "French", Slug = "french" },
        };

        context.Languages.AddRange(languages);
        await context.SaveChangesAsync();
        Console.WriteLine("? Languages seeded");
    }

    private static async Task SeedUsers(UserManager<User> userManager, AppDbContext context)
    {
        // ALL USERS: Primary instructors (experienced educators)
        await SeedUserWithBothRolesAsync(userManager, context, "john.instructor@example.com", "John", "Doe",
            "Expert web developer with 10+ years experience in full-stack development.", 10, isPrimaryInstructor: true);
        await SeedUserWithBothRolesAsync(userManager, context, "sarah.instructor@example.com", "Sarah", "Williams",
            "Data science professional specializing in ML and AI.", 8, isPrimaryInstructor: true);
        await SeedUserWithBothRolesAsync(userManager, context, "mike.instructor@example.com", "Mike", "Chen",
            "Mobile app developer and UI/UX designer with 50+ apps.", 6, isPrimaryInstructor: true);

        // ALL USERS: Primary students (active learners, can also teach)
        await SeedUserWithBothRolesAsync(userManager, context, "alice.student@example.com", "Alice", "Johnson",
            "Aspiring web developer passionate about new technologies.", 1, isPrimaryInstructor: false);
        await SeedUserWithBothRolesAsync(userManager, context, "bob.student@example.com", "Bob", "Smith",
            "Computer science student interested in data science.", 2, isPrimaryInstructor: false);
        await SeedUserWithBothRolesAsync(userManager, context, "charlie.student@example.com", "Charlie", "Brown",
            "Career switcher learning software development.", 0, isPrimaryInstructor: false);
        await SeedUserWithBothRolesAsync(userManager, context, "diana.student@example.com", "Diana", "Prince",
            "Designer transitioning to front-end development.", 1, isPrimaryInstructor: false);
        await SeedUserWithBothRolesAsync(userManager, context, "emma.student@example.com", "Emma", "Watson",
            "Software engineering student expanding skill set.", 2, isPrimaryInstructor: false);

        Console.WriteLine("? Users seeded (ALL with both Student & Instructor roles)");
    }

    private static async Task SeedSkills(AppDbContext context)
    {
        if (context.Set<Skill>().Any()) return;

        var users = await context.Users.ToListAsync();
        if (!users.Any()) return;

        var skills = new List<Skill>();

        var instructorSkills = new[] { "JavaScript", "React", "Node.js", "Python", "Machine Learning", "Data Analysis", "Swift", "SwiftUI", "Figma", "UI/UX Design" };
        var studentSkills = new[] { "HTML", "CSS", "JavaScript", "Python", "SQL", "Git", "React", "TypeScript" };

        foreach (var user in users.Take(3))
        {
            var userSkills = instructorSkills.OrderBy(_ => Guid.NewGuid()).Take(4);
            foreach (var skill in userSkills)
            {
                skills.Add(new Skill { UserId = user.Id, SkillName = skill, SkillType = "Instructor" });
            }
        }

        foreach (var user in users.Skip(3))
        {
            var userSkills = studentSkills.OrderBy(_ => Guid.NewGuid()).Take(3);
            foreach (var skill in userSkills)
            {
                skills.Add(new Skill { UserId = user.Id, SkillName = skill, SkillType = "Student" });
            }
        }

        context.Set<Skill>().AddRange(skills);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Skills seeded ({skills.Count} skills)");
    }

    private static async Task SeedUserSocialLinks(AppDbContext context)
    {
        if (context.UserSocialLinks.Any()) return;

        var users = await context.Users.ToListAsync();
        if (!users.Any()) return;

        var socialLinks = new List<UserSocialLink>();

        foreach (var user in users)
        {
            socialLinks.Add(new UserSocialLink
            {
                UserId = user.Id,
                SocialPlatform = SocialPlatform.Github,
                Url = $"https://github.com/{user.FirstName.ToLower()}{user.LastName.ToLower()}"
            });
            socialLinks.Add(new UserSocialLink
            {
                UserId = user.Id,
                SocialPlatform = SocialPlatform.LinkedIn,
                Url = $"https://linkedin.com/in/{user.FirstName.ToLower()}-{user.LastName.ToLower()}"
            });
        }

        context.UserSocialLinks.AddRange(socialLinks);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Social links seeded ({socialLinks.Count} links)");
    }

    private static async Task SeedCoursesAndTracks(AppDbContext context)
    {
        if (context.Courses.Any()) return;

        var instructors = await context.InstructorProfiles.OrderBy(i => i.InstructorId).ToListAsync();
        if (instructors.Count < 3) return;

        // At least 3 courses per instructor (9 total courses)
        var courses = new List<Course>
        {
            // Instructor 1 (John) - Web Development
            new() { Title = "Complete Web Development Bootcamp", Description = "Master web development from scratch. Learn HTML, CSS, JavaScript, React, Node.js.", Level = CourseLevel.Beginner, InstructorId = instructors[0].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1498050108023-c5249f4df085", Status = LearningEntityStatus.Published, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)) },
            new() { Title = "Advanced React and Redux Masterclass", Description = "Take your React skills to the next level with advanced patterns, hooks, and Redux.", Level = CourseLevel.Advanced, InstructorId = instructors[0].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1633356122544-f134324a6cee", Status = LearningEntityStatus.Published, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-5)) },
            new() { Title = "Node.js Backend Development", Description = "Build scalable backend applications with Node.js, Express, and MongoDB.", Level = CourseLevel.Intermediate, InstructorId = instructors[0].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1627398242454-45a1465c2479", Status = LearningEntityStatus.Pending, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)) },
            
            // Instructor 2 (Sarah) - Data Science
            new() { Title = "Python for Data Science", Description = "Learn Python programming with focus on data analysis using NumPy, Pandas, Matplotlib.", Level = CourseLevel.Beginner, InstructorId = instructors[1].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1526379095098-d400fd0bf935", Status = LearningEntityStatus.Published, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4)) },
            new() { Title = "Machine Learning Fundamentals", Description = "Complete hands-on ML course covering regression, classification, and neural networks.", Level = CourseLevel.Intermediate, InstructorId = instructors[1].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1555949963-aa79dcee981c", Status = LearningEntityStatus.Published, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-3)) },
            new() { Title = "Deep Learning with TensorFlow", Description = "Master deep learning and neural networks using TensorFlow and Keras.", Level = CourseLevel.Advanced, InstructorId = instructors[1].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1677442136019-21780ecad995", Status = LearningEntityStatus.Pending, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)) },
            
            // Instructor 3 (Mike) - Mobile & Design
            new() { Title = "iOS App Development with Swift", Description = "Build iOS applications from scratch using Swift and SwiftUI.", Level = CourseLevel.Intermediate, InstructorId = instructors[2].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c", Status = LearningEntityStatus.Published, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)) },
            new() { Title = "UI/UX Design Fundamentals", Description = "Master user interface and user experience design with Figma and prototyping.", Level = CourseLevel.Beginner, InstructorId = instructors[2].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1561070791-2526d30994b5", Status = LearningEntityStatus.Published, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)) },
            new() { Title = "Android App Development with Kotlin", Description = "Build modern Android applications using Kotlin and Jetpack Compose.", Level = CourseLevel.Intermediate, InstructorId = instructors[2].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1607252650355-f7fd0460ccdb", Status = LearningEntityStatus.Published, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)) },
        };

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();

        var tracks = new List<Track>
        {
            new() { Title = "Full Stack Web Developer Track", Description = "Complete learning path to become a professional full-stack web developer.", Status = LearningEntityStatus.Published, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)) },
            new() { Title = "Data Science Professional Track", Description = "Comprehensive track covering Python, statistics, ML, and data visualization.", Status = LearningEntityStatus.Published, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-5)) },
            new() { Title = "Mobile Developer Track", Description = "Learn mobile development for iOS and Android from experts.", Status = LearningEntityStatus.Published, CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4)) },
        };

        context.Tracks.AddRange(tracks);
        await context.SaveChangesAsync();

        // Link courses to categories and languages
        var savedCourses = await context.Courses.Include(c => c.Categories).Include(c => c.Languages).ToListAsync();
        var webDevCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "web-development");
        var mlCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "machine-learning");
        var dataAnalysisCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "data-analysis");
        var mobileCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "mobile-development");
        var uiuxCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "ui-ux-design");
        var englishLanguage = await context.Languages.FirstOrDefaultAsync(l => l.Slug == "english");
        var arabicLanguage = await context.Languages.FirstOrDefaultAsync(l => l.Slug == "arabic");

        if (webDevCategory != null && englishLanguage != null)
        {
            savedCourses[0].Categories.Add(webDevCategory);
            savedCourses[0].Languages.Add(englishLanguage);
            savedCourses[1].Categories.Add(webDevCategory);
            savedCourses[1].Languages.Add(englishLanguage);
            savedCourses[2].Categories.Add(webDevCategory);
            savedCourses[2].Languages.Add(englishLanguage);
        }
        if (dataAnalysisCategory != null && mlCategory != null && englishLanguage != null)
        {
            savedCourses[3].Categories.Add(dataAnalysisCategory);
            savedCourses[3].Languages.Add(englishLanguage);
            savedCourses[4].Categories.Add(mlCategory);
            savedCourses[4].Languages.Add(englishLanguage);
            savedCourses[5].Categories.Add(mlCategory);
            savedCourses[5].Languages.Add(englishLanguage);
            savedCourses[5].Languages.Add(arabicLanguage);
        }
        if (mobileCategory != null && uiuxCategory != null && englishLanguage != null)
        {
            savedCourses[6].Categories.Add(mobileCategory);
            savedCourses[6].Languages.Add(englishLanguage);
            savedCourses[7].Categories.Add(uiuxCategory);
            savedCourses[7].Languages.Add(englishLanguage);
            savedCourses[8].Categories.Add(mobileCategory);
            savedCourses[8].Languages.Add(englishLanguage);
        }
        await context.SaveChangesAsync();

        // Link Tracks to Courses
        var savedTracks = await context.Tracks.OrderBy(t => t.CreatedDate).ToListAsync();
        context.TrackCourses.AddRange(
        [
            new Track_Course { TrackId = savedTracks[0].Id, CourseId = savedCourses[0].Id },
            new Track_Course { TrackId = savedTracks[0].Id, CourseId = savedCourses[1].Id },
            new Track_Course { TrackId = savedTracks[0].Id, CourseId = savedCourses[2].Id },
            new Track_Course { TrackId = savedTracks[1].Id, CourseId = savedCourses[3].Id },
            new Track_Course { TrackId = savedTracks[1].Id, CourseId = savedCourses[4].Id },
            new Track_Course { TrackId = savedTracks[2].Id, CourseId = savedCourses[6].Id },
            new Track_Course { TrackId = savedTracks[2].Id, CourseId = savedCourses[7].Id },
            new Track_Course { TrackId = savedTracks[2].Id, CourseId = savedCourses[8].Id },
        ]);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Courses and Tracks seeded ({savedCourses.Count} courses, {savedTracks.Count} tracks)");
    }

    private static async Task SeedModules(AppDbContext context)
    {
        if (context.Modules.Any()) return;

        var courses = await context.Courses.OrderBy(c => c.CreatedDate).ToListAsync();
        if (courses.Count == 0) return;

        var modules = new List<Module>();

        // Generate 4-6 modules per course
        foreach (var course in courses)
        {
            int moduleCount = 4 + (course.Id % 3); // 4-6 modules
            for (int i = 1; i <= moduleCount; i++)
            {
                modules.Add(new Module
                {
                    CourseId = course.Id,
                    Title = $"Module {i}: {course.Title.Split(' ').Take(3).Aggregate((a, b) => a + " " + b)} - Part {i}",
                    Description = $"Comprehensive coverage of key concepts in module {i}",
                    Order = i
                });
            }
        }

        context.Modules.AddRange(modules);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Modules seeded ({modules.Count} modules)");
    }

    private static async Task SeedLessons(AppDbContext context)
    {
        if (context.Lessons.Any()) return;

        var modules = await context.Modules.OrderBy(m => m.CourseId).ThenBy(m => m.Order).ToListAsync();
        if (modules.Count == 0) return;

        var lessons = new List<Lesson>();

        foreach (var module in modules)
        {
            var lessonCount = 4 + (module.ModuleId % 3); // 4-6 lessons per module
            for (int i = 1; i <= lessonCount; i++)
            {
                lessons.Add(new Lesson
                {
                    ModuleId = module.ModuleId,
                    Title = $"{module.Title} - Lesson {i}",
                    ContentType = i % 3 == 0 ? LessonContentType.Article : LessonContentType.Video,
                    Order = i
                });
            }
        }

        context.Lessons.AddRange(lessons);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Lessons seeded ({lessons.Count} lessons)");
    }

    private static async Task SeedLessonContents(AppDbContext context)
    {
        if (context.LessonContents.Any()) return;

        var lessons = await context.Lessons.ToListAsync();
        if (lessons.Count == 0) return;

        var lessonContents = new List<LessonContent>();

        foreach (var lesson in lessons)
        {
            if (lesson.ContentType == LessonContentType.Video)
            {
                lessonContents.Add(new VideoContent
                {
                    LessonId = lesson.LessonId,
                    VideoUrl = $"https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    DurationInSeconds = 300 + (lesson.Order * 180) // 5-35 minutes
                });
            }
            else
            {
                lessonContents.Add(new ArticleContent
                {
                    LessonId = lesson.LessonId,
                    Content = $"<h1>{lesson.Title}</h1><p>Comprehensive article content for {lesson.Title}. This lesson covers fundamental concepts and practical applications.</p><h2>Key Points</h2><ul><li>Point 1</li><li>Point 2</li><li>Point 3</li></ul>"
                });
            }
        }

        context.LessonContents.AddRange(lessonContents);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Lesson contents seeded ({lessonContents.Count} contents)");
    }

    private static async Task SeedLearningOutcomes(AppDbContext context)
    {
        if (context.CourseLearningOutcomes.Any()) return;

        var courses = await context.Courses.OrderBy(c => c.CreatedDate).ToListAsync();
        if (courses.Count == 0) return;

        var outcomes = new List<CourseLearningOutcome>();

        foreach (var course in courses)
        {
            for (int i = 1; i <= 4; i++)
            {
                outcomes.Add(new CourseLearningOutcome
                {
                    CourseId = course.Id,
                    Title = $"Learning Outcome {i} for {course.Title.Split(' ').First()}",
                    Description = $"Master key concepts and practical skills in area {i}"
                });
            }
        }

        context.CourseLearningOutcomes.AddRange(outcomes);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Learning outcomes seeded ({outcomes.Count} outcomes)");
    }

    private static async Task SeedEnrollments(AppDbContext context)
    {
        if (context.Enrollments.Any()) return;

        var students = await context.StudentProfiles.OrderBy(s => s.StudentId).ToListAsync();
        var courses = await context.Courses.Where(c => c.Status == LearningEntityStatus.Published).OrderBy(c => c.CreatedDate).ToListAsync();
        var tracks = await context.Tracks.OrderBy(t => t.CreatedDate).ToListAsync();

        if (students.Count == 0 || courses.Count == 0 || tracks.Count == 0) return;

        var enrollments = new List<EnrollmentBase>();

        // Each student enrolls in at least one track + multiple courses
        foreach (var student in students)
        {
            // Enroll in one track (REQUIREMENT: at least one track per student)
            var trackIndex = (student.StudentId - 1) % tracks.Count;
            enrollments.Add(new TrackEnrollment
            {
                StudentId = student.StudentId,
                TrackId = tracks[trackIndex].Id,
                EnrollmentDate = DateTime.Now.AddMonths(-3),
                Status = EnrollmentStatus.InProgress,
                ProgressPercentage = 35.00m + (student.StudentId * 5)
            });

            // Enroll in 2-4 courses
            var courseCount = 2 + (student.StudentId % 3);
            for (int i = 0; i < courseCount && i < courses.Count; i++)
            {
                var courseIndex = ((student.StudentId - 1) * 2 + i) % courses.Count;
                var isCompleted = i == 0 && student.StudentId % 3 == 0; // Some completed courses

                enrollments.Add(new CourseEnrollment
                {
                    StudentId = student.StudentId,
                    CourseId = courses[courseIndex].Id,
                    EnrollmentDate = DateTime.Now.AddMonths(-2 - i),
                    Status = isCompleted ? EnrollmentStatus.Completed : EnrollmentStatus.InProgress,
                    ProgressPercentage = isCompleted ? 100.00m : (25.00m + (student.StudentId + i) * 10)
                });
            }
        }

        context.Enrollments.AddRange(enrollments);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Enrollments seeded ({enrollments.Count} enrollments)");
    }

    private static async Task SeedLessonProgress(AppDbContext context)
    {
        if (context.LessonProgress.Any()) return;

        var students = await context.StudentProfiles.OrderBy(s => s.StudentId).ToListAsync();
        var courseEnrollments = await context.CourseEnrollments
            .Include(e => e.Course)
            .ThenInclude(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .ToListAsync();

        if (students.Count == 0 || courseEnrollments.Count == 0) return;

        var progressList = new List<LessonProgress>();

        foreach (var enrollment in courseEnrollments)
        {
            var allLessons = enrollment.Course.Modules
                .SelectMany(m => m.Lessons)
                .OrderBy(l => l.ModuleId)
                .ThenBy(l => l.Order)
                .ToList();

            if (!allLessons.Any()) continue;

            // Mark progress based on enrollment percentage
            var completionRate = (int)(enrollment.ProgressPercentage / 100m * allLessons.Count);

            for (int i = 0; i < allLessons.Count; i++)
            {
                var isCompleted = i < completionRate;
                progressList.Add(new LessonProgress
                {
                    StudentId = enrollment.StudentId,
                    LessonId = allLessons[i].LessonId,
                    IsCompleted = isCompleted,
                    StartedDate = DateTime.Now.AddDays(-30 + i),
                    CompletedDate = isCompleted ? DateTime.Now.AddDays(-29 + i) : null
                });
            }
        }

        context.LessonProgress.AddRange(progressList);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Lesson progress seeded ({progressList.Count} records)");
    }

    private static async Task SeedCertificates(AppDbContext context)
    {
        if (context.CourseCertificates.Any() || context.TrackCertificates.Any()) return;

        var students = await context.StudentProfiles.ToListAsync();
        var completedCourseEnrollments = await context.CourseEnrollments
            .Where(e => e.Status == EnrollmentStatus.Completed)
            .Include(e => e.Course)
            .ToListAsync();

        var highProgressCourseEnrollments = await context.CourseEnrollments
            .Where(e => e.ProgressPercentage >= 80 && e.Status != EnrollmentStatus.Completed)
            .Include(e => e.Course)
            .ToListAsync();

        var trackEnrollments = await context.TrackEnrollments
            .Where(e => e.ProgressPercentage >= 70)
            .Include(e => e.Track)
            .ToListAsync();

        var courseCertificates = new List<CourseCertificate>();
        var trackCertificates = new List<TrackCertificate>();

        // Course certificates for completed enrollments
        foreach (var enrollment in completedCourseEnrollments)
        {
            courseCertificates.Add(new CourseCertificate
            {
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                Title = $"Certificate of Completion - {enrollment.Course.Title}",
                Link = $"https://certificates.example.com/course-{enrollment.CourseId}-student-{enrollment.StudentId}",
                IssuedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10))
            });
        }

        // Additional course certificates for high-progress students (MORE CERTIFICATES)
        foreach (var enrollment in highProgressCourseEnrollments.Take(10))
        {
            courseCertificates.Add(new CourseCertificate
            {
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                Title = $"Advanced Achievement - {enrollment.Course.Title}",
                Link = $"https://certificates.example.com/progress-{enrollment.CourseId}-student-{enrollment.StudentId}",
                IssuedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-3))
            });
        }

        // Track certificates for high-progress tracks
        foreach (var enrollment in trackEnrollments)
        {
            trackCertificates.Add(new TrackCertificate
            {
                StudentId = enrollment.StudentId,
                TrackId = enrollment.TrackId,
                Title = $"Track Completion Certificate - {enrollment.Track.Title}",
                Link = $"https://certificates.example.com/track-{enrollment.TrackId}-student-{enrollment.StudentId}",
                IssuedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5))
            });
        }

        // Add bonus certificates for first 3 students (MORE CERTIFICATES)
        var publishedCourses = await context.Courses
            .Where(c => c.Status == LearningEntityStatus.Published)
            .OrderBy(c => c.CreatedDate)
            .Take(3)
            .ToListAsync();

        for (int i = 0; i < Math.Min(3, students.Count); i++)
        {
            if (publishedCourses.Count > i)
            {
                courseCertificates.Add(new CourseCertificate
                {
                    StudentId = students[i].StudentId,
                    CourseId = publishedCourses[i].Id,
                    Title = $"Excellence Award - {publishedCourses[i].Title}",
                    Link = $"https://certificates.example.com/excellence-{publishedCourses[i].Id}-student-{students[i].StudentId}",
                    IssuedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1))
                });
            }
        }

        context.CourseCertificates.AddRange(courseCertificates);
        context.TrackCertificates.AddRange(trackCertificates);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Certificates seeded ({courseCertificates.Count} course certs, {trackCertificates.Count} track certs)");
    }

    private static async Task SeedAssignments(AppDbContext context)
    {
        if (context.Assignments.Any()) return;

        var modules = await context.Modules.ToListAsync();
        if (modules.Count == 0) return;

        var assignments = modules.Select(m => new Assignment
        {
            ModuleId = m.ModuleId,
            Title = $"{m.Title} - Practical Assignment",
            Instruction = $"Complete the hands-on project for {m.Title}. Apply the concepts learned in this module."
        }).ToList();

        context.Assignments.AddRange(assignments);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Assignments seeded ({assignments.Count} assignments)");
    }

    private static async Task SeedLessonResources(AppDbContext context)
    {
        if (context.LessonResources.Any()) return;

        var lessons = await context.Lessons.OrderBy(l => l.LessonId).ToListAsync();
        if (lessons.Count == 0) return;

        var resources = new List<LessonResource>();

        for (int i = 0; i < lessons.Count; i++)
        {
            var resourceType = i % 3;
            if (resourceType == 0)
                resources.Add(new PdfResource { LessonId = lessons[i].LessonId, Title = "Lesson Notes (PDF)", Url = $"https://example.com/pdf/lesson-{lessons[i].LessonId}.pdf" });
            else if (resourceType == 1)
                resources.Add(new UrlResource { LessonId = lessons[i].LessonId, Title = "Additional Reading", Url = $"https://example.com/articles/lesson-{lessons[i].LessonId}" });
            else
                resources.Add(new ZipResource { LessonId = lessons[i].LessonId, Title = "Project Files", Url = $"https://example.com/zip/lesson-{lessons[i].LessonId}.zip" });
        }

        context.LessonResources.AddRange(resources);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Lesson resources seeded ({resources.Count} resources)");
    }

    private static async Task SeedNotifications(AppDbContext context)
    {
        if (context.Notifications.Any()) return;

        var users = await context.Users.ToListAsync();
        if (!users.Any()) return;

        var notifications = new List<Notification>();

        foreach (var user in users)
        {
            notifications.Add(new Notification { UserId = user.Id, Title = "Welcome to Masar!", Message = "Start your learning journey today.", Url = "/student/dashboard", CreatedAt = DateTime.Now.AddDays(-5) });
            notifications.Add(new Notification { UserId = user.Id, Title = "New Courses Available", Message = "Check out our latest course offerings.", Url = "/browse-courses", CreatedAt = DateTime.Now.AddDays(-2) });
        }

        // Global notification
        notifications.Add(new Notification { UserId = null, Title = "Platform Update", Message = "New features have been added to enhance your learning experience.", Url = "/", CreatedAt = DateTime.Now.AddDays(-1) });

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Notifications seeded ({notifications.Count} notifications)");
    }

    /// <summary>
    /// Seeds a user with BOTH Student and Instructor roles
    /// </summary>
    private static async Task SeedUserWithBothRolesAsync(UserManager<User> userManager, AppDbContext context,
        string email, string firstName, string lastName, string bio, int yearsOfExperience, bool isPrimaryInstructor)
    {
        if (await userManager.FindByEmailAsync(email) != null) return;

        var user = new User { UserName = email, Email = email, FirstName = firstName, LastName = lastName, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, "Test@123");

        if (result.Succeeded)
        {
            // Add BOTH roles to every user
            await userManager.AddToRoleAsync(user, "Student");
            await userManager.AddToRoleAsync(user, "Instructor");

            // Create both profiles
            context.StudentProfiles.Add(new StudentProfile { UserId = user.Id, Bio = bio });
            context.InstructorProfiles.Add(new InstructorProfile 
            { 
                UserId = user.Id, 
                Bio = bio, 
                YearsOfExperience = yearsOfExperience 
            });
            
            await context.SaveChangesAsync();
        }
    }
}