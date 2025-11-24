using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Instructor;

public class InstructorDashboardDto
{
    public int InstructorId { get; set; }
    public string InstructorName { get; set; }

    // Stats
    public InstructorGeneralStatsDto GeneralStats { get; set; }
    public InstructorCurrentMonthStatsDto CurrentMonthStats { get; set; }
    public IEnumerable<InstructorTopPerformingCourseDto> TopPerformingCourses { get; set; }
    public IEnumerable<CourseCardDto> CourseCards { get; set; }
}
