namespace Core.Entities.Enums;

public enum LearningEntityStatus
{
    Draft = 0,      // Default: Hidden, editable
    Pending = 1,    // Submitted for review
    Published = 2,  // Live, visible to students
    Archived = 3    // Hidden, read-only
}