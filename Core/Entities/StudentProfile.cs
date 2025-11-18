using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents student-specific profile information
/// One-to-One relationship with User
/// </summary>
public class StudentProfile
{
    [Key]
    [Column("student_id")]
    [Display(Name = "Student ID")]
    public int StudentId { get; set; }


    [Required]
    [Column("user_id")]
    [Display(Name = "User ID")]
    public int UserId { get; set; }

    [Column("bio")]
    [DataType(DataType.MultilineText)]
    [StringLength(1000, ErrorMessage = "{0} cannot exceed {1} characters")]
    [Display(Name = "Biography")]
    public string? Bio { get; set; }



    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    //public virtual ICollection<CourseEnrollment>? CourseEnrollments { get; set; }
    //public virtual ICollection<TrackEnrollment>? TrackEnrollments { get; set; }
    //public ICollection<CourseCertificate>? CourseCertificates { get; set; }
    //public ICollection<TrackCertificate>? TrackCertificates { get; set; }
    public virtual ICollection<EnrollmentBase> Enrollments { get; set; }
    public virtual ICollection<CertificateBase> Certificates { get; set; }

    public StudentProfile()
    {
        //CourseEnrollments = new HashSet<CourseEnrollment>();
        //TrackEnrollments = new HashSet<TrackEnrollment>();
        //CourseCertificates = new HashSet<CourseCertificate>();
        //TrackCertificates = new HashSet<TrackCertificate>();

        Certificates = new HashSet<CertificateBase>();
        Enrollments = new HashSet<EnrollmentBase>();
    }
}