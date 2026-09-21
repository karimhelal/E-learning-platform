using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class TrackEnrollmentConfiguration : IEntityTypeConfiguration<TrackEnrollment>
    {
        public void Configure(EntityTypeBuilder<TrackEnrollment> entity)
        {
            entity.HasBaseType<EnrollmentBase>();
            entity.HasIndex(te => new { te.TrackId, te.StudentId }).IsUnique().HasDatabaseName("IX_TrackEnrollment_Track_Student");

            entity.HasOne(te => te.Track)
                  .WithMany(t => t.Enrollments)
                  .HasForeignKey(te => te.TrackId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
