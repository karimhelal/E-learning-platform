using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class TrackCourseConfiguration : IEntityTypeConfiguration<Track_Course>
    {
        public void Configure(EntityTypeBuilder<Track_Course> entity)
        {
            entity.ToTable("Track_Course");
            entity.HasKey(tc => new { tc.TrackId, tc.CourseId });

            entity.HasOne(tc => tc.Track)
                  .WithMany(t => t.TrackCourses)
                  .HasForeignKey(tc => tc.TrackId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(tc => tc.Course)
                  .WithMany(c => c.CourseTracks)
                  .HasForeignKey(tc => tc.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
