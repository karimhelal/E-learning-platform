namespace BLL.Interfaces.Account;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
