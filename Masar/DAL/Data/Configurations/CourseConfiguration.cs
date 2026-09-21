using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> entity)
        {
            entity.ToTable("Courses");
            entity.HasBaseType<LearningEntity>();

            entity.HasOne(c => c.Instructor)
                  .WithMany(i => i.OwnedCourses)
                  .HasForeignKey(c => c.InstructorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(c => c.Level).HasConversion<string>();
        }
    }
}
