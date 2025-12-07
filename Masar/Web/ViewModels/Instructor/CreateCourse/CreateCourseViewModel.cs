using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Instructor.CreateCourse;

public class CreateCourseViewModel
{
    [Display(Name = "Course Title")]
    [Required(ErrorMessage = "{0} is required and can't be null")]
    [MinLength(10, ErrorMessage = "{0} should be at least {1} chars")]
    [MaxLength(40, ErrorMessage = "{0} shouldn't be more than {1} chars")]
    public string CourseTitle { get; set; }


    [Required(ErrorMessage = "{0} should be supplied")]
    [Display(Name = "Main Category")]
    public int? MainCategoryId { get; set; }

    [BindNever]
    [ValidateNever]
    public IEnumerable<SelectListItem> CategoryOptions { get; set; }
}
