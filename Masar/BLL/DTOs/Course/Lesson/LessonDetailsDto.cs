using Core.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Course.Lesson;

public class LessonDetailsDto
{
    public int LessonId { get; set; }
    public int LessonOrder { get; set; }
    public string LessonName { get; set; }
    public int DurationInSeconds { get; set; }
    public LessonContentType LessonContentType { get; set; }



    // reference to the owning entity
    public int ModuleId { get; set; }
}
