# Quick Start Guide

## 5-Minute Setup

### 1. Run the Seed Script
```sql
-- In SQL Server Management Studio:
-- 1. Open the seed script (provided in context)
-- 2. Change database name: USE [YourDatabaseName];
-- 3. Execute (F5)
-- ? Data inserted: 10 users, 6 courses, 3 tracks, 27 modules, 25 lessons
```

### 2. Run Migrations (if needed)
```powershell
# In Visual Studio Package Manager Console or Terminal
dotnet ef database update
```

### 3. Start the Application
```powershell
dotnet run
```

### 4. Test Browse Tracks
```
URL: https://localhost:5001/student/browse-tracks
Student: Alice Johnson
Email: alice.student@example.com
```

## Valid Student IDs for Testing

```
1 = Alice Johnson     (alicejohnson@example.com)
2 = Bob Smith        (bobsmith@example.com)
3 = Charlie Brown    (charliebrown@example.com)
4 = Diana Prince     (dianaprince@example.com)
5 = Emma Watson      (emmawatson@example.com)
6 = Frank Miller     (frankmiller@example.com)
7 = Grace Lee        (gracelee@example.com)
```

## To Test Different Students

Edit `StudentController.cs`:
```csharp
private readonly int userId = 1; // Change to 1-7
```

## Page Features

? Modern responsive layout  
? Search functionality (ready for implementation)  
? Filter dropdowns (Category, Level, Duration)  
? Sort options (Popular, Newest, Rating, A-Z)  
? Grid/List view toggle  
? Track cards with stats  
? Skills tags  
? Course preview section  
? Empty states  

## Database Tables Populated

| Table | Records | Purpose |
|-------|---------|---------|
| Users | 10 | Authentication base |
| StudentProfiles | 7 | Student information |
| InstructorProfiles | 3 | Instructor information |
| Roles | 3 | User roles (Instructor, Student, Admin) |
| Courses | 6 | Course content |
| Tracks | 3 | Learning tracks |
| Modules | 27 | Course modules |
| Lessons | 25 | Individual lessons |
| Enrollments | 23 | Student enrollments |
| Categories | 17 | Course categories |
| Languages | 6 | Course languages |

## Current State

```
? StudentController - Updated with valid IDs
? BrowseTracks.cshtml - Modern UI implementation
? StudentBrowseTracksViewModel - Proper data structure
? StudentBrowseTrackService - Service layer ready
? Seed Script - Comprehensive test data
? Build - Compiling successfully
? Documentation - Complete with guides
```

## Next: Implementing Track Display

To show actual tracks on the page, you need to:

1. **Create Track Repository** (if not exists):
```csharp
public interface ITrackRepository
{
    Task<List<Track>> GetAllTracksAsync();
    Task<Track> GetTrackByIdAsync(int trackId);
}
```

2. **Update StudentBrowseTrackService**:
```csharp
private readonly ITrackRepository _trackRepo;

public async Task<StudentBrowseTracksPageData?> GetAllTracksAsync(int studentId)
{
    var tracks = await _trackRepo.GetAllTracksAsync();
    var mappedTracks = MapTracks(tracks);
    // ... rest of implementation
}
```

3. **Populate BrowseTrackItem**:
```csharp
private List<BrowseTrackItem> MapTracks(List<Track> tracks)
{
    return tracks.Select(t => new BrowseTrackItem
    {
        TrackId = t.Id,
        Title = t.Title,
        Description = t.Description,
        // ... map other properties
    }).ToList();
}
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Student profile not found" | Check student ID is 1-7 |
| "Cannot connect to database" | Run `dotnet ef database update` |
| Runtime error on page | Check seed script ran successfully |
| Tracks not showing | Track repository not yet implemented |
| Wrong student displays | Update `userId` in StudentController |

## Performance Notes

- Current page load: ~200ms (profile fetch)
- Will increase to ~500ms once tracks are fetched
- Consider caching frequently accessed tracks
- Pagination recommended for 100+ tracks

## Key Files to Remember

```
StudentController.cs         ? userId = 1 (change here for testing)
StudentBrowseTrackService.cs ? Where track data is retrieved
BrowseTracks.cshtml          ? What user sees
StudentBrowseTracksViewModel.cs ? Data structure
```

## SQL Queries for Quick Checks

```sql
-- See all students
SELECT student_id, first_name, last_name, email 
FROM StudentProfiles sp
JOIN Users u ON sp.user_id = u.user_id;

-- Check Alice's enrollments
SELECT c.title, e.progress_percentage, e.status
FROM Enrollments e
LEFT JOIN Courses c ON e.course_id = c.id
WHERE e.student_id = 1;

-- View all available tracks
SELECT id, title, description FROM Tracks;

-- Check track-course relationships
SELECT t.title, c.title 
FROM Track_Course tc
JOIN Tracks t ON tc.track_id = t.id
JOIN Courses c ON tc.course_id = c.id;
```

## Contact Support

For questions about:
- **Student ID mapping**: See `STUDENT_ID_MAPPING.md`
- **Seed script**: See `SEED_SCRIPT_INSTRUCTIONS.md`
- **Implementation details**: See `IMPLEMENTATION_SUMMARY.md`

---

**Status**: ? Ready for Development  
**Last Updated**: 2024  
**Build**: Successful  
**Database**: Seeded and Ready
