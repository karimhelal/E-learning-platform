using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class LearningEntityLanguageConfiguration : IEntityTypeConfiguration<LearningEntity_Language>
    {
        public void Configure(EntityTypeBuilder<LearningEntity_Language> entity)
        {
            entity.ToTable("LearningEntity_Language");
            entity.HasKey(el => new { el.LearningEntityId, el.LanguageId });

            entity.HasOne(el => el.LearningEntity)
                  .WithMany(e => e.LearningEntity_Languages)
                  .HasForeignKey(el => el.LearningEntityId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(el => el.Language)
                  .WithMany(l => l.LearningEntity_Languages)
                  .HasForeignKey(el => el.LanguageId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
