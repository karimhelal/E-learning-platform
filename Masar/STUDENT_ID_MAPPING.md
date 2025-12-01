# Student IDs and Mapping Reference

## Valid Student IDs (from Seeded Data)

Based on the SQL seed script, the following student IDs are valid (1-7):

| Student ID | User ID | First Name | Last Name | Email | Status |
|-----------|---------|-----------|----------|-------|--------|
| 1 | 4 | Alice | Johnson | alice.student@example.com | Active |
| 2 | 5 | Bob | Smith | bob.student@example.com | Active |
| 3 | 6 | Charlie | Brown | charlie.student@example.com | Active |
| 4 | 7 | Diana | Prince | diana.student@example.com | Active |
| 5 | 8 | Emma | Watson | emma.student@example.com | Active |
| 6 | 9 | Frank | Miller | frank.student@example.com | Active |
| 7 | 10 | Grace | Lee | grace.student@example.com | Active |

## ID Mapping in Application

### StudentController (Web\Controllers\Student\StudentController.cs)
- **Current userId**: 1 (Alice Johnson)
- **Valid range**: 1-7
- **Usage**: All student endpoints use this ID
- **TODO**: Replace with actual authenticated user ID from claims

### StudentBrowseTrackService (Web\Services\StudentBrowseTrackService.cs)
- **Parameter**: `studentId` (StudentProfile ID)
- **Valid range**: 1-7
- **Mapping**: Direct mapping from StudentProfile IDs

### Database Relationships

```
Users (user_id: 1-10)
??? Instructors (user_id: 1-3)
?   ??? InstructorProfiles (instructor_id: 1-3, user_id: 1-3)
??? Students (user_id: 4-10)
    ??? StudentProfiles (student_id: 1-7, user_id: 4-10)
```

## Seeded Data Examples

### Enrollments
The following enrollments are available:
- **Alice (Student 1)**: 
  - Course 1 (Web Dev) - 75.5% progress
  - Course 2 (React) - 30% progress
  - Course 5 (iOS) - 50% progress
  - Track 7 (Full Stack) - 55% progress

- **Bob (Student 2)**:
  - Course 1 (Web Dev) - 45.25% progress
  - Course 3 (Python) - 80% progress
  - Course 4 (ML) - 25% progress
  - Track 8 (Data Science) - 70.5% progress

- **Diana (Student 4)**:
  - Course 1 (Web Dev) - 100% COMPLETED
  - Course 5 (iOS) - 22.5% progress

- **Grace (Student 7)**:
  - Course 3 (Python) - 100% COMPLETED
  - Course 4 (ML) - 10.5% progress

## Testing Guide

### To test Browse Tracks with Student 1 (Alice):
```
GET /student/browse-tracks
```
Will show:
- Student Name: Alice
- User Initials: AJ
- Available Tracks for enrollment

### To test with different students:
Modify `userId` in StudentController:
- `userId = 1` for Alice
- `userId = 2` for Bob
- `userId = 3` for Charlie
- etc.

## Next Steps

1. **Implement Track Repository**: Create ITrackRepository to fetch actual tracks
2. **Update StudentBrowseTrackService**: Use track repository to populate tracks
3. **Implement Authentication**: Replace hard-coded userId with actual user claims
4. **Add Track Filtering**: Implement category and difficulty filtering
5. **Seed More Data**: Add more tracks and courses as needed

## SQL to Check Current Data

```sql
-- View all students
SELECT student_id, user_id, bio FROM StudentProfiles;

-- View student enrollments
SELECT 
    sp.student_id,
    u.first_name,
    COUNT(*) as enrollment_count
FROM StudentProfiles sp
JOIN Users u ON sp.user_id = u.user_id
JOIN Enrollments e ON sp.student_id = e.student_id
GROUP BY sp.student_id, u.first_name;

-- View specific student's enrollments
SELECT 
    e.enrollment_id,
    c.title as course_title,
    t.title as track_title,
    e.progress_percentage,
    e.status
FROM Enrollments e
LEFT JOIN Courses c ON e.course_id = c.id
LEFT JOIN Tracks t ON e.track_id = t.id
WHERE e.student_id = 1;
```
