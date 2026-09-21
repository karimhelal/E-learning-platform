using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class SkillConfiguration : IEntityTypeConfiguration<Skill>
    {
        public void Configure(EntityTypeBuilder<Skill> entity)
        {
            entity.ToTable("Skills");
            entity.HasKey(s => s.SkillId);

            entity.HasOne(s => s.User)
                  .WithMany(u => u.Skills)
                  .HasForeignKey(s => s.UserId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
