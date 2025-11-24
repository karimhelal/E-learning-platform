using Core.Entities;
using Core.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Instructor;

public class CourseCardDto
{
    public int CourseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Category MainCategory { get; set; }
    public CourseLevel Level { get; set; }
    public string Status { get; set; }
    public float Rating { get; set; }

    // Stats
    public int StudentsCount { get; set; }
    public int ModulesCount { get; set; }
    public double DurationHours { get; set; }
}
