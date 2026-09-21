using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class InstructorProfileConfiguration : IEntityTypeConfiguration<InstructorProfile>
    {
        public void Configure(EntityTypeBuilder<InstructorProfile> entity)
        {
            entity.ToTable("InstructorProfiles");
            entity.HasKey(i => i.InstructorId);
            entity.HasIndex(i => i.UserId).IsUnique().HasDatabaseName("IX_InstructorProfile_UserId");
        }
    }
}
