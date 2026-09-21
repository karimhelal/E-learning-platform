using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.ToTable("Users");
            entity.Property(u => u.Id).HasColumnName("user_id");
            entity.HasKey(u => u.Id);

            entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("IX_User_Email");
            entity.Property(u => u.Email).IsRequired().HasColumnName("email");

            entity.HasOne(u => u.StudentProfile)
                  .WithOne(sp => sp.User)
                  .HasForeignKey<StudentProfile>(sp => sp.UserId);

            entity.HasOne(u => u.InstructorProfile)
                  .WithOne(ip => ip.User)
                  .HasForeignKey<InstructorProfile>(ip => ip.UserId);

            entity.Property(u => u.PasswordHash).HasColumnName("password_hash");
        }
    }
}
