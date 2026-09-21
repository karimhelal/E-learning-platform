using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class CertificateBaseConfiguration : IEntityTypeConfiguration<CertificateBase>
    {
        public void Configure(EntityTypeBuilder<CertificateBase> entity)
        {
            entity.ToTable("Certificates");
            entity.HasKey(cb => cb.CertificateId);

            entity.HasOne(c => c.Student)
                  .WithMany(sp => sp.Certificates)
                  .HasForeignKey(c => c.StudentId);

            entity.HasDiscriminator<string>("cetificate_type")
                  .HasValue<CourseCertificate>("CourseCertificate")
                  .HasValue<TrackCertificate>("TrackCertificate");
        }
    }
}
