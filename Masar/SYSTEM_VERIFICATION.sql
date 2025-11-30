-- =====================================================
-- COMPREHENSIVE SYSTEM VERIFICATION SCRIPT
-- E-Learning Platform - Database: ELearningPlatformDB11
-- =====================================================

USE ELearningPlatformDB11;
GO

PRINT '========================================';
PRINT 'SYSTEM VERIFICATION REPORT';
PRINT '========================================';
PRINT '';

-- =====================================================
-- 1. DATABASE CONNECTIVITY TEST
-- =====================================================
PRINT '1. DATABASE CONNECTION';
PRINT '   Status: ? Connected';
PRINT '   Database: ' + DB_NAME();
PRINT '   Server: ' + @@SERVERNAME;
PRINT '';

-- =====================================================
-- 2. TABLE INTEGRITY CHECK
-- =====================================================
PRINT '2. TABLE INTEGRITY CHECK';

SELECT 
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

PRINT '';
PRINT '   Total Tables: ' + CAST((SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE') AS VARCHAR);
PRINT '';

-- =====================================================
-- 3. USER ACCOUNTS AND ROLES VERIFICATION
-- =====================================================
PRINT '3. USER ACCOUNTS AND ROLES';

SELECT 
    'Total Users' AS Metric,
    COUNT(*) AS Count
FROM Users
UNION ALL
SELECT 'Admin Users', COUNT(*)
FROM Users u
INNER JOIN UserRoles ur ON u.user_id = ur.user_id
INNER JOIN Roles r ON ur.role_id = r.Id
WHERE r.Name = 'Admin'
UNION ALL
SELECT 'Instructor Users', COUNT(*)
FROM Users u
INNER JOIN UserRoles ur ON u.user_id = ur.user_id
INNER JOIN Roles r ON ur.role_id = r.Id
WHERE r.Name = 'Instructor'
UNION ALL
SELECT 'Student Users', COUNT(*)
FROM Users u
INNER JOIN UserRoles ur ON u.user_id = ur.user_id
INNER JOIN Roles r ON ur.role_id = r.Id
WHERE r.Name = 'Student';

PRINT '';

-- =====================================================
-- 4. INSTRUCTOR DATA VERIFICATION
-- =====================================================
PRINT '4. INSTRUCTOR DATA';

SELECT 
    ip.instructor_id AS InstructorID,
    u.first_name + ' ' + u.last_name AS InstructorName,
    u.email AS Email,
    COUNT(DISTINCT c.id) AS TotalCourses,
    COUNT(DISTINCT e.student_id) AS TotalStudents
FROM InstructorProfiles ip
INNER JOIN Users u ON ip.user_id = u.user_id
LEFT JOIN Courses c ON ip.instructor_id = c.instructor_id
LEFT JOIN Enrollments e ON c.id = e.course_id AND e.enrollment_type = 'CourseEnrollment'
GROUP BY ip.instructor_id, u.first_name, u.last_name, u.email
ORDER BY TotalCourses DESC;

PRINT '';

-- =====================================================
-- 5. STUDENT DATA VERIFICATION
-- =====================================================
PRINT '5. STUDENT DATA';

SELECT 
    sp.student_id AS StudentID,
    u.first_name + ' ' + u.last_name AS StudentName,
    u.email AS Email,
    COUNT(DISTINCT e.course_id) AS EnrolledCourses,
    AVG(e.progress_percentage) AS AvgProgress
FROM StudentProfiles sp
INNER JOIN Users u ON sp.user_id = u.user_id
LEFT JOIN Enrollments e ON sp.student_id = e.student_id AND e.enrollment_type = 'CourseEnrollment'
GROUP BY sp.student_id, u.first_name, u.last_name, u.email
ORDER BY EnrolledCourses DESC;

PRINT '';

-- =====================================================
-- 6. COURSE DATA VERIFICATION
-- =====================================================
PRINT '6. COURSE DATA';

SELECT 
    c.id AS CourseID,
    c.title AS CourseTitle,
    u.first_name + ' ' + u.last_name AS InstructorName,
    c.difficulty_level AS Level,
    COUNT(DISTINCT m.module_id) AS Modules,
    COUNT(DISTINCT l.lesson_id) AS Lessons,
    COUNT(DISTINCT e.student_id) AS Students
FROM Courses c
INNER JOIN InstructorProfiles ip ON c.instructor_id = ip.instructor_id
INNER JOIN Users u ON ip.user_id = u.user_id
LEFT JOIN Modules m ON c.id = m.course_id
LEFT JOIN Lessons l ON m.module_id = l.module_id
LEFT JOIN Enrollments e ON c.id = e.course_id AND e.enrollment_type = 'CourseEnrollment'
GROUP BY c.id, c.title, u.first_name, u.last_name, c.difficulty_level
ORDER BY Students DESC;

PRINT '';

-- =====================================================
-- 7. ENROLLMENT STATUS BREAKDOWN
-- =====================================================
PRINT '7. ENROLLMENT STATUS';

SELECT 
    status AS Status,
    COUNT(*) AS Count
FROM Enrollments
WHERE enrollment_type = 'CourseEnrollment'
GROUP BY status;

PRINT '';

-- =====================================================
-- 8. DATA COMPLETENESS CHECK
-- =====================================================
PRINT '8. DATA COMPLETENESS';

SELECT 
    'Users without Profiles' AS Issue,
    COUNT(*) AS Count
FROM Users u
LEFT JOIN StudentProfiles sp ON u.user_id = sp.user_id
LEFT JOIN InstructorProfiles ip ON u.user_id = ip.user_id
WHERE sp.student_id IS NULL AND ip.instructor_id IS NULL

UNION ALL

SELECT 
    'Courses without Modules',
    COUNT(*)
FROM Courses c
LEFT JOIN Modules m ON c.id = m.course_id
WHERE m.module_id IS NULL

UNION ALL

SELECT 
    'Modules without Lessons',
    COUNT(*)
FROM Modules m
LEFT JOIN Lessons l ON m.module_id = l.module_id
WHERE l.lesson_id IS NULL

UNION ALL

SELECT 
    'Lessons without Content',
    COUNT(*)
FROM Lessons l
LEFT JOIN LessonContents lc ON l.lesson_id = lc.lesson_id
WHERE lc.lesson_content_id IS NULL;

PRINT '';

-- =====================================================
-- 9. AUTHENTICATION TEST ACCOUNTS
-- =====================================================
PRINT '9. TEST ACCOUNTS (For Login Testing)';

SELECT 
    u.user_id AS UserID,
    u.email AS Email,
    r.Name AS Role,
    CASE 
        WHEN ip.instructor_id IS NOT NULL THEN 'Instructor Profile: ID ' + CAST(ip.instructor_id AS VARCHAR)
        WHEN sp.student_id IS NOT NULL THEN 'Student Profile: ID ' + CAST(sp.student_id AS VARCHAR)
        ELSE 'No Profile'
    END AS ProfileInfo
FROM Users u
INNER JOIN UserRoles ur ON u.user_id = ur.user_id
INNER JOIN Roles r ON ur.role_id = r.Id
LEFT JOIN InstructorProfiles ip ON u.user_id = ip.user_id
LEFT JOIN StudentProfiles sp ON u.user_id = sp.user_id
ORDER BY r.Name, u.email;

PRINT '';

-- =====================================================
-- 10. CATEGORY AND CLASSIFICATION DATA
-- =====================================================
PRINT '10. CATEGORIES';

SELECT 
    c.category_id AS CategoryID,
    c.category_name AS CategoryName,
    COUNT(DISTINCT lec.learning_entity_id) AS AssociatedCourses
FROM Categories c
LEFT JOIN LearningEntity_Category lec ON c.category_id = lec.category_id
GROUP BY c.category_id, c.category_name
ORDER BY AssociatedCourses DESC;

PRINT '';

-- =====================================================
-- 11. SYSTEM HEALTH INDICATORS
-- =====================================================
PRINT '11. SYSTEM HEALTH INDICATORS';

SELECT 
    'Total Courses' AS Metric,
    COUNT(*) AS Value,
    CASE WHEN COUNT(*) > 0 THEN '? PASS' ELSE '? FAIL' END AS Status
FROM Courses
UNION ALL
SELECT 
    'Total Students',
    COUNT(*),
    CASE WHEN COUNT(*) > 0 THEN '? PASS' ELSE '? FAIL' END
FROM StudentProfiles
UNION ALL
SELECT 
    'Total Instructors',
    COUNT(*),
    CASE WHEN COUNT(*) > 0 THEN '? PASS' ELSE '? FAIL' END
FROM InstructorProfiles
UNION ALL
SELECT 
    'Total Enrollments',
    COUNT(*),
    CASE WHEN COUNT(*) > 0 THEN '? PASS' ELSE '??  WARN' END
FROM Enrollments
UNION ALL
SELECT 
    'Total Modules',
    COUNT(*),
    CASE WHEN COUNT(*) > 0 THEN '? PASS' ELSE '??  WARN' END
FROM Modules
UNION ALL
SELECT 
    'Total Lessons',
    COUNT(*),
    CASE WHEN COUNT(*) > 0 THEN '? PASS' ELSE '??  WARN' END
FROM Lessons
UNION ALL
SELECT 
    'Total Lesson Content',
    COUNT(*),
    CASE WHEN COUNT(*) > 0 THEN '? PASS' ELSE '??  WARN' END
FROM LessonContents;

PRINT '';

-- =====================================================
-- 12. SPECIFIC USER VERIFICATION
-- =====================================================
PRINT '12. SAMPLE USER VERIFICATION';

-- Check a specific instructor
DECLARE @SampleInstructorId INT;
SELECT TOP 1 @SampleInstructorId = instructor_id FROM InstructorProfiles;

IF @SampleInstructorId IS NOT NULL
BEGIN
    PRINT '   Sample Instructor ID: ' + CAST(@SampleInstructorId AS VARCHAR);
    
    SELECT 
        'Instructor Profile' AS CheckType,
        CASE WHEN COUNT(*) > 0 THEN '? EXISTS' ELSE '? MISSING' END AS Status
    FROM InstructorProfiles
    WHERE instructor_id = @SampleInstructorId
    
    UNION ALL
    
    SELECT 
        'Has Courses',
        CASE WHEN COUNT(*) > 0 THEN '? YES (' + CAST(COUNT(*) AS VARCHAR) + ')' ELSE '??  NO' END
    FROM Courses
    WHERE instructor_id = @SampleInstructorId;
END
ELSE
BEGIN
    PRINT '   ??  WARNING: No instructors found in database';
END

PRINT '';

-- Check a specific student
DECLARE @SampleStudentId INT;
SELECT TOP 1 @SampleStudentId = student_id FROM StudentProfiles;

IF @SampleStudentId IS NOT NULL
BEGIN
    PRINT '   Sample Student ID: ' + CAST(@SampleStudentId AS VARCHAR);
    
    SELECT 
        'Student Profile' AS CheckType,
        CASE WHEN COUNT(*) > 0 THEN '? EXISTS' ELSE '? MISSING' END AS Status
    FROM StudentProfiles
    WHERE student_id = @SampleStudentId
    
    UNION ALL
    
    SELECT 
        'Has Enrollments',
        CASE WHEN COUNT(*) > 0 THEN '? YES (' + CAST(COUNT(*) AS VARCHAR) + ')' ELSE '??  NO' END
    FROM Enrollments
    WHERE student_id = @SampleStudentId;
END
ELSE
BEGIN
    PRINT '   ??  WARNING: No students found in database';
END

PRINT '';

-- =====================================================
-- 13. FOREIGN KEY INTEGRITY
-- =====================================================
PRINT '13. FOREIGN KEY INTEGRITY';

-- Check for orphaned courses (instructor doesn't exist)
SELECT 
    'Orphaned Courses' AS Issue,
    COUNT(*) AS Count
FROM Courses c
LEFT JOIN InstructorProfiles ip ON c.instructor_id = ip.instructor_id
WHERE ip.instructor_id IS NULL

UNION ALL

-- Check for orphaned enrollments (student or course doesn't exist)
SELECT 
    'Orphaned Enrollments',
    COUNT(*)
FROM Enrollments e
LEFT JOIN StudentProfiles sp ON e.student_id = sp.student_id
LEFT JOIN Courses c ON e.course_id = c.id
WHERE sp.student_id IS NULL OR (c.id IS NULL AND e.enrollment_type = 'CourseEnrollment')

UNION ALL

-- Check for orphaned modules
SELECT 
    'Orphaned Modules',
    COUNT(*)
FROM Modules m
LEFT JOIN Courses c ON m.course_id = c.id
WHERE c.id IS NULL;

PRINT '';

-- =====================================================
-- FINAL SUMMARY
-- =====================================================
PRINT '========================================';
PRINT 'VERIFICATION COMPLETE';
PRINT '========================================';
PRINT '';
PRINT 'Database: ELearningPlatformDB11';
PRINT 'Date: ' + CAST(GETDATE() AS VARCHAR);
PRINT '';
PRINT 'Legend:';
PRINT '  ? PASS  - Check passed successfully';
PRINT '  ??  WARN  - Warning: May need attention';
PRINT '  ? FAIL  - Critical issue detected';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Review any ? FAIL or ??  WARN items';
PRINT '  2. Ensure test users have correct roles assigned';
PRINT '  3. Verify connection string in appsettings.json';
PRINT '  4. Test login with sample accounts shown above';
PRINT '';
