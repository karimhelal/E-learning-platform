using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
    {
        public void Configure(EntityTypeBuilder<StudentProfile> entity)
        {
            entity.ToTable("StudentProfiles");
            entity.HasKey(s => s.StudentId);
            entity.HasIndex(s => s.UserId).IsUnique().HasDatabaseName("IX_StudentProfile_UserId");
        }
    }
}
