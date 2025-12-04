using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Core.Entities.Enums;

namespace DAL.Data
{
    /// <summary>
    /// Database context for E-Learning Platform
    /// Configures all entities and their relationships
    /// </summary>
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        // DbSets - Entity Collections
        //public DbSet<User> Users { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<InstructorProfile> InstructorProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonContent> LessonContents { get; set; }
        public DbSet<LessonResource> LessonResources { get; set; }
        public DbSet<LessonProgress> LessonProgress { get; set; }
        public DbSet<CourseLearningOutcome> CourseLearningOutcomes { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Track_Course> TrackCourses { get; set; }
        public DbSet<EnrollmentBase> Enrollments { get; set; }
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
        public DbSet<TrackEnrollment> TrackEnrollments { get; set; }
        public DbSet<CourseCertificate> CourseCertificates { get; set; }
        public DbSet<TrackCertificate> TrackCertificates { get; set; }


        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
           
            modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                      .WithMany()
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<IdentityUserRole<int>>(entity =>
            {
                entity
                    .ToTable("UserRoles");

                entity
                    .Property(e => e.RoleId)
                    .HasColumnName("role_id");
                
                entity
                    .Property(e => e.UserId)
                    .HasColumnName("user_id");
            });

            modelBuilder.Entity<User>(entity =>
            {
                // User - Table Name
                entity.ToTable("Users");

                // User - Id
                entity
                    .Property(u => u.Id)
                    .HasColumnName("user_id");

                // User - Primary Key
                entity
                    .HasKey(u => u.Id);

                // User - Unique Constraint on Email - Unique constraint: Email must be unique across all users
                entity
                    .HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_User_Email");

                // User - Email
                entity
                    .Property(u => u.Email)
                    .IsRequired()
                    .HasColumnName("email");

                // User - StudentProfile (One-to-One)
                entity
                    .HasOne(u => u.StudentProfile)
                    .WithOne(sp => sp.User)
                    .HasForeignKey<StudentProfile>(sp => sp.UserId);

                // User - InstructorProfile (One-to-One)
                entity
                    .HasOne(u => u.InstructorProfile)
                    .WithOne(ip => ip.User)
                    .HasForeignKey<InstructorProfile>(ip => ip.UserId);

                // User - Password Hash
                entity
                    .Property(u => u.PasswordHash)
                    .HasColumnName("password_hash");
            });

            modelBuilder.Entity<StudentProfile>(entity =>
            {
                // StudentProfile - Table Name
                entity.ToTable("StudentProfiles");

                // StudentProfile - Primary Key
                entity.HasKey(s => s.StudentId);

                // StudentProfile - Unique Constraint on UserId - Unique constraint: no two StudentProfile should have the same UserId
                entity
                    .HasIndex(s => s.UserId)
                    .IsUnique()
                    .HasDatabaseName("IX_StudentProfile_UserId");
            });

            modelBuilder.Entity<InstructorProfile>(entity =>
            {
                // InstructorProfile - Table Name
                entity.ToTable("InstructorProfiles");

                // InstructorProfile - Primary Key
                entity.HasKey(i => i.InstructorId);

                // InstructorProfile - Unique Constraint on UserId - Unique constraint: no two InstructorProfile should have the same UserId
                entity
                    .HasIndex(i => i.UserId)
                    .IsUnique()
                    .HasDatabaseName("IX_InstructorProfile_UserId");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                // Category - Table Name
                entity.ToTable("Categories");

                // Category - Primary Key
                entity.HasKey(c => c.CategoryId);

                // Category - Category (One-to-Many)
                entity
                    .HasOne(c => c.ParentCategory)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                // Category - no two categories should have the same slug
                entity
                    .HasIndex(c => c.Slug)
                    .HasDatabaseName("IX_Category_Slug")
                    .IsUnique();
            });

            modelBuilder.Entity<Language>(entity =>
            {
                // Language - Table Name
                entity.ToTable("Languages");

                // Language - Primary Key
                entity.HasKey(l => l.LanguageId);

                // Language - no two languages should have the same slug
                entity
                    .HasIndex(c => c.Slug)
                    .HasDatabaseName("IX_Language_Slug")
                    .IsUnique();
            });

            modelBuilder.Entity<LearningEntity_Category>(entity =>
            {
                // LearningEntity_Category - Table Name
                entity.ToTable("LearningEntity_Category");

                // LearningEntity_Category - Composite Primary Key
                entity.HasKey(ec => new { ec.LearningEntityId, ec.CategoryId });

                // LearningEntity_Category - LearningEntity (One-to-Many)
                entity
                    .HasOne(ec => ec.LearningEntity)
                    .WithMany(l => l.LearningEntity_Categories)
                    .HasForeignKey(ec => ec.LearningEntityId)
                    .OnDelete(DeleteBehavior.Cascade);

                // LearningEntity_Category - Category (One-to-Many)
                entity
                    .HasOne(ec => ec.Category)
                    .WithMany(c => c.LearningEntity_Categories)
                    .HasForeignKey(ec => ec.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LearningEntity_Language>(entity =>
            {
                // LearningEntity_Language - Table Name
                entity.ToTable("LearningEntity_Language");

                // LearningEntity_Language - Composite Primary Key
                entity.HasKey(el => new { el.LearningEntityId, el.LanguageId });

                // LearningEntity_Language - LearningEntity (One-to-Many)
                entity
                    .HasOne(el => el.LearningEntity)
                    .WithMany(e => e.LearningEntity_Languages)
                    .HasForeignKey(el => el.LearningEntityId)
                    .OnDelete(DeleteBehavior.Cascade);

                // LearningEntity_Language - Language (One-to-Many)
                entity
                    .HasOne(el => el.Language)
                    .WithMany(l => l.LearningEntity_Languages)
                    .HasForeignKey(el => el.LanguageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LearningEntity>(entity =>
            {
                // LearningEntity - Table Name
                entity.ToTable("LearningEntities");

                // LearningEntity - Primary Key
                entity.HasKey(l => l.Id);

                // LearningEntity - Category (Many-to-Many)
                entity
                    .HasMany(l => l.Categories)
                    .WithMany()
                    .UsingEntity<LearningEntity_Category>();

                // LearningEntity - Language (Many-to-Many)
                entity
                    .HasMany(l => l.Languages)
                    .WithMany()
                    .UsingEntity<LearningEntity_Language>();

                entity
                    .Property(l => l.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                //entity
                //    .HasDiscriminator<string>("learning_entity_type")
                //    .HasValue<Course>("Course")
                //    .HasValue<Track>("Track");
            });

            modelBuilder.Entity<Track>(entity =>
            {
                // Track - Table Name
                entity.ToTable("Tracks");

                // Track - BaseType
                entity.HasBaseType<LearningEntity>();

                // Track - Course (Many-to-Many)
                entity
                    .HasMany(t => t.Courses)
                    .WithMany(c => c.Tracks)
                    .UsingEntity<Track_Course>();
            });

            modelBuilder.Entity<Course>(entity =>
            {
                // Course - Table Name
                entity.ToTable("Courses");

                // Course - BaseType
                entity.HasBaseType<LearningEntity>();

                // Course - InstructorProfile (One-to-Many)
                entity
                    .HasOne(c => c.Instructor)
                    .WithMany(i => i.OwnedCourses)
                    .HasForeignKey(c => c.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict);     // prevent deleting an instructor if he owns courses
            });

            modelBuilder.Entity<Module>(entity =>
            {
                // Module - Table Name
                entity.ToTable("Modules");

                // Module - Primary Key
                entity.HasKey(m => m.ModuleId);

                // Module - Unique Constraint on (CourseId, Order) - Unique constraint: Order must be unique within a Course
                entity
                    .HasIndex(m => new { m.CourseId, m.Order })
                    .IsUnique()
                    .HasDatabaseName("IX_Module_Course_Order");

                // Module - Course (One-to-Many - WEAK Entity)
                entity
                    .HasOne(m => m.Course)
                    .WithMany(c => c.Modules)
                    .HasForeignKey(m => m.CourseId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                // Lesson - Table Name
                entity.ToTable("Lessons");

                // Lesson - Primary Key
                entity.HasKey(l => l.LessonId);

                // Lesson - Unique Constraint on (ModuleId, Order) - Unique constraint: Order must be unique within a Module
                entity
                    .HasIndex(l => new { l.ModuleId, l.Order })
                    .IsUnique()
                    .HasDatabaseName("IX_Lesson_Module_Order");

                // Lesson - Module (One-to-Many - WEAK Entity)
                // TODO: deleting a module shouldn't enforce deleting its lessons (unless stated explicitly)
                entity
                    .HasOne(l => l.Module)
                    .WithMany(m => m.Lessons)
                    .HasForeignKey(l => l.ModuleId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LessonContent>(entity =>
            {
                // LessonContent - Table Name
                entity.ToTable("LessonContents");

                // LessonContent - Primary Key
                entity.HasKey(lc => lc.LessonContentId);

                // LesssonContent - Lesson (One-to-One -- Identyfying relationship)
                entity
                    .HasOne(lc => lc.Lesson)
                    .WithOne(l => l.LessonContent)
                    .HasForeignKey<LessonContent>(lc => lc.LessonId)
                    .OnDelete(DeleteBehavior.Cascade)   // Cascade delete when Lesson is deleted
                    .IsRequired();                      // Force total participation (LessonContent can't exist without an associated Lesson)

                // TPH - One table for all the content types
                entity
                    .HasDiscriminator<string>("content_type")
                    .HasValue<VideoContent>("Video")
                    .HasValue<ArticleContent>("Article");
            });

            modelBuilder.Entity<LessonResource>(entity =>
            {
                // LessonResource - Table Name
                entity.ToTable("LessonResources");

                // LessonResource - Primary Key
                entity.HasKey(lr => lr.LessonResourceId);

                // LessonResource - Lesson (One-to-Many)
                entity
                    .HasOne(lr => lr.Lesson)
                    .WithMany(l => l.LessonResources)
                    .HasForeignKey(lr => lr.LessonId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                // TPH (One table for all the derived types)
                entity
                    .HasDiscriminator<string>("resource_type")
                    .HasValue<PdfResource>(LessonResourceType.PDF.ToString())
                    .HasValue<ZipResource>(LessonResourceType.ZIP.ToString())
                    .HasValue<UrlResource>(LessonResourceType.URL.ToString());
            });

            modelBuilder.Entity<LessonProgress>(entity =>
            {
                // LessonProgress - Table Name
                entity.ToTable("LessonProgresses");

                // LessonProgress - Composite Primary Key
                entity.HasKey(lp => lp.LessonProgressId);

                // LessonProgress - Unique Contraint on (StudentId, LessonId) - Unique constraint: a LessonProgress can't repeat for the same combination of (StudentId, LessonId)
                entity
                    .HasIndex(lp => new { lp.StudentId, lp.LessonId })
                    .IsUnique()
                    .HasDatabaseName("IX_LessonProgress_Student_Lesson");

                // LessonProgress - Lesson (One-to-Many - WEAK Entity)
                entity
                    .HasOne(lp => lp.Lesson)
                    .WithMany(l => l.LessonProgresses)
                    .HasForeignKey(lp => lp.LessonId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                // LessonProgress - StudentProfile (One-to-Many - WEAK Entity)
                entity
                    .HasOne(lp => lp.Student)
                    .WithMany()
                    .HasForeignKey(lp => lp.StudentId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CourseLearningOutcome>(entity =>
            {
                // CourseLearningOutcome - Table Name
                entity.ToTable("CourseLearningOutcomes");

                // CourseLearningOutcome - Composite Primary Key
                entity.HasKey(co => new { co.Id, co.CourseId });

                // CourseLearningOutcome - Course (One-to-Many - WEAK Entity)
                entity
                    .HasOne(co => co.Course)
                    .WithMany(c => c.LearningOutcomes)
                    .HasForeignKey(co => co.CourseId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity<EnrollmentBase>(entity =>
            {
                // EnrollmentBase - Table Name
                entity.ToTable("Enrollments");

                // EnrollmentBase - Primary Key
                entity.HasKey(e => e.EnrollmentId);

                // Tell EF Core to use a specific precision and scale
                entity.Property(e => e.ProgressPercentage)
                      .HasPrecision(5, 2); // 5 total digits, 2 after the decimal

                // EnrollmentBase - StudentProfile (One-to-Many)
                entity
                    .HasOne(eb => eb.Student)
                    .WithMany(sp => sp.Enrollments)
                    .HasForeignKey(eb => eb.StudentId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of StudentProfile => he is enrolled in at least a course or a track

                entity
                    .HasDiscriminator<string>("enrollment_type")
                    .HasValue<CourseEnrollment>("CourseEnrollment")
                    .HasValue<TrackEnrollment>("TrackEnrollment");
            });

            modelBuilder.Entity<CourseEnrollment>(entity =>
            {
                // CourseEnrollment - BaseType
                entity.HasBaseType<EnrollmentBase>();

                // CourseEnrollment - Unique Contraint on (CourseId, StudentId) - Unique constraint: StudentId must not repeat for the same Course
                entity
                    .HasIndex(ce => new { ce.CourseId, ce.StudentId })
                    .IsUnique()
                    .HasDatabaseName("IX_CourseEnrollment_Course_Student");

                // CourseEnrollment -Course (One-to-Many)
                entity
                    .HasOne(ce => ce.Course)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(ce => ce.CourseId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of Course => Course have at least a student enrolled in
            });

            modelBuilder.Entity<TrackEnrollment>(entity =>
            {
                // TrackEnrollment - BaseType
                entity.HasBaseType<EnrollmentBase>();

                // TrackEnrollment - Unique Contraint on (TrackId, StudentId) - Unique constraint: StudentId must not repeat for the same Track
                entity
                    .HasIndex(te => new { te.TrackId, te.StudentId })
                    .IsUnique()
                    .HasDatabaseName("IX_TrackEnrollment_Track_Student");

                // TrackEnrollment - Track (One-to-Many)
                entity
                    .HasOne(te => te.Track)
                    .WithMany(t => t.Enrollments)
                    .HasForeignKey(te => te.TrackId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of Track => Track have at least a student enrolled in
            });

            modelBuilder.Entity<Track_Course>(entity =>
            {
                // Track_Course - Table Name
                entity.ToTable("Track_Course");

                // Track_Course - Composite Primary Key
                entity.HasKey(tc => new { tc.TrackId, tc.CourseId });

                // Track_Course - Track (One-to-Many)
                entity
                    .HasOne(tc => tc.Track)
                    .WithMany(t => t.TrackCourses)
                    .HasForeignKey(tc => tc.TrackId)
                    .OnDelete(DeleteBehavior.NoAction);         // when a track is deleted => trackId just becomes null (trackId: null, courseId: 2) => meaining that Course(id = 2) is not in a specific track

                // Track_Course - Course (One-to-Many)
                entity
                    .HasOne(tc => tc.Course)
                    .WithMany(c => c.CourseTracks)
                    .HasForeignKey(tc => tc.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);          // when a course is deleted => entry in TrackCourse table associating a course to a specific track gets deleted => there is no point in saying (trackId: 1, courseId: null) => doesn't make any sense
            });

            modelBuilder.Entity<CertificateBase>(entity =>
            {
                // CertificateBase - Table Name
                entity.ToTable("Certificates");

                // CertificateBase - Primary Key
                entity.HasKey(cb => cb.CertificateId);

                // CertificateBase - StudentProfile (One-to-Many)
                entity
                    .HasOne(c => c.Student)
                    .WithMany(sp => sp.Certificates)
                    .HasForeignKey(c => c.StudentId);

                entity
                    .HasDiscriminator<string>("cetificate_type")
                    .HasValue<CourseCertificate>("CourseCertificate")
                    .HasValue<TrackCertificate>("TrackCertificate");
            });

            modelBuilder.Entity<CourseCertificate>(entity =>
            {
                // CourseCertificate - BaseType
                entity.HasBaseType<CertificateBase>();

                // CourseCertificate - Course (One-to-Many)
                entity
                    .HasOne(cc => cc.Course)
                    .WithMany(c => c.Certificates)
                    .HasForeignKey(cc => cc.CourseId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<TrackCertificate>(entity =>
            {
                // TrackCertificate - BaseType
                entity.HasBaseType<CertificateBase>();

                // TrackCertificate - Track (One-to-Many)
                entity
                    .HasOne(tc => tc.Track)
                    .WithMany(t => t.Certificates)
                    .HasForeignKey(tc => tc.TrackId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Assignment>(entity =>
            {
                // Assignment - Table Name
                entity.ToTable("Assignments");

                // Assignment - Primary Key
                entity.HasKey(a => a.AssignmentId);

                // Assignments - Module (One-to-Many - WEAK Entity)
                entity
                    .HasOne(a => a.Module)
                    .WithMany(m => m.Assignments)
                    .HasForeignKey(a => a.ModuleId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // ==================== MODELS KEYS CONFIGURATIONS ====================

            #region Primary Keys & Composite Keys & Partial Keys & Unique Constraints

            //// User - Primary Key
            //modelBuilder.Entity<User>()
            //    .HasKey(u => u.UserId);

            //// User - Unique Constraint on Email - Unique constraint: Email must be unique across all users
            //modelBuilder.Entity<User>()
            //    .HasIndex(u => u.Email)
            //    .IsUnique()
            //    .HasDatabaseName("IX_User_Email");



            //// StudentProfile - Primary Key
            //modelBuilder.Entity<StudentProfile>()
            //    .HasKey(s => s.StudentId);

            //// StudentProfile - Unique Constraint on UserId - Unique constraint: no two StudentProfile should have the same UserId
            //modelBuilder.Entity<StudentProfile>()
            //    .HasIndex(s => s.UserId)
            //    .IsUnique()
            //    .HasDatabaseName("IX_StudentProfile_UserId");



            //// InstructorProfile - Primary Key
            //modelBuilder.Entity<InstructorProfile>()
            //    .HasKey(i => i.InstructorId);

            //// InstructorProfile - Unique Constraint on UserId - Unique constraint: no two InstructorProfile should have the same UserId
            //modelBuilder.Entity<InstructorProfile>()
            //    .HasIndex(i => i.UserId)
            //    .IsUnique()
            //    .HasDatabaseName("IX_InstructorProfile_UserId");



            //// Track - Primary Key
            //modelBuilder.Entity<Track>()
            //    .HasBaseType<LearningEntity>()
            //    .HasKey(t => t.Id);

            //modelBuilder.Entity<Track>()
            //    .Property(t => t.Id)
            //    .HasColumnName("track_id");



            //// Course - Primary Key
            //modelBuilder.Entity<Course>()
            //    .HasBaseType<LearningEntity>()
            //    .HasKey(c => c.Id);

            //modelBuilder.Entity<Course>()
            //    .Property(c => c.Id)
            //    .HasColumnName("course_id");



            //// Module - Primary Key
            //modelBuilder.Entity<Module>()
            //    .HasKey(m => m.ModuleId);

            //// Module - Unique Constraint on (CourseId, Order) - Unique constraint: Order must be unique within a Course
            //modelBuilder.Entity<Module>()
            //    .HasIndex(m => new { m.CourseId, m.Order })
            //    .IsUnique()
            //    .HasDatabaseName("IX_Module_Course_Order");



            //// Lesson - Primary Key
            //modelBuilder.Entity<Lesson>()
            //    .HasKey(l => l.LessonId);

            //// Lesson - Unique Constraint on (ModuleId, Order) - Unique constraint: Order must be unique within a Module
            //modelBuilder.Entity<Lesson>()
            //    .HasIndex(l => new { l.ModuleId, l.Order })
            //    .IsUnique()
            //    .HasDatabaseName("IX_Lesson_Course_Module_Order");



            //// LessonContent - Primary Key
            //modelBuilder.Entity<LessonContent>()
            //    .HasKey(lc => lc.LessonContentId);



            //// LessonProgress - Composite Primary Key
            //modelBuilder.Entity<LessonProgress>()
            //    .HasKey(lp => lp.LessonProgressId);

            //// LessonProgress - Unique Contraint on (StudentId, LessonId) - Unique constraint: a LessonProgress can't repeat for the same combination of (StudentId, LessonId)
            //modelBuilder.Entity<LessonProgress>()
            //    .HasIndex(lp => new { lp.StudentId, lp.LessonId })
            //    .IsUnique()
            //    .HasDatabaseName("IX_LessonProgress_Student_Lesson");



            //// CourseEnrollment - Primary Key
            //modelBuilder.Entity<CourseEnrollment>()
            //    .HasBaseType<EnrollmentBase>()
            //    .HasKey(ce => ce.EnrollmentId);

            //// CourseEnrollment - Unique Contraint on (CourseId, StudentId) - Unique constraint: StudentId must not repeat for the same Course
            //modelBuilder.Entity<CourseEnrollment>()
            //    .HasIndex(ce => new { ce.CourseId, ce.StudentId })
            //    .IsUnique()
            //    .HasDatabaseName("IX_CourseEnrollment_Course_Student");



            //// TrackEnrollment - Primary Key
            //modelBuilder.Entity<TrackEnrollment>()
            //    .HasBaseType<EnrollmentBase>()
            //    .HasKey(te => te.EnrollmentId);

            //// TrackEnrollment - Unique Contraint on (TrackId, StudentId) - Unique constraint: StudentId must not repeat for the same Track
            //modelBuilder.Entity<TrackEnrollment>()
            //    .HasIndex(te => new { te.TrackId, te.StudentId })
            //    .IsUnique()
            //    .HasDatabaseName("IX_TrackEnrollment_Track_Student");



            //// TrackCoure - Composite Primary Key
            //modelBuilder.Entity<TrackCourse>()
            //    .HasKey(tc => new { tc.TrackId, tc.CourseId });



            //// CourseCertificate - Primary Key
            //modelBuilder.Entity<CourseCertificate>()
            //    .HasBaseType<CertificateBase>()
            //    .HasKey(cc => cc.CertificateId);



            //// TarckCertificate - Primary Key
            //modelBuilder.Entity<TrackCertificate>()
            //    .HasBaseType<CertificateBase>()
            //    .HasKey(tc => tc.CertificateId);



            //// Assignment - Primary Key
            //modelBuilder.Entity<Assignment>()
            //    .HasKey(a => a.AssignmentId);

            #endregion

            // ==================== ONE-TO-ONE RELATIONSHIPS CONFIGURATIONS ====================

            #region One-to-One Relationships Configurations

            //// User - StudentProfile (One-to-One)
            //modelBuilder.Entity<User>()
            //    .HasOne(u => u.StudentProfile)
            //    .WithOne(sp => sp.User)
            //    .HasForeignKey<StudentProfile>(sp => sp.UserId);



            //// User - InstructorProfile (One-to-One)
            //modelBuilder.Entity<User>()
            //    .HasOne(u => u.InstructorProfile)
            //    .WithOne(ip => ip.User)
            //    .HasForeignKey<InstructorProfile>(ip => ip.UserId);



            //// Lesson - LesssonContent (One-to-One -- Identyfying relationship)
            //modelBuilder.Entity<LessonContent>()
            //    .HasOne(lc => lc.Lesson)
            //    .WithOne(l => l.Content)
            //    .HasForeignKey<LessonContent>(lc => lc.LessonId)
            //    .OnDelete(DeleteBehavior.Cascade)   // Cascade delete when Lesson is deleted
            //    .IsRequired();                      // Force total participation (LessonContent can't exist without an associated Lesson)

            #endregion


            // ==================== ONE-TO-MANY RELATIONSHIPS CONFIGURATIONS ====================

            #region One-to-Many Relationships Configurations

            //// Course - InstructorProfile (One-to-Many)
            //modelBuilder.Entity<Course>()
            //    .HasOne(c => c.Instructor)
            //    .WithMany(i => i.OwnedCourses)
            //    .HasForeignKey(c => c.InstructorId)
            //    .OnDelete(DeleteBehavior.Restrict);     // prevent deleting an instructor if he owns courses



            //// EnrollmentBase - StudentProfile (One-to-Many)
            //modelBuilder.Entity<EnrollmentBase>()
            //    .ToTable(nameof(EnrollmentBase))
            //    .HasOne(e => e.Student)
            //    .WithMany(sp => sp.Enrollments)
            //    .HasForeignKey(e => e.StudentId)
            //    .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of StudentProfile => he is enrolled in at least a course or a track



            //// CourseEnrollment -Course (One-to-Many)
            //modelBuilder.Entity<CourseEnrollment>()
            //    .HasBaseType<EnrollmentBase>()
            //    .HasOne(ce => ce.Course)
            //    .WithMany(c => c.Enrollments)
            //    .HasForeignKey(ce => ce.CourseId)
            //    .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of Course => Course have at least a student enrolled in



            //// TrackEnrollment - Track (One-to-Many)
            //modelBuilder.Entity<TrackEnrollment>()
            //    .HasBaseType<EnrollmentBase>()
            //    .HasOne(te => te.Track)
            //    .WithMany(t => t.Enrollments)
            //    .HasForeignKey(te => te.TrackId)
            //    .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of Track => Track have at least a student enrolled in



            //// TrackCourse - Track (One-to-Many)
            //modelBuilder.Entity<TrackCourse>()
            //    .HasOne(tc => tc.Track)
            //    .WithMany(t => t.TrackCourses)
            //    .HasForeignKey(tc => tc.TrackId)
            //    .OnDelete(DeleteBehavior.NoAction);         // when a track is deleted => trackId just becomes null (trackId: null, courseId: 2) => meaining that Course(id = 2) is not in a specific track

            //// TrackCourse - Course (One-to-Many)
            //modelBuilder.Entity<TrackCourse>()
            //    .HasOne(tc => tc.Course)
            //    .WithMany(c => c.CourseTracks)
            //    .HasForeignKey(tc => tc.CourseId)
            //    .OnDelete(DeleteBehavior.Cascade);          // when a course is deleted => entry in TrackCourse table associating a course to a specific track gets deleted => there is no point in saying (trackId: 1, courseId: null) => doesn't make any sense



            //// CertificateBase - StudentProfile (One-to-Many)
            //modelBuilder.Entity<CertificateBase>()
            //    .HasOne(c => c.Student)
            //    .WithMany(sp => sp.Certificates)
            //    .HasForeignKey(c => c.StudentId);



            //// TrackCertificate - Track (One-to-Many)
            //modelBuilder.Entity<TrackCertificate>()
            //    .HasBaseType<CertificateBase>()
            //    .HasOne(tc => tc.Track)
            //    .WithMany(t => t.Certificates)
            //    .HasForeignKey(tc => tc.TrackId);



            //// CourseCertificate - Course (One-to-Many)
            //modelBuilder.Entity<CourseCertificate>()
            //    .HasOne(cc => cc.Course)
            //    .WithMany(c => c.Certificates)
            //    .HasForeignKey(cc => cc.CourseId);



            //// Module - Course (One-to-Many - WEAK Entity)
            //modelBuilder.Entity<Module>()
            //    .HasOne(m => m.Course)
            //    .WithMany(c => c.Modules)
            //    .HasForeignKey(m => m.CourseId)
            //    .IsRequired()
            //    .OnDelete(DeleteBehavior.Cascade);

            //// Module - Lesson (One-to-Many - WEAK Entity)
            //// TODO: deleting a module shouldn't enforce deleting its lessons (unless stated explicitly)
            //modelBuilder.Entity<Lesson>()
            //    .HasOne(l => l.Module)
            //    .WithMany(m => m.Lessons)
            //    .HasForeignKey(l => l.ModuleId)
            //    .IsRequired()
            //    .OnDelete(DeleteBehavior.Cascade);



            //// Assignments - Module (One-to-Many - WEAK Entity)
            //modelBuilder.Entity<Assignment>()
            //    .HasOne(a => a.Module)
            //    .WithMany(m => m.Assignments)
            //    .HasForeignKey(a => a.ModuleId)
            //    .IsRequired()
            //    .OnDelete(DeleteBehavior.Cascade);


            //// LessonProgress - Lesson (One-to-Many - WEAK Entity)
            //modelBuilder.Entity<LessonProgress>()
            //    .HasOne(lp => lp.Lesson)
            //    .WithMany(l => l.LessonProgresses)
            //    .HasForeignKey(lp => lp.LessonId)
            //    .IsRequired()
            //    .OnDelete(DeleteBehavior.Cascade);

            //// LessonProgress - StudentProfile (One-to-Many - WEAK Entity)
            //modelBuilder.Entity<LessonProgress>()
            //    .HasOne(lp => lp.Student)
            //    .WithMany()
            //    .HasForeignKey(lp => lp.StudentId)
            //    .IsRequired()
            //    .OnDelete(DeleteBehavior.Cascade);

            #endregion


            // ==================== ENUM CONVERSIONS ====================

            #region Enums

            // Store enums as strings in database =====================================>
            //modelBuilder.Entity<User>()
            //    .Property(u => u.Role)
            //    .HasConversion<string>();

            modelBuilder.Entity<CourseEnrollment>()
                .Property(uc => uc.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Lesson>()
                .Property(l => l.ContentType)
                .HasConversion<string>();

            modelBuilder.Entity<Course>()
                .Property(c => c.Level)
                .HasConversion<string>();

            #endregion


            // ==================== SEED DATA (Optional) ====================

            #region Seed Data
            /*
            // You can add seed data here if needed
            // Example:
            
            modelBuilder.Entity<TrackCertificate>().HasData(
                new TrackCertificate { CertificateId = 1, Title = "Course Completion Certificate" },
                new TrackCertificate { CertificateId = 2, Title = "Excellence Certificate" }
            );
            */
            #endregion

        }
    }
}
