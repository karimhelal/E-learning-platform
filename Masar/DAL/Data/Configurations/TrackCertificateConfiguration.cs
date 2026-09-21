using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class TrackCertificateConfiguration : IEntityTypeConfiguration<TrackCertificate>
    {
        public void Configure(EntityTypeBuilder<TrackCertificate> entity)
        {
            entity.HasBaseType<CertificateBase>();

            entity.HasOne(tc => tc.Track)
                  .WithMany(t => t.Certificates)
                  .HasForeignKey(tc => tc.TrackId)
                  .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
