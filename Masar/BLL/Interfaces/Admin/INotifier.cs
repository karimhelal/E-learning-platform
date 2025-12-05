namespace BLL.Interfaces.Admin
{
    public interface INotifier
    {
        Task SendToAdminsAsync(string title, string message, string url);
        Task SendToUserAsync(int userId, string title, string message, string url);
    }
}
