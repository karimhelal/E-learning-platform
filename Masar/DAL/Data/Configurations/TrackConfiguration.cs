using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class TrackConfiguration : IEntityTypeConfiguration<Track>
    {
        public void Configure(EntityTypeBuilder<Track> entity)
        {
            entity.ToTable("Tracks");
            entity.HasBaseType<LearningEntity>();

            entity.HasMany(t => t.Courses)
                  .WithMany(c => c.Tracks)
                  .UsingEntity<Track_Course>();
        }
    }
}
