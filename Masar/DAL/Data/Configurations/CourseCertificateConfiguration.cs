using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class CourseCertificateConfiguration : IEntityTypeConfiguration<CourseCertificate>
    {
        public void Configure(EntityTypeBuilder<CourseCertificate> entity)
        {
            entity.HasBaseType<CertificateBase>();

            entity.HasOne(cc => cc.Course)
                  .WithMany(c => c.Certificates)
                  .HasForeignKey(cc => cc.CourseId)
                  .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
