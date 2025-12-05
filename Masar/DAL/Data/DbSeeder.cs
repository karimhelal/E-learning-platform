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
        //return;

        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Check if already seeded
            if (context.Users.Any() && context.Courses.Any() && context.Modules.Any())
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
            // STEP 5: Seed Courses and Tracks
            // ============================================
            await SeedCoursesAndTracks(context);

            // ============================================
            // STEP 6: Seed Modules
            // ============================================
            await SeedModules(context);

            // ============================================
            // STEP 7: Seed Lessons
            // ============================================
            await SeedLessons(context);

            // ============================================
            // STEP 8: Seed Lesson Contents
            // ============================================
            await SeedLessonContents(context);

            // ============================================
            // STEP 9: Seed Course Learning Outcomes
            // ============================================
            await SeedLearningOutcomes(context);

            // ============================================
            // STEP 10: Seed Enrollments (BOTH Course and Track)
            // ============================================
            await SeedEnrollments(context);

            // ============================================
            // STEP 11: Seed Lesson Progress
            // ============================================
            await SeedLessonProgress(context);

            // ============================================
            // STEP 12: Seed Certificates
            // ============================================
            await SeedCertificates(context);

            // ============================================
            // STEP 13: Seed Assignments
            // ============================================
            await SeedAssignments(context);

            // ============================================
            // STEP 14: Seed Lesson Resources
            // ============================================
            await SeedLessonResources(context);

            Console.WriteLine("? Database seeded successfully!");
            Console.WriteLine("========================================");
            Console.WriteLine("Seeded Data Summary:");
            Console.WriteLine($"- Users: {await context.Users.CountAsync()}");
            Console.WriteLine($"- Instructors: {await context.InstructorProfiles.CountAsync()}");
            Console.WriteLine($"- Students: {await context.StudentProfiles.CountAsync()}");
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
            Console.WriteLine($"- Total Enrollments: {await context.Enrollments.CountAsync()}");
            Console.WriteLine($"- Lesson Progress: {await context.LessonProgress.CountAsync()}");
            Console.WriteLine($"- Certificates: {await context.CourseCertificates.CountAsync() + await context.TrackCertificates.CountAsync()}");
            Console.WriteLine($"- Assignments: {await context.Assignments.CountAsync()}");
            Console.WriteLine($"- Lesson Resources: {await context.LessonResources.CountAsync()}");
            Console.WriteLine("========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? ERROR during seeding: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    private static async Task SeedRoles(RoleManager<IdentityRole<int>> roleManager)
    {
        string[] roles = { "Student", "Instructor", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }
        Console.WriteLine("? Roles seeded");
    }

    private static async Task SeedCategories(AppDbContext context)
    {
        if (context.Categories.Any()) return;

        var categories = new List<Category>
            {
                // Root Categories
                new Category { Name = "Development", Slug = "development" },
                new Category { Name = "Data Science", Slug = "data-science" },
                new Category { Name = "Design", Slug = "design" },
                new Category { Name = "Business", Slug = "business" },
                new Category { Name = "Marketing", Slug = "marketing" },
            };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // Sub-categories
        var development = await context.Categories.FirstAsync(c => c.Slug == "development");
        var subCategories = new List<Category>
            {
                new Category { Name = "Web Development", Slug = "web-development", ParentCategoryId = development.CategoryId },
                new Category { Name = "Mobile Development", Slug = "mobile-development", ParentCategoryId = development.CategoryId },
                new Category { Name = "Programming Languages", Slug = "programming-languages", ParentCategoryId = development.CategoryId },
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
                new Language { Name = "English", Slug = "english" },
                new Language { Name = "Arabic", Slug = "arabic" },
                new Language { Name = "Spanish", Slug = "spanish" },
                new Language { Name = "French", Slug = "french" },
            };

        context.Languages.AddRange(languages);
        await context.SaveChangesAsync();
        Console.WriteLine("? Languages seeded");
    }

    private static async Task SeedUsers(UserManager<User> userManager, AppDbContext context)
    {
        // Seed Instructors
        await SeedInstructorAsync(userManager, context, "john.instructor@example.com", "John", "Doe",
            "Expert web developer with 10+ years of experience in full-stack development. Passionate about teaching modern web technologies.", 10);

        await SeedInstructorAsync(userManager, context, "sarah.instructor@example.com", "Sarah", "Williams",
            "Data science professional specializing in machine learning and AI. Former researcher at top tech companies.", 8);

        await SeedInstructorAsync(userManager, context, "mike.instructor@example.com", "Mike", "Chen",
            "Mobile app developer and UI/UX designer. Created 50+ successful mobile applications.", 6);

        // Seed Students
        await SeedStudentAsync(userManager, context, "alice.student@example.com", "Alice", "Johnson",
            "Aspiring web developer with a passion for learning new technologies.");

        await SeedStudentAsync(userManager, context, "bob.student@example.com", "Bob", "Smith",
            "Computer science student interested in data science and machine learning.");

        await SeedStudentAsync(userManager, context, "charlie.student@example.com", "Charlie", "Brown",
            "Career switcher learning software development.");

        await SeedStudentAsync(userManager, context, "diana.student@example.com", "Diana", "Prince",
            "Experienced designer transitioning to front-end development.");

        await SeedStudentAsync(userManager, context, "emma.student@example.com", "Emma", "Watson",
            "Software engineering student looking to expand my skill set.");

        Console.WriteLine("? Users seeded");
    }

    private static async Task SeedCoursesAndTracks(AppDbContext context)
    {
        if (context.Courses.Any())
        {
            Console.WriteLine("? Courses already seeded (skipping)");
            return;
        }

        var instructors = await context.InstructorProfiles.OrderBy(i => i.InstructorId).ToListAsync();
        if (!instructors.Any())
        {
            Console.WriteLine("? No instructors found! Cannot seed courses.");
            return;
        }

        // Create Courses
        var courses = new List<Course>
            {
                new Course
                {
                    Title = "Complete Web Development Bootcamp",
                    Description = "Master web development from scratch. Learn HTML, CSS, JavaScript, React, Node.js, and build real-world projects.",
                    Level = CourseLevel.Beginner,
                    InstructorId = instructors[0].InstructorId,
                    ThumbnailImageUrl = "https://images.unsplash.com/photo-1498050108023-c5249f4df085",
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6))
                },
                new Course
                {
                    Title = "Advanced React and Redux",
                    Description = "Take your React skills to the next level with advanced patterns, hooks, context, and Redux state management.",
                    Level = CourseLevel.Advanced,
                    InstructorId = instructors[0].InstructorId,
                    ThumbnailImageUrl = "https://images.unsplash.com/photo-1633356122544-f134324a6cee",
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-5))
                },
                new Course
                {
                    Title = "Python for Data Science",
                    Description = "Learn Python programming with focus on data analysis. Master NumPy, Pandas, and Matplotlib.",
                    Level = CourseLevel.Beginner,
                    InstructorId = instructors[1].InstructorId,
                    ThumbnailImageUrl = "https://images.unsplash.com/photo-1526379095098-d400fd0bf935",
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4))
                },
                new Course
                {
                    Title = "Machine Learning Fundamentals",
                    Description = "Complete hands-on machine learning course. Learn regression, classification, and neural networks.",
                    Level = CourseLevel.Intermediate,
                    InstructorId = instructors[1].InstructorId,
                    ThumbnailImageUrl = "https://images.unsplash.com/photo-1555949963-aa79dcee981c",
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-3))
                },
                new Course
                {
                    Title = "iOS App Development with Swift",
                    Description = "Build iOS applications from scratch using Swift and SwiftUI. Learn Xcode and publish to App Store.",
                    Level = CourseLevel.Intermediate,
                    InstructorId = instructors[2].InstructorId,
                    ThumbnailImageUrl = "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c",
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2))
                },
                new Course
                {
                    Title = "UI/UX Design Fundamentals",
                    Description = "Master user interface and user experience design. Learn Figma, wireframing, and prototyping.",
                    Level = CourseLevel.Beginner,
                    InstructorId = instructors[2].InstructorId,
                    ThumbnailImageUrl = "https://images.unsplash.com/photo-1561070791-2526d30994b5",
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1))
                },
            };

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();

        // Create Tracks
        var tracks = new List<Track>
            {
                new Track
                {
                    Title = "Full Stack Web Developer Track",
                    Description = "Complete learning path to become a professional full-stack web developer.",
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6))
                },
                new Track
                {
                    Title = "Data Science Professional Track",
                    Description = "Comprehensive track covering Python, statistics, machine learning, and data visualization.",
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-5))
                },
            };

        context.Tracks.AddRange(tracks);
        await context.SaveChangesAsync();

        // Reload courses from database with navigation properties
        var savedCourses = await context.Courses
            .Include(c => c.Categories)
            .Include(c => c.Languages)
            .OrderBy(c => c.CreatedDate)
            .ToListAsync();

        var savedTracks = await context.Tracks.OrderBy(t => t.CreatedDate).ToListAsync();

        // Link Courses to Categories and Languages
        var webDevCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "web-development");
        var dataScienceCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "data-science");
        var englishLanguage = await context.Languages.FirstOrDefaultAsync(l => l.Slug == "english");

        if (webDevCategory != null && englishLanguage != null)
        {
            foreach (var course in savedCourses.Take(2))
            {
                course.Categories.Add(webDevCategory);
                course.Languages.Add(englishLanguage);
            }
        }

        if (dataScienceCategory != null && englishLanguage != null)
        {
            foreach (var course in savedCourses.Skip(2).Take(2))
            {
                course.Categories.Add(dataScienceCategory);
                course.Languages.Add(englishLanguage);
            }
        }

        await context.SaveChangesAsync();

        // Link Tracks to Courses
        if (savedTracks.Count >= 2 && savedCourses.Count >= 4)
        {
            context.TrackCourses.AddRange(new[]
            {
                    new Track_Course { TrackId = savedTracks[0].Id, CourseId = savedCourses[0].Id },
                    new Track_Course { TrackId = savedTracks[0].Id, CourseId = savedCourses[1].Id },
                    new Track_Course { TrackId = savedTracks[1].Id, CourseId = savedCourses[2].Id },
                    new Track_Course { TrackId = savedTracks[1].Id, CourseId = savedCourses[3].Id },
                });

            await context.SaveChangesAsync();
        }

        Console.WriteLine($"? Courses and Tracks seeded ({savedCourses.Count} courses, {savedTracks.Count} tracks)");
    }

    private static async Task SeedModules(AppDbContext context)
    {
        if (context.Modules.Any())
        {
            Console.WriteLine("? Modules already seeded (skipping)");
            return;
        }

        // Use AsNoTracking and explicit query
        var courses = await context.Courses
            .AsNoTracking()
            .OrderBy(c => c.CreatedDate)
            .Select(c => new { c.Id, c.Title })
            .ToListAsync();

        if (!courses.Any())
        {
            Console.WriteLine("? No courses found! Cannot seed modules.");
            return;
        }

        Console.WriteLine($"Found {courses.Count} courses to create modules for");

        var modules = new List<Module>();

        // Course 1: Web Development Bootcamp (6 modules)
        if (courses.Count > 0)
        {
            modules.AddRange(new[]
            {
                    new Module { CourseId = courses[0].Id, Title = "Introduction to Web Development", Description = "Get started with web development fundamentals", Order = 1 },
                    new Module { CourseId = courses[0].Id, Title = "HTML & CSS Mastery", Description = "Learn HTML5 and CSS3 to build beautiful web pages", Order = 2 },
                    new Module { CourseId = courses[0].Id, Title = "JavaScript Fundamentals", Description = "Master JavaScript basics and ES6+ features", Order = 3 },
                    new Module { CourseId = courses[0].Id, Title = "React for Beginners", Description = "Introduction to React library and components", Order = 4 },
                    new Module { CourseId = courses[0].Id, Title = "Backend with Node.js", Description = "Build server-side applications with Node.js", Order = 5 },
                    new Module { CourseId = courses[0].Id, Title = "Full Stack Project", Description = "Build a complete full-stack application", Order = 6 },
                });
        }

        // Course 2: Advanced React (4 modules)
        if (courses.Count > 1)
        {
            modules.AddRange(new[]
            {
                    new Module { CourseId = courses[1].Id, Title = "Advanced React Patterns", Description = "Learn advanced React patterns and best practices", Order = 1 },
                    new Module { CourseId = courses[1].Id, Title = "React Hooks Deep Dive", Description = "Master all React hooks and create custom hooks", Order = 2 },
                    new Module { CourseId = courses[1].Id, Title = "Redux State Management", Description = "Implement Redux for complex state management", Order = 3 },
                    new Module { CourseId = courses[1].Id, Title = "Performance Optimization", Description = "Optimize React applications for production", Order = 4 },
                });
        }

        // Course 3: Python for Data Science (5 modules)
        if (courses.Count > 2)
        {
            modules.AddRange(new[]
            {
                    new Module { CourseId = courses[2].Id, Title = "Python Programming Basics", Description = "Learn Python syntax and fundamentals", Order = 1 },
                    new Module { CourseId = courses[2].Id, Title = "NumPy for Numerical Computing", Description = "Master NumPy for efficient computations", Order = 2 },
                    new Module { CourseId = courses[2].Id, Title = "Data Analysis with Pandas", Description = "Perform data manipulation with Pandas", Order = 3 },
                    new Module { CourseId = courses[2].Id, Title = "Data Visualization", Description = "Create visualizations with Matplotlib", Order = 4 },
                    new Module { CourseId = courses[2].Id, Title = "Statistical Analysis", Description = "Apply statistical methods to data", Order = 5 },
                });
        }

        // Course 4: Machine Learning (4 modules)
        if (courses.Count > 3)
        {
            modules.AddRange(new[]
            {
                    new Module { CourseId = courses[3].Id, Title = "Introduction to Machine Learning", Description = "Understanding ML concepts and terminology", Order = 1 },
                    new Module { CourseId = courses[3].Id, Title = "Supervised Learning", Description = "Learn regression and classification", Order = 2 },
                    new Module { CourseId = courses[3].Id, Title = "Unsupervised Learning", Description = "Explore clustering algorithms", Order = 3 },
                    new Module { CourseId = courses[3].Id, Title = "Neural Networks", Description = "Introduction to deep learning", Order = 4 },
                });
        }

        if (modules.Any())
        {
            context.Modules.AddRange(modules);
            await context.SaveChangesAsync();
            Console.WriteLine($"? Modules seeded ({modules.Count} modules created)");
        }
        else
        {
            Console.WriteLine("? No modules created");
        }
    }

    private static async Task SeedLessons(AppDbContext context)
    {
        if (context.Lessons.Any())
        {
            Console.WriteLine("? Lessons already seeded (skipping)");
            return;
        }

        var modules = await context.Modules
            .AsNoTracking()
            .OrderBy(m => m.CourseId)
            .ThenBy(m => m.Order)
            .Select(m => new { m.ModuleId, m.CourseId, m.Order })
            .ToListAsync();

        if (!modules.Any() || modules.Count < 11)
        {
            Console.WriteLine($"? Not enough modules found ({modules.Count}). Cannot seed lessons.");
            return;
        }

        var lessons = new List<Lesson>
            {
                // Module 1: Introduction (3 lessons)
                new Lesson { ModuleId = modules[0].ModuleId, Title = "What is Web Development?", ContentType = LessonContentType.Video, Order = 1 },
                new Lesson { ModuleId = modules[0].ModuleId, Title = "Setting Up Your Development Environment", ContentType = LessonContentType.Video, Order = 2 },
                new Lesson { ModuleId = modules[0].ModuleId, Title = "Your First HTML Page", ContentType = LessonContentType.Video, Order = 3 },

                // Module 2: HTML & CSS (5 lessons)
                new Lesson { ModuleId = modules[1].ModuleId, Title = "HTML5 Structure and Semantic Tags", ContentType = LessonContentType.Video, Order = 1 },
                new Lesson { ModuleId = modules[1].ModuleId, Title = "CSS Fundamentals and Selectors", ContentType = LessonContentType.Video, Order = 2 },
                new Lesson { ModuleId = modules[1].ModuleId, Title = "Flexbox Layout", ContentType = LessonContentType.Video, Order = 3 },
                new Lesson { ModuleId = modules[1].ModuleId, Title = "CSS Grid System", ContentType = LessonContentType.Video, Order = 4 },
                new Lesson { ModuleId = modules[1].ModuleId, Title = "Responsive Design with Media Queries", ContentType = LessonContentType.Video, Order = 5 },

                // Module 3: JavaScript (6 lessons)
                new Lesson { ModuleId = modules[2].ModuleId, Title = "JavaScript Basics: Variables and Data Types", ContentType = LessonContentType.Video, Order = 1 },
                new Lesson { ModuleId = modules[2].ModuleId, Title = "Functions and Scope", ContentType = LessonContentType.Video, Order = 2 },
                new Lesson { ModuleId = modules[2].ModuleId, Title = "Arrays and Objects", ContentType = LessonContentType.Video, Order = 3 },
                new Lesson { ModuleId = modules[2].ModuleId, Title = "DOM Manipulation", ContentType = LessonContentType.Video, Order = 4 },
                new Lesson { ModuleId = modules[2].ModuleId, Title = "ES6+ Features", ContentType = LessonContentType.Video, Order = 5 },
                new Lesson { ModuleId = modules[2].ModuleId, Title = "Asynchronous JavaScript", ContentType = LessonContentType.Video, Order = 6 },

                // Python Module (4 lessons)
                new Lesson { ModuleId = modules[10].ModuleId, Title = "Python Installation and Setup", ContentType = LessonContentType.Video, Order = 1 },
                new Lesson { ModuleId = modules[10].ModuleId, Title = "Variables and Data Types", ContentType = LessonContentType.Video, Order = 2 },
                new Lesson { ModuleId = modules[10].ModuleId, Title = "Control Flow and Loops", ContentType = LessonContentType.Video, Order = 3 },
                new Lesson { ModuleId = modules[10].ModuleId, Title = "Functions and Modules", ContentType = LessonContentType.Video, Order = 4 },
            };

        context.Lessons.AddRange(lessons);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Lessons seeded ({lessons.Count} lessons)");
    }

    private static async Task SeedLessonContents(AppDbContext context)
    {
        if (context.LessonContents.Any())
        {
            Console.WriteLine("? Lesson contents already seeded (skipping)");
            return;
        }

        var lessons = await context.Lessons.AsNoTracking().ToListAsync();

        if (!lessons.Any())
        {
            Console.WriteLine("? No lessons found! Cannot seed lesson contents.");
            return;
        }

        var lessonContents = new List<LessonContent>();

        foreach (var lesson in lessons)
        {
            lessonContents.Add(new VideoContent
            {
                LessonId = lesson.LessonId,
                VideoUrl = $"https://example.com/videos/lesson{lesson.LessonId}.mp4",
                DurationInSeconds = 600 + (lesson.Order * 300)
            });
        }

        context.LessonContents.AddRange(lessonContents);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Lesson contents seeded ({lessonContents.Count} contents)");
    }

    private static async Task SeedLearningOutcomes(AppDbContext context)
    {
        if (context.CourseLearningOutcomes.Any())
        {
            Console.WriteLine("? Learning outcomes already seeded (skipping)");
            return;
        }

        var courses = await context.Courses
            .AsNoTracking()
            .OrderBy(c => c.CreatedDate)
            .Select(c => c.Id)
            .ToListAsync();

        if (!courses.Any() || courses.Count < 3)
        {
            Console.WriteLine("? Not enough courses found! Cannot seed learning outcomes.");
            return;
        }

        var outcomes = new List<CourseLearningOutcome>
            {
                // Course 1
                new CourseLearningOutcome { Id = 1, CourseId = courses[0], Title = "Build responsive websites", Description = "Build responsive websites with HTML5 and CSS3" },
                new CourseLearningOutcome { Id = 2, CourseId = courses[0], Title = "Master JavaScript", Description = "Master JavaScript fundamentals and ES6+ features" },
                new CourseLearningOutcome { Id = 3, CourseId = courses[0], Title = "Create React apps", Description = "Create dynamic web applications with React" },
                new CourseLearningOutcome { Id = 4, CourseId = courses[0], Title = "Build REST APIs", Description = "Build RESTful APIs with Node.js and Express" },

                // Course 2
                new CourseLearningOutcome { Id = 1, CourseId = courses[1], Title = "Master React Hooks", Description = "Master React Hooks and Context API" },
                new CourseLearningOutcome { Id = 2, CourseId = courses[1], Title = "Implement Redux", Description = "Implement Redux for state management" },
                new CourseLearningOutcome { Id = 3, CourseId = courses[1], Title = "Optimize performance", Description = "Optimize React application performance" },

                // Course 3
                new CourseLearningOutcome { Id = 1, CourseId = courses[2], Title = "Master Python", Description = "Master Python programming fundamentals" },
                new CourseLearningOutcome { Id = 2, CourseId = courses[2], Title = "Data analysis", Description = "Perform data analysis with Pandas" },
                new CourseLearningOutcome { Id = 3, CourseId = courses[2], Title = "Data visualization", Description = "Create data visualizations with Matplotlib" },
            };

        context.CourseLearningOutcomes.AddRange(outcomes);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Learning outcomes seeded ({outcomes.Count} outcomes)");
    }

    private static async Task SeedEnrollments(AppDbContext context)
    {
        if (context.Enrollments.Any())
        {
            Console.WriteLine("? Enrollments already seeded (skipping)");
            return;
        }

        var students = await context.StudentProfiles.OrderBy(s => s.StudentId).ToListAsync();
        var courses = await context.Courses.OrderBy(c => c.CreatedDate).ToListAsync();
        var tracks = await context.Tracks.OrderBy(t => t.CreatedDate).ToListAsync();

        var enrollments = new List<EnrollmentBase>();

        if (students.Count >= 3 && courses.Count >= 3)
        {
            enrollments.AddRange(new List<CourseEnrollment>
                {
                    new CourseEnrollment
                    {
                        StudentId = students[0].StudentId,
                        CourseId = courses[0].Id,
                        EnrollmentDate = DateTime.Now.AddMonths(-3),
                        Status = EnrollmentStatus.Active,
                        ProgressPercentage = 75.50m
                    },
                    new CourseEnrollment
                    {
                        StudentId = students[0].StudentId,
                        CourseId = courses[1].Id,
                        EnrollmentDate = DateTime.Now.AddMonths(-2),
                        Status = EnrollmentStatus.Active,
                        ProgressPercentage = 30.00m
                    },
                    new CourseEnrollment
                    {
                        StudentId = students[1].StudentId,
                        CourseId = courses[0].Id,
                        EnrollmentDate = DateTime.Now.AddMonths(-2),
                        Status = EnrollmentStatus.Active,
                        ProgressPercentage = 45.25m
                    },
                    new CourseEnrollment
                    {
                        StudentId = students[1].StudentId,
                        CourseId = courses[2].Id,
                        EnrollmentDate = DateTime.Now.AddMonths(-3),
                        Status = EnrollmentStatus.Active,
                        ProgressPercentage = 80.00m
                    },
                    new CourseEnrollment
                    {
                        StudentId = students[2].StudentId,
                        CourseId = courses[0].Id,
                        EnrollmentDate = DateTime.Now.AddMonths(-4),
                        Status = EnrollmentStatus.Completed,
                        ProgressPercentage = 100.00m
                    },
                    new CourseEnrollment
                    {
                        StudentId = students[2].StudentId,
                        CourseId = courses[2].Id,
                        EnrollmentDate = DateTime.Now.AddMonths(-1),
                        Status = EnrollmentStatus.Active   ,
                        ProgressPercentage = 65.50m
                    }
                });
        }

        if (students.Count >= 2 && tracks.Count >= 2)
        {
            enrollments.AddRange(new List<TrackEnrollment>
                {
                    new TrackEnrollment
                    {
                        StudentId = students[0].StudentId,
                        TrackId = tracks[0].Id,
                        EnrollmentDate = DateTime.Now.AddMonths(-3),
                        Status = EnrollmentStatus.Active,
                        ProgressPercentage = 55.00m
                    },
                    new TrackEnrollment
                    {
                        StudentId = students[1].StudentId,
                        TrackId = tracks[1].Id,
                        EnrollmentDate = DateTime.Now.AddMonths(-3),
                        Status = EnrollmentStatus.  Active,
                        ProgressPercentage = 70.50m
                    }
                });
        }

        if (enrollments.Any())
        {
            context.Enrollments.AddRange(enrollments);
            await context.SaveChangesAsync();
            Console.WriteLine($"? Enrollments seeded ({enrollments.Count} total: {enrollments.OfType<CourseEnrollment>().Count()} course, {enrollments.OfType<TrackEnrollment>().Count()} track)");
        }
        else
        {
            Console.WriteLine("? No enrollments created");
        }
    }

    private static async Task SeedLessonProgress(AppDbContext context)
    {
        if (context.LessonProgress.Any())
        {
            Console.WriteLine("? Lesson progress already seeded (skipping)");
            return;
        }

        var students = await context.StudentProfiles.OrderBy(s => s.StudentId).ToListAsync();
        var lessons = await context.Lessons.OrderBy(l => l.LessonId).Take(10).ToListAsync();

        if (!students.Any() || !lessons.Any())
        {
            Console.WriteLine("? Skipping lesson progress (no students or lessons)");
            return;
        }

        var progressList = new List<LessonProgress>();

        if (students.Count > 0)
        {
            for (int i = 0; i < Math.Min(7, lessons.Count); i++)
            {
                progressList.Add(new LessonProgress
                {
                    StudentId = students[0].StudentId,
                    LessonId = lessons[i].LessonId,
                    IsCompleted = true,
                    StartedDate = DateTime.Now.AddDays(-30 + i),
                    CompletedDate = DateTime.Now.AddDays(-30 + i + 1)
                });
            }
        }

        if (students.Count > 1)
        {
            for (int i = 0; i < Math.Min(5, lessons.Count); i++)
            {
                progressList.Add(new LessonProgress
                {
                    StudentId = students[1].StudentId,
                    LessonId = lessons[i].LessonId,
                    IsCompleted = true,
                    StartedDate = DateTime.Now.AddDays(-20 + i),
                    CompletedDate = DateTime.Now.AddDays(-20 + i + 1)
                });
            }
        }

        if (progressList.Any())
        {
            context.LessonProgress.AddRange(progressList);
            await context.SaveChangesAsync();
            Console.WriteLine($"? Lesson progress seeded ({progressList.Count} records)");
        }
    }

    private static async Task SeedCertificates(AppDbContext context)
    {
        if (context.CourseCertificates.Any())
        {
            Console.WriteLine("? Certificates already seeded (skipping)");
            return;
        }

        var students = await context.StudentProfiles.OrderBy(s => s.StudentId).ToListAsync();
        var courses = await context.Courses.OrderBy(c => c.CreatedDate).ToListAsync();

        if (!students.Any() || !courses.Any())
        {
            Console.WriteLine("? Skipping certificates (no students or courses)");
            return;
        }

        var certificates = new List<CourseCertificate>
            {
                new CourseCertificate
                {
                    StudentId = students.Count > 2 ? students[2].StudentId : students[0].StudentId,
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
        if (context.Assignments.Any())
        {
            Console.WriteLine("? Assignments already seeded (skipping)");
            return;
        }

        var modules = await context.Modules.OrderBy(m => m.CourseId).ThenBy(m => m.Order).Take(5).ToListAsync();

        if (!modules.Any() || modules.Count < 3)
        {
            Console.WriteLine("? Skipping assignments (not enough modules)");
            return;
        }

        var assignments = new List<Assignment>
            {
                new Assignment
                {
                    ModuleId = modules[1].ModuleId,
                    Title = "Build a Personal Portfolio Page",
                    Instruction = "Create a responsive portfolio website using HTML and CSS. Include sections for About, Projects, and Contact."
                },
                new Assignment
                {
                    ModuleId = modules[2].ModuleId,
                    Title = "JavaScript Calculator Project",
                    Instruction = "Build a functional calculator using vanilla JavaScript. Implement all basic operations."
                },
                new Assignment
                {
                    ModuleId = modules.Count > 3 ? modules[3].ModuleId : modules[2].ModuleId,
                    Title = "Todo App with React",
                    Instruction = "Create a todo list application with React. Include add, delete, and mark as complete features."
                },
            };

        context.Assignments.AddRange(assignments);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Assignments seeded ({assignments.Count} assignments)");
    }

    private static async Task SeedLessonResources(AppDbContext context)
    {
        if (context.LessonResources.Any())
        {
            Console.WriteLine("? Lesson resources already seeded (skipping)");
            return;
        }

        var lessons = await context.Lessons.OrderBy(l => l.LessonId).Take(5).ToListAsync();

        if (!lessons.Any() || lessons.Count < 3)
        {
            Console.WriteLine("? Skipping lesson resources (not enough lessons)");
            return;
        }

        var resources = new List<LessonResource>
            {
                new PdfResource
                {
                    LessonId = lessons[0].LessonId,
                    Url = "https://example.com/resources/web-dev-intro.pdf"
                },
                new UrlResource
                {
                    LessonId = lessons[1].LessonId,
                    Url = "https://code.visualstudio.com/download"
                },
                new ZipResource
                {
                    LessonId = lessons[2].LessonId,
                    Url = "https://example.com/resources/html-templates.zip"
                },
            };

        context.LessonResources.AddRange(resources);
        await context.SaveChangesAsync();
        Console.WriteLine($"? Lesson resources seeded ({resources.Count} resources)");
    }

    private static async Task SeedInstructorAsync(UserManager<User> userManager, AppDbContext context,
        string email, string firstName, string lastName, string bio, int yearsOfExperience)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Test@123");
            if (result.Succeeded)
            {
                // Add BOTH roles to every user
                await userManager.AddToRoleAsync(user, "Student");
                await userManager.AddToRoleAsync(user, "Instructor");

                // Create StudentProfile
                var studentProfile = new StudentProfile
                {
                    UserId = user.Id,
                    Bio = bio
                };
                context.StudentProfiles.Add(studentProfile);

                // Create InstructorProfile
                var instructorProfile = new InstructorProfile
                {
                    UserId = user.Id,
                    Bio = bio,
                    YearsOfExperience = yearsOfExperience
                };
                context.InstructorProfiles.Add(instructorProfile);

                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedStudentAsync(UserManager<User> userManager, AppDbContext context,
        string email, string firstName, string lastName, string bio)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Test@123");
            if (result.Succeeded)
            {
                // Add BOTH roles to every user
                await userManager.AddToRoleAsync(user, "Student");
                await userManager.AddToRoleAsync(user, "Instructor");

                // Create StudentProfile
                var studentProfile = new StudentProfile
                {
                    UserId = user.Id,
                    Bio = bio
                };
                context.StudentProfiles.Add(studentProfile);

                // Create InstructorProfile (with default values)
                var instructorProfile = new InstructorProfile
                {
                    UserId = user.Id,
                    Bio = "New instructor on the platform",
                    YearsOfExperience = 0
                };
                context.InstructorProfiles.Add(instructorProfile);

                await context.SaveChangesAsync();
            }
        }
    }
}