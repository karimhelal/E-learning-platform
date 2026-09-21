using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
    {
        public void Configure(EntityTypeBuilder<CourseEnrollment> entity)
        {
            entity.HasBaseType<EnrollmentBase>();
            entity.HasIndex(ce => new { ce.CourseId, ce.StudentId }).IsUnique().HasDatabaseName("IX_CourseEnrollment_Course_Student");

            entity.HasOne(ce => ce.Course)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(ce => ce.CourseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(uc => uc.Status).HasConversion<string>();
        }
    }
}
