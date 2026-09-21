using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class LessonContentConfiguration : IEntityTypeConfiguration<LessonContent>
    {
        public void Configure(EntityTypeBuilder<LessonContent> entity)
        {
            entity.ToTable("LessonContents");
            entity.HasKey(lc => lc.LessonContentId);

            entity.HasOne(lc => lc.Lesson)
                  .WithOne(l => l.LessonContent)
                  .HasForeignKey<LessonContent>(lc => lc.LessonId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();

            entity.HasDiscriminator<string>("content_type")
                  .HasValue<VideoContent>("Video")
                  .HasValue<ArticleContent>("Article");
        }
    }
}
