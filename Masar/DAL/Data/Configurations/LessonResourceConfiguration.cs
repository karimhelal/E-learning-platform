using Core.Entities;
using Core.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class LessonResourceConfiguration : IEntityTypeConfiguration<LessonResource>
    {
        public void Configure(EntityTypeBuilder<LessonResource> entity)
        {
            entity.ToTable("LessonResources");
            entity.HasKey(lr => lr.LessonResourceId);

            entity.HasOne(lr => lr.Lesson)
                  .WithMany(l => l.LessonResources)
                  .HasForeignKey(lr => lr.LessonId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasDiscriminator<string>("resource_type")
                  .HasValue<PdfResource>(LessonResourceType.PDF.ToString())
                  .HasValue<ZipResource>(LessonResourceType.ZIP.ToString())
                  .HasValue<UrlResource>(LessonResourceType.URL.ToString());
        }
    }
}
