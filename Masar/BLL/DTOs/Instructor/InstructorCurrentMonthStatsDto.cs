using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Instructor;

public class InstructorCurrentMonthStatsDto
{
    public int NewStudents { get; set; }
    public int Completions { get; set; }
    public int NewReviews { get; set; }
}
