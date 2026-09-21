using BLL.DTOs.Student;
using Web.Interfaces;

namespace Web.ViewModels.Student;

public class StudentCertificatesViewModel
{
    public StudentCertificatesDto Data { get; set; } = new();
    public string PageTitle { get; set; } = "My Certificates";
}