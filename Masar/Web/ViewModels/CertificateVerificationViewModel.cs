namespace Web.ViewModels;

public class CertificateVerificationViewModel
{
    public bool IsValid { get; set; }
    public string VerificationId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string IssuedDate { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
}