using Web.Interfaces;

namespace Web.ViewModels.Student;

public class StudentCertificatesViewModel
{
    public StudentCertificatesData Data { get; set; } = new();
    public string PageTitle { get; set; } = "My Certificates";
}