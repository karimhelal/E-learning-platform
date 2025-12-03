using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Admin
{
    public class AdminCourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string InstructorName { get; set; }
        public string Categories { get; set; }
        public int StudentsCount { get; set; }
        public DateOnly CreatedDate { get; set; }
    }
}
