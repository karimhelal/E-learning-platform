using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
    {
        public void Configure(EntityTypeBuilder<Assignment> entity)
        {
            entity.ToTable("Assignments");
            entity.HasKey(a => a.AssignmentId);

            entity.HasOne(a => a.Module)
                  .WithMany(m => m.Assignments)
                  .HasForeignKey(a => a.ModuleId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
