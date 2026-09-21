using BLL.Interfaces.Student;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.AspNetCore.Mvc;
using Web.Interfaces;
using Web.ViewModels;
using Web.ViewModels.Student;

namespace Web.Controllers.Student;

public class StudentTracksController(
    IStudentTrackService tracksService,
    IStudentTrackDetailsService trackDetailsService,
    IStudentBrowseTrackService browseTrackService,
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    AppDbContext context,
    ILogger<StudentTracksController> logger)
    : StudentBaseController(currentUserService, userRepository, context, logger)
{
    [HttpGet("/student/my-tracks")]
    public async Task<IActionResult> MyTracks()
    {
        ViewBag.Title = "My Tracks | Masar";
        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Unauthorized();

        var data = await tracksService.GetStudentTracksAsync(studentId);
        return data == null ? NotFound() : View(new StudentTracksViewModel { PageTitle = "My Tracks", Data = data });
    }

    [HttpGet("/student/browse-tracks")]
    public async Task<IActionResult> BrowseTracks()
    {
        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Unauthorized();

        var data = await browseTrackService.GetAllTracksAsync(studentId);
        return data == null ? View("Error") : View(new StudentBrowseTracksViewModel { Data = data });
    }

    [HttpGet("/student/track/{trackId}")]
    public async Task<IActionResult> TrackDetails(int trackId)
    {
        ViewBag.Title = "Track Details | Masar";
        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Unauthorized();

        var data = await trackDetailsService.GetTrackDetailsAsync(studentId, trackId);
        return data == null ? NotFound() : View(new StudentTrackDetailsViewModel { Data = data, PageTitle = data.Title });
    }
}