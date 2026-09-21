using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
    {
        public void Configure(EntityTypeBuilder<LessonProgress> entity)
        {
            entity.ToTable("LessonProgresses");
            entity.HasKey(lp => lp.LessonProgressId);
            entity.HasIndex(lp => new { lp.StudentId, lp.LessonId }).IsUnique().HasDatabaseName("IX_LessonProgress_Student_Lesson");

            entity.HasOne(lp => lp.Lesson)
                  .WithMany(l => l.LessonProgresses)
                  .HasForeignKey(lp => lp.LessonId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lp => lp.Student)
                  .WithMany()
                  .HasForeignKey(lp => lp.StudentId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
