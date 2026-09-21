using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class LearningEntityConfiguration : IEntityTypeConfiguration<LearningEntity>
    {
        public void Configure(EntityTypeBuilder<LearningEntity> entity)
        {
            entity.ToTable("LearningEntities");
            entity.HasKey(l => l.Id);

            entity.HasMany(l => l.Categories)
                  .WithMany()
                  .UsingEntity<LearningEntity_Category>();

            entity.HasMany(l => l.Languages)
                  .WithMany()
                  .UsingEntity<LearningEntity_Language>();

            entity.Property(l => l.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
        }
    }
}
