using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> entity)
        {
            entity.ToTable("Modules");
            entity.HasKey(m => m.ModuleId);
            entity.HasIndex(m => new { m.CourseId, m.Order }).IsUnique().HasDatabaseName("IX_Module_Course_Order");

            entity.HasOne(m => m.Course)
                  .WithMany(c => c.Modules)
                  .HasForeignKey(m => m.CourseId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
