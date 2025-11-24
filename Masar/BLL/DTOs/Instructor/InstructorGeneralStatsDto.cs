using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Instructor;

public class InstructorGeneralStatsDto
{
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public float AverageRating { get; set; }
    public int CompletionRate { get; set; }
}
