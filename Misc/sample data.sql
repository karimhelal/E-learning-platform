-- Disable constraints for a fast, safe bulk insert
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
GO

-- 1. Users (80 total: 50 Students, 30 Instructors)
PRINT 'Inserting Users...'
BEGIN
    DECLARE @i INT = 1
    WHILE @i <= 50
    BEGIN
        INSERT INTO [Users] (first_name, last_name, email, password_hash, role)
        VALUES ('Student', CONCAT('User', @i), CONCAT('student', @i, '@example.com'), 'hash-placeholder', 'Student')
        SET @i = @i + 1
    END

    SET @i = 1
    WHILE @i <= 30
    BEGIN
        INSERT INTO [Users] (first_name, last_name, email, password_hash, role)
        VALUES ('Instructor', CONCAT('User', @i), CONCAT('instructor', @i, '@example.com'), 'hash-placeholder', 'Instructor')
        SET @i = @i + 1
    END
END
GO

-- 2. StudentProfiles (50 total)
-- (Linked to User IDs 1-50)
PRINT 'Inserting StudentProfiles...'
BEGIN
    DECLARE @i INT = 1
    WHILE @i <= 50
    BEGIN
        INSERT INTO [StudentProfiles] (user_id, bio)
        VALUES (@i, CONCAT('Bio for student ', @i))
        SET @i = @i + 1
    END
END
GO
-- StudentProfile PKs are 1-50

-- 3. InstructorProfiles (30 total)
-- (Linked to User IDs 51-80)
PRINT 'Inserting InstructorProfiles...'
BEGIN
    DECLARE @i INT = 1
    DECLARE @userId INT = 51
    WHILE @i <= 30
    BEGIN
        INSERT INTO [InstructorProfiles] (user_id, bio, years_of_experience)
        VALUES (@userId, CONCAT('Bio for instructor ', @i), (@i % 10) + 1)
        SET @i = @i + 1
        SET @userId = @userId + 1
    END
END
GO
-- InstructorProfile PKs are 1-30

-- 4. Categories (20 total, with parent/child)
PRINT 'Inserting Categories...'
INSERT INTO [Categories] (category_name, category_slug, parent_category_id)
VALUES
('Programming', 'programming', NULL),
('Data Science', 'data-science', NULL),
('Databases', 'databases', NULL),
('Cloud Computing', 'cloud', NULL),
('Design', 'design', NULL),
('Web Development', 'web-dev', 1),
('Mobile Development', 'mobile-dev', 1),
('Machine Learning', 'ml', 2),
('Python', 'python', 1),
('JavaScript', 'javascript', 6),
('React', 'react', 10),
('Node.js', 'nodejs', 10),
('SQL', 'sql', 3),
('NoSQL', 'nosql', 3),
('AWS', 'aws', 4),
('Azure', 'azure', 4),
('DevOps', 'devops', 4),
('UI/UX', 'ui-ux', 5),
('Project Management', 'pm', NULL),
('Agile', 'agile', 19);
GO
-- Category PKs are 1-20

-- 5. LearningEntities (65 total: 60 Courses, 5 Tracks)
PRINT 'Inserting LearningEntities...'
BEGIN
    DECLARE @i INT = 1
    WHILE @i <= 65
    BEGIN
        INSERT INTO [LearningEntities] (created_date) VALUES (GETDATE())
        SET @i = @i + 1
    END
END
GO
-- LearningEntity PKs are 1-65

-- 6. Courses (60 total, TPT)
-- (Linked to LearningEntity IDs 1-60)
-- (Linked to InstructorProfile IDs 1-30, repeating)
PRINT 'Inserting Courses...'
BEGIN
    DECLARE @i INT = 1
    DECLARE @instructorId INT = 1
    WHILE @i <= 60
    BEGIN
        INSERT INTO [Courses] (id, instructor_id, title, description, language, difficulty_level)
        VALUES (@i, @instructorId, CONCAT('Course Title ', @i), CONCAT('Description for course ', @i), 'English', 'Beginner')
        
        SET @i = @i + 1
        SET @instructorId = @instructorId + 1
        IF @instructorId > 30
            SET @instructorId = 1
    END
END
GO

-- 7. Tracks (5 total, TPT)
-- (Linked to LearningEntity IDs 61-65)
PRINT 'Inserting Tracks...'
INSERT INTO [Tracks] (id, title, description)
VALUES
(61, 'Full Stack Developer Track', 'Become a full stack master.'),
(62, 'Data Scientist Track', 'Learn Python, ML, and SQL.'),
(63, 'Cloud Engineer Track', 'Master AWS and Azure.'),
(64, 'Mobile Developer Track', 'Build for iOS and Android.'),
(65, 'UI/UX Designer Track', 'Go from wireframe to prototype.');
GO

