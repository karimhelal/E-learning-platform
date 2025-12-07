using Core.Entities;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Track;

public interface ITrackService
{
    Task<List<Core.Entities.Track>> GetFeaturedTracksAsync(int count = 6);
    Task<List<Core.Entities.Track>> GetAllTracksAsync();
}

public class TrackService : ITrackService
{
    private readonly AppDbContext _context;

    public TrackService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Core.Entities.Track>> GetFeaturedTracksAsync(int count = 6)
    {
        return await _context.Tracks
            .Include(t => t.TrackCourses)
                .ThenInclude(tc => tc.Course)
            .Include(t => t.Enrollments)
            .Include(t => t.Categories)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Core.Entities.Track>> GetAllTracksAsync()
    {
        return await _context.Tracks
            .Include(t => t.TrackCourses)
            .Include(t => t.Enrollments)
            .Include(t => t.Categories)
            .ToListAsync();
    }
}