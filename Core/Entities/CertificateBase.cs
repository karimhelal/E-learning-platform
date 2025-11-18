using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public abstract class CertificateBase
{
    [Key]
    [Column("certificate_id")]
    [Display(Name = "Certificate ID")]
    public int CertificateId { get; set; }



    [Required(ErrorMessage = "{0} is required")]
    [Column("student_id")]
    [Display(Name = "Student ID")]
    public int StudentId { get; set; }



    [Required(ErrorMessage = "{0} is required")]
    [StringLength(200, ErrorMessage = "{0} cannot exceed {1} characters")]
    [Column("title")]
    [Display(Name = "Certificate Title")]
    public string Title { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [DataType(DataType.Date)]
    [Column("issued_date")]
    [Display(Name = "Issued Date")]
    public DateOnly IssuedDate { get; set; }


    [Column("link")]
    [Display(Name = "Certificate Link")]
    [DataType(DataType.Url)]
    [StringLength(500)]
    public string Link { get; set; }



    [NotMapped]
    public string CertificateType => GetType().Name; // "CourseCertificate" or "TrackCertificate"


    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual StudentProfile? Student { get; set; }



    public abstract LearningEntity GetLearningEntity();
}
