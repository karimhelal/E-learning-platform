# Database Seed Script Instructions

## Overview

The SQL seed script populates the database with comprehensive test data for the E-Learning Platform. It includes users, courses, tracks, enrollments, and lesson progress data.

## Prerequisites

- SQL Server or Azure SQL Database
- SQL Server Management Studio or Azure Data Studio
- Connection to your target database

## Running the Script

### Step 1: Prepare the Database

Before running the seed script, ensure your database is created with all tables. Run the EF Core migrations first:

```powershell
# In the Web project directory
dotnet ef database update
```

### Step 2: Run the Seed Script

1. Open SQL Server Management Studio or Azure Data Studio
2. Connect to your database
3. Open a new query window
4. **Modify the database name** in the script:
   ```sql
   USE [YourDatabaseName]; -- Replace with your actual database name
   ```
5. Copy and paste the entire seed script
6. Execute (F5 or Execute button)

### Alternative: Run from Command Line

```sql
sqlcmd -S "SERVER_NAME" -d "DATABASE_NAME" -U "USERNAME" -P "PASSWORD" -i "path\to\seed-script.sql"
```

## Data Structure

### Users (10 total)
- **3 Instructors** (IDs 1-3):
  - John Doe - Web Development Expert (10+ years)
  - Sarah Williams - Data Science (8 years)
  - Mike Chen - Mobile Development (6 years)

- **7 Students** (IDs 4-10):
  - Alice Johnson (ID: 4 ? Student ID: 1)
  - Bob Smith (ID: 5 ? Student ID: 2)
  - Charlie Brown (ID: 6 ? Student ID: 3)
  - Diana Prince (ID: 7 ? Student ID: 4)
  - Emma Watson (ID: 8 ? Student ID: 5)
  - Frank Miller (ID: 9 ? Student ID: 6)
  - Grace Lee (ID: 10 ? Student ID: 7)

### Courses (6 total)

| ID | Title | Difficulty | Instructor | Students Enrolled |
|----|-------|-----------|-----------|------------------|
| 1 | Complete Web Development Bootcamp | Beginner | John (1) | 5 |
| 2 | Advanced React and Redux Masterclass | Advanced | John (1) | 3 |
| 3 | Python for Data Science | Beginner | Sarah (2) | 4 |
| 4 | Machine Learning A-Z | Intermediate | Sarah (2) | 3 |
| 5 | iOS App Development with Swift | Intermediate | Mike (3) | 2 |
| 6 | UI/UX Design Fundamentals | Beginner | Mike (3) | 3 |

### Tracks (3 total)

| ID | Title | Description |
|----|-------|-------------|
| 7 | Full Stack Web Developer | Frontend + Backend + Database |
| 8 | Data Science Professional | Python + ML + Analytics |
| 9 | Mobile App Developer | iOS + Android + Tools |

### Categories (17 total)

**Root Categories:**
- Development
- Data Science
- Design
- Business

**Sub-categories:**
- Web Development (Frontend, Backend, Full Stack)
- Mobile Development
- Programming Languages
- Machine Learning
- Data Analysis
- Deep Learning
- UI/UX Design
- Graphic Design
- Web Design

### Enrollments (23 total)

**Course Enrollments:**
- 20 course enrollments with varying progress (10% - 100%)
- Progress range: 10.5% to 100%
- Mix of In-Progress and Completed statuses

**Track Enrollments:**
- 3 track enrollments (30-70% progress)

### Modules (27 total)

Examples:
- Course 1: 6 modules (HTML/CSS, JavaScript, React, Node.js, etc.)
- Course 3: 5 modules (Python Basics, NumPy, Pandas, Visualization, Statistics)
- Course 5: 4 modules (Swift, SwiftUI, iOS Frameworks, App Store)

### Lessons (25 total)

Each module contains 3-6 lessons with:
- Descriptive titles
- Content type (Video/Article)
- Order within module

### Lesson Progress (19 total)

Tracks student completion:
- **Completed lessons**: Have start and end dates
- **In-progress lessons**: Have start date but no end date
- Mix of students at different progress levels

## Verification

After running the script, verify the data:

```sql
-- Check all data was inserted
SELECT 
    (SELECT COUNT(*) FROM Users) as Users,
    (SELECT COUNT(*) FROM StudentProfiles) as Students,
    (SELECT COUNT(*) FROM InstructorProfiles) as Instructors,
    (SELECT COUNT(*) FROM Courses) as Courses,
    (SELECT COUNT(*) FROM Tracks) as Tracks,
    (SELECT COUNT(*) FROM Modules) as Modules,
    (SELECT COUNT(*) FROM Lessons) as Lessons,
    (SELECT COUNT(*) FROM Enrollments) as Enrollments;

-- Check specific student
SELECT * FROM StudentProfiles WHERE student_id = 1;
SELECT * FROM Enrollments WHERE student_id = 1;

-- Check course data
SELECT * FROM Courses;
SELECT * FROM Track_Course;
```

## Troubleshooting

### Error: "Violation of PRIMARY KEY constraint"

**Cause**: Data already exists in the table
**Solution**: 
1. Run the DELETE statements separately first
2. Or drop and recreate the database

### Error: "Violation of FOREIGN KEY constraint"

**Cause**: Related data isn't inserted in correct order
**Solution**: 
1. Ensure script runs completely without interruption
2. Check that all tables exist (run migrations first)

### Error: "Cannot find database"

**Cause**: Database doesn't exist
**Solution**: 
1. Create the database first in SSMS
2. Run migrations: `dotnet ef database update`

## Resetting the Data

To clear and re-seed:

```sql
-- Run just the cleanup section
DELETE FROM [dbo].[LessonProgresses];
DELETE FROM [dbo].[LessonResources];
-- ... (all other delete statements)
```

Then run the full script again.

## Customization

To modify the seed data:

1. **Edit enrollment progress**: Modify progress_percentage values
2. **Add more students**: Duplicate INSERT statements with new IDs
3. **Add new courses**: Insert into Courses and link to categories/languages
4. **Adjust skill levels**: Modify difficulty_level values

## Testing the Browse Tracks Page

After seeding:

1. Start the application
2. Navigate to `/student/browse-tracks`
3. Default student (Alice - ID 1) will display
4. You should see the available tracks and enrollments

## Performance Notes

- Full seed completes in < 1 second (minimal dataset)
- Can be extended with 100x more data if needed
- Identity seeds are reset, so you can re-run multiple times
