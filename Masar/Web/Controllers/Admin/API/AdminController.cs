using BLL.DTOs.Admin;
using BLL.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Admin.API
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }


        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] string role, [FromQuery] int page = 1)
        {
            var result = await _adminService.GetUsersAsync(search ?? "", role, page);
            return Ok(result);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _adminService.DeleteUserAsync(id);
            if (!success)
            {
                return BadRequest(new { message = "Cannot delete this user. They might be enrolled in courses or have taught courses." });
            }
            return Ok(new { message = "Deleted successfully" });
        }



        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses([FromQuery] string? search, [FromQuery] string? category, [FromQuery] int page = 1)
        {
            var result = await _adminService.GetCoursesAsync(search ?? "", category ?? "", page);
            return Ok(result);
        }

        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var success = await _adminService.DeleteCourseAsync(id);
            if (!success) return NotFound(new { message = "Course not found" });
            return Ok(new { message = "Deleted successfully" });
        }


        [HttpGet("pending-courses")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _adminService.GetPendingCoursesAsync();
            return Ok(result);
        }

        [HttpPost("courses/{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            await _adminService.ApproveCourseAsync(id);
            return Ok(new { message = "Course Approved" });
        }

        [HttpPost("courses/{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectDto model)
        {
            if (string.IsNullOrEmpty(model.Reason)) return BadRequest("Reason is required");

            await _adminService.RejectCourseAsync(id, model.Reason);
            return Ok(new { message = "Course Rejected" });
        }

        [HttpGet("tracks")]
        public async Task<IActionResult> GetTracks([FromQuery] string? search, [FromQuery] int page = 1)
        {
            var result = await _adminService.GetTracksAsync(search ?? "", page);
            return Ok(result);
        }

        [HttpDelete("tracks/{id}")]
        public async Task<IActionResult> DeleteTrack(int id)
        {
            var success = await _adminService.DeleteTrackAsync(id);
            if (!success) return NotFound(new { message = "Track not found" });
            return Ok(new { message = "Track deleted successfully" });
        }

        [HttpGet("courses/simple")]
        public async Task<IActionResult> GetCoursesSimple()
        {
            var result = await _adminService.GetAllCoursesSimpleAsync();
            return Ok(result);
        }

        [HttpPost("tracks")]
        public async Task<IActionResult> CreateTrack([FromBody] CreateTrackDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Title is required" });

            var trackId = await _adminService.CreateTrackAsync(dto);
            return Ok(new { message = "Track created successfully", id = trackId });
        }
    }
}
