using AutoMapper;
using BLL.Interfaces;
using Core.RepositoryInterfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Interfaces;
using Web.ViewModels.Public;

namespace Web.Controllers;

public class PublicController(
    IPublicInstructorService _publicInstructorService,
    ICurrentUserService _currentUserService,
    IUserRepository _userRepository,
    IMapper _mapper) : Controller
{
    [HttpGet("/instructor/{instructorId:int}")]
    public async Task<IActionResult> InstructorProfile(int instructorId)
    {
        var profileData = await _publicInstructorService.GetInstructorPublicProfileAsync(instructorId);
        if (profileData == null) return NotFound("Instructor not found");

        int? studentId = null;
        string? studentName = null;
        string? userInitials = null;

        var userId = _currentUserService.GetUserId();
        if (userId > 0)
        {
            var studentProfile = await _userRepository.GetStudentProfileForUserAsync(userId, includeUserBase: true);
            if (studentProfile?.User != null)
            {
                studentId = studentProfile.StudentId;
                studentName = studentProfile.User.FirstName;
                userInitials = $"{studentProfile.User.FirstName[0]}{studentProfile.User.LastName[0]}".ToUpper();
            }
        }

        var mappedData = _mapper.Map<PublicInstructorProfileDataViewModel>(profileData);

        var gradients = new[] {
            "linear-gradient(135deg, #667eea 0%, #764ba2 100%)", "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
            "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)", "linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)",
            "linear-gradient(135deg, #fa709a 0%, #fee140 100%)", "linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)"
        };
        for (int i = 0; i < mappedData.Courses.Count; i++)
            mappedData.Courses[i].GradientStyle = gradients[i % gradients.Length];

        var viewModel = new PublicInstructorProfileViewModel
        {
            Data = mappedData,
            PageTitle = $"{profileData.FullName} | Instructor Profile",
            StudentId = studentId,
            StudentName = studentName,
            UserInitials = userInitials
        };

        ViewBag.Title = viewModel.PageTitle;
        return View("InstructorProfile", viewModel);
    }
}