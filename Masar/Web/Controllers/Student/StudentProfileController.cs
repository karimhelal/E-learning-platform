using AutoMapper;
using BLL.DTOs.Student;
using BLL.Interfaces.Student;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.AspNetCore.Mvc;
using Web.Interfaces;
using Web.ViewModels.Student;

namespace Web.Controllers.Student;

public class StudentProfileController(
    IStudentProfileService studentProfileService,
    IStudentCertificatesService certificatesService,
    IWebHostEnvironment webHostEnvironment,
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    AppDbContext context,
    ILogger<StudentProfileController> logger,
    IMapper mapper)
    : StudentBaseController(currentUserService, userRepository, context, logger)
{
    private readonly AppDbContext _context = context;
    private readonly IWebHostEnvironment _env = webHostEnvironment;

    [HttpGet("/student/profile")]
    public async Task<IActionResult> Profile()
    {
        ViewBag.Title = "My Profile | Masar";

        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Unauthorized();

        var profileData = await studentProfileService.GetStudentProfileAsync(studentId);
        if (profileData == null) return NotFound();

        var mappedData = mapper.Map<StudentProfileDataViewModel>(profileData);

        var viewModel = new StudentProfileViewModel
        {
            Data = mappedData,
            PageTitle = "My Profile"
        };

        return View(viewModel);
    }

    [HttpGet("/student/profile/edit")]
    public async Task<IActionResult> GetEditProfileForm()
    {
        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Unauthorized();

        var profileData = await studentProfileService.GetStudentProfileAsync(studentId);
        if (profileData == null) return NotFound();

        return PartialView("_EditProfileModal", profileData);
    }

    [HttpPost("/student/profile/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateStudentProfileDto request)
    {
        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Json(new { success = false, message = "Student profile not found" });

        if (!ModelState.IsValid) return Json(new { success = false, message = "Validation failed" });

        var result = await studentProfileService.UpdateStudentProfileAsync(studentId, request);
        return Json(new { success = result });
    }

    [HttpGet("/student/certificates")]
    public async Task<IActionResult> Certificates()
    {
        ViewBag.Title = "My Certificates | Masar";
        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Unauthorized();

        var certs = await certificatesService.GetStudentCertificatesAsync(studentId);
        return certs == null ? NotFound() : View(new StudentCertificatesViewModel { Data = certs, PageTitle = "My Certificates" });
    }
}