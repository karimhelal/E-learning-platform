using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class LearningEntityCategoryConfiguration : IEntityTypeConfiguration<LearningEntity_Category>
    {
        public void Configure(EntityTypeBuilder<LearningEntity_Category> entity)
        {
            entity.ToTable("LearningEntity_Category");
            entity.HasKey(ec => new { ec.LearningEntityId, ec.CategoryId });

            entity.HasOne(ec => ec.LearningEntity)
                  .WithMany(l => l.LearningEntity_Categories)
                  .HasForeignKey(ec => ec.LearningEntityId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ec => ec.Category)
                  .WithMany(c => c.LearningEntity_Categories)
                  .HasForeignKey(ec => ec.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
