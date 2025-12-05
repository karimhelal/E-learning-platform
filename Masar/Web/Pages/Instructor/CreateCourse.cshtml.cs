using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core.RepositoryInterfaces;
using Web.Interfaces;
using Core.Entities;

namespace Web.Pages.Instructor
{
    [Authorize(Roles = "Instructor")]
    public class CreateCourseModel : PageModel
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly ILanguageRepository _languageRepo;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;

        public int InstructorId { get; set; }
        public List<Category> Categories { get; set; } = new();
        public List<Language> Languages { get; set; } = new();

        public CreateCourseModel(
            ICategoryRepository categoryRepo,
            ILanguageRepository languageRepo,
            ICurrentUserService currentUserService,
            IUserRepository userRepository)
        {
            _categoryRepo = categoryRepo;
            _languageRepo = languageRepo;
            _currentUserService = currentUserService;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _currentUserService.GetUserId();
            if (userId == 0)
                return Unauthorized();

            var instructorProfile = await _userRepository.GetInstructorProfileForUserAsync(userId, includeUserBase: false);
            if (instructorProfile == null)
                return Forbid();

            InstructorId = instructorProfile.InstructorId;

            // Load categories and languages - Convert to List
            var categoriesResult = await _categoryRepo.GetAllAsync();
            Categories = categoriesResult.ToList(); // Add .ToList()
            
            var languagesResult = await _languageRepo.GetAllAsync();
            Languages = languagesResult.ToList(); // Add .ToList()

            ViewData["InstructorName"] = $"{instructorProfile.User?.FirstName} {instructorProfile.User?.LastName}";

            return Page();
        }
    }
}