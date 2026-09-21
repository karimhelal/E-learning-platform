using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class CourseLearningOutcomeConfiguration : IEntityTypeConfiguration<CourseLearningOutcome>
    {
        public void Configure(EntityTypeBuilder<CourseLearningOutcome> entity)
        {
            entity.ToTable("CourseLearningOutcomes");
            entity.HasKey(co => co.Id);

            entity.HasOne(co => co.Course)
                  .WithMany(c => c.LearningOutcomes)
                  .HasForeignKey(co => co.CourseId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();
        }
    }
}
