using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Instructor;

public class InstructorTopPerformingCourseDto
{
    public string Title { get; set; }
    public int StudentsEnrolled { get; set; }
    public float AverageRating { get; set; }
}
