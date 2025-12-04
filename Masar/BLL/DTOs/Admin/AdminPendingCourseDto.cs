using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Admin
{
    public class AdminPendingCourseDto : AdminCourseDto
    {
        public int ModulesCount { get; set; }
        public int LessonsCount { get; set; }
        public string Level { get; set; }
        public int InstructorCoursesCount { get; set; }
        public string? ThumbnailImageUrl { get; set; }
    }
}
