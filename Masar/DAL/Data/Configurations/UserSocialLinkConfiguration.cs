using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class UserSocialLinkConfiguration : IEntityTypeConfiguration<UserSocialLink>
    {
        public void Configure(EntityTypeBuilder<UserSocialLink> entity)
        {
            entity.ToTable("UserSocialLinks");
            entity.HasKey(s => s.UserSocialLinkId);

            entity.Property(sl => sl.SocialPlatform).HasConversion<string>().HasMaxLength(50);

            entity.HasOne(s => s.User)
                  .WithMany(u => u.UserSocialLinks)
                  .HasForeignKey(s => s.UserId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
