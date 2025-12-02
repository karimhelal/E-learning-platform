namespace Web.Interfaces;

public interface IStudentCertificatesService
{
    Task<StudentCertificatesData?> GetStudentCertificatesAsync(int studentId);
}

public class StudentCertificatesData
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD";
    public int TotalCertificates { get; set; }
    public int CourseCertificatesCount { get; set; }
    public int TrackCertificatesCount { get; set; }
    public List<CertificateItem> Certificates { get; set; } = new();
}

public class CertificateItem
{
    public int CertificateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Course" or "Track"
    public string IssuedDate { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string? DownloadLink { get; set; }
    public string VerificationId { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
}