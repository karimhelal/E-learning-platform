using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Entities.Enums;

namespace Core.Entities;

/// <summary>
/// Represents a user in the e-learning platform
/// Can be a Student, Instructor, or Admin
/// </summary>
public class User
{
    [Key]
    [Column("user_id")]
    [Display(Name = "User ID")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [StringLength(50, ErrorMessage = "{0} cannot exceed {1} characters")]
    [Column("first_name")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [StringLength(50, ErrorMessage = "{0} cannot exceed {1} characters")]
    [Column("last_name")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [EmailAddress(ErrorMessage = "Invalid {0}")]
    [StringLength(100)]
    [Column("email")]
    [Display(Name = "Email Address")]
    public string Email { get; set; }

    [Required]
    [Column("password_hash")]
    [Display(Name = "Password Hash")]
    [DataType(DataType.Password)]
    public string PasswordHash { get; set; }

    [Required]
    [Column("role")]
    [Display(Name = "Role")]
    public UserRoles Role { get; set; }

    [Column("picture")]
    [Display(Name = "User Profile Picture")]
    [DataType(DataType.ImageUrl)]
    public string? Picture { get; set; }

    // Computed property
    [NotMapped]
    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}";



    // Navigation Properties
    public virtual StudentProfile? StudentProfile { get; set; }
    public virtual InstructorProfile? InstructorProfile { get; set; }

    public User()
    {
        Role = UserRoles.Student;
    }
}