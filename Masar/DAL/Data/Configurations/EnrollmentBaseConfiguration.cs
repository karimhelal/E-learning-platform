using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class EnrollmentBaseConfiguration : IEntityTypeConfiguration<EnrollmentBase>
    {
        public void Configure(EntityTypeBuilder<EnrollmentBase> entity)
        {
            entity.ToTable("Enrollments");
            entity.HasKey(e => e.EnrollmentId);

            entity.Property(e => e.ProgressPercentage).HasPrecision(5, 2);

            entity.HasOne(eb => eb.Student)
                  .WithMany(sp => sp.Enrollments)
                  .HasForeignKey(eb => eb.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasDiscriminator<string>("enrollment_type")
                  .HasValue<CourseEnrollment>("CourseEnrollment")
                  .HasValue<TrackEnrollment>("TrackEnrollment");
        }
    }
}
