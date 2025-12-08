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
        return;

        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Check if already seeded
            if (context.Users.Any() && context.Courses.Any() && context.Modules.Any() && context.Lessons.Any())
            {
                Console.WriteLine("Database already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Starting database seeding...");

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
            // STEP 4: Seed Users (Instructors & Students)
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
            // STEP 14: Seed Certificates
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
            Console.WriteLine($"- Tracks: {await context.Tracks.CountAsync()}");
            Console.WriteLine($"- Modules: {await context.Modules.CountAsync()}");
            Console.WriteLine($"- Lessons: {await context.Lessons.CountAsync()}");
            Console.WriteLine($"- Lesson Contents: {await context.LessonContents.CountAsync()}");
            Console.WriteLine($"- Learning Outcomes: {await context.CourseLearningOutcomes.CountAsync()}");
            Console.WriteLine($"- Course Enrollments: {await context.CourseEnrollments.CountAsync()}");
            Console.WriteLine($"- Track Enrollments: {await context.TrackEnrollments.CountAsync()}");
            Console.WriteLine($"- Lesson Progress: {await context.LessonProgress.CountAsync()}");
            Console.WriteLine($"- Certificates: {await context.CourseCertificates.CountAsync() + await context.TrackCertificates.CountAsync()}");
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
        // Instructors
        await SeedInstructorAsync(userManager, context, "john.instructor@example.com", "John", "Doe",
            "Expert web developer with 10+ years experience in full-stack development.", 10);
        await SeedInstructorAsync(userManager, context, "sarah.instructor@example.com", "Sarah", "Williams",
            "Data science professional specializing in ML and AI.", 8);
        await SeedInstructorAsync(userManager, context, "mike.instructor@example.com", "Mike", "Chen",
            "Mobile app developer and UI/UX designer with 50+ apps.", 6);

        // Students
        await SeedStudentAsync(userManager, context, "alice.student@example.com", "Alice", "Johnson",
            "Aspiring web developer passionate about new technologies.");
        await SeedStudentAsync(userManager, context, "bob.student@example.com", "Bob", "Smith",
            "Computer science student interested in data science.");
        await SeedStudentAsync(userManager, context, "charlie.student@example.com", "Charlie", "Brown",
            "Career switcher learning software development.");
        await SeedStudentAsync(userManager, context, "diana.student@example.com", "Diana", "Prince",
            "Designer transitioning to front-end development.");
        await SeedStudentAsync(userManager, context, "emma.student@example.com", "Emma", "Watson",
            "Software engineering student expanding skill set.");

        Console.WriteLine("? Users seeded");
    }

    private static async Task SeedSkills(AppDbContext context)
    {
        if (context.Set<Skill>().Any()) return;

        var users = await context.Users.ToListAsync();
        if (!users.Any()) return;

        var skills = new List<Skill>();

        // Instructor skills (teaching)
        var instructorSkills = new[] { "JavaScript", "React", "Node.js", "Python", "Machine Learning", "Data Analysis", "Swift", "SwiftUI", "Figma", "UI/UX Design" };
        var studentSkills = new[] { "HTML", "CSS", "JavaScript", "Python", "SQL", "Git", "React", "TypeScript" };

        foreach (var user in users.Take(3)) // Instructors
        {
            var userSkills = instructorSkills.OrderBy(_ => Guid.NewGuid()).Take(4);
            foreach (var skill in userSkills)
            {
                skills.Add(new Skill { UserId = user.Id, SkillName = skill, SkillType = "Instructor" });
            }
        }

        foreach (var user in users.Skip(3)) // Students
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

        foreach (var user in users.Take(5))
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

        var courses = new List<Course>
        {
            new() { Title = "Complete Web Development Bootcamp", Description = "Master web development from scratch. Learn HTML, CSS, JavaScript, React, Node.js.", Level = CourseLevel.Beginner, InstructorId = instructors[0].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1498050108023-c5249f4df085", CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)) },
            new() { Title = "Advanced React and Redux Masterclass", Description = "Take your React skills to the next level with advanced patterns, hooks, and Redux.", Level = CourseLevel.Advanced, InstructorId = instructors[0].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1633356122544-f134324a6cee", CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-5)) },
            new() { Title = "Python for Data Science", Description = "Learn Python programming with focus on data analysis using NumPy, Pandas, Matplotlib.", Level = CourseLevel.Beginner, InstructorId = instructors[1].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1526379095098-d400fd0bf935", CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4)) },
            new() { Title = "Machine Learning Fundamentals", Description = "Complete hands-on ML course covering regression, classification, and neural networks.", Level = CourseLevel.Intermediate, InstructorId = instructors[1].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1555949963-aa79dcee981c", CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-3)) },
            new() { Title = "iOS App Development with Swift", Description = "Build iOS applications from scratch using Swift and SwiftUI.", Level = CourseLevel.Intermediate, InstructorId = instructors[2].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c", CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)) },
            new() { Title = "UI/UX Design Fundamentals", Description = "Master user interface and user experience design with Figma and prototyping.", Level = CourseLevel.Beginner, InstructorId = instructors[2].InstructorId, ThumbnailImageUrl = "https://images.unsplash.com/photo-1561070791-2526d30994b5", CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)) },
        };

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();

        var tracks = new List<Track>
        {
            new() { Title = "Full Stack Web Developer Track", Description = "Complete learning path to become a professional full-stack web developer.", CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)) },
            new() { Title = "Data Science Professional Track", Description = "Comprehensive track covering Python, statistics, ML, and data visualization.", CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-5)) },
            new() { Title = "Mobile & Design Track", Description = "Learn mobile development and UI/UX design from experts.", CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4)) },
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

        if (webDevCategory != null && englishLanguage != null)
        {
            savedCourses[0].Categories.Add(webDevCategory);
            savedCourses[0].Languages.Add(englishLanguage);
            savedCourses[1].Categories.Add(webDevCategory);
            savedCourses[1].Languages.Add(englishLanguage);
        }
        if (dataAnalysisCategory != null && mlCategory != null && englishLanguage != null)
        {
            savedCourses[2].Categories.Add(dataAnalysisCategory);
            savedCourses[2].Languages.Add(englishLanguage);
            savedCourses[3].Categories.Add(mlCategory);
            savedCourses[3].Languages.Add(englishLanguage);
        }
        if (mobileCategory != null && uiuxCategory != null && englishLanguage != null)
        {
            savedCourses[4].Categories.Add(mobileCategory);
            savedCourses[4].Languages.Add(englishLanguage);
            savedCourses[5].Categories.Add(uiuxCategory);
            savedCourses[5].Languages.Add(englishLanguage);
        }
        await context.SaveChangesAsync();

        // Link Tracks to Courses
        var savedTracks = await context.Tracks.OrderBy(t => t.CreatedDate).ToListAsync();
        context.TrackCourses.AddRange(
        [
            new Track_Course { TrackId = savedTracks[0].Id, CourseId = savedCourses[0].Id },
            new Track_Course { TrackId = savedTracks[0].Id, CourseId = savedCourses[1].Id },
            new Track_Course { TrackId = savedTracks[1].Id, CourseId = savedCourses[2].Id },
            new Track_Course { TrackId = savedTracks[1].Id, CourseId = savedCourses[3].Id },
            new Track_Course { TrackId = savedTracks[2].Id, CourseId = savedCourses[4].Id },
            new Track_Course { TrackId = savedTracks[2].Id, CourseId = savedCourses[5].Id },
        ]);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Courses and Tracks seeded ({savedCourses.Count} courses, {savedTracks.Count} tracks)");
    }

    private static async Task SeedModules(AppDbContext context)
    {
        if (context.Modules.Any()) return;

        var courses = await context.Courses.OrderBy(c => c.CreatedDate).ToListAsync();
        if (courses.Count < 6) return;

        var modules = new List<Module>
        {
            // Course 1: Web Development Bootcamp (6 modules)
            new() { CourseId = courses[0].Id, Title = "Introduction to Web Development", Description = "Web development fundamentals", Order = 1 },
            new() { CourseId = courses[0].Id, Title = "HTML & CSS Mastery", Description = "Learn HTML5 and CSS3", Order = 2 },
            new() { CourseId = courses[0].Id, Title = "JavaScript Fundamentals", Description = "Master JavaScript basics and ES6+", Order = 3 },
            new() { CourseId = courses[0].Id, Title = "React for Beginners", Description = "Introduction to React", Order = 4 },
            new() { CourseId = courses[0].Id, Title = "Backend with Node.js", Description = "Build server-side applications", Order = 5 },
            new() { CourseId = courses[0].Id, Title = "Full Stack Project", Description = "Build a complete application", Order = 6 },

            // Course 2: Advanced React (4 modules)
            new() { CourseId = courses[1].Id, Title = "Advanced React Patterns", Description = "Learn advanced React patterns", Order = 1 },
            new() { CourseId = courses[1].Id, Title = "React Hooks Deep Dive", Description = "Master all React hooks", Order = 2 },
            new() { CourseId = courses[1].Id, Title = "Redux State Management", Description = "Implement Redux", Order = 3 },
            new() { CourseId = courses[1].Id, Title = "Performance Optimization", Description = "Optimize React apps", Order = 4 },

            // Course 3: Python for Data Science (5 modules)
            new() { CourseId = courses[2].Id, Title = "Python Programming Basics", Description = "Learn Python syntax", Order = 1 },
            new() { CourseId = courses[2].Id, Title = "NumPy for Numerical Computing", Description = "Master NumPy", Order = 2 },
            new() { CourseId = courses[2].Id, Title = "Data Analysis with Pandas", Description = "Data manipulation with Pandas", Order = 3 },
            new() { CourseId = courses[2].Id, Title = "Data Visualization", Description = "Create visualizations", Order = 4 },
            new() { CourseId = courses[2].Id, Title = "Statistical Analysis", Description = "Apply statistics to data", Order = 5 },

            // Course 4: Machine Learning (4 modules)
            new() { CourseId = courses[3].Id, Title = "Introduction to Machine Learning", Description = "ML concepts", Order = 1 },
            new() { CourseId = courses[3].Id, Title = "Supervised Learning", Description = "Regression and classification", Order = 2 },
            new() { CourseId = courses[3].Id, Title = "Unsupervised Learning", Description = "Clustering algorithms", Order = 3 },
            new() { CourseId = courses[3].Id, Title = "Neural Networks & Deep Learning", Description = "Deep learning intro", Order = 4 },

            // Course 5: iOS Development (4 modules)
            new() { CourseId = courses[4].Id, Title = "Swift Programming Basics", Description = "Learn Swift", Order = 1 },
            new() { CourseId = courses[4].Id, Title = "SwiftUI Fundamentals", Description = "Build UIs with SwiftUI", Order = 2 },
            new() { CourseId = courses[4].Id, Title = "iOS App Architecture", Description = "MVVM and clean architecture", Order = 3 },
            new() { CourseId = courses[4].Id, Title = "Publishing to App Store", Description = "Prepare and publish", Order = 4 },

            // Course 6: UI/UX Design (5 modules)
            new() { CourseId = courses[5].Id, Title = "Design Fundamentals", Description = "Design principles", Order = 1 },
            new() { CourseId = courses[5].Id, Title = "User Research", Description = "Conduct user research", Order = 2 },
            new() { CourseId = courses[5].Id, Title = "Wireframing & Prototyping", Description = "Create prototypes", Order = 3 },
            new() { CourseId = courses[5].Id, Title = "Figma Mastery", Description = "Master Figma", Order = 4 },
            new() { CourseId = courses[5].Id, Title = "Design Systems", Description = "Build design systems", Order = 5 },
        };

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

        // Generate 3-5 lessons per module
        foreach (var module in modules)
        {
            var lessonCount = 3 + (module.ModuleId % 3); // 3-5 lessons
            for (int i = 1; i <= lessonCount; i++)
            {
                lessons.Add(new Lesson
                {
                    ModuleId = module.ModuleId,
                    Title = $"{module.Title} - Part {i}",
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
                    VideoUrl = $"https://example.com/videos/lesson-{lesson.LessonId}.mp4",
                    DurationInSeconds = 300 + (lesson.Order * 120)
                });
            }
            else
            {
                lessonContents.Add(new ArticleContent
                {
                    LessonId = lesson.LessonId,
                    Content = $"<h1>{lesson.Title}</h1><p>Comprehensive article content for {lesson.Title}.</p>"
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
        if (courses.Count < 6) return;

        var outcomes = new List<CourseLearningOutcome>
        {
            new() { Id = 1, CourseId = courses[0].Id, Title = "Build responsive websites", Description = "Build responsive websites with HTML5, CSS3" },
            new() { Id = 2, CourseId = courses[0].Id, Title = "Master JavaScript", Description = "Master JavaScript fundamentals and ES6+" },
            new() { Id = 3, CourseId = courses[0].Id, Title = "Create React apps", Description = "Create dynamic web applications with React" },
            new() { Id = 1, CourseId = courses[1].Id, Title = "Master React Hooks", Description = "Master React Hooks and Context API" },
            new() { Id = 2, CourseId = courses[1].Id, Title = "Implement Redux", Description = "Implement Redux for state management" },
            new() { Id = 1, CourseId = courses[2].Id, Title = "Master Python", Description = "Master Python programming fundamentals" },
            new() { Id = 2, CourseId = courses[2].Id, Title = "Data analysis", Description = "Perform data analysis with Pandas" },
            new() { Id = 1, CourseId = courses[3].Id, Title = "Understand ML", Description = "Understand machine learning concepts" },
            new() { Id = 2, CourseId = courses[3].Id, Title = "Build ML models", Description = "Build and train ML models" },
            new() { Id = 1, CourseId = courses[4].Id, Title = "Master Swift", Description = "Master Swift programming" },
            new() { Id = 2, CourseId = courses[4].Id, Title = "Build iOS apps", Description = "Build iOS applications with SwiftUI" },
            new() { Id = 1, CourseId = courses[5].Id, Title = "Design principles", Description = "Understand design principles" },
            new() { Id = 2, CourseId = courses[5].Id, Title = "Master Figma", Description = "Master Figma for UI design" },
        };

        context.CourseLearningOutcomes.AddRange(outcomes);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Learning outcomes seeded ({outcomes.Count} outcomes)");
    }

    private static async Task SeedEnrollments(AppDbContext context)
    {
        if (context.Enrollments.Any()) return;

        var students = await context.StudentProfiles.OrderBy(s => s.StudentId).ToListAsync();
        var courses = await context.Courses.OrderBy(c => c.CreatedDate).ToListAsync();
        var tracks = await context.Tracks.OrderBy(t => t.CreatedDate).ToListAsync();

        if (students.Count < 3 || courses.Count < 6 || tracks.Count < 3) return;

        var enrollments = new List<EnrollmentBase>
        {
            new CourseEnrollment { StudentId = students[0].StudentId, CourseId = courses[0].Id, EnrollmentDate = DateTime.Now.AddMonths(-3), Status = EnrollmentStatus.InProgress, ProgressPercentage = 75.50m },
            new CourseEnrollment { StudentId = students[0].StudentId, CourseId = courses[1].Id, EnrollmentDate = DateTime.Now.AddMonths(-2), Status = EnrollmentStatus.InProgress, ProgressPercentage = 30.00m },
            new CourseEnrollment { StudentId = students[1].StudentId, CourseId = courses[0].Id, EnrollmentDate = DateTime.Now.AddMonths(-2), Status = EnrollmentStatus.InProgress, ProgressPercentage = 45.25m },
            new CourseEnrollment { StudentId = students[1].StudentId, CourseId = courses[2].Id, EnrollmentDate = DateTime.Now.AddMonths(-3), Status = EnrollmentStatus.InProgress, ProgressPercentage = 80.00m },
            new CourseEnrollment { StudentId = students[2].StudentId, CourseId = courses[0].Id, EnrollmentDate = DateTime.Now.AddMonths(-4), Status = EnrollmentStatus.Completed, ProgressPercentage = 100.00m },
            new CourseEnrollment { StudentId = students[2].StudentId, CourseId = courses[4].Id, EnrollmentDate = DateTime.Now.AddMonths(-1), Status = EnrollmentStatus.InProgress, ProgressPercentage = 55.50m },
            new CourseEnrollment { StudentId = students[3].StudentId, CourseId = courses[5].Id, EnrollmentDate = DateTime.Now.AddMonths(-2), Status = EnrollmentStatus.InProgress, ProgressPercentage = 40.00m },
            new CourseEnrollment { StudentId = students[4].StudentId, CourseId = courses[3].Id, EnrollmentDate = DateTime.Now.AddMonths(-1), Status = EnrollmentStatus.InProgress, ProgressPercentage = 20.00m },
            new TrackEnrollment { StudentId = students[0].StudentId, TrackId = tracks[0].Id, EnrollmentDate = DateTime.Now.AddMonths(-3), Status = EnrollmentStatus.InProgress, ProgressPercentage = 55.00m },
            new TrackEnrollment { StudentId = students[1].StudentId, TrackId = tracks[1].Id, EnrollmentDate = DateTime.Now.AddMonths(-3), Status = EnrollmentStatus.InProgress, ProgressPercentage = 70.50m },
            new TrackEnrollment { StudentId = students[2].StudentId, TrackId = tracks[2].Id, EnrollmentDate = DateTime.Now.AddMonths(-2), Status = EnrollmentStatus.InProgress, ProgressPercentage = 35.00m },
        };

        context.Enrollments.AddRange(enrollments);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Enrollments seeded ({enrollments.Count} enrollments)");
    }

    private static async Task SeedLessonProgress(AppDbContext context)
    {
        if (context.LessonProgress.Any()) return;

        var students = await context.StudentProfiles.OrderBy(s => s.StudentId).ToListAsync();
        var lessons = await context.Lessons.OrderBy(l => l.LessonId).ToListAsync();

        if (students.Count == 0 || lessons.Count == 0) return;

        var progressList = new List<LessonProgress>();

        // Student 1: First 15 lessons
        for (int i = 0; i < Math.Min(15, lessons.Count); i++)
        {
            progressList.Add(new LessonProgress
            {
                StudentId = students[0].StudentId,
                LessonId = lessons[i].LessonId,
                IsCompleted = i < 12,
                StartedDate = DateTime.Now.AddDays(-30 + i),
                CompletedDate = i < 12 ? DateTime.Now.AddDays(-29 + i) : null
            });
        }

        // Student 2: First 10 lessons
        for (int i = 0; i < Math.Min(10, lessons.Count); i++)
        {
            progressList.Add(new LessonProgress
            {
                StudentId = students[1].StudentId,
                LessonId = lessons[i].LessonId,
                IsCompleted = i < 8,
                StartedDate = DateTime.Now.AddDays(-20 + i),
                CompletedDate = i < 8 ? DateTime.Now.AddDays(-19 + i) : null
            });
        }

        context.LessonProgress.AddRange(progressList);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Lesson progress seeded ({progressList.Count} records)");
    }

    private static async Task SeedCertificates(AppDbContext context)
    {
        if (context.CourseCertificates.Any()) return;

        var students = await context.StudentProfiles.OrderBy(s => s.StudentId).ToListAsync();
        var courses = await context.Courses.OrderBy(c => c.CreatedDate).ToListAsync();

        if (students.Count == 0 || courses.Count == 0) return;

        var certificates = new List<CourseCertificate>
        {
            new()
            {
                StudentId = students[2].StudentId,
                CourseId = courses[0].Id,
                Title = "Complete Web Development Bootcamp Certificate",
                Link = "https://certificates.example.com/cert-12345",
                IssuedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1))
            },
        };

        context.CourseCertificates.AddRange(certificates);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Certificates seeded ({certificates.Count} certificates)");
    }

    private static async Task SeedAssignments(AppDbContext context)
    {
        if (context.Assignments.Any()) return;

        var modules = await context.Modules.OrderBy(m => m.CourseId).ThenBy(m => m.Order).ToListAsync();
        if (modules.Count == 0) return;

        var assignments = modules.Where(m => m.Order > 1).Select(m => new Assignment
        {
            ModuleId = m.ModuleId,
            Title = $"{m.Title} - Practical Assignment",
            Instruction = $"Complete the hands-on project for {m.Title}."
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

        for (int i = 0; i < lessons.Count; i += 3)
        {
            var type = i % 9;
            if (type < 3)
                resources.Add(new PdfResource { LessonId = lessons[i].LessonId, Title = "Lesson Notes", Url = $"https://example.com/pdf/lesson-{lessons[i].LessonId}.pdf" });
            else if (type < 6)
                resources.Add(new UrlResource { LessonId = lessons[i].LessonId, Title = "Additional Reading", Url = $"https://example.com/articles/lesson-{lessons[i].LessonId}" });
            else
                resources.Add(new ZipResource { LessonId = lessons[i].LessonId, Title = "Starter Files", Url = $"https://example.com/zip/lesson-{lessons[i].LessonId}.zip" });
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

        var notifications = new List<Notification>
        {
            new() { UserId = users[0].Id, Title = "Welcome to Masar!", Message = "Start your learning journey today.", Url = "/student/dashboard", CreatedAt = DateTime.Now.AddDays(-5) },
            new() { UserId = users[0].Id, Title = "New Course Available", Message = "Check out our new Advanced React course.", Url = "/browse-courses", CreatedAt = DateTime.Now.AddDays(-2) },
            new() { UserId = users[1].Id, Title = "Complete Your Profile", Message = "Add your skills to get personalized recommendations.", Url = "/student/profile", CreatedAt = DateTime.Now.AddDays(-3) },
            new() { UserId = null, Title = "System Update", Message = "New features have been added to the platform.", Url = "/", CreatedAt = DateTime.Now.AddDays(-1) },
        };

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Notifications seeded ({notifications.Count} notifications)");
    }

    private static async Task SeedInstructorAsync(UserManager<User> userManager, AppDbContext context,
        string email, string firstName, string lastName, string bio, int yearsOfExperience)
    {
        if (await userManager.FindByEmailAsync(email) != null) return;

        var user = new User { UserName = email, Email = email, FirstName = firstName, LastName = lastName, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, "Test@123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Student");
            await userManager.AddToRoleAsync(user, "Instructor");
            context.StudentProfiles.Add(new StudentProfile { UserId = user.Id, Bio = bio });
            context.InstructorProfiles.Add(new InstructorProfile { UserId = user.Id, Bio = bio, YearsOfExperience = yearsOfExperience });
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedStudentAsync(UserManager<User> userManager, AppDbContext context,
        string email, string firstName, string lastName, string bio)
    {
        if (await userManager.FindByEmailAsync(email) != null) return;

        var user = new User { UserName = email, Email = email, FirstName = firstName, LastName = lastName, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, "Test@123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Student");
            await userManager.AddToRoleAsync(user, "Instructor");
            context.StudentProfiles.Add(new StudentProfile { UserId = user.Id, Bio = bio });
            context.InstructorProfiles.Add(new InstructorProfile { UserId = user.Id, Bio = "New instructor", YearsOfExperience = 0 });
            await context.SaveChangesAsync();
        }
    }
}