-- 8. Modules (120 total: 2 per Course)
PRINT 'Inserting Modules...'
BEGIN
    DECLARE @courseId INT = 1
    DECLARE @order INT
    WHILE @courseId <= 60
    BEGIN
        SET @order = 1
        WHILE @order <= 2
        BEGIN
            INSERT INTO [Modules] (course_id, title, description, [order])
            VALUES (@courseId, CONCAT('Module ', @order, ' for Course ', @courseId), 'Module description', @order)
            SET @order = @order + 1
        END
        SET @courseId = @courseId + 1
    END
END
GO
-- Module PKs are 1-120

-- 9. Lessons (360 total: 3 per Module)
PRINT 'Inserting Lessons...'
BEGIN
    DECLARE @moduleId INT = 1
    DECLARE @order INT
    WHILE @moduleId <= 120
    BEGIN
        SET @order = 1
        WHILE @order <= 3
        BEGIN
            -- This sets the 'content_type' from your Lesson enum
            DECLARE @contentType VARCHAR(20) = CASE WHEN @order = 3 THEN 'Article' ELSE 'Video' END
            
            INSERT INTO [Lessons] (module_id, title, content_type, [order])
            VALUES (@moduleId, CONCAT('Lesson ', @order, ' for Module ', @moduleId), @contentType, @order)
            SET @order = @order + 1
        END
        SET @moduleId = @moduleId + 1
    END
END
GO
-- Lesson PKs are 1-360

-- 10. LessonContents (360 total: 1 per Lesson - TPH)
-- (2 Video, 1 Article per module)
PRINT 'Inserting LessonContents (TPH)...'
BEGIN
    DECLARE @lessonId INT = 1
    WHILE @lessonId <= 360
    BEGIN
        -- Lessons 1 and 2 (from the loop above) are Video
        IF @lessonId % 3 = 1 OR @lessonId % 3 = 2
        BEGIN
            INSERT INTO [LessonContents] (lesson_id, content, content_type, pdf_url, duration_seconds)
            VALUES (@lessonId, 'Video content goes here...', 'Video', NULL, (@lessonId % 50) * 10 + 300) -- duration 300-790s
        END
        -- Lesson 3 (from the loop above) is Article
        ELSE
        BEGIN
            INSERT INTO [LessonContents] (lesson_id, content, content_type, pdf_url, duration_seconds)
            VALUES (@lessonId, 'Article content goes here...', 'Article', CONCAT('http://example.com/pdf/', @lessonId, '.pdf'), NULL)
        END
        SET @lessonId = @lessonId + 1
    END
END
GO

-- 11. LessonResources (360 total: 1 per Lesson - TPH)
PRINT 'Inserting LessonResources (TPH)...'
BEGIN
    DECLARE @lessonId INT = 1
    WHILE @lessonId <= 360
    BEGIN
        IF @lessonId % 3 = 1
        BEGIN
            INSERT INTO [LessonResources] (lesson_id, resource_type, pdf_url, link_url, zip_url)
            VALUES (@lessonId, 'PDF', CONCAT('http://example.com/res', @lessonId, '.pdf'), NULL, NULL)
        END
        ELSE IF @lessonId % 3 = 2
        BEGIN
            INSERT INTO [LessonResources] (lesson_id, resource_type, pdf_url, link_url, zip_url)
            VALUES (@lessonId, 'Zip', NULL, NULL, CONCAT('http://example.com/res', @lessonId, '.zip'))
        END
        ELSE
        BEGIN
            INSERT INTO [LessonResources] (lesson_id, resource_type, pdf_url, link_url, zip_url)
            VALUES (@lessonId, 'Url', NULL, CONCAT('http://example.com/link/', @lessonId), NULL)
        END
        SET @lessonId = @lessonId + 1
    END
END
GO

-- 12. Assignments (120 total: 1 per Module)
PRINT 'Inserting Assignments...'
BEGIN
    DECLARE @moduleId INT = 1
    WHILE @moduleId <= 120
    BEGIN
        INSERT INTO [Assignments] (module_id, title, instruction)
        VALUES (@moduleId, CONCAT('Assignment for Module ', @moduleId), 'Complete the exercise.')
        SET @moduleId = @moduleId + 1
    END
END
GO

