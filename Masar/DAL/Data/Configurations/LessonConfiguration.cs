using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> entity)
        {
            entity.ToTable("Lessons");
            entity.HasKey(l => l.LessonId);
            entity.HasIndex(l => new { l.ModuleId, l.Order }).IsUnique().HasDatabaseName("IX_Lesson_Module_Order");

            entity.HasOne(l => l.Module)
                  .WithMany(m => m.Lessons)
                  .HasForeignKey(l => l.ModuleId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(l => l.ContentType).HasConversion<string>();
        }
    }
}
