using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class LanguageConfiguration : IEntityTypeConfiguration<Language>
    {
        public void Configure(EntityTypeBuilder<Language> entity)
        {
            entity.ToTable("Languages");
            entity.HasKey(l => l.LanguageId);
            entity.HasIndex(c => c.Slug).HasDatabaseName("IX_Language_Slug").IsUnique();
        }
    }
}