-- 13. Track_Course (40 total: 8 Courses per Track)
PRINT 'Inserting Track_Course...'
BEGIN
    DECLARE @trackId INT = 61
    DECLARE @courseId INT = 1
    WHILE @trackId <= 65
    BEGIN
        DECLARE @i INT = 1
        WHILE @i <= 8
        BEGIN
            INSERT INTO [Track_Course] (track_id, course_id)
            VALUES (@trackId, @courseId)
            
            SET @i = @i + 1
            SET @courseId = @courseId + 1
        END
        SET @trackId = @trackId + 1
    END
END
GO

-- 14. LearningEntity_Category (70 total)
PRINT 'Inserting LearningEntity_Category...'
BEGIN
    DECLARE @leId INT = 1
    WHILE @leId <= 65
    BEGIN
        INSERT INTO [LearningEntity_Category] (learning_entity_id, category_id)
        VALUES (@leId, (@leId % 20) + 1) -- Link each LE to one category
        SET @leId = @leId + 1
    END
    -- Add some second categories
    SET @leId = 1
    WHILE @leId <= 5
    BEGIN
        INSERT INTO [LearningEntity_Category] (learning_entity_id, category_id)
        VALUES (@leId, (@leId % 5) + 2)
        SET @leId = @leId + 1
    END
END
GO

-- 15. Enrollments (100 total: 1 Course and 1 Track per Student - TPH)
PRINT 'Inserting Enrollments (TPH)...'
BEGIN
    DECLARE @studentId INT = 1
    WHILE @studentId <= 50
    BEGIN
        -- Enroll in a Course (Student 1 in Course 1, Student 2 in Course 2, etc.)
        INSERT INTO [Enrollments] (student_id, course_id, track_id, enrollment_date, status, progress_percentage, enrollment_type)
        VALUES (@studentId, @studentId, NULL, GETDATE(), 'InProgress', (@studentId % 10) * 10.5, 'CourseEnrollment')
        
        -- Enroll in a Track (Student 1 in Track 61, Student 2 in Track 62, etc.)
        INSERT INTO [Enrollments] (student_id, course_id, track_id, enrollment_date, status, progress_percentage, enrollment_type)
        VALUES (@studentId, NULL, (@studentId % 5) + 61, GETDATE(), 'InProgress', (@studentId % 10) * 5.5, 'TrackEnrollment')

        SET @studentId = @studentId + 1
    END
END
GO

-- 16. Certificates (50 total: 25 Course, 25 Track - TPH)
PRINT 'Inserting Certificates (TPH)...'
BEGIN
    DECLARE @studentId INT = 1
    WHILE @studentId <= 50
    BEGIN
        IF @studentId % 2 = 1
        BEGIN
            -- Course Certificate
            INSERT INTO [Certificates] (student_id, title, issued_date, link, cetificate_type, course_id, track_id)
            VALUES (@studentId, CONCAT('Certificate for Course ', @studentId), GETDATE(), 'http://example.com/cert/c', 'CourseCertificate', @studentId, NULL)
        END
        ELSE
        BEGIN
            -- Track Certificate
            INSERT INTO [Certificates] (student_id, title, issued_date, link, cetificate_type, course_id, track_id)
            VALUES (@studentId, CONCAT('Certificate for Track ', (@studentId % 5) + 61), GETDATE(), 'http://example.com/cert/t', 'TrackCertificate', NULL, (@studentId % 5) + 61)
        END
        SET @studentId = @studentId + 1
    END
END
GO

-- 17. LessonProgresses (250 total: 5 per Student)
PRINT 'Inserting LessonProgresses...'
BEGIN
    DECLARE @studentId INT = 1
    DECLARE @lessonId INT = 1
    WHILE @studentId <= 50
    BEGIN
        DECLARE @i INT = 1
        WHILE @i <= 5
        BEGIN
            -- Ensure (student_id, lesson_id) is unique
            DECLARE @lId INT = (@lessonId + @studentId + @i)
            IF @lId > 360 SET @lId = (@lId % 360) + 1

            INSERT INTO [LessonProgresses] (student_id, lesson_id, started_date, completed_date, is_completed)
            VALUES (@studentId, @lId, GETDATE(), CASE WHEN @i = 1 THEN GETDATE() ELSE NULL END, CASE WHEN @i = 1 THEN 1 ELSE 0 END)
            
            SET @i = @i + 1
            SET @lessonId = @lessonId + 1
        END
        SET @studentId = @studentId + 1
    END
END
GO

-- Re-enable constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'
GO

PRINT 'Data generation complete.'