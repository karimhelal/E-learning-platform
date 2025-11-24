using BLL.DTOs.Course.Lesson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Course.Module;

public class ModuleDetailsDto
{
    public int ModuleId { get; set; }
    public int ModuleOrder { get; set; }
    public string ModuleName { get; set; }
    public IEnumerable<LessonDetailsDto> LessonsDetails { get; set; }


    // calculated fields
    public int TotalDurationInSeconds { get; set; }


    // Calculated properties (Computed on demand, extremely fast)
    public int Hours => TotalDurationInSeconds / 3600;
    public int Minutes => (TotalDurationInSeconds % 3600) / 60;


    // reference to the owning entity
    public int CourseId { get; set; }
}
