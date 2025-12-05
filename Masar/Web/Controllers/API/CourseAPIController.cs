using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTOs.Course;
using Web.Interfaces;
using Core.RepositoryInterfaces;

namespace Web.Controllers.API
{
    [Route("api/courses")]
    [ApiController]
    [Authorize(Roles = "Instructor")]
    public class CourseAPIController : ControllerBase
    {
        private readonly ICourseCreationService _courseCreationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;

        public CourseAPIController(
            ICourseCreationService courseCreationService,
            ICurrentUserService currentUserService,
            IUserRepository userRepository)
        {
            _courseCreationService = courseCreationService;
            _currentUserService = currentUserService;
            _userRepository = userRepository;
        }

        private async Task<int> GetInstructorIdAsync()
        {
            var userId = _currentUserService.GetUserId();
            if (userId == 0) return 0;
            
            var instructor = await _userRepository.GetInstructorProfileForUserAsync(userId, false);
            return instructor?.InstructorId ?? 0;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
        {
            try
            {
                Console.WriteLine("=== API Create Course Called ===");
                Console.WriteLine($"Title: {dto.Title}");
                Console.WriteLine($"Level: {dto.Level}");
                Console.WriteLine($"CategoryId: {dto.CategoryId}");
                Console.WriteLine($"LanguageId: {dto.LanguageId}");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("? Model state is invalid:");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    
                    return BadRequest(new 
                    { 
                        success = false, 
                        message = "Validation failed",
                        errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                    });
                }
                
                var instructorId = await GetInstructorIdAsync();
                if (instructorId == 0)
                {
                    Console.WriteLine("? Instructor ID is 0 - Unauthorized");
                    return Unauthorized(new { success = false, message = "Instructor profile not found" });
                }

                Console.WriteLine($"? Instructor ID: {instructorId}");
                dto.InstructorId = instructorId;
                
                var result = await _courseCreationService.CreateCourseAsync(dto);

                if (result.Success)
                {
                    Console.WriteLine($"? Course created successfully! ID: {result.CourseId}");
                    return Ok(new { 
                        success = true, 
                        courseId = result.CourseId, 
                        message = "Course published successfully!" 
                    });
                }
                
                Console.WriteLine($"? Course creation failed: {result.ErrorMessage}");
                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Exception in CreateCourse: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Server error: {ex.Message}" 
                });
            }
        }

        [HttpPost("upload-thumbnail")]
        public async Task<IActionResult> UploadThumbnail(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file uploaded" });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { success = false, message = "Invalid file type" });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { success = false, message = "File size exceeds 5MB" });

            try
            {
                var fileName = $"course-{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courses");
                Directory.CreateDirectory(uploadsFolder);
                
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                var fileUrl = $"/uploads/courses/{fileName}";
                return Ok(new { success = true, url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Upload failed: " + ex.Message });
            }
        }
    }
